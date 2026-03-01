# Phân tích chi tiết Script: CelestialBody.cs

Dạ thưa Đại ca, kịch bản (script) `CelestialBody.cs` được gắn thẳng lên từng vật thể trong vũ trụ (Mặt Trời, Trái Đất, Mặt Trăng,...). Nó vừa chứa đựng dữ liệu vật lý siêu chính xác (Double), vừa xử lý đồ hoạ nén (Float). Dưới đây là phân tích chi tiết toàn bộ các hàm ạ:

## Tổng quan
Linh hồn của script này nằm ở **sự tách biệt giữa Vật Lý (Physics) và Hình Ảnh (Visual)**.
- **Physics**: Chạy trong bóng tối bằng toạ độ DoubleVector3 vô cùng rộng lớn (chuẩn AU).
- **Visual**: Thu hẹp, nén và dồn toạ độ đó lại thành Vector3 nhỏ gọ để nhét vừa màn hình hiển thị.

---

## Chi tiết các hàm (Methods)

### 1. `Initialize()`
- **Giải thích code:** Khởi tạo bộ ba quyền lực của vật lý: `position`, `velocity`, `acceleration` thông qua các biến cấu hình Unity Float ban đầu sang chuẩn Toán học `DoubleVector3` của Hệ thiên văn. Xoay trục đối tượng theo `axialTilt`.
- **Mục đích:** Bắt đầu vòng đời mô phỏng của một vì sao với góc nghiêng và phương hướng chuẩn vật lý.
- **Nơi được sử dụng:** GravitySimulation gọi nhồi xuống khi bắt đầu hoặc nhấn Reset.

### 2. `UpdateRotation(float timeScale)`
- **Giải thích code:** Phân tính góc xoay mỗi frame dựa theo số thời gian / 1 vòng quay (`rotationPeriod`) đổi ra độ. Chạy phép xoay Tương đối (Local Space).
- **Mục đích:** Giúp hành tinh / mặt trời tự quay quay trục của chính nó trơn tru theo gia tốc dòng thời gian.
- **Nơi được sử dụng:** Trình Game Loop (`GravitySimulation.Update`).

### 3. `UpdateVisualPosition(SimulationSettings settings, DoubleVector3 sunPhysicsPos, Vector3 sunVisualPos)`
- **Giải thích code:** "Nhà máy" xử lý nén ảo thị:
  - Hành tinh mẹ thì lấy khoảng cách vật lý thực với Mặt Trời đem ra Nén lại (áp dụng hàm nén của Settings) rồi tuồn ra Model.
  - Vệ tinh (Mặt Trăng) thì phóng đại `visualScalePump = 60f` khoảng cách vật lý của Mẹ->Con lên gấp 60 lần trên màn hình, giúp Mặt Trăng thoát khỏi lưới không gian bị đục thủng bởi bề mặt trái đất 3D khi zoom sát.
  - Điều chỉnh `localScale` khổng lồ/nhỏ xíu tương đối theo Settings biến dạng.
- **Mục đích:** Xử lý render cho mọi trường phái (Game mượt, hay Realistic khô khan). 
- **Nơi được sử dụng:** Trình Game Loop (`GravitySimulation.Update`).

### 4. `SetupTrail(SimulationSettings settings)`
- **Giải thích code:** Xóa bỏ `TrailRenderer` vô dụng cũ của Unity, bơm vào `LineRenderer` thần thánh. Configure Gradient màu đầu vạch sáng rực, đuôi mờ dần. Bo tròn các góc. Khai báo độ dày nở phình đầu bẹt cúp đuôi như sao chổi. Set Buffer tĩnh độ dài 1000 Slots.
- **Mục đích:** Thiết kế công cụ vẽ quỹ đạo phát sáng đỉnh cao, mượt mà ở vận tốc bàn thờ!
- **Nơi được sử dụng:** Gọi 1 lần ở Init.

### 5. `AddOrbitPoint(Vector3 pos)`
- **Giải thích code:** Lấp điểm ảnh vào cái khay Buffer Array vòng tròn (Circular Buffer). Nếu Array chưa đầy thì ghi thẳng vào điểm cuối rồi báo UI vẽ thêm 1 đốt. Nếu đầy khay (1000 điểm trào ra), lập trật tự nén đè lại điểm nhọ nhất - Cập nhật đồng bộ vạn điểm cùng lúc `SetPositions`.
- **Mục đích:** Cơ chế rải mảnh vụn ánh sáng trên bầu trời ít tốn rác CPU nhất có thể.
- **Nơi được sử dụng:** Khung hình (Frame) sau khi tính Toạ độ Visual.

### 6. `SetOrbitMaxPoints(int points)`
- **Giải thích code:** Dựa trên Định luật bảo toàn Kepler (tính chu kỳ n năm T vòng quanh mặt trời) trích ra 1 cái hàm tính số Point giới hạn để Vẽ ĐÚNG 1 VÒNG quỹ đạo là dừng. Rập khuôn Buffer cắt tỉa mảng mượt.
- **Mục đích:** Quỹ đạo luôn tạo thành cái vòng tròn kín kẽ hoàn hảo, không bị đứt đoạn, không bị đè lên nhau.
- **Nơi được sử dụng:** Đầu Init Simulation chuẩn bị mô phỏng tĩnh.

### 7. `ClearTrail()`
- **Giải thích code:** Đánh sập đếm khay buffer về 0. Làm rỗng line vẽ quỹ đạo.
- **Mục đích:** Vỡ trận reset game xóa vạch sáng.
- **Nơi được sử dụng:** Nút bấm Reset trong System.

### 8. `TriggerCollisionVFX(double victimMass)`
- **Giải thích code:** Tính mức độ cực hạn cháy nổ. Nhỏ văng lửa nhẹ, nuốt cực lớn cháy rực. Gọi Coroutine xử lý chuỗi Effect Flash hừng hực.
- **Mục đích:** Visual "Mãn nhãn" cho người chơi khi chứng kiến các siêu tân tinh va vào nhau cắn xé khói lửa.
- **Nơi được sử dụng:** Tương tác Vật Lý Phản Ứng (Collisions) ở Trình cha.

### 9. `HeatUpAndCoolDownVFX(float intensity)`
- **Giải thích code:** Bật biến Shader `_EMISSION`. 
  - (0.2s đầu) Bơm màu lửa Đỏ Cam chói lọi cướp đoạt màn hình (`Color.Lerp`). 
  - (Vài s sau đó) Từ từ làm nguội khối thép nóng chảy đó chìm vào bóng tối.
- **Mục đích:** Coroutine tạo cảm giác nổ rền dẻo dai thay vì bùm 1 phát rồi biến mất thô tiễn.
- **Nơi được sử dụng:** Chạy ngầm từ hàm Trigger ở trên.

### 10. `CheckBlackHole(SimulationSettings settings)`
- **Giải thích code:** Kiểm tra Tích Lượng vượt ngưỡng `blackHoleMassThreshold` chưa? (Chandrasekhar Limit). Trúng phóc thì nhuộm Đen Tuyền khối cầu, bóc sạch vạc Lửa (`Disable _EMISSION`), chuyển vòng Plasma Line đường bay thành màu Tím Bóng Đêm, và chóp tụ thu nhỏ Scale khối lượng siêu đặc. Bắn Console Log Cảnh Báo Siêu Cấp.
- **Mục đích:** Event Cấp Độ Vũ Trụ tối thượng, cái giá phải trả khi nuốt nghẹn hóa hắc ín.
- **Nơi được sử dụng:** Kể từ lúc Nuốt Khối Lượng kình địch (HandleCollision).
