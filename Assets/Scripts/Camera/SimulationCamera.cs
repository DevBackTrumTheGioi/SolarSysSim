using UnityEngine;

/// <summary>
/// Camera controller cho Solar System Simulation.
/// 
/// === CHỨC NĂNG ===
/// - Scroll để zoom in/out
/// - Click vào hành tinh để focus
/// - Nhấn phím số 1-9 để chọn hành tinh (1=Sun, 2=Mercury, ..., 9=Neptune)
/// - Chuột phải + kéo để xoay camera
/// - Space để reset về nhìn toàn cảnh
/// </summary>
public class SimulationCamera : MonoBehaviour
{
    [Header("=== TARGET ===")]
    [Tooltip("Thiên thể đang focus. Null = nhìn tổng quan.")]
    public Transform target;
    
    [Tooltip("Kéo SimulationSettings vào đây để tự chỉnh tốc độ khi focus.")]
    public SimulationSettings settings;

    [Header("=== ZOOM ===")]
    public float zoomSpeed = 5f;
    public float minDistance = 0.1f;
    public float maxDistance = 200f;
    public float currentDistance = 20f;
    public float targetDistance = 20f; // Dùng để lerp zoom mượt mà

    [Header("=== MOVEMENT ===")]
    public float moveSpeed = 30f;

    [Header("=== ROTATION ===")]
    public float rotationSpeed = 3f;
    private float rotationX = 30f;  // Bắt đầu nghiêng 30° để nhìn từ trên xuống
    private float rotationY = 0f;

    [Header("=== SMOOTH ===")]
    public float smoothSpeed = 8f;

    private Vector3 targetPosition;
    private CelestialBody[] allBodies;

    void Start()
    {
        allBodies = FindObjectsOfType<CelestialBody>();
        targetPosition = Vector3.zero;

        // Vị trí ban đầu: nhìn từ trên xuống, zoom vừa đủ thấy toàn bộ hệ
        currentDistance = 20f;
        targetDistance = 20f;
        rotationX = 60f;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.nearClipPlane = 0.0001f;
        }
    }

    void LateUpdate()
    {
        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        // === ZOOM: Mouse Scroll ===
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            // Logarithmic zoom - feels more natural at different scales
            targetDistance *= (1f - scroll * zoomSpeed);
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        
        // Cập nhật currentDistance mượt mà
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * smoothSpeed);

        // === WASD / ARROWS MOVEMENT ===
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        if (h != 0 || v != 0)
        {
            // Bỏ target để di chuyển tự do
            target = null;
            
            // Tính hướng chạy ngang (phẳng) trên the XZ plane theo camera view
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0; right.y = 0;
            if (forward.sqrMagnitude > 0) forward.Normalize();
            if (right.sqrMagnitude > 0) right.Normalize();
            
            targetPosition += (forward * v + right * h) * moveSpeed * Time.deltaTime;
        }

        // === ROTATE: Right Mouse Button + Drag ===
        if (Input.GetMouseButton(1))
        {
            rotationY += Input.GetAxis("Mouse X") * rotationSpeed;
            rotationX -= Input.GetAxis("Mouse Y") * rotationSpeed;
            rotationX = Mathf.Clamp(rotationX, -89f, 89f);
        }

        // === SELECT PLANET: Number Keys ===
        // 1=Sun, 2=Mercury, 3=Venus, 4=Earth, 5=Mars, 
        // 6=Jupiter, 7=Saturn, 8=Uranus, 9=Neptune
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            {
                SelectBody(i);
                break;
            }
        }

        // === RESET: Space ===
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (allBodies != null)
            {
                foreach (var body in allBodies)
                {
                    if (body.bodyName == "Sun")
                    {
                        target = body.transform;
                        break;
                    }
                }
            }
            if (settings != null) settings.timeScale = 40f;
            targetDistance = 60f;
            rotationX = 60f;
            rotationY = 0f;
        }

        // === CLICK TO SELECT ===
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                CelestialBody body = hit.collider.GetComponent<CelestialBody>();
                if (body != null)
                {
                    FocusOnBody(body);
                }
            }
        }
    }

    void SelectBody(int index)
    {
        if (allBodies == null) return;

        // Tìm body theo tên (vì FindObjectsOfType không đảm bảo thứ tự)
        string[] names = { "Sun", "Mercury", "Venus", "Earth", "Mars", 
                          "Jupiter", "Saturn", "Uranus", "Neptune" };
        
        if (index >= names.Length) return;

        foreach (var body in allBodies)
        {
            if (body.bodyName == names[index])
            {
                FocusOnBody(body);
                return;
            }
        }
    }

    void FocusOnBody(CelestialBody body)
    {
        target = body.transform;
        if (settings != null) settings.timeScale = 0.05f; // Giảm xuống 0.05 days/sec
        
        // Auto-zoom lại gần
        float baseScale = body.baseVisualScale; 
        
        // Tùy mặt trời hoặc hành tinh mà góc nhìn khác nhau
        float zoomMultiplier = (body.bodyName == "Sun") ? 4f : 3f;
        
        // Cân nhắc theo tỷ lệ hệ thống (Nếu ở Realistic Mode 0.01x thì cam phải zoom sát rạt)
        // Lưu ý: Mặt Trời không bị ảnh hưởng bởi visualScaleMultiplier, nên bỏ qua systemScale nếu đang focus Sun.
        float systemScale = (settings != null && body.bodyName != "Sun") ? settings.visualScaleMultiplier : 1f;
        
        targetDistance = baseScale * zoomMultiplier * systemScale;
        
        // Mở biên độ minDistance nhỏ hơn nữa để Realistic Mode có thể chúi sát đất cho Mặt Trăng
        targetDistance = Mathf.Clamp(targetDistance, 0.001f, maxDistance);
    }

    void UpdateCameraPosition()
    {
        // Target position: follow selected body or origin
        if (target != null)
        {
            targetPosition = Vector3.Lerp(targetPosition, target.position, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Khi target == null, targetPosition do WASD điều khiển trực tiếp
        }

        // Tính vị trí camera từ rotation và distance
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);

        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition);
    }
}

