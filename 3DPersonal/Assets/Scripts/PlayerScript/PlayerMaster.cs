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
        
    // �÷��̾� �ൿ�� ���� bool ����
    [HideInInspector] public bool canMove = false;
    [HideInInspector] public bool canRun = false;        // �ٱ�
    [HideInInspector] public bool canHide = false;       // ����
    [HideInInspector] public bool canSwim = false;     // ����
    [HideInInspector] public bool canDown = false;      // �پ����
    [HideInInspector] public bool canWire = false;       // ���̾�Ÿ�� ������
    [HideInInspector] public bool canRope = false;      // ����Ÿ�� �ǳʰ���
    [HideInInspector] public bool canClimb = false;     // ������� �ö󰡱�
    [HideInInspector] public bool canPush = false;       // ��ġ��
    [HideInInspector] public bool canSteal = false;       // ��ġ��
    [HideInInspector] public bool canJump = false;     // �ǳʶٱ� 
    [HideInInspector] public bool canCarry = false;     // ������Ʈ ��� 
    [HideInInspector] public bool canCuting = false;   // ������Ʈ �ڸ���
    [HideInInspector] public bool canFallkill = false;     // �����ϻ�
    [HideInInspector] public bool canBreak = false;     // ������Ʈ �μ���
    [HideInInspector] public bool canHealing = false;  // ȸ����Ű��

    // �÷��̾ ���� ������ ��� ���� bool ����
    [HideInInspector] protected bool attackElite = false;    // 2�ܰ� �� ���� ���ɿ���
    [HideInInspector] protected bool attackBoss = false;    // 3�ܰ� �� ���� ���ɿ���

    [HideInInspector] protected int carryingCount = 1;   // ������Ʈ�� �ű� �� �ִ� ����

    public int maxHp = 3;                     // �ִ� ü��
    public int hp = 3;                            // ���� ü��
    public float moveSpeed = 5.0f;     // �����̴� �ӵ�
    public float range = 1.0f;               // ���ݻ�Ÿ�
    public float sound = 1.0f;              // ������

    float currentTime = 0.0f;   // �����ð�

    // ������ ������Ʈ
    protected Rigidbody rigid;

    // enum �ν��Ͻ�
    playerState state;

    // �׺���̼� �Ž� ������Ʈ
    NavMeshAgent agent;

    // �÷��̾��� ��ġ (Transform)
    protected Transform player;
    protected GameObject selectPlayer;

    // �ִϸ����� ������Ʈ
    Animator anim;

    private Camera mainCamera; // ���� ī�޶�

    protected Vector3 targetPos; // ĳ������ �̵� ����

    private int clickCount = 0;
    private float timeBetweenClicks = 0.2f; // Ŭ�� ������ �ִ� ��� �ð�
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
        // ���콺 �Է��� �޾��� ��
        if (Input.GetMouseButtonDown(0))
        {
            if (resetClickCountCoroutine != null)
            {
                StopCoroutine(resetClickCountCoroutine);
            }
            resetClickCountCoroutine = StartCoroutine(ResetClickCountAfterDelay(timeBetweenClicks));

            clickCount++;

            Debug.Log("��Ŭ�� ����");
            // ���콺�� ���� ��ġ�� ��ǥ ���� �����´�
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast �˻縦 �����Ͽ� hit ��ü ������Ʈ
            if (Physics.Raycast(ray, out hit, 10000f))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.point, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    // ù ��° Ŭ�� ����
                    if (clickCount == 1)
                    {
                        // ��ȿ�� ��ΰ� �ְ�, ������������ ��ΰ� �����ϴٸ� �̵� ����
                        Debug.Log("��ȿ�� ��ġ�� �̵��մϴ�.");
                        canMove = true;
                        canRun = false;
                        state = playerState.Move;
                        Debug.Log("�ȴ� ��");
                        agent.speed = moveSpeed;
                        agent.isStopped = false;
                        agent.SetDestination(hit.point);
                    }
                    // �� ��° Ŭ�� ����
                    else if (clickCount >= 2)
                    {
                        canRun = true; // �޸��� ����
                        if(canRun && canHide)
                        {
                            runHide = true;
                            canHide= false;
                            Debug.Log(canHide + "<-�޸��� �� hide ����");
                        }
                        Debug.Log("����Ŭ��");
                        clickCount = 0;
                        state = playerState.Run;

                        agent.speed += moveSpeed + 5.0f;
                        agent.isStopped = false;
                        agent.SetDestination(hit.point);
                    }
                }
                else
                {
                    // ��ΰ� ��ȿ���� �ʰų� ���������� ������ �� ���� ���
                    Debug.Log("�̵��� �� ���� �����Դϴ�.");
                    canMove = false; // �̵����� ����
                }
            }
        }
    }

    IEnumerator ResetClickCountAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        clickCount = 0; // ������ �ð��� ������ Ŭ�� ī��Ʈ ����
    }

    void ChangeCheck()
    {
        if (selectPlayer != CameraController.instance.choicePlayer)
        {
            selectPlayer = CameraController.instance.choicePlayer;
        }

        for (int i = 0; i < CameraController.instance.players.Length; i++)
        {
            if (CameraController.instance.players[i] == null) continue; // players[i]�� null�̸� �ǳʶٱ�

            PlayerMaster playerMaster = CameraController.instance.players[i].GetComponent<PlayerMaster>();
            if (playerMaster == null) continue; // PlayerMaster ������Ʈ�� ������ �ǳʶٱ�

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
        // �� ������Ʈ�� ���õ� �÷��̾��� �� Ȱ��ȭ�Ǿ�� �ϴ� ����
        this.enabled = true; // ���� ������Ʈ Ȱ��ȭ
    }

    protected virtual void DisablePlayer()
    {
        // �� ������Ʈ�� ���õ� �÷��̾ �ƴ� �� ��Ȱ��ȭ�Ǿ�� �ϴ� ����
        this.enabled = false; // ���� ������Ʈ ��Ȱ��ȭ
    }

    public void MoveStop()
    {
        if (canMove && agent != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // �������� �����߰ų�, �̵��� ��ΰ� ���� ���
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    // �̵��� �����ϰ�, ���¸� Idle�� ����
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
                Debug.Log("player ���� ����");

            }
            else
            {
                canHide = false;
               moveSpeed = moveSpeed * 3.0f;
                Debug.Log("player ���� ����");
            }
        }
    }
}