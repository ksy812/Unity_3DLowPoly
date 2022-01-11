using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name; //곡 이름
    public AudioClip clip; //곡
}

public class SoundManager : MonoBehaviour
{
    //싱글턴. sington. 1개
    static public SoundManager instance;
    #region singleton
    void Awake() //객체 생성시 최초 실행
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject); //this 굳이 없어도 됨
    }
    #endregion singleton

    public AudioSource[] audioSourceEffects; //이펙트는 한 번에 여러 개 실행 될 수 있으니까!!
    public AudioSource audioSourceBgm; //브금은 당연히 한 번에 하나 실행임

    public string[] playSoundName;

    public Sound[] effectSounds;
    public Sound[] bgmSounds;

    void Start()
    {
        playSoundName = new string[audioSourceEffects.Length];
    }


    public void PlaySE(string _name)
    {
        for (int i = 0; i < effectSounds.Length; i++)
        {
            if(_name == effectSounds[i].name)
            {
                for (int j = 0; j < audioSourceEffects.Length; j++)
                {
                    if (!audioSourceEffects[j].isPlaying)
                    {
                        playSoundName[j] = effectSounds[i].name; //재생 중인 오디오 소스와 이름 일치시킴
                        audioSourceEffects[j].clip = effectSounds[i].clip;
                        audioSourceEffects[j].Play();
                        return; //재생 시켰으니까 더이상 for문 돌릴 이유Xx
                    }
                }//End For
                Debug.Log("모든 가용 AudioSource가 사용중입니다.");
                return;
            }//End If
        }//End For
        Debug.Log(_name + "사운드가 SoundManager에 등록되지 않았습니다.");
    }
    public void StopAllSE()
    {
        for (int i = 0; i < audioSourceEffects.Length; i++)
        {
            audioSourceEffects[i].Stop();
        }
    }
    public void StopSE(string _name)
    {
        for (int i = 0; i < audioSourceEffects.Length; i++)
        {
            if(playSoundName[i] == _name)
            {
                audioSourceEffects[i].Stop();
                return;
            }
        }
        Debug.Log("재생 중인" + _name + "사운드가 없습니다.");
    }
}
