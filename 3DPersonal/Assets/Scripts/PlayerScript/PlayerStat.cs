using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{ 
    public bool canSwim = false;     // ����
    public bool canDown = false;      // �پ����
    public bool canWire = false;       // ���̾�Ÿ�� ������
    public bool canRope = false;      // ����Ÿ�� �ǳʰ���
    public bool canClimb = false;     // ������� �ö󰡱�
    public bool canPush = false;       // ��ġ��
    public bool canSteal = false;       // ��ġ��
    public bool canJump = false;     // �ǳʶٱ� 
    public bool canCarry = false;     // ������Ʈ ��� 
    public bool canCuting = false;   // ������Ʈ �ڸ���
    public bool canFallkill = false;     // �����ϻ�
    public bool canBreak = false;     // ������Ʈ �μ���
    public bool canHealing = false;  // ȸ����Ű��

    // �÷��̾ ���� ������ ��� ���� bool ����
    [HideInInspector]public bool attackElite = false;    // 2�ܰ� �� ���� ���ɿ���
    [HideInInspector] public bool attackBoss = false;    // 3�ܰ� �� ���� ���ɿ���

    int carryingCount = 1;   // ������Ʈ�� �ű� �� �ִ� ����
    int healingCount = 1;
}
