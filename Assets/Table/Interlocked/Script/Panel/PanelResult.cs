using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanelResult : MonoBehaviour {

    public GameObject winTxtObj;
    public GameObject loseTxtObj;
    public Button nextBtn;
    public Button retryBtn;
    public Button mainmenuBtn;
    public Text besttime;
    public Text beststep;

    gamemanager1 gameMgr;

    private void Awake()
    {

        nextBtn.onClick.AddListener(()=> { onNextLevelBtn(); });
        retryBtn.onClick.AddListener(() => { OnRetryBtn(); });
        mainmenuBtn.onClick.AddListener(() => { OnMainMenuBtn(); });

        gameMgr = GameObject.Find("gamemanager1").GetComponent<gamemanager1>();
        besttime = GameObject.Find("besttime").GetComponent<Text>();
        beststep = GameObject.Find("beststep").GetComponent<Text>();


    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //进行下一关
    void onNextLevelBtn()
    {
        gameMgr.NextLevel();
    }

    //再来一次
    void OnRetryBtn()
    {
        gameMgr.RetryLevel();
    }

    void OnMainMenuBtn()
    {
        gameMgr.OpenLevelSelectPanel();
    }

    //设置比赛结果相关信息显示
    public void MatchResult(bool win)
    {

        //nextBtn.gameObject.SetActive(win);
        //retryBtn.gameObject.SetActive(win);
        string level_key = "level";    //主题名+下划线作为值键
        if (win)
        {
            
            PlayerPrefs.SetInt(level_key,gameMgr.levelId+1);  //保存当前解锁的关卡编号,实际是当前关卡的下一关
         //   print(gamemanager1.step_text.text+" "+gameMgr.time_text.text);
            string time = gameMgr.time_text.text;
            int minte = int.Parse(time.Substring(0, 2));
            int second = int.Parse(time.Substring(5,2));
            int currentSeconds = minte * 60 + second;
            string r_time = PlayerPrefs.GetString(level_key+ gameMgr.levelId + "time");
            int r_minte = int.Parse(r_time.Substring(0, 2));
            int r_second = int.Parse(r_time.Substring(5, 2));
            int recordSeconds = r_minte * 60 + r_second;
            if (currentSeconds < recordSeconds)
            {
                PlayerPrefs.SetString(level_key+ gameMgr.levelId + "time",time);
            }

            int recordStep = PlayerPrefs.GetInt(level_key+ gameMgr.levelId + "step");
            if (recordStep <= 0 || recordStep > gamemanager1.steps)
            {
                PlayerPrefs.SetInt(level_key+ gameMgr.levelId + "step", gamemanager1.steps);
            }
         


        }
        beststep.text = PlayerPrefs.GetInt(level_key + gameMgr.levelId + "step") + "  步";
        besttime.text = PlayerPrefs.GetString(level_key + gameMgr.levelId + "time");
    }
}
