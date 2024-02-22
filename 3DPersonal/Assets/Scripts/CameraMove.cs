using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform[] playerObjects; // 플레이어 오브젝트 배열
    private Transform mainPlayer; // 메인 플레이어

    private Vector3 originalPosition; // 카메라의 초기 위치
    private Quaternion originalRotation; // 카메라의 초기 회전값

    private float originalZoom; // 카메라의 초기 줌 값
    private float zoomLevel = 10f; // 현재 줌 레벨

    [SerializeField]
    private float moveSpeed = 10f; // 이동 속도를 증가시킴
    [SerializeField]
    private float rotationSpeed = 90f; // 카메라 회전 속도
    [SerializeField]
    private int rotateMouse = 5; // 마우스 이동값

    void Start()
    {
        mainPlayer = playerObjects[0]; // 첫 번째 오브젝트를 메인 플레이어로 설정
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalZoom = Camera.main.fieldOfView;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, 90);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, -90);
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetAxis("Mouse X") < 0)
        {
            RotateCamera(rotateMouse * - 1); // 회전 각도를 직접 지정
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetAxis("Mouse X") > 0)
        {
            RotateCamera(rotateMouse); // 회전 각도를 직접 지정

        }

        if (Input.GetMouseButton(2)) // 마우스 휠 버튼
        {
            MoveCamera();
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            ZoomCamera(Input.mouseScrollDelta.y);
        }

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ChangeMainPlayer(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ResetCamera();
        }
    }

    // 마우스로 이동하는 카메라 rotate
    void RotateCamera(int direction)
    {
        transform.RotateAround(Vector3.zero, Vector3.up, rotationSpeed * direction * Time.deltaTime);
    }

    void MoveCamera()
    {
        var moveX = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
        var moveY = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
        transform.Translate(new Vector3(moveX, 0, moveY), Space.World);
    }

    void ZoomCamera(float increment)
    {
        zoomLevel -= increment;
        Camera.main.fieldOfView = Mathf.Clamp(zoomLevel, 5f, originalZoom);
    }

    void ChangeMainPlayer(int index)
    {
        if (index >= 0 && index < playerObjects.Length)
        {
            mainPlayer = playerObjects[index];
            transform.position = mainPlayer.position - transform.forward * 10f; // 카메라를 메인 플레이어 뒤로 이동
        }
    }

    void ResetCamera()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        Camera.main.fieldOfView = originalZoom;
    }
}


// 카메라 회전(Q/E , LAlt)
// 카메라 확대(휠돌리기)
// 카메라 이동(조작키 , 맵 양옆으로 이동 , 휠 버튼)
// 카메라 초기위치 저장
// 카메라 초기위치 불러오기(W)

// 캐릭터 위치로 카메라 이동, 