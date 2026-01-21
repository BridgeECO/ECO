using System.Collections.Generic;
using System.Linq;
using ECO;
using UnityEngine;

public class TempBGMPlayer : MonoBehaviour
{
    [SerializeField]
    private List<AudioSource> bgmSounds;

    public List<bool> isBgmOns;
    
    //배경음악이 언제든 켜져도 전체 음악에 어색하지 않고 바로 들어갈 수 있도록, 
    // 오디오를 껐다 키는 대신 음소거여부를 조절하는 방식으로 대응
    void Start()
    {
        //배경음악 오브젝트들 전부 등록해서 일단은 음소거시킴
        bgmSounds = GetComponentsInChildren<AudioSource>().ToList();
        foreach (AudioSource audioSource in bgmSounds)
        {
            audioSource.mute = true;
            //audioSource.Play();

            //그리고 각 오디오 소스 당 하나씩 isBgmOns에 bool을 등록
            isBgmOns.Add(false);
        }


    }

    // 여기서는, 만약 isBgmOns가 True 값이라면, 거기에 대응되는 AudioSource의 볼륨을 원상복귀시킴
    void Update()
    {
        for(int i=0; i < isBgmOns.Count; i++)
        {
            // mute 상태 업데이트
            if (bgmSounds[i].mute != !isBgmOns[i])
            {
                bgmSounds[i].mute = !isBgmOns[i];
            }
        }
    }
}
