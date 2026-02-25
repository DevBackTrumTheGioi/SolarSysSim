using UnityEngine;

/// <summary>
/// Script khởi tạo toàn bộ Hệ Mặt Trời từ PlanetData.
/// 
/// === CÁCH SỬ DỤNG ===
/// 1. Tạo Empty GameObject "SolarSystem"
/// 2. Gắn SolarSystemBuilder component
/// 3. Gắn GravitySimulation component 
/// 4. Tạo SimulationSettings asset (Create > Solar System > Simulation Settings)
/// 5. Kéo settings vào cả 2 components
/// 6. Nhấn Play → Hệ Mặt Trời tự tạo và bắt đầu chạy!
///
/// === PHYSICS vs VISUAL ===
/// - initialPositionV3 và initialVelocityV3 → PHYSICS (khoảng cách thật, AU)
/// - transform.position → VISUAL (khoảng cách nén, Unity units)
/// - GravitySimulation tính gravity trên physics positions → quỹ đạo ĐÚNG
/// - CelestialBody.UpdateVisualPosition() map physics → visual mỗi frame
/// </summary>
public class SolarSystemBuilder : MonoBehaviour
{
    [Header("=== SETTINGS ===")]
    public SimulationSettings settings;

    [Header("=== OPTIONS ===")]
    [Tooltip("Hướng 'lên' của mặt phẳng quỹ đạo. Mặc định: Y-up (XZ plane).")]
    public bool orbitalPlaneXZ = true;

    void Awake()
    {
        BuildSolarSystem();
    }

    /// <summary>
    /// Tạo tất cả hành tinh từ PlanetData.
    /// </summary>
    public void BuildSolarSystem()
    {
        foreach (var data in PlanetData.AllBodies)
        {
            CreateBody(data);
        }

        Debug.Log($"[SolarSystemBuilder] Created {PlanetData.AllBodies.Length} celestial bodies.");
    }

    /// <summary>
    /// Tạo một thiên thể từ BodyInfo.
    /// 
    /// === KIẾN TRÚC QUAN TRỌNG: PHYSICS ≠ VISUAL ===
    /// 
    /// CelestialBody lưu 2 loại data:
    ///   initialPositionV3 = vị trí PHYSICS (AU thật) → dùng để tính gravity
    ///   initialVelocityV3 = vận tốc PHYSICS (AU/day thật) → dùng cho integration
    ///   transform.position = vị trí VISUAL (Unity units, nén) → chỉ để render
    ///
    /// Physics chạy ở khoảng cách thật:
    ///   Earth position = (1.0, 0, 0) AU → gravity đúng
    ///   Earth velocity = (0, 0, 0.01720) AU/day → quỹ đạo tròn
    ///
    /// Visual hiển thị ở khoảng cách nén:
    ///   Earth visual position = (3.5, 0, 0) Unity units → nhìn đẹp
    ///
    /// → Quỹ đạo vẫn CHÍNH XÁC vì gravity dùng physics position!
    /// → Visual chỉ "zoom" position để dễ nhìn, không ảnh hưởng physics.
    /// </summary>
    private void CreateBody(PlanetData.BodyInfo data)
    {
        // Tạo Sphere
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = data.name;
        obj.transform.parent = this.transform;

        // Visual scale (exaggerated - actual planet sizes are invisible at AU scale)
        float scale = data.visualScale * (settings != null ? settings.visualScaleMultiplier : 1f);
        obj.transform.localScale = Vector3.one * scale;

        // Màu sắc
        Renderer bodyRenderer = obj.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            // Tạo material mới để mỗi hành tinh có màu riêng
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = data.color;

            // Sun phát sáng (emission)
            if (data.name == "Sun")
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", data.color * 2f);
            }

            bodyRenderer.material = mat;
        }

        // Gắn CelestialBody component
        CelestialBody body = obj.AddComponent<CelestialBody>();
        body.bodyName = data.name;
        body.mass = data.mass;
        body.bodyRadius = data.radius;
        body.orbitColor = data.color;

        // === PHYSICS POSITIONS (khoảng cách THẬT - AU) ===
        // Đây là giá trị dùng để tính gravity — KHÔNG ĐƯỢC NÉN!
        float dist = (float)data.distanceFromSun;

        if (orbitalPlaneXZ)
        {
            // Quỹ đạo trên mặt phẳng XZ (Y = up)
            body.initialPositionV3 = new Vector3(dist, 0f, 0f);

            // Vận tốc vuông góc với hướng radial → quỹ đạo tròn
            body.initialVelocityV3 = new Vector3(0f, 0f, (float)data.orbitalVelocity);
        }
        else
        {
            // Quỹ đạo trên mặt phẳng XY (Z = up)
            body.initialPositionV3 = new Vector3(dist, 0f, 0f);
            body.initialVelocityV3 = new Vector3(0f, (float)data.orbitalVelocity, 0f);
        }

        // === VISUAL POSITION (khoảng cách NÉN - Unity units) ===
        // Chỉ ảnh hưởng render ban đầu, GravitySimulation sẽ cập nhật mỗi frame
        if (settings != null)
        {
            float visualDist = settings.RealToVisualDistance(data.distanceFromSun);
            if (orbitalPlaneXZ)
                obj.transform.position = new Vector3(visualDist, 0f, 0f);
            else
                obj.transform.position = new Vector3(visualDist, 0f, 0f);
        }
        else
        {
            obj.transform.position = body.initialPositionV3;
        }
    }

    /// <summary>
    /// Xoá tất cả hành tinh (để rebuild).
    /// </summary>
    public void ClearAll()
    {
        // Destroy tất cả child objects
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

