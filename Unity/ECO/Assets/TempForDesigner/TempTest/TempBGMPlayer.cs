using System.Collections.Generic;
using System.Linq;
using ECO;
using UnityEngine;

public class TempBGMPlayer : MonoBehaviour
{
    [SerializeField]
    private List<AkEvent> bgmSounds;

    public List<bool> isBgmOns;

    [SerializeField]
    private AkEvent originalBgmSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bgmSounds = GetComponentsInChildren<AkEvent>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
