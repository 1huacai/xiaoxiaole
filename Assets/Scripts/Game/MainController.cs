using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class MainController : GameController
{
    private Animator _prePareAnim; // 准备开始动画
    private Text _timer; // 倒计时
    private Button _upBtn; // 方块上升一行按钮
    private Button _setupBtn; // 设置按钮
    private GameObject _resultObj; // 结果UI
    private Text _resultInfoText; // 结果信息
    private GameObject _mainScoreObj; // 单人模式得分板
    private Text _scoreText; // 得分显示文本
    private GameObject _rivalObj; // 对手展示区
    private GameObject _comingSoonObj;
    private Tweener _comingSoonTw;

    void Awake()
    {
        _controller = this;
        Config.msgHandler._mainControllerInst = this;
    }

    void Start()
    {
        // unity component init
        _prePareAnim = GameObject.Find(Config.preparePath).GetComponent<Animator>();
        _prePareAnim.gameObject.SetActive(true);

        _timer = GameObject.Find(Config.timerTextPath).GetComponent<Text>();
        _timer.gameObject.SetActive(false);
        
        _upBtn = GameObject.Find(Config.upButtonPath).GetComponent<Button>();
        _upBtn.onClick.AddListener(OnUpBtnClick);

        _setupBtn = GameObject.Find(Config.setupButtonPath).GetComponent<Button>();
        _setupBtn.onClick.AddListener(OnOtherBtnClick);

        _resultObj = GameObject.Find(Config.resultPath);
        _resultInfoText = GameObject.Find(Config.resultInfoPath).GetComponent<Text>();

        _mainScoreObj = GameObject.Find(Config.mainScorePath);
        _mainScoreObj.SetActive(!_multiPlayer);

        _scoreText = GameObject.Find(Config.mainScoreTextPath).GetComponent<Text>();

        _rivalObj = GameObject.Find(Config.rivalShowPath);
        _rivalObj.SetActive(_multiPlayer);

        // state handler init        
        InitStateHandlers();

        // variable init     
        InitMembers();

        // initial block init
        InitBlocks();

        // 删除初始生成的空行
        DestroyBlankRow();
    }

    void Update()
    {
        if (_gameOver)
            return;

        if (_gameReady)
        {
            var stateInfo = _prePareAnim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                _prePareAnim.gameObject.SetActive(false);
                _timer.gameObject.SetActive(true);
                _gameReady = false;
                if (!_multiPlayer)
                    _gameStart = true;
            }
            return;
        }

        if (!_gameStart)
            return;

        if (_curRowCnt > Config.rows)
        {
            TouchTop();
            return;
        }
        if(_curMaxRowCnt > Config.rows)
        {
            TouchTop();
            return;
        }

        if (_raiseOneRow)
        {
            RaiseOneRow();
            return;
        }

        if (_addNewRow)
        {
            var newRow = SpawnBlock();
            AddNewRow(newRow);
            return;
        }

        if (_alarmSet)
        {
            SoundAlarm();
        }

        UpdateState();

        if (!_suspendRaise && _delta * 1000 >= _curRaiseTime)
        {
            RaiseOneStep();
        }

        _delta += Time.deltaTime;
    }

    void OnDestroy()
    {
        if (_controller != null)
        {
            _controller.DestroyObj();
            _controller = null;
        }
    }

    void OnUpBtnClick()
    {
        Util.PlayClickSound(_upBtn.gameObject);

        _raiseOneRow = true;
        if (_multiPlayer)
        {
            var req = new SprotoType.up_row.request();
            NetSender.Send<Protocol.up_row>(req, (data) =>
            {
                var resp = data as SprotoType.up_row.response;
                Debug.LogFormat(" up_row response : {0}", resp.e);
                if (resp.e == 0) { }
            });
        }
    }

    void OnOtherBtnClick()
    {
        Debug.Log("OnOtherBtnClick");
        Util.PlayClickSound(_setupBtn.gameObject);
        if (_comingSoonObj == null)
        {
            _comingSoonObj = Instantiate(Config._comingSoonObj, this.transform) as GameObject;
            _comingSoonTw = _comingSoonObj.GetComponent<Image>().DOFade(0.1f, 3.5f);
            _comingSoonTw.OnComplete(DestroyComingSoonObj);
        }
        else
        {
            _comingSoonTw.Restart();
        }
    }

    void DestroyComingSoonObj()
    {
        if (_comingSoonObj != null)
        {
            GameObject.Destroy(_comingSoonObj);
            _comingSoonObj = null;
        }
    }

    public override bool IsMultiPlayer()
    {
        return _multiPlayer;
    }

    public override void ChangeScore(int score, int combo_cnt)
    {
        base.ChangeScore(score, combo_cnt);
        _scoreText.text = _score.ToString();
        SyncScore(score, combo_cnt);
    }

    void SyncScore(int score, int combo_cnt)
    {
        var req = new SprotoType.score.request();
        req.score = score;
        req.combo_cnt = combo_cnt;
        NetSender.Send<Protocol.score>(req, (data) =>
        {
            var resp = data as SprotoType.score.response;
            Debug.LogFormat(" score response : {0}", resp.e);
            if (resp.e == 0) { }
        });
    }

    void ShowResult()
    {
        _resultObj.SetActive(true);
        _resultInfoText.text = "You scored " + _score.ToString() + " in " + Util.FormatTime(Time.realtimeSinceStartup);
    }

    void TouchTop()
    {
        _gameOver = true;
        if (_multiPlayer)
        {
            var req = new SprotoType.touch_top.request();
            NetSender.Send<Protocol.touch_top>(req, (data) =>
            {
                var resp = data as SprotoType.touch_top.response;
                Debug.LogFormat(" touch_top response : {0}", resp.e);
                if (resp.e == 0) { }
            });
        }
        else
        {
            ShowResult();
        }
    }

    public void GameStart(SprotoType.game_start.request data)
    {
        _gameStart = true;
    }

    public void GameOver(SprotoType.game_over.request data)
    {
        _gameOver = true;
        ShowResult();
    }

    public void SyncScore(SprotoType.sync_score.request data)
    {
        int comboCnt = (int)data.combo_cnt;
        if (isExMode)
        {
            if (comboCnt > 7)
                comboCnt = 7;
            if (comboCnt > 3)
            {
                var obj = Instantiate(Config.obstacleObjs[comboCnt - 4], _blockBoardObj.transform) as GameObject;
            }
        }
    }
}
