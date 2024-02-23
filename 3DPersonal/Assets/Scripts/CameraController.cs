using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // �÷��̾� �� �⺻ ī�޶��� �߽����� �޾ƿ� �ν��Ͻ�
    public static CameraController instance;

    // �÷��̾ ������ Ʈ������
    public Transform[] followTransform;

    // �⺻ ī�޶� Ʈ������
    public Transform cameraTranform;

    // �÷��̾� ������Ʈ��
    public GameObject[] players;

    // ī�޶� �̵��ӵ�
    public float movementSpeed;

    // ī�޶� �̵��ϴ� �ð� ���
    public float movementTime;

    // ī�޶� ȸ�� ��ġ
    public float rotationAmount;

    // ī�޶� �� ��ġ
    public Vector3 zoomAmount;

    // ����Ǵ� ��ġ�� ���� ����
    public Vector3 newPosition;

    // ����Ǵ� ȸ���� ���� ����
    public Quaternion newRotation;

    // ����Ǵ� Ȯ�밪 ���� ����
    public Vector3 newZoom;

    // ���콺 �巡�� ���� ���� (���콺�� �� �̵���)
    public Vector3 dragStartPosition;

    // ���콺 �巡�� ���� ���� (���콺�� �� �̵���)
    public Vector3 dragCurrentPosition;

    // ȸ�� ���� ���� (ȸ������)
    public Vector3 rotateStartPosition;

    // ȸ�� ���� ���� (ȸ������)
    public Vector3 rotateCurrentPosition;

    // ȸ���� �� �̵� ���� �Է� ������ bool ����
    private bool camMove = true;

    // ���õ� �÷��̾� ĳ���� ���� ����
    public GameObject choicePlayer;

    // Start is called before the first frame update
    void Start()
    {
        // ī�޶��� �ν��Ͻ�
        instance = this;    

        // ���� ī�޶� ������ �÷��̾�1 ĳ���͸� �߽����� ����
        transform.position = followTransform[0].position;

        // ī�޶��� ��ġ, ȸ����, �ܰ��� ����
        newPosition = transform.position;
        newRotation= transform.rotation;
        newZoom = cameraTranform.localPosition;

    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        HandleMovementInput();
        HandleKeyInput();
    }

    // ���콺 ���� �޼���
    void HandleMouseInput()
    {
        if(Input.mouseScrollDelta.y !=0)
        {
            newZoom -= Input.mouseScrollDelta.y * zoomAmount;
        }

        if(Input.GetMouseButtonDown(1) && camMove)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if(plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(1) && camMove)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }

        if(Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            camMove= false;
            rotateStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            camMove= false;
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }

        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Debug.Log("ķ���� Ʈ��");
            camMove = true;
        }
    }

    // ī�޶� ���� �޼���
    void HandleMovementInput()
    {
        // ī�޶� �̵�
        if(Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
        }
        
        // ī�޶� ȸ��
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
         
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTranform.localPosition = Vector3.Lerp(cameraTranform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    // ���ڹ�ư���� �÷��̾ �߽����� �̵�
    void HandleKeyInput()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            if (followTransform[0] != null)
            {
                choicePlayer = players[0];
                newPosition = followTransform[0].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (followTransform[1] != null)
            {
                choicePlayer = players[1];
                newPosition= followTransform[1].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha3))
        {
            if (followTransform[2] != null)
            {
                choicePlayer = players[2];
                newPosition = followTransform[2].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha4))
        {
            if (followTransform[3] != null)
            {
                choicePlayer= players[3];
                newPosition = followTransform[3].position;
            }
        }
        if (Input.GetKey(KeyCode.Alpha5))
        {
            if (followTransform[4] != null)
            {
                choicePlayer= players[4];
                newPosition = followTransform[4].position;
            }
        }
    }
}
