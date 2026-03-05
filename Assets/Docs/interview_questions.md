# Bộ Câu Hỏi Vấn Đáp Dự Án Solar System

Dưới đây là danh sách các câu hỏi dự kiến từ giáo viên (giám khảo) dành cho 3 tổ trưởng phụ trách 3 mảng khác nhau của dự án, bao gồm cả lý thuyết và thực hành code.

---

## 👨‍💻 Tổ 1: Tổ trưởng "Mặt Tiền" (UI, Camera & Cảnh quan)

**Câu 1 (Lý thuyết UI):**
> Giao diện của nhóm đang sử dụng công nghệ gì của Unity? (OnGUI hay UI Toolkit/Canvas?). Ưu nhược điểm của việc dùng công nghệ này so với các loại UI khác là gì?

**Câu 2 (Thực hành Code Camera):**
> "Thầy thấy lúc cuộn chuột để Zoom in/Zoom out, tốc độ nó nhanh quá. Em sửa ở file nào, dòng nào để làm tốc độ Zoom chậm lại một nửa cho thầy xem?"
*(Gợi ý: Trỏ vào `SimulationCamera.cs`, tìm biến `zoomSpeed` hoặc tương tự).*

**Câu 3 (Thực hành Code UI):**
> "Giờ ở bảng thông tin Góc Trên Bên Trái, thầy muốn thêm một dòng text màu Đỏ ghi chữ 'Demo Version', em chèn vào file nào và dùng lệnh gì?"
*(Gợi ý: Mở `SimulationUI.cs`, dùng `GUILayout.Label` kết hợp `GUI.color` hoặc `GUIStyle`).*

**Câu 4 (Lý thuyết Camera):**
> Làm sao Camera có thể luôn luôn "nhìn" (Focus) vào một hành tinh đang di chuyển mà không bị giật lag? Sự khác biệt giữa việc update camera trong `Update()` và `LateUpdate()` là gì?

**Câu 5 (Thực hành Viền Quỹ Đạo):**
> "Thầy thấy khi zoom sát vào hành tinh thì cái vệt line quỹ đạo (Trail) nó tự động mỏng dính lại trông rất thực. Đoạn code đó nằm ở đâu và hoạt động theo nguyên lý nào?"
*(Gợi ý: File `CelestialBody.cs`, hàm `Update()`, thuật toán chia tỷ lệ theo khoảng cách tĩnh `Vector3.Distance` tới Camera).*

---

## 👨‍💻 Tổ 2: Tổ trưởng "Hậu Cần" (Dữ liệu & Môi trường)

**Câu 1 (Thực hành Code Data):**
> "Nhóm em đang lưu trữ thông số quỹ đạo của các hành tinh ở đâu? Thầy muốn thêm hành tinh 'Pluto' (Sao Diêm Vương) màu xám, vận tốc 4.7 km/s vào hệ thì em thêm đoạn code nào và ở file nào?"
*(Gợi ý: Mở `PlanetData.cs`, add thêm 1 object vào mảng/danh sách khởi tạo).*

**Câu 2 (Lý thuyết Khởi tạo):**
> Game Object của hành tinh thực sự được sinh ra (Instantiate) trên Scene ở bước nào? Nếu người dùng bấm "Quay lại mặc định" (Reset), dữ liệu hành tinh được lấy lại như thế nào?
*(Gợi ý: Hàm `RebuildSystem` trong `SolarSystemBuilder.cs` và `PlanetData.ResetToDefaults()`).*

**Câu 3 (Thực hành Settings):**
> "Trong file `SimulationSettings.cs`, biến `timeScale` đang có ý nghĩa gì? Nếu thầy để `timeScale = 1` thì 1 giây ngoài đời trôi qua bằng thời gian bao lâu trong game?"

**Câu 4 (Khắc phục lỗi Layout):**
> "Sao hành tinh Trái Đất và Mặt Trăng ngoài không gian thực cách nhau rất xa, mà trong mô phỏng của các em lại không bị dính vào nhau khi hiển thị? Dữ liệu đồ hoạ được tùy biến so với vật lý thế nào?"
*(Gợi ý: Cơ chế nhân Scale biểu kiến `visualScalePump` cho vệ tinh bật trong phần Cài đặt Hình ảnh).*

**Câu 5 (Thực hành Thêm Chức năng):**
> "Thầy thấy có nút Bắn Mưa Sao Băng. Mưa sao băng thực chất là các cục đá nhỏ được sinh ra ở tọa độ nào, và vòng lặp (Coroutine) nào dọn dẹp (Destroy) chúng để không gây tràn RAM?"
*(Gợi ý: Hàm `SpawnMeteorSwarm` và `DestroyMeteorAfterTime` trong `SolarSystemBuilder.cs`).*

---

## 👨‍💻 Tổ 3: Tổ trưởng "Đầu Não" (Vật lý & Lực hấp dẫn)

**Câu 1 (Lý thuyết Toán/Vật lý):**
> Hệ thống của các em đang áp dụng định luật vật lý nào để tính gia tốc rơi? Công thức lực hấp dẫn $F = G \frac{m_1 m_2}{r^2}$ được thể hiện ở dòng code nào trong `GravitySimulation.cs`?

**Câu 2 (Lý thuyết Lập Trình):**
> Tại sao file `DoubleVector3.cs` lại cần thiết? Unity đã có sẵn `Vector3`, tại sao nhóm em không dùng Unity Vector3 để tính khoảng cách giữa các hành tinh mà phải tự viết `DoubleVector3` lằng nhằng vậy?
*(Gợi ý: Vấn đề sai số thập phân Float Precision limit khi số liệu khoảng cách là quá to (AU) hoặc quá nhỏ. Double chính xác gấp đôi float).*

**Câu 3 (Thực hành Mô Phỏng):**
> "Nếu thầy chơi game và tăng Time Scale lên quá cao (như x400 tốc độ), quỹ đạo của các hành tinh có bị méo mó hay bay chệch quỹ đạo không? Hệ thống làm thế nào để đảm bảo tính ổn định của tích phân?"
*(Gợi ý: Vòng lặp `VelocityVerletStep` và thuật toán chia nhỏ SubStepping tự động động `dynamicSubSteps`).*

**Câu 4 (Lý thuyết Tích Phân):**
> Thuật toán tính tiến trình vật lý của hệ thống sử dụng là Euler hay Verlet? Tại sao Verlet (hay Semi-Implicit Euler) lại bảo toàn năng lượng quỹ đạo hình Elip tốt hơn Euler cơ bản?
*(Gợi ý: Vận tốc và Vị trí được update xen kẽ nhau dựa trên gia tốc của cùng 1 frame).*

**Câu 5 (Thực hành Kiến Trúc):**
> "Cốt lõi kiến trúc của bọn em là **Tách biệt Đồ họa và Vật lý (#Physics != Visual)**. Thầy không hiểu, hãy chỉ cho thầy trong `CelestialBody.cs`, biến nào lưu tọa độ chạy Toán Vật lý, biến nào lưu tọa độ vẽ hình 3D trên màn hình Unity?"
*(Gợi ý: `DoubleVector3 position` dùng cho vật lý, `transform.position` dùng cho hình ảnh hiển thị).*
