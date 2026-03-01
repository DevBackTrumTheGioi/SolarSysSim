using UnityEngine;

/// <summary>
/// UI đơn giản hiển thị thông tin simulation và điều khiển time scale.
/// Dùng OnGUI (legacy) để không cần Canvas setup - đại ca có thể nâng cấp lên UI Toolkit sau.
/// </summary>
public class SimulationUI : MonoBehaviour
{
    public GravitySimulation simulation;
    public SimulationSettings settings;
    public SimulationCamera simCamera;

    private bool showHelp = true;
    private Texture2D bgTexture;
    
    // Lưu tạm string nhập vào UI Editor
    private CelestialBody currentEditingBody;
    private string editMassStr = "1.0";
    private string editVelocityStr = "29.78";
    private string editDistanceStr = "";

    void OnGUI()
    {
        if (bgTexture == null)
        {
            bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 0.85f));
            bgTexture.Apply();
        }

        // Style setup
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = bgTexture;
        
        // === SIMULATION INFO (top-left) ===
        GUILayout.BeginArea(new Rect(10, 10, 320, 480));
        GUI.Box(new Rect(0, 0, 320, 480), "", boxStyle); // Vẽ background xám phía sau nội dung
        GUILayout.Label("☀ Solar System Simulation", titleStyle);
        
        if (settings != null)
        {
            GUILayout.Space(10);
            
            // Time Scale (Slider thay vì Button cứng nhắt)
            GUILayout.Label($"Time Scale: {settings.timeScale:F1} days/sec");
            settings.timeScale = GUILayout.HorizontalSlider(settings.timeScale, 0f, 400f);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("⏸ Pause")) settings.timeScale = 0f;
            if (GUILayout.Button("▶ Play (10x)")) settings.timeScale = 10f;
            GUILayout.EndHorizontal();

            // Gravity Multiplier
            GUILayout.Space(5);
            GUILayout.Label($"Gravity: {settings.gravityMultiplier:F1}x");
            settings.gravityMultiplier = GUILayout.HorizontalSlider(settings.gravityMultiplier, 0.1f, 10f);

            GUILayout.Space(15);
            GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("Display Settings:", boldLabel);
            
            // Visual Modes (Realistic vs Friendly)
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("☄ Friendly Mode"))
            {
                settings.visualScaleMultiplier = 0.55f;
            }
            if (GUILayout.Button("🔭 Realistic Mode"))
            {
                settings.visualScaleMultiplier = 0.05f;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            settings.showOrbits = GUILayout.Toggle(settings.showOrbits, " Show Orbits (Trails)");

            GUILayout.Space(5);
            settings.enableCollisions = GUILayout.Toggle(settings.enableCollisions, " Enable Physics Collisions");

            GUILayout.Space(5);
            settings.enableSunDrift = GUILayout.Toggle(settings.enableSunDrift, " Enable Sun Drift (Galaxy Motion)");
            if (settings.enableSunDrift)
            {
                GUILayout.Label($"  Sun Drift Speed: {settings.sunDriftSpeed:F3}");
                settings.sunDriftSpeed = GUILayout.HorizontalSlider(settings.sunDriftSpeed, 0f, 0.2f);
            }


            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("☄️ Mời Gọi Kẻ Hủy Diệt (Rogue Planet)", GUILayout.Height(30)))
            {
                SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
                if (builder != null) builder.SpawnRoguePlanet();
            }

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
            if (GUILayout.Button("🌧 Triệu Hỏi Mưa Thiên Thạch", GUILayout.Height(30)))
            {
                SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
                if (builder != null) builder.SpawnMeteorSwarm();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15);
            if (GUILayout.Button("🔄 Reset Planets to Default", GUILayout.Height(25)))
            {
                ResetAllPlanetsToDefault();
            }
        }
        GUILayout.EndArea();

        // === CONTROLS / HELP (bottom-center) ===
        if (showHelp)
        {
            // Tính toán vị trí ra giữa đáy màn hình
            float helpWidth = 700f;
            float helpHeight = 60f;
            float helpX = (Screen.width - helpWidth) / 2f;
            float helpY = Screen.height - helpHeight - 10f;
            
            GUILayout.BeginArea(new Rect(helpX, helpY, helpWidth, helpHeight));
            GUI.Box(new Rect(0, 0, helpWidth, helpHeight), "", boxStyle);
            GUILayout.BeginHorizontal();
            
            GUIStyle controlStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label("Scroll: Zoom", controlStyle);
            GUILayout.Label("|", controlStyle);
            GUILayout.Label("Right-click + Drag: Rotate", controlStyle);
            GUILayout.Label("|", controlStyle);
            GUILayout.Label("Left-click: Select planet", controlStyle);
            GUILayout.Label("|", controlStyle);
            GUILayout.Label("1-9: Quick select", controlStyle);
            GUILayout.Label("|", controlStyle);
            GUILayout.Label("Space: Reset cam", controlStyle);
            GUILayout.Label("|", controlStyle);
            GUILayout.Label("R: Reset Sim", controlStyle);
            GUILayout.Label("|", controlStyle);
            GUILayout.Label("H: Toggle Help", controlStyle);
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        if (Input.GetKeyDown(KeyCode.H))
            showHelp = !showHelp;

        // Reset simulation
        if (Input.GetKeyDown(KeyCode.R) && simulation != null)
            simulation.ResetSimulation();

        // === SELECTED BODY INFO (right-center) ===
        if (simCamera != null && simCamera.target != null && simCamera.target.gameObject != null && simCamera.target.gameObject.activeInHierarchy)
        {
            CelestialBody selected = simCamera.target.GetComponent<CelestialBody>();
            if (selected != null)
            {
                float infoWidth = 320f;
                float infoHeight = 265f;
                float infoX = Screen.width - infoWidth - 20f;
                float infoY = (Screen.height - infoHeight) / 2f;

                GUILayout.BeginArea(new Rect(infoX, infoY, infoWidth, infoHeight));
                GUI.Box(new Rect(0, 0, infoWidth, infoHeight), "", boxStyle);
                
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold
                };
                
                GUILayout.Space(5);
                GUILayout.Label($"  ❖ {selected.bodyName.ToUpper()}", nameStyle);
                GUILayout.Space(5);
                
                // Track body changes để reset text field
                if (currentEditingBody != selected)
                {
                    currentEditingBody = selected;
                    editMassStr = selected.mass.ToString("E3"); // Khoa học (ex: 3.003E-006)
                    double speedKmS_init = selected.velocity.magnitude * 1731.5;
                    editVelocityStr = speedKmS_init.ToString("F2");
                    
                    DoubleVector3 sunPos = default;
                    if (simulation != null && simulation.sunBody != null)
                        sunPos = simulation.sunBody.position;
                    
                    editDistanceStr = (selected.position - sunPos).magnitude.ToString("F4");
                }
                
                // === EDIT MASS ===
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Mass (x M☉):", GUILayout.Width(110));
                editMassStr = GUILayout.TextField(editMassStr);
                GUILayout.EndHorizontal();
                
                // === EDIT VELOCITY ===
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Velocity (km/s):", GUILayout.Width(110));
                editVelocityStr = GUILayout.TextField(editVelocityStr);
                GUILayout.EndHorizontal();
                
                // === EDIT DISTANCE ===
                GUILayout.BeginHorizontal();
                GUILayout.Label("  Distance/Spawn(AU):", GUILayout.Width(130));
                editDistanceStr = GUILayout.TextField(editDistanceStr);
                GUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                if (GUILayout.Button("⚡ APPLY ALTERS", GUILayout.Height(30)))
                {
                    ApplyEditing(selected);
                }
                
                GUILayout.Space(5);
                GUI.backgroundColor = new Color(1f, 0.5f, 0f);
                if (GUILayout.Button("☄️ Phóng Mưa Thiên Thạch (Dội Bom)", GUILayout.Height(30)))
                {
                    SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
                    if (builder != null) builder.SpawnTargetedMeteorSwarm(selected);
                }
                
                GUILayout.Space(5);
                GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
                if (GUILayout.Button("❌ Xóa Hành Tinh", GUILayout.Height(30)))
                {
                    GravitySimulation sim = FindObjectOfType<GravitySimulation>();
                    if (sim != null) sim.RemoveBody(selected);
                    simCamera.target = null; // Bỏ focus để đóng bảng UI
                }
                GUI.backgroundColor = Color.white;
                
                GUILayout.EndArea();
            }
        }
    }

    void ApplyEditing(CelestialBody body)
    {
        try 
        {
            // 1. Áp dụng khối lượng mới
            if (double.TryParse(editMassStr, out double newMass))
            {
                body.mass = newMass;
                if (settings != null) body.CheckBlackHole(settings);
            }

            // 2. Cập nhật Vector Vận tốc mới
            if (double.TryParse(editVelocityStr, out double newSpeedKmS))
            {
                double newSpeedAU = newSpeedKmS / 1731.5; // Đổi lại ra AU/day
                
                // Lấy hướng (direction) của vận tốc hiện tại
                DoubleVector3 direction = DoubleVector3.zero;
                if (body.velocity.sqrMagnitude > 1e-15)
                {
                    direction = body.velocity / body.velocity.magnitude;
                }
                else 
                {
                    // Nếu vật đang đứng yên (vel=0), gán đại một hướng rơi (như trục X) để chạy 
                    direction = new DoubleVector3(1, 0, 0); 
                }

                // Gán vận tốc mới
                body.velocity = direction * newSpeedAU;
            }

            // 3. Cập nhật Khoảng cách vật lí tới Vị Trí Mới & LƯU LÀM MỐC SPAWN
            if (double.TryParse(editDistanceStr, out double newDistanceAU))
            {
                DoubleVector3 sunPos = default;
                if (simulation != null && simulation.sunBody != null)
                    sunPos = simulation.sunBody.position;
                    
                DoubleVector3 dir = new DoubleVector3(1, 0, 0);
                DoubleVector3 diff = body.position - sunPos;
                if (diff.sqrMagnitude > 1e-15)
                {
                    dir = diff / diff.magnitude;
                }
                body.position = sunPos + dir * newDistanceAU;
                
                // Lưu khoảng cách này thành khoảng cách Spawn mặc định nếu reset
                PlanetData.UpdateSpawnDistance(body.bodyName, newDistanceAU);
            }

            // 4. Xoá trail cũ (vì quỹ đạo vừa bị thay đổi đột ngột)
            body.ClearTrail();
            
            Debug.Log($"[SimulationUI] Applied alters to {body.bodyName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SimulationUI] Lỗi parse dữ liệu. Vui lòng thử lại. Error: {e.Message}");
        }
    }

    void ResetAllPlanetsToDefault()
    {
        SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
        if (builder != null)
        {
            builder.RebuildSystem();
        }
        
        // Trả lại các Mode phá hoại không gian về mặc định
        if (settings != null)
        {
            settings.gravityMultiplier = 1.0f;
            settings.timeScale = 10f;
        }

        // Gọi hàm Render lại toàn bộ Physics từ Script cha
        if (simulation != null)
        {
            simulation.InitializeSimulation();
        }
        
        // Reset Text field cache để Panel Info tự vẽ lại
        currentEditingBody = null;
        
        Debug.Log("[SimulationUI] Restored all planetary masses and reset the simulation.");
    }
}

