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
    public void CreateSunLight()
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

        // Visual scale (Mặt Trời không bị ảnh hưởng bởi hệ số thu nhỏ biểu kiến)
        float scale = data.visualScale;
        if (data.name != "Sun" && settings != null)
        {
            scale *= settings.visualScaleMultiplier;
        }
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
        body.baseVisualScale = data.visualScale; // Lưu scale gốc để tính toán live sau này

        // Lưu reference Sun
        if (data.name == "Sun")
        {
            sunTransform = obj.transform;
        }

        // === PHYSICS POSITIONS (khoảng cách THẬT - AU) ===
        float dist = (float)data.distanceFromSun;
        
        // Tách random angle
        float randomAngle = 0f;
        if (data.name != "Sun")
        {
            randomAngle = Random.Range(0f, 2f * Mathf.PI);
        }

        // 1. Tính toán Toạ độ & Vận tốc ĐỘC LẬP TƯƠNG ĐỐI (Local Space) so với Mẹ
        Vector3 localPos = Vector3.zero;
        Vector3 localVel = Vector3.zero;
        if (orbitalPlaneXZ)
        {
            localPos = new Vector3(dist * Mathf.Cos(randomAngle), 0f, dist * Mathf.Sin(randomAngle));
            localVel = new Vector3(-(float)data.orbitalVelocity * Mathf.Sin(randomAngle), 0f, (float)data.orbitalVelocity * Mathf.Cos(randomAngle));
        }
        else
        {
            localPos = new Vector3(dist * Mathf.Cos(randomAngle), dist * Mathf.Sin(randomAngle), 0f);
            localVel = new Vector3(-(float)data.orbitalVelocity * Mathf.Sin(randomAngle), (float)data.orbitalVelocity * Mathf.Cos(randomAngle), 0f);
        }

        // 2. Chuyển đổi thành Toạ độ & Vận tốc TUYỆT ĐỐI (World Space) nếu có Mẹ
        if (!string.IsNullOrEmpty(data.orbitParentName))
        {
            // Tìm Transform hoặc CelestialBody của hành tinh mẹ đã được sinh ra trước đó
            Transform parentTransform = transform.Find(data.orbitParentName);
            if (parentTransform != null)
            {
                body.orbitParent = parentTransform; // Hook Mẹ - Con để render offset sau này
                
                CelestialBody parentCelestial = parentTransform.GetComponent<CelestialBody>();
                if (parentCelestial != null)
                {
                    // Lấy vị trí và vận tốc vật lý thực sự của Mẹ cộng dồn vào Con
                    body.initialPositionV3 = parentCelestial.initialPositionV3 + localPos;
                    body.initialVelocityV3 = parentCelestial.initialVelocityV3 + localVel;
                }
            }
            else 
            {
                Debug.LogWarning($"[SolarSystemBuilder] Không tìm thấy hành tinh mẹ {data.orbitParentName} cho {data.name}. Rơi về toạ độ độc lập quanh mặt trời.");
                body.initialPositionV3 = localPos;
                body.initialVelocityV3 = localVel;
            }
        }
        else 
        {
            // Hành tinh độc lập bay quanh Sun
            body.initialPositionV3 = localPos;
            body.initialVelocityV3 = localVel;
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

    /// <summary>
    /// Gọi để tái tạo hoàn toàn Cấu trúc Hệ mặt trời nguyên bản
    /// </summary>
    public void RebuildSystem()
    {
        ClearAll();
        BuildSolarSystem();
        CreateSunLight();
    }

    /// <summary>
    /// Bọn nhện quăng "Hành tinh Lang thang" (Rogue Planet) siêu nặng vào hệ thống
    /// </summary>
    public void SpawnRoguePlanet()
    {
        string bodyName = "Rogue Planet";
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = bodyName;
        obj.transform.parent = this.transform;

        Renderer bodyRenderer = obj.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.2f, 0.2f); // Màu Đỏ máu mộng tinh
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", mat.color * 2f);
            bodyRenderer.material = mat;
        }

        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Tạo Parameter
        CelestialBody body = obj.AddComponent<CelestialBody>();
        body.bodyName = bodyName;
        // Siêu Khổng Lồ: Bằng Nửa Mặt Trời (kéo tuột quỹ đạo sao Mộc)
        body.mass = 500.0; 
        body.bodyRadius = 0.003; 
        body.orbitColor = new Color(1f, 0.2f, 0.2f);
        body.baseVisualScale = 0.5f; 

        // Spawn ở rìa xa (50 AU) và bắn thẳng vào Tâm với Tốc độ Cực lớn
        body.initialPositionV3 = new Vector3(30f, 0f, 30f); 
        body.initialVelocityV3 = new Vector3(-0.5f, 0f, -0.5f); // 0.5 AU/ngày (Siêu thanh càn quét)

        // Inject vào System
        GravitySimulation gravSim = FindObjectOfType<GravitySimulation>();
        if (gravSim != null)
        {
            gravSim.AddDynamicBody(body);
        }
        else 
        {
            Debug.LogError("Chưa tìm thấy GravitySimulation để thêm Rogue Planet");
        }
    }

    /// <summary>
    /// Triệu hồi Bão Thiên Thạch đục khoét hệ mặt trời
    /// </summary>
    public void SpawnMeteorSwarm()
    {
        GravitySimulation gravSim = FindObjectOfType<GravitySimulation>();
        if (gravSim == null) return;

        int meteorCount = 20;
        float spawnRadius = 35f; // AU (Biên giới xa)

        for (int i = 0; i < meteorCount; i++)
        {
            string bodyName = "Meteor " + i;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = bodyName;
            obj.transform.parent = this.transform;

            Renderer bodyRenderer = obj.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.6f, 0.6f, 0.65f); // Đá xám
                bodyRenderer.material = mat;
            }

            Collider col = obj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            CelestialBody body = obj.AddComponent<CelestialBody>();
            body.bodyName = bodyName;
            // Thiên thạch thì khối lượng siêu bé gọn nhẹ
            body.mass = Random.Range(0.001f, 0.05f); 
            body.bodyRadius = Random.Range(0.0001f, 0.0005f); 
            body.orbitColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            body.baseVisualScale = 0.1f; 

            // Random điểm xuất phát trên Vòng móng ngựa bao quanh Hệ (khoảng 35AU)
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float height = Random.Range(-5f, 5f);
            Vector3 spawnPos = new Vector3(Mathf.Cos(angle) * spawnRadius, height, Mathf.Sin(angle) * spawnRadius);
            body.initialPositionV3 = spawnPos;

            // Chỉnh góc Cắm Đầu vào Tâm (Mặt Trời). Cộng thêm sai số nhiễu cho sinh động.
            Vector3 aimDir = -spawnPos.normalized;
            Vector3 scatter = Random.insideUnitSphere * 0.15f; 
            Vector3 velocityDir = (aimDir + scatter).normalized;
            
            float speed = Random.Range(0.2f, 0.8f); // AU/ngày (~ siêu thanh)
            body.initialVelocityV3 = velocityDir * speed;

            gravSim.AddDynamicBody(body);
        }
    }

    /// <summary>
    /// Triệu hồi Bão Thiên Thạch dội bom tập trung vào một Cụm Hành Tinh mục tiêu
    /// </summary>
    public void SpawnTargetedMeteorSwarm(CelestialBody targetBody)
    {
        if (targetBody == null) return;
        
        GravitySimulation gravSim = FindObjectOfType<GravitySimulation>();
        if (gravSim == null) return;

        int meteorCount = 25;
        float spawnRadius = 2.5f; // AU (Tạo ra khá gần nạn nhân để đảm bảo tỷ lệ trúng)
        Vector3 targetPos = targetBody.initialPositionV3; // Dùng vị trí khởi tạo làm gốc hoặc lấy vị trí hiện tại
        
        // Để nhắm trúng thì ta lấy vị trí hiện tại trên lưới Physics của nó thay vì initialPositionV3
        targetPos = targetBody.position.ToVector3();

        for (int i = 0; i < meteorCount; i++)
        {
            string bodyName = "Targeted Meteor " + i;
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = bodyName;
            obj.transform.parent = this.transform;

            Renderer bodyRenderer = obj.GetComponent<Renderer>();
            if (bodyRenderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.8f, 0.4f, 0.1f); // Đá rực lửa đỏ
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", mat.color * 1.5f);
                bodyRenderer.material = mat;
            }

            Collider col = obj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            CelestialBody body = obj.AddComponent<CelestialBody>();
            body.bodyName = bodyName;
            body.mass = Random.Range(0.005f, 0.1f);  // Lớn hơn xíu để tăng độ tàn phá
            body.bodyRadius = Random.Range(0.0002f, 0.0008f); 
            body.orbitColor = new Color(1f, 0.5f, 0f, 0.6f);
            body.baseVisualScale = 0.15f; 

            // Random điểm xuất phát dạng Cloud xoay quanh Nạn Nhân
            Vector3 randomOffset = Random.onUnitSphere * spawnRadius;
            Vector3 spawnPos = targetPos + randomOffset;
            body.initialPositionV3 = spawnPos;

            // Chỉnh góc Cắm Đầu vào Nạn Nhân
            Vector3 aimDir = (targetPos - spawnPos).normalized;
            Vector3 scatter = Random.insideUnitSphere * 0.05f; // Sai số nhỏ để đạn chụm lại
            Vector3 velocityDir = (aimDir + scatter).normalized;
            
            float speed = Random.Range(0.5f, 1.5f); // AU/ngày (Siêu tốc độ vũ trụ)
            body.initialVelocityV3 = velocityDir * speed;

            gravSim.AddDynamicBody(body);
        }
    }
}

