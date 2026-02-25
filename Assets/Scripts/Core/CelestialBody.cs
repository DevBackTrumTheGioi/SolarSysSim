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

    // ==================== TRAIL ====================
    private TrailRenderer trailRenderer;

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
    /// Trong GameFriendly mode: dùng khoảng cách nén.
    /// Trong Realistic mode: dùng khoảng cách thật.
    /// </summary>
    public void UpdateVisualPosition(SimulationSettings settings)
    {
        if (settings != null && settings.mode == SimulationSettings.SimMode.GameFriendly)
        {
            // Nén khoảng cách cho visual: giữ hướng, đổi độ dài
            DoubleVector3 visualPos = settings.PhysicsToVisualPosition(position);
            transform.position = visualPos.ToVector3();
        }
        else
        {
            // Realistic: 1:1 mapping
            transform.position = position.ToVector3();
        }
    }

    /// <summary>
    /// Setup TrailRenderer để vẽ quỹ đạo.
    /// </summary>
    public void SetupTrail(SimulationSettings settings)
    {
        if (!settings.showOrbits) return;

        trailRenderer = gameObject.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
            trailRenderer = gameObject.AddComponent<TrailRenderer>();

        trailRenderer.time = settings.trailDuration;
        trailRenderer.startWidth = 0.02f;
        trailRenderer.endWidth = 0.005f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = orbitColor;
        trailRenderer.endColor = new Color(orbitColor.r, orbitColor.g, orbitColor.b, 0f);
        trailRenderer.numCornerVertices = 4;
        trailRenderer.numCapVertices = 4;
        trailRenderer.minVertexDistance = 0.02f;
    }

    /// <summary>
    /// Xóa trail (hữu ích khi reset simulation).
    /// </summary>
    public void ClearTrail()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
    }
}

