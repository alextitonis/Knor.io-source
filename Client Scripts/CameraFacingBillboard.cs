using UnityEngine;

public class CameraFacingBillboard : MonoBehaviour
{
    Camera cam;
    
    void Update()
    {
        if (!NetworkManager.getInstance.LocalPlayerExists)
            return;

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return;

        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }
}
