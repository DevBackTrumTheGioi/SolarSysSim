# Phân tích chi tiết Script: PlanetData.cs

Dạ bẩm Đại ca, đây là Quyển Sách Sinh Mệnh `PlanetData.cs`, cuốn gia phả chứa đựng số đo 3 vòng của các Đấng Sáng Tạo trong Hệ thống Hệ Mặt Trời dựa theo chuẩn Cơ Quan Hàng Không Vũ Trụ NASA.

## Tổng quan
100% Cấu trúc File này là Static Data (Dữ liệu Tĩnh tĩnh tại) không biến động. Kịch bản này không thèm thừa kế `MonoBehaviour` của Unity. Điểm đặc sắc là bộ Code-base đã quy đổi tất tần tật sang Chuẩn Đơn vị Hàng Không chuẩn `AU` / Trọng Lượng Mặt Trời thay vì m/s.

---

## Chi tiết các phần tử & hàm (Methods)

### 1. `struct BodyInfo`
- **Giải thích code:** Cấu trúc dữ liệu khung xương bao gồm:
  - Cân nặng (mass),
  - Khoảng cách rẽ nước so với Mặt trời (distanceFromSun),
  - Độ bự bản đồ vật lí (radius) và màn ảnh (visualScale),
  - Vận tốc cào xé không gian góc 90 độ ngang (orbitalVelocity),
  - Độ Nghiêng (axialTilt), Thời lượng Ngày Đêm (rotationPeriod)
  - Khóa Quỹ đạo Con - Mẹ (orbitParentName).
  - Tên File chứa cái Vỏ 3D (prefabPath).
- **Mục đích:** Gom nhóm các thông số rời rạc vào một Thể thống nhất rành mạch.

### 2. Các hằng số TĨNH (`public static readonly`)
- **Giải thích code:** Khởi xướng Hằng Số Hấp Dẫn Siêu Vỡ Lòng `G_SIM = 2.9592e-4`.
- Liên lỉ liệt kê Data Khởi Tạo cho 9 vị thần tiên: `Sun`, `Mercury`, `Venus`, `Earth` (với cả `Moon`), `Mars`, `Jupiter`, `Saturn`, `Uranus`, `Neptune`.
- Ở mỗi Khối khởi tạo chứa đầy số liệu toán khổng lồ như Khối lượng chênh hàng chục số 0, hay Sao Kim (Venus) bị Âm góc xoay trục làm nó quay lùi khác bọt.
- Gói toàn bộ cả 9 Hành Tinh này chôn vào 1 List Array có tên `AllBodies` cho hệ thống Builder trích xuất.

### 3. `UpdateSpawnDistance(string bodyName, double distance)`
- **Giải thích code:** Vòng lặp duyệt Mảng `AllBodies`. Nếu khớp tên Đại ca vừa chỉnh trong Game (In-game UI), nó thay đổi biến `distanceFromSun` ban đầu của Quả bóng vũ trụ đó sang độ dài mới mà Đại ca gõ vào.
- **Mục đích:** Để khi Đại ca bực mình nhấn Reset World, hành tinh nó sẽ chòi lại đúng chỗ Đại ca vừa chỉnh mà không bị trôi ngược về điểm gốc vĩnh hằng ban sơ. Nâng lùi biên giới dễ dàng!
