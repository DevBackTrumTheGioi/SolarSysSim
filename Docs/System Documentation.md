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

## 6. Triển Khai Mô Phỏng Va Chạm Hành Tinh (Planetary Collisions)
*Cơ chế tàn khốc của Vũ trụ đã được tích hợp thành công vào Base Code N-Body.*

### Góc nhìn Vật lý thực tế
- **Không có chuyện nảy bật (Elastic Collision):** Các thiên thể khổng lồ không giống như quả bida. Khi va chạm, lực lượng hấp dẫn và động năng quá lớn sẽ phá vỡ cấu trúc và làm bề mặt nung chảy (đá hóa lỏng).
- **Va chạm không đàn hồi (Inelastic Collision) hoặc Sáp nhập (Merger):** Thiên thể lớn hơn (Survivor) sẽ nuốt chửng thiên thể nhỏ hơn (Victim). 

### Giải pháp Toán học & Lập trình đã triển khai (Trong `GravitySimulation.cs`)

**1. Bảo toàn Động lượng (Momentum) và Khối lượng (Mass):**
Khi hai hành tinh lồng vào nhau, Lõi của thuật toán tính toán lại thông số cho kẻ Sống sót:
- Tổng khối lượng mới: $M_{new} = M_1 + M_2$
- Vận tốc mới duy trì Động lượng hệ kín: $\vec{v}_{new} = \frac{M_1 \vec{v}_1 + M_2 \vec{v}_2}{M_1 + M_2}$
- Bán kính mới (Radius): Thể tích hình cầu tỷ lệ thuận với Mass, nên Bán kính mới bằng Bán kính cũ nhân với $\sqrt[3]{\frac{M_{new}}{M_{old}}}$. Kích thước đồ họa (`baseVisualScale`) cũng phình to tương ứng.

**2. Khắc phục Lỗi Xuyên hầm (Continuous Collision Detection - CCD):**
Ở chế độ `timeScale` cực nhanh (Time Warp), bước nhảy tích phân `dt` rất lớn. Một hành tinh bay ở tốc độ cao (VD: 1 AU/day) có thể nhảy chéo "xuyên qua" ruột Trái Đất chỉ trong 1 Frame mã nguồn, khiến lệnh `if(khoảng_cách < tổng_bán_kính)` không kịp bắt được khoảnh khắc va chạm.
- **Cách giải quyết:** Áp dụng phương trình hình học Không gian. Tính Toán Vector Vận tốc tương đối (`relVel`) và Vị trí tương đối (`relPos`). Giải phương trình bậc 2 ($at^2 + bt + c = 0$) để tìm điểm giao cắt trên đường thẳng quỹ đạo di chuyển DỰ ĐOÁN của 2 thực thể TRONG NỘI QUÁ TRÌNH TIMESTEP `dt`. Chỉ cần nghiệm $t$ rơi vào khoảng `[0, dt]`, Hệ thống tính là Va chạm thành công dù cuối khung hình khoảng cách hai thực thể đã văng xa nhau! 

**3. Tối ưu Hiệu Năng Vòng lặp N-Body:**
Không mạo hiểm dùng danh sách Động (List) bên trong vòng lặp Vật lý `Update()` cấp thấp để tránh rác (Garbage Collector Spike). Khi có va chạm, `Victim` chỉ bị vô hiệu hóa biến Mass = 0, ẩn Game Object. Hệ thống sẽ cấp phát (Rebuild) mảng Array `bodies` mới và chép dữ liệu các hành tinh còn sống sang chỉ 1 LẦN DUY NHẤT ngay sau khi vụ nổ xảy ra.

**4. Kịch bản Đồ hoạ (VFX): Sức nóng Tới tột đỉnh**
Ngay khoảnh khắc nuốt chửng Victim, `Survivor.TriggerCollisionVFX(victimMass)` sẽ được gọi. 
1. Sử dụng Coroutine nhúng trong `CelestialBody.cs`.
2. Mở khóa từ khóa Shader vật liệu `_EMISSION`.
3. Bơm ánh sáng cực gắt màu Đỏ Cam với cường độ (Intensity) tính toán dựa vào trọng lượng nạn nhân (Nạn nhân càng lớn, lõi năng lượng tỏa ra càng sáng).
4. Phơi sáng bùng nổ trong 0.2s đầu tiên, sau đó nguội dần (Cooldown) làm mờ phát quang trở về màu vật lý đá gốc trong 3 đến 5 giây tiếp theo.
5. `Victim` bị nuốt chửng biến mất vĩnh viễn khỏi cảnh. Khán giả được cảm nhận sức giật nảy Quỹ đạo cực mượt nhờ hiệu ứng Line Renderer của Trail tự động bẻ góc gắt đi theo Vận tốc mới (`newVelocity`) của Kẻ Sống Sót.
