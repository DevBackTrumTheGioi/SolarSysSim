# ☀️ Solar System Simulation — Hướng Dẫn Triển Khai & Test

> **Unity Project:** SolarSysSim
> **Ngày tạo:** 25/02/2026
> **Engine:** Unity 2022+ (URP)

---

## 📑 Mục Lục

1. [Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
2. [Nền Tảng Vật Lý](#2-nền-tảng-vật-lý)
3. [Hệ Đơn Vị & Tại Sao Không Dùng SI](#3-hệ-đơn-vị--tại-sao-không-dùng-si)
4. [Vấn Đề Khoảng Cách & Giải Pháp Nén](#4-vấn-đề-khoảng-cách--giải-pháp-nén)
5. [Cấu Trúc File & Script](#5-cấu-trúc-file--script)
6. [Hướng Dẫn Setup Từng Bước](#6-hướng-dẫn-setup-từng-bước)
7. [Dữ Liệu Hành Tinh](#7-dữ-liệu-hành-tinh)
8. [Hướng Dẫn Test Từng Thiên Thể](#8-hướng-dẫn-test-từng-thiên-thể)
9. [Tinh Chỉnh Thông Số](#9-tinh-chỉnh-thông-số)
10. [Xử Lý Lỗi Thường Gặp](#10-xử-lý-lỗi-thường-gặp)
11. [Mở Rộng](#11-mở-rộng)

---

## 1. Tổng Quan Kiến Trúc

### Nguyên tắc cốt lõi: PHYSICS ≠ VISUAL

```
┌──────────────────────────────────────────────────────────┐
│                    PHYSICS LAYER                         │
│  (double precision, khoảng cách thật AU)                 │
│                                                          │
│  CelestialBody.position  → dùng để tính gravity         │
│  CelestialBody.velocity  → dùng cho integration         │
│  GravitySimulation       → Velocity Verlet mỗi frame    │
│                                                          │
│  Earth position = (1.0, 0, 0) AU ← LUÔN ĐÚNG           │
├──────────────────────────────────────────────────────────┤
│              ↓ PhysicsToVisualPosition() ↓               │
├──────────────────────────────────────────────────────────┤
│                    VISUAL LAYER                          │
│  (float, Unity units, khoảng cách NÉN)                  │
│                                                          │
│  transform.position → chỉ để render                     │
│  TrailRenderer      → vẽ quỹ đạo                        │
│                                                          │
│  Earth visual = (3.5, 0, 0) units ← NÉN CHO ĐẸP        │
└──────────────────────────────────────────────────────────┘
```

**Tại sao phải tách?**

- Nếu dùng khoảng cách nén để tính gravity → `F ∝ 1/r²` sẽ sai → quỹ đạo méo, bay lung tung
- Physics chạy ở khoảng cách thật → gravity đúng → quỹ đạo tự nhiên đúng Kepler
- Visual chỉ "zoom" position cho dễ nhìn, không ảnh hưởng tính toán

---

## 2. Nền Tảng Vật Lý

### 2.1 Newton's Law of Universal Gravitation

```
F = G × m₁ × m₂ / r²
```

Mỗi thiên thể hút MỌI thiên thể khác (N-body simulation).
Gia tốc của body `i` do body `j` gây ra:

```
a_i = G × m_j × (pos_j - pos_i) / |pos_j - pos_i|³
```

> **Lưu ý:** Dùng `r³` (không phải `r²`) vì đã nhân với vector đơn vị `r̂ = r⃗/|r⃗|`

### 2.2 Velocity Verlet Integration (Symplectic)

Đây là thuật toán tích phân **bảo toàn năng lượng**, tiêu chuẩn trong mô phỏng thiên văn.

**Tại sao không dùng Euler?**

| Phương pháp | Ưu điểm | Nhược điểm |
|---|---|---|
| **Euler** | Đơn giản | Năng lượng TĂNG dần → quỹ đạo xoắn ra, hành tinh bay mất |
| **Velocity Verlet** | Bảo toàn năng lượng | Phức tạp hơn chút |
| **RK4 (Runge-Kutta)** | Rất chính xác | Nặng gấp 4×, không symplectic |

**Thuật toán 3 bước mỗi timestep `dt`:**

```
STEP 1 — Cập nhật vị trí:
    x(t+dt) = x(t) + v(t)·dt + 0.5·a(t)·dt²

STEP 2 — Tính gia tốc mới từ vị trí mới:
    a(t+dt) = computeGravity(x(t+dt))

STEP 3 — Cập nhật vận tốc (trung bình gia tốc cũ + mới):
    v(t+dt) = v(t) + 0.5·(a(t) + a(t+dt))·dt
```

### 2.3 Tại Sao Trái Đất Xoay Quanh Mặt Trời?

```
        vận tốc tiếp tuyến →
        ─────────────►
        ┌─────┐
        │Earth│
        └──┬──┘
           │  ← lực hấp dẫn kéo về Sun
           │
           ▼
        ┌─────┐
        │ Sun │
        └─────┘
```

1. Trái Đất ở vị trí `(1.0, 0, 0)` AU
2. Vận tốc ban đầu `(0, 0, 0.01720)` AU/day — **VUÔNG GÓC** với hướng về Sun
3. Gravity liên tục kéo Earth về phía Sun (centripetal force)
4. Nhưng vận tốc tiếp tuyến giữ cho Earth không rơi vào
5. → Quỹ đạo tròn/ellipse tự nhiên xuất hiện!

**Công thức vận tốc quỹ đạo tròn:**

```
v_circular = √(G × M_sun / r)
```

- Nếu `v > v_circular` → quỹ đạo ellipse dẹt ra
- Nếu `v < v_circular` → quỹ đạo ellipse bẹp lại
- Nếu `v = v_circular` → quỹ đạo tròn hoàn hảo

### 2.4 Tối Ưu Hoá

| Kỹ thuật | Mô tả | Hiệu quả |
|---|---|---|
| **Newton 3rd Law** | `F_ij = -F_ji`, tính 1 lần dùng cho 2 body | N² → N(N-1)/2 |
| **Sub-stepping** | Chia 1 FixedUpdate thành 4-8 sub-steps | Chính xác hơn, không nặng hơn nhiều |
| **Softening** | `a = G·m / (r² + ε)` tránh chia cho 0 | Tránh lực vô cực khi gần |
| **Double precision** | DoubleVector3 cho physics | Tránh mất precision ở khoảng cách lớn |

---

## 3. Hệ Đơn Vị & Tại Sao Không Dùng SI

### Vấn đề với SI (mét, kg, giây):

```
Khối lượng Sun       = 1,989,000,000,000,000,000,000,000,000,000 kg  (2×10³⁰)
Khoảng cách Earth-Sun = 149,597,870,700 m                            (1.5×10¹¹)
G                     = 0.0000000000667 m³/(kg·s²)                   (6.67×10⁻¹¹)
```

> Float chỉ có 7 chữ số precision → **MẤT DỮ LIỆU** khi tính `G × M_sun`!

### Hệ đơn vị Simulation:

| Đại lượng | Đơn vị SI | Đơn vị Sim | Hệ số chuyển đổi |
|---|---|---|---|
| **Khoảng cách** | mét | **AU** (1 AU = 1.496×10¹¹ m) | ÷ 1.496e11 |
| **Khối lượng** | kg | **Solar Mass M☉** (= 1.989×10³⁰ kg) | ÷ 1.989e30 |
| **Thời gian** | giây | **Earth Day** (= 86,400 s) | ÷ 86400 |
| **G** | 6.674×10⁻¹¹ | **2.9592×10⁻⁴** AU³/(M☉·day²) | Derived |

**Kết quả:** Tất cả giá trị nằm trong khoảng `0.0001 → 1000` — hoàn hảo cho float/double!

### Cách tính G_sim:

```
G_sim = G_SI × M☉_kg × (86400 s/day)² / (AU_m)³
      = 6.674e-11 × 1.989e30 × 7.4649e9 / 3.348e33
      ≈ 2.9592e-4 AU³/(M☉·day²)
```

---

## 4. Vấn Đề Khoảng Cách & Giải Pháp Nén

### Vấn đề thực tế:

```
Hệ Mặt Trời THẬT (đúng tỉ lệ):

Sun ●                                                              · Neptune
     · Mercury                                                     (30 AU)
       · Venus
         · Earth (1 AU)
           · Mars
                          · Jupiter (5.2 AU)
                                        · Saturn (9.5 AU)
                                                         · Uranus (19.2 AU)
```

- Mercury → Neptune: chênh **78 lần**
- Nếu Sun = quả bóng rổ → Earth = hạt đậu cách 26m, Neptune cách 780m!
- Hành tinh nhỏ xíu: Earth radius = 0.0000426 AU → **VÔ HÌNH**

### Giải pháp: Power Function Compression

```
visual_dist = baseDistance + distanceMultiplier × real_dist ^ compressionPower
```

**Default settings:** `base=1.5, multiplier=2.0, power=0.45`

| Hành tinh | Khoảng cách thật (AU) | Visual (Unity units) | Tỉ lệ nén |
|---|---|---|---|
| Mercury | 0.387 | **2.78** | — |
| Venus | 0.723 | **3.20** | — |
| Earth | 1.000 | **3.50** | — |
| Mars | 1.524 | **3.89** | — |
| Jupiter | 5.203 | **5.87** | — |
| Saturn | 9.537 | **7.36** | — |
| Uranus | 19.19 | **9.53** | — |
| Neptune | 30.07 | **11.13** | — |

**Chênh lệch Neptune/Mercury:** 78× (thật) → **4×** (visual) → Fit hết trong camera!

### 2 Chế độ trong SimulationSettings:

| Mode | Khoảng cách | Khi nào dùng |
|---|---|---|
| `Realistic` | 1:1 (AU = Unity unit) | Giáo dục, nghiên cứu, video khoa học |
| `GameFriendly` | Nén bằng power function | Game, demo, presentation |

---

## 5. Cấu Trúc File & Script

```
Assets/Scripts/
├── Core/
│   ├── DoubleVector3.cs        — Vector3 double precision (tránh mất precision)
│   ├── SimulationSettings.cs   — ScriptableObject cấu hình (G, timeScale, compression)
│   ├── CelestialBody.cs        — Component cho mỗi thiên thể (mass, velocity, position)
│   ├── GravitySimulation.cs    — N-body manager, Velocity Verlet loop
│   └── SolarSystemBuilder.cs   — Tự động tạo hệ mặt trời từ PlanetData
├── Data/
│   └── PlanetData.cs           — Dữ liệu thực NASA: 9 thiên thể
├── Camera/
│   └── SimulationCamera.cs     — Camera zoom/rotate/focus
└── UI/
    └── SimulationUI.cs         — UI hiển thị info + điều khiển
```

### Vai trò từng script:

| Script | Vai trò | Gắn lên |
|---|---|---|
| `DoubleVector3` | Struct toán học, không gắn | — (dùng trong code) |
| `SimulationSettings` | Cấu hình toàn bộ sim | ScriptableObject asset |
| `CelestialBody` | Lưu mass, vận tốc, vị trí | Mỗi hành tinh (tự động bởi Builder) |
| `GravitySimulation` | Tính gravity + integration | GameObject "SolarSystem" |
| `SolarSystemBuilder` | Tạo 9 hành tinh lúc Awake | GameObject "SolarSystem" |
| `PlanetData` | Static data, không gắn | — (dùng trong code) |
| `SimulationCamera` | Điều khiển camera | Main Camera |
| `SimulationUI` | UI overlay | Bất kỳ GameObject |

---

## 6. Hướng Dẫn Setup Từng Bước

### Bước 1: Tạo SimulationSettings Asset

1. Trong Project window: **Right-click → Create → Solar System → Simulation Settings**
2. Đặt tên: `DefaultSimSettings`
3. Trong Inspector, chỉnh:
   - **Mode:** `GameFriendly` (mặc định)
   - **Time Scale:** `10` (10 ngày/giây, Earth quay 1 vòng trong ~36.5 giây)
   - **Sub Steps:** `4`
   - **Show Orbits:** ✅

### Bước 2: Tạo SolarSystem GameObject

1. Hierarchy → **Create Empty** → đặt tên `SolarSystem`
2. Position: `(0, 0, 0)`
3. Gắn component: **SolarSystemBuilder**
   - Kéo `DefaultSimSettings` vào field `Settings`
4. Gắn component: **GravitySimulation**
   - Kéo `DefaultSimSettings` vào field `Settings`

### Bước 3: Setup Camera

1. Chọn **Main Camera** trong Hierarchy
2. Gắn component: **SimulationCamera**
3. Settings mặc định OK (zoom = 20, rotation = 60°)

### Bước 4: Setup UI

1. Tạo **Empty GameObject** → đặt tên `UIManager`
2. Gắn component: **SimulationUI**
3. Kéo references:
   - `Simulation` → kéo object `SolarSystem` (component GravitySimulation)
   - `Settings` → kéo `DefaultSimSettings`
   - `Sim Camera` → kéo Main Camera (component SimulationCamera)

### Bước 5: Nhấn Play! 🎮

**Kết quả mong đợi:**

- 9 quả cầu xuất hiện (Sun + 8 hành tinh)
- Sun vàng to ở giữa, phát sáng
- Các hành tinh xoay quanh Sun, vẽ trail quỹ đạo
- Mercury xoay nhanh nhất, Neptune chậm nhất

---

## 7. Dữ Liệu Hành Tinh

### Bảng dữ liệu đầy đủ (đã quy đổi sang hệ đơn vị sim):

| # | Hành tinh | Mass (M☉) | Khoảng cách (AU) | Vận tốc (AU/day) | Vận tốc (km/s) | Chu kỳ (năm) | Visual Scale |
|---|---|---|---|---|---|---|---|
| 1 | ☀ Sun | 1.0 | 0 | 0 | 0 | — | 0.80 |
| 2 | ☿ Mercury | 1.659×10⁻⁷ | 0.387 | 0.02765 | 47.87 | 0.24 | 0.15 |
| 3 | ♀ Venus | 2.448×10⁻⁶ | 0.723 | 0.02023 | 35.02 | 0.62 | 0.25 |
| 4 | 🌍 Earth | 3.003×10⁻⁶ | 1.000 | 0.01720 | 29.78 | 1.00 | 0.25 |
| 5 | ♂ Mars | 3.227×10⁻⁷ | 1.524 | 0.01393 | 24.13 | 1.88 | 0.20 |
| 6 | ♃ Jupiter | 9.543×10⁻⁴ | 5.203 | 0.007541 | 13.06 | 11.86 | 0.50 |
| 7 | ♄ Saturn | 2.857×10⁻⁴ | 9.537 | 0.005572 | 9.65 | 29.46 | 0.45 |
| 8 | ♅ Uranus | 4.366×10⁻⁵ | 19.19 | 0.003927 | 6.80 | 84.01 | 0.35 |
| 9 | ♆ Neptune | 5.151×10⁻⁵ | 30.07 | 0.003137 | 5.43 | 164.8 | 0.35 |

### Cách tính vận tốc quỹ đạo:

```
v = √(G_sim × M_sun / r)
  = √(2.9592e-4 × 1.0 / r)
  = √(2.9592e-4 / r)  AU/day

Ví dụ Earth (r = 1.0 AU):
  v = √(2.9592e-4 / 1.0) = 0.01720 AU/day

Chuyển sang km/s:
  1 AU/day = 1.496e8 km / 86400 s = 1731.5 km/s
  → 0.01720 × 1731.5 = 29.78 km/s ✓ (khớp NASA data)
```

---

## 8. Hướng Dẫn Test Từng Thiên Thể

### Test 1: Chỉ Sun + Earth (test cơ bản)

**Mục tiêu:** Xác nhận Earth xoay quanh Sun thành quỹ đạo tròn.

**Cách làm:** Tạm sửa `PlanetData.AllBodies` chỉ giữ `Sun` và `Earth`:

```csharp
// Trong PlanetData.cs, tạm thay AllBodies:
public static readonly BodyInfo[] AllBodies = new BodyInfo[]
{
    Sun, Earth
};
```

**Checklist kiểm tra:**

- [ ] Earth xoay quanh Sun thành hình tròn (không phải xoắn ốc)
- [ ] Trail vẽ đường tròn khép kín sau ~36.5 giây (= 1 năm với timeScale=10)
- [ ] Earth không bay ra xa dần (Euler bug) hoặc rơi vào Sun
- [ ] Khoảng cách Earth-Sun luôn ~1.0 AU (xem trong UI info)
- [ ] Vận tốc Earth luôn ~29.78 km/s (xem trong UI info)

**Nếu lỗi:**

- Quỹ đạo xoắn ra → tăng `subSteps` lên 8-16
- Earth bay mất → kiểm tra `initialVelocityV3` có vuông góc với position không
- Earth rơi vào Sun → vận tốc quá thấp, kiểm tra giá trị

---

### Test 2: Sun + Earth + Moon (test multi-body nhỏ)

**Mục tiêu:** Thêm Mặt Trăng, xem nó có xoay quanh Earth không.

**Cách làm:** Thêm Moon data vào `PlanetData.cs`:

```csharp
// Thêm sau Neptune:
public static readonly BodyInfo Moon = new BodyInfo(
    "Moon",
    mass: 3.694e-8,          // 7.342e22 kg
    dist: 1.00257,           // 1 AU + 0.00257 AU (384,400 km from Earth)
    vel: 0.01720 + 0.000588, // Earth velocity + Moon orbital velocity relative to Earth
    radius: 1.161e-5,        // 1,737 km
    color: new Color(0.8f, 0.8f, 0.8f),
    visualScale: 0.1f
);

// Cập nhật AllBodies:
public static readonly BodyInfo[] AllBodies = new BodyInfo[]
{
    Sun, Earth, Moon
};
```

> ⚠️ **Lưu ý:** Moon orbit quanh Earth rất nhỏ (0.00257 AU) — trong GameFriendly mode gần như dính với Earth. Cần zoom rất gần để thấy.

**Checklist kiểm tra:**

- [ ] Moon xoay quanh Earth (không xoay quanh Sun trực tiếp)
- [ ] Earth + Moon cùng xoay quanh Sun
- [ ] Hệ 3 vật thể ổn định

---

### Test 3: Inner Solar System (Mercury → Mars)

**Mục tiêu:** Test 5 hành tinh gần, xem tương tác N-body.

```csharp
public static readonly BodyInfo[] AllBodies = new BodyInfo[]
{
    Sun, Mercury, Venus, Earth, Mars
};
```

**Checklist kiểm tra:**

- [ ] Mỗi hành tinh có quỹ đạo riêng, không chồng lên nhau
- [ ] Mercury xoay nhanh nhất (~88 ngày = ~8.8 giây với timeScale=10)
- [ ] Mars xoay chậm nhất trong nhóm
- [ ] Quỹ đạo gần tròn (hơi ellipse là OK — đó là ảnh hưởng N-body)
- [ ] Không có hành tinh nào bay khỏi hệ

---

### Test 4: Full Solar System (9 thiên thể)

**Mục tiêu:** Test toàn bộ hệ.

```csharp
public static readonly BodyInfo[] AllBodies = new BodyInfo[]
{
    Sun, Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune
};
```

**Checklist kiểm tra:**

- [ ] Tất cả 9 thiên thể xuất hiện
- [ ] Sun ở trung tâm, không di chuyển nhiều (hơi rung nhẹ là đúng — do phản lực từ Jupiter)
- [ ] Hành tinh trong xoay nhanh, hành tinh ngoài xoay chậm
- [ ] Trail không bị gãy hoặc nhảy
- [ ] FPS ổn định (9 body = 36 pairs, rất nhẹ)

---

### Test 5: Energy Conservation (kiểm tra thuật toán)

**Mục tiêu:** Xác nhận Velocity Verlet bảo toàn năng lượng.

1. Mở Inspector → chọn object `SolarSystem`
2. Xem field `Total Energy` trong GravitySimulation component
3. Chạy sim 5-10 phút
4. **Total Energy phải gần như không đổi** (drift < 0.1%)

**Nếu drift lớn:**

- Tăng `subSteps`: 4 → 8 → 16
- Giảm `timeScale`: 10 → 5 → 1

---

### Test 6: Stress Test — Thay đổi vận tốc ban đầu

**Mục tiêu:** Hiểu ảnh hưởng của vận tốc lên quỹ đạo.

Tạm sửa Earth velocity trong `PlanetData.cs`:

| Thử nghiệm | Velocity (AU/day) | Kết quả mong đợi |
|---|---|---|
| `v = 0.01720` | Đúng chuẩn | Quỹ đạo tròn |
| `v = 0.01200` | Quá chậm | Quỹ đạo ellipse hẹp, Earth đến gần Sun |
| `v = 0.02400` | Quá nhanh | Quỹ đạo ellipse rộng, Earth ra xa |
| `v = 0.02432` | Vận tốc thoát | Earth bay ra vô cực (v_escape = v_circular × √2) |
| `v = 0` | Không có vận tốc | Earth rơi thẳng vào Sun |

---

## 9. Tinh Chỉnh Thông Số

### 9.1 SimulationSettings — Các giá trị quan trọng

#### Time Scale

```
timeScale = 1   → 1 ngày/giây      (chậm, xem chi tiết inner planets)
timeScale = 10  → 10 ngày/giây     (Earth quay 1 vòng / 36.5s) ← MẶC ĐỊNH
timeScale = 50  → 50 ngày/giây     (xem outer planets di chuyển)
timeScale = 365 → 1 năm/giây       (xem Neptune quay)
```

#### Sub Steps

```
subSteps = 1  → Nhanh nhưng ít chính xác (OK cho demo nhanh)
subSteps = 4  → Cân bằng tốt ← MẶC ĐỊNH
subSteps = 8  → Chính xác cao (cho timeScale lớn)
subSteps = 16 → Rất chính xác (nếu energy drift > 1%)
```

#### Distance Compression (GameFriendly mode)

```
compressionPower = 1.0  → Không nén (= Realistic)
compressionPower = 0.5  → Nén trung bình (căn bậc 2)
compressionPower = 0.45 → Nén hơi mạnh ← MẶC ĐỊNH
compressionPower = 0.3  → Nén rất mạnh (Neptune gần hơn nhiều)
compressionPower = 0.2  → Nén cực mạnh (gần như đều nhau)
```

```
baseDistance      = 1.5  → Khoảng cách tối thiểu Sun-Mercury (visual)
distanceMultiplier = 2.0 → Hệ số nhân sau khi nén
```

**Cách chỉnh:** Mở `DefaultSimSettings` asset trong Inspector → kéo slider → nhấn Play để xem kết quả.

### 9.2 Bảng so sánh compression settings

| Setting | Mercury | Earth | Neptune | Tỉ lệ Nep/Mer |
|---|---|---|---|---|
| Realistic (power=1.0) | 0.387 | 1.0 | 30.07 | **78×** |
| power=0.5 | 2.74 | 3.50 | 12.47 | **4.5×** |
| **power=0.45 (default)** | **2.78** | **3.50** | **11.13** | **4.0×** |
| power=0.3 | 2.96 | 3.50 | 8.92 | **3.0×** |

### 9.3 Visual Scale — Kích thước hành tinh

Trong `PlanetData.cs`, field `visualScale` quyết định kích thước Unity Sphere:

```
Hiện tại (game-friendly):
  Sun     = 0.80  (to nhất)
  Jupiter = 0.50
  Saturn  = 0.45
  Uranus  = 0.35
  Neptune = 0.35
  Venus   = 0.25
  Earth   = 0.25
  Mars    = 0.20
  Mercury = 0.15  (nhỏ nhất)
```

**Nhân thêm** `visualScaleMultiplier` trong SimulationSettings:

- `0.5` = tất cả nhỏ đi 50%
- `1.0` = mặc định
- `2.0` = tất cả to gấp đôi

---

## 10. Xử Lý Lỗi Thường Gặp

### ❌ Hành tinh bay mất / quỹ đạo xoắn ốc

**Nguyên nhân:** TimeScale quá lớn, sub-steps quá ít → dt quá lớn → Verlet mất chính xác

**Fix:** Giảm `timeScale` hoặc tăng `subSteps`

---

### ❌ Hành tinh không di chuyển

**Nguyên nhân:** `timeScale = 0` (pause) hoặc chưa gán SimulationSettings

**Fix:** Kiểm tra settings reference và timeScale > 0

---

### ❌ Tất cả hành tinh dính vào 1 điểm

**Nguyên nhân:** Mode = GameFriendly nhưng `baseDistance = 0` và `distanceMultiplier = 0`

**Fix:** Reset default: base=1.5, multiplier=2.0, power=0.45

---

### ❌ Quỹ đạo hình xoắn (spiral) thay vì ellipse

**Nguyên nhân thường gặp:**

1. Dùng Euler thay vì Verlet → đã fix (code hiện tại dùng Verlet)
2. `subSteps` quá thấp với `timeScale` quá cao

**Fix:** Đảm bảo `timeScale / subSteps < 5` (mỗi sub-step < 5 ngày)

---

### ❌ Sun di chuyển nhiều

**Đúng hành vi!** Sun rung nhẹ vì bị Jupiter kéo (Jupiter mass = 0.001 M☉).
Nếu muốn Sun cố định, set `Sun.mass = 0` (nhưng sẽ mất tương tác 2 chiều).

---

### ❌ Trail bị gãy / nhảy

**Nguyên nhân:** `trailRenderer.minVertexDistance` quá lớn

**Fix:** Giảm `minVertexDistance` trong `CelestialBody.SetupTrail()`

---

### ❌ Không thấy hành tinh (quá nhỏ)

**Fix:** Tăng `visualScaleMultiplier` trong SimulationSettings, hoặc tăng `visualScale` trong PlanetData

---

## 11. Mở Rộng

### 11.1 Thêm Mặt Trăng (Moon)

Thêm vào `PlanetData.cs`:

```csharp
public static readonly BodyInfo Moon = new BodyInfo(
    "Moon",
    mass: 3.694e-8,
    dist: 1.00257,             // Earth dist + Moon orbital radius
    vel: 0.01720 + 0.000588,   // Earth vel + Moon vel (relative)
    radius: 1.161e-5,
    color: new Color(0.8f, 0.8f, 0.8f),
    visualScale: 0.1f
);
```

### 11.2 Thêm Tiểu Hành Tinh / Sao Chổi

Sao chổi có quỹ đạo ellipse rất dẹt — chỉ cần đặt vận tốc khác v_circular:

```csharp
public static readonly BodyInfo Halley = new BodyInfo(
    "Halley's Comet",
    mass: 1.1e-16,             // Rất nhẹ
    dist: 0.586,               // Perihelion (gần Sun nhất)
    vel: 0.03126,              // Nhanh hơn v_circular tại perihelion
    radius: 1e-7,
    color: Color.cyan,
    visualScale: 0.05f
);
```

### 11.3 Thêm Vành Đai Saturn

Dùng nhiều particle nhỏ orbit quanh Saturn — mỗi particle là 1 CelestialBody với mass gần bằng 0.

### 11.4 Collision Detection

Thêm vào `GravitySimulation.ComputeAllAccelerations()`:

```csharp
double combinedRadius = allBodies[i].bodyRadius + allBodies[j].bodyRadius;
if (dist < combinedRadius)
{
    // Collision detected! Merge, bounce, or destroy
}
```

### 11.5 Camera Improvements

- **Floating origin:** Khi camera follow hành tinh xa (Neptune), re-center world around camera → tránh float precision loss
- **Log-scale zoom:** Zoom nhanh ở xa, chậm ở gần
- **Planet labels:** TextMeshPro billboards hiển thị tên hành tinh

---

## 📋 Checklist Setup Nhanh

```
□ 1. Create → Solar System → Simulation Settings → đặt tên "DefaultSimSettings"
□ 2. Create Empty "SolarSystem" tại (0,0,0)
□ 3. Gắn SolarSystemBuilder → kéo settings vào
□ 4. Gắn GravitySimulation → kéo settings vào
□ 5. Main Camera → gắn SimulationCamera
□ 6. Create Empty "UIManager" → gắn SimulationUI → kéo references
□ 7. Nhấn Play → enjoy! 🎮
```

---

## 🎮 Controls

| Phím | Hành động |
|---|---|
| **Scroll** | Zoom in/out |
| **Right-click + Drag** | Xoay camera |
| **Left-click** vào hành tinh | Focus vào hành tinh đó |
| **1 → 9** | Chọn nhanh (1=Sun, 2=Mercury, ..., 9=Neptune) |
| **Space** | Reset camera về nhìn toàn cảnh |
| **R** | Reset simulation (quay về trạng thái đầu) |
| **H** | Bật/tắt help overlay |

---

*Tài liệu tổng hợp toàn bộ kiến thức vật lý, cách triển khai, và hướng dẫn test cho Solar System Simulation.*
*Chúc đại ca code vui! 🚀*

