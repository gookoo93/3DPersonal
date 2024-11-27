using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


    public enum BGMType { None, Title, Normal, Warning}

    // SE ����
    public enum SEType
    {
       None, Warning, Battle, Attack, Damage, EnemyAttack, Walk, Run, HideWalk, Hide, Clear, LiftUp, LiftDown, Batto, Noto, Click
    }

public class SoundManager : MonoBehaviour
{
    public Slider bgmVolumeSlider;      // �ɼ� BGM �������� ��
    public Slider seVolumeSlider;       // �ɼ� SE ���� ���� ��

    private AudioSource bgmAudioSource;
    private AudioSource seAudioSource;

    public AudioClip bgmInTitle;        // BGM (Ÿ��Ʋ)
    public AudioClip bgmInNormal;      // BGM (�Ϲ�)
    public AudioClip bgmInWarning;        // BGM (�߰ߵ� �� ����)

    public AudioClip seWarning;         // SE �ǽɻ���
    public AudioClip seBattle;    // SE �߰���
    public AudioClip seAttack;         // SE ���� ����
    public AudioClip seDamage;               // SE �ǰ�
    public AudioClip seEnemyAttack;         // SE ���� ����
    public AudioClip seWalk;           // SE �ȱ�
    public AudioClip seRun;          // SE �ٱ�
    public AudioClip seHideWalk;           // SE ��� �ȱ�
    public AudioClip seLiftUp;           // SE ���
    public AudioClip seLiftDown;           // SE ������
    public AudioClip seBatto;           // SE ������
    public AudioClip seNoto;           // SE ������
    public AudioClip seClick;           // SE ������

    public AudioClip seHide;        // SE �÷��̾� ����
    public AudioClip seClear;           // SE Ŭ����

    // ù SoundManager�� ������ static ����
    public static SoundManager soundManager;

    //// ���� ��� ���� BGM
    public static BGMType playingBGM = BGMType.None;
    public static SEType playingSE = SEType.None;


    // Start is called before the first frame update
    private void Awake()
    {
        // BGM ���
        if (soundManager == null)
        {
            // static ������ �ڱ��ڽ��� ����
            soundManager = this;

            // Scene �� �̵��ص� ������Ʈ�� �ı����� ����
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // ������ ���ԵǾ� �ִٸ� ��� �ı�
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
                bgmAudioSource.clip = bgmInTitle;    // Ÿ��Ʋ BGM
            else if (type == BGMType.Normal)
                bgmAudioSource.clip = bgmInNormal;     // ���� BGM
            else if (type == BGMType.Warning)
                bgmAudioSource.clip = bgmInWarning;     // �ʵ� BGM

            bgmAudioSource.Play(); // ���� ���
        }
    }

    // BGM ����
    public void StopBGM()
    {
        GetComponent<AudioSource>().Stop();
        playingBGM = BGMType.None;
    }

    // SE ���
    public void SEPlay(SEType type)
    {
        // ���� ��� ���� SE�� ���� Ÿ���� SE�� ����Ϸ��� ���, ������� ����
        if (type == playingSE)
        {
            return;
        }

        // ��� ���� SE Ÿ�� ������Ʈ
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
        // ���õ� AudioClip�� �ִٸ� ���
        if (clipToPlay != null)
        {
            seAudioSource.PlayOneShot(clipToPlay);
        }
    }
    }
