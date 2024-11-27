using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    [HideInInspector]
    public TextColorChange Instance;

    [HideInInspector]
    public bool select = false;
    [HideInInspector]
    public Text menuText;
    
    private void Start()
    {
        menuText = this.GetComponent<Text>(); 
        menuText.color = Color.white ;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menuText.color = Color.black;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (select == false)
        {
            menuText.color = Color.white;
        }
    }
}
