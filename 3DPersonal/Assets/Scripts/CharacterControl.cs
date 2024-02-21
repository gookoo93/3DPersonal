using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private Behavior behavior; // ĳ������ �ൿ ��ũ��Ʈ
    private Camera mainCamera; // ���� ī�޶�
    private Vector3 targetPos; // ĳ������ �̵� Ÿ�� ��ġ

    void Start()
    {
        behavior = GetComponent<Behavior>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        // ���콺 �Է��� �޾� �� ��
        if (Input.GetMouseButtonUp(0))
        {
            // ���콺�� ���� ��ġ�� ��ǥ ���� �����´�
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f))
            {
                targetPos = hit.point;

                Debug.Log("�̵�");
            }
        }

        // ĳ���Ͱ� �����̰� �ִٸ�
        if (behavior.Run(targetPos))
        {
            // ȸ���� ���� �����ش�
            behavior.Turn(targetPos);
                Debug.Log("ȸ��");
        }
        else
        {
                Debug.Log("����");
            // ĳ���� �ִϸ��̼�(���� ����)
            //behavior.SetAnim(PlayerAnim.ANIM_IDLE);
        }

    }
}