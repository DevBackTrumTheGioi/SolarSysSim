using UnityEngine;

/// <summary>
/// Script tạo ra một dải sao lấp lánh khổng lồ bao quanh Camera.
/// Nó sẽ tự động sinh ra hàng ngàn đốm sao 3D ngẫu nhiên và luôn bám theo góc nhìn của bạn,
/// tạo ra cảm giác không gian vũ trụ vô cực chân thực nhất.
/// </summary>
public class StarfieldBackground : MonoBehaviour
{
    [Header("=== STAR SETTINGS ===")]
    [Tooltip("Số lượng ngôi sao trên bầu trời")]
    public int maxStars = 10000;
    
    [Tooltip("Kích thước hạt sao")]
    public float starSize = 0.5f;
    
    [Tooltip("Bán kính màng cầu sao bao quanh camera (phải lớn hơn max camera zoom)")]
    public float starDistance = 500f; 

    [Header("=== SHOOTING STARS ===")]
    public bool enableShootingStars = true;
    public int shootingStarCount = 50;
    public float shootingStarSpeed = 200f;

    private ParticleSystem particleSys;
    private ParticleSystem shootingParticleSys;
    private ParticleSystem.Particle[] stars;
    private Transform starTransform;

    void Start()
    {
        // Tạo một object con độc lập để giữ các ngôi sao
        GameObject starObj = new GameObject("StarfieldSphere");
        starTransform = starObj.transform;
        
        // Setup ParticleSystem
        particleSys = starObj.AddComponent<ParticleSystem>();
        
        var main = particleSys.main;
        var emission = particleSys.emission;
        var shape = particleSys.shape;

        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = maxStars;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        emission.enabled = false;
        shape.enabled = false;

        // Dùng Material mặc định, vòng tròn, không đổ bóng, tự sáng
        ParticleSystemRenderer pRenderer = particleSys.GetComponent<ParticleSystemRenderer>();
        pRenderer.material = new Material(Shader.Find("Sprites/Default"));

        stars = new ParticleSystem.Particle[maxStars];
        CreateStars();

        // TẠO HỆ THỐNG SAO BĂNG (Shooting Stars / Flying Stars)
        if (enableShootingStars)
        {
            GameObject sObj = new GameObject("ShootingStars");
            sObj.transform.parent = starObj.transform;
            shootingParticleSys = sObj.AddComponent<ParticleSystem>();
            
            var sMain = shootingParticleSys.main;
            sMain.loop = true;
            sMain.playOnAwake = true;
            sMain.simulationSpace = ParticleSystemSimulationSpace.Local;
            sMain.maxParticles = shootingStarCount;
            sMain.startSpeed = new ParticleSystem.MinMaxCurve(shootingStarSpeed * 0.5f, shootingStarSpeed);
            sMain.startLifetime = new ParticleSystem.MinMaxCurve(2f, 6f);
            sMain.startSize = new ParticleSystem.MinMaxCurve(starSize * 0.5f, starSize * 2f);
            sMain.startColor = new Color(0.85f, 0.95f, 1f, 0.7f);

            var sEmission = shootingParticleSys.emission;
            sEmission.enabled = true;
            sEmission.rateOverTime = shootingStarCount / 3f;

            var sShape = shootingParticleSys.shape;
            sShape.enabled = true;
            sShape.shapeType = ParticleSystemShapeType.Sphere;
            sShape.radius = starDistance * 1.2f;

            var sRenderer = shootingParticleSys.GetComponent<ParticleSystemRenderer>();
            sRenderer.material = pRenderer.material; // Dùng chung material đốm sáng
            sRenderer.renderMode = ParticleSystemRenderMode.Stretch; // Kéo dãn theo chiều dọc (vệt sao lướt)
            sRenderer.lengthScale = 5f; 

            shootingParticleSys.Play();
        }
    }

    void CreateStars()
    {
        for (int i = 0; i < maxStars; i++)
        {
            // Phân bổ sao ngẫu nhiên trên một vỏ cầu khổng lồ bao quanh tâm
            Vector3 pos = Random.onUnitSphere * Random.Range(starDistance * 0.9f, starDistance * 1.5f);

            stars[i].position = pos;
            stars[i].startSize = Random.Range(starSize * 0.2f, starSize);
            
            // Random màu sắc để bầu trời chân thực hơn (xanh lam, cam nhạt, trắng)
            float colorType = Random.value;
            Color c = Color.white;
            if (colorType > 0.8f) c = new Color(0.6f, 0.85f, 1f);      // Hơi xanh lam
            else if (colorType < 0.2f) c = new Color(1f, 0.85f, 0.6f); // Hơi vàng/đỏ nhạt
            
            c.a = Random.Range(0.2f, 1f); // Độ sáng (opacity) khác nhau
            stars[i].startColor = c;
            
            // Khởi tạo thời gian sống cực dài để các ngôi sao sống mãi
            stars[i].startLifetime = Mathf.Infinity;
            stars[i].remainingLifetime = Mathf.Infinity;
        }
        
        particleSys.SetParticles(stars, stars.Length);
        particleSys.Play(); // Ép hệ thống render hạt
    }

    void LateUpdate()
    {
        // Luôn kéo khối sao bám theo vị trí của Camera để tạo cảm giác không gian vô tận.
        // Nhưng KHÔNG BỊ XOAY THEO Camera để nền sao luôn cố định trong mắt nhìn.
        if (Camera.main != null && starTransform != null)
        {
            starTransform.position = Camera.main.transform.position;
            starTransform.rotation = Quaternion.identity; 
        }
    }
}
