using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bush : MonoBehaviour
{
    bool deadBodyCheck = false;
    GameObject deadBody;

    private void Update()
    {
        if (deadBody != null && deadBody.transform.parent == null && deadBodyCheck)
        {
            StartCoroutine(DeadBodyDelete(1.0f));
        }
        else
        {
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.tag == "DeadBody")
        {
            deadBody = other.gameObject;
            deadBodyCheck = true;


        }
    }

    IEnumerator DeadBodyDelete(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        Destroy(deadBody);
        deadBodyCheck = false;
    }
}
