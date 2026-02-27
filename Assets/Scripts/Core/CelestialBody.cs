using UnityEngine;

/// <summary>
/// Component gắn lên mỗi thiên thể (Mặt Trời, hành tinh, mặt trăng, etc.)
/// 
/// === KIẾN TRÚC: PHYSICS ≠ VISUAL ===
/// Điểm quan trọng nhất: PHYSICS và VISUAL hoàn toàn TÁCH BIỆT.
///
/// PHYSICS (double precision, AU):
///   - position, velocity, acceleration chạy ở khoảng cách THẬT
///   - Gravity tính trên khoảng cách thật → quỹ đạo ĐÚNG
///   - Trái Đất luôn ở ~1 AU từ Mặt Trời trong physics
///
/// VISUAL (float, Unity units):
///   - transform.position dùng khoảng cách NÉN
///   - Hành tinh nhìn gần nhau hơn, to hơn → đẹp mắt
///   - Không ảnh hưởng tính toán vật lý
///
/// Tại sao phải tách? Vì nếu dùng khoảng cách nén để tính gravity,
/// lực hấp dẫn sẽ sai (F ∝ 1/r²), quỹ đạo méo mó hoặc bay lung tung!
/// </summary>
public class CelestialBody : MonoBehaviour
{
    [Header("=== PHYSICAL PROPERTIES ===")]
    [Tooltip("Khối lượng tính bằng Solar Mass (M☉). VD: Earth = 3.003e-6")]
    public double mass = 1.0;

    [Tooltip("Bán kính thực tế (AU) - dùng cho visual scaling")]
    public double bodyRadius = 0.00465; // Sun radius in AU

    [Header("=== INITIAL CONDITIONS ===")]
    [Tooltip("Vận tốc ban đầu (AU/day). Đặt tiếp tuyến với quỹ đạo để có orbit tròn.")]
    public Vector3 initialVelocityV3 = Vector3.zero;

    [Tooltip("Vị trí ban đầu (AU). Override transform.position khi simulation bắt đầu.")]
    public Vector3 initialPositionV3 = Vector3.zero;

    [Tooltip("Màu quỹ đạo")]
    public Color orbitColor = Color.white;

    [Tooltip("Tên hiển thị")]
    public string bodyName = "Unknown";

    [Header("=== RELATIONSHIP TIER ===")]
    [Tooltip("Khoá quỹ đạo theo hành tinh này (VD: Earth kéo Moon). Null nếu bay quanh Sun.")]
    public Transform orbitParent;

    [Header("=== ROTATION ===")]
    [Tooltip("Độ nghiêng của trục tự quay so với mặt phẳng quỹ đạo (độ)")]
    public float axialTilt = 0f;

    [Tooltip("Thời gian tự quay quanh trục (tính bằng Số ngày Trái Đất/vòng)")]
    public float rotationPeriod = 1f;

    // ==================== RUNTIME STATE (double precision) ====================
    // Physics state - khoảng cách THỰC (AU)
    [HideInInspector] public DoubleVector3 position;
    [HideInInspector] public DoubleVector3 velocity;
    [HideInInspector] public DoubleVector3 acceleration;

    // Visual state
    [HideInInspector] public float baseVisualScale = 1f;

    // ==================== TRAIL (LineRenderer custom) ====================
    private LineRenderer orbitLine;

    // Circular buffer lưu các điểm quỹ đạo (visual positions)
    private Vector3[] orbitPoints;
    private int orbitHead = 0;        // Index slot tiếp theo sẽ ghi
    private int orbitCount = 0;       // Số điểm hiện có (tăng đến maxOrbitPoints rồi giữ)
    private int maxOrbitPoints = 1000; // Số điểm tối đa — đủ cao để trail mượt

    // reusable temp array để set vào LineRenderer (tránh alloc mỗi frame)
    private Vector3[] linePositionsTemp;
    private bool lineIsFull = false;  // true khi buffer đã wrap around lần đầu

    /// <summary>
    /// Khởi tạo trạng thái từ initial conditions.
    /// Được gọi bởi GravitySimulation.
    /// </summary>
    public void Initialize()
    {
        position = new DoubleVector3(initialPositionV3.x, initialPositionV3.y, initialPositionV3.z);
        velocity = new DoubleVector3(initialVelocityV3.x, initialVelocityV3.y, initialVelocityV3.z);
        acceleration = DoubleVector3.zero;

        transform.position = position.ToVector3();

        // Áp dụng độ nghiêng trục ban đầu (Nghiêng quanh trục X của thế giới)
        transform.rotation = Quaternion.Euler(axialTilt, 0, 0);
    }

    /// <summary>
    /// Xoay hành tinh quang trục Y Local mỗi frame dựa theo rotationPeriod và TimeScale
    /// </summary>
    public void UpdateRotation(float timeScale)
    {
        if (rotationPeriod == 0) return; // Tránh lỗi chia cho 0

        // 1 day in sim = 360 degrees rotation for Earth. 
        // Vận tốc góc = (360 độ / rotationPeriod) * timeScale * deltaTime
        float rotationSpeed = (360f / rotationPeriod) * timeScale;
        
        // Quanh quanh trục Y cục bộ (đã bị nghiêng bởi axialTilt)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// Cập nhật visual position từ physics position.
    /// sunPhysicsPos: vị trí physics của Mặt Trời (tham chiếu để nén khoảng cách).
    /// 
    /// === FIX: Nén khoảng cách TƯƠNG ĐỐI so với Mặt Trời ===
    /// Trước đây nén từ gốc (0,0,0) → sai khi Mặt Trời drift.
    /// Bây giờ nén từ vị trí Mặt Trời thực → luôn đúng.
    /// </summary>
    public void UpdateVisualPosition(SimulationSettings settings, DoubleVector3 sunPhysicsPos, Vector3 sunVisualPos)
    {
        if (settings != null && settings.mode == SimulationSettings.SimMode.GameFriendly)
        {
            if (orbitParent != null)
            {
                // Hành tinh vệ tinh (như Mặt trăng) -> cần phóng đại khoảng cách tương đối kẻo bị thụt vào trong lõi Mẹ
                CelestialBody parentBody = orbitParent.GetComponent<CelestialBody>();
                if (parentBody != null)
                {
                    // Khoảng cách Vật lý thực sự từ Vệ tinh -> Mẹ
                    DoubleVector3 diffPhysics = position - parentBody.position;
                    
                    // Exaggerate khoảng cách biểu kiến này lên 60 lần để mặt trăng thoát ra khỏi bề mặt trái đất 3D
                    float visualScalePump = 60f; 
                    
                    Vector3 parentVisual = parentBody.transform.position;
                    Vector3 localVisualOffset = diffPhysics.ToVector3() * visualScalePump;
                    
                    transform.position = parentVisual + localVisualOffset;
                }
            }
            else
            {
                // Hành tinh quay quanh mặt trời bình thường
                DoubleVector3 relativePhysics = position - sunPhysicsPos;
                DoubleVector3 visualOffset = settings.PhysicsToVisualPosition(relativePhysics);
                transform.position = sunVisualPos + visualOffset.ToVector3();
            }
        }
        else
        {
            // Các hành tinh bay độc lập hoặc mode Realistic
            transform.position = position.ToVector3();
        }

        // === CẬP NHẬT KÍCH THƯỚC (SCALE) ĐỘNG (Real-time) ===
        if (settings != null)
        {
            transform.localScale = Vector3.one * (baseVisualScale * settings.visualScaleMultiplier);
        }
    }

    /// <summary>
    /// Setup LineRenderer thay vì TrailRenderer — mượt hơn ở mọi tốc độ.
    /// </summary>
    public void SetupTrail(SimulationSettings settings)
    {
        // Xóa TrailRenderer cũ nếu có
        TrailRenderer oldTrail = gameObject.GetComponent<TrailRenderer>();
        if (oldTrail != null) DestroyImmediate(oldTrail);

        orbitLine = gameObject.GetComponent<LineRenderer>();
        if (orbitLine == null)
            orbitLine = gameObject.AddComponent<LineRenderer>();

        orbitLine.enabled = settings.showOrbits;

        orbitLine.material = new Material(Shader.Find("Sprites/Default"));
        
        // --- 1. Gradient Màu sắc tuyệt đẹp ---
        // Đuôi (index 0) hoàn toàn trong suốt. Đầu (index 1) màu rực rỡ sáng chói.
        Gradient gradient = new Gradient();
        Color headColor = new Color(
            Mathf.Min(1f, orbitColor.r + 0.3f), // Làm head sáng rực rỡ hơn tý
            Mathf.Min(1f, orbitColor.g + 0.3f), 
            Mathf.Min(1f, orbitColor.b + 0.3f)
        );
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(orbitColor, 0f), 
                new GradientColorKey(orbitColor, 0.8f), 
                new GradientColorKey(headColor, 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),     // Đuôi mờ tịt
                new GradientAlphaKey(0.2f, 0.5f), // Giữa mờ dần
                new GradientAlphaKey(1f, 1f)      // Đầu đặc
            }
        );
        orbitLine.colorGradient = gradient;

        // --- 2. Đường cong mượt độ dày (Width Curve) ---
        // Giống đuôi sao chổi: Đuôi vuốt nhọn, Đầu hơi bầu bĩnh
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(new Keyframe(0f, 0f));
        widthCurve.AddKey(new Keyframe(0.6f, 0.02f));
        widthCurve.AddKey(new Keyframe(1f, 0.08f));
        orbitLine.widthCurve = widthCurve;

        // --- 3. Bo góc mượt (Smoothness) ---
        orbitLine.numCornerVertices = 5; // Tránh hiện tượng gãy gập thành đường thẳng ở tốc độ cao
        orbitLine.numCapVertices = 5;    // Bo tròn đầu cuối
        orbitLine.useWorldSpace = true;
        orbitLine.positionCount = 0;

        // Pre-allocate buffer
        maxOrbitPoints = 1000;
        orbitPoints = new Vector3[maxOrbitPoints];
        linePositionsTemp = new Vector3[maxOrbitPoints];
        orbitHead = 0;
        orbitCount = 0;
        lineIsFull = false;
    }

    /// <summary>
    /// Thêm điểm mới. Khi buffer chưa đầy: append + tăng positionCount.
    /// Khi buffer đã đầy: ghi đè điểm cũ nhất, shift toàn bộ chỉ 1 lần/frame.
    /// </summary>
    public void AddOrbitPoint(Vector3 pos)
    {
        if (orbitLine == null || orbitPoints == null) return;

        // Ghi vào head
        orbitPoints[orbitHead] = pos;
        orbitHead = (orbitHead + 1) % maxOrbitPoints;

        if (!lineIsFull)
        {
            orbitCount++;
            orbitLine.positionCount = orbitCount;
            // Chỉ set điểm mới nhất — nhanh O(1)
            orbitLine.SetPosition(orbitCount - 1, pos);

            if (orbitCount >= maxOrbitPoints)
                lineIsFull = true;
        }
        else
        {
            // Buffer đầy: rebuild theo đúng thứ tự từ head (điểm cũ nhất) → head-1 (mới nhất)
            // Chỉ rebuild khi lineIsFull vừa được set hoặc mỗi frame (orbitHead % rebuild rate)
            // Rebuild toàn bộ 1 lần — unavoidable khi circular, nhưng dùng SetPositions(array) thay vì loop
            for (int i = 0; i < maxOrbitPoints; i++)
            {
                int idx = (orbitHead + i) % maxOrbitPoints;
                linePositionsTemp[i] = orbitPoints[idx];
            }
            orbitLine.positionCount = maxOrbitPoints;
            orbitLine.SetPositions(linePositionsTemp); // 1 draw call thay vì N SetPosition
        }
    }

    /// <summary>
    /// Cập nhật số điểm tối đa theo orbital period.
    /// Được gọi 1 lần sau Init — không gọi liên tục mỗi frame.
    /// </summary>
    public void SetOrbitMaxPoints(int points)
    {
        int newMax = Mathf.Clamp(points, 60, 2000);

        // Nếu chưa có buffer (gọi trước SetupTrail) → chỉ lưu giá trị
        if (orbitPoints == null)
        {
            maxOrbitPoints = newMax;
            return;
        }

        if (newMax == maxOrbitPoints) return;

        // Giữ lại các điểm gần nhất
        Vector3[] newBuffer = new Vector3[newMax];
        Vector3[] newTemp   = new Vector3[newMax];
        int copyCount = Mathf.Min(orbitCount, newMax);

        for (int i = 0; i < copyCount; i++)
        {
            int srcIdx = (orbitHead - copyCount + i + maxOrbitPoints) % maxOrbitPoints;
            newBuffer[i] = orbitPoints[srcIdx];
        }

        orbitPoints       = newBuffer;
        linePositionsTemp = newTemp;
        maxOrbitPoints    = newMax;
        orbitHead         = copyCount % newMax;
        orbitCount        = copyCount;
        lineIsFull        = (orbitCount >= newMax);

        if (orbitLine != null)
        {
            orbitLine.positionCount = orbitCount;
            for (int i = 0; i < orbitCount; i++)
                orbitLine.SetPosition(i, orbitPoints[i]);
        }
    }

    /// <summary>
    /// Xóa trail.
    /// </summary>
    public void ClearTrail()
    {
        if (orbitLine != null)
            orbitLine.positionCount = 0;
        orbitHead  = 0;
        orbitCount = 0;
        lineIsFull = false;
    }

    public void SetTrailDuration(float duration) { }
}
