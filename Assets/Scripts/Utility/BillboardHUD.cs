using UnityEngine;
public class BillboardHUD : MonoBehaviour
{
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if(mainCamera != null)
        {
            //allinea la rotazione della HUD a quella della telecamera
            transform.forward = mainCamera.transform.forward;
        }
    }
}