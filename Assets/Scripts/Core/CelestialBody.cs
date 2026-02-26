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

    [Header("=== VISUAL ===")]
    [Tooltip("Màu quỹ đạo")]
    public Color orbitColor = Color.white;

    [Tooltip("Tên hiển thị")]
    public string bodyName = "Unknown";

    // ==================== RUNTIME STATE (double precision) ====================
    // Physics state - khoảng cách THỰC (AU)
    [HideInInspector] public DoubleVector3 position;
    [HideInInspector] public DoubleVector3 velocity;
    [HideInInspector] public DoubleVector3 acceleration;

    // ==================== TRAIL (LineRenderer custom) ====================
    private LineRenderer orbitLine;

    // Circular buffer lưu các điểm quỹ đạo (visual positions)
    private Vector3[] orbitPoints;
    private int orbitHead = 0;      // Index điểm mới nhất
    private int orbitCount = 0;     // Số điểm hiện có
    private int maxOrbitPoints = 300; // Số điểm tối đa (~1 vòng quỹ đạo)

    private float minPointSpacing = 0.01f; // Khoảng cách tối thiểu giữa 2 điểm (Unity units)

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
            // Vector từ Mặt Trời đến hành tinh (physics)
            DoubleVector3 relativePhysics = position - sunPhysicsPos;
            DoubleVector3 visualOffset = settings.PhysicsToVisualPosition(relativePhysics);
            transform.position = sunVisualPos + visualOffset.ToVector3();
        }
        else
        {
            transform.position = position.ToVector3();
        }
    }

    /// <summary>
    /// Setup LineRenderer thay vì TrailRenderer — mượt hơn ở mọi tốc độ.
    /// </summary>
    public void SetupTrail(SimulationSettings settings)
    {
        if (!settings.showOrbits) return;

        // Xóa TrailRenderer cũ nếu có
        TrailRenderer oldTrail = gameObject.GetComponent<TrailRenderer>();
        if (oldTrail != null) DestroyImmediate(oldTrail);

        orbitLine = gameObject.GetComponent<LineRenderer>();
        if (orbitLine == null)
            orbitLine = gameObject.AddComponent<LineRenderer>();

        orbitLine.material = new Material(Shader.Find("Sprites/Default"));
        orbitLine.startWidth = 0.05f;
        orbitLine.endWidth = 0.01f;
        orbitLine.startColor = orbitColor;
        orbitLine.endColor = new Color(orbitColor.r, orbitColor.g, orbitColor.b, 0f);
        orbitLine.numCornerVertices = 0;
        orbitLine.numCapVertices = 0;
        orbitLine.useWorldSpace = true;
        orbitLine.positionCount = 0;

        // Init circular buffer
        maxOrbitPoints = 300;
        orbitPoints = new Vector3[maxOrbitPoints];
        orbitHead = 0;
        orbitCount = 0;
    }

    /// <summary>
    /// Thêm điểm mới vào orbit trail. Gọi sau mỗi lần UpdateVisualPosition.
    /// Circular buffer: điểm cũ nhất tự bị xóa khi đầy.
    /// </summary>
    public void AddOrbitPoint(Vector3 pos)
    {
        if (orbitLine == null || orbitPoints == null) return;

        // Chỉ thêm nếu đủ xa điểm trước (tránh quá nhiều điểm chồng nhau)
        if (orbitCount > 0)
        {
            int lastIdx = (orbitHead - 1 + maxOrbitPoints) % maxOrbitPoints;
            if (Vector3.SqrMagnitude(pos - orbitPoints[lastIdx]) < minPointSpacing * minPointSpacing)
                return;
        }

        orbitPoints[orbitHead] = pos;
        orbitHead = (orbitHead + 1) % maxOrbitPoints;
        if (orbitCount < maxOrbitPoints) orbitCount++;

        // Rebuild LineRenderer từ circular buffer (theo thứ tự cũ → mới)
        orbitLine.positionCount = orbitCount;
        for (int i = 0; i < orbitCount; i++)
        {
            int idx = (orbitHead - orbitCount + i + maxOrbitPoints) % maxOrbitPoints;
            orbitLine.SetPosition(i, orbitPoints[idx]);
        }

        // Fade: điểm đầu (cũ nhất) trong suốt, điểm cuối (mới nhất) đậm
        orbitLine.startColor = new Color(orbitColor.r, orbitColor.g, orbitColor.b, 0f);
        orbitLine.endColor = orbitColor;
    }

    /// <summary>
    /// Cập nhật số điểm tối đa dựa theo tốc độ — speed nhanh thì giữ ít điểm hơn.
    /// </summary>
    public void SetOrbitMaxPoints(int points)
    {
        if (points == maxOrbitPoints || orbitPoints == null) return;

        // Resize buffer, giữ lại các điểm gần nhất
        int newMax = Mathf.Max(50, points);
        Vector3[] newBuffer = new Vector3[newMax];
        int copyCount = Mathf.Min(orbitCount, newMax);

        for (int i = 0; i < copyCount; i++)
        {
            int srcIdx = (orbitHead - copyCount + i + maxOrbitPoints) % maxOrbitPoints;
            newBuffer[i] = orbitPoints[srcIdx];
        }

        orbitPoints = newBuffer;
        maxOrbitPoints = newMax;
        orbitHead = copyCount % newMax;
        orbitCount = copyCount;
    }

    /// <summary>
    /// Xóa trail (hữu ích khi reset simulation).
    /// </summary>
    public void ClearTrail()
    {
        if (orbitLine != null)
        {
            orbitLine.positionCount = 0;
        }
        orbitHead = 0;
        orbitCount = 0;
    }

    /// <summary>
    /// Legacy — giữ lại để không break compile, không dùng nữa.
    /// </summary>
    public void SetTrailDuration(float duration) { }
}
