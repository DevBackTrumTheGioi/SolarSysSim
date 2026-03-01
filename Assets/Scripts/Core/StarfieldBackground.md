# Phân tích chi tiết Script: StarfieldBackground.cs

Dạ thưa Đại ca, kịch bản `StarfieldBackground.cs` là lớp áo choàng lông bào khoác lên toàn bộ khung cảnh u tối của game. Nó giả lập ra dải ngân hà với hàng vạn tinh tú. Dưới đây là phân tích chi tiết ạ:

## Tổng quan
Script lợi dụng triệt để sức mạnh của Hệ Thống Hạt (`ParticleSystem` GPU) của Unity để vẽ nên 10.000 ngôi sao cực kỳ nhẹ máy. Điểm xuyết trên đó là dàn sao băng xẹt xẹt siêu ấn tượng. Các vì sao được khoá cố định trên bầu trời thay vì trôi lệch như đồ vật.

---

## Chi tiết các hàm (Methods)

### 1. `Start()`
- **Giải thích code:** 
  - Khởi tạo GameObject độc lập "StarfieldSphere".
  - Châm ngòi Hệ Thống Hạt (Particle System) chính cho background. Tắt các module động lượng, chỉ giữ lại Particle Mesh cơ bản với material Default không đổ bóng. Phóng cấu hình 10.000 điểm.
  - Phân nhánh đẻ thêm Cụm Hạt Số 2 mang tên "ShootingStars" (Sao Băng). Gói hạt này được Setup Shape Sphere khổng lồ, StartSpeed cực cao, Scale bị `Stretch` nhòe đứt đuôi tạo ra vệt sáng chói lóa.
- **Mục đích:** Xây nhà máy sản xuất rác vũ trụ (Hạt bụi ánh sáng) ở khâu chuẩn bị đồ họa.

### 2. `CreateStars()`
- **Giải thích code:** Chạy vòng lặp cực lớn 10k lần ngốn CPU tí chút ở lúc Load:
  - Spawn tỏa đều 10k hạt bám dính vào Vỏ Màng Không Gian Cầu (`OnUnitSphere`) cách Camera 500 Đơn Vị.
  - Ramdomize (Xóc tỷ lệ ngẫu nhiên) màu Sắc: Đa phần là trắng, thi thoảng lẫn Vàng, Xanh Lam nhạt sặc sỡ và độ sáng Alpha lốm đốm.
  - Set Thời gian sống của tụi nó là `Mathf.Infinity` (Bất tử trường tồn).
  - Ép Mảng Hạt Cứng (`Particle[]`) quất thẳng xuống Engine C++ bên dưới cho Card đồ họa tự vẽ vĩnh viễn không cần CPU đoái hoài nữa.
- **Mục đích:** Sơn 10 nghìn chấm màu lên phông nền lười biếng. Cực kì tiết kiệm năng lượng tính toán vì CPU chỉ làm việc đúng 1 Frame.

### 3. `LateUpdate()`
- **Giải thích code:** Cứ mỗi khung hình cuối cùng, Bụi Sao Băng và Bụi Khung Nền này sẽ Auto-Follow theo dịch chuyển `Camera.transform` (Dời Toạ độ Tịnh Tiến).
  - TUY NHIÊN, nó khóa cứng Trục Xoay `Quaternion.identity`. Nghĩa là khi ta lắt tròng mắt để liếc nhìn, Dải sao cũng không bị léo nhéo xoay theo mũi ta, tạo ảo giác Phông nến rất rất xa cự li vài Năm Ánh Sáng.
- **Mục đích:** Đu bám Camera như một cái lồng kính nhốt người chơi để đi đâu cũng được vây quanh bởi triệu vì sao.
