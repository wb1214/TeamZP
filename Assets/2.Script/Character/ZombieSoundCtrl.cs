using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSoundCtrl : MonoBehaviour
{
    public AudioClip[] walk;
    public AudioClip[] dash;
    public AudioClip[] idle;
    public AudioClip[] attack;
    public AudioClip[] dashAttack;
    public AudioClip[] hit;
    public AudioClip[] bite;
    public AudioClip transition;
    public PhotonView pv;
    public AudioSource audio;
    public AudioSource bodyAudio;

    public float soundDelay;
    public float bodySoundDelay;
    public string nowSound;
    public string nowBodySound;

    private int walkIdx;

    public void PlaySound(string name)
    {//1회성 소리 재생 메서드
        if (soundDelay != 0 && name == "Idle") return;
        if (nowSound != name && nowSound != "") { audio.Stop(); pv.RPC("Net_SoundOff", PhotonTargets.Others, "audio"); }

        switch (name)
        {
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                soundDelay = Random.Range(7.0f, 13.0f);
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Attack":
                nowSound = name;
                audio.clip = attack[Random.Range(0, attack.Length)];
                soundDelay = Random.Range(5.0f, 7.0f);
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "DashAttack":
                nowSound = name;
                audio.clip = dashAttack[Random.Range(0, dashAttack.Length)];
                soundDelay = Random.Range(5.0f, 7.0f);
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Bite"://Bite 는 사운드 딜레이 없음
                nowSound = name;
                audio.clip = bite[Random.Range(0, bite.Length)];
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Transition":
                nowSound = name;
                audio.clip = transition;
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Hit":
                nowSound = name;
                audio.clip = hit[Random.Range(0, hit.Length)];
                audio.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
        }
        nowSound = name;
    }

    public void PlayBodySound(string name, bool isOff = false)
    { //발소리같은 지속적인 소리 재생 메서드
        if (isOff) { bodyAudio.Stop(); pv.RPC("Net_PlayBodySound", PhotonTargets.Others, "None"); return; }
        if ((bodySoundDelay != 0 || name == "None") && name != "Dash") return;
        if (nowBodySound != name && nowSound != "") { bodyAudio.Stop(); pv.RPC("Net_SoundOff", PhotonTargets.Others, "bodyAudio"); }


        switch (name)
        {
            case "Walk":
                nowBodySound = name;
                bodyAudio.clip = walk[walkIdx++];
                bodySoundDelay = 0.4f;
                bodyAudio.Play();
                pv.RPC("Net_PlayBodySound", PhotonTargets.Others, name);
                if (walkIdx > 1) walkIdx = 0;
                break;
            case "Dash":
                Debug.Log("Dash sound ");
                nowBodySound = name;
                bodyAudio.clip = dash[0];
                bodySoundDelay = 3;
                bodyAudio.Play();
                pv.RPC("Net_PlayBodySound", PhotonTargets.Others, name);

                break;
        }
    }

    [PunRPC]
    public void Net_PlaySound(string name)
    {
        switch (name)
        {
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                audio.Play();
                break;
            case "Attack":
                audio.clip = attack[Random.Range(0, attack.Length)];
                audio.Play();
                break;
            case "DashAttack":
                audio.clip = dashAttack[Random.Range(0, dashAttack.Length)];
                audio.Play();
                break;
            case "Bite":
                audio.clip = bite[Random.Range(0, bite.Length)];
                audio.Play();
                break;
            case "Hit":
                audio.clip = hit[Random.Range(0, hit.Length)];
                audio.Play();
                break;
        }
    }

    [PunRPC]
    public void Net_PlayBodySound(string name)
    {
        switch (name)
        {
            case "Walk":
                bodyAudio.clip = walk[walkIdx++];
                bodyAudio.Play();
                if (walkIdx > 1) walkIdx = 0;
                break;
            case "Dash":
                bodyAudio.clip = dash[0];
                bodyAudio.Play();
                break;
            case "None":
                bodyAudio.Stop();
                break;
        }
    }

    [PunRPC]
    public void Net_SoundOff(string name)
    {
        if (name == "bodyAudio")
        {
            bodyAudio.Stop();
        }
        else
        {
            audio.Stop();
        }
    }
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        audio = GetComponent<AudioSource>();
        bodyAudio = transform.Find("ZombieBody").GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        bodySoundDelay = 0;
        soundDelay = 0;
        walkIdx = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (soundDelay >= 0f)
        {
            //Debug.Log("Left Delay" + soundDelay);
            soundDelay -= Time.deltaTime;
        }
        else
        {
            soundDelay = 0;
        }

        if (bodySoundDelay >= 0f)
        {
            //Debug.Log("Left Delay" + soundDelay);
            bodySoundDelay -= Time.deltaTime;
        }
        else
        {
            bodySoundDelay = 0;
        }
    }
}
