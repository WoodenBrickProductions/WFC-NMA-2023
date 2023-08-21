using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public static Vector3 cameraDirection;
    public static float cameraAngleRad;
    public static Camera mainCamera;
    [SerializeField] private bool cameraScroll = true;
    [SerializeField] private float slowDragDeadzone;
    [SerializeField] private float fastDragDeadzone = 10;
    [SerializeField] private float CameraSpeed = 100;
    [SerializeField] private Transform cameraTM;
    public Transform TRBound;
    public Transform BLBound;

    void Update()
    {
        MoveCamera();
    }
    private void MoveCamera()
    {
        int horizontal = ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) || (cameraScroll && Input.mousePosition.x < fastDragDeadzone) ? -1 : 0) +
                         ((Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) || (cameraScroll && Input.mousePosition.x > Screen.width - fastDragDeadzone) ? 1 : 0);

        int vertical = ((Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) || (cameraScroll && Input.mousePosition.y > Screen.height - fastDragDeadzone) ? 1 : 0) +
                       ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) || (cameraScroll && Input.mousePosition.y < fastDragDeadzone) ? -1 : 0);

        float x = horizontal * CameraSpeed * Time.deltaTime;
        var nX = cameraTM.position.x + x;
        if (nX > TRBound.position.x ||
            nX < BLBound.position.x)
            x = 0;

        float z = vertical * CameraSpeed * Time.deltaTime;
        var nZ = cameraTM.position.z + z;
        if (nZ > TRBound.position.z ||
            nZ < BLBound.position.z)
            z = 0;

        cameraTM.position += new Vector3(x, 0, z);
    }
}
