using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{ 
    public bool canSwim = false;     // 수영
    public bool canDown = false;      // 뛰어내리기
    public bool canWire = false;       // 와이어타고 오르기
    public bool canRope = false;      // 로프타고 건너가기
    public bool canClimb = false;     // 덩굴잡고 올라가기
    public bool canPush = false;       // 밀치기
    public bool canSteal = false;       // 훔치기
    public bool canJump = false;     // 건너뛰기 
    public bool canCarry = false;     // 오브젝트 들기 
    public bool canCuting = false;   // 오브젝트 자르기
    public bool canFallkill = false;     // 강습암살
    public bool canBreak = false;     // 오브젝트 부수기
    public bool canHealing = false;  // 회복시키기

    // 플레이어가 공격 가능한 대상에 관한 bool 변수
    [HideInInspector]public bool attackElite = false;    // 2단계 적 공격 가능여부
    [HideInInspector] public bool attackBoss = false;    // 3단계 적 공격 가능여부

    int carryingCount = 1;   // 오브젝트를 옮길 수 있는 수량
    int healingCount = 1;
}
