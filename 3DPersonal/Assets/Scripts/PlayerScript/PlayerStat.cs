using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public bool canMove = false;
    public bool canRun = false;
    public bool canHide = false;
    public bool canSwim = false;     // 수영
    public bool canDown = false;      // 뛰어내리기
    public bool canWire = false;       // 와이어타고 오르기
    public bool canRope = false;      // 로프타고 건너가기
    public bool canClimb = false;     // 덩굴잡고 올라가기
    public bool canPush = false;       // 밀치기
    public bool canSteal = false;       // 훔치기
    public bool canJump = false;     // 건너뛰기 
    public bool canCarry = false;     // 오브젝트 들기 
    public bool canUp = false;   // 오브젝트 자르기
    public bool canFallkill = false;     // 강습암살
    public bool canBreak = false;     // 오브젝트 부수기
    public bool canHealing = false;  // 회복시키기
    public bool canAttack = false;  // 공격

    // 플레이어가 공격 가능한 대상에 관한 bool 변수
    public bool attackElite = false;    // 2단계 적 공격 가능여부

    public int carryingCount = 1;   // 오브젝트를 옮길 수 있는 수량
    public int healingCount = 1;

    public int maxHp = 3;                     // 최대 체력
    public int hp = 3;                            // 현재 체력
    public float moveSpeed = 5.0f;     // 움직이는 속도
    public float range = 3.0f;               // 공격사거리
    public float sound = 1.0f;              // 소음도

    public int power = 1;


}
