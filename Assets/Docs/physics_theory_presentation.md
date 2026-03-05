# Báo Cáo Lý Thuyết Vật Lý & Toán Học Trong Mô Phỏng

Dự án **Solar System Simulator** sử dụng các nguyên lý vật lý và toán học mô phỏng số (numerical simulation) để tái hiện quỹ đạo và tương tác hấp dẫn giữa các thiên thể. Dưới đây là toàn bộ nền tảng lý thuyết được áp dụng.

---

## 1. Định Luật Vạn Vật Hấp Dẫn Của Newton

Trái tim của hệ thống là **Định luật vạn vật hấp dẫn của Newton**. Bất kỳ hai hạt hoặc vật thể nào có khối lượng trong vũ trụ đều hút nhau bằng một lực tỷ lệ thuận với tích các khối lượng của chúng và tỷ lệ nghịch với bình phương khoảng cách giữa chúng.

**Công thức gốc:**
$$ F_{ij} = G \cdot \frac{m_i \cdot m_j}{r^2} $$

Trong đó:
- $F_{ij}$: Độ lớn lực hấp dẫn giữa vật $i$ và vật $j$.
- $G$: Hằng số hấp dẫn (Gravitational Constant). Trong hệ quy chiếu **AU / Solar Mass / Day**, $G \approx 2.9592 \times 10^{-4}$.
- $m_i, m_j$: Khối lượng của hai vật (tính bằng khối lượng Mặt Trời $M_\odot$).
- $r$: Khoảng cách giữa tâm hai vật (tính bằng AU).

**Trong code (File `GravitySimulation.cs`):**
Thay vì tính Lực ($F$), thuật toán tính trực tiếp **Gia tốc ($a$)** dựa theo Định luật II Newton ($F = m.a \Rightarrow a = F/m$):

$$ a_i = \sum_{j \neq i} G \cdot m_j \cdot \frac{\vec{r_{ji}}}{|\vec{r_{ji}}|^3 + \epsilon} $$

*Giải thích công thức code:*
- Phân số có mẫu là $r^3$ thay vì $r^2$ vì tử số đã được nhân với vector $\vec{r}$ (khoảng cách có hướng) thay vì vector đơn vị $\hat{r}$. ($r^2 \cdot \frac{\vec{r}}{r} = \frac{\vec{r}}{r^3}$).
- Hằng số $\epsilon$ (Softening Factor) được cộng thêm vào mẫu số để tránh hiện tượng **chia cho 0** (Singularity) nếu hai vật thể va chạm vào đúng tâm của nhau, khiến lực hấp dẫn vọt lên vô cực gây hỏng mô phỏng.

---

## 2. Tích Phân Số (Numerical Integration): Thuật toán Velocity Verlet

Để vẽ ra đường bay (quỹ đạo) xoay vòng quanh Mặt Trời của các hành tinh, chúng ta cần tìm **Vị trí** và **Vận tốc** của vật dựa trên phần Gia tốc đã tính ở trên qua từng khung hình $\Delta t$ (timestep).

Hệ thống dùng thuật toán **Semi-Implicit Euler** (hay còn gọi tắt trong thiết kế là Symplectic Euler/Velocity Verlet variant).

**Kịch bản 3 bước (Symplectic Euler):**
1. **Tìm Vận Tốc mới:** Cập nhật vận tốc dựa trên gia tốc của khung hình trước đó.
   $$ \vec{v}_{new} = \vec{v}_{old} + \vec{a} \cdot \Delta t $$
2. **Cập nhật Vị trí mới:** Cập nhật vị trí dựa trên *Vận tốc mới* vừa tính.
   $$ \vec{x}_{new} = \vec{x}_{old} + \vec{v}_{new} \cdot \Delta t $$
3. **Tính toán lực hấp dẫn cho khung hình tiếp theo:** Dùng $\vec{x}_{new}$ để tính ra mảng Gia tốc mới $\vec{a}_{new}$ áp dụng cho vòng lặp sau.

**🌟 Tại sao không dùng thuật toán Euler cơ bản?**
Euler cơ bản (Explicit Euler) tính Vị trí bằng Vận tốc cũ ($\vec{x}_{new} = \vec{x}_{old} + \vec{v}_{old} \cdot \Delta t$). Thuật toán này không bảo toàn năng lượng cơ học của chu kỳ kín. Nếu dùng tích phân này, quỹ đạo hình Elip của hành tinh theo thời gian sẽ bị xoắn ốc và trôi dạt bay mất vào không gian (Energy Drift). Trái lại, Symplectic/N-body Verlet bảo toàn bảo nguyên cấu trúc không gian hình học lâu dài.

---

## 3. Độ Chính Xác Kép (Double Precision) - Giải Quyết Sai Số Dấu Phẩy Động

Khoảng cách trong vũ trụ là khổng lồ (Ví dụ Diêm Vương Tinh cách Mặt Trời $\approx 40$ AU), nhưng sự chênh lệch nhỏ về khoảng cách khi tính gia tốc cũng quyết định sinh tử của quỹ đạo.
Trong khoa học máy tính tiêu chuẩn (và engine game Unity), biến `float` (Single Precision - 32 bit) chỉ mang độ chính xác $\approx 7$ chữ số thập phân.
- Nếu tọa độ của Mặt Trời là $0$ và Trái Đất là $1$, thì khác biệt $0.000000001$ AU sẽ bị máy tính cắt cụt, làm mất đi một lượng cực lớn lực hấp dẫn, khiến quỹ đạo rối loạn.

**Giải pháp:**
Hệ thống sử dụng tự định nghĩa cấu trúc không gian `DoubleVector3` dựa trên biến kiểu `double` (Double Precision - 64 bit), đem lại độ chính xác lên tới $15-17$ chữ số thập phân. Toàn bộ Lõi Tính Toán (Core Math) ngầm chạy bằng kiểu `double`, và chỉ Convert sang `float` khi đem dán lên màn hình vẽ `Transform` ngoài cùng.

---

## 4. Tách Biệt Đồ Hoạ và Tính Toán (Visual vs Physics Scaling)

Nếu vẽ Trái đất cách Mặt Trời đúng tỷ lệ thật (1 AU $\approx 150.000.000$ km) mà kích thước Trái Đất chỉ là $6.400$ km, thì Trái Đất trên màn hình máy tính chỉ là một hạt bụi không thể nhìn thấy, màn hình sẽ chỉ có một màu đen đặc.

Hệ thống áp dụng **Quy luật Tỷ Lệ Tuyến Toán**:
- **Lõi Vật lý:** Chạy khoảng cách THẬT theo đơn vị AU = 1.0 (Trái Đất tới Sun). Lực hấp dẫn được tính chính xác bằng $F=G\frac{m_1 m_2}{1.0^2}$.
- **Hệ thống Vẽ Hình:** Lấy kết quả vị trí vật lý, nhân (hoặc chia) với một `Visual Scale Factor`. (VD: $0.05$). Quỹ đạo vẫn là quỹ đạo chuẩn xác, nhưng màn hình hiển thị đã được dùng "kính hiển vi" nén vào gần nhau để cho đẹp mắt.

## Tổng Kết

Dự án áp dụng chặt chẽ:
1. Định luật vạn vật hấp dẫn Newton ($F=G \cdot \frac{Mm}{r^2}$)
2. Tích phân số học bảo toàn năng lượng (Symplectic Euler / Verlet)
3. Float-point 64-bit Engineering (Hạn chế sai số máy tính)
