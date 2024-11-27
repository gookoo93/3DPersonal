using UnityEngine;
using UnityEngine.UI;

public class CharaUI : MonoBehaviour
{
    Image chara;

    public Sprite char1;
    public Sprite char2;
    public Sprite char3;

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.soundManager.PlayBGM(BGMType.Normal);
        chara = GetComponent<Image>();

    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangePlayer1();
        }
       if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangePlayer2();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangePlayer3();
        }
    }

    public void ChangePlayer1()
    {
        chara.sprite = char1;
    }
    public void ChangePlayer2()
    {
        chara.sprite = char2;
    }
    public void ChangePlayer3()
    {
        chara.sprite = char3;
    }
}
