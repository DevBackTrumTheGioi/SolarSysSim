# Phân tích chi tiết Script: DoubleVector3.cs

Dạ thưa Đại ca, đây là phân tích về tệp xương sống toán học `DoubleVector3.cs` của chúng ta, giúp mô phỏng chính xác tuyệt đối ở cấp độ Hệ Mặt Trời ạ:

## Tổng quan
Mặc định vector trong Unity là Float (dấu phẩy động 7 chữ số) - Rất thảm hoạ nếu đo khoảng cách Mặt Trời cách sao Vương (vài chục tỷ KM).
Script Custom `DoubleVector3` ép hệ tĩnh tọa độ vật lý lên tầm **Double-Precision (15 chữ số thập phân)**, giải cứu game khỏi cảnh hành tinh bay giật giật, văng sai quỹ đạo vì sai số thập phân dồn dập.

---

## Chi tiết các hàm (Methods)

### 1. Hàm tạo tĩnh `DoubleVector3(x, y, z)` và Const tĩnh
- **Giải thích code:** Định nghĩa constructor với tham số truyền vào là Double. Đi kèm là các Hằng số (Constants) như `.zero`, `.one`, `.up` đúc khuôn từ tọa độ số thực kép nguyên chất.
- **Mục đích:** Cung cấp cú pháp khai báo giống hệt `Vector3` truyền thống của Unity để dễ bảo trì/sử dụng. 

### 2. Thuộc tính `sqrMagnitude` & `magnitude`
- **Giải thích code:** 
  - `sqrMagnitude`: Cộng tổng bình phương $(X^2 + Y^2 + Z^2)$ của Vector.
  - `magnitude`: Trả về Căn Bậc 2 chiều dài của vector.
- **Mục đích:** Tính khoảng cách vector. Luôn ưu tiên dùng `sqrMagnitude` trong đếm toán va chạm vì làm hàm căn (`Sqrt`) sẽ rút máu CPU rất kinh khủng.

### 3. Thuộc tính `normalized`
- **Giải thích code:** Nếu Vector có độ dài (magnitude) > siêu rập 1e-15, nó chia từng thành tố vector cho chiều dài đó để ép độ lớn toàn cục thành đúng 1 đơn vị. Ngược lại set = Zero.
- **Mục đích:** Lấy "Hướng Đi" tuần túy (Scale=1), xóa bỏ biên độ lực. Sử dụng để đo vector gia tốc tiến / lùi cực chuẩn xác trong Physics lực kéo.

### 4. Overload Các Phép Toán Cơ Bản (`+, -, *, /`)
- **Giải thích code:** Định nghĩa lại bằng Keyword `operator` phương thức cộng/trừ 2 `DoubleVector3` cho ra cấu hình mới. Định nghĩa nhân/chia vector với 1 biến số Scale (Double).
- **Mục đích:** Giúp thuật toán trọng trường viết tự nhiên: `body.velocity * dt + body.acceleration` chả hạn, biến code Toán học thành thơ.
- **Nơi được sử dụng:** Tính tích phân Update Verlet và rải công thức Kepler ở toàn bộ dự án.

### 5. `Dot(a, b)` và `Cross(a, b)`
- **Giải thích code:** 
  - Dot: Tính tích vô hướng của 2 Vector ($a_x*b_x + a_y*b_y + a_z*b_z$).
  - Cross: Tính tích có hướng ảo tung chảo để tạo 1 vector vuông góc với cả mặt phẳng a và b tạo ra.
- **Mục đích:** 
  - Tích vô hướng dùng trong hàm dò tìm Điểm cắt va chạm (Continuous Collision Detection). 
  - Tích có hướng dự trữ cho vẽ mặt cắt (Planar).
- **Nơi được sử dụng:** Module Giải toán phương trình kiểm soát sáp nhập Thiên thạch siêu vận tốc (CCD).

### 6. `Distance(a, b)` & `SqrDistance(a, b)`
- **Giải thích code:** Tính khoảng cách của 2 đối tượng đếm qua Hiệu vector.
- **Mục đích:** Dò khoảng cách thực đo Lực hút và Vùng an toàn của Collision hit boxes.

### 7. Module Bridge Data (`ToVector3`, `FromVector3`)
- **Giải thích code:** 
  - `ToVector3`: Hạ cấp và gọt số nhét từ Double về lại Float Unity `Vector3`.
  - `FromVector3`: Thăng cấp đẩy Float UnityEngine trọn vẹn lên Double cấu trúc số ảo.
- **Mục đích:** Vật lý tính toán bằng Double (Tỷ số AU), nhưng khi nộp bài cho Unity Màn hình vẽ lên đồ họa vật thể bắt buộc nó chỉ hiểu hệ Float (Thứ nguyên Render). Do đó đây là trạm Kiểm định trung chuyển thông dịch mã duy nhất ở cuối chu trình. Cầu nối sống còn của thiết kế Architecture.
- **Nơi được sử dụng:** Trong mọi ngóc ngách Script `Visual` và Gán `transform.position`.
