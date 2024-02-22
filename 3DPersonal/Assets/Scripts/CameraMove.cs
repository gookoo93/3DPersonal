using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform[] playerObjects; // �÷��̾� ������Ʈ �迭
    private Transform mainPlayer; // ���� �÷��̾�

    private Vector3 originalPosition; // ī�޶��� �ʱ� ��ġ
    private Quaternion originalRotation; // ī�޶��� �ʱ� ȸ����

    private float originalZoom; // ī�޶��� �ʱ� �� ��
    private float zoomLevel = 10f; // ���� �� ����

    [SerializeField]
    private float moveSpeed = 10f; // �̵� �ӵ��� ������Ŵ
    [SerializeField]
    private float rotationSpeed = 90f; // ī�޶� ȸ�� �ӵ�
    [SerializeField]
    private int rotateMouse = 5; // ���콺 �̵���

    void Start()
    {
        mainPlayer = playerObjects[0]; // ù ��° ������Ʈ�� ���� �÷��̾�� ����
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalZoom = Camera.main.fieldOfView;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, 90);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            transform.RotateAround(Vector3.zero, Vector3.up, -90);
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetAxis("Mouse X") < 0)
        {
            RotateCamera(rotateMouse * - 1); // ȸ�� ������ ���� ����
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetAxis("Mouse X") > 0)
        {
            RotateCamera(rotateMouse); // ȸ�� ������ ���� ����

        }

        if (Input.GetMouseButton(2)) // ���콺 �� ��ư
        {
            MoveCamera();
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            ZoomCamera(Input.mouseScrollDelta.y);
        }

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                ChangeMainPlayer(i);
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ResetCamera();
        }
    }

    // ���콺�� �̵��ϴ� ī�޶� rotate
    void RotateCamera(int direction)
    {
        transform.RotateAround(Vector3.zero, Vector3.up, rotationSpeed * direction * Time.deltaTime);
    }

    void MoveCamera()
    {
        var moveX = Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
        var moveY = Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;
        transform.Translate(new Vector3(moveX, 0, moveY), Space.World);
    }

    void ZoomCamera(float increment)
    {
        zoomLevel -= increment;
        Camera.main.fieldOfView = Mathf.Clamp(zoomLevel, 5f, originalZoom);
    }

    void ChangeMainPlayer(int index)
    {
        if (index >= 0 && index < playerObjects.Length)
        {
            mainPlayer = playerObjects[index];
            transform.position = mainPlayer.position - transform.forward * 10f; // ī�޶� ���� �÷��̾� �ڷ� �̵�
        }
    }

    void ResetCamera()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        Camera.main.fieldOfView = originalZoom;
    }
}


// ī�޶� ȸ��(Q/E , LAlt)
// ī�޶� Ȯ��(�ٵ�����)
// ī�޶� �̵�(����Ű , �� �翷���� �̵� , �� ��ư)
// ī�޶� �ʱ���ġ ����
// ī�޶� �ʱ���ġ �ҷ�����(W)

// ĳ���� ��ġ�� ī�޶� �̵�, 