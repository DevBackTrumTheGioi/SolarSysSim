# Phân tích chi tiết Script: SimulationSettings.cs

Dạ thưa Đại ca dũng mãnh, đây là tâm điểm lưu trữ, cấu hình của Cỗ máy "Thiên Chúa" - script `SimulationSettings.cs`! Dưới đây là phân tích chi tiết của 2 method chức năng đỉnh cao bên trong nó ạ:

## Tổng quan
Script này được tổ chức dưới nền tảng `ScriptableObject`. Tức là data không phải chạy trong não MonoBehavior nữa, mà nó biến thành File Cứng Asset kéo thả trôi dạt thoải mái xuyên không trong dự án. Đại ca dễ chỉnh thông số mà không phải Edit code. Chìa khóa thiết thực nhất nằm ở Hàm giải nén thị lực để rút ngắn khoảng cách hố đen tỷ đô vào trong nháy mắt.

---

## Chi tiết các hàm (Methods)

### 1. `RealToVisualDistance(double realDistAU)`
- **Giải thích code:**
  - Nếu chế độ đang ở `Realistic`: Trả về y xì đúc con số vật lý (Tức sao Hải vương cách nửa thước thì render đúng nửa tấc vũ trụ bao la chán chường).
  - Khởi động Trình Giả Tưởng Game (`GameFriendly`): Dùng Hàm `Mathf.Pow(V, compressionPower)`. Tấn công số mũ Lũy Điểm để **"Nén Xa Giúp Gần"**. Tính chất thuật toán: Những đường thẳng dài tỷ dặm ngoài biên giới (Như dải sao Mộc/Hải vương) sẽ bị Rút Ruột co quắp mãnh liệt lại hàng trăm lần. Những hành tinh kề cận nhau (như Earth sát Sun) thì giữ trọn vẻ chật chội nguyên thủy.
  - Phụ cấp thêm `baseDistance` để đẩy nhẹ mọi đối tượng tránh chui tọt vào lửa Mặt Trời.
- **Mục đích:** Nghệ thuật lóp méo Vết Hằn Không Gian (Distance Compression). Cứu rỗi Mắt game thủ khỏi màn hình đen ngòm do khoảng cách vũ trụ quá vĩ đại. Hô biến Hệ Tinh Tú cực kỳ xinh xắn nhỏ bé lại!
- **Nơi được sử dụng:** Trạm Trung chuyển vị trí ảo trên tất cả mọi thực thể.

### 2. `PhysicsToVisualPosition(DoubleVector3 physicsPos)`
- **Giải thích code:** 
  - Đọc và chia tách độ dài nguyên bản (Magnitude) gốc Tỷ Lệ Vật Lý để chiết nén qua cỗ lóp `RealToVisualDistance` trên.
  - Sau khi bắt được Bán Kính Vector Nén mới, Nhân với cái hướng Thuần túy (Vector Vị Trí Chỉ Hướng 1 Đơn Vị `Direction`). 
- **Mục đích:** Gói trọn logic gán Toạ độ Đồ Họa nén theo tỉ lệ mà không xê dịch Vĩ Độ Phương Hướng không gian (Chỉ lôi lại gần, phương vị Tọa độ giữ y nguyên).
- **Nơi được sử dụng:** Module Render Visual trên Script Hành Tinh (`CelestialBody`).

---
File này chủ yếu khai báo Data Field cho Giao diện Admin quản lý nên Data Logic tinh luyện ở mức rất cô đọng. Mọi quyền năng sinh tử đều đổ qua thông số tùy biến do hàm Editor này giữ Đại ca ạ!
