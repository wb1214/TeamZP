using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AudioSource))]
public class csSoundManager : MonoBehaviour
{
    public static csSoundManager instance { get; private set; }

    public AudioClip[] soundFile;

    public float soundVolume = 1.0f;
    public bool isSoundMute = false;

    public Slider bgmSl;
    public Toggle bgmTg;
    
    public AudioSource audio;

    public GameObject sBar;
    bool Active = false;

    private void Awake()
    {
        if(instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        audio = GetComponent<AudioSource>();
        LoadSoundData();
    }

    private void Start()
    {
        soundVolume = bgmSl.value;
        isSoundMute = bgmTg.isOn;
        AudioSet();
    }

    public void OnSoundBtnClick()
    {
        if (!Active)
        {
            sBar.SetActive(true);
            Active = true;
        }
        else
        {
            SaveSoundData();
            sBar.SetActive(false);
            Active = false;
        }

    }

    public void SetSound()
    {
        soundVolume = bgmSl.value;
        isSoundMute = bgmTg.isOn;
        AudioSet();
    }
    void AudioSet()
    {
        audio.volume = soundVolume;
        audio.mute = isSoundMute;
    }

    public void PlayBgm(int sNum)
    {
        GetComponent<AudioSource>().clip = soundFile[sNum - 1];
        AudioSet();
        GetComponent<AudioSource>().Play();
    }

    public bool BgmCheck(int sNum)
    {
        if (GetComponent<AudioSource>().clip == soundFile[sNum - 1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void PlayChasingBgm(bool isActive)
    {
        if (isActive)
        {
            GetComponent<AudioSource>().clip = GetSfx("ChasingBgm");
            AudioSet();
            GetComponent<AudioSource>().Play();
        }
        else
        {
            GetComponent<AudioSource>().Stop();
        }

    }
    #region 사운드 저장 코드
    public void SaveSoundData()
    {
        PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        PlayerPrefs.SetInt("IsSoundMute", System.Convert.ToInt32(isSoundMute));
    }

    public void LoadSoundData()
    {
        bgmSl.value = PlayerPrefs.GetFloat("SoundVolume");
        bgmTg.isOn = System.Convert.ToBoolean(PlayerPrefs.GetInt("IsSoundMute"));

        int isSoundSave = PlayerPrefs.GetInt("IsSoundSave");

        if (isSoundSave == 0)
        {
            bgmSl.value = 1.0f;
            bgmTg.isOn = false;
            SaveSoundData();
            PlayerPrefs.SetInt("IsSoundSave", 1);
        }
    }


    #endregion
    public void PlayEffect(Vector3 pos, string name)
    {
      
        Debug.Log("vector :" + pos + " , " + name);
        AudioClip sfx = null;
        sfx = GetSfx(name);

        GameObject _soundObj = new GameObject("sfx");


        AudioSource _audioSource = _soundObj.AddComponent<AudioSource>();
        _audioSource.clip = sfx;
        _audioSource.volume = 1;
        _audioSource.minDistance = 3.0f;
        _audioSource.spatialBlend = 1;
        _audioSource.maxDistance = 8.0f;

        _audioSource.playOnAwake = true;
        Instantiate(_soundObj, pos, Quaternion.identity);

        Destroy(_soundObj, sfx.length + 0.2f);
        
    }

    AudioClip GetSfx(string name)
    {
        AudioClip sfx = null;
        for (int i = 0; i < soundFile.Length; i++)
        {
            if (soundFile[i].name.Contains(name))
            {
                sfx = soundFile[i];

            }
        }
        return sfx;
    }
}
