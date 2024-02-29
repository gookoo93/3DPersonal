using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.HID;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerMaster : MonoBehaviour
{
    enum playerState
    {
        Idle,
        Hide,
        Move,
        Run,
        Attack,
        Damaged,
        Die
    }

    private bool playerOnOFF = true;
        
    // 플레이어 행동에 관한 bool 변수
    [HideInInspector] public bool canMove = false;
    [HideInInspector] public bool canRun = false;        // 뛰기
    [HideInInspector] public bool canHide = false;       // 숨기
    [HideInInspector] public bool canSwim = false;     // 수영
    [HideInInspector] public bool canDown = false;      // 뛰어내리기
    [HideInInspector] public bool canWire = false;       // 와이어타고 오르기
    [HideInInspector] public bool canRope = false;      // 로프타고 건너가기
    [HideInInspector] public bool canClimb = false;     // 덩굴잡고 올라가기
    [HideInInspector] public bool canPush = false;       // 밀치기
    [HideInInspector] public bool canSteal = false;       // 훔치기
    [HideInInspector] public bool canJump = false;     // 건너뛰기 
    [HideInInspector] public bool canCarry = false;     // 오브젝트 들기 
    [HideInInspector] public bool canCuting = false;   // 오브젝트 자르기
    [HideInInspector] public bool canFallkill = false;     // 강습암살
    [HideInInspector] public bool canBreak = false;     // 오브젝트 부수기
    [HideInInspector] public bool canHealing = false;  // 회복시키기

    // 플레이어가 공격 가능한 대상에 관한 bool 변수
    [HideInInspector] protected bool attackElite = false;    // 2단계 적 공격 가능여부
    [HideInInspector] protected bool attackBoss = false;    // 3단계 적 공격 가능여부

    [HideInInspector] protected int carryingCount = 1;   // 오브젝트를 옮길 수 있는 수량

    public int maxHp = 3;                     // 최대 체력
    public int hp = 3;                            // 현재 체력
    public float moveSpeed = 5.0f;     // 움직이는 속도
    public float range = 1.0f;               // 공격사거리
    public float sound = 1.0f;              // 소음도

    float currentTime = 0.0f;   // 누적시간

    // 리지드 컴포넌트
    protected Rigidbody rigid;

    // enum 인스턴스
    playerState state;

    // 네비게이션 매쉬 에이전트
    NavMeshAgent agent;

    // 플레이어의 위치 (Transform)
    protected Transform player;
    protected GameObject selectPlayer;

    // 애니메이터 컴포넌트
    Animator anim;

    private Camera mainCamera; // 메인 카메라

    protected Vector3 targetPos; // 캐릭터의 이동 지점

    private int clickCount = 0;
    private float timeBetweenClicks = 0.2f; // 클릭 사이의 최대 허용 시간
    private Coroutine resetClickCountCoroutine;

    private bool runHide = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        mainCamera = Camera.main;
        state = playerState.Idle;
        player = GetComponent<Transform>();
        rigid = player.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnOFF)
        {
            ChangeCheck();
            LeftMouseClick();
            MoveStop();
            PlayerHiding();
        }
    }

    void LeftMouseClick()
    {
        // 마우스 입력을 받았을 때
        if (Input.GetMouseButtonDown(0))
        {
            if (resetClickCountCoroutine != null)
            {
                StopCoroutine(resetClickCountCoroutine);
            }
            resetClickCountCoroutine = StartCoroutine(ResetClickCountAfterDelay(timeBetweenClicks));

            clickCount++;

            Debug.Log("왼클릭 받음");
            // 마우스로 찍은 위치의 좌표 값을 가져온다
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast 검사를 수행하여 hit 객체 업데이트
            if (Physics.Raycast(ray, out hit, 10000f))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.point, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    // 첫 번째 클릭 로직
                    if (clickCount == 1)
                    {
                        // 유효한 경로가 있고, 목적지까지의 경로가 완전하다면 이동 실행
                        Debug.Log("유효한 위치로 이동합니다.");
                        canMove = true;
                        canRun = false;
                        state = playerState.Move;
                        Debug.Log("걷는 중");
                        agent.speed = moveSpeed;
                        agent.isStopped = false;
                        agent.SetDestination(hit.point);
                    }
                    // 두 번째 클릭 로직
                    else if (clickCount >= 2)
                    {
                        canRun = true; // 달리기 시작
                        if(canRun && canHide)
                        {
                            runHide = true;
                            canHide= false;
                            Debug.Log(canHide + "<-달리기 중 hide 상태");
                        }
                        Debug.Log("더블클릭");
                        clickCount = 0;
                        state = playerState.Run;

                        agent.speed += moveSpeed + 5.0f;
                        agent.isStopped = false;
                        agent.SetDestination(hit.point);
                    }
                }
                else
                {
                    // 경로가 유효하지 않거나 목적지까지 도달할 수 없는 경우
                    Debug.Log("이동할 수 없는 지역입니다.");
                    canMove = false; // 이동하지 않음
                }
            }
        }
    }

    IEnumerator ResetClickCountAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        clickCount = 0; // 지정된 시간이 지나면 클릭 카운트 리셋
    }

    void ChangeCheck()
    {
        if (selectPlayer != CameraController.instance.choicePlayer)
        {
            selectPlayer = CameraController.instance.choicePlayer;
        }

        for (int i = 0; i < CameraController.instance.players.Length; i++)
        {
            if (CameraController.instance.players[i] == null) continue; // players[i]가 null이면 건너뛰기

            PlayerMaster playerMaster = CameraController.instance.players[i].GetComponent<PlayerMaster>();
            if (playerMaster == null) continue; // PlayerMaster 컴포넌트가 없으면 건너뛰기

            if (CameraController.instance.players[i].name == selectPlayer.name)
            {
                playerMaster.EnablePlayer();
            }
            else
            {
                playerMaster.DisablePlayer();
            }
        }
    }

    protected virtual void EnablePlayer()
    {
        // 이 오브젝트가 선택된 플레이어일 때 활성화되어야 하는 로직
        this.enabled = true; // 현재 컴포넌트 활성화
    }

    protected virtual void DisablePlayer()
    {
        // 이 오브젝트가 선택된 플레이어가 아닐 때 비활성화되어야 하는 로직
        this.enabled = false; // 현재 컴포넌트 비활성화
    }

    public void MoveStop()
    {
        if (canMove && agent != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // 목적지에 도달했거나, 이동할 경로가 없는 경우
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // 이동을 종료하고, 상태를 Idle로 변경
                    canMove = false;
                    state = playerState.Idle;

                    if (runHide)
                    {
                        runHide = false;
                        canHide = true;
                    }
                }
            }
        }
    }

    public void PlayerHiding()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!canHide)
            {
                canHide = true;
                moveSpeed = moveSpeed / 3.0f;
                Debug.Log("player 숨은 상태");

            }
            else
            {
                canHide = false;
               moveSpeed = moveSpeed * 3.0f;
                Debug.Log("player 원래 상태");
            }
        }
    }
}