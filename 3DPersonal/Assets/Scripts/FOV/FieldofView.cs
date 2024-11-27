using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FieldofView : MonoBehaviour
{

    // 고정형이냐 순찰형이냐의 여부(고정형은 좌우로 시야확인, 순찰형은 고정 / false가 고정 / true가 patrol )
    public bool fixOrPatrol;
    // Player 탐색 여부 확인
    public bool searchPlayer = false;

    // Player 탐색 완료
    public bool findPlayer = false;

    // 타겟 도주 여부
    bool targetRun = false;

    // 숨는 범위 내에 있는지 여부
    bool viewHideRange = false;

    // 코루틴 반복 실행 방지
    private bool isExpandingFOVRunning = false;

    // 타겟의 마지막 알려진 위치를 저장
    public Vector3 lastKnownPosition = Vector3.zero;

    // 시야범위
    public float viewRadius;
    [Range(0, 360)]

    // 시야각
    public float viewAngle;

    // FOV 타겟 레이어
    public LayerMask targetMask;

    // 회전 지연시간
    [SerializeField]
    public float delayRotate;

    // 시야를 막는 물체의 레이어
    public LayerMask obstacleMask;

    // 보이는 타겟을 리스트에 저장
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    // 매쉬를 얼마나 계산할지
    public float meshResolution;

    // 가장자리 반복계산
    public int edgeResolveIterations;

    // 반복계산 임계값
    public float edgeDstThreshold;

    // 시야의 필터
    public MeshFilter viewMeshFilter;

    int SoundCount = 0;
    // 필터 매쉬
    public Mesh viewMesh;

    // FOV의 Material 참조
    public Material fovMaterial;

    public MeshRenderer fovMeshRenderer;
    
    // 확장되는 FOV를 위한 마테리얼
    private Material expandingFovMaterial;


    // 확장되는 FOV의 현재 반경
    private float expandingRadius = 0f;

    // 확장되는 FOV의 속도
    [SerializeField]
    private float expandingSpeed;

    // 확장되는 FOV용 메쉬 필터
    public MeshFilter expandingViewMeshFilter;
    public Mesh expandingViewMesh;
    public MeshRenderer expandingViewMeshRenderer;

    // FOV가 좌우로 움직일 수 있는 최대 각도
    public float patrolRange = 30f;

    // FOV가 움직이는 속도
    public float patrolSpeed = 2f;

    // 게임 시작 시 오브젝트의 원래 Y축 회전값
    private float originalRotationY;

    // FOV가 확장 중인지 여부
    private bool isFovExpanding = false;

    // 축소 지연 시간(2초)
    [SerializeField]
    private float shrinkingDelay;

    Transform childTransform;

    public Transform target;
    EnemyMaster em;

    NavMeshAgent nav;

    private void Start()
    {
        nav = GetComponentInParent<NavMeshAgent>();
        em = GetComponent<EnemyMaster>();
        fovMaterial = new Material(fovMaterial);

        // 매쉬 초기화
        viewMesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1f, 0f, 0f),   // 왼쪽 상단
            new Vector3(0f, 1f, 0f),    // 중앙 상단
            new Vector3(0f, 0f, 0f),    // 중앙점
            new Vector3(-1f, -1f, 0f),  // 왼쪽 하단
            new Vector3(0f, -1f, 0f)    // 중앙 하단
        };

        Vector2[] uv = new Vector2[]
        {
            // 첫 번째 쿼드의 UV 좌표
            new Vector2(0f, 0.5f), // 왼쪽 상단 꼭짓점
            new Vector2(0.5f, 1f), // 중앙 상단 꼭짓점
            new Vector2(0.5f, 0.5f), // 중앙점
            new Vector2(0f, 0f), // 왼쪽 하단 꼭짓점
            new Vector2(0.5f, 0f) // 중앙 하단 꼭짓점
        };

        int[] triangles = new int[]
        {
            // 첫 번째 쿼드를 구성하는 삼각형들
            0, 2, 1, // 상단 삼각형
            0, 3, 2, // 왼쪽 삼각형
            2, 3, 4, // 하단 삼각형
            1, 2, 4  // 오른쪽 삼각형
        };

        viewMesh.vertices = vertices;
        viewMesh.uv = uv;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();

        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;


        // 확장되는 FOV 메쉬 초기화
        expandingViewMesh = new Mesh();
        expandingViewMesh.name = "Expanding View Mesh";
        expandingViewMeshFilter.mesh = expandingViewMesh;


        if (viewMeshFilter.GetComponent<MeshRenderer>())
        {
            viewMeshFilter.GetComponent<MeshRenderer>().material = fovMaterial;
        }

        Color greenWithAlpha = new Color(0f, 1f, 0f, 0.3f);
        fovMaterial.color = greenWithAlpha;

        // 확장되는 FOV의 마테리얼을 초기화합니다.
        expandingFovMaterial = new Material(Shader.Find("Standard"));

        // 초기 색상을 노란색으로 설정
        Color yellowWithAlpha = new Color(1f, 1f, 0f, 0.3f);
        expandingFovMaterial.color = yellowWithAlpha;

        // 게임 시작 시 원래 시야각을 저장
        originalRotationY = transform.eulerAngles.y;

        childTransform = transform.Find("View");
         
        // MeshRenderer 컴포넌트 찾기
         fovMeshRenderer = childTransform.GetComponent<MeshRenderer>();

            FindMeshRenderer();

        // 지연 시간을 갖고 주기적으로 타겟 탐색
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    // 타겟 탐색 지연을 위한 코루틴
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    private void LateUpdate()
    {
        if (!searchPlayer && !fixOrPatrol && !findPlayer)
        {
            if (expandingRadius <= 0)
            {
                PatrolFOV();
            }
        }
        else if (searchPlayer && visibleTargets.Count > 0)
        {
            if (!fixOrPatrol)
            {
                RotateTowardsTarget(); // 타겟을 찾았을 때, 타겟을 바라보도록 회전
            }
            else if (fixOrPatrol && em.patrolCheck && !findPlayer)
            {
                nav.isStopped = true;
                RotateTowardsTarget();
            }
            else if(fixOrPatrol && em.patrolCheck && findPlayer)
            {
                nav.isStopped = false;
                RotateTowardsTarget();
            }
            else
            {
                RotateTowardsTarget(); // 타겟을 찾았을 때, 타겟을 바라보도록 회전
            }

        }
        
        if (!searchPlayer && !findPlayer && targetRun)
        {
            StartCoroutine(ShrinkFOVAfterDelay(shrinkingDelay)); // 축소 지연 시작
        }
        SearchFOV();

        DrawFieldOfView();
    }

    IEnumerator ShrinkFOVAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float expandSpeed = viewRadius / expandingSpeed; // 확장 속도를 조정합니다.
        expandingRadius -= Time.deltaTime * expandSpeed;
        DrawExpandingFieldOfView(); // 확장되는 FOV를 그립니다.

        if(expandingRadius <= 0)
        {
            expandingRadius = 0f; // 확장 반경을 초기화합니다.
            searchPlayer= false;
            targetRun=false;
            nav.isStopped = false;
        }
    }

    // 시야의 가장자리를 저장하는 구조체
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

    // 보이는 타겟을 찾는 함수
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        bool canCreateNewFOV = true; // 새로운 FOV 생성 가능 여부

        // 시야반경의 모든 콜라이더를 탐지
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            // 타겟 방향 계산
            target = targetsInViewRadius[i].transform;

            // 타겟이 시야각에 있는지 판단
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                // 타겟과의 거리
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                // 장애물에 막히는지 여부 판단
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    // PlayerStat 스크립트에서 canHide 값을 확인합니다.
                    PlayerStat playerStat = target.GetComponent<PlayerStat>();
                    if (playerStat != null && playerStat.canHide && dstToTarget >= viewRadius * 0.7f)
                    {
                        // 타겟이 숨을 수 있는 상태이며, viewRadius의 70% 거리에 있다면 새로운 FOV 생성을 막습니다.
                        viewHideRange = true;
                        canCreateNewFOV = false;
                    }
                    else
                    {
                        // 안막히면 타겟을 리스트에 추가
                        lastKnownPosition = target.position;
                        viewHideRange = false;
                        visibleTargets.Add(target);
                    }
                }
            }
        }

        if (visibleTargets.Count > 0)
        {
            searchPlayer = true;
            if (!findPlayer && canCreateNewFOV)
            {
                StartCoroutine(FindFieldOfView());
            }
        }
        else
        {
            searchPlayer = false;
            if (!searchPlayer && findPlayer)
            {
                // 타겟이 FOV 밖으로 이동하면 기존 FOV의 색상을 노란색으로 변경
                Color yellowWithAlpha = new Color(1f, 1f, 0f, 0.5f);
                fovMaterial.color = yellowWithAlpha;
            }
        }

        if(searchPlayer && findPlayer)
        {
            Color redWithAlpha = new Color(1f, 0f, 0f, 0.5f);
            fovMaterial.color = redWithAlpha; // 타겟을 찾으면 기존 FOV의 색상을 빨간색으로 변경합니다.

        }
    }

    IEnumerator FindFieldOfView()
    {
        if (isExpandingFOVRunning)
        {
            yield break; // 이미 실행 중이면 중복 실행 방지
        }

        SoundManager.soundManager.SEPlay(SEType.Warning);
        isExpandingFOVRunning = true;

        float expandSpeed = viewRadius / expandingSpeed; // 확장 속도를 조정합니다.

        while (searchPlayer && expandingRadius < viewRadius)
        {
            expandingRadius += Time.deltaTime * expandSpeed;
            DrawExpandingFieldOfView(); // 확장되는 FOV를 그립니다.

            // 확장되는 FOV가 타겟에 도달하는지 검사합니다.
            foreach (var target in visibleTargets)
            {
                if (Vector3.Distance(transform.position, target.position) <= expandingRadius)
                {
                    findPlayer = true;
                    viewHideRange = false;
                    expandingViewMesh.Clear();
                    Color redWithAlpha = new Color(1f, 0f, 0f, 0.5f);
                    fovMaterial.color = redWithAlpha; // 타겟을 찾으면 기존 FOV의 색상을 빨간색으로 변경합니다.
                    fixOrPatrol = true;
                    SoundManager.soundManager.SEPlay(SEType.Battle);
                    SoundManager.soundManager.PlayBGM(BGMType.Warning);
                    yield break;
                }
            }
            targetRun = true;
            yield return null;
        }
        if (!searchPlayer && findPlayer)
        {
            // 타겟이 기존 FOV 영역 밖으로 이동하면 색상을 노란색으로 변경합니다.
            fovMaterial.color = Color.yellow;

        }
        isExpandingFOVRunning = false; // 코루틴 종료 시 실행 상태 업데이트
    }


    // 시야 메쉬를 그리는 메서드
    void DrawFieldOfView()
    {
        // 시야 영역을 분할하여 그리기 위한 준비
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        // 각 스탭마다 시야를 캐스팅
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                // 이전 캐스팅과의 차이를 비교하여 가장자리 찾기
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }

                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            // 캐스팅 결과 저장
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // 시야 메쉬를 생성하기 위한 점과 삼각형 생성
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        // 시야 매쉬에 적용
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    // 시야의 가장자리를 찾고 계산하는 메서드
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }

        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    // 시야 캐스팅 정보 구조체
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        // 레이 캐스트로 시야 캐스팅 진행
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    // 각도를 벡터로 변환하는 함수
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // 시야 캐스팅 정보 구조체
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    void DrawExpandingFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ExpandingViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // FOV의 원점
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        expandingViewMesh.Clear();
        expandingViewMesh.vertices = vertices;
        expandingViewMesh.triangles = triangles;
        expandingViewMesh.RecalculateNormals();

        // 확장되는 FOV의 마테리얼 설정
        if (expandingViewMeshFilter.GetComponent<MeshRenderer>())
        {
            Debug.Log("확장");
            expandingViewMeshFilter.GetComponent<MeshRenderer>().material = expandingFovMaterial;
        }
    }

    ViewCastInfo ExpandingViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, expandingRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * expandingRadius, expandingRadius, globalAngle);
        }
    }

    // 발견시 오브젝트 회전
    void RotateTowardsTarget()
    {
        if (visibleTargets.Count > 0 && !viewHideRange)
        {

            //회전 지연시간
            float rotationSpeed = delayRotate;

            // 타겟 중 하나를 바라보도록 설정 (예시에서는 첫 번째 타겟을 사용)
            Vector3 directionToTarget = visibleTargets[0].position - transform.position;
            // Y축 회전 값만 계산하여 오브젝트가 수평면에서 타겟을 바라보도록 함
            float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

            // 회전값 전달
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            // 회전 지연
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // 순찰 시야
    void PatrolFOV()
    {
        float angle = Mathf.Sin(Time.time * patrolSpeed) * patrolRange;
        transform.rotation = Quaternion.Euler(0f, originalRotationY + angle, 0f);
    }

    public void VisibleFov()
    {
        fovMeshRenderer.enabled = true;
    }

    void SearchFOV()
    {
        if(fovMeshRenderer.enabled == false)
        {
            if(searchPlayer || findPlayer)
            {
                VisibleFov();
            }
        }
        else if(!searchPlayer && !findPlayer)
        {
            if(!em.onClickCheck)
            {
                StartCoroutine(InvisibleFov());
            }
        }
    }

    IEnumerator InvisibleFov()
    {
        // 두번째라디우스가 축소되는 것을 기다림
        yield return new WaitUntil(() => expandingRadius <= 0f);

        // FOV 메쉬 렌더러를 비활성화합니다.
        fovMeshRenderer.enabled = false;
    }

    private void FindMeshRenderer()
    {
        if (childTransform != null)
        {
            // MeshRenderer가 있다면 비활성화
            if (fovMeshRenderer != null)
            {
                fovMeshRenderer.enabled = false;
            }
        }
    }

}