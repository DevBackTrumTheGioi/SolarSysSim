# 🌌 BÀI THUYẾT TRÌNH DỰ ÁN
# MÔ PHỎNG HỆ MẶT TRỜI 3D (Solar System Simulator)

---

## PHẦN 1: LÝ DO CHỌN ĐỀ TÀI

### 1.1. Bối cảnh & Động lực
- Thiên văn học luôn là một lĩnh vực đầy mê hoặc nhưng khó tiếp cận. Học sinh, sinh viên thường chỉ được nhìn vào hình ảnh 2D tĩnh trên sách giáo khoa, rất khó hình dung được sự vận động thực sự của Hệ Mặt Trời.
- Câu hỏi đặt ra: **"Liệu chúng ta có thể tái tạo cả một vũ trụ thu nhỏ trên chính máy tính, nơi các hành tinh bay theo đúng quỹ đạo vật lý thực, chứ không phải chỉ là hoạt hình giả lập?"**

### 1.2. Mục tiêu dự án
1. **Tính chính xác khoa học:** Quỹ đạo các hành tinh phải tuân theo đúng Định luật Vạn vật Hấp dẫn Newton, không phải hoạt hình cố định theo đường tròn.
2. **Trực quan hóa:** Biến những con số khô khan (khối lượng, khoảng cách, vận tốc) thành hình ảnh 3D sống động mà bất kỳ ai cũng có thể quan sát và tương tác.
3. **Tính tương tác:** Cho phép người dùng can thiệp vào mô phỏng (thay đổi khối lượng, vận tốc, triệu hồi thiên thạch...) để trải nghiệm hệ quả vật lý theo thời gian thực.

### 1.3. Tại sao chọn Unity 3D?
- Unity cung cấp môi trường đồ họa 3D mạnh mẽ, hỗ trợ C#.
- Dễ dàng triển khai trên nhiều nền tảng (Windows, WebGL, Mobile).
- Hệ sinh thái Prefab, Material, Shader phong phú để tạo hiệu ứng vũ trụ đẹp mắt.

---

## PHẦN 2: Ý TƯỞNG & THIẾT KẾ HỆ THỐNG

### 2.1. Kiến trúc tổng quan

```
┌─────────────────────────────────────────────────┐
│                   NGƯỜI DÙNG                     │
│         (Click, Zoom, Thay đổi thông số)         │
└──────────────────────┬──────────────────────────┘
                       ▼
┌──────────────────────────────────────────────────┐
│            TẦNG GIAO DIỆN (UI Layer)              │
│  SimulationUI.cs  |  SimulationCamera.cs          │
│  (Nút bấm, Slider, Bảng thông tin hành tinh)     │
└──────────────────────┬───────────────────────────┘
                       ▼
┌──────────────────────────────────────────────────┐
│         TẦNG DỮ LIỆU (Data Layer)                │
│  PlanetData.cs  |  SimulationSettings.cs          │
│  (Thông số hành tinh, Cấu hình mô phỏng)         │
└──────────────────────┬───────────────────────────┘
                       ▼
┌──────────────────────────────────────────────────┐
│      TẦNG VẬT LÝ LÕI (Physics Core Layer)        │
│  GravitySimulation.cs  |  CelestialBody.cs        │
│  DoubleVector3.cs                                 │
│  (Tính lực hấp dẫn, Tích phân quỹ đạo)           │
└──────────────────────┬───────────────────────────┘
                       ▼
┌──────────────────────────────────────────────────┐
│       TẦNG TRÌNH DIỄN (Rendering Layer)           │
│  SolarSystemBuilder.cs  |  StarfieldBackground.cs │
│  (Sinh hành tinh 3D, Vẽ bầu trời sao, Trails)    │
└──────────────────────────────────────────────────┘
```

### 2.2. Nguyên tắc cốt lõi: Physics ≠ Visual

> **Đây là quyết định thiết kế quan trọng nhất của dự án.**

| Đặc tính | Lõi Vật Lý (Physics) | Đồ Họa (Visual) |
|---|---|---|
| Kiểu dữ liệu | `double` (64-bit) | `float` (32-bit) |
| Đơn vị khoảng cách | AU (thực) | Unity units (nén) |
| Mục đích | Tính lực, quỹ đạo CHÍNH XÁC | Hiển thị ĐẸP MẮT |
| Biến lưu trữ | `DoubleVector3 position` | `transform.position` |

**Tại sao phải tách?** Nếu dùng khoảng cách nén để tính Lực Hấp Dẫn ($F \propto 1/r^2$), sai số sẽ khiến quỹ đạo bị méo mó hoặc hành tinh bay lung tung. Ngược lại, nếu vẽ khoảng cách thật, Trái Đất sẽ bé như hạt bụi trên màn hình.

---

## PHẦN 3: NỀN TẢNG LÝ THUYẾT VẬT LÝ

### 3.1. Định Luật Vạn Vật Hấp Dẫn Newton

Mọi vật có khối lượng đều hút nhau bằng một lực:

$$F = G \cdot \frac{m_1 \cdot m_2}{r^2}$$

- $G$: Hằng số hấp dẫn. Trong hệ AU-M☉-Day: $G \approx 2.9592 \times 10^{-4}$
- $m_1, m_2$: Khối lượng hai vật (đơn vị khối lượng Mặt Trời)
- $r$: Khoảng cách giữa hai vật (đơn vị AU)

**Áp dụng trong code:** Thay vì tính Lực rồi chia cho khối lượng, code tính trực tiếp **Gia tốc**:

$$\vec{a}_i = \sum_{j \neq i} G \cdot m_j \cdot \frac{\vec{r}_{ji}}{(|\vec{r}_{ji}|^2 + \epsilon)^{3/2}}$$

- Mẫu là $r^3$ vì đã gộp vector đơn vị $\hat{r} = \vec{r}/r$ vào.
- $\epsilon$ (Softening Factor): Số rất nhỏ cộng thêm để tránh chia cho 0.

### 3.2. Thuật Toán Tích Phân: Velocity Verlet (Symplectic Euler)

Với Gia tốc $\vec{a}$ đã tính, cần cập nhật Vận tốc ($\vec{v}$) và Vị trí ($\vec{x}$) qua mỗi bước thời gian $\Delta t$:

**Bước 1:** Cập nhật vận tốc
$$\vec{v}_{new} = \vec{v}_{old} + \vec{a} \cdot \Delta t$$

**Bước 2:** Cập nhật vị trí (dùng vận tốc MỚI)
$$\vec{x}_{new} = \vec{x}_{old} + \vec{v}_{new} \cdot \Delta t$$

**Bước 3:** Tính lại gia tốc mới từ vị trí mới cho khung hình tiếp theo.

> **Tại sao không dùng Euler thường?**
> Euler cơ bản dùng $\vec{v}_{old}$ để tính $\vec{x}_{new}$, gây ra hiện tượng **Energy Drift** — quỹ đạo elip sẽ dần xoắn ốc ra ngoài hoặc lao vào tâm. Symplectic Euler bảo toàn cấu trúc hình học của quỹ đạo trong dài hạn.

### 3.3. Adaptive Sub-Stepping (Chia nhỏ bước tự động)

Khi người dùng tăng tốc độ mô phỏng (Time Scale) lên rất cao (ví dụ x400), mỗi khung hình phải nhảy một bước thời gian rất lớn. Nếu bước quá lớn, tích phân sẽ sai nghiêm trọng.

**Giải pháp:** Tự động chia $\Delta t$ thành nhiều bước nhỏ (sub-steps):

$$n_{substeps} = \max\left(n_{default},\ \left\lceil \frac{\Delta t_{total}}{\Delta t_{max}} \right\rceil \right)$$

Với $\Delta t_{max} = 0.05$ ngày. Điều này đảm bảo mỗi bước tích phân luôn đủ nhỏ để giữ quỹ đạo ổn định.

### 3.4. Độ Chính Xác Kép (Double Precision)

| Kiểu | Bit | Chữ số chính xác | Phạm vi |
|---|---|---|---|
| `float` | 32 | ~7 chữ số | Đủ cho đồ họa |
| `double` | 64 | ~15-17 chữ số | Cần cho thiên văn |

Khoảng cách Sao Hải Vương ~ 30 AU, nhưng chênh lệch vị trí nhỏ giữa hai frame có thể chỉ $10^{-8}$ AU. Với `float`, sai số tích lũy theo thời gian sẽ phá hủy quỹ đạo.

→ Hệ thống tự xây dựng cấu trúc `DoubleVector3` thay thế `Vector3` của Unity cho toàn bộ phép toán vật lý.

---

## PHẦN 4: DEMO CÁC CHỨC NĂNG

### 4.1. Quan sát Hệ Mặt Trời
- **Thao tác:** Cuộn chuột để Zoom, giữ chuột phải + kéo để xoay góc nhìn.
- **Điểm nhấn:** 8 hành tinh quay quanh Mặt Trời theo quỹ đạo elip tự nhiên, không phải đường tròn cứng nhắc.
- Đường quỹ đạo (Trail) tự động co lại khi zoom sát, không bị phình to che màn hình.

### 4.2. Chọn & Xem thông tin hành tinh
- **Thao tác:** Click chuột trái vào hành tinh, hoặc nhấn phím số 1-9.
- **Hiển thị:**
  - Bảng điều khiển góc phải: Tên, Khối lượng, Vận tốc, Khoảng cách tới Mặt Trời (real-time).
  - Mô tả khoa học ngắn gọn về đặc điểm của hành tinh.

### 4.3. Tùy chỉnh thông số hành tinh
- **Thao tác:** Nhập giá trị mới vào ô Mass, Velocity, Distance rồi nhấn **"APPLY ALTERS"**.
- **Hệ quả:** Quỹ đạo hành tinh thay đổi NGAY LẬP TỨC theo đúng vật lý. Ví dụ: tăng vận tốc Trái Đất → quỹ đạo giãn ra xa Mặt Trời hơn.

### 4.4. Sự kiện vũ trụ
- **Spawn Rogue Planet:** Triệu hồi một hành tinh lang thang khổng lồ lao vào hệ, gây xáo trộn quỹ đạo toàn bộ.
- **Meteor Swarm:** Bắn mưa thiên thạch ngẫu nhiên hoặc nhắm vào một hành tinh cụ thể. Thiên thạch tự hủy sau 5 giây.
- **Shooting Stars:** Hiệu ứng sao băng trang trí bay xẹt qua bầu trời.

### 4.5. Chế độ hiển thị
- **Friendly Mode:** Hành tinh to, dễ nhìn, phù hợp trình bày.
- **Realistic Mode:** Hành tinh thu nhỏ về tỷ lệ gần thực tế hơn, hiển thị thêm Mặt Trăng.
- **Sun Drift:** Bật mô phỏng toàn bộ Hệ Mặt Trời đang trôi trong Thiên Hà, tạo quỹ đạo xoắn ốc 3D.

### 4.6. Các phím tắt
| Phím | Chức năng |
|---|---|
| Scroll | Zoom In/Out |
| Chuột phải + kéo | Xoay camera |
| Click trái / 1-9 | Chọn hành tinh |
| Space | Reset Camera |
| R | Reset toàn bộ mô phỏng |
| H | Ẩn/Hiện toàn bộ UI |

---

## PHẦN 5: KẾT LUẬN & HƯỚNG PHÁT TRIỂN

### 5.1. Kết quả đạt được
- Mô phỏng N-body (nhiều vật thể) chạy theo đúng lý thuyết Vạn vật Hấp dẫn Newton.
- Quỹ đạo ổn định ở mọi tốc độ nhờ thuật toán Verlet + Adaptive Sub-Stepping.
- Giao diện hoàn chỉnh, trực quan, hỗ trợ tương tác thời gian thực.

### 5.2. Hướng phát triển
- Bổ sung VR (Virtual Reality) để trải nghiệm vũ trụ bằng kính thực tế ảo.
- Mở rộng thêm Vành Đai Tiểu Hành Tinh, Sao Chổi.
- Tích hợp AI giải thích hiện tượng thiên văn khi người dùng quan sát.

---

> *"Chúng tôi không chỉ vẽ hành tinh bay — chúng tôi để Vật Lý tự vẽ."*
