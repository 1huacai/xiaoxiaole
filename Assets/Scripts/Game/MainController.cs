using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class MainController : GameController
{
    public CheckerboardType type;

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
    private Button _skill1Btn; // 设置按钮
    private Button _skill2Btn; // 设置按钮

    private SkeletonGraphic _minRole;
    private SkeletonGraphic _emmyRole;

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

        _skill1Btn = GameObject.Find(Config.skill1Path).GetComponent<Button>();
        _skill1Btn.onClick.AddListener(OnSkill1BtnClick);

        _skill2Btn = GameObject.Find(Config.skill2Path).GetComponent<Button>();
        _skill2Btn.onClick.AddListener(OnSkill2BtnClick);

        _resultObj = GameObject.Find(Config.resultPath);
        _resultInfoText = GameObject.Find(Config.resultInfoPath).GetComponent<Text>();

        _mainScoreObj = GameObject.Find(Config.mainScorePath);
        _mainScoreObj.SetActive(!_multiPlayer);

        _scoreText = GameObject.Find(Config.mainScoreTextPath).GetComponent<Text>();

        _rivalObj = GameObject.Find(Config.rivalShowPath);
        _rivalObj.SetActive(_multiPlayer);

        _minRole = GameObject.Find(Config.uiGameRoot + "/Player1/role").GetComponent<SkeletonGraphic>();
        _minRole.AnimationState.Complete += (a) =>
        {
            PlayAnima("idle");
        };
        _emmyRole = GameObject.Find(Config.uiGameRoot + "/Player2/role").GetComponent<SkeletonGraphic>();
        _emmyRole.AnimationState.Complete += (a) =>
        {
            PlayAnima("idle");
        };

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
            AddNewRow(newRow, CheckerboardType.mine);
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
            var req = new SprotoType.game_up_row.request();
            NetSender.Send<Protocol.game_up_row>(req, (data) =>
            {
                var resp = data as SprotoType.game_up_row.response;
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

    void OnSkill1BtnClick()
    {
        Debug.Log("OnSkill1BtnClick");
        Util.PlayClickSound(_setupBtn.gameObject);

        var req = new SprotoType.game_use_skill.request();
        req.skill_id = 1001;
        NetSender.Send<Protocol.game_use_skill>(req, (data) =>
        {
            var resp = data as SprotoType.game_use_skill.response;
            Debug.LogFormat(" game_use_skill response : {0}", resp.e);
            if (resp.e == 0) { }
            _minRole.AnimationState.SetAnimation(0, "atk", false);
        });
    }
    void OnSkill2BtnClick()
    {
        Debug.Log("OnSkill2BtnClick");
        Util.PlayClickSound(_setupBtn.gameObject);

        var req = new SprotoType.game_use_skill.request();
        req.skill_id = 1002;
        NetSender.Send<Protocol.game_use_skill>(req, (data) =>
        {
            var resp = data as SprotoType.game_use_skill.response;
            Debug.LogFormat(" game_use_skill response : {0}", resp.e);
            if (resp.e == 0) { }
            _minRole.AnimationState.SetAnimation(0, "atk2", false);
        });
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
        //var req = new SprotoType.score.request();
        //req.score = score;
        //req.combo_cnt = combo_cnt;
        //NetSender.Send<Protocol.score>(req, (data) =>
        //{
        //    var resp = data as SprotoType.score.response;
        //    Debug.LogFormat(" score response : {0}", resp.e);
        //    if (resp.e == 0) { }
        //});
    }

    void ShowResult()
    {
        _resultObj.SetActive(true);
        _resultInfoText.text = Util.FormatTime(Time.realtimeSinceStartup);
    }

    void TouchTop()
    {
        _gameOver = true;
        if (_multiPlayer)
        {
            var req = new SprotoType.game_over.request();
            NetSender.Send<Protocol.game_over>(req, (data) =>
            {
                var resp = data as SprotoType.game_over.response;
                Debug.LogFormat("game_over : {0}", resp.e);
                if (resp.e == 0) { }
            });
            ShowResult();
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

    public void GameOver(SprotoType.game_over_broadcast.request data)
    {
        _gameOver = true;
        ShowResult();
    }

    public void SyncNewpreBlock(SprotoType.eliminate_broadcast.request data)
    {
        GreatPressureBlock((int)data.count);
        //_curRowCnt = (int)data.cur_row_cnt;
        //_totalRowCnt = (int)data.total_row_cnt;
    }

    public void Usekill(SprotoType.game_use_skill_broadcast.request data)
    {
        PlayAnima(data.skill_id == 1001 ? "atk" : "atk2", false);
    }

    private void PlayAnima(string animaname,bool ismin = true)
    {
        if (ismin)
        {
            _minRole.AnimationState.SetAnimation(0, animaname, false);
        }
        else
        {
            _emmyRole.AnimationState.SetAnimation(0, animaname, false);
        }
    }
}
