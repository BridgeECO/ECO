using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECO;
using UnityEngine;

public class TempPlatformLine : MonoBehaviour
{
    public LineRenderer platformConnectLine;

    [SerializeField]
    private List<Platform> platforms;

    [SerializeField]
    private List<float> platformPlayingTimes;

    //연주와 관련된 코드들
    private float totalTime;
    private float currentTime;
    public bool isNowPlaying;

    private void Awake()
    {
        totalTime = 1000f;
        isNowPlaying = true;
        
        if(platforms.Count == 0)
        {
            Debug.LogError("플랫폼 마디 중에 플랫폼이 없는 마디가 있습니다");
            return;
        }

        if(platforms.Count != platformPlayingTimes.Count)
        {
            Debug.LogError("마디 중에 플랫폼 갯수와 연주 타이밍 지정 갯수가 맞지 않는 마디가 있습니다");
            return;
        }

        platformConnectLine.positionCount = platforms.Count;
        
        //우선 플랫폼들을 연결 및 플랫폼 보이게 하도록 변경 후 전부 비활성화
        for(int i = 0; i < platforms.Count; i++)
        {
            platformConnectLine.SetPosition(i, platforms[i].transform.position);
            platforms[i].GetComponent<ResonanceObject>().SetAlpha(0);
            platforms[i].GetComponent<ResonanceObject>()._boxCol.enabled = false;
        }

        StartCoroutine(PlayLineLoop());
    }

    private IEnumerator PlayLineLoop()
    {
        while(isNowPlaying)
        {
            bool endChecking = false;
            currentTime = 0;
            int platformCount = 0;

            while(true)
            {                
                currentTime += 200 * Time.deltaTime;
                Debug.Log(currentTime);

                if(!endChecking)
                {
                    if(platformPlayingTimes[platformCount] <= currentTime)
                    {
                        platforms[platformCount].PlayWwiseEvt();
                        StartCoroutine(platforms[platformCount]._resonanceObj.ShowingTemp());
                        platformCount++; 

                        if(platformCount >= platforms.Count)
                        {
                            endChecking = true;                                             
                        }
                    }
                }

                yield return null;

                if(currentTime >= totalTime)
                {
                    Debug.Log("End Loop Once");
                    break;
                }
            }
            yield return null;

        }
    }
}
