using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class EventManager : MonoBehaviour {

        
    public eventCtrl curEvent;

    public int maxEventNum;
    public int curEventNum=0;
    public ScreenFader screenFader;

    public UIManger UIMgr;



    private static EventManager s_Instance = null;

    public static EventManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance //= new GameManager();
                    = FindObjectOfType(typeof(EventManager)) as EventManager;
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Debug.LogError("Cannot have two instances of EventManager.");
            return;
        }
        s_Instance = this;
        //신넘어가도 유지시키는 함수
        //DontDestroyOnLoad(this);
        Debug.Log("EventManager Awake");

        screenFader=gameObject.GetComponent<ScreenFader>();
        maxEventNum = transform.childCount;
        curEvent = transform.GetChild(curEventNum).GetComponent<eventCtrl>();
    }

    void Start()
    {
        UIMgr = UIManger.Instance;
        StartEvent();
    }


    void Update()
    {
        if (curEvent.isFinish == true && curEventNum<(maxEventNum-1))
        {

            ChangeEvent();
        }

    }
    public void StartEvent()
    {
        UIMgr.ChangeEvent(curEvent.eEventState);
        curEvent.GetComponent<eventCtrl>().StartEvent();
    }
    public void ChangeEvent()
    {
        curEvent.transform.gameObject.SetActive(false);
        curEventNum++;
        curEvent = transform.GetChild(curEventNum).GetComponent<eventCtrl>();
        curEvent.transform.gameObject.SetActive(true);
        StartEvent();
    }


}


