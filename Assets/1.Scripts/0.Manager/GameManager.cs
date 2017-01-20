using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {


    public int stageNum;
    public bool isStageClear;

    public int userLife;
    public int userLifeMax;

    public bool isGameOver = false;
    
   // public Text lifeText;

    private static GameManager s_Instance = null;

    public static GameManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance //= new GameManager();
                    = FindObjectOfType(typeof(GameManager)) as GameManager;
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Debug.LogError("Cannot have two instances of GameManager.");
            return;
        }
        s_Instance = this;
        //신넘어가도 유지시키는 함수
        DontDestroyOnLoad(this);
        Debug.Log("GameManager Awake");
    }

    void Start()
    {
        userLife = userLifeMax;
        LifeSet();

    }


    void Update()
    {
        if (isGameOver)
            return;

        if (userLife <= 0)
            GameOver();
    }


    public void StageClear()
    {
        isStageClear = true;
        stageNum++;
    }

    public void LifePlus(int value)
    {
        userLife+=value;
        LifeSet();

    }
    public void LifeMinus(int value)
    {
        userLife-= value;
        LifeSet();

    }
    public void LifeSet()
    {
       // lifeText.text = "Life :" + userLife + "/" + userLifeMax;
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        isGameOver = true;
    }

}
