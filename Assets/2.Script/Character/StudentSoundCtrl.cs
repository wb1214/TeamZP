using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentSoundCtrl : MonoBehaviour
{
    public AudioClip[] idle;
    public AudioClip[] walk;
    public AudioClip[] run;
    private int walkIdx;
    public AudioClip[] attack;

    public AudioClip[] hit;
    public AudioClip[] scary;
    public AudioClip[] health;
    public AudioClip[] drink;
    public AudioClip[] death;




    public PhotonView pv;
    public AudioSource audio;
    public AudioSource audioBody;

    public float soundDelay;
    public float bodySoundDelay;
    public string nowSound;
    public string nowAniSound;

    public void PlaySound(string name)
    {

        if (soundDelay > 0) { return; }
        if (nowSound != name && nowSound != "")
        {
            audio.Stop();
            pv.RPC("Net_SoundOff", PhotonTargets.Others, "audio");
        }
        switch (name)
        {
            //case "None":
            //    if (audio.isPlaying)
            //    {
            //        audio.Stop();
            //    }
            //    break;
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                soundDelay = Random.Range(7.0f, 13.0f);
                audio.Play();
                break;
            case "Walk":
                Debug.Log("walk");
                audio.clip = walk[walkIdx];
                walkIdx++;
                soundDelay = 0.48f;
                audio.volume = 0.2f;
                if (walkIdx > 1)
                {
                    walkIdx = 0;
                }
                if (!audio.isPlaying)
                {
                    audio.Play();
                    pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                }
                break;
            case "Run":
                audio.volume = 1f;
                if (!audio.isPlaying)
                {
                    audio.clip = run[0];
                    audio.Play();
                    pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                }
                break;
            case "Attack":
                audio.clip = attack[Random.Range(0, attack.Length)];
                soundDelay = Random.Range(5.0f, 7.0f);
                audio.Play();
                break;
        }
        nowSound = name;
    }

    public void PlayBodySound(string name)
    {
        if (bodySoundDelay > 0 && name == "Scary") { return; }
        if (nowAniSound != name && nowAniSound != "")
        {
            audioBody.Stop();
            pv.RPC("Net_SoundOff", PhotonTargets.Others, "audioBody");

        }
        switch (name)
        {

            case "Hit": //Hit 는 사운드 딜레이 없음
                Debug.Log("is Hitted");
                int num = Random.Range(0, hit.Length);
                Debug.Log(num);
                if (audio.isPlaying)
                {
                    soundDelay = 1.0f;
                    audio.Stop();
                }

                nowAniSound = name;
                audioBody.clip = hit[num];
                audioBody.volume = 1f;
                audioBody.Play();
                break;
            case "Scary":
                audioBody.clip = scary[Random.Range(0, scary.Length)];
                audioBody.Play();
                bodySoundDelay = 3.5f;
                break;
            case "Health":
                audioBody.clip = health[Random.Range(0, health.Length)];
                audioBody.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Drink":
                audioBody.clip = drink[Random.Range(0, drink.Length)];
                audioBody.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
            case "Death":
                audioBody.clip = death[Random.Range(0, death.Length)];
                audioBody.Play();
                pv.RPC("Net_PlaySound", PhotonTargets.Others, name);
                break;
        }
        nowAniSound = name;
    }



    [PunRPC]
   public void Net_SoundOff(string name)
    {
        if (name == "audioBody")
        {
            audioBody.Stop();
        }
        else
        {
            audio.Stop();
        }
    }


    [PunRPC]
    void Net_PlaySound(string name)
    {

        switch (name)
        {
            case "None":
                if (audio.isPlaying)
                {
                    audio.Stop();
                }
                break;
            case "Idle":
                audio.clip = idle[Random.Range(0, idle.Length)];
                audio.Play();
                break;
            case "Walk":
                audio.clip = walk[walkIdx];
                walkIdx++;
                audio.volume = 0.8f;
                if (walkIdx > 1)
                {
                    walkIdx = 0;
                }
                if (!audio.isPlaying)
                {
                    audio.Play();
                }
                break;
            case "Run":
                audio.clip = run[0];
                audio.Play();

                break;
            case "Attack":
                audio.clip = attack[Random.Range(0, attack.Length)];
                audio.Play();
                break;
            case "Scary":
                audio.clip = scary[Random.Range(0, scary.Length)];
                audio.Play();
                break;
            case "Health":
                audioBody.clip = health[Random.Range(0, scary.Length)];
                audioBody.Play();
                break;
            case "Drink":
                audioBody.clip = drink[Random.Range(0, scary.Length)];
                audioBody.Play();
                break;
            case "Death":
                audioBody.clip = death[Random.Range(0, scary.Length)];
                audioBody.Play();
                break;
        }
    }



    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        audio = GetComponent<AudioSource>();
        audioBody = transform.Find("StudentBody").GetComponent<AudioSource>();
        walkIdx = 0;
        nowAniSound = "";
        nowSound = "";
    }

    // Start is called before the first frame update
    void Start()
    {

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
