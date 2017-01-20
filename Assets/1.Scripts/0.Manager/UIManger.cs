using UnityEngine;
using System.Collections;

public class UIManger : MonoBehaviour
{
    private static UIManger s_Instance = null;



    public const int userNum = 2;
    EventManager eventMgr;


    public GameObject[] goCrossHairs = new GameObject[userNum];
    public GameObject[] goScores = new GameObject[userNum];
    public GameObject goCockpit;
    bool onCrossHair;
    bool onScore;
    bool onCockpit;


    public Camera camUI;//왼쪽 카메라 받음

    public static UIManger Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance //= new GameManager();
                    = FindObjectOfType(typeof(UIManger)) as UIManger;
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Debug.LogError("Cannot have two instances of UIManger.");
            return;
        }
        s_Instance = this;
        //신넘어가도 유지시키는 함수
        DontDestroyOnLoad(this);
        Debug.Log("UIManger Awake");


        //set UI objects

        for (int i = 0; i < userNum; i++)
        {
            goCrossHairs[i] = transform.GetChild(0).GetChild(i).gameObject;
            goScores[i] = transform.GetChild(1).GetChild(i).gameObject;
        }
        goCockpit = transform.GetChild(2).gameObject;
        //왼쪽카메라를 받아옴
        camUI = transform.GetChild(3).GetChild(1).GetComponent<Camera>();

    }

    void Start()
    {
        eventMgr = EventManager.Instance;
    }


    void Update()
    {
        if (onCrossHair)
            moveCrossHairs();


        SetTransForm();
    }



    void moveCrossHairs()
    {
        //수정 해야됨 현재는 마우스 포지션으로 이동함
        goCrossHairs[0].transform.position = camUI.ScreenToWorldPoint(Input.mousePosition) + camUI.transform.forward;
    }

    public void ChangeEvent(EventState eEventState)
    {
        //Debug.Log("바뀌어라 이벤트여"+eEventState);
        switch (eEventState)
        {
            case EventState.eAction:
                callEventUI(true, true, true);
                break;
            case EventState.eScore:
                break;
            case EventState.eNormal:
                callEventUI(false, true, true);
                break;
            case EventState.eOpening:
                callEventUI(false, false, false);
                break;
            case EventState.eEnding:
                break;
            default:
                break;

        }

    }
    void callEventUI(bool _onCrossHair,bool _onScore,bool _onCockpit)
    {
        for (int i = 0; i < userNum; i++)
        {
            goCrossHairs[i].SetActive(_onCrossHair);
            goScores[i].SetActive(_onScore);
        }
        goCockpit.SetActive(_onCockpit);


        this.onCrossHair = _onCrossHair;
        this.onScore = _onScore;
        this.onCockpit = _onCockpit;



    }

    void SetTransForm()
    {
        //현재 이벤트의 카메라의 트랜스폼을 받아옴
        transform.position = eventMgr.curEvent.CurCamera.transform.position;
        transform.rotation = eventMgr.curEvent.CurCamera.transform.rotation;
        transform.position += transform.forward ;
    }


}
