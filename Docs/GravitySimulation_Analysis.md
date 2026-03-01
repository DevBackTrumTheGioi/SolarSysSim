# Phân tích chi tiết Script: GravitySimulation.cs

Dạ thưa Đại ca, em đã đọc và phân tích thành phần, chi tiết các hàm trong script `GravitySimulation.cs`. Dưới đây là báo cáo đầy đủ về toàn bộ các hàm bên trong file này, bao gồm giải thích logic code, vị trí được gọi tới và mục đích cơ bản của từng hàm chức năng một cách chi tiết nhất để Đại ca dễ nắm bắt ạ.

## Tổng quan
Script `GravitySimulation.cs` là "trái tim" của dự án SolarSysSim, đóng vai trò làm Trình Quản Lý Mô Phỏng Lực Hấp Dẫn N-Body. Script này áp dụng thuật toán tích phân **Velocity Verlet** (Störmer-Verlet) tiêu chuẩn trong vật lý thiên văn để duy trì sự bảo toàn năng lượng hệ thống, đảm bảo các hành tinh di chuyển theo quỹ đạo mượt mà và thực tế nhất.

---

## Chi tiết các hàm (Methods)

### 1. `Start()`
- **Giải thích code:** Hàm khởi tạo vòng đời chuẩn của Unity. Code tiến hành tìm kiếm đối tượng `SimulationCamera` trong scene và gọi hàm đóng gói `InitializeSimulation()` ngay lập tức để mô phỏng sẵn sàng.
- **Mục đích:** Thiết lập tham chiếu và khởi động bộ máy vật lý ngay khi game vừa bắt đầu.
- **Nơi được sử dụng:** Được Unity Engine **tự động gọi ngầm** khi Object chứa script này load vào màn chơi.

### 2. `InitializeSimulation()`
- **Giải thích code:** 
  - Quét lại toàn bộ Scene để tìm tất cả các Object có gắn script `CelestialBody` và gom vào mảng `bodies`.
  - Xác định một vật thể làm trung tâm vũ trụ (gán vào `sunBody`). Nó ưu tiên tìm vật nào mang tên "Sun", nếu không có sẽ tự lấy vật thể nặng nhất làm tâm.
  - Khởi tạo bộ nhớ mảng `newAccelerations` cho các gia tốc tính toán nhằm tránh tình trạng xả rác bộ nhớ (Garbage Collection lag spike).
  - Loop qua mọi body để Setup thông tin hiển thị quỹ đạo (Trail) và tính số điểm kết xuất qua `CalcOrbitPoints()`.
  - Bước nháp cuối cùng là tính toán lực kéo tĩnh đầu tiên bằng `ComputeAllAccelerations()`.
- **Mục đích:** Điểm danh lại tất cả các thực thể cần mô phỏng lực hấp dẫn trên map hiện tại để Engine chuẩn bị update tọa độ cho chúng.
- **Nơi được sử dụng:** Từ hàm `Start()` (ở local của script này) và thường được gọi lại chéo từ UI hệ thống (`SimulationUI.cs`) mỗi khi Đại ca thay đổi mode hoặc load map vũ trụ mới.

### 3. `Update()`
- **Giải thích code:**
  - Nhận tổng biến số thời gian mô phỏng `totalDt` bằng `Time.deltaTime * timeScale`.
  - **Sub-stepping (Chia nhỏ bước tính):** Để thuật toán đủ chính xác với tốc độ cao mà không làm lệch đường bay, code sẽ tự chia `totalDt` ra làm nhiều cụm nhỏ liên tiếp (`dynamicSubSteps`) sao cho khoảng cách giữa mỗi bước nhỏ không bao giờ vượt qua `MAX_DT_PER_STEP=0.05`.
  - Chạy hàm Loop qua các bước nhỏ để gọi `VelocityVerletStep` và xử lý va chạm `HandleCollisions`.
  - Xử lý **Sun Drift**: Dịch chuyển đồng đều toàn bộ hệ mặt trời theo trục Y để mô phỏng "Hệ mặt trời đang trôi trong dải Ngân Hà".
  - Chạy Graphics Update: Chia làm hai Lượt (Lượt 1 cho các hành tinh quanh Sun, Lượt 2 cho vệ tinh như Mặt Trăng) – đây là thủ thuật fix lỗi lag chậm 1-frame về mặt hình ảnh trên màn hình.
  - Cập nhật số liệu Enegy/Time.
- **Mục đích:** Cập nhật liên tục trạng thái vị trí trên màn hình của tất cả các khối cầu dựa trên logic vật lý mỗi một khung hình vẽ.
- **Nơi được sử dụng:** Unity Engine **tự động gọi** mỗi frame.

### 4. `CalcOrbitPoints(CelestialBody body)`
- **Giải thích code:** Thuật toán dựa trên định luật 3 Kepler ($T = 365.25 \times a^{1.5}$) để dự đoán trước chu kỳ quay (T) của Hành tinh vòng quanh Mặt Trời mất số lượng Frame tương ứng là bao nhiêu đối với `timeScale` hiện tại.
- **Mục đích:** Trả về một số nguyên `points` (giới hạn từ 60 đến 2000). Số điểm này chính là độ dài của cái "đuôi" ánh sáng vẽ quỹ đạo, giúp quỹ đạo luôn vẽ thành 1 vòng tròn khép kín đúng 100% không dư không thiếu nét.
- **Nơi được sử dụng:** Trong lòng hàm `InitializeSimulation()` và khi Đẩy nhanh thêm 1 hành tinh mới ở hàm `AddDynamicBody()`.

### 5. `VelocityVerletStep(double dt)`
- **Giải thích code:** Thực hiện thuật toán tích phân Symplectic Verlet bao gồm chuẩn 3 bước vật lý:
  - **B1:** Ước đoán và Di dời vị trí `(Position)` mới bằng Vận tốc + 1/2 Gia tốc hiện tại.
  - **B2:** Dựa theo tọa độ hoàn toàn mới, đo đạc lại Lực kéo vạn vật để ra Gia Tốc Mới (`ComputeAllAccelerations`).
  - **B3:** Chỉnh lý và bồi đắp Vận tốc `(Velocity)` bằng Trung Bình Cộng của (Gia tốc cũ + Gia tốc vừa đẻ ra).
- **Mục đích:** Tính toán chính xác đường đi theo phương pháp Bảo Toàn Năng Lượng Vũ Trụ, không khiến các hành tinh văng từ từ ra ngoài không gian hoặc lao đầu cắm vào Mặt Trời.
- **Nơi được sử dụng:** Gọi liên tục bởi vòng lặp mô phỏng nằm trong hàm `Update()`.

### 6. `HandleCollisions(double dt)`
- **Giải thích code:** 
  - Detect và giải quyết Va Chạm Sáp Nhập (Inelastic Collision).
  - Code sử dụng 2 cách quét: Kiểm tra bán kính đè lên nhau, TRỘN LẪN VỚI Continuous Collision Detection (CCD; bằng cách giải phương trình bậc 2). CCD rất quan trọng để phát hiện được "đạn/thiên thạch" bay quá nhanh, tránh hiện tượng đạn chui/xuyên ngang lọt thỏm qua hành tinh mà Engine không kịp thấy do tốc độ cao.
  - Khối lượng của đối tượng sống sót sẽ bao gồm trọn vẹn của con chết, thể tích (Bán kính) của đối tượng sống sót sẽ tăng phình lên theo tỷ lệ Khối Lập Phương (Căn bậc 3 của hệ số khối lượng mới). 
  - Phá hủy GameObject hành tinh bị ăn.
- **Mục đích:** Cho phép các hành tinh va vào nhau và nuốt chửng lẫn nhau dựa theo Luật bảo toàn Động Lượng không gian.
- **Nơi được sử dụng:** Trong `Update()`, chỉ kích hoạt khi scale map được set nhỏ hơn thông số phù hợp.

### 7. `RebuildArraysAfterCollision()`
- **Giải thích code:** Khi một (hoặc nhiều) khối cầu bị nuốt chửng và biến mất khỏi hệ thống, hàm này sẽ quét mảng `bodies` và `newAccelerations`, bỏ qua các `null` để nén các list mảng này ngắn lại với chi phí vòng lặp chạy tối ưu là `O(N)`.
- **Mục đích:** Dọn dẹp lại đội hình logic mạng máy tính. Tránh lỗi NULL Reference và triệt tiêu luôn thời gian lặp thừa thãi để hệ thống tính vật lý được mượt.
- **Nơi được sử dụng:** Được gọi vào cuối giai đoạn `HandleCollisions()` và trong lệnh hàm `RemoveBody()`.

### 8. `RemoveBody(CelestialBody bodyToRemove)`
- **Giải thích code:** Xóa bỏ an toàn một `CelestialBody` theo chỉ định, set Khối lượng về 0, Destroy nó khỏi màn chơi và dọn mảng bằng `RebuildArraysAfterCollision()`. 
- **Mục đích:** Công cụ an toàn để triệt tiêu lập tức 1 hành tinh.
- **Nơi được sử dụng:** Dùng như một public API, thường được bấm gọi từ giao diện người chơi UI khi Đại ca select một hành tinh và nhấn nút Xóa.

### 9. `ComputeAllAccelerations(CelestialBody[] allBodies)`
- **Giải thích code:** Module cốt lõi đếm Newton! Thay vì lấy mọi thằng để kết đôi nhau chìm vòng lặp N^2, nó tuân thủ Định Luật 3 Newton (F1 = -F2) để tính lực chung của cặp sinh vật A->B  thành vector, và ép vector ngược cho B->A. Vận hành công thức $\frac{G \cdot M \cdot v_{dir}}{R^3}$. Code thậm chí nhét thêm Softening Epsilon để R mẫu số không chia cho 0.
- **Mục đích:** Sản xuất ra "Gia Tốc Mới Nhất" trong mili giây của toàn thể Hệ Thống.
- **Nơi được sử dụng:** Gọi tại `InitializeSimulation()`, `VelocityVerletStep()` và `ResetSimulation()`.

### 10. `ComputeTotalEnergy()`
- **Giải thích code:** Quét lại Động Năng (Kinetic: $0.5 \cdot m \cdot v^2$) và Thế Năng (Potential: $\frac{-G \cdot M \cdot m}{r}$) của mọi vật thể để show ra 1 thông số đại diện tổng lực nén.
- **Mục đích:** Là công cụ Diagnostic Debugging. Nếu số Enegy này bị nhảy loạn nhịp theo giờ, có nghĩa là hệ thống đang "Rỉ năng lượng" và sắp sập.
- **Nơi được sử dụng:** Show số liệu thầm lặng trong hàm `Update()`.

### 11. `ResetSimulation()`
- **Giải thích code:** Xóa số ngày bay trôi qua, kích hoạt lại Initilze cho mọi tọa độ vật thể gốc rồi bắt tính toán chớp nhoáng lại Gia Tốc nháp số 0 như ban đầu. Đặt lại hiển thị render của Mặt Trời vào trọng tâm tâm nhãn Cảnh.
- **Mục đích:** Vứt bỏ hiện tại, tua lại từ đầu thế giới vật lý của hệ thống.
- **Nơi được sử dụng:** Nút bấm "Restart/Reset" tại Script Giao Diện UI (`SimulationUI.cs`).

### 12. `AddDynamicBody(CelestialBody newBody)`
- **Giải thích code:** Giãn bộ nhớ tĩnh (Array) cũ ra thêm 1 Slot trống nữa một cách khéo léo và nhét hành tinh mới vào mà KHÔNG CẦN phải Stop hay Reset lại thế giới. Lập Trail mượt mà cho nhân viên mới đó.
- **Mục đích:** "Bơm nóng" đối tượng vào bầu trời thời gian thực.
- **Nơi được sử dụng:** Khi Đại ca xài tính năng Spawn Bão Thiên Thạch (ở script `SolarSystemBuilder.cs`) hoặc nhấn đẻ ra Planet bấm ngoài Live UI.

---
Bản báo cáo này đã tổng hợp toàn bộ tri thức về các hàm chức năng thuộc script `GravitySimulation.cs`. Dạ mong Đại ca xem qua ạ, có bất cứ sửa đổi gì Đại ca cứ căn dặn để em tiếp tục nâng cấp nha! 🚀
