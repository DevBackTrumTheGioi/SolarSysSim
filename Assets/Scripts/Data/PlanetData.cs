using UnityEngine;

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
        public double distanceFromSun; // AU (semi-major axis)
        public double orbitalVelocity; // AU/day (circular orbit approximation)
        public double radius;          // AU (actual physical radius)
        public Color color;            // Visual color
        public float visualScale;      // Visual sphere scale in Unity units

        public BodyInfo(string name, double mass, double dist, double vel, 
                       double radius, Color color, float visualScale)
        {
            this.name = name;
            this.mass = mass;
            this.distanceFromSun = dist;
            this.orbitalVelocity = vel;
            this.radius = radius;
            this.color = color;
            this.visualScale = visualScale;
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
        radius: 0.00465,        // 696,340 km = 0.00465 AU
        color: new Color(1f, 0.9f, 0.3f),
        visualScale: 0.8f       // Lớn nhất, nổi bật ở trung tâm
    );

    /// <summary>Sao Thuỷ - hành tinh gần Mặt Trời nhất</summary>
    public static readonly BodyInfo Mercury = new BodyInfo(
        "Mercury",
        mass: 1.659e-7,         // 3.301e23 kg
        dist: 0.387,            // 0.387 AU
        vel: 0.02765,           // √(G/0.387) = 0.02765 AU/day ≈ 47.87 km/s
        radius: 1.631e-5,       // 2,439 km
        color: new Color(0.7f, 0.7f, 0.7f),
        visualScale: 0.15f      // Nhỏ nhất
    );

    /// <summary>Sao Kim - "sao Mai/sao Hôm", kích thước gần bằng Trái Đất</summary>
    public static readonly BodyInfo Venus = new BodyInfo(
        "Venus",
        mass: 2.448e-6,         // 4.867e24 kg
        dist: 0.723,            // 0.723 AU
        vel: 0.02023,           // √(G/0.723) = 0.02023 AU/day ≈ 35.02 km/s
        radius: 4.045e-5,       // 6,052 km
        color: new Color(0.9f, 0.7f, 0.3f),
        visualScale: 0.25f      // Gần bằng Trái Đất
    );

    /// <summary>Trái Đất - nhà của chúng ta, 1 AU = chuẩn khoảng cách</summary>
    public static readonly BodyInfo Earth = new BodyInfo(
        "Earth",
        mass: 3.003e-6,         // 5.972e24 kg
        dist: 1.0,              // 1.0 AU (by definition)
        vel: 0.01720,           // √(G/1.0) = 0.01720 AU/day ≈ 29.78 km/s
        radius: 4.259e-5,       // 6,371 km
        color: new Color(0.2f, 0.5f, 1f),
        visualScale: 0.25f      // "Nhà" của chúng ta
    );

    /// <summary>Sao Hoả - hành tinh đỏ</summary>
    public static readonly BodyInfo Mars = new BodyInfo(
        "Mars",
        mass: 3.227e-7,         // 6.417e23 kg
        dist: 1.524,            // 1.524 AU
        vel: 0.01393,           // √(G/1.524) = 0.01393 AU/day ≈ 24.13 km/s
        radius: 2.266e-5,       // 3,390 km
        color: new Color(0.8f, 0.3f, 0.2f),
        visualScale: 0.2f       // Nhỏ hơn Trái Đất
    );

    /// <summary>Sao Mộc - hành tinh lớn nhất, "vacuum cleaner" của hệ mặt trời</summary>
    public static readonly BodyInfo Jupiter = new BodyInfo(
        "Jupiter",
        mass: 9.543e-4,         // 1.898e27 kg
        dist: 5.203,            // 5.203 AU
        vel: 0.007541,          // √(G/5.203) = 0.007541 AU/day ≈ 13.06 km/s
        radius: 4.673e-4,       // 69,911 km
        color: new Color(0.8f, 0.6f, 0.4f),
        visualScale: 0.5f       // Khổng lồ - gần bằng Sun visual
    );

    /// <summary>Sao Thổ - hành tinh có vành đai nổi tiếng</summary>
    public static readonly BodyInfo Saturn = new BodyInfo(
        "Saturn",
        mass: 2.857e-4,         // 5.683e26 kg
        dist: 9.537,            // 9.537 AU
        vel: 0.005572,          // √(G/9.537) = 0.005572 AU/day ≈ 9.65 km/s
        radius: 3.893e-4,       // 58,232 km
        color: new Color(0.9f, 0.8f, 0.5f),
        visualScale: 0.45f      // Gần bằng Jupiter
    );

    /// <summary>Sao Thiên Vương - nghiêng 98° so với mặt phẳng quỹ đạo</summary>
    public static readonly BodyInfo Uranus = new BodyInfo(
        "Uranus",
        mass: 4.366e-5,         // 8.681e25 kg
        dist: 19.19,            // 19.19 AU
        vel: 0.003927,          // √(G/19.19) = 0.003927 AU/day ≈ 6.80 km/s
        radius: 1.695e-4,       // 25,362 km
        color: new Color(0.5f, 0.8f, 0.9f),
        visualScale: 0.35f      // Hành tinh băng khổng lồ
    );

    /// <summary>Sao Hải Vương - hành tinh xa nhất</summary>
    public static readonly BodyInfo Neptune = new BodyInfo(
        "Neptune",
        mass: 5.151e-5,         // 1.024e26 kg
        dist: 30.07,            // 30.07 AU
        vel: 0.003137,          // √(G/30.07) = 0.003137 AU/day ≈ 5.43 km/s
        radius: 1.646e-4,       // 24,622 km
        color: new Color(0.3f, 0.4f, 0.9f),
        visualScale: 0.35f      // Gần bằng Uranus
    );

    /// <summary>
    /// Array tất cả hành tinh để dễ iterate.
    /// </summary>
    public static readonly BodyInfo[] AllBodies = new BodyInfo[]
    {
        Sun, Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune
    };
}

