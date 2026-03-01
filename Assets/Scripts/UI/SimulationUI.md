# Phân tích chi tiết Script: SimulationUI.cs

Dạ bẩm Đại ca vĩ đại, hệ thống Bảng Điền Khiển Trung Tâm - Nơi Đại ca vẩy tay là Thao Túng Vũ Trụ - `SimulationUI.cs` đang chờ đợi mòn mỏi sự quan tâm của Đại ca! Kịch bản này lo liệu tất tần tật Phím Bấm Giả Lập trên Màn Hình.

## Tổng quan
Nó sử dụng hệ thống Đồ Họa Cốt Lõi `OnGUI()` (chuẩn Cổ điển Unity) để vẽ vời giao diện cực kỳ lỳ lợm không bị hỏng hóc giữa chừng. Giúp điều chỉnh Tua Thời Gian sống, Bắn Mưa Thiên Thạch, Đổi Scale Map Trực Tiếp hoặc Độ Cân Nặng Hành Tinh Online trực tuyến.

---

## Chi tiết các phần & hàm (Methods)

### 1. `OnGUI()`
- **Giải thích code:** Dựng Giao Diện Lên Màn Hình:
  - Chỗ Vẽ Góc Trái: Cụm thanh trượt Time Scale thời gian bóp méo gia tốc (Đại ca thích tua 10x 400x thì nhấc tay trượt slider), Hệ Số Gravity lực kéo, Nút Phá Cấu Trúc Đổi Sang "Friendly Mode" khổng lồ cho đẹp góc hoặc "Realistic Mode" thực hóa teo nhỏ. Tắt bật cái Dải Quỹ đạo đường bay của Sao `showOrbits`.
  - Hai Cái Nút Tai Họa Đỏ/Vàng để Triệu Hồi Bão Thiên Thạch/RoguePlanet siêu ngầu hủy diệt không gian. Nút Reset Default World bẻ lái về như cũ.
  - Chỗ Ghi Giữa Đáy: Cụm Chữ Help xám nhỏ gọn liệt kê các phím nóng WASD 1,2,3 Space để chỉ điểm cho người chơi. Tắt bật nhờ phím H `showHelp`.
  - Tracking Phím Tắt: Bắt tín hiệu phím R chọc thẳng lệnh Refresh lại Core Universe Gravity.
  - Bảng Điểm Mặt Chọn Thắng (Góc Phải): Khi ngắm bắn dính vào thằng nào đó, bảng này lòi ra Tên thằng đó, Cung cấp 3 cục InPut Chữ Box để Đại ca điền Gõ chữ vào (Sửa Cân Nặng M☉, Thay đổi trực tuyến Tốc độ Bay km/s, Độ xa kéo giãn AU). Kèm nút Xóa Hành tinh (xóa sổ). Chốt Nút "Apply Alters" múa lửa để ép code lưu mốc chỉnh sửa.
- **Mục đích:** Quản Gia Giao Diện lo Mạch Tương Tác cho Vua Vũ Trụ. 
- **Nơi được sử dụng:** Render đè qua Camera DrawCall bằng luồng riêng.

### 2. `ApplyEditing(CelestialBody body)`
- **Giải thích code:** Nuốt cục Data Thập phân (String chữ cái) mới đánh chữ trên Box gởi về, dịch nó thành số thật (Double.TryParse):
  - **Khối lượng:** Ốp thẳng số Mass và check xem liệu với khối lượng khủng điên cuồng này có đủ châm ngòi hàm Biến Nó Thành "LỖ ĐEN" (`CheckBlackHole`) hút kiệt hệ thống hay không!
  - **Vận Tốc:** Quy đổi số KM/s về Tỷ lệ Vũ Trụ Nội tại AU/Ngày (Chia 1731.5). Trích Vector chiều bay cũ dồn vô độ dài tốc độ mới. Nếu đang đứng khựng thì đút Hướng Đông (1, 0, 0) xô bắt nó đi.
  - **Khoảng chênh:** Áp sát vị trí Vector Dịch chuyển AU cách lõi Mặt trời ra một cung trăng mới y theo Số Điền. Kéo còi Báo Cho Script Data (`UpdateSpawnDistance`) lưu Mốc Save này vĩnh viễn đừng mất trong session nếu Đại ca nhấn Reset World.
  - **Kết Hậu:** Clear sạch Bóng Trail cũ dọn rác dơ dáy của dải phát sáng.
- **Mục đích:** Công cụ Hack Mod in-game trực tuyến siêu cấp tối cao giúp ép chín trái cây ép ép hệ thống ngấm lực.

### 3. `ResetAllPlanetsToDefault()`
- **Giải thích code:** Thả búa đập sạch đồ họa 3D lởn vởn. Điểm tên Thằng Builder Cò mồi `RebuildSystem` để đúc lô Hành Tinh Mẻ Mới nguyên xi. Reset Gia tốc Thời Gian, Gravity về Default = 1.0. Kêu gọi Gravity Core khởi động dây chuyền dò chạm lại 1 lần nữa (`InitializeSimulation`).
- **Mục đích:** "Đổi Mới Khởi Đầu", đem con Tạo hóa về 0 cho sạch đẹp không màng chuyện dĩ vãng ăn uống nhau vỡ tan tành nữa.
