using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior : MonoBehaviour
{
    public float speed = 2.0f;
    private Rigidbody rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public bool Run(Vector3 targetPos)
    {
        float dis = Vector3.Distance(transform.position, targetPos);
        if (dis >= 0.01f)
        {
            Vector3 newPosition = Vector3.MoveTowards(rigid.position, targetPos, speed * Time.deltaTime);
            rigid.MovePosition(newPosition);
            return true;
        }
        return false;
    }

    public void Turn(Vector3 targetPos)
    {
        // 캐릭터를 이동하고자 하는 좌표값 방향으로 회전시킨다
        Vector3 dir = targetPos - transform.position;
        Vector3 dirXZ = new Vector3(dir.x, 0f, dir.z);
        Quaternion targetRot = Quaternion.LookRotation(dirXZ);
        rigid.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 550.0f * Time.deltaTime);
    }
}