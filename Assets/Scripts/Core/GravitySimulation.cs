using UnityEngine;

/// <summary>
/// N-Body Gravity Simulation Manager
/// 
/// === THUẬT TOÁN: VELOCITY VERLET (Störmer-Verlet) ===
/// 
/// Đây là phương pháp tích phân symplectic, nghĩa là nó BẢO TOÀN NĂNG LƯỢNG
/// theo thời gian, không giống Euler integration (năng lượng tăng dần → quỹ đạo xoắn ra).
/// 
/// Velocity Verlet là tiêu chuẩn trong mô phỏng thiên văn, được dùng bởi:
/// - Universe Sandbox
/// - Sebastian Lague's Solar System
/// - NASA JPL Horizons (biến thể)
/// - Hầu hết n-body simulator trên GitHub
/// 
/// === CÔNG THỨC ===
/// 
/// Newton's Law of Universal Gravitation:
///   F = G × m₁ × m₂ / r²
///   → Acceleration on body i from body j:
///   a_i = G × m_j / r² × r̂_ij
///   (hướng từ i về phía j)
///
/// Velocity Verlet Integration (mỗi timestep dt):
///   1. x(t+dt) = x(t) + v(t)·dt + 0.5·a(t)·dt²    [position update]
///   2. a(t+dt) = computeGravity(x(t+dt))             [new acceleration]
///   3. v(t+dt) = v(t) + 0.5·(a(t) + a(t+dt))·dt     [velocity update]
///
/// === TỐI ƯU HOÁ ===
/// - SubStepping: Chia mỗi FixedUpdate thành nhiều sub-steps nhỏ hơn
///   → Chính xác hơn mà không cần giảm FixedUpdate interval
/// - Softening: Thêm ε vào r² để tránh lực vô cực khi 2 body quá gần
///   → a = G × m / (r² + ε)
/// - Pairwise optimization: Tính F_ij một lần, áp dụng cho cả 2 body (Newton 3rd law)
///   → Giảm computation từ N² xuống N(N-1)/2
/// </summary>
public class GravitySimulation : MonoBehaviour
{
    [Header("=== SETTINGS ===")]
    [Tooltip("Kéo SimulationSettings ScriptableObject vào đây")]
    public SimulationSettings settings;

    [Header("=== RUNTIME INFO (Read Only) ===")]
    [SerializeField] private int bodyCount;
    [SerializeField] private double totalEnergy;
    [SerializeField] private float simulationDays;

    // Cached references
    private CelestialBody[] bodies;
    private CelestialBody sunBody; // Tham chiếu Mặt Trời để làm gốc visual
    private bool isInitialized = false;

    // Pre-allocated arrays to avoid GC
    private DoubleVector3[] newAccelerations;

    private SimulationCamera simCamera;

    void Start()
    {
        simCamera = FindObjectOfType<SimulationCamera>();
        InitializeSimulation();
    }

    /// <summary>
    /// Tìm tất cả CelestialBody trong scene và khởi tạo simulation.
    /// </summary>
    public void InitializeSimulation()
    {
        bodies = FindObjectsOfType<CelestialBody>();
        bodyCount = bodies.Length;

        if (bodyCount == 0)
        {
            Debug.LogWarning("[GravitySimulation] Không tìm thấy CelestialBody nào trong scene!");
            return;
        }

        // Tìm Mặt Trời để dùng làm gốc tham chiếu visual
        sunBody = null;
        for (int i = 0; i < bodyCount; i++)
        {
            if (bodies[i].bodyName == "Sun")
            {
                sunBody = bodies[i];
                break;
            }
        }
        if (sunBody == null)
        {
            // Fallback: dùng body nặng nhất làm "Sun"
            double maxMass = 0;
            for (int i = 0; i < bodyCount; i++)
            {
                if (bodies[i].mass > maxMass) { maxMass = bodies[i].mass; sunBody = bodies[i]; }
            }
        }

        // Allocate arrays
        newAccelerations = new DoubleVector3[bodyCount];

        // Initialize each body
        for (int i = 0; i < bodyCount; i++)
        {
            bodies[i].Initialize();
            bodies[i].SetupTrail(settings);
            // Tính số điểm trail = đúng 1 vòng quỹ đạo
            int pts = CalcOrbitPoints(bodies[i]);
            bodies[i].SetOrbitMaxPoints(pts);
        }

        // Compute initial accelerations (needed for Verlet step 1)
        ComputeAllAccelerations(bodies);
        for (int i = 0; i < bodyCount; i++)
        {
            bodies[i].acceleration = newAccelerations[i];
        }

        isInitialized = true;
        Debug.Log($"[GravitySimulation] Initialized with {bodyCount} bodies. G = {settings.gravitationalConstant}");
    }

    // === FIX 1 & 2: Giới hạn dt tối đa mỗi sub-step để tránh energy drift và orbit thẳng ===
    // Giảm từ 0.5 xuống 0.05 để đảm bảo ở tốc độ rất cao (125), các hành tinh như Mercury
    // không bị nhảy bước quá lớn trong tích phân vật lý gây ra rung lắc quỹ đạo.
    private const double MAX_DT_PER_STEP = 0.05; // days

    void FixedUpdate()
    {
        if (!isInitialized || bodies == null || bodyCount == 0) return;

        // Tính dt cho mỗi sub-step
        // timeScale = số ngày Earth per giây real-time
        // Time.fixedDeltaTime = giây real-time per FixedUpdate
        double totalDt = settings.timeScale * Time.fixedDeltaTime; // days per FixedUpdate

        // === FIX 1 & 2: Tự động tăng subSteps khi timeScale lớn ===
        // Đảm bảo mỗi sub-step không vượt quá MAX_DT_PER_STEP
        // → quỹ đạo giữ nguyên hình tròn, không drift vào Mặt Trời, không thành đường thẳng
        int dynamicSubSteps = Mathf.Max(settings.subSteps, Mathf.CeilToInt((float)(totalDt / MAX_DT_PER_STEP)));
        double subDt = totalDt / dynamicSubSteps;

        for (int step = 0; step < dynamicSubSteps; step++)
        {
            VelocityVerletStep(subDt);
        }

        // === SUN DRIFT: Dịch TẤT CẢ bodies lên trên trục Y cùng tốc độ ===
        // Mô phỏng hệ Mặt Trời di chuyển trong thiên hà.
        // Vì tất cả dịch cùng vector → khoảng cách tương đối KHÔNG ĐỔI → gravity KHÔNG bị ảnh hưởng.
        // Trail ghi world position → tự tạo hình xoắn ốc 3D đẹp mắt.
        if (settings.enableSunDrift && settings.sunDriftSpeed > 0f)
        {
            // Áp dụng Time Scale multiplier (totalDt = Time.fixedDeltaTime * timeScale)
            double driftY = settings.sunDriftSpeed * totalDt;
            DoubleVector3 drift = new DoubleVector3(0, driftY, 0);
            for (int i = 0; i < bodyCount; i++)
            {
                bodies[i].position = bodies[i].position + drift;
            }
        }

        // Update visual positions + trail
        DoubleVector3 sunPhysicsPos = sunBody != null ? sunBody.position : DoubleVector3.zero;

        // === Sun phải update visual TRƯỚC để các hành tinh khác lấy sunVisualPos đúng ===
        // Sun visual position = physics position trực tiếp (không relative vì nó là gốc)
        if (sunBody != null)
        {
            if (settings.mode == SimulationSettings.SimMode.GameFriendly)
            {
                // GameFriendly: Sun ở gốc XZ=0, nhưng giữ Y từ drift
                sunBody.transform.position = new Vector3(0f, (float)sunPhysicsPos.y, 0f);
            }
            else
            {
                sunBody.transform.position = sunPhysicsPos.ToVector3();
            }
        }

        Transform focusTarget = simCamera != null ? simCamera.target : null;
        Vector3 sunVisualPos = sunBody != null ? sunBody.transform.position : Vector3.zero;
        
        for (int i = 0; i < bodyCount; i++)
        {
            // Chỉ cập nhật Visual rotation khi tốc độ thời gian thật chậm (timeScale < 1)
            // để tránh người xem bị chóng mặt khi mô phỏng ở tốc độ cao
            if (settings.timeScale < 1f)
            {
                bodies[i].UpdateRotation(settings.timeScale);
            }

            // Sun đã update ở trên, skip update position again
            if (bodies[i] == sunBody) continue;

            bodies[i].UpdateVisualPosition(settings, sunPhysicsPos, sunVisualPos);
            bodies[i].AddOrbitPoint(bodies[i].transform.position);
        }


        // Track simulation time
        simulationDays += (float)totalDt;

        // Calculate total energy for debugging (conservation check)
        totalEnergy = ComputeTotalEnergy();
    }

    /// <summary>
    /// Tính số điểm trail cần thiết để hiển thị đúng 1 vòng quỹ đạo.
    /// Kepler's 3rd: T (days) = 365.25 × a^1.5  (a = semi-major axis AU)
    /// Points = T (days) / timeScale (days/sec) × 50 (fps) = số frame cho 1 vòng
    /// </summary>
    private int CalcOrbitPoints(CelestialBody body)
    {
        double dist = body.position.magnitude; // AU từ Mặt Trời
        if (dist < 0.01) return 50; // Sun hoặc quá gần

        // Kepler's 3rd law: T (years) = a^1.5 → T (days) = 365.25 × a^1.5
        double periodDays = 365.25 * System.Math.Pow(dist, 1.5);

        // Số FixedUpdate frame để hoàn thành 1 vòng
        float fixedFps = 1f / Time.fixedDeltaTime; // ~50
        float timeScaleSafe = Mathf.Max(settings.timeScale, 0.1f);
        int points = Mathf.RoundToInt((float)(periodDays / timeScaleSafe) * fixedFps);

        return Mathf.Clamp(points, 60, 2000);
    }

    /// <summary>
    /// Một bước Velocity Verlet integration.
    /// 
    /// Đây là trái tim của simulation. Thuật toán 3 bước:
    /// 1. Cập nhật position dùng velocity + acceleration hiện tại
    /// 2. Tính acceleration mới từ positions mới  
    /// 3. Cập nhật velocity dùng trung bình acceleration cũ và mới
    /// </summary>
    private void VelocityVerletStep(double dt)
    {
        double halfDt = 0.5 * dt;
        double halfDtSq = 0.5 * dt * dt;

        // === STEP 1: Update positions ===
        // x(t+dt) = x(t) + v(t)·dt + 0.5·a(t)·dt²
        for (int i = 0; i < bodyCount; i++)
        {
            CelestialBody body = bodies[i];
            body.position = body.position
                + body.velocity * dt
                + body.acceleration * halfDtSq;
        }

        // === STEP 2: Compute new accelerations from updated positions ===
        ComputeAllAccelerations(bodies);

        // === STEP 3: Update velocities using average of old and new accelerations ===
        // v(t+dt) = v(t) + 0.5·(a(t) + a(t+dt))·dt
        for (int i = 0; i < bodyCount; i++)
        {
            CelestialBody body = bodies[i];
            body.velocity = body.velocity + (body.acceleration + newAccelerations[i]) * halfDt;
            body.acceleration = newAccelerations[i];
        }
    }

    /// <summary>
    /// Tính gia tốc hấp dẫn cho TẤT CẢ bodies.
    /// 
    /// Sử dụng Newton's 3rd Law optimization:
    /// F_ij = -F_ji, nên chỉ cần tính N(N-1)/2 cặp thay vì N².
    /// 
    /// Công thức cho body i do body j:
    ///   a_i += G × m_j × (pos_j - pos_i) / (|pos_j - pos_i|² + ε)^(3/2)
    /// 
    /// Lưu ý: dùng r³ ở mẫu số (không phải r²) vì đã nhân với vector đơn vị r̂ = r/|r|.
    ///   F/m = G × M / r² × r̂ = G × M × r⃗ / r³
    /// </summary>
    private void ComputeAllAccelerations(CelestialBody[] allBodies)
    {
        double G = settings.gravitationalConstant;
        double softening = settings.softeningFactor;

        // Reset accelerations
        for (int i = 0; i < bodyCount; i++)
        {
            newAccelerations[i] = DoubleVector3.zero;
        }

        // Pairwise computation (Newton's 3rd Law optimization)
        for (int i = 0; i < bodyCount; i++)
        {
            for (int j = i + 1; j < bodyCount; j++)
            {
                // Vector từ body i đến body j
                DoubleVector3 rij = allBodies[j].position - allBodies[i].position;

                // Khoảng cách bình phương + softening
                double distSqr = rij.sqrMagnitude + softening;

                // 1/r³ = 1/(r² × r) = 1/(r² × sqrt(r²))
                double invDistCube = 1.0 / (distSqr * System.Math.Sqrt(distSqr));

                // Gia tốc = G × m × r⃗ / r³
                // Body i bị kéo về phía j: a_i += G × m_j × rij / r³
                // Body j bị kéo về phía i: a_j -= G × m_i × rij / r³ (Newton 3rd Law)
                DoubleVector3 forceDirection = rij * invDistCube;

                newAccelerations[i] = newAccelerations[i] + forceDirection * (G * allBodies[j].mass);
                newAccelerations[j] = newAccelerations[j] - forceDirection * (G * allBodies[i].mass);
            }
        }
    }

    /// <summary>
    /// Tính tổng năng lượng (Kinetic + Potential) của hệ.
    /// Trong hệ bảo toàn, tổng năng lượng phải gần như không đổi.
    /// Nếu năng lượng drift > 1%, cần giảm dt hoặc tăng sub-steps.
    /// 
    /// E_kinetic = 0.5 × m × v²
    /// E_potential = -G × m_i × m_j / r
    /// E_total = ΣE_kinetic + ΣE_potential (should be constant)
    /// </summary>
    private double ComputeTotalEnergy()
    {
        double G = settings.gravitationalConstant;
        double kinetic = 0;
        double potential = 0;

        for (int i = 0; i < bodyCount; i++)
        {
            // Kinetic energy: 0.5 × m × v²
            kinetic += 0.5 * bodies[i].mass * bodies[i].velocity.sqrMagnitude;

            // Potential energy: -G × mi × mj / r (for each pair)
            for (int j = i + 1; j < bodyCount; j++)
            {
                double dist = DoubleVector3.Distance(bodies[i].position, bodies[j].position);
                if (dist > 1e-10)
                {
                    potential -= G * bodies[i].mass * bodies[j].mass / dist;
                }
            }
        }

        return kinetic + potential;
    }

    /// <summary>
    /// Reset simulation về trạng thái ban đầu.
    /// </summary>
    public void ResetSimulation()
    {
        simulationDays = 0;
        for (int i = 0; i < bodyCount; i++)
        {
            bodies[i].Initialize();
            bodies[i].ClearTrail();
        }

        // Recompute initial accelerations
        ComputeAllAccelerations(bodies);
        DoubleVector3 sunPhysicsPos = sunBody != null ? sunBody.position : DoubleVector3.zero;
        if (sunBody != null)
        {
            if (settings.mode == SimulationSettings.SimMode.GameFriendly)
                sunBody.transform.position = new Vector3(0f, (float)sunPhysicsPos.y, 0f);
            else
                sunBody.transform.position = sunPhysicsPos.ToVector3();
        }
        Vector3 sunVisualPos = sunBody != null ? sunBody.transform.position : Vector3.zero;
        for (int i = 0; i < bodyCount; i++)
        {
            bodies[i].acceleration = newAccelerations[i];
            if (bodies[i] != sunBody)
                bodies[i].UpdateVisualPosition(settings, sunPhysicsPos, sunVisualPos);
        }
    }
}

