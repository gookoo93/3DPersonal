using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puase : MonoBehaviour
{
    public PlayerMaster pm;
    bool pauseCheck = false;
    public GameObject pause;
    public GameObject soundMenu;
    public GameObject quitWindow;

    private void Awake()
    {
        pause.SetActive(false);
        soundMenu.SetActive(false);
        quitWindow.SetActive(false);
        SoundManager.soundManager.PlayBGM(BGMType.Normal);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!pauseCheck)
            {
                pm.controlAble = false;
                pause.SetActive(true);
                pauseCheck = true;
                DoSlowMotion();
            }
            else if(pauseCheck)
            {
                pm.controlAble = true;
                pause.SetActive(false);
                DoMotion();
                pauseCheck = false;

            }
        }
    }

   public void DoSlowMotion()
    {
        Time.timeScale = 0.0f;
        Time.fixedDeltaTime = Time.timeScale;
    }

    public void DoMotion()
    { 
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = Time.timeScale;
    }

    public void MenuReturn()
    {
        pause.SetActive(false);
        DoMotion();
        pauseCheck = false;
    }

    public void Quit()
    {
        pause.SetActive(false);
        quitWindow.SetActive(true);
    }

    public void QuitYes()
    {
        Application.Quit();
    }

    public void QuitNo()
    {
        pause.SetActive(true);
        quitWindow.SetActive(false);
    }
    public void SoundMenu()
    {
        soundMenu.SetActive(true);
        pause.SetActive(false);
    }
    public void SoundMenuNo()
    {
        soundMenu.SetActive(false);
        pause.SetActive(true);
    }
    public void SoundMenuYes()
    {
        soundMenu.SetActive(false);
        pause.SetActive(true);
    }
}
