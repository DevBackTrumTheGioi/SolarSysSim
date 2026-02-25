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

    void OnGUI()
    {
        // Style setup
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };

        // === SIMULATION INFO (top-left) ===
        GUILayout.BeginArea(new Rect(10, 10, 320, 250));
        GUILayout.Label("☀ Solar System Simulation", titleStyle);
        
        if (settings != null)
        {
            // Mode indicator
            string modeText = settings.mode == SimulationSettings.SimMode.GameFriendly 
                ? "🎮 Game-Friendly (distances compressed)" 
                : "🔬 Realistic (true AU scale)";
            GUILayout.Label(modeText);

            GUILayout.Label($"Time Scale: {settings.timeScale:F1} days/sec");

            // Distance compression info (GameFriendly only)
            if (settings.mode == SimulationSettings.SimMode.GameFriendly)
            {
                GUILayout.Label($"Compression: r^{settings.compressionPower:F2}");
            }
        }

        // Time scale controls
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("◀ Slower")) 
            settings.timeScale = Mathf.Max(0.1f, settings.timeScale * 0.5f);
        if (GUILayout.Button("▶ Faster")) 
            settings.timeScale = Mathf.Min(1000f, settings.timeScale * 2f);
        if (GUILayout.Button("⏸ Pause")) 
            settings.timeScale = 0f;
        if (GUILayout.Button("▶ Play")) 
            settings.timeScale = 10f;
        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        // === HELP (top-right) ===
        if (showHelp)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 280, 10, 270, 240));
            GUI.Box(new Rect(0, 0, 270, 240), "");
            GUILayout.Label("  Controls:");
            GUILayout.Label("  Scroll: Zoom in/out");
            GUILayout.Label("  Right-click + Drag: Rotate");
            GUILayout.Label("  Left-click: Select planet");
            GUILayout.Label("  1-9: Quick select planet");
            GUILayout.Label("  Space: Reset camera");
            GUILayout.Label("  R: Reset simulation");
            GUILayout.Label("  H: Toggle this help");
            GUILayout.EndArea();
        }

        if (Input.GetKeyDown(KeyCode.H))
            showHelp = !showHelp;

        // Reset simulation
        if (Input.GetKeyDown(KeyCode.R) && simulation != null)
            simulation.ResetSimulation();

        // === SELECTED BODY INFO (bottom-left) ===
        if (simCamera != null && simCamera.target != null)
        {
            CelestialBody selected = simCamera.target.GetComponent<CelestialBody>();
            if (selected != null)
            {
                GUILayout.BeginArea(new Rect(10, Screen.height - 150, 380, 140));
                GUI.Box(new Rect(0, 0, 380, 140), "");
                
                GUIStyle nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                };
                GUILayout.Label($"  {selected.bodyName}", nameStyle);
                GUILayout.Label($"  Mass: {selected.mass:E3} M☉");
                GUILayout.Label($"  Physics Position: {selected.position}");
                
                double speed = selected.velocity.magnitude;
                double speedKmS = speed * 1731.5; // AU/day → km/s
                GUILayout.Label($"  Velocity: {speedKmS:F2} km/s ({speed:E4} AU/day)");
                
                double distFromSun = selected.position.magnitude;
                GUILayout.Label($"  Distance from Sun: {distFromSun:F4} AU");

                // Show visual distance too in GameFriendly mode
                if (settings != null && settings.mode == SimulationSettings.SimMode.GameFriendly)
                {
                    float visualDist = settings.RealToVisualDistance(distFromSun);
                    GUILayout.Label($"  Visual Distance: {visualDist:F2} units (compressed)");
                }
                
                GUILayout.EndArea();
            }
        }
    }
}

