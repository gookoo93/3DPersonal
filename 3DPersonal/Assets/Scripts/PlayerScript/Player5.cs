using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player5 : PlayerMaster
{
    protected override void Start()
    {
        base.Start();

        canMove = false;
        canRun = false;        // �ٱ�
        canHide = false;       // ����
        canSwim = true;     // ����
        canDown = true;      // �پ����
        canWire = true;       // ���̾�Ÿ�� ������
        canRope = true;      // ����Ÿ�� �ǳʰ���
        canClimb = true;     // ������� �ö󰡱�
        canPush = true;       // ��ġ��
        canSteal = false;       // ��ġ��
        canJump = true;     // �ǳʶٱ� 
        canCarry = false;     // ������Ʈ ��� 
        canCuting = true;   // ������Ʈ �ڸ���
        canFallkill = false;     // �����ϻ�
        canBreak = false;     // ������Ʈ �μ���
        canHealing = true;  // ȸ����Ű��

        // �÷��̾ ���� ������ ��� ���� bool ����
        attackElite = false;    // 2�ܰ� �� ���� ���ɿ���
        attackBoss = false;    // 3�ܰ� �� ���� ���ɿ���

        carryingCount = 1;   // ������Ʈ�� �ű� �� �ִ� ����

        maxHp = 6;                     // �ִ� ü��
        hp = 6;                            // ���� ü��
        moveSpeed = 5.0f;     // �����̴� �ӵ�
        range = 1.0f;               // ���ݻ�Ÿ�
        sound = 1.0f;              // ������   
    }

    // Update is called once per frame
    void Update()
    {
    }
}
