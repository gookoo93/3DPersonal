using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player5 : PlayerMaster
{
    protected override void Start()
    {
        base.Start();

        canMove = false;
        canRun = false;        // 뛰기
        canHide = false;       // 숨기
        canSwim = true;     // 수영
        canDown = true;      // 뛰어내리기
        canWire = true;       // 와이어타고 오르기
        canRope = true;      // 로프타고 건너가기
        canClimb = true;     // 덩굴잡고 올라가기
        canPush = true;       // 밀치기
        canSteal = false;       // 훔치기
        canJump = true;     // 건너뛰기 
        canCarry = false;     // 오브젝트 들기 
        canCuting = true;   // 오브젝트 자르기
        canFallkill = false;     // 강습암살
        canBreak = false;     // 오브젝트 부수기
        canHealing = true;  // 회복시키기

        // 플레이어가 공격 가능한 대상에 관한 bool 변수
        attackElite = false;    // 2단계 적 공격 가능여부
        attackBoss = false;    // 3단계 적 공격 가능여부

        carryingCount = 1;   // 오브젝트를 옮길 수 있는 수량

        maxHp = 6;                     // 최대 체력
        hp = 6;                            // 현재 체력
        moveSpeed = 5.0f;     // 움직이는 속도
        range = 1.0f;               // 공격사거리
        sound = 1.0f;              // 소음도   
    }

    // Update is called once per frame
    void Update()
    {
    }
}
