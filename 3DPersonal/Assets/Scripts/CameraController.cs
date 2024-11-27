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
    [HideInInspector]
    public bool camMove = true;

    // ���õ� �÷��̾� ĳ���� ���� ����
    [HideInInspector]
    public GameObject choicePlayer;

    // �÷��̾� ���� ����
    [HideInInspector]
    public GameObject beforePlayer = null;

    // Ŭ��ī��Ʈ
    private int clickCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ���� ī�޶� ������ �÷��̾�1 ĳ���͸� �߽����� ����
        transform.position = followTransform[0].position;
        choicePlayer = players[0];

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
        HandleZoomAndReset();
    }

    // ���콺 ���� �޼���
    void HandleMouseInput()
    {
        if(Input.mouseScrollDelta.y !=0)
        {
            newZoom -= Input.mouseScrollDelta.y * zoomAmount;
        }

        if(Input.GetMouseButtonDown(1))
        {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    // collider�� ���� �±� Ȯ��
                    if (hit.collider.CompareTag("Player"))
                {
                    beforePlayer = choicePlayer; // ������ ���õ� ������Ʈ�� ������Ʈ
                    // ���� ����: ���õ� ������Ʈ�� ������� �߰� �۾��� �����մϴ�.
                    if (clickCount == 1)
                    {
                        if (beforePlayer != choicePlayer)
                        {
                            // ���ο� ������Ʈ�� ���õǾ��� ��
                            choicePlayer = hit.collider.gameObject; // ���� ���õ� ������Ʈ�� ������Ʈ
                            Debug.Log(choicePlayer.name + " ���õ�");

                        }
                        else
                        {
                            // ������ ������Ʈ�� �ٽ� �������� ��: ī�޶� ��ġ�� ����
                            // ���õ� ������Ʈ�� players �迭 �� ��� �ε����� �ش��ϴ��� ã��
                            int index = System.Array.IndexOf(players, choicePlayer);
                            if (index != -1 && index < followTransform.Length)
                            {
                                newPosition = followTransform[index].transform.position; ;
                                Debug.Log(choicePlayer.name + " ��ġ�� ī�޶� �̵� ó��");
                            }
                            clickCount = 0; // Ŭ�� ī��Ʈ�� �ʱ�ȭ�Ͽ� ���� ���� �غ�
                        }
                    }
                    else
                    {
                        // ù ��° Ŭ���� �ƴ� ���(��, �ʱ� ���¿��� ������Ʈ�� �������� ��)
                        clickCount = 1;
                        choicePlayer = hit.collider.gameObject;

                        beforePlayer = choicePlayer; // beforePlayer�� choicePlayer�� ���� ���õ� ������Ʈ�� ����
                        Debug.Log(choicePlayer.name + " ���õ�");
                    }
                }

                   camMove = true;
                   // ī�޶� �̵� ����
                   Plane plane = new Plane(Vector3.up, Vector3.zero);
                   float entry;
                   if (plane.Raycast(ray, out entry))
                   {
                       dragStartPosition = ray.GetPoint(entry);
                   }
                }
        }

        if (Input.GetMouseButton(1) && camMove)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // "Ground"�� "Default" ���̾�� �����ϵ��� ���̾� ����ũ ����
            int layerMask = ~(LayerMask.GetMask("Player", "Enemy" , "Object", "DeadBody"));

            float entry;
             
            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                // ���̾� ����ũ�� �����Ͽ� ����ĳ��Ʈ
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    dragCurrentPosition = ray.GetPoint(entry); // ���� ����ĳ��Ʈ �浹 ������ ���
                }

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                // newPosition�� ����Ͽ� ������Ʈ ��ġ ������Ʈ ���� �߰� �۾��� ����
            }
        }

        // leftAlt Ű�� ������ ���� ��
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            camMove = false; // ī�޶� �̵��� ����

            if (rotateStartPosition == Vector3.zero)
            {
                rotateStartPosition = Input.mousePosition; // ȸ�� ���� ��ġ �ʱ�ȭ
            }
            else
            {
                rotateCurrentPosition = Input.mousePosition; // ���� ���콺 ��ġ ������Ʈ
                Vector3 difference = rotateStartPosition - rotateCurrentPosition; // �̵��� �Ÿ� ���
                rotateStartPosition = rotateCurrentPosition; // ���� ��ġ�� ���� ��ġ�� ������Ʈ

                newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f)); // ȸ�� ����
            }
        }
        else
        {
            camMove = true; // leftAlt Ű�� �������� ī�޶� �̵� ���
            rotateStartPosition = Vector3.zero; // ȸ�� ���� ��ġ �ʱ�ȭ
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
            //newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
            RotateCamera(90);
        }
        if (Input.GetKey(KeyCode.E))
        {
            //newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
            RotateCamera(-90);
        }
         
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTranform.localPosition = Vector3.Lerp(cameraTranform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    // ���ڹ�ư���� �÷��̾ �߽����� �̵�
    void HandleKeyInput()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // KeyCode.Alpha1�� 1�� Ű, i�� ���� ����Ű 1���� ������� ��Ī
            {
                if (followTransform[i] != null)
                {
                    if (choicePlayer != players[i])
                    {
                        // ���ο� �÷��̾ ������ ���
                        beforePlayer = choicePlayer; // ���� ���õ� �÷��̾� ������Ʈ
                        choicePlayer = players[i]; // ���ο� �÷��̾� ����
                        clickCount = 1; // Ŭ�� ī��Ʈ�� 1�� ����
                        Debug.Log(players[i].name + " ���õ�");
                    }
                    else if (clickCount == 1)
                    {
                        // ���� �÷��̾ �ٽ� ������ ���(ī�޶� �̵�)
                        newPosition = followTransform[i].position; // ī�޶��� �� ��ġ�� ���õ� �÷��̾��� ��ġ�� ����
                        Debug.Log(players[i].name + " ��ġ�� ī�޶� �̵� ó��");
                        clickCount = 0; // Ŭ�� ī��Ʈ �ʱ�ȭ
                    }
                    break; // �ش� Ű�� ���� ó���� ��ġ�� for ���� ����
                }
            }
        }
    }

    void RotateCamera(float angle)
    {
        float currentAngle = transform.eulerAngles.y;
        float adjustedAngle = 0;

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            // Left Alt ��� �� ���� ȸ��
            adjustedAngle = currentAngle + angle;
        }
        else
        {
            // Q �Ǵ� E Ű ��� �� 90�� ������ ����
            if (angle > 0) // Q�� ������ ���, ���� 90�� ������ ����
            {
                adjustedAngle = Mathf.Ceil((currentAngle + 1) / 90) * 90;
            }
            else if (angle < 0) // E�� ������ ���, ���� 90�� ������ ����
            {
                adjustedAngle = Mathf.Floor((currentAngle - 1) / 90) * 90;
            }
        }

        // 360�� �ʰ� �� ��ȯ ó��
        adjustedAngle = adjustedAngle % 360;
        if (adjustedAngle < 0) adjustedAngle += 360;

        newRotation = Quaternion.Euler(0, adjustedAngle, 0);
    }

    void HandleZoomAndReset()
    {
        // �� ���� ����
        newZoom.y = Mathf.Clamp(newZoom.y, 100, 500);
        newZoom.z = Mathf.Clamp(newZoom.z, -500, -100); // �� ���� �Ϲ������� �����Դϴ�.

        // W Ű�� ������ �⺻ �ܰ��� ȸ�������� ����
        if (Input.GetKeyDown(KeyCode.W))
        {
            newZoom = new Vector3(0, 200, -200); // �⺻ �ܰ����� ����
            newRotation = Quaternion.Euler(0, 45, 0); // �⺻ ȸ�������� ����
        }
    }
}
