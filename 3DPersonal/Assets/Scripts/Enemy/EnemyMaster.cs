using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

using static EnemyController;

public class EnemyMaster : MonoBehaviour
{
    #region Scripts

    // �þ߽�ũ��Ʈ
    public FieldofView fov;
    public PlayerStat PS;
    #endregion

    //////////////////////////////////////////////

    #region ������Ʈ

    // �ִϸ����� ������Ʈ
    Animator anim;

    #endregion

    //////////////////////////////////////////////

    #region public ����

    public GameObject GameEnding = null;

    // �÷��̾� ��ġ�� ������ ����
    private Vector3 player = Vector3.zero;

    private GameObject attackPlayer = null;

    // ���õ� ���� Ȯ���ϴ� �뵵�� ����
    public static GameObject enemyChoice = null;

    // �� ��ũ��Ʈ�� ������ ������Ʈ

    public GameObject myEnemy;

    #endregion

    ////////////////////////////////////////////////

    #region private ����

    // ���� �ð�
    protected float currentTime = 0.0f;

    // ������
    protected float attackDelay = 3.0f;

    // �� �ʱ� ��ġ
    Vector3 originPos;
    Quaternion originRot;

    #endregion

    ////////////////////////////////////////////////

    #region �Ӽ�����

    public bool onClickCheck = false;
    // �� ���� enum ����ü
    enum EnemyState
    {
        None,
        Idle,
        Patrol,
        Chase,
        Attack,
        Damaged,
        Die,
        Delete
    }

    // enum �ν��Ͻ� ����
    EnemyState ES;

    public bool gameTarget = false;


    #endregion

    ////////////////////////////////////////////////

    #region �̵� �� ���� �̺�Ʈ

    private NavMeshAgent agent;
    public Transform[] wayPoints = null;
    private int wayPointIndex;

    // ���� ��Ʈ ��ǥ
    Vector3 target;

    public bool patrolCheck = false;
    bool patrolWay = true;

    float delayTime = 1f;

    #endregion

    #region ����

    // �� ü��
    public int maxHp;
    private int hp;

    // �̵��ӵ�
    [SerializeField]
    float moveSpeed;
    [SerializeField]
    public int power = 1;

    [SerializeField]
    float attackDistance;
    
    #endregion

    //////////////////////////////////////////////

    private void Start()
    {
        if(GameEnding != null)
        {
            GameEnding.SetActive(false);
        }
         hp = maxHp;
        ES = EnemyState.Idle;
        myEnemy = this.transform.parent.gameObject;
        fov = this.gameObject.GetComponent<FieldofView>();
        agent = GetComponentInParent<NavMeshAgent>();
        anim = GetComponent<Animator>();
     }

    private void Update()
    {
        OnMouseDown();
        EnemyStatCheck();
    }

    private void OnMouseDown()
    {
        // ��Ŭ���� ����
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.tag == "Enemy")
                {
                    enemyChoice = hit.collider.gameObject;

                    if (myEnemy == enemyChoice)
                    {
                        onClickCheck = true;
                        fov.fovMeshRenderer.enabled = true;
                    }
                    else if (myEnemy != enemyChoice)
                    {
                        onClickCheck = false;
                        fov.fovMeshRenderer.enabled = false;
                    }
                }
            }
        }
    }

    public void EnemyStatCheck()
    {
        switch (ES)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Delete:
                break;
            case EnemyState.Damaged:
                break;
            case EnemyState.Die:
                break;
        }
    }



    void Idle()
    {
        if (fov.fixOrPatrol && patrolCheck && !fov.searchPlayer)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }

            currentTime += Time.deltaTime;
            if(currentTime > delayTime)
            {
                // enum ������ ���� ��ȯ
                ES = EnemyState.Patrol;

                // �̵� �ִϸ��̼� ��ȯ�ϱ�
                anim.SetTrigger("IdleToPatrol");

                currentTime = 0;

                agent.isStopped = false;
            }
        }

        if (fov.searchPlayer && fov.findPlayer)
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
            }

            // enum ������ ���� ��ȯ
            ES = EnemyState.Chase;

            // �̵� �ִϸ��̼� ��ȯ�ϱ�
            anim.SetTrigger("Chased");

            agent.isStopped = false;
        }

    }

    void Patrol()
    {
        if (fov.searchPlayer)
        {
            agent.isStopped = false;
            ES = EnemyState.Idle;
            // �̵� �ִϸ��̼� ��ȯ�ϱ�
            anim.SetTrigger("PatrolToIdle");
        }
        else
        {
            MoveToNextWayPoint();
        }
    }

    void MoveToNextWayPoint()
    {
        if (agent.velocity == Vector3.zero && !agent.isStopped)
        {
            agent.SetDestination(wayPoints[wayPointIndex++].position);

            if (wayPointIndex >= wayPoints.Length)
            {
                wayPointIndex = 0;
            }
        }
    }


    void Chase()
    {
        if (fov.searchPlayer && fov.findPlayer)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, fov.lastKnownPosition);

            if (Vector3.Distance(transform.position, fov.lastKnownPosition) > attackDistance)
            {
                ChaseLastSeenPosition();
            }
            else if (distanceToPlayer <= attackDistance && fov.searchPlayer && fov.findPlayer)
            {
                agent.isStopped = true;
                ES = EnemyState.Attack;
                print("���� ��ȯ : Chase -> Attack");

                // ���� �ð��� ������ �ð���ŭ �̸� ������ѵд� (��� ����)
                currentTime = attackDelay;

                // ���� ��� �ִϸ��̼�
                anim.SetTrigger("ChaseToAttackDelay");
            }
        }
        else
        {
            ChaseLastSeenPosition();
        }
    }
    
    void ChaseLastSeenPosition()
    {
        if (fov.searchPlayer && fov.findPlayer)
        {
            if (fov.searchPlayer && fov.lastKnownPosition != Vector3.zero)
            {
                agent.isStopped = false;
                agent.SetDestination(fov.lastKnownPosition);
            }
        }
        else if (!fov.searchPlayer && fov.findPlayer)
        {
            SearchNearbyObjects();
        }
    }

    // ������ �˷��� ��ġ �ֺ��� Ž���ϴ� �޼���
    void SearchNearbyObjects()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // �ֺ��� ��� 'bush' �±׸� ���� ������Ʈ ã��
            Collider[] hits = Physics.OverlapSphere(transform.position, 200f); // 10f�� �˻� �ݰ��Դϴ�.
            List<GameObject> bushes = new List<GameObject>();
            foreach (var hit in hits)
            {
                if (hit.gameObject.tag == "Bush")
                {
                    bushes.Add(hit.gameObject);
                }
            }

            // ���� ����� �� ���� 'bush' ã��
            bushes.Sort((a, b) => (a.transform.position - transform.position).sqrMagnitude.CompareTo((b.transform.position - transform.position).sqrMagnitude));
            bushes = bushes.Take(3).ToList();

            // ������ 'bush'�� �̵�
            if (bushes.Count > 0)
            {
                GameObject targetBush = bushes[Random.Range(0, bushes.Count)];
                agent.SetDestination(targetBush.transform.position);
            }

            // ��� ��ٷȴٰ� �ֺ� ����
            StartCoroutine(CheckSurroundingsAfterDelay(2.0f)); // 2�� �Ŀ� �ֺ� ����
        }
    }

    IEnumerator CheckSurroundingsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Collider[] playerHits = Physics.OverlapSphere(transform.position, 3.0f); // 3.0f�� �˻� �ݰ��Դϴ�.
        bool playerNearby = playerHits.Any(hit => hit.gameObject.tag == "Player");

        if (!playerNearby)
        {
            // 'Player' �±׸� ���� ������Ʈ�� �ֺ��� ������, ���� ���� ��ġ�� ���ư��ϴ�.
            MoveToNextWayPoint();
        }
    }

    void Attack()
    {
        if(fov.searchPlayer && fov.findPlayer)
        {
            if (Vector3.Distance(transform.position, fov.lastKnownPosition) < attackDistance)
            {
                // �����ð����� �����Ѵ�
                // ������ �ð��� �����̸� �Ѿ ������ �ʱ�ȭ
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                currentTime += Time.deltaTime;
                if (currentTime > attackDelay)
                {
                    currentTime = 0;

                    // ���� �ִϸ��̼�
                    anim.SetTrigger("StartAttack");
                    SoundManager.soundManager.SEPlay(SEType.EnemyAttack);
                    attackPlayer = fov.target.gameObject;

                    if (attackPlayer != null)
                    {
                        PlayerMaster playerMaster = attackPlayer.GetComponent<PlayerMaster>();
                        if (playerMaster != null)
                        {
                            playerMaster.DamageAction(power); // power�� ���� ���ݷ��Դϴ�.
                        }
                    }

                    agent.isStopped = false;
                }

                anim.SetTrigger("AttackToAttackDelay");
            }
            else
            {
                anim.SetTrigger("AttackToChase");
                agent.isStopped = false;
            }
        }
     }

    // ������Ʈ�� Ʈ���� �ݶ��̴��� �������� �� ȣ��
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            attackPlayer = other.gameObject;
            PS = other.gameObject.GetComponent<PlayerStat>();
        }
        // �ν� �ȿ� �÷��̾ �ִ��� Ȯ��
        if (other.CompareTag("Bush"))
        { 
            // �ν� ���ο� �ִ� ��� ������Ʈ�� �˻�
            Collider[] hits = Physics.OverlapSphere(other.transform.position, other.bounds.size.x / 2);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    // �÷��̾ �߰��ߴٸ� ����
                    player = hit.transform.position;

                    ES = EnemyState.Attack;

                    // ���� �ð��� ������ �ð���ŭ �̸� ������ѵд� (��� ����)
                    currentTime = attackDelay;

                    // ���� ��� �ִϸ��̼�
                    anim.SetTrigger("ChaseToAttackDelay");

                    break; // �÷��̾ �߰��ϸ� ������ �ߴ�
                }
            }
        }
    }

    public void AttackAction()
    {
        if (attackPlayer != null)
        {
            // attackPlayer GameObject���� PlayerMaster ������Ʈ�� �����ɴϴ�.
            PlayerMaster playerMaster = attackPlayer.GetComponent<PlayerMaster>();
            if (playerMaster != null)
            {
                // PlayerMaster�� �޼��� ȣ��
                playerMaster.DamageAction(power);
            }
        }
    }

    public void HitEnemy(int hitPower)
    {
        // �ǰ�, ���, ���� ������ ��쿡�� �Լ� ��� ����
        if (ES == EnemyState.Damaged ||
            ES == EnemyState.Die)
        {
            return;
        }

        // �÷��̾��� ���ݷ¸�ŭ �� ü���� ���ҽ����ش�
        hp -= hitPower;

        Debug.Log("���ݹ���");
        // ������Ʈ�� �̵��� �����ϰ� ��θ� �ʱ�ȭ
        agent.isStopped = true;
        agent.ResetPath();

        // �� ü���� 0���� ũ�� �ǰ� ���·� ��ȯ
        if (hp > 0 && hp < 2)
        {
            hp = maxHp;
            ES = EnemyState.Damaged;
            Debug.Log("���ݹ���2");


            // �ǰ� �ִϸ��̼� ���
            anim.SetTrigger("Damage");
            SoundManager.soundManager.SEPlay(SEType.Damage);

            StartCoroutine(DamageAction(5.0f));

        }
        else if(hp>1 && hp <3)
        {
            hp = maxHp;
            ES = EnemyState.Damaged;
            Block();
            anim.SetTrigger("Block");
        }

        // �׷��� �ʴٸ� ��� ���·� ��ȯ
        else
        {
            fov.fixOrPatrol = true;
            Debug.Log("���ݹ���3");
            ES = EnemyState.Die;
            print("���� ��ȯ : Any State -> Die");

            // ��� �ִϸ��̼� ���
            anim.SetTrigger("Died");
            fov.enabled = false;
            fov.expandingViewMeshRenderer.enabled = false;
            fov.fovMeshRenderer.enabled = false;
 
            Die();
            
        }
    }

    IEnumerator DamageAction(float delay)
    {

        yield return new WaitForSeconds(delay);
        anim.SetTrigger("Damaged");
        agent.isStopped = false;
        fov.fixOrPatrol = true;
        fov.findPlayer = true;
        patrolCheck = true;
        Damaged();
    }

    void Block()
    {
        if (player != null)
        {
            fov.findPlayer = true;
            StartCoroutine(BlockAction(0.5f));
        }
    }

    IEnumerator BlockAction(float delay)
    {
        yield return new WaitForSeconds(delay);

        fov.fixOrPatrol = true;
        fov.findPlayer = true;
        patrolCheck = true;

        // �÷��̾��� ��ġ�� �ٶ󺸰� ��
        transform.LookAt(target);
        anim.SetTrigger("Blocked");
    }

    void Damaged()
    {
        StartCoroutine(DamagedAction(0.5f));
    }

    IEnumerator DamagedAction(float delay)
    {
        yield return new WaitForSeconds(delay);

        anim.SetTrigger("Damaged");
        SoundManager.soundManager.SEPlay(SEType.Damage);
    }

    void Die()
    {
        if(gameTarget)
        {
            GameEnding.SetActive(true);
            Time.timeScale = 0.3f;
            Time.fixedDeltaTime = Time.timeScale;
            anim.SetTrigger("DeadBody");
            SoundManager.soundManager.SEPlay(SEType.Clear);
            StartCoroutine(ObjectDestroy(0.5f));
        }
        else
        {
            GameObject all = this.gameObject;

            all.transform.parent.tag = "DeadBody";
            gameObject.tag = "DeadBody";
            gameObject.layer = 12;
            anim.SetTrigger("DeadBody");
        }
    }

    IEnumerator ObjectDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
  
        Destroy(this.gameObject);

        SceneManager.LoadScene("Title");

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = Time.timeScale;

    }
}


