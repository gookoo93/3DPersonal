using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


    public enum BGMType { None, Title, Normal, Warning}

    // SE 종류
    public enum SEType
    {
       None, Warning, Battle, Attack, Damage, EnemyAttack, Walk, Run, HideWalk, Hide, Clear, LiftUp, LiftDown, Batto, Noto, Click
    }

public class SoundManager : MonoBehaviour
{
    public Slider bgmVolumeSlider;      // 옵션 BGM 볼륨조절 바
    public Slider seVolumeSlider;       // 옵션 SE 볼륨 조절 바

    private AudioSource bgmAudioSource;
    private AudioSource seAudioSource;

    public AudioClip bgmInTitle;        // BGM (타이틀)
    public AudioClip bgmInNormal;      // BGM (일반)
    public AudioClip bgmInWarning;        // BGM (발견된 후 전투)

    public AudioClip seWarning;         // SE 의심상태
    public AudioClip seBattle;    // SE 발각됨
    public AudioClip seAttack;         // SE 내가 공격
    public AudioClip seDamage;               // SE 피격
    public AudioClip seEnemyAttack;         // SE 적이 공격
    public AudioClip seWalk;           // SE 걷기
    public AudioClip seRun;          // SE 뛰기
    public AudioClip seHideWalk;           // SE 숨어서 걷기
    public AudioClip seLiftUp;           // SE 들기
    public AudioClip seLiftDown;           // SE 내리기
    public AudioClip seBatto;           // SE 내리기
    public AudioClip seNoto;           // SE 내리기
    public AudioClip seClick;           // SE 내리기

    public AudioClip seHide;        // SE 플레이어 공격
    public AudioClip seClear;           // SE 클리어

    // 첫 SoundManager를 저장할 static 변수
    public static SoundManager soundManager;

    //// 현재 재생 중인 BGM
    public static BGMType playingBGM = BGMType.None;
    public static SEType playingSE = SEType.None;


    // Start is called before the first frame update
    private void Awake()
    {
        // BGM 재생
        if (soundManager == null)
        {
            // static 변수에 자기자신을 저장
            soundManager = this;

            // Scene 이 이동해도 오브젝트를 파기하지 않음
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 정보가 삽입되어 있다면 즉시 파기
            Destroy(gameObject);
        }
    }
    void Start()
    {
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        seAudioSource = gameObject.AddComponent<AudioSource>();

        float savedBgmVolume = PlayerPrefs.GetFloat("BgmVolume", 1.0f);
        float savedSeVolume = PlayerPrefs.GetFloat("SeVolume", 1.0f);

        SetBgmVolume(savedBgmVolume);
        SetSeVolume(savedSeVolume);

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = savedBgmVolume;
            bgmVolumeSlider.onValueChanged.AddListener(ChangeBgmVolume);
        }

        if (seVolumeSlider != null)
        {
            seVolumeSlider.value = savedSeVolume;
            seVolumeSlider.onValueChanged.AddListener(ChangeSeVolume);
        }
        soundManager.PlayBGM(BGMType.Title);
    }

    void ChangeBgmVolume(float volume)
    {
        SetBgmVolume(volume);
    }

    void ChangeSeVolume(float volume)
    {
        SetSeVolume(volume);
    }

    void SetBgmVolume(float volume)
    {
        bgmAudioSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("BgmVolume", bgmAudioSource.volume);
    }

    void SetSeVolume(float volume)
    {
        seAudioSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SeVolume", seAudioSource.volume);
    }

    ///////////////////////////////////////////

    public void PlayBGM(BGMType type)
    {
        if (type != playingBGM)
        {
            playingBGM = type;

            if (bgmAudioSource.isPlaying) bgmAudioSource.Stop();

            if (type == BGMType.Title)
                bgmAudioSource.clip = bgmInTitle;    // 타이틀 BGM
            else if (type == BGMType.Normal)
                bgmAudioSource.clip = bgmInNormal;     // 마을 BGM
            else if (type == BGMType.Warning)
                bgmAudioSource.clip = bgmInWarning;     // 필드 BGM

            bgmAudioSource.Play(); // 사운드 재생
        }
    }

    // BGM 정지
    public void StopBGM()
    {
        GetComponent<AudioSource>().Stop();
        playingBGM = BGMType.None;
    }

    // SE 재생
    public void SEPlay(SEType type)
    {
        // 현재 재생 중인 SE와 같은 타입의 SE를 재생하려는 경우, 재생하지 않음
        if (type == playingSE)
        {
            return;
        }

        // 재생 중인 SE 타입 업데이트
        playingSE = type;

        AudioClip clipToPlay = null;
        switch (type)
        {
            case SEType.Warning:
                clipToPlay = seWarning;
                break;
            case SEType.Battle:
                clipToPlay = seBattle;
                break;
            case SEType.Attack:
                clipToPlay = seAttack;
                break;
            case SEType.Damage:
                clipToPlay = seDamage;
                break;
            case SEType.EnemyAttack:
                clipToPlay = seEnemyAttack;
                break;
            case SEType.Walk:
                clipToPlay = seWalk;
                break;
            case SEType.HideWalk:
                clipToPlay = seHideWalk;
                break;
            case SEType.Run:
                clipToPlay = seRun;
                break;
            case SEType.Hide:
                clipToPlay = seHide;
                break;
            case SEType.Clear:
                clipToPlay = seClear;
                break;
            case SEType.LiftUp:
                clipToPlay = seLiftUp;
                break;
            case SEType.LiftDown:
                clipToPlay = seLiftDown;
                break;
            case SEType.Batto:
                clipToPlay = seBatto;
                break;
            case SEType.Noto:
                clipToPlay = seNoto;
                break;
            case SEType.Click:
                clipToPlay = seClick;
                break;
        }
        // 선택된 AudioClip이 있다면 재생
        if (clipToPlay != null)
        {
            seAudioSource.PlayOneShot(clipToPlay);
        }
    }
    }
