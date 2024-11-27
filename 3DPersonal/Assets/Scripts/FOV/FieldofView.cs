using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FieldofView : MonoBehaviour
{

    // �������̳� �������̳��� ����(�������� �¿�� �þ�Ȯ��, �������� ���� / false�� ���� / true�� patrol )
    public bool fixOrPatrol;
    // Player Ž�� ���� Ȯ��
    public bool searchPlayer = false;

    // Player Ž�� �Ϸ�
    public bool findPlayer = false;

    // Ÿ�� ���� ����
    bool targetRun = false;

    // ���� ���� ���� �ִ��� ����
    bool viewHideRange = false;

    // �ڷ�ƾ �ݺ� ���� ����
    private bool isExpandingFOVRunning = false;

    // Ÿ���� ������ �˷��� ��ġ�� ����
    public Vector3 lastKnownPosition = Vector3.zero;

    // �þ߹���
    public float viewRadius;
    [Range(0, 360)]

    // �þ߰�
    public float viewAngle;

    // FOV Ÿ�� ���̾�
    public LayerMask targetMask;

    // ȸ�� �����ð�
    [SerializeField]
    public float delayRotate;

    // �þ߸� ���� ��ü�� ���̾�
    public LayerMask obstacleMask;

    // ���̴� Ÿ���� ����Ʈ�� ����
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    // �Ž��� �󸶳� �������
    public float meshResolution;

    // �����ڸ� �ݺ����
    public int edgeResolveIterations;

    // �ݺ���� �Ӱ谪
    public float edgeDstThreshold;

    // �þ��� ����
    public MeshFilter viewMeshFilter;

    int SoundCount = 0;
    // ���� �Ž�
    public Mesh viewMesh;

    // FOV�� Material ����
    public Material fovMaterial;

    public MeshRenderer fovMeshRenderer;
    
    // Ȯ��Ǵ� FOV�� ���� ���׸���
    private Material expandingFovMaterial;


    // Ȯ��Ǵ� FOV�� ���� �ݰ�
    private float expandingRadius = 0f;

    // Ȯ��Ǵ� FOV�� �ӵ�
    [SerializeField]
    private float expandingSpeed;

    // Ȯ��Ǵ� FOV�� �޽� ����
    public MeshFilter expandingViewMeshFilter;
    public Mesh expandingViewMesh;
    public MeshRenderer expandingViewMeshRenderer;

    // FOV�� �¿�� ������ �� �ִ� �ִ� ����
    public float patrolRange = 30f;

    // FOV�� �����̴� �ӵ�
    public float patrolSpeed = 2f;

    // ���� ���� �� ������Ʈ�� ���� Y�� ȸ����
    private float originalRotationY;

    // FOV�� Ȯ�� ������ ����
    private bool isFovExpanding = false;

    // ��� ���� �ð�(2��)
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

        // �Ž� �ʱ�ȭ
        viewMesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1f, 0f, 0f),   // ���� ���
            new Vector3(0f, 1f, 0f),    // �߾� ���
            new Vector3(0f, 0f, 0f),    // �߾���
            new Vector3(-1f, -1f, 0f),  // ���� �ϴ�
            new Vector3(0f, -1f, 0f)    // �߾� �ϴ�
        };

        Vector2[] uv = new Vector2[]
        {
            // ù ��° ������ UV ��ǥ
            new Vector2(0f, 0.5f), // ���� ��� ������
            new Vector2(0.5f, 1f), // �߾� ��� ������
            new Vector2(0.5f, 0.5f), // �߾���
            new Vector2(0f, 0f), // ���� �ϴ� ������
            new Vector2(0.5f, 0f) // �߾� �ϴ� ������
        };

        int[] triangles = new int[]
        {
            // ù ��° ���带 �����ϴ� �ﰢ����
            0, 2, 1, // ��� �ﰢ��
            0, 3, 2, // ���� �ﰢ��
            2, 3, 4, // �ϴ� �ﰢ��
            1, 2, 4  // ������ �ﰢ��
        };

        viewMesh.vertices = vertices;
        viewMesh.uv = uv;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();

        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;


        // Ȯ��Ǵ� FOV �޽� �ʱ�ȭ
        expandingViewMesh = new Mesh();
        expandingViewMesh.name = "Expanding View Mesh";
        expandingViewMeshFilter.mesh = expandingViewMesh;


        if (viewMeshFilter.GetComponent<MeshRenderer>())
        {
            viewMeshFilter.GetComponent<MeshRenderer>().material = fovMaterial;
        }

        Color greenWithAlpha = new Color(0f, 1f, 0f, 0.3f);
        fovMaterial.color = greenWithAlpha;

        // Ȯ��Ǵ� FOV�� ���׸����� �ʱ�ȭ�մϴ�.
        expandingFovMaterial = new Material(Shader.Find("Standard"));

        // �ʱ� ������ ��������� ����
        Color yellowWithAlpha = new Color(1f, 1f, 0f, 0.3f);
        expandingFovMaterial.color = yellowWithAlpha;

        // ���� ���� �� ���� �þ߰��� ����
        originalRotationY = transform.eulerAngles.y;

        childTransform = transform.Find("View");
         
        // MeshRenderer ������Ʈ ã��
         fovMeshRenderer = childTransform.GetComponent<MeshRenderer>();

            FindMeshRenderer();

        // ���� �ð��� ���� �ֱ������� Ÿ�� Ž��
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    // Ÿ�� Ž�� ������ ���� �ڷ�ƾ
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
                RotateTowardsTarget(); // Ÿ���� ã���� ��, Ÿ���� �ٶ󺸵��� ȸ��
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
                RotateTowardsTarget(); // Ÿ���� ã���� ��, Ÿ���� �ٶ󺸵��� ȸ��
            }

        }
        
        if (!searchPlayer && !findPlayer && targetRun)
        {
            StartCoroutine(ShrinkFOVAfterDelay(shrinkingDelay)); // ��� ���� ����
        }
        SearchFOV();

        DrawFieldOfView();
    }

    IEnumerator ShrinkFOVAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float expandSpeed = viewRadius / expandingSpeed; // Ȯ�� �ӵ��� �����մϴ�.
        expandingRadius -= Time.deltaTime * expandSpeed;
        DrawExpandingFieldOfView(); // Ȯ��Ǵ� FOV�� �׸��ϴ�.

        if(expandingRadius <= 0)
        {
            expandingRadius = 0f; // Ȯ�� �ݰ��� �ʱ�ȭ�մϴ�.
            searchPlayer= false;
            targetRun=false;
            nav.isStopped = false;
        }
    }

    // �þ��� �����ڸ��� �����ϴ� ����ü
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

    // ���̴� Ÿ���� ã�� �Լ�
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        bool canCreateNewFOV = true; // ���ο� FOV ���� ���� ����

        // �þ߹ݰ��� ��� �ݶ��̴��� Ž��
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            // Ÿ�� ���� ���
            target = targetsInViewRadius[i].transform;

            // Ÿ���� �þ߰��� �ִ��� �Ǵ�
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                // Ÿ�ٰ��� �Ÿ�
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                // ��ֹ��� �������� ���� �Ǵ�
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    // PlayerStat ��ũ��Ʈ���� canHide ���� Ȯ���մϴ�.
                    PlayerStat playerStat = target.GetComponent<PlayerStat>();
                    if (playerStat != null && playerStat.canHide && dstToTarget >= viewRadius * 0.7f)
                    {
                        // Ÿ���� ���� �� �ִ� �����̸�, viewRadius�� 70% �Ÿ��� �ִٸ� ���ο� FOV ������ �����ϴ�.
                        viewHideRange = true;
                        canCreateNewFOV = false;
                    }
                    else
                    {
                        // �ȸ����� Ÿ���� ����Ʈ�� �߰�
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
                // Ÿ���� FOV ������ �̵��ϸ� ���� FOV�� ������ ��������� ����
                Color yellowWithAlpha = new Color(1f, 1f, 0f, 0.5f);
                fovMaterial.color = yellowWithAlpha;
            }
        }

        if(searchPlayer && findPlayer)
        {
            Color redWithAlpha = new Color(1f, 0f, 0f, 0.5f);
            fovMaterial.color = redWithAlpha; // Ÿ���� ã���� ���� FOV�� ������ ���������� �����մϴ�.

        }
    }

    IEnumerator FindFieldOfView()
    {
        if (isExpandingFOVRunning)
        {
            yield break; // �̹� ���� ���̸� �ߺ� ���� ����
        }

        SoundManager.soundManager.SEPlay(SEType.Warning);
        isExpandingFOVRunning = true;

        float expandSpeed = viewRadius / expandingSpeed; // Ȯ�� �ӵ��� �����մϴ�.

        while (searchPlayer && expandingRadius < viewRadius)
        {
            expandingRadius += Time.deltaTime * expandSpeed;
            DrawExpandingFieldOfView(); // Ȯ��Ǵ� FOV�� �׸��ϴ�.

            // Ȯ��Ǵ� FOV�� Ÿ�ٿ� �����ϴ��� �˻��մϴ�.
            foreach (var target in visibleTargets)
            {
                if (Vector3.Distance(transform.position, target.position) <= expandingRadius)
                {
                    findPlayer = true;
                    viewHideRange = false;
                    expandingViewMesh.Clear();
                    Color redWithAlpha = new Color(1f, 0f, 0f, 0.5f);
                    fovMaterial.color = redWithAlpha; // Ÿ���� ã���� ���� FOV�� ������ ���������� �����մϴ�.
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
            // Ÿ���� ���� FOV ���� ������ �̵��ϸ� ������ ��������� �����մϴ�.
            fovMaterial.color = Color.yellow;

        }
        isExpandingFOVRunning = false; // �ڷ�ƾ ���� �� ���� ���� ������Ʈ
    }


    // �þ� �޽��� �׸��� �޼���
    void DrawFieldOfView()
    {
        // �þ� ������ �����Ͽ� �׸��� ���� �غ�
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        // �� ���Ǹ��� �þ߸� ĳ����
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                // ���� ĳ���ð��� ���̸� ���Ͽ� �����ڸ� ã��
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

            // ĳ���� ��� ����
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        // �þ� �޽��� �����ϱ� ���� ���� �ﰢ�� ����
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

        // �þ� �Ž��� ����
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    // �þ��� �����ڸ��� ã�� ����ϴ� �޼���
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

    // �þ� ĳ���� ���� ����ü
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        // ���� ĳ��Ʈ�� �þ� ĳ���� ����
        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    // ������ ���ͷ� ��ȯ�ϴ� �Լ�
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // �þ� ĳ���� ���� ����ü
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

        vertices[0] = Vector3.zero; // FOV�� ����
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

        // Ȯ��Ǵ� FOV�� ���׸��� ����
        if (expandingViewMeshFilter.GetComponent<MeshRenderer>())
        {
            Debug.Log("Ȯ��");
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

    // �߽߰� ������Ʈ ȸ��
    void RotateTowardsTarget()
    {
        if (visibleTargets.Count > 0 && !viewHideRange)
        {

            //ȸ�� �����ð�
            float rotationSpeed = delayRotate;

            // Ÿ�� �� �ϳ��� �ٶ󺸵��� ���� (���ÿ����� ù ��° Ÿ���� ���)
            Vector3 directionToTarget = visibleTargets[0].position - transform.position;
            // Y�� ȸ�� ���� ����Ͽ� ������Ʈ�� ����鿡�� Ÿ���� �ٶ󺸵��� ��
            float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;

            // ȸ���� ����
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            // ȸ�� ����
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // ���� �þ�
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
        // �ι�°���콺�� ��ҵǴ� ���� ��ٸ�
        yield return new WaitUntil(() => expandingRadius <= 0f);

        // FOV �޽� �������� ��Ȱ��ȭ�մϴ�.
        fovMeshRenderer.enabled = false;
    }

    private void FindMeshRenderer()
    {
        if (childTransform != null)
        {
            // MeshRenderer�� �ִٸ� ��Ȱ��ȭ
            if (fovMeshRenderer != null)
            {
                fovMeshRenderer.enabled = false;
            }
        }
    }

}