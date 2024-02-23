using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 플레이어 및 기본 카메라의 중심점을 받아올 인스턴스
    public static CameraController instance;

    // 플레이어를 추적할 트랜스폼
    public Transform[] followTransform;

    // 기본 카메라 트랜스폼
    public Transform cameraTranform;

    // 플레이어 오브젝트들
    public GameObject[] players;

    // 카메라 이동속도
    public float movementSpeed;

    // 카메라가 이동하는 시간 계산
    public float movementTime;

    // 카메라 회전 수치
    public float rotationAmount;

    // 카메라 줌 수치
    public Vector3 zoomAmount;

    // 변경되는 위치값 저장 변수
    public Vector3 newPosition;

    // 변경되는 회전값 저장 변수
    public Quaternion newRotation;

    // 변경되는 확대값 저장 변수
    public Vector3 newZoom;

    // 마우스 드래그 시작 지점 (마우스로 뷰 이동용)
    public Vector3 dragStartPosition;

    // 마우스 드래그 종료 지점 (마우스로 뷰 이동용)
    public Vector3 dragCurrentPosition;

    // 회전 시작 시점 (회전계산용)
    public Vector3 rotateStartPosition;

    // 회전 종료 시점 (회전계산용)
    public Vector3 rotateCurrentPosition;

    // 회전과 맵 이동 동시 입력 방지용 bool 변수
    private bool camMove = true;

    // 선택된 플레이어 캐릭터 저장 정보
    public GameObject choicePlayer;

    // Start is called before the first frame update
    void Start()
    {
        // 카메라의 인스턴스
        instance = this;    

        // 최초 카메라 시점은 플레이어1 캐릭터를 중심으로 시작
        transform.position = followTransform[0].position;

        // 카메라의 위치, 회전값, 줌값을 지정
        newPosition = transform.position;
        newRotation= transform.rotation;
        newZoom = cameraTranform.localPosition;

    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        HandleMovementInput();
        HandleKeyInput();
    }

    // 마우스 조작 메서드
    void HandleMouseInput()
    {
        if(Input.mouseScrollDelta.y !=0)
        {
            newZoom -= Input.mouseScrollDelta.y * zoomAmount;
        }

        if(Input.GetMouseButtonDown(1) && camMove)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if(plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(1) && camMove)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }

        if(Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            camMove= false;
            rotateStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            camMove= false;
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }

        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Debug.Log("캠무브 트루");
            camMove = true;
        }
    }

    // 카메라 조작 메서드
    void HandleMovementInput()
    {
        // 카메라 이동
        if(Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
        }
        
        // 카메라 회전
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
         
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTranform.localPosition = Vector3.Lerp(cameraTranform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    // 숫자버튼으로 플레이어를 중심으로 이동
    void HandleKeyInput()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            if (followTransform[0] != null)
            {
                choicePlayer = players[0];
                newPosition = followTransform[0].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (followTransform[1] != null)
            {
                choicePlayer = players[1];
                newPosition= followTransform[1].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            if (followTransform[2] != null)
            {
                choicePlayer = players[2];
                newPosition = followTransform[2].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            if (followTransform[3] != null)
            {
                choicePlayer= players[3];
                newPosition = followTransform[3].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            if (followTransform[4] != null)
            {
                choicePlayer= players[4];
                newPosition = followTransform[4].position;
            }
        }
    }
}
