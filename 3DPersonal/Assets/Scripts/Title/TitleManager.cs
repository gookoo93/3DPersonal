using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public GameObject MenuCanvas;
    public GameObject OptionCanvas;
    public GameObject GameQuit;

    private void Start()
    {
        if (GameQuit.activeSelf == true)
        {
            GameQuit.SetActive(false);
        }
        if (OptionCanvas.activeSelf == true)
        {
            OptionCanvas.SetActive(false);
        }

    }

    

    
    public void NewGame()
    {
        SceneManager.LoadScene("Stage0");
    }

    public void LoadGame()
    {


    }

    public void Option()
    {

        if (OptionCanvas.activeSelf == false)
        {
            SoundManager.soundManager.SEPlay(SEType.Click);
            OptionCanvas.SetActive(true);
        }
    }

    public void Quit()
    {
        if(GameQuit.activeSelf == false)
        {
            SoundManager.soundManager.SEPlay(SEType.Click);
            GameQuit.SetActive(true);
        }
    }

    public void CancleBTN()
    {
        SoundManager.soundManager.SEPlay(SEType.Click);
        OptionCanvas.SetActive(false);
    }

    public void ApplyBTN()
    {
        SoundManager.soundManager.SEPlay(SEType.Click);
        OptionCanvas.SetActive(false);
    }

    public void QuitYes()
    {
        SoundManager.soundManager.SEPlay(SEType.Click);
        Debug.Log("게임종료");
        Application.Quit();
    }

    public void QuitNo()
    {
        SoundManager.soundManager.SEPlay(SEType.Click);
        GameQuit.SetActive(false);
    }
}
