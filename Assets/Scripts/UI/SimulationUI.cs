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

    public bool isMainMenu = true;

    private bool showUI = true;
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
        
        if (isMainMenu)
        {
            // Tuân lệnh đại ca: Pause mọi thứ về game khi ở Main Menu
            if (settings != null) settings.timeScale = 0f;
            
            DrawMainMenu(boxStyle, titleStyle);
            return;
        }

        // Toggle toàn bộ UI in-game bằng phím H
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H)
        {
            showUI = !showUI;
            Event.current.Use(); // Tiêu thụ event ngay để tránh nhảy đúp do OnGUI gọi nhiều lần/frame
        }

        if (!showUI) return;

        // === SIMULATION INFO (top-left) ===
        GUILayout.BeginArea(new Rect(10, 10, 320, 720));
        GUI.Box(new Rect(0, 0, 320, 720), "", boxStyle); // Vẽ background xám phía sau nội dung
        
        GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
        if (GUILayout.Button("⬅ Back to Menu", GUILayout.Height(30)))
        {
            isMainMenu = true;
            if (settings != null) settings.timeScale = 0f; // Tạm dừng game khi ra menu
            
            // Đảm bảo đưa mọi thứ về trạng thái tinh khôi
            ResetAllPlanetsToDefault();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);

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

            GUILayout.Space(5);
            ShootingStarSpawner starSpawner = FindObjectOfType<ShootingStarSpawner>();
            if (starSpawner != null)
            {
                starSpawner.enableShootingStars = GUILayout.Toggle(starSpawner.enableShootingStars, " ✨ Shooting Stars");
            }

            GUILayout.Space(5);
            settings.enableSunDrift = GUILayout.Toggle(settings.enableSunDrift, " Enable Sun Drift (Galaxy Motion)");
            if (settings.enableSunDrift)
            {
                GUILayout.Label($"  Sun Drift Speed: {settings.sunDriftSpeed:F3}");
                settings.sunDriftSpeed = GUILayout.HorizontalSlider(settings.sunDriftSpeed, 0f, 0.2f);
            }


            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("☄️ Spawn Rogue Planet", GUILayout.Height(30)))
            {
                SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
                if (builder != null) builder.SpawnRoguePlanet();
            }

            GUILayout.Space(5);
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
            if (GUILayout.Button("🌧 Spawn Meteor Swarm", GUILayout.Height(30)))
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
            
            // === CONTROLS INSTRUCTIONS ===
            GUILayout.Space(20);
            GUILayout.Label("Controls:", boldLabel);
            GUIStyle controlStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            GUILayout.Label("• Scroll: Zoom In/Out\n• Right Click + Drag: Rotate Camera\n• Left Click / Number 1-9: Select Planet\n• Ctrl + Number + Number: Measure Distance\n• Space: Reset Camera View\n• R: Reset Simulation (Reset positions to default)\n• H: Toggle UI Visibility", controlStyle);
        }
        GUILayout.EndArea();

        // === CONTROLS / HELP (bottom-center) ===
        // Reset simulation - chỉ hoạt động khi không ở Menu
        if (Input.GetKeyDown(KeyCode.R) && !isMainMenu)
        {
            ResetAllPlanetsToDefault();
        }

        // === SELECTED BODY INFO (top-right) ===
        if (simCamera != null && simCamera.target != null && simCamera.target.gameObject != null && simCamera.target.gameObject.activeInHierarchy)
        {
            CelestialBody selected = simCamera.target.GetComponent<CelestialBody>();
            if (selected != null)
            {
                float infoWidth = 320f;
                float infoHeight = 490f; // Cao hơn để chứa hiển thị parameters & description
                float infoX = Screen.width - infoWidth - 20f;
                float infoY = 10f; // Đưa lên góc trên cùng bên phải theo ý user

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
                if (GUILayout.Button("☄️ Targeted Meteor Strike", GUILayout.Height(30)))
                {
                    SolarSystemBuilder builder = FindObjectOfType<SolarSystemBuilder>();
                    if (builder != null) builder.SpawnTargetedMeteorSwarm(selected);
                }
                
                GUILayout.Space(5);
                GUI.backgroundColor = new Color(0.9f, 0.2f, 0.2f);
                if (GUILayout.Button("❌ Delete Planet", GUILayout.Height(30)))
                {
                    GravitySimulation sim = FindObjectOfType<GravitySimulation>();
                    if (sim != null) sim.RemoveBody(selected);
                    simCamera.target = null; // Bỏ focus để đóng bảng UI
                }
                GUI.backgroundColor = Color.white;
                
                // === REALTIME INFO ===
                GUILayout.Space(15);
                GUIStyle localBold = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
                GUILayout.Label("--- REALTIME INFO ---", localBold);
                
                double realSpeedKmS = selected.velocity.magnitude * 1731.5;
                DoubleVector3 sunPosition = default;
                if (simulation != null && simulation.sunBody != null) sunPosition = simulation.sunBody.position;
                double realDistanceAU = (selected.position - sunPosition).magnitude;
                
                GUILayout.Label($"Speed: {realSpeedKmS:F2} km/s");
                GUILayout.Label($"Distance to Sun: {realDistanceAU:F4} AU");
                GUILayout.Label($"Mass: {selected.mass:E3} M☉");
                
                // === DESCRIPTION ===
                GUILayout.Space(15);
                GUILayout.Label("--- DESCRIPTION ---", localBold);
                GUIStyle descStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 13 };
                GUILayout.Label(GetPlanetDescription(selected.bodyName), descStyle);
                
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
        // 1. Phục hồi mảng Data cứng về đúng như ban đầu (Mass, Distance, Velocity chuẩn)
        PlanetData.ResetToDefaults();
        
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

    void DrawMainMenu(GUIStyle boxStyle, GUIStyle titleStyle)
    {
        // Fallback đen xám nguyên thủy (Bỏ ảnh nền theo ý đại ca)
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "", boxStyle);

        float menuWidth = 400f;
        float menuHeight = 250f;
        float menuX = (Screen.width - menuWidth) / 2f;
        float menuY = (Screen.height - menuHeight) / 2f;

        GUILayout.BeginArea(new Rect(menuX, menuY, menuWidth, menuHeight));
        GUI.Box(new Rect(0, 0, menuWidth, menuHeight), "", boxStyle);
        
        GUILayout.BeginVertical();
        GUILayout.Space(20);
        
        GUIStyle mainTitleStyle = new GUIStyle(titleStyle)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.Label("☀ SOLAR SYSTEM SIMULATION", mainTitleStyle);
        
        GUILayout.Space(30);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };

        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("▶ START GAME", buttonStyle, GUILayout.Height(50)))
        {
            isMainMenu = false;
            // Tuân lệnh đại ca: Start game thì set speed về 15 day/s
            if (settings != null)
            {
                settings.timeScale = 15f;
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(15);

        GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
        if (GUILayout.Button("❌ EXIT TO DESKTOP", buttonStyle, GUILayout.Height(50)))
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private string GetPlanetDescription(string name)
    {
        switch (name)
        {
            case "Sun": return "A G-type main-sequence star. The center of our solar system.";
            case "Mercury": return "The smallest and closest planet to the Sun. A cratered, sun-scorched world.";
            case "Venus": return "The second planet, wrapped in a thick, toxic atmosphere. Hottest planet in the system.";
            case "Earth": return "Our home planet. The only place we know of so far that's inhabited by living things.";
            case "Mars": return "The Red Planet. A dusty, cold, desert world with a very thin atmosphere.";
            case "Jupiter": return "The largest planet. A gas giant with a Great Red Spot and dozens of moons.";
            case "Saturn": return "A gas giant adorned with a dazzling, complex system of icy rings.";
            case "Uranus": return "An ice giant that rotates on its side. It has a blue-green color from methane.";
            case "Neptune": return "The most distant major planet. Dark, cold, and whipped by supersonic winds.";
            default: return "An unknown celestial body floating in the vastness of space.";
        }
    }
}

