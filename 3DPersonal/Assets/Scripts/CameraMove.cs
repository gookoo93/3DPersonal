using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // 각 플레이어 별로 카메라 초기 위치 중심점
    public Transform[] PlayerTransform;

    // 현재 설정된 메인 캐릭터
    private GameObject mainPlayer;

    // 카메라 위치
    public Transform cameraTransform;

    // 초기 카메라 좌표 / 회전 / 줌 값
    private Vector3[] posOrigin;
    private Quaternion rotOrigin;
    private Vector3 zoomOrigin;

    // 줌 인 ·  줌아웃 수치
    public Vector3 zoomAmount;

    // 카메라 이동속도
    public float cameraSpeed;

    // 카메라 각도 (360도 기준 각도)
    public float rotationAmount;

    // 카메라 좌표 / 회전 / 줌 값
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


// 카메라 회전(Q/E , LAlt)
// 카메라 확대(휠돌리기)
// 카메라 이동(조작키 , 맵 양옆으로 이동 , 휠 버튼)
// 카메라 초기위치 저장
// 카메라 초기위치 불러오기(W)

// 캐릭터 위치로 카메라 이동, 