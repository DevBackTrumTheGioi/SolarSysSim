using UnityEngine;

/// <summary>
/// Spawner tạo các vì sao băng (shooting stars) bay ngang vũ trụ.
/// Sao băng KHÔNG tham gia gravity — chỉ là hiệu ứng visual thuần túy.
/// Dùng TrailRenderer để tạo vệt sáng lưu luyến trên bầu trời.
/// Gắn script này vào GameObject bất kỳ (VD: cùng SolarSystemBuilder).
/// </summary>
public class ShootingStarSpawner : MonoBehaviour
{
    [Header("=== SHOOTING STAR SETTINGS ===")]
    [Tooltip("Bật/tắt spawn sao băng")]
    public bool enableShootingStars = true;

    [Tooltip("Khoảng cách spawn tối thiểu (Unity visual units)")]
    public float minSpawnDistance = 12f;

    [Tooltip("Khoảng cách spawn tối đa (Unity visual units)")]
    public float maxSpawnDistance = 25f;

    [Tooltip("Thời gian tối thiểu giữa các lần spawn (giây)")]
    public float minSpawnInterval = 0.3f;

    [Tooltip("Thời gian tối đa giữa các lần spawn (giây)")]
    public float maxSpawnInterval = 1.5f;

    [Tooltip("Tốc độ bay tối thiểu (Unity units/s)")]
    public float minSpeed = 8f;

    [Tooltip("Tốc độ bay tối đa (Unity units/s)")]
    public float maxSpeed = 20f;

    [Tooltip("Thời gian sống tối thiểu (giây)")]
    public float minLifetime = 1.0f;

    [Tooltip("Thời gian sống tối đa (giây)")]
    public float maxLifetime = 2.5f;

    private float nextSpawnTime;

    void Start()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        if (!enableShootingStars) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnShootingStar();
            nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    /// <summary>
    /// Spawn một sao băng tại vị trí ngẫu nhiên, bay theo hướng ngẫu nhiên, tự hủy.
    /// </summary>
    private void SpawnShootingStar()
    {
        // === 1. TẠO GAMEOBJECT NHỎ GỌN ===
        GameObject star = new GameObject("ShootingStar");
        
        // Vị trí: random trên mặt cầu xung quanh camera/gốc
        Vector3 cameraPos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        float spawnDist = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPos = cameraPos + Random.onUnitSphere * spawnDist;
        star.transform.position = spawnPos;

        // === 2. MESH NHỎ (sphere tí hon) ĐỂ TRAIL CÓ CHỖ BÁM ===
        // Dùng MeshFilter + MeshRenderer thay vì CreatePrimitive để tránh tạo Collider
        MeshFilter mf = star.AddComponent<MeshFilter>();
        MeshRenderer mr = star.AddComponent<MeshRenderer>();

        // Tạo mesh sphere nhỏ xíu
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mf.sharedMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempSphere);

        // Scale siêu nhỏ (gần vô hình, chỉ trail là quan trọng)
        star.transform.localScale = Vector3.one * 0.02f;

        // Material phát sáng (build-safe: thử nhiều shader)
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");
        if (litShader == null) litShader = Shader.Find("Sprites/Default");
        if (litShader != null)
        {
            Material starMat = new Material(litShader);
            float colorRand = Random.value;
            Color starColor;
            if (colorRand < 0.5f)
                starColor = new Color(1f, 1f, 0.9f);
            else if (colorRand < 0.8f)
                starColor = new Color(1f, 0.85f, 0.5f);
            else
                starColor = new Color(0.6f, 0.8f, 1f);
            
            starMat.color = starColor;
            if (litShader.name.Contains("Lit") || litShader.name == "Standard")
            {
                starMat.EnableKeyword("_EMISSION");
                starMat.SetColor("_EmissionColor", starColor * 3f);
            }
            mr.material = starMat;

        // === 3. TRAIL RENDERER ===
        TrailRenderer trail = star.AddComponent<TrailRenderer>();
        float lifetime = Random.Range(minLifetime, maxLifetime);
        trail.time = lifetime * 0.8f; // Trail tồn tại gần bằng lifetime
        trail.startWidth = Random.Range(0.03f, 0.08f);
        trail.endWidth = 0f;

        // Material trail (build-safe)
        SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
        if (builder != null && builder.particleMaterial != null)
            trail.material = builder.particleMaterial;
        else
        {
            Shader trailShader = Shader.Find("Sprites/Default");
            if (trailShader != null) trail.material = new Material(trailShader);
        }
        
        // Gradient: sáng rực ở đầu → mờ dần → trong suốt ở đuôi
        Gradient gradient = new Gradient();
        Color trailColor = starColor;
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),      // Đầu trắng sáng chói
                new GradientColorKey(trailColor, 0.3f),     // Chuyển sang màu sao
                new GradientColorKey(trailColor * 0.5f, 1f) // Đuôi tối dần
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        trail.colorGradient = gradient;
        trail.numCornerVertices = 3;
        trail.numCapVertices = 3;
        trail.minVertexDistance = 0.05f;

        // === 4. GẮN SCRIPT DI CHUYỂN ===
        ShootingStarMover mover = star.AddComponent<ShootingStarMover>();
        
        // Hướng bay: ngẫu nhiên nhưng hơi chéo (không bay thẳng vào camera)
        Vector3 flyDir = Random.onUnitSphere;
        // Thêm chút cong nhẹ bằng cách xoay hướng bay
        flyDir = Quaternion.Euler(
            Random.Range(-30f, 30f),
            Random.Range(-30f, 30f),
            0
        ) * flyDir;
        
        mover.direction = flyDir.normalized;
        mover.speed = Random.Range(minSpeed, maxSpeed);
        mover.lifetime = lifetime;

        // Tự hủy sau lifetime + trail time (chờ trail tan hết)
        Destroy(star, lifetime + trail.time + 0.5f);
        }
        else
        {
            // Không tìm thấy shader nào — skip spawn
            Destroy(star);
        }
    }
}

/// <summary>
/// Component đơn giản di chuyển sao băng theo hướng cố định.
/// Không tham gia GravitySimulation — chỉ pure visual.
/// </summary>
public class ShootingStarMover : MonoBehaviour
{
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public float speed;
    [HideInInspector] public float lifetime;

    private float elapsed = 0f;

    void Update()
    {
        elapsed += Time.deltaTime;

        // Bay thẳng theo hướng, tốc độ không đổi
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // Tắt MeshRenderer sau khi hết lifetime (chỉ giữ trail bay tiếp cho đẹp)
        if (elapsed >= lifetime)
        {
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }
    }
}
