using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MoveObject : MonoBehaviour
{
    public GameObject climbObject; // ����� ������Ʈ
    public GameObject player; // ����� �÷��̾�

    public Transform climbStart; // ������ ���� ����
    public Transform climbEnd; // ������ ���� ����

    protected bool playerNear = false; // �÷��̾� ���� ����
    protected bool isClick = false; // Ŭ�� ����

    protected float climbSpeed = 3.0f; // ������ �ӵ�

    private Rigidbody playerRb;
    private NavMeshAgent playerNav;

    void Start()
    {
        climbObject = gameObject; // ���� ������Ʈ�� ��� ������Ʈ�� ����
    }

    void Update()
    {
        if (playerNear && isClick)
        {
            StartCoroutine(ClimbRoutine()); // Ŭ���̹� �ڷ�ƾ ����
        }
    }

    private IEnumerator ClimbRoutine()
    {
        isClick = false; // �ߺ� Ŭ�� ����
        StartClimbing(); // Ŭ���̹� ���� ó��

        // �÷��̾ ��ٸ��� ��ܿ� �� ������� �ϴܿ� �� ������� ����
        bool isClimbingUp = Vector3.Distance(player.transform.position, climbStart.position) > Vector3.Distance(player.transform.position, climbEnd.position);

        Vector3 start = isClimbingUp ? climbEnd.position : climbStart.position; // ���� ���� ����
        Vector3 end = isClimbingUp ? climbStart.position : climbEnd.position; // ���� ���� ����

        float climbDuration = Vector3.Distance(start, end) / climbSpeed;
        float startTime = Time.time;

        while (Time.time - startTime < climbDuration)
        {
            float fracJourney = (Time.time - startTime) / climbDuration;
            player.transform.position = Vector3.Lerp(start, end, fracJourney);
            yield return null;
        }

        player.transform.position = end; // ���� ��ġ ����
        StopClimbing(); // Ŭ���̹� ���� ó��
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
            isClick = false; // Ŭ�� ���� �ʱ�ȭ
        }
    }

    private void OnMouseDown()
    {
        if (playerNear) // �÷��̾ ������ ���� ���� Ŭ���� ���
        {
            isClick = true;
        }
    }

    public void StartClimbing()
    {
        if (playerRb != null)
        {
            playerRb.useGravity = false; // �߷� ���� ����
            if (playerNav != null) playerNav.enabled = false; // NavMeshAgent ��Ȱ��ȭ
        }
    }

    public void StopClimbing()
    {
        if (playerRb != null)
        {
            playerRb.useGravity = true; // �߷� �ٽ� ����
            if (playerNav != null) playerNav.enabled = true; // NavMeshAgent Ȱ��ȭ
        }
    }
}