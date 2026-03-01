﻿using UnityEngine;

/// <summary>
/// Dữ liệu thực tế của các hành tinh trong Hệ Mặt Trời.
/// 
/// === NGUỒN DỮ LIỆU ===
/// - NASA JPL Horizons System (https://ssd.jpl.nasa.gov/horizons/)
/// - IAU 2012 nominal solar/planetary constants
/// 
/// === HỆ ĐƠN VỊ ===
/// - Khoảng cách: AU (1 AU = 149,597,870,700 m)
/// - Khối lượng: Solar Mass (M☉ = 1.98892 × 10³⁰ kg)
/// - Vận tốc: AU/day
/// - G = 2.9592e-4 AU³/(M☉·day²)
///
/// === VẬN TỐC QUỸ ĐẠO TRÒN ===
/// Công thức: v = √(G × M_sun / r)
/// Với M_sun = 1 M☉, G = 2.9592e-4:
///   v = √(2.9592e-4 / r) AU/day
///
/// Earth: v = √(2.9592e-4 / 1.0) = 0.01720 AU/day ≈ 29.78 km/s ✓
/// 
/// === VẤN ĐỀ KHOẢNG CÁCH TRONG GAME ===
/// Thực tế: Mercury 0.387 AU → Neptune 30.07 AU = chênh 78 lần!
///   Nếu Mercury = 1cm trên màn hình → Neptune ở 78cm → không vừa màn hình
///   Hành tinh thật bé: Trái Đất = 0.0000426 AU = VÔ HÌNH
///
/// Giải pháp (dùng trong SimulationSettings.GameFriendly mode):
///   1. PHYSICS: chạy ở khoảng cách THẬT (mass, dist, vel đều thật)
///      → Gravity đúng, quỹ đạo đúng
///   2. VISUAL: nén khoảng cách bằng power function
///      → visual_dist = base + multiplier × real_dist^power
///   3. PLANET SIZE: phóng to gấp hàng nghìn lần so với thực tế
///      → visualScale là kích thước Unity sphere, KHÔNG liên quan physics
///
/// === LÀM SAO HÀNH TINH XOAY QUANH MẶT TRỜI? ===
/// 1. Đặt hành tinh ở khoảng cách r từ Mặt Trời
/// 2. Cho vận tốc ban đầu VUÔNG GÓC với hướng Mặt Trời - Hành tinh
/// 3. Độ lớn vận tốc = √(G × M_sun / r) cho quỹ đạo tròn
/// 4. Lực hấp dẫn sẽ liên tục kéo hành tinh về phía Mặt Trời
/// 5. Nhưng vận tốc tiếp tuyến giữ cho hành tinh không rơi vào
/// → Kết quả: quỹ đạo ellipse (hoặc gần tròn nếu v đúng)
/// 
/// Đây chính là Kepler's First Law tự nhiên xuất hiện từ F = ma + Newton gravity!
/// </summary>
public static class PlanetData
{
    /// <summary>
    /// Struct chứa dữ liệu một thiên thể.
    /// </summary>
    public struct BodyInfo
    {
        public string name;
        public double mass;            // Solar masses
        public double distanceFromSun; // AU (current/initial spawn distance)
        public double defaultSpawnDistance; // AU (to reset to true original without losing data)
        public double orbitalVelocity; // AU/day (circular orbit approximation)
        public double radius;          // AU (actual physical radius)
        public Color color;            // Visual color
        public float visualScale;      // Visual sphere scale in Unity units
        public float axialTilt;        // Degrees (tilt of the rotation axis)
        public float rotationPeriod;   // Earth Days for one full rotation
        public string orbitParentName; // Name of the parent body (e.g., "Earth" for Moon). Null for orbiting Sun.
        public string prefabPath;      // Đường dẫn prefab từ gói "Planets of the Solar System 3D"

        public BodyInfo(string name, double mass, double dist, double vel, 
                       double radius, Color color, float visualScale, 
                       float axialTilt = 0f, float rotationPeriod = 1f, 
                       string orbitParentName = null, string prefabPath = null)
        {
            this.name = name;
            this.mass = mass;
            this.distanceFromSun = dist;
            this.defaultSpawnDistance = dist;
            this.orbitalVelocity = vel;
            this.radius = radius;
            this.color = color;
            this.visualScale = visualScale;
            this.axialTilt = axialTilt;
            this.rotationPeriod = rotationPeriod;
            this.orbitParentName = orbitParentName;
            this.prefabPath = prefabPath;
        }
    }

    // ==================== GRAVITATIONAL CONSTANT ====================
    // G = 6.674e-11 m³/(kg·s²) → converted to AU³/(M☉·day²)
    // G_sim = G × M☉ × (86400²) / (AU³)
    //       = 6.674e-11 × 1.989e30 × 7.4649e9 / (3.348e33)
    //       ≈ 2.9592e-4
    public const double G_SIM = 2.9592e-4;

    // ==================== PLANETARY DATA ====================
    // Orbital velocities calculated as v = √(G_SIM × M_sun / r)
    // cho quỹ đạo gần tròn. Giá trị khớp với NASA JPL data.

    /// <summary>Mặt Trời - trung tâm hệ, khối lượng = 1 M☉</summary>
    public static readonly BodyInfo Sun = new BodyInfo(
        "Sun",
        mass: 1.0,
        dist: 0.0,
        vel: 0.0,
        radius: 0.00465,
        color: new Color(1f, 0.9f, 0.3f),
        visualScale: 0.8f,
        axialTilt: 7.25f,
        rotationPeriod: 25.05f, // ~25 days at equator
        prefabPath: "Planets of the Solar System 3D/Prefabs/Sun"
    );

    /// <summary>Sao Thuỷ - hành tinh gần Mặt Trời nhất</summary>
    public static readonly BodyInfo Mercury = new BodyInfo(
        "Mercury",
        mass: 1.659e-7,
        dist: 0.387,
        vel: 0.02765,
        radius: 1.631e-5,
        color: new Color(0.7f, 0.7f, 0.7f),
        visualScale: 0.15f,
        axialTilt: 0.034f,
        rotationPeriod: 58.646f,
        prefabPath: "Planets of the Solar System 3D/Prefabs/Mercury"
    );

    /// <summary>Sao Kim - "sao Mai/sao Hôm", kích thước gần bằng Trái Đất</summary>
    public static readonly BodyInfo Venus = new BodyInfo(
        "Venus",
        mass: 2.448e-6,
        dist: 0.723,
        vel: 0.02023,
        radius: 4.045e-5,
        color: new Color(0.9f, 0.7f, 0.3f),
        visualScale: 0.25f,
        axialTilt: 177.36f, // Retrograde rotation
        rotationPeriod: -243.025f, // Negative means opposite direction
        prefabPath: "Planets of the Solar System 3D/Prefabs/Venus"
    );

    /// <summary>Trái Đất - nhà của chúng ta, 1 AU = chuẩn khoảng cách</summary>
    public static readonly BodyInfo Earth = new BodyInfo(
        "Earth",
        mass: 3.003e-6,
        dist: 1.0,
        vel: 0.01720, 
        radius: 4.259e-5,
        color: new Color(0.2f, 0.5f, 1f),
        visualScale: 0.25f,
        axialTilt: 23.44f,
        rotationPeriod: 1.0f, // 1 Earth day
        orbitParentName: null, // Quay quanh Sun mặc định
        prefabPath: "Planets of the Solar System 3D/Prefabs/Earth"
    );

    /// <summary>Mặt Trăng - Vệ tinh 1 của Trái Đất (Khoảng cách nén nhỏ, mass nhỏ)</summary>
    public static readonly BodyInfo Moon = new BodyInfo(
        "Moon",
        mass: 3.69e-8,       // ~0.0123 Earth masses
        dist: 0.00257,       // ~384,400 km
        vel: 0.00059,        // ~1.022 km/s (relative to Earth)
        radius: 1.162e-5,    // Mỏng mảnh
        color: new Color(0.8f, 0.8f, 0.8f),
        visualScale: 0.08f,  // Nhỏ gọn
        axialTilt: 1.54f,    // Gần như đứng thẳng
        rotationPeriod: 27.32f, // Khoá thuỷ triều
        orbitParentName: "Earth", // CÚ PHÁP ĐẶC BIỆT LÀM MẶT TRĂNG
        prefabPath: "Planets of the Solar System 3D/Prefabs/Moon" // Bạn có thể kéo thả Prefab Mặt trăng vào nếu có
    );

    /// <summary>Sao Hoả - hành tinh đỏ</summary>
    public static readonly BodyInfo Mars = new BodyInfo(
        "Mars",
        mass: 3.227e-7,
        dist: 1.524,
        vel: 0.01393,
        radius: 2.266e-5,
        color: new Color(0.8f, 0.3f, 0.2f),
        visualScale: 0.2f,
        axialTilt: 25.19f,
        rotationPeriod: 1.026f, // 24.6 hours
        prefabPath: "Planets of the Solar System 3D/Prefabs/Mars"
    );

    /// <summary>Sao Mộc - hành tinh lớn nhất</summary>
    public static readonly BodyInfo Jupiter = new BodyInfo(
        "Jupiter",
        mass: 9.543e-4,
        dist: 5.203,
        vel: 0.007541,
        radius: 4.673e-4,
        color: new Color(0.8f, 0.6f, 0.4f),
        visualScale: 0.5f,
        axialTilt: 3.13f,
        rotationPeriod: 0.413f, // 9.9 hours
        prefabPath: "Planets of the Solar System 3D/Prefabs/Jupiter"
    );

    /// <summary>Sao Thổ - hành tinh có vành đai nổi tiếng</summary>
    public static readonly BodyInfo Saturn = new BodyInfo(
        "Saturn",
        mass: 2.857e-4,
        dist: 9.537,
        vel: 0.005572,
        radius: 3.893e-4,
        color: new Color(0.9f, 0.8f, 0.5f),
        visualScale: 0.45f,
        axialTilt: 26.73f,
        rotationPeriod: 0.444f, // 10.7 hours
        prefabPath: "Planets of the Solar System 3D/Prefabs/Saturn"
    );

    /// <summary>Sao Thiên Vương - nghiêng 98° so với mặt phẳng quỹ đạo</summary>
    public static readonly BodyInfo Uranus = new BodyInfo(
        "Uranus",
        mass: 4.366e-5,
        dist: 19.19,
        vel: 0.003927,
        radius: 1.695e-4,
        color: new Color(0.5f, 0.8f, 0.9f),
        visualScale: 0.35f,
        axialTilt: 97.77f, // Rolls on its side
        rotationPeriod: -0.718f, // 17.2 hours (retrograde inside tilt)
        prefabPath: "Planets of the Solar System 3D/Prefabs/Uranus"
    );

    /// <summary>Sao Hải Vương - hành tinh xa nhất</summary>
    public static readonly BodyInfo Neptune = new BodyInfo(
        "Neptune",
        mass: 5.151e-5,
        dist: 30.07,
        vel: 0.003137,
        radius: 1.646e-4,
        color: new Color(0.3f, 0.4f, 0.9f),
        visualScale: 0.35f,
        axialTilt: 28.32f,
        rotationPeriod: 0.671f, // 16.1 hours
        prefabPath: "Planets of the Solar System 3D/Prefabs/Neptune"
    );

    /// <summary>
    /// Array tất cả hành tinh để dễ iterate. Không để readonly nội dung array.
    /// </summary>
    public static BodyInfo[] AllBodies = new BodyInfo[]
    {
        Sun, Mercury, Venus, Earth, Moon, Mars, Jupiter, Saturn, Uranus, Neptune
    };

    public static void UpdateSpawnDistance(string bodyName, double distance)
    {
        for (int i = 0; i < AllBodies.Length; i++)
        {
            if (AllBodies[i].name == bodyName)
            {
                AllBodies[i].distanceFromSun = distance;
                break;
            }
        }
    }
}

