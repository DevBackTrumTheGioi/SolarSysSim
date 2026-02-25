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

    [Header("=== ZOOM ===")]
    public float zoomSpeed = 5f;
    public float minDistance = 0.1f;
    public float maxDistance = 200f;
    public float currentDistance = 20f;

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
        rotationX = 60f;
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
            currentDistance *= (1f - scroll * zoomSpeed);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
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
            target = null;
            currentDistance = 20f;
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
                    target = body.transform;
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
                target = body.transform;
                return;
            }
        }
    }

    void UpdateCameraPosition()
    {
        // Target position: follow selected body or origin
        Vector3 desiredTarget = target != null ? target.position : Vector3.zero;
        targetPosition = Vector3.Lerp(targetPosition, desiredTarget, Time.deltaTime * smoothSpeed);

        // Tính vị trí camera từ rotation và distance
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);

        transform.position = targetPosition + offset;
        transform.LookAt(targetPosition);
    }
}

