using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMaster : MonoBehaviour
{
    enum playerState
    {
        Idle,
        Walk,
        Hide,
        HideWalk,
        Run,
        Attack,
        Damaged,
        Die
    }

    public bool controlAble = true;
    private bool playerOnOFF = true;

    public PlayerStat ps;

    public EnemyMaster EM = null;

    float currentTime = 0.0f;   // 누적시간

    // 리지드 컴포넌트
    protected Rigidbody rigid;

    // enum 인스턴스
    playerState state;

    // 네비게이션 매쉬 에이전트
    protected NavMeshAgent agent;

    // 플레이어의 위치 (Transform)
    protected Transform player;
    protected GameObject selectPlayer;

    // 공격 타겟
    public GameObject target = null;
    // 애니메이터 컴포넌트
    protected Animator anim;

    protected Camera mainCamera; // 메인 카메라

    protected Vector3 targetPos; // 캐릭터의 이동 지점

    protected int clickCount = 0;
    protected float timeBetweenClicks = 0.2f; // 클릭 사이의 최대 허용 시간
    protected Coroutine resetClickCountCoroutine;

    private bool runHide = false;

    public float rotateVelocity;//회전속도
    public float rotateSpeedForAttack;//공격회전속도

    // 발도 납도 카운트
    int count = -1;

    public EnemyMaster enemy;

    public GameObject enemyObject = null;
    public Animator enemyAnim = null;

    #region skill

    //GameObject skillPlayer;

    //[Tooltip("0 : Human / 1 : Dwarf / 2 : Elf")]
    //[SerializeField]
    //public int skill;
    //public bool skillDelay = false;

    //public GameObject stonePrefab; // 돌멩이 Prefab
    //public Transform throwPoint; // 던지는 지점
    //public LineRenderer trajectoryLine; // 궤적을 그리는 LineRenderer
    //public float throwForce = 40f; // 던지는 힘
    //public int trajectoryResolution = 30; // 궤적의 해상도

    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        mainCamera = Camera.main;
        state = playerState.Idle;
        player = GetComponent<Transform>();
        rigid = player.GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ps = GetComponent<PlayerStat>();

    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnOFF)
        {
            ChangeCheck();
            PlayerStateChange();
            LeftMouseClick();
            PlayerHiding();
            MoveStop();
            AttackSystem();
            AttackDelay();
            CarryDeadBody();
            //SkillShot();
        }
    }

    // 선택한 플레이어만 움직이기
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

    void PlayerStateChange()
    {
        switch (state)
        {
            case playerState.Idle:
                break;
            case playerState.Walk:
                break;
            case playerState.Run:
                break;
            case playerState.Hide:
                break;
            case playerState.HideWalk:
                break;
            case playerState.Attack:
                break;
            case playerState.Die:
                Die();
                break;
        }
    }


    void LeftMouseClick()
    {
        // 마우스 입력을 받았을 때
        if (Input.GetMouseButtonDown(0) && controlAble)
        {
            // 만일 입력된 이동 코루틴이 존재한다면 리셋해버리고 다시 이동
            if (resetClickCountCoroutine != null)
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                StopCoroutine(resetClickCountCoroutine);
            }
            // 너무 연달아서 클릭하지 않기 위한 방지 코루틴
            resetClickCountCoroutine = StartCoroutine(ResetClickCountAfterDelay(timeBetweenClicks));

            clickCount++;

            Debug.Log("왼클릭 받음");
            // 마우스로 찍은 위치의 좌표 값을 가져온다
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = ~(LayerMask.GetMask("Player", "Object"));

            // Raycast 검사를 수행하여 hit 객체 업데이트
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.tag == "Ground" && !ps.canCarry)
                {
                    NavMeshPath path = new NavMeshPath();

                    if (agent.CalculatePath(hit.point, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        // 첫 번째 클릭 로직
                        if (clickCount == 1)
                        {
                            // 유효한 경로가 있고, 목적지까지의 경로가 완전하다면 이동 실행
                            Debug.Log("유효한 위치로 이동합니다.");
                            ps.canMove = true;
                            ps.canRun = false;
                            Debug.Log("걷는 중");
                            agent.speed = ps.moveSpeed;
                            agent.isStopped = false;

                            if(ps.canUp)
                            {
                                state = playerState.Walk;
                                SoundManager.soundManager.SEPlay(SEType.Walk);
                                anim.SetTrigger("UpWalk");
                            }

                            else if (ps.canHide && state == playerState.Hide)
                            {
                                state = playerState.HideWalk;
                                SoundManager.soundManager.SEPlay(SEType.HideWalk);
                                anim.SetTrigger("CrouchToCrouchWalk");
                            }
                            else if (ps.canHide && state == playerState.Walk)
                            {
                                state = playerState.HideWalk;
                                SoundManager.soundManager.SEPlay(SEType.HideWalk);
                                anim.SetTrigger("WalkToCrouchWalk");
                            }
                            else if (!ps.canHide && state == playerState.Idle)
                            {
                                state = playerState.Walk;
                                anim.SetTrigger("IdleToWalk");
                                SoundManager.soundManager.SEPlay(SEType.Walk);
                            }
                            else if (!ps.canHide && state == playerState.HideWalk)
                            {
                                state = playerState.Walk;
                                anim.SetTrigger("CrouchWalkToWalk");
                                SoundManager.soundManager.SEPlay(SEType.Walk);
                            }
                            else if (runHide && state == playerState.Run)
                            {
                                state = playerState.HideWalk;
                                anim.SetTrigger("RunToCrouchWalk");
                                SoundManager.soundManager.SEPlay(SEType.HideWalk);
                            }
                            else if (state == playerState.Run)
                            {
                                state = playerState.Walk;
                                anim.SetTrigger("RunToWalk");
                                SoundManager.soundManager.SEPlay(SEType.Walk);
                            }
                            agent.SetDestination(hit.point);
                        }
                        // 두 번째 클릭 로직
                        else if (clickCount >= 2)
                        {
                            ps.canRun = true; // 달리기 시작
                            if (ps.canRun && ps.canHide && ps.canMove)
                            {
                                runHide = true;
                                ps.canHide = false;
                                Debug.Log(ps.canHide + "<-달리기 중 hide 상태");

                                state = playerState.Run;
                                Debug.Log("은신달리기");
                                anim.SetTrigger("CrouchWalkToRun");
                                SoundManager.soundManager.SEPlay(SEType.Run);
                            }

                            else if (!ps.canHide && ps.canRun)
                            {
                                state = playerState.Run;
                                Debug.Log("일반달리기");
                                anim.SetTrigger("WalkToRun");
                                SoundManager.soundManager.SEPlay(SEType.Run);
                            }
                            Debug.Log("더블클릭");
                            clickCount = 0;

                            agent.speed += ps.moveSpeed + 5.0f;
                            agent.SetDestination(hit.point);
                        }
                    }
                    else
                    {
                        // 경로가 유효하지 않거나 목적지까지 도달할 수 없는 경우
                        Debug.Log("이동할 수 없는 지역입니다.");
                        ps.canMove = false; // 이동하지 않음
                    }
                }
                else if (hit.collider.tag == "Enemy" && !ps.canCarry)
                {
                    Debug.Log("적클릭");
                    target = hit.collider.gameObject;
                    anim.SetTrigger("StartAttack");//어택 시작
                }
            }
        }
    }

    IEnumerator ResetClickCountAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        clickCount = 0; // 지정된 시간이 지나면 클릭 카운트 리셋
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
        if (ps.canMove && agent != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("이동 멈춤 메서드 2차 진입");

                if (state == playerState.Run && !runHide)
                {
                    state = playerState.Walk;
                    anim.SetTrigger("RunToWalk");
                    SoundManager.soundManager.SEPlay(SEType.Walk);
                }

                else if (state == playerState.Run && runHide)
                {
                    runHide = false;
                    ps.canHide = true;
                    state = playerState.HideWalk;
                    anim.SetTrigger("RunToCrouchWalk");
                }

                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    Debug.Log("이동중지");
                    ps.canMove = false;

                    // 추가적인 이동 명령이 없는 경우에만 Idle 상태로 전환
                    if (!ps.canMove)
                    {
                        if(ps.canUp)
                        {
                            state = playerState.Idle;
                            anim.SetTrigger("UpWalkToIdle");
                        }
                        else if (state == playerState.Walk)
                        {
                            state = playerState.Idle;
                            anim.SetTrigger("WalkToIdle");
                        }
                        else if (ps.canHide)
                        {
                            state = playerState.Hide;
                            anim.SetTrigger("CrouchWalkToCrouch");
                        }
                    }
                }
            }
        }
    }

    public void PlayerHiding()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ps.canHide)
            {
                ps.canHide = false;
                ps.moveSpeed = ps.moveSpeed * 3.0f;
            }
            else
            {
                ps.canHide = true;
                ps.moveSpeed = ps.moveSpeed / 3.0f;
            }

            if (ps.canHide && !ps.canMove && !ps.canRun)
            {
                // 숨기 상태로 전환될 때
                state = playerState.Hide;
                Debug.Log("player 숨은 상태");
                anim.SetTrigger("StartCrouching"); // 여기로 이동
                SoundManager.soundManager.SEPlay(SEType.Hide);

            }
            else if (!ps.canHide && !ps.canRun && !ps.canMove)
            {
                state = playerState.Idle;
                // 숨기 상태 해제될 때
                Debug.Log("player 원래 상태");
                anim.SetTrigger("StopCrouching"); // 여기로 이동
                SoundManager.soundManager.SEPlay(SEType.Hide);
            }
            else if (ps.canHide && ps.canMove && !ps.canRun)
            {
                state = playerState.HideWalk;
                anim.SetTrigger("WalkToCrouchWalk");
            }
            else if (!ps.canHide && ps.canMove && !ps.canRun)
            {
                state = playerState.Walk;
                anim.SetTrigger("CrouchWalkToWalk");
            }
        }
    }

    void AttackSystem()
    {
        if (count == -1)
        {
            anim.SetTrigger("Noto");
            StartCoroutine(NotoWait(1.0f));

            count = 0;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            agent.isStopped = true;
            count++;
            ps.canAttack = true;
            if (count == 1)
            {

                anim.SetTrigger("Batto");
                SoundManager.soundManager.SEPlay(SEType.Batto);
                StartCoroutine(BattoWait(1.0f));
            }
            if (count == 2)
            {
                ps.canAttack = false;
                anim.SetTrigger("Noto");
                SoundManager.soundManager.SEPlay(SEType.Noto);
                StartCoroutine(NotoWait(1.0f));

                count = 0;
            }
        }
    }

    IEnumerator BattoWait(float delay)
    {
        yield return new WaitForSeconds(1.2f);

        if (ps.canHide && !ps.canMove)
        {
            anim.SetTrigger("CrouchBattoToIdle");

        }
        else if (!ps.canHide && !ps.canMove)
        {
            anim.SetTrigger("BattoToIdle");
        }
        else if (ps.canHide && ps.canMove)
        {
            anim.SetTrigger("CrouchBattoToWalk");
        }
        else if (!ps.canHide && ps.canMove)
        {
            anim.SetTrigger("BattoToWalk");
        }
        else
        {
            ps.canRun = false;

            if (!ps.canMove)
            {
                anim.SetTrigger("BattoToIdle");
            }
            else if (ps.canMove)
            {
                anim.SetTrigger("BattoToWalk");
            }
        }
        agent.isStopped = false;
    }

    IEnumerator NotoWait(float delay)
    {
        yield return new WaitForSeconds(1.66f);

        if (ps.canHide && !ps.canMove)
        {
            anim.SetTrigger("CrouchNotoToIdle");
        }
        else if (!ps.canHide && !ps.canMove)
        {
            anim.SetTrigger("NotoToIdle");
        }
        else if (ps.canHide && ps.canMove)
        {
            anim.SetTrigger("CrouchNotoToWalk");
        }
        else if (!ps.canHide && ps.canMove)
        {
            anim.SetTrigger("NotoToWalk");
        }
        else
        {
            ps.canRun = false;

            if (!ps.canMove)
            {
                anim.SetTrigger("NotoToIdle");
            }
            else if (ps.canMove)
            {
                anim.SetTrigger("NotoToWalk");
            }
        }
        agent.isStopped = false;
    }

    void AttackDelay()
    {
        if (ps.canAttack && target != null &&!ps.canCarry)
        {
            if (Vector3.Distance(transform.position, target.transform.position) > ps.range)
            {//target의 위치와 플레이어의 위치의 거리가 공격범위보다 크면
                agent.SetDestination(target.transform.position);//목표지점을 타겟의 위치 로 지정
                agent.stoppingDistance = ps.range;//공격범위를 넘어가면 멈춤
            }
            else
            {
                Quaternion rotationToLookAt = Quaternion.LookRotation(target.transform.position - transform.position);//타겟의 위치와 플레이어의 위치의 거리를 계산하여 회전값을 계산
                float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y, ref rotateVelocity, rotateSpeedForAttack * (Time.deltaTime * 5));
                transform.eulerAngles = new Vector3(0, rotationY, 0); //위의 계산을 통해 회전함 (타겟을 쳐다봄)
                agent.SetDestination(transform.position);
                if (target != null)
                {
                    enemy = target.GetComponentInChildren<EnemyMaster>();
                    anim.SetTrigger("Attack");
                    SoundManager.soundManager.SEPlay(SEType.Attack);
                    if (target != null)
                    {
                        if (enemy != null)
                        {
                            Debug.Log("공격여부");
                            enemy.HitEnemy(ps.power); // ps.power는 플레이어의 공격력입니다.
                        }
                    }
                    StartCoroutine(AttackDelayCoroutine(1.3f));

                }
            }
        }
    }

    IEnumerator AttackDelayCoroutine(float delay)
    {

        yield return new WaitForSeconds(delay);

        if (ps.canHide)
        {
            anim.SetTrigger("AttackToCrouch");
        }
        else if (!ps.canHide)
        {
            anim.SetTrigger("AttackToIdle");
        }
    }

    // 플레이어 피격 함수
    public void DamageAction(int damage)
    {
        // 적의 공격력만큼 플레이어의 체력을 깎는다
        agent.isStopped = true;
        ps.hp -= damage;
        Debug.Log(ps.hp);
        // 플레이어의 HP가 0보다 크면 피격 효과 ON
        if (ps.hp > 0)
        {
            if (ps.canHide)
            {
                anim.SetTrigger("CrouchDamage");
                SoundManager.soundManager.SEPlay(SEType.Damage);
            }
            else
            {
                anim.SetTrigger("Damage");
                SoundManager.soundManager.SEPlay(SEType.Damage);
            }
            StartCoroutine(WaitForDamaged(0.5f));
        }

        else if (ps.hp <= 0)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            state = playerState.Die;
            anim.SetTrigger("Died");
        }
    }

    IEnumerator WaitForDamaged(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (ps.canHide)
        {
            anim.SetTrigger("CrouchDamaged");
        }
        else
        {
            anim.SetTrigger("Damaged");

        }
        agent.isStopped = false;
    }

    // 사망 상태
    void Die()
    {
        // 진행 중인 피격 코루틴 함수를 중지한다
        StopAllCoroutines();

        // 사망 상태를 처리하기 위한 코루틴을 실행한다
        StartCoroutine(DieProcess());
    }

    // 사망 상태 처리용 코루틴
    IEnumerator DieProcess()
    {
        // 캐릭터 컨트롤러를 비활성화한다
        this.enabled = false;

        // 2초 동안 기다린 이후 자기자신을 제거한다
        yield return new WaitForSeconds(2.0f);
        print("소멸!");
        Destroy(gameObject);
    }

    void CarryDeadBody()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            ps.canCarry = true;

            if (Input.GetMouseButtonDown(0) && ps.canCarry && !ps.canUp)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Debug.Log("적클릭");
                // Raycast 검사를 수행하여 hit 객체 업데이트
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log("레이캐스트 여부");
                    Debug.Log(hit.collider.tag);
                    if (hit.collider.tag == "DeadBody")
                    {
                        enemyObject = hit.collider.gameObject;
                        enemyAnim = hit.collider.GetComponentInChildren<Animator>();

                        agent.isStopped = true;
                        anim.SetTrigger("UpBody");
                        enemyAnim.SetTrigger("UpBody");
                        SoundManager.soundManager.SEPlay(SEType.LiftUp);
                        enemy.transform.SetParent(this.transform);

                        StartCoroutine(CarryDeadBodyIdle(0.03f));
                    }
                }
            }

            else if (ps.canUp && Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                Debug.Log("적클릭");
                // Raycast 검사를 수행하여 hit 객체 업데이트
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.collider.tag == "Ground" || hit.collider.tag == "Bush")
                    {
                        agent.isStopped = true;
                        anim.SetTrigger("DownBody");
                        enemyAnim.SetTrigger("DownBody");
                        SoundManager.soundManager.SEPlay(SEType.LiftDown);

                        StartCoroutine(CarryDeadBodyDown(0.5f));
                    }
                }
            }
        }
        else 
        {
            ps.canCarry= false;
        }
    }

    IEnumerator CarryDeadBodyIdle(float delay)
    {
        yield return new WaitForSeconds(delay);
        ps.canHide = false;
        ps.canUp = true;
        anim.SetTrigger("UpIdle");
        agent.isStopped = false;
    }

    IEnumerator CarryDeadBodyDown(float delay)
    {
        yield return new WaitForSeconds(delay);

        Transform deadBody = player.transform.Find("Enemy");
        deadBody.SetParent(null);

        ps.canUp = false;
        anim.SetTrigger("DownBodyToIdle");
        agent.isStopped = false;
    }

    //void SkillShot()
    //{
    //    if (Input.GetKeyDown(KeyCode.S) && !skillDelay)
    //    {
    //        skillDelay = true;
    //    }

    //    else if (Input.GetKeyDown(KeyCode.S) && skillDelay)
    //    {
    //        agent.isStopped = false;
    //        skillDelay = false;
    //    }

    //    if(skillDelay)
    //    {
    //        agent.isStopped = true;
    //        skillDelay = true;
    //        if (skill == 0)
    //        {
    //            ThrowStone();
    //        }
    //        else if (skill == 1 && skillDelay)
    //        {

    //        }
    //        else if (skill == 2 && skillDelay)
    //        {

    //        }
    //    }
    //}

    //void ThrowStone()
    //{
    //        agent.isStopped = true;
    //        skillDelay = true;
    //        anim.SetTrigger("SkillDelay");
    //        ShowTrajectory();

    //        if (Input.GetMouseButtonDown(0))
    //        {
    //            GameObject stoneInstance = Instantiate(stonePrefab, throwPoint.position, Quaternion.identity);
    //            Rigidbody stoneRb = stoneInstance.GetComponent<Rigidbody>();
    //            stoneRb.AddForce(throwPoint.forward * throwForce, ForceMode.VelocityChange);
    //        }
    //}

    //void ShowTrajectory()
    //{
    //    Vector3[] points = new Vector3[trajectoryResolution];
    //    Vector3 startingPosition = throwPoint.position;
    //    Vector3 startingVelocity = throwPoint.forward * throwForce;

    //    for (int i = 0; i < trajectoryResolution; i++)
    //    {
    //        float time = i * 0.1f;
    //        points[i] = startingPosition + startingVelocity * time + Physics.gravity * time * time / 2f;
    //        if (points[i].y < startingPosition.y)
    //        {
    //            trajectoryLine.positionCount = i + 1;
    //            break;
    //        }
    //    }

    //    trajectoryLine.SetPositions(points);
    //}
}