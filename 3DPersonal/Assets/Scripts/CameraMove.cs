using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // �� �÷��̾� ���� ī�޶� �ʱ� ��ġ �߽���
    public Transform[] PlayerTransform;

    // ���� ������ ���� ĳ����
    private GameObject mainPlayer;

    // ī�޶� ��ġ
    public Transform cameraTransform;

    // �ʱ� ī�޶� ��ǥ / ȸ�� / �� ��
    private Vector3[] posOrigin;
    private Quaternion rotOrigin;
    private Vector3 zoomOrigin;

    // �� �� ��  �ܾƿ� ��ġ
    public Vector3 zoomAmount;

    // ī�޶� �̵��ӵ�
    public float cameraSpeed;

    // ī�޶� ���� (360�� ���� ����)
    public float rotationAmount;

    // ī�޶� ��ǥ / ȸ�� / �� ��
    private Vector3 cameraPos;
    private Quaternion cameraRot;
    private Vector3 cameraZoom;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


// ī�޶� ȸ��(Q/E , LAlt)
// ī�޶� Ȯ��(�ٵ�����)
// ī�޶� �̵�(����Ű , �� �翷���� �̵� , �� ��ư)
// ī�޶� �ʱ���ġ ����
// ī�޶� �ʱ���ġ �ҷ�����(W)

// ĳ���� ��ġ�� ī�޶� �̵�, 