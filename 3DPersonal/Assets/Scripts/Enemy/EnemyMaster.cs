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

    // 시야스크립트
    public FieldofView fov;
    public PlayerStat PS;
    #endregion

    //////////////////////////////////////////////

    #region 컴포넌트

    // 애니메이터 컴포넌트
    Animator anim;

    #endregion

    //////////////////////////////////////////////

    #region public 변수

    public GameObject GameEnding = null;

    // 플레이어 위치를 가져올 변수
    private Vector3 player = Vector3.zero;

    private GameObject attackPlayer = null;

    // 선택된 적을 확인하는 용도의 변수
    public static GameObject enemyChoice = null;

    // 이 스크립트가 연동된 오브젝트

    public GameObject myEnemy;

    #endregion

    ////////////////////////////////////////////////

    #region private 변수

    // 누적 시간
    protected float currentTime = 0.0f;

    // 딜레이
    protected float attackDelay = 3.0f;

    // 적 초기 위치
    Vector3 originPos;
    Quaternion originRot;

    #endregion

    ////////////////////////////////////////////////

    #region 속성관련

    public bool onClickCheck = false;
    // 적 상태 enum 구조체
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

    // enum 인스턴스 선언
    EnemyState ES;

    public bool gameTarget = false;


    #endregion

    ////////////////////////////////////////////////

    #region 이동 및 순찰 이벤트

    private NavMeshAgent agent;
    public Transform[] wayPoints = null;
    private int wayPointIndex;

    // 순찰 루트 좌표
    Vector3 target;

    public bool patrolCheck = false;
    bool patrolWay = true;

    float delayTime = 1f;

    #endregion

    #region 스탯

    // 적 체력
    public int maxHp;
    private int hp;

    // 이동속도
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
        // 우클릭을 감지
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
                // enum 변수의 상태 전환
                ES = EnemyState.Patrol;

                // 이동 애니메이션 전환하기
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

            // enum 변수의 상태 전환
            ES = EnemyState.Chase;

            // 이동 애니메이션 전환하기
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
            // 이동 애니메이션 전환하기
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
                print("상태 전환 : Chase -> Attack");

                // 누적 시간을 딜레이 시간만큼 미리 진행시켜둔다 (즉시 공격)
                currentTime = attackDelay;

                // 공격 대기 애니메이션
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

    // 마지막 알려진 위치 주변을 탐색하는 메서드
    void SearchNearbyObjects()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // 주변의 모든 'bush' 태그를 가진 오브젝트 찾기
            Collider[] hits = Physics.OverlapSphere(transform.position, 200f); // 10f는 검색 반경입니다.
            List<GameObject> bushes = new List<GameObject>();
            foreach (var hit in hits)
            {
                if (hit.gameObject.tag == "Bush")
                {
                    bushes.Add(hit.gameObject);
                }
            }

            // 가장 가까운 세 개의 'bush' 찾기
            bushes.Sort((a, b) => (a.transform.position - transform.position).sqrMagnitude.CompareTo((b.transform.position - transform.position).sqrMagnitude));
            bushes = bushes.Take(3).ToList();

            // 랜덤한 'bush'로 이동
            if (bushes.Count > 0)
            {
                GameObject targetBush = bushes[Random.Range(0, bushes.Count)];
                agent.SetDestination(targetBush.transform.position);
            }

            // 잠시 기다렸다가 주변 조사
            StartCoroutine(CheckSurroundingsAfterDelay(2.0f)); // 2초 후에 주변 조사
        }
    }

    IEnumerator CheckSurroundingsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Collider[] playerHits = Physics.OverlapSphere(transform.position, 3.0f); // 3.0f는 검색 반경입니다.
        bool playerNearby = playerHits.Any(hit => hit.gameObject.tag == "Player");

        if (!playerNearby)
        {
            // 'Player' 태그를 가진 오브젝트가 주변에 없으면, 원래 순찰 위치로 돌아갑니다.
            MoveToNextWayPoint();
        }
    }

    void Attack()
    {
        if(fov.searchPlayer && fov.findPlayer)
        {
            if (Vector3.Distance(transform.position, fov.lastKnownPosition) < attackDistance)
            {
                // 일정시간마다 공격한다
                // 누적된 시간이 딜레이를 넘어설 때마다 초기화
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                currentTime += Time.deltaTime;
                if (currentTime > attackDelay)
                {
                    currentTime = 0;

                    // 공격 애니메이션
                    anim.SetTrigger("StartAttack");
                    SoundManager.soundManager.SEPlay(SEType.EnemyAttack);
                    attackPlayer = fov.target.gameObject;

                    if (attackPlayer != null)
                    {
                        PlayerMaster playerMaster = attackPlayer.GetComponent<PlayerMaster>();
                        if (playerMaster != null)
                        {
                            playerMaster.DamageAction(power); // power는 적의 공격력입니다.
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

    // 오브젝트가 트리거 콜라이더에 진입했을 때 호출
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            attackPlayer = other.gameObject;
            PS = other.gameObject.GetComponent<PlayerStat>();
        }
        // 부쉬 안에 플레이어가 있는지 확인
        if (other.CompareTag("Bush"))
        { 
            // 부쉬 내부에 있는 모든 오브젝트를 검색
            Collider[] hits = Physics.OverlapSphere(other.transform.position, other.bounds.size.x / 2);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    // 플레이어를 발견했다면 공격
                    player = hit.transform.position;

                    ES = EnemyState.Attack;

                    // 누적 시간을 딜레이 시간만큼 미리 진행시켜둔다 (즉시 공격)
                    currentTime = attackDelay;

                    // 공격 대기 애니메이션
                    anim.SetTrigger("ChaseToAttackDelay");

                    break; // 플레이어를 발견하면 루프를 중단
                }
            }
        }
    }

    public void AttackAction()
    {
        if (attackPlayer != null)
        {
            // attackPlayer GameObject에서 PlayerMaster 컴포넌트를 가져옵니다.
            PlayerMaster playerMaster = attackPlayer.GetComponent<PlayerMaster>();
            if (playerMaster != null)
            {
                // PlayerMaster의 메서드 호출
                playerMaster.DamageAction(power);
            }
        }
    }

    public void HitEnemy(int hitPower)
    {
        // 피격, 사망, 복귀 상태일 경우에는 함수 즉시 종료
        if (ES == EnemyState.Damaged ||
            ES == EnemyState.Die)
        {
            return;
        }

        // 플레이어의 공격력만큼 적 체력을 감소시켜준다
        hp -= hitPower;

        Debug.Log("공격받음");
        // 에이전트의 이동을 정지하고 경로를 초기화
        agent.isStopped = true;
        agent.ResetPath();

        // 적 체력이 0보다 크면 피격 상태로 전환
        if (hp > 0 && hp < 2)
        {
            hp = maxHp;
            ES = EnemyState.Damaged;
            Debug.Log("공격받음2");


            // 피격 애니메이션 재생
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

        // 그렇지 않다면 사망 상태로 전환
        else
        {
            fov.fixOrPatrol = true;
            Debug.Log("공격받음3");
            ES = EnemyState.Die;
            print("상태 전환 : Any State -> Die");

            // 사망 애니메이션 재생
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

        // 플레이어의 위치를 바라보게 함
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


