# Phân tích chi tiết Script: SimulationCamera.cs

Dạ thưa Đại ca, dưới đây là phân tích chi tiết các hàm trong script điều khiển Camera (`SimulationCamera.cs`) để Đại ca tiện nắm bắt cách hoạt động của ống kính vũ trụ này ạ:

## Tổng quan
`SimulationCamera.cs` là hệ thống "mắt thần" giúp Đại ca quan sát toàn bộ Thái Dương Hệ. Góc nhìn được điều khiển thông qua chuột và phím điều hướng. Nó hỗ trợ zoom tới sát mặt từng hành tinh hoặc lùi xa quan sát tổng quan với chuyển động mượt mà (Lerp/Smooth).

---

## Chi tiết các hàm (Methods)

### 1. `Start()`
- **Giải thích code:** Khởi tạo các giá trị ban đầu cho khoảng cách Zoom của Camera (`currentDistance = 20`, `targetDistance = 20`), góc xoay chúi xuống 60 độ để có góc nhìn "God view". Đồng thời ép `nearClipPlane` của Camera cực nhỏ (0.0001) để khi zoom sát rạt vào bề mặt các ngôi sao / mặt trăng sẽ không bị lỗi xuyên thấu mất hình.
- **Mục đích:** Set up cấu hình khởi động an toàn cho ống kính.
- **Nơi được sử dụng:** Unity gọi 1 lần khi bắt đầu game.

### 2. `LateUpdate()`
- **Giải thích code:** Hàm gọi nối tiếp sau khi chạy xong `Update()`. Mã gọi tới 2 module con là `HandleInput()` và `UpdateCameraPosition()`.
- **Mục đích:** Tại sao không xài `Update`? Để chắc chắn rằng hệ thống vật lý (Gravity) đã hoàn tất việc dời chỗ các hành tinh trong khung hình này, sau đó Camera mới bắt đầu dò theo. Như vậy không bao giờ sinh ra lỗi giật hình (stuttering).
- **Nơi được sử dụng:** Unity gọi tự động mỗi khung hình cuối.

### 3. `HandleInput()`
- **Giải thích code:** 
  - Đọc Scroll Chuột để nội suy cấp số nhân (Logarithmic) thay đổi `targetDistance` của Cữ Zoom Camera.
  - Quét ấn phím WASD / Mũi Tên Đứng để trượt camera dạo ngang - Nếu có trượt ngang thì Camera lập tức mất khóa mục tiêu hành tinh (`target = null`).
  - Quét Chuột Phải để xoay góc X/Y của camera ngắm trần hay sàn không gian.
  - Quét phím (1-9) để tra cứu gọi hàm FocusOnBody chọn nhanh hành tinh.
  - Nhận phím cách (Space) để Reset ống kính về Tâm Vũ Trụ nguyên thủy.
  - Truy vết (Raycast) cú click Chuột Trái để chọn bất kì Thiên Thể nào nằm dưới con trỏ chuột.
- **Mục đích:** Bộ tổng đài trung tâm lắng nghe mọi phím bấm của Đại ca.
- **Nơi được sử dụng:** Từ `LateUpdate()`.

### 4. `SelectBody(int index)`
- **Giải thích code:** Dựa trên thứ tự index (0-8 Tương đương 1-9), lấy tên Hành tinh cố định (VD: index 1 "Mercury"). Quét mảng tìm ra thiên thể đúng cái tên này và nạp vào Hàm Focus.
- **Mục đích:** Cung cấp tính năng phím tắt siêu tốc trên bàn phím cho các hành tinh mặc định.
- **Nơi được sử dụng:** Lệnh phím WASD/1~9 nằm dưới bộ Input.

### 5. `FocusOnBody(CelestialBody body)`
- **Giải thích code:** 
  - Set `target` camera bám tụ vào object 3D của Hành tinh này.
  - Lập tức Hãm TimeScale (tốc độ dòng thời gian của Vũ trụ) rẽ xu hướng `0.05` cực trễ để ngắm bề mặt rực rỡ và quỹ đạo nó quay chậm, tránh việc hành tinh trôi xượt qua màn hình trong một chớp mắt.
  - Auto-Zoom tự động lại gần bề mặt Hành Tinh, tính toán thông minh dựa trên Khổ Scale chuẩn của quả cầu đó x (3 hoặc 4) kèm độ biến dạng của `visualScaleMultiplier` hiện tại.
- **Mục đích:** Lock mục tiêu bằng "Súng ngắn tự động Zoom In" siêu việt.
- **Nơi được sử dụng:** Gọi bởi chuột Click trên màn hình và Phím ấn bàn phím báo về từ Input.

### 6. `UpdateCameraPosition()`
- **Giải thích code:** Dựa vào hai luồng dữ liệu là `targetPosition` và `offset` (gồm góc cuộn X,Y + Khoảng cách lùi Z `currentDistance`).
  - Nếu `target` còn sống và Active, dùng Linear Interpolation (Lerp) trượt mượt mà con mắt theo đít khối cầu trên bầu trời.
  - Tính vị trí tuyệt đối = `targetPosition` (tâm ngắm) + `offset` (kéo lùi ống quay lại đằng sau tạo shot nhìn đẹp mượt). Update quay góc hướng ống kính `LookAt` đối diện đối tượng.
- **Mục đích:** Dọn rác toán học để tính ra chính xác Vec-tơ khung tọa độ ảo của camera nạp vào hệ thống 3D.
- **Nơi được sử dụng:** Chốt sổ mỗi khung Frame ở `LateUpdate()`.
