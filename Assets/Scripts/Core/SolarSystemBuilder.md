# Phân tích chi tiết Script: SolarSystemBuilder.cs

Dạ thưa Đại ca, tệp tin này - `SolarSystemBuilder.cs` - chính là Đấng Tạo Hóa (Creator) thực sự của toàn cõi không gian. Nó đọc số liệu khô khan, nhào nặn bùn đất tạo vật thể 3D và thắp sáng thế giới. Xin gửi Đại ca báo cáo chi tiết:

## Tổng quan
Script hoạt động như một Lò Ấp Hành Tinh thời gian ban đầu (Và cả trong Runtime lúc chơi mượt). Lấy mẫu Prefab 3D từ các gói Assets lung linh siêu hạng, khớp nối nó vào cấu trúc Toán Học, và đặc biệt là đẻ các thảm họa Diệt Chủng như Mưa Thiên Thạch hay Hành Tinh Ăn Thịt.

---

## Chi tiết các hàm (Methods)

### 1. `Awake()` & `LateUpdate()`
- **Giải thích code:** 
  - `Awake`: Dập Tắt Skybox làm Không Gian tối trui rèn Đen huyền bí để nhường chỗ ánh sao. Gọi Lò Ấp đẻ hành tinh. Khai hỏa Mồi Ánh Sáng Trung tâm tâm lõi hệ Mặt Trời.
  - `LateUpdate`: Giúp Nguồn Đèn Pin trung tâm luông chạy theo khóa dính vào Mặt Trời dẫu dải Tinh hà có trôi dạt Sun Drift.
- **Mục đích:** Điểm hỏa vòng lặp bắt đầu.
- **Nơi được sử dụng:** Auto gọi.

### 2. `SetupBackground()`
- **Giải thích code:** Đánh tan Mây Trời Skybox Engine rườm rà. Ép Màu Base màu Đen Đậm Nhá Không Gian vũ trụ (`Color.black`). Môi trường bóng đổ flat mờ ám uẩn.
- **Mục đích:** Setting màn chơi thành Không gian Tối Cao sâu thẳm chân thật nhất.

### 3. `FindPrefab(string bodyName)`
- **Giải thích code:** Scan list `planetPrefabs` Array thả vào ở inspector, làm sạch chuỗi `ToLower()`, tìm khớp với File 3D chuẩn xác có tên tương đồng trong gói Assets đẹp mê li. 
- **Mục đích:** Ghép linh hồn Dữ liệu với Thân xác Đồ họa (Mesh).

### 4. `CreateSunLight()`
- **Giải thích code:** Trảm sạch sẽ ngọn đèn nhân tạo cũ (Directional Light). Vẽ một mồi Đèn `PointLight` (Bóng sáng tròn chiếu xung quanh) kẹp đúng vào điểm trung tâm của Lõi Mặt Trời với Cường độ nướng khét lửa tỏa.
- **Mục đích:** Tạo ra Mặt Trời có thể chiếu sáng hắt sáng 1 phía bán cầu của mọi hành tinh xoay quanh, tạo hiện tượng Nhật Thực / Không Gian Bối cảnh thực.

### 5. `BuildSolarSystem()` & `CreateBody(PlanetData.BodyInfo data)`
- **Giải thích code:** Trái tim của nhà in!
  - Loop mảng tĩnh chứa Tàng hình dữ liệu Hành tinh thực tế lấy từ JSON cứng `PlanetData`.
  - Nếu lỡ không móc được Model đẹp, rơi xuống nấc **Fallback** tự in khối Cầu (Sphere) xi măng Unity bôi sơn màu trần trụi nhét vô xài tạm.
  - Tích hợp Script Vật Lí vào nó `CelestialBody`. Gán cân nặng, kích thước Visual nguyên thủy. Bóc sạch Vật Lý Collider 3D nhảm (do ta tự tính CCD).
  - Thuật Toán Hồi Qui Dữ Liệu Tọa Độ Phương Hướng (Quỹ đạo tròn đều) dựa trên Tận Khoảng Cự li mặt trời tính ra Cos & Sin góc xoay phẳng cho chuẩn đòn. Nếu Khối lượng "Trẻ em" có Bố Mẹ (Vệ Tinh) thì Hook Lệnh Offset dịch nó bao quanh Quỹ đạo ông Bố thay vì quay quanh Rốn vũ trụ.
- **Mục đích:** Factory Nhà Sản Xuất Khối Tinh Cầu hàng loạt 0 độ sai vặt.

### 6. `ClearAll()` & `RebuildSystem()`
- **Giải thích code:** Đạp sập Loop Child Node xé toạc mọi vật thể Object bay rụng vỡ về với rác thùng thu gom `DestroyImmediate`. Gọi đẻ lại Hệ Mặt Trời từ vũng bùn tro tàn sơ khai và cắm đèn sáng mới.
- **Mục đích:** Đồ Tể đập đi xây lại khi Đại ca nhấn Reset Restart cho sướng tay.

### 7. `SpawnRoguePlanet()`
- **Giải thích code:** Đúc một Quả Cầu Sinh Mệnh Đỏ thẫm đầy sát khí (Nặng 500 Tấn Mặt trời, Scale quái vật). Tọa độ quăng tận Rìa không gian (Góc xa X:30 Z:30). Chích thẳng Véc Tơ Đâm Xuyên Căm Phẫn (Vận tốc 0.5 AU/Ngày Siêu thanh càn quét Core) cắm cổ đâm thẳng vào trung tâm Thái dương hệ Lõi Mặt Trời. Setup hàm trích ép động năng (Dynamic) Live Simulation.
- **Mục đích:** Boss Ải Khủng Cuối. Con Hành tinh lang thang bay tới nghiền nát hệ quy chiếu không gian.

### 8. `SpawnMeteorSwarm()`
- **Giải thích code:** Móc một chùm 20 viên Sỏi Đá Không Gian Trôi Dạt rải lộn xộn bao bọc Viền mâm hệ Mặt trời. Ban cho nó cú Hích Động năng chệch ngẫu nhiên bay xoáy trôn ốc bắn vãi rác vào phía vòng xuyến Trái Đất/Hỏa Tinh. Tạo Mưa Bão Thiên Thạch bao chùm toàn cục Map game phá bỏ hệ sinh thái.
- **Mục đích:** Gieo rắc bụi siêu thực, sự kiện hỗn mang.

### 9. `SpawnTargetedMeteorSwarm(CelestialBody targetBody)`
- **Giải thích code:** Sự Kiện Thiên Tai Có Chủ Đích! Thay vì bay rải rác vòng viền map, nó spawn hẳn 1 Đám Cụm Bão Cát Siêu Đạn cách "Thanh niên xấu số được chọn" có 2.5 Không Gian AU. Mọi thông số hướng lực góc lượn bị Bắn Khóa Tiêu Điểm Tập Pháo (`aimDir`) cắm đầu thẳng vào quả cầu đó với Vận Tốc Bàn Thờ Cực Phẩm bứt phá Không gian.
- **Mục đích:** Đây chính là tính năng Mới Update chói lọi! Nhắm vào Earth để tạo đại hỏa hoạn tuyệt đỉnh phá màn cho Đại ca chơi!
