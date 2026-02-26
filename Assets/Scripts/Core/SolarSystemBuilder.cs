using UnityEngine;

/// <summary>
/// Script khởi tạo toàn bộ Hệ Mặt Trời từ PlanetData.
/// Dùng prefab từ gói "Planets of the Solar System 3D".
/// Tạo Directional Light phát từ Mặt Trời.
/// Set background đen + Skybox đen.
/// </summary>
public class SolarSystemBuilder : MonoBehaviour
{
    [Header("=== SETTINGS ===")]
    public SimulationSettings settings;

    [Header("=== PREFABS (kéo prefab từ Planets of the Solar System 3D/Prefabs vào đây) ===")]
    [Tooltip("Kéo prefab hành tinh vào đây. Script sẽ match theo tên (Sun, Mercury, Venus, ...)")]
    public GameObject[] planetPrefabs;

    [Header("=== OPTIONS ===")]
    [Tooltip("Hướng 'lên' của mặt phẳng quỹ đạo. Mặc định: Y-up (XZ plane).")]
    public bool orbitalPlaneXZ = true;

    // Directional Light sẽ follow Sun mỗi frame
    private Light sunLight;
    private Transform sunTransform;

    void Awake()
    {
        SetupBackground();
        BuildSolarSystem();
        CreateSunLight();
    }

    void LateUpdate()
    {
        // Directional Light luôn follow Sun position
        if (sunLight != null && sunTransform != null)
        {
            sunLight.transform.position = sunTransform.position;
        }
    }

    /// <summary>
    /// Set background đen: Camera + Skybox.
    /// </summary>
    private void SetupBackground()
    {
        // Camera background đen
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // Tắt skybox → background = camera color = đen (vũ trụ)
        RenderSettings.skybox = null;

        // Ambient light rất tối — không gian vũ trụ
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.08f);
    }

    /// <summary>
    /// Tìm prefab trong planetPrefabs array theo tên (case-insensitive contains).
    /// VD: data.name = "Sun" → match prefab "Sun", "Sun Sphere", etc.
    /// </summary>
    private GameObject FindPrefab(string bodyName)
    {
        if (planetPrefabs == null || planetPrefabs.Length == 0) return null;

        string lowerName = bodyName.ToLower();
        foreach (var prefab in planetPrefabs)
        {
            if (prefab != null && prefab.name.ToLower().Contains(lowerName))
                return prefab;
        }
        return null;
    }

    /// <summary>
    /// Tạo Directional Light phát từ vị trí Mặt Trời.
    /// Ánh sáng chiếu ra mọi hướng (mô phỏng ánh sáng Mặt Trời).
    /// </summary>
    private void CreateSunLight()
    {
        // Xóa Directional Light mặc định nếu có
        Light[] existingLights = FindObjectsOfType<Light>();
        foreach (var l in existingLights)
        {
            if (l.type == LightType.Directional && l.gameObject != this.gameObject)
            {
                Destroy(l.gameObject);
            }
        }

        // Tạo Point Light ở vị trí Sun — chiếu sáng tất cả hành tinh xung quanh
        GameObject lightObj = new GameObject("Sun_PointLight");
        lightObj.transform.parent = this.transform;
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = new Color(1f, 0.95f, 0.8f); // Ánh sáng ấm của Mặt Trời
        pointLight.intensity = 2f;
        pointLight.range = 100f; // Đủ xa để chiếu tới Neptune
        pointLight.shadows = LightShadows.None; // Tắt shadow cho performance

        if (sunTransform != null)
            lightObj.transform.position = sunTransform.position;

        sunLight = pointLight;
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
    /// Tạo một thiên thể: Instantiate prefab từ gói, fallback về Sphere nếu không tìm thấy.
    /// </summary>
    private void CreateBody(PlanetData.BodyInfo data)
    {
        GameObject obj = null;

        // === Tìm prefab trong array theo tên ===
        GameObject prefab = FindPrefab(data.name);
        if (prefab != null)
        {
            obj = Instantiate(prefab, this.transform);
            obj.name = data.name;

            // Xóa các script có sẵn trong prefab (tránh xung đột với simulation)
            foreach (var s in obj.GetComponentsInChildren<MonoBehaviour>())
                Destroy(s);

            Debug.Log($"[SolarSystemBuilder] Loaded prefab for {data.name}");
        }

        // === Fallback: tạo Sphere primitive nếu không có prefab ===
        if (obj == null)
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = data.name;
            obj.transform.parent = this.transform;

            // Material fallback
            Renderer bodyRenderer = obj.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = data.color;

                if (data.name == "Sun")
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", data.color * 2f);
                }

                bodyRenderer.material = mat;
            }

            Debug.LogWarning($"[SolarSystemBuilder] Prefab not found for {data.name}, using sphere fallback.");
        }

        // Visual scale
        float scale = data.visualScale * (settings != null ? settings.visualScaleMultiplier : 1f);
        obj.transform.localScale = Vector3.one * scale;

        // Xóa Collider (không cần physics collision trong simulation)
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Gắn CelestialBody component
        CelestialBody body = obj.AddComponent<CelestialBody>();
        body.bodyName = data.name;
        body.mass = data.mass;
        body.bodyRadius = data.radius;
        body.orbitColor = data.color;

        // Lưu reference Sun
        if (data.name == "Sun")
        {
            sunTransform = obj.transform;
        }

        // === PHYSICS POSITIONS (khoảng cách THẬT - AU) ===
        float dist = (float)data.distanceFromSun;
        
        // Tạo góc ngẫu nhiên trên quỹ đạo (thử dùng seed tự do, chỉ áp dụng nếu không phải Mặt Trời)
        float randomAngle = 0f;
        if (data.name != "Sun")
        {
            randomAngle = Random.Range(0f, 2f * Mathf.PI);
        }

        if (orbitalPlaneXZ)
        {
            body.initialPositionV3 = new Vector3(dist * Mathf.Cos(randomAngle), 0f, dist * Mathf.Sin(randomAngle));
            body.initialVelocityV3 = new Vector3(-(float)data.orbitalVelocity * Mathf.Sin(randomAngle), 0f, (float)data.orbitalVelocity * Mathf.Cos(randomAngle));
        }
        else
        {
            body.initialPositionV3 = new Vector3(dist * Mathf.Cos(randomAngle), dist * Mathf.Sin(randomAngle), 0f);
            body.initialVelocityV3 = new Vector3(-(float)data.orbitalVelocity * Mathf.Sin(randomAngle), (float)data.orbitalVelocity * Mathf.Cos(randomAngle), 0f);
        }

        // === VISUAL POSITION (khoảng cách NÉN - Unity units) ===
        if (settings != null)
        {
            float visualDist = settings.RealToVisualDistance(data.distanceFromSun);
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
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}

