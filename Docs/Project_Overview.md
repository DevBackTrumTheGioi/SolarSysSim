# Solar System Simulation (SolarSysSim) - Technical Design Document

## 1. Tổng quan Dự án (Project Executive Summary)

Tài liệu này mô tả kiến trúc kỹ thuật và lộ trình phát triển cho dự án giả lập Hệ Mặt Trời (Solar System Simulation). Mục tiêu chính của dự án là xây dựng một môi trường mô phỏng vật lý thiên văn chính xác, nơi các thiên thể tương tác với nhau thông qua lực hấp dẫn theo thời gian thực, thay vì sử dụng các chuyển động hoạt hình (animation) định sẵn.

Dự án được phát triển trên Unity Engine, sử dụng ngôn ngữ C#.

---

## 2. Nguyên lý Vật lý Cốt lõi (Gravity Core Physics)

Hệ thống mô phỏng dựa trên cơ chế **N-body Gravity Simulation** (Mô phỏng đa vật thể). Mọi tính toán chuyển động đều tuân thủ nghiêm ngặt các định luật vật lý Newton.

### 2.1. Công thức Toán học
Động lực học của các thiên thể được điều khiển bởi **Định luật Vạn vật hấp dẫn của Newton**:

$$ F = G \frac{m_1 m_2}{r^2} $$

Trong đó:
*   **$F$**: Lực hấp dẫn (Vector lực).
*   **$G$**: Hằng số hấp dẫn. *Lưu ý: Trong môi trường Game Unity, hằng số này sẽ được điều chỉnh (scale) để phù hợp với tọa độ thế giới ảo, tránh việc các giá trị quá nhỏ hoặc quá lớn gây lỗi Floating Point.*
*   **$m_1, m_2$**: Khối lượng của hai thiên thể đang tương tác.
*   **$r$**: Khoảng cách Vector giữa tâm hai thiên thể.

### 2.2. Triển khai Kỹ thuật trong Unity
*   **Unity Physics Engine**: Không sử dụng Gravity mặc định của Unity (Set `Physics.gravity = Vector3.zero`). Tự xây dựng hệ thống quản lý lực riêng.
*   **Time Step**: Mọi tính toán vật lý được thực hiện trong vòng lặp `FixedUpdate()` để đảm bảo tính đồng bộ và ổn định, độc lập với Frame Rate (FPS).
*   **Phương pháp Tích phân (Integration Method)**: Sử dụng phương pháp Semi-Implicit Euler hoặc Velocity Verlet thông qua `Rigidbody.AddForce()` để cập nhật vận tốc và vị trí.

---

## 3. Các Phân hệ Chức năng (Functional Modules)

Dự án được chia thành 3 nhóm chức năng chính, sắp xếp theo thứ tự ưu tiên triển khai.

### Nhóm 1: Cơ chế Lõi & Môi trường (Core Mechanics)
*Mục tiêu: Xây dựng nền tảng vật lý và hiển thị cơ bản.*

1.  **Hệ thống Render Thiên thể (Celestial Rendering)**
    *   **Mô tả**: Hiển thị các hành tinh và Mặt trời dưới dạng 3D.
    *   **Kỹ thuật**: 
        *   Sử dụng Sphere Mesh độ phân giải cao.
        *   Shader Graph: Diffuse map (bề mặt), Normal map (địa hình), Emission map (phát sáng cho sao).
        *   Lighting: Point Light đặt tại tâm Mặt trời để tạo bóng đổ thực tế cho các hành tinh.

2.  **Bộ giả lập Trọng lực (Gravity Manager)**
    *   **Độ phức tạp**: Cao.
    *   **Mô tả**: Class trung tâm quản lý danh sách tất cả các thiên thể (`CelestialBody`). Trong mỗi frame vật lý, hệ thống duyệt qua từng cặp thiên thể, tính toán vector lực hấp dẫn và áp dụng lực đó lên `Rigidbody`.
    *   **Thách thức**: Cân bằng thông số $G$ và `Initial Velocity` (Vận tốc ban đầu) để tạo ra quỹ đạo Ellipse ổn định, ngăn chặn hiện tượng hành tinh bị hút chặt vào mặt trời hoặc văng ra khỏi hệ thống.

3.  **Điều khiển Thời gian (Time Controller)**
    *   **Mô tả**: Hệ thống cho phép bẻ cong thời gian mô phỏng mà không ảnh hưởng đến hiệu năng render.
    *   **Kỹ thuật**: Sử dụng một biến `static float simulationSpeed`. Giá trị của biến này sẽ nhân với vector vận tốc hoặc `Time.fixedDeltaTime` trong các tính toán vật lý. Không can thiệp vào `Time.timeScale` của Unity để đảm bảo UI và Input vẫn mượt mà.

### Nhóm 2: Tương tác & Trải nghiệm Người dùng (UI/UX)
*Mục tiêu: Cung cấp công cụ quan sát và phân tích dữ liệu.*

1.  **Camera Quỹ đạo (Orbital Camera Controller)**
    *   **Tính năng**:
        *   **Free Look**: Xoay quanh tâm hệ mặt trời.
        *   **Zoom**: Thay đổi Field of View hoặc di chuyển tịnh tiến camera.
        *   **Target Lock (Focus)**: Tự động bám theo (Smooth Follow) một hành tinh đang di chuyển với vận tốc cao.
    *   **Kỹ thuật**: Sử dụng phép nội suy `Vector3.SmoothDamp` hoặc `Lerp` để tránh hiện tượng giật cục khi chuyển đổi mục tiêu.

2.  **Hệ thống Hiển thị Thông tin (Data Visualization)**
    *   **Mô tả**: Panel UI hiển thị thông số thời gian thực.
    *   **Dữ liệu**: Tên, Khối lượng, Vận tốc hiện tại, Khoảng cách tới sao chủ.
    *   **Kỹ thuật**:
        *   Sử dụng **ScriptableObject** để lưu trữ dữ liệu tĩnh (Tên, Mô tả, Texture).
        *   Sử dụng Raycasting để phát hiện tương tác chuột (Click chọn hành tinh).

### Nhóm 3: Tính năng Nâng cao (Simulation Sandbox)
*Mục tiêu: Mở rộng khả năng tương tác, cho phép người dùng thử nghiệm.*

1.  **Chế độ Sandbox (God Mode)**
    *   **Mô tả**: Cho phép người dùng can thiệp vào hệ thống đang chạy.
    *   **Chức năng**:
        *   **Spawn System**: Kéo thả để tạo thêm hành tinh mới.
        *   **Modify Parameters**: Chỉnh sửa khối lượng hoặc vận tốc tức thời để quan sát sự thay đổi quỹ đạo (Hiệu ứng cánh bướm).
    *   **Kỹ thuật**: Xử lý Input 3D (Mouse Position to World Point), tính toán vector vận tốc dựa trên thao tác Drag & Drop của người dùng.

---

## 4. Kiến trúc Hệ thống & Công nghệ

### 4.1. Tech Stack
*   **Engine**: Unity (Phiên bản LTS mới nhất).
*   **Ngôn ngữ**: C# (.NET Standard 2.1).
*   **VCS**: Git (Cấu trúc branch: `main`, `develop`, `feature/*`).

### 4.2. Cấu trúc Dữ liệu
Sử dụng kiến trúc hướng đối tượng (OOP) kết hợp với Component-based của Unity.
*   `CelestialBody.cs`: Component gắn trên mỗi hành tinh.
*   `GravitySimulation.cs`: Manager singleton quản lý tính toán vật lý toàn cục.
*   `SolarSystemData`: ScriptableObject chứa cấu hình khởi tạo của hệ mặt trời.

---
*Tài liệu này đóng vai trò là kim chỉ nam kỹ thuật cho toàn bộ quá trình phát triển dự án.*
