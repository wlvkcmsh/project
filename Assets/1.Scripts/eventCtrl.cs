using UnityEngine;
using System.Collections;

public enum EventState
{
    eAction,
    eNormal,
    eBoss,
    eOpening,
    eEnding,
    eScore

}



public class eventCtrl : MonoBehaviour {


    public EventState eEventState;


    public bool isFinish = false;

    public GameObject CurCamera;
    // Use this for initialization

    public void Start()
    {

        if (CurCamera==null &&( transform.GetChild(0).name=="Camera"))
            CurCamera = transform.GetChild(0).gameObject;
        
    }
    public void StartEvent()
    {
        gameObject.GetComponent<Animator>().SetTrigger("StartEvent");
        //fade in &out setting
        EventManager.Instance.screenFader.fadeIn = true;
    }

    public void EndEvent()
    {
        //fade out 시작
        EventManager.Instance.screenFader.fadeIn = false;
    }
}
