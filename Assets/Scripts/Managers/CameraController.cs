using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 20f;
    public float zoomSpeed = 500f;
    public float rotationSpeed = 3f;

    [Header("Boundaries")]
    public Vector2 panLimitX = new Vector2(-50f, 50f); 
    public Vector2 panLimitZ = new Vector2(-50f, 50f);
    public float minZoomY = 5f;
    public float maxZoomY = 40f;

    [Header("Double Click Settings")]
    public float doubleClickTime = 0.3f;
    private float lastClickTime;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private float currentPitch; //variabile per evitare ribaltamenti camera

    private void Start()
    {
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
        currentPitch = transform.eulerAngles.x;
    }

    private void Update()
    {
        HandleDoubleClickReset();
        HandlePan();
        HandleZoom();
        HandleRotation();
        ClampPosition();
    }

    private void HandlePan()
    {
        //2 = rotellina centrale mouse
        if(Input.GetMouseButton(2)) 
        {
            float mouseX = -Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");

            //il calcolo del movimento si basa su dove sta guardando la telecamera, ignorando l'asse Y
            Vector3 move = new Vector3(mouseX, 0, mouseY) * panSpeed * Time.deltaTime;
            move = transform.TransformDirection(move);
            move.y = 0;
            
            transform.Translate(move, Space.World);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(Mathf.Abs(scroll) > 0.01f)
        {
            //zoom in avanti
            Vector3 move = transform.forward * scroll * zoomSpeed * Time.deltaTime;
            transform.position += move;
        }
    }

    private void HandleRotation()
    {
        //1= tasto destro mouse
        if(Input.GetMouseButton(1)) 
        {
            float rotateHorizontal = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotateVertical = -Input.GetAxis("Mouse Y") * rotationSpeed;

            transform.Rotate(Vector3.up, rotateHorizontal, Space.World);
            currentPitch += rotateVertical;
            currentPitch = Mathf.Clamp(currentPitch, 10f, 85f);
            
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.x = currentPitch;
            transform.eulerAngles = currentEuler;
        }
    }

    private void HandleDoubleClickReset()
    {
        //0= tasto sinistro mouse
        if(Input.GetMouseButtonDown(0))
        {
            if(UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                
            if(Time.time - lastClickTime < doubleClickTime)
            {
                ResetCamera();
            }
            lastClickTime = Time.time;
        }
    }

    private void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        //blocca la posizione per rimanere entro i confini
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, panLimitX.x, panLimitX.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, panLimitZ.x, panLimitZ.y);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minZoomY, maxZoomY);
        transform.position = clampedPosition;
    }

    private void ResetCamera()
    {
        transform.position = defaultPosition;
        transform.rotation = defaultRotation;
        currentPitch = defaultRotation.eulerAngles.x;
    }
}