using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class FindPlayerAlarm : MonoBehaviour
{
    public GameObject[] enemys;
    [HideInInspector]
    public FieldofView[] fovs;
    [HideInInspector]
    public NavMeshAgent[] agents;
    [HideInInspector]
    public EnemyMaster[] em;
    [HideInInspector]
    public Vector3 alarmPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        fovs = new FieldofView[enemys.Length];
        agents = new NavMeshAgent[enemys.Length];
        em = new EnemyMaster[enemys.Length];

        for (int i = 0; i < enemys.Length; i++)
        {
            
            fovs[i] = enemys[i].GetComponent<FieldofView>();
            agents[i] = enemys[i].GetComponentInParent<NavMeshAgent>();
            em[i] = enemys[i].GetComponent<EnemyMaster>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i =0; i< fovs.Length; i++)
        {
            if(fovs[i].findPlayer == true)
            {
                alarmPosition = fovs[i].lastKnownPosition;

                for(int j = 0; j < enemys.Length; j++)
                {
                    fovs[j].findPlayer= true;
                    fovs[j].fixOrPatrol= true;
                    em[j].patrolCheck= true;
                    agents[j].isStopped = false;
                    fovs[j].lastKnownPosition = alarmPosition;
                    agents[j].SetDestination(alarmPosition);
                }

                break;
            }
        }
    }
}
