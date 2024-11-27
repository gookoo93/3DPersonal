using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    public GameObject player;
    private PlayerStat ps;
    public Slider hp;

    private void Start()
    {

        player = this.gameObject;
        ps = player.GetComponent<PlayerStat>();
    }

    void Update()
    {
        if (hp != null)
        {
            hp.minValue = 0;
            hp.maxValue = ps.maxHp;
            hp.value = ps.hp;
        }
    }

}
