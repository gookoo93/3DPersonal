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
    [HideInInspector]
    public bool camMove = true;

    // 선택된 플레이어 캐릭터 저장 정보
    [HideInInspector]
    public GameObject choicePlayer;

    // 플레이어 이전 선택
    [HideInInspector]
    public GameObject beforePlayer = null;

    // 클릭카운트
    private int clickCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // 최초 카메라 시점은 플레이어1 캐릭터를 중심으로 시작
        transform.position = followTransform[0].position;
        choicePlayer = players[0];

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
        HandleZoomAndReset();
    }

    // 마우스 조작 메서드
    void HandleMouseInput()
    {
        if(Input.mouseScrollDelta.y !=0)
        {
            newZoom -= Input.mouseScrollDelta.y * zoomAmount;
        }

        if(Input.GetMouseButtonDown(1))
        {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // collider를 통해 태그 확인
                    if (hit.collider.CompareTag("Player"))
                {
                    beforePlayer = choicePlayer; // 이전에 선택된 오브젝트를 업데이트
                    // 선택 로직: 선택된 오브젝트를 기반으로 추가 작업을 수행합니다.
                    if (clickCount == 1)
                    {
                        if (beforePlayer != choicePlayer)
                        {
                            // 새로운 오브젝트가 선택되었을 때
                            choicePlayer = hit.collider.gameObject; // 현재 선택된 오브젝트를 업데이트
                            Debug.Log(choicePlayer.name + " 선택됨");

                        }
                        else
                        {
                            // 동일한 오브젝트를 다시 선택했을 때: 카메라 위치를 변경
                            // 선택된 오브젝트가 players 배열 내 어느 인덱스에 해당하는지 찾기
                            int index = System.Array.IndexOf(players, choicePlayer);
                            if (index != -1 && index < followTransform.Length)
                            {
                                newPosition = followTransform[index].transform.position; ;
                                Debug.Log(choicePlayer.name + " 위치로 카메라 이동 처리");
                            }
                            clickCount = 0; // 클릭 카운트를 초기화하여 다음 선택 준비
                        }
                    }
                    else
                    {
                        // 첫 번째 클릭이 아닌 경우(즉, 초기 상태에서 오브젝트를 선택했을 때)
                        clickCount = 1;
                        choicePlayer = hit.collider.gameObject;

                        beforePlayer = choicePlayer; // beforePlayer와 choicePlayer를 현재 선택된 오브젝트로 설정
                        Debug.Log(choicePlayer.name + " 선택됨");
                    }
                }

                   camMove = true;
                   // 카메라 이동 로직
                   Plane plane = new Plane(Vector3.up, Vector3.zero);
                   float entry;
                   if (plane.Raycast(ray, out entry))
                   {
                       dragStartPosition = ray.GetPoint(entry);
                   }
                }
        }

        if (Input.GetMouseButton(1) && camMove)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // "Ground"와 "Default" 레이어에만 반응하도록 레이어 마스크 설정
            int layerMask = ~(LayerMask.GetMask("Player", "Enemy" , "Object", "DeadBody"));

            float entry;
             
            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                // 레이어 마스크를 적용하여 레이캐스트
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    dragCurrentPosition = ray.GetPoint(entry); // 실제 레이캐스트 충돌 지점을 사용
                }

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                // newPosition을 사용하여 오브젝트 위치 업데이트 등의 추가 작업을 수행
            }
        }

        // leftAlt 키를 누르고 있을 때
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            camMove = false; // 카메라 이동을 방지

            if (rotateStartPosition == Vector3.zero)
            {
                rotateStartPosition = Input.mousePosition; // 회전 시작 위치 초기화
            }
            else
            {
                rotateCurrentPosition = Input.mousePosition; // 현재 마우스 위치 업데이트
                Vector3 difference = rotateStartPosition - rotateCurrentPosition; // 이동한 거리 계산
                rotateStartPosition = rotateCurrentPosition; // 시작 위치를 현재 위치로 업데이트

                newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f)); // 회전 적용
            }
        }
        else
        {
            camMove = true; // leftAlt 키가 떼어지면 카메라 이동 허용
            rotateStartPosition = Vector3.zero; // 회전 시작 위치 초기화
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
            //newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
            RotateCamera(90);
        }
        if (Input.GetKey(KeyCode.E))
        {
            //newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
            RotateCamera(-90);
        }
         
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTranform.localPosition = Vector3.Lerp(cameraTranform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    // 숫자버튼으로 플레이어를 중심으로 이동
    void HandleKeyInput()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // KeyCode.Alpha1은 1번 키, i를 더해 숫자키 1부터 순서대로 매칭
            {
                if (followTransform[i] != null)
                {
                    if (choicePlayer != players[i])
                    {
                        // 새로운 플레이어를 선택한 경우
                        beforePlayer = choicePlayer; // 이전 선택된 플레이어 업데이트
                        choicePlayer = players[i]; // 새로운 플레이어 선택
                        clickCount = 1; // 클릭 카운트를 1로 설정
                        Debug.Log(players[i].name + " 선택됨");
                    }
                    else if (clickCount == 1)
                    {
                        // 같은 플레이어를 다시 선택한 경우(카메라 이동)
                        newPosition = followTransform[i].position; // 카메라의 새 위치를 선택된 플레이어의 위치로 설정
                        Debug.Log(players[i].name + " 위치로 카메라 이동 처리");
                        clickCount = 0; // 클릭 카운트 초기화
                    }
                    break; // 해당 키에 대한 처리를 마치면 for 루프 종료
                }
            }
        }
    }

    void RotateCamera(float angle)
    {
        float currentAngle = transform.eulerAngles.y;
        float adjustedAngle = 0;

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            // Left Alt 사용 시 자유 회전
            adjustedAngle = currentAngle + angle;
        }
        else
        {
            // Q 또는 E 키 사용 시 90도 단위로 정렬
            if (angle > 0) // Q를 눌렀을 경우, 상위 90도 각도로 정렬
            {
                adjustedAngle = Mathf.Ceil((currentAngle + 1) / 90) * 90;
            }
            else if (angle < 0) // E를 눌렀을 경우, 하위 90도 각도로 정렬
            {
                adjustedAngle = Mathf.Floor((currentAngle - 1) / 90) * 90;
            }
        }

        // 360도 초과 시 순환 처리
        adjustedAngle = adjustedAngle % 360;
        if (adjustedAngle < 0) adjustedAngle += 360;

        newRotation = Quaternion.Euler(0, adjustedAngle, 0);
    }

    void HandleZoomAndReset()
    {
        // 줌 제한 적용
        newZoom.y = Mathf.Clamp(newZoom.y, 100, 500);
        newZoom.z = Mathf.Clamp(newZoom.z, -500, -100); // 줌 값은 일반적으로 음수입니다.

        // W 키를 누르면 기본 줌값과 회전값으로 리셋
        if (Input.GetKeyDown(KeyCode.W))
        {
            newZoom = new Vector3(0, 200, -200); // 기본 줌값으로 설정
            newRotation = Quaternion.Euler(0, 45, 0); // 기본 회전값으로 설정
        }
    }
}
