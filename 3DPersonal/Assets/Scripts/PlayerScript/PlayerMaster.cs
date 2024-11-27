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

    float currentTime = 0.0f;   // �����ð�

    // ������ ������Ʈ
    protected Rigidbody rigid;

    // enum �ν��Ͻ�
    playerState state;

    // �׺���̼� �Ž� ������Ʈ
    protected NavMeshAgent agent;

    // �÷��̾��� ��ġ (Transform)
    protected Transform player;
    protected GameObject selectPlayer;

    // ���� Ÿ��
    public GameObject target = null;
    // �ִϸ����� ������Ʈ
    protected Animator anim;

    protected Camera mainCamera; // ���� ī�޶�

    protected Vector3 targetPos; // ĳ������ �̵� ����

    protected int clickCount = 0;
    protected float timeBetweenClicks = 0.2f; // Ŭ�� ������ �ִ� ��� �ð�
    protected Coroutine resetClickCountCoroutine;

    private bool runHide = false;

    public float rotateVelocity;//ȸ���ӵ�
    public float rotateSpeedForAttack;//����ȸ���ӵ�

    // �ߵ� ���� ī��Ʈ
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

    //public GameObject stonePrefab; // ������ Prefab
    //public Transform throwPoint; // ������ ����
    //public LineRenderer trajectoryLine; // ������ �׸��� LineRenderer
    //public float throwForce = 40f; // ������ ��
    //public int trajectoryResolution = 30; // ������ �ػ�

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

    // ������ �÷��̾ �����̱�
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
        // ���콺 �Է��� �޾��� ��
        if (Input.GetMouseButtonDown(0) && controlAble)
        {
            // ���� �Էµ� �̵� �ڷ�ƾ�� �����Ѵٸ� �����ع����� �ٽ� �̵�
            if (resetClickCountCoroutine != null)
            {
                agent.velocity = Vector3.zero;
                agent.ResetPath();
                StopCoroutine(resetClickCountCoroutine);
            }
            // �ʹ� ���޾Ƽ� Ŭ������ �ʱ� ���� ���� �ڷ�ƾ
            resetClickCountCoroutine = StartCoroutine(ResetClickCountAfterDelay(timeBetweenClicks));

            clickCount++;

            Debug.Log("��Ŭ�� ����");
            // ���콺�� ���� ��ġ�� ��ǥ ���� �����´�
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = ~(LayerMask.GetMask("Player", "Object"));

            // Raycast �˻縦 �����Ͽ� hit ��ü ������Ʈ
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.tag == "Ground" && !ps.canCarry)
                {
                    NavMeshPath path = new NavMeshPath();

                    if (agent.CalculatePath(hit.point, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        // ù ��° Ŭ�� ����
                        if (clickCount == 1)
                        {
                            // ��ȿ�� ��ΰ� �ְ�, ������������ ��ΰ� �����ϴٸ� �̵� ����
                            Debug.Log("��ȿ�� ��ġ�� �̵��մϴ�.");
                            ps.canMove = true;
                            ps.canRun = false;
                            Debug.Log("�ȴ� ��");
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
                        // �� ��° Ŭ�� ����
                        else if (clickCount >= 2)
                        {
                            ps.canRun = true; // �޸��� ����
                            if (ps.canRun && ps.canHide && ps.canMove)
                            {
                                runHide = true;
                                ps.canHide = false;
                                Debug.Log(ps.canHide + "<-�޸��� �� hide ����");

                                state = playerState.Run;
                                Debug.Log("���Ŵ޸���");
                                anim.SetTrigger("CrouchWalkToRun");
                                SoundManager.soundManager.SEPlay(SEType.Run);
                            }

                            else if (!ps.canHide && ps.canRun)
                            {
                                state = playerState.Run;
                                Debug.Log("�Ϲݴ޸���");
                                anim.SetTrigger("WalkToRun");
                                SoundManager.soundManager.SEPlay(SEType.Run);
                            }
                            Debug.Log("����Ŭ��");
                            clickCount = 0;

                            agent.speed += ps.moveSpeed + 5.0f;
                            agent.SetDestination(hit.point);
                        }
                    }
                    else
                    {
                        // ��ΰ� ��ȿ���� �ʰų� ���������� ������ �� ���� ���
                        Debug.Log("�̵��� �� ���� �����Դϴ�.");
                        ps.canMove = false; // �̵����� ����
                    }
                }
                else if (hit.collider.tag == "Enemy" && !ps.canCarry)
                {
                    Debug.Log("��Ŭ��");
                    target = hit.collider.gameObject;
                    anim.SetTrigger("StartAttack");//���� ����
                }
            }
        }
    }

    IEnumerator ResetClickCountAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        clickCount = 0; // ������ �ð��� ������ Ŭ�� ī��Ʈ ����
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
        if (ps.canMove && agent != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("�̵� ���� �޼��� 2�� ����");

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
                    Debug.Log("�̵�����");
                    ps.canMove = false;

                    // �߰����� �̵� ����� ���� ��쿡�� Idle ���·� ��ȯ
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
                // ���� ���·� ��ȯ�� ��
                state = playerState.Hide;
                Debug.Log("player ���� ����");
                anim.SetTrigger("StartCrouching"); // ����� �̵�
                SoundManager.soundManager.SEPlay(SEType.Hide);

            }
            else if (!ps.canHide && !ps.canRun && !ps.canMove)
            {
                state = playerState.Idle;
                // ���� ���� ������ ��
                Debug.Log("player ���� ����");
                anim.SetTrigger("StopCrouching"); // ����� �̵�
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
            {//target�� ��ġ�� �÷��̾��� ��ġ�� �Ÿ��� ���ݹ������� ũ��
                agent.SetDestination(target.transform.position);//��ǥ������ Ÿ���� ��ġ �� ����
                agent.stoppingDistance = ps.range;//���ݹ����� �Ѿ�� ����
            }
            else
            {
                Quaternion rotationToLookAt = Quaternion.LookRotation(target.transform.position - transform.position);//Ÿ���� ��ġ�� �÷��̾��� ��ġ�� �Ÿ��� ����Ͽ� ȸ������ ���
                float rotationY = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationToLookAt.eulerAngles.y, ref rotateVelocity, rotateSpeedForAttack * (Time.deltaTime * 5));
                transform.eulerAngles = new Vector3(0, rotationY, 0); //���� ����� ���� ȸ���� (Ÿ���� �Ĵٺ�)
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
                            Debug.Log("���ݿ���");
                            enemy.HitEnemy(ps.power); // ps.power�� �÷��̾��� ���ݷ��Դϴ�.
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

    // �÷��̾� �ǰ� �Լ�
    public void DamageAction(int damage)
    {
        // ���� ���ݷ¸�ŭ �÷��̾��� ü���� ��´�
        agent.isStopped = true;
        ps.hp -= damage;
        Debug.Log(ps.hp);
        // �÷��̾��� HP�� 0���� ũ�� �ǰ� ȿ�� ON
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

    // ��� ����
    void Die()
    {
        // ���� ���� �ǰ� �ڷ�ƾ �Լ��� �����Ѵ�
        StopAllCoroutines();

        // ��� ���¸� ó���ϱ� ���� �ڷ�ƾ�� �����Ѵ�
        StartCoroutine(DieProcess());
    }

    // ��� ���� ó���� �ڷ�ƾ
    IEnumerator DieProcess()
    {
        // ĳ���� ��Ʈ�ѷ��� ��Ȱ��ȭ�Ѵ�
        this.enabled = false;

        // 2�� ���� ��ٸ� ���� �ڱ��ڽ��� �����Ѵ�
        yield return new WaitForSeconds(2.0f);
        print("�Ҹ�!");
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

                Debug.Log("��Ŭ��");
                // Raycast �˻縦 �����Ͽ� hit ��ü ������Ʈ
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    Debug.Log("����ĳ��Ʈ ����");
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

                Debug.Log("��Ŭ��");
                // Raycast �˻縦 �����Ͽ� hit ��ü ������Ʈ
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