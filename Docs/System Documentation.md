# Bách Khoa Toàn Thư: Solar System N-Body Simulation
*Tài liệu tổng hợp Kiến trúc, Thủ thuật và Xử lý Lỗi trong quá trình xây dựng Hệ Mặt Trời 3D (Phiên bản Unity C#).*

---

## 1. Kiến Trúc Cốt Lõi: Tách Biệt Thế Giới Tự Nhiên và Màn Hình Hiện Thị
Vấn đề kinh điển của lập trình Không gian Vũ trụ là **Tỉ lệ (Scale)**. 
Khoảng cách thực tế giữa các hành tinh lớn gấp hàng triệu lần bản thân kích thước của chúng. Nếu render đúng tỷ lệ 1:1, bạn sẽ chỉ thấy MỘT MÀN HÌNH ĐEN THUI vì các hành tinh quá bé lọt thỏm trong không gian.

**Giải pháp:** Tách biệt thành 2 tầng dữ liệu (Dual-Layer Architecture).
- **Physics Layer (Toán học):** Sử dụng `DoubleVector3` (Độ chính xác kép - Double Precision) cực kỳ cao để tính toán vị trí `position` theo định luật vạn vật hấp dẫn (Lực tỉ lệ nghịch với bình phương khoảng cách). Hoàn toàn dùng đơn vị chuẩn thiên văn `AU` và `AU/day`.
- **Visual Layer (Đồ hoạ):** Hàm `PhysicsToVisualPosition()` dùng thuật toán *Power Compression* (Nén bằng Luỹ thừa, ví dụ `X^0.45`). Nó bóp ngắn các khoảng cách khổng lồ lại, nhưng vẫn giữ đúng thứ tự lớp phôi quỹ đạo và góc quay đồng phẳng. Phác thảo hình ảnh bằng Vector3 (Float).

**Kết quả:** Quỹ đạo bay hoàn toàn chính xác theo Toán học Kepler, nhưng màn hình máy tính vẫn hiển thị đẹp đẽ và lấp đầy bởi các hành tinh.

---

## 2. Các Thủ thuật Tích hợp Động Lực Học (Physics Integration)

### Tích phân Velocity Verlet
Thay vì dùng phương pháp Euler đơn giản (Cộng dồn Vận tốc vào Vị trí) gây sai số tích luỹ khiến quỹ đạo lệch văng ra ngoài, dự án dùng thuật toán **Velocity Verlet**:
1. Cập nhật vị trí dựa trên vận tốc VÀ gia tốc hiện tại của t.
2. Tính toán gia tốc mới (Lực hấp dẫn) cho bước t+dt tiếp theo.
3. Cập nhật Vận tốc dựa trên trung bình cộng của gia tốc cũ và gia tốc mới.
**Lợi ích:** Cực kỳ ổn định cho thuật toán N-Body nhiều hành tinh, bảo toàn năng lượng hệ thống (Energy Conservation).

### Đồng bộ Refresh Rate (Loại bỏ Jitter)
Lỗi giật lag rách hình (Jitter/Stutter) khi Camera bám theo hành tinh quay nhanh có nguyên nhân gốc rễ là việc nội suy vật lý nằm trong `FixedUpdate` (50Hz - cố định của Engine) nhưng màn hình xuất ảnh ở frame rate linh hoạt (60-144Hz).
**Cách gỡ:** Rê vòng lặp Tích phân vật lý vào `Update()`, tính step vòng lặp dựa trên `Time.deltaTime` nhân tốc độ mô phỏng. Nó khớp 1-1 với tần số quét rendering của màn hình.

---

## 3. Quản Lý Hệ Sinh Thái Hành Tinh Và Vệ Tinh (Hierarchy)

### Spawning toạ độ và quỹ đạo lý tưởng
Để thiết lập một hành tinh tự do quay tròn thay vì rớt thẳng vào tâm hố đen, hệ thống cấp Vận tốc ban đầu **Vuông góc 90 độ** với vector Vị trí của nó so với Mặt trời. Đồng thời chèn thêm góc Random Orbital phase `randomAngle` để cả Hệ Mặt Trời không xếp thành hàng dọc.

### Hệ thống Hành tinh Mẹ - Vệ tinh Con (Mặt trăng)
Vật lý N-Body sẽ tự động khoá quỹ đạo của Mặt Trăng quanh Trái Đất nếu và chỉ nếu Mặt trăng được sinh ra với **Toạ độ gốc của Trái Đất** và **Vận tốc bay tịnh tiến của Trái Đất**.
- Mô hình: Khai báo String tên của mẹ trong danh sách Scriptable Objects.
- Tại `SolarSystemBuilder`, Vệ tinh sẽ dò tìm Mẹ và vector cộng gộp trước khi add lực hấp dẫn đẩy vuông góc.

### Ẩn Mặt Trăng Khi Thu Phóng Kích Thước Quá Lớn (Visual Clipping Fix)
Khi người chơi bật **Friendly Mode**, hành tinh mẹ (Trái Đất) có kích thước hiển thị Scale là `0.55x`. Do bản chất phình to lấn át thể tích chân không, vỏ của Trái Đất sẽ nuốt chửng Mặt Trăng bên trong.
**Quyết định Thiết kế:** Thay vì làm hỏng tỷ lệ quỹ đạo của vệ tinh. Ở chế độ Friendly Mode, hệ thống sẽ chèn code tại `CelestialBody.cs` tự động phát hiện `if(zoom > 0.05f)` và ra lệnh tắt toàn bộ Mesh Renderer lẫn Line Renderer (Orbit). Chỉ khi lùi về `Realistic Mode (Scale = 0.05)` Mặt trăng bé nhỏ mới hiện hình. Nhưng lực hấp dẫn của nó trong tầng Database thì vẫn luôn tồn tại.

---

## 4. Xử Lí Kỹ Thuật Đồ hoạ Camera và Trail

### Góc nhìn Mặt Trời (Sun Zoom Factor)
Tất cả các hành tinh đều chịu lệ thuộc vào "Tỷ lệ Thu/Phóng Hệ thống" (`visualScaleMultiplier` do UI thay đổi), nhưng đối tượng cốt lõi là **Mặt Trời thì luôn đứng im ở kích thước Base Scale**. Camera cần có logic phân nhóm mục tiêu tại lệnh Focus. Nếu `Target == "Sun"`, Camera bỏ qua hằng số Zoom của hệ thống, giúp góc nhìn cận cảnh quả cầu sao trung tâm này luôn ngợp ở bất cứ Mode UI nào.

### Camera Near Clip Plane Fix (Xuyên Lủng Đối Tượng)
Ngược lại, khi chuyển sang Realistic Mode, các hành tinh nhỏ liti đi (`0.05x`). Zoom Camera lại quá sát sẽ khiến Camera "đâm xuyên tường" do chạm vào khoảng viền mù mặc định của bộ Engine (Near Clip Plane mặc định là 0.3 đơn vị).
**Cách khắc phục:** Cần ép dòng lệnh `cam.nearClipPlane = 0.0001f` tại Start().

### Glowing Trail (Quỹ đạo Phát sáng Đuôi Sao Chổi)
Tuyệt đối không dùng component cỗ lỗ sĩ `TrailRenderer`. Dự án triển khai bằng `LineRenderer`:
- Đùn Gradient Color từ đầu đến đuôi (Chóp đỉnh độ đục Alpha = 1, Đuôi chóp rớt về 0 mờ dần tan vào vũ trụ).
- Dùng AnimationCurve làm WidthCurve để bóp nhọn nhỏ đuôi tạo cảm giác xé gió. Tự động vẽ `(Vector3.Distance)` mỗi Frame.

---

## 5. UI Tương tác Thời gian thực (Live Editing)
Editor In-Game được thiết kế tách rời hoàn toàn Parameter Editor vs Entity Lifecycle. Không cần Load lại Cảnh.
- **Trọng lực:** Việc nhân ngầm biến số Slider `Gravity Multiplier` trực tiếp lồng vào bộ máy tính `Force` giúp tạo cú sốc trọng lực tức thời.
- **Visual Scale multiplier:** Bóc tách hằng số Scale hệ thống để nó Live Update lên `transform.localScale` của tất cả CelestialBody ngay trong Frame hiện hành nhưng KHÔNG ẢNH HƯỞNG BÁN KÍNH QUỸ ĐẠO. Cho phép biến hóa chế độ Ngắm lướt Friendly vs Ngắm thực Realistic không có độ rễ.
- **Cắt bỏ Trail cũ ngáng đường:** UI cung cấp nút Toggle bật/tắt Orbit Trail độc lập. Nếu người chơi thay đổi các thuộc tính vật lý quá đỗi, quĩ đạo cũ sẽ vô nghĩa. Thiết kế một Trigger để dể dàng Reset `orbitLine.positionCount = 0` dọn chỗ khởi đầu nét vẽ mới.

---

## 6. Tính Khả Thi Mô Phỏng Va Chạm Hành Tinh (Planetary Collisions)
*Phân tích R&D (Research & Development) dựa trên Base Code N-Body hiện tại.*

### Góc nhìn Vật lý thực tế
- **Không có chuyện nảy bật (Elastic Collision):** Các thiên thể khổng lồ không giống như quả bida. Khi va chạm, lực lượng hấp dẫn và động năng quá lớn sẽ phá vỡ cấu trúc và làm bề mặt nung chảy (đá hóa lỏng).
- **Va chạm không đàn hồi (Inelastic Collision) hoặc Sáp nhập (Merger):** Thiên thể lớn hơn sẽ hấp thụ thiên thể nhỏ hơn (Ví dụ: Thuyết Vụ va chạm lớn tạo ra Mặt Trăng Theia). Khối lượng được cộng gộp, và động lượng được bảo toàn để sinh ra vận tốc và điểm trọng tâm mới.
- **Vỡ vụn (Fragmentation):** Sinh ra hệ thống tiểu hành tinh N-body khổng lồ (Asteroid belt) - quá phức tạp để mô phỏng Real-time.

### Tính Khả thi trên Codebase Hấp dẫn (GravitySimulation)
Đánh giá: **Rất Khả thi (Toán học)** nhưng **Cần tinh chỉnh (Đồ hoạ)**.

**1. Lợi thế hiện tại:**
- `DoubleVector3` và cơ chế N-Body hoàn hảo để tính toán Định luật bảo toàn Động lượng.
- Các Body lưu trữ sẵn `mass`, `currentVelocity`, và `position` độc lập.
- `PlanetData.cs` cung cấp thông số `radius` vật lý thực tế của mỗi hành tinh (Đơn vị AU).

**2. Công thức Sáp nhập (Merger Logic):**
Ngay trong vòng lặp cập nhật vận tốc `Update()`, nội suy khoảng cách: `if (dist < radiusA + radiusB)`
- Bảo toàn động lượng (Momentum): `v_mới = (m1*v1 + m2*v2) / (m1+m2)`
- Update Thể tích (dựa trên Scale tỷ lệ thuận với Căn bậc ba của Mass).
- `Destroy(Victim)` (Huỷ game object nhỏ hơn) và `Remove` khỏi array duyệt vòng lặp.

**3. Thách thức Kỹ thuật (Cần khắc phục nếu Triển khai):**
- **Lỗi Xuyên hầm (Tunneling Effect):** Ở chế độ `timeScale` cực nhanh (Time Warp), bước nhảy `dt` quá lớn. Các hành tinh bay quãng đường rất xa lố qua quỹ đạo trong 1 Frame. Nó có thể bay "xuyên qua" nhau trước khi vòng lặp kịp kích hoạt cờ kiểm tra khoảng cách. Bắt buộc phải triển khai thuật toán Phát hiện Va chạm Liên tục bằng Toán học (Toán học cắt đường thẳng Line-sphere intersection cho Quãng đường di chuyển trong `dt`) thay vì đo khoảng cách khung hình cuối.
- **Xung đột Đồ hoạ (Friendly Mode):** Do vỏ Sphere Unity bị phóng to hàng ngàn lần so với `radius` thật. Người chơi sẽ thấy 2 hành tinh đâm xuyên lút cán vào nhau trên màn ảnh mà Game vẫn không cho nổ (do chưa chạm lõi). **Giải pháp:** Chỉ kích hoạt Mode Va Chạm Vỡ Nát ở chế độ **Realistic Mode**, vô hiệu hoá ở Friendly Mode.
- **VFX Particle:** Cần lập trình sinh ra cấu trúc lớp phủ Vụ Nổ siêu lớn (hệ Thuyết vụ nổ) để làm mờ đi khoảnh khắc `Destroy(Victim)` và tăng kích thước đột ngột của `Survivor`.
