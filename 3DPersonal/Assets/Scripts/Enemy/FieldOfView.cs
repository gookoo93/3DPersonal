using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine("FindTargetsWithDelay", 0.2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i =0; i<targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward,dirToTarget)< viewAngle /2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position,dirToTarget,dstToTarget,obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngel(float angleINDegrees, bool angleIsGlobal)
    {
        if(!angleIsGlobal)
        {
            angleINDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleINDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleINDegrees * Mathf.Deg2Rad));
    }
}
