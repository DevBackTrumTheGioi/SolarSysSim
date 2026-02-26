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
        GUILayout.BeginArea(new Rect(10, 10, 320, 300));
        GUI.Box(new Rect(0, 0, 320, 300), "", boxStyle); // Vẽ background xám phía sau nội dung
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

            GUILayout.Space(15);
            GUIStyle boldLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            GUILayout.Label("Display Settings:", boldLabel);
            
            // Friendly Toggles
            bool toggleOrbits = GUILayout.Toggle(settings.showOrbits, " Show Orbits (Trails)");
            if (toggleOrbits != settings.showOrbits)
            {
                settings.showOrbits = toggleOrbits;
                CelestialBody[] allBodies = FindObjectsOfType<CelestialBody>();
                foreach (var body in allBodies)
                {
                    LineRenderer lr = body.GetComponent<LineRenderer>();
                    if (lr != null) lr.enabled = settings.showOrbits;
                }
            }
            
            GUILayout.Space(5);
            settings.enableSunDrift = GUILayout.Toggle(settings.enableSunDrift, " Enable Sun Drift (Galaxy Motion)");
            if (settings.enableSunDrift)
            {
                GUILayout.Label($"  Sun Drift Speed: {settings.sunDriftSpeed:F3}");
                settings.sunDriftSpeed = GUILayout.HorizontalSlider(settings.sunDriftSpeed, 0f, 0.2f);
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
        if (simCamera != null && simCamera.target != null)
        {
            CelestialBody selected = simCamera.target.GetComponent<CelestialBody>();
            if (selected != null)
            {
                float infoWidth = 300f;
                float infoHeight = 150f;
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
                
                GUILayout.Label($"  Mass: {selected.mass:E3} M☉");
                
                double speed = selected.velocity.magnitude;
                double speedKmS = speed * 1731.5; // AU/day → km/s
                GUILayout.Label($"  Velocity: {speedKmS:F2} km/s");
                
                double distFromSun = selected.position.magnitude;
                GUILayout.Label($"  Distance from Sun: {distFromSun:F4} AU");
                
                GUILayout.EndArea();
            }
        }
    }
}

