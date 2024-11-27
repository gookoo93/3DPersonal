using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoveObject : MonoBehaviour
{
    public GameObject climbObject; // 사용할 오브젝트
    public GameObject player; // 사용할 플레이어

    public Transform climbStart; // 오르기 시작 지점
    public Transform climbEnd; // 오르기 종료 지점

    protected bool playerNear = false; // 플레이어 근접 여부
    protected bool isClick = false; // 클릭 여부

    protected float climbSpeed = 3.0f; // 오르기 속도

    private Rigidbody playerRb;
    private NavMeshAgent playerNav;

    void Start()
    {
        climbObject = gameObject; // 현재 오브젝트를 사용 오브젝트로 설정
    }

    void Update()
    {
        if (playerNear && isClick)
        {
            StartCoroutine(ClimbRoutine()); // 클라이밍 코루틴 시작
        }
    }

    private IEnumerator ClimbRoutine()
    {
        isClick = false; // 중복 클릭 방지
        StartClimbing(); // 클라이밍 시작 처리

        // 플레이어가 사다리의 상단에 더 가까운지 하단에 더 가까운지 결정
        bool isClimbingUp = Vector3.Distance(player.transform.position, climbStart.position) > Vector3.Distance(player.transform.position, climbEnd.position);

        Vector3 start = isClimbingUp ? climbEnd.position : climbStart.position; // 실제 시작 지점
        Vector3 end = isClimbingUp ? climbStart.position : climbEnd.position; // 실제 종료 지점

        float climbDuration = Vector3.Distance(start, end) / climbSpeed;
        float startTime = Time.time;

        while (Time.time - startTime < climbDuration)
        {
            float fracJourney = (Time.time - startTime) / climbDuration;
            player.transform.position = Vector3.Lerp(start, end, fracJourney);
            yield return null;
        }

        player.transform.position = end; // 최종 위치 보정
        StopClimbing(); // 클라이밍 종료 처리
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
            player = other.gameObject;
            playerRb = player.GetComponent<Rigidbody>();
            playerNav = player.GetComponent<NavMeshAgent>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
            isClick = false; // 클릭 상태 초기화
        }
    }

    private void OnMouseDown()
    {
        if (playerNear) // 플레이어가 근접해 있을 때만 클릭을 허용
        {
            isClick = true;
        }
    }

    public void StartClimbing()
    {
        if (playerRb != null)
        {
            playerRb.useGravity = false; // 중력 영향 제거
            if (playerNav != null) playerNav.enabled = false; // NavMeshAgent 비활성화
        }
    }

    public void StopClimbing()
    {
        if (playerRb != null)
        {
            playerRb.useGravity = true; // 중력 다시 적용
            if (playerNav != null) playerNav.enabled = true; // NavMeshAgent 활성화
        }
    }
}