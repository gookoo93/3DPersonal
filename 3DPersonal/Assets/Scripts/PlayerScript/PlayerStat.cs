using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public bool canMove = false;
    public bool canRun = false;
    public bool canHide = false;
    public bool canSwim = false;     // ����
    public bool canDown = false;      // �پ����
    public bool canWire = false;       // ���̾�Ÿ�� ������
    public bool canRope = false;      // ����Ÿ�� �ǳʰ���
    public bool canClimb = false;     // ������� �ö󰡱�
    public bool canPush = false;       // ��ġ��
    public bool canSteal = false;       // ��ġ��
    public bool canJump = false;     // �ǳʶٱ� 
    public bool canCarry = false;     // ������Ʈ ��� 
    public bool canUp = false;   // ������Ʈ �ڸ���
    public bool canFallkill = false;     // �����ϻ�
    public bool canBreak = false;     // ������Ʈ �μ���
    public bool canHealing = false;  // ȸ����Ű��
    public bool canAttack = false;  // ����

    // �÷��̾ ���� ������ ��� ���� bool ����
    public bool attackElite = false;    // 2�ܰ� �� ���� ���ɿ���

    public int carryingCount = 1;   // ������Ʈ�� �ű� �� �ִ� ����
    public int healingCount = 1;

    public int maxHp = 3;                     // �ִ� ü��
    public int hp = 3;                            // ���� ü��
    public float moveSpeed = 5.0f;     // �����̴� �ӵ�
    public float range = 3.0f;               // ���ݻ�Ÿ�
    public float sound = 1.0f;              // ������

    public int power = 1;


}
