# Bản Giới Thiệu Dự Án: Hệ Mặt Trời Mô Phỏng (Solar System Simulator)

## 🌌 1. Tổng Quan Dự Án
**Solar System Simulator** là một trình mô phỏng bầu trời đêm 3D trực quan, tái hiện lại Hệ Mặt Trời thu nhỏ của chúng ta ngay trên màn hình máy tính.

Dự án nhấn mạnh vào việc cân bằng hoàn hảo giữa tính vật lý chân thực và vẻ đẹp đồ hoạ viễn tưởng, mang tới trải nghiệm vũ trụ tuyệt đẹp cho người dùng.

## ⚙️ 2. Công Nghệ và Kiến Trúc
- **Nền tảng:** Unity 3D Engine.
- **Ngôn ngữ:** C# (Oriented Object Programming).
- **Kiến trúc vạn vật hấp dẫn (Gravity Core):**
  - Tách biệt hoàn toàn tính toán **Vật lý (Physics)** và **Đồ hoạ (Visual)**.
  - Sử dụng định lượng số chấm động chính xác kép (`double precision`) kết hợp thuật toán **Velocity Verlet** bảo toàn năng lượng để vẽ ra các đường quỹ đạo hình elip hoàn hảo. Khoảng cách tính bằng AU thật.
  - Khoảng cách đồ hoạ (Visual) dùng toạ độ `float` nén lại để vừa văn màn hình mà không phá vỡ vật lý chuẩn.

## 🚀 3. Các Tính Năng Nổi Bật (Features)

### 3.1. Phóng đại thực tế & Cung cấp Kiến thức
- Thông tin **thực tế theo thời gian thực (Real-time)** về các hành tinh: bao gồm Vận Tốc Hiện Tại (km/s), Khoảng cách tới Mặt Trời (AU), Khối Lượng.
- Tính năng **Bách khoa toàn thư**: Click chuột vào hành tinh bất kì để tìm hiểu thông tin tổng quan, đặc điểm nổi bật nhất theo chuẩn khoa học tiếng Anh.

### 3.2. Hiệu Ứng Bầu Trời (Visual Polish)
- Chế độ hiển thị thiên hà 4K (*Milky Way High-Res Skybox*) cực kỳ chi tiết.
- Hiệu ứng **Mưa Sao Băng (Shooting Stars)** bay xẹt qua khung hình tạo cảm giác không gian sinh động.
- Tính năng **Dynamic Trail Anti-Zoom Bloat**: Tự động tinh chỉnh nội suy kích cỡ viền quỹ đạo dựa vào góc máy quay của người dùng, giúp đuôi hành tinh luôn mượt mà thanh tịnh kể cả khi zoom cực đại.

### 3.3. Tương tác Thảm Họa Định Mệnh
- Tính năng triệu hồi **Mưa Thiên Thạch (Meteor Swarm)** trực tiếp dội thẳng vào Trái Đất hoặc ngẫu nhiên. Sau 5 giây, mảnh vỡ bay ngang qua bề mặt thiên thể sẽ tự động thiêu rụi.
- Nhập **Hành Tinh Sát Thủ (Rogue Planet)** từ ngoài cõi hắc ám lao thẳng vào Hệ Mặt Trời với khối lượng gấp trăm lần, đủ sức xé nát quỹ đạo mọi thứ trên đường đi của nó hoặc tạo thành *Lỗ Đen* nhân tạo.
- Bảng điều khiển năng động cho phép tùy chỉnh: Tăng giảm vận tốc quỹ đạo vòng quanh Mặt Trời, thay đổi Gia tốc trọng trường thời gian thực mô phỏng sự biến dạng.

### 3.4. Trải Nghiệm Giao Diện Người Dùng
- Tuẩn thủ tiêu chuẩn **English In-Game UI Interface** đẹp mắt và súc tích.
- **Control Instruction Board:** Luôn hiển thị trên màn hình để hỗ trợ người dùng mới biết cách điều hướng góc nhìn và tương tác với vũ trụ.
- Pause hệ thống thời gian ảo gọn gàng mỗi khi thoát ra menu chính, và reset Simulation sạch sẽ dễ dàng qua phím (R).

## 💡 4. Lời Kết
**Solar System Simulator** không chỉ là một bài toán tính quỹ đạo nhàm chán mà là bức tranh vũ trụ tương tác kết tinh từ sức mạnh của toán học Verlet. Đây chính là tấm vé đưa người trải nghiệm lơ lửng giữa những vì chòm sao diệu kì.
