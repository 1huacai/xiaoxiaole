using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;
using Spine;
using System.Collections;

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

    private Transform _minRoleTF;
    private Transform _emmyRoleTF;

    private SkeletonGraphic _minRole;
    private SkeletonGraphic _emmyRole;

    public Role _minroleData;
    public Role _emmyroleData;

    private Slider _minHp;
    private Slider _minShield;
    private Text _minHpLable;
    private Text _minShieldLable;


    private Slider _emmyHp;
    private Slider _emmyShield;
    private Text _emmyHpLable;
    private Text _emmyShieldLable;

    private Text _skill1Cd;
    private Text _skill2Cd;
    void Awake()
    {
        _controller = this;
        Config.msgHandler._mainControllerInst = this;
        StartTimer();
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

        _minRoleTF = GameObject.Find(Config.uiGameRoot + "/Player1/role").transform;
        _emmyRoleTF = GameObject.Find(Config.uiGameRoot + "/Player2/role").transform;

        _minHp = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Hp").GetComponent<Slider>();
        _minShield = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Shield").GetComponent<Slider>();
        _minHpLable = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Hp/Text").GetComponent<Text>();
        _minShieldLable = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Shield/Text").GetComponent<Text>();


        _emmyHp = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Hp").GetComponent<Slider>();
        _emmyShield = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Shield").GetComponent<Slider>();
        _emmyHpLable = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Hp/Text").GetComponent<Text>();
        _emmyShieldLable = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Shield/Text").GetComponent<Text>();

        _skill1Cd = _skill1Btn.transform.Find("Text").GetComponent<Text>();
        _skill2Cd = _skill2Btn.transform.Find("Text").GetComponent<Text>();

        InitRoleData(MainManager.Ins.players);

        // state handler init        
        InitStateHandlers();

        // variable init     
        InitMembers();

        // initial block init
        InitBlocks();

        // 删除初始生成的空行
        DestroyBlankRow();
    }

    public void InitRoleData(List<SprotoType.player_info> infos)
    {
        for (int i = 0; i < infos.Count; i++)
        {
            if (infos[i].rid == MainManager.Ins.Uid)
            {
                _minroleData = new Role();
                _minroleData.sid = (int)infos[i].render;
            }
            else
            {
                _emmyroleData = new Role();
                _emmyroleData.sid = (int)infos[i].render;
            }
        }

        _minHp.maxValue = _minroleData.MaxHp;
        _minShield.maxValue = _minroleData.MaxShield;
        UpdateMinSlider();

        _emmyHp.maxValue = _emmyroleData.MaxHp;
        _emmyShield.maxValue = _emmyroleData.MaxShield;
        UpdateEmmySlider();
        ShowRolrModel();

        ShowInitAnima();

        UpdateSkill2Cd();
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

        UpdateState();

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
        if (_minroleData.Skill_1_CD)
        {
            Debug.LogError("skill _ cd");
            return;
        }
            Debug.Log("OnSkill1BtnClick");
        Util.PlayClickSound(_setupBtn.gameObject);

        var req = new SprotoType.game_use_skill.request();
        req.skill_id = _minroleData.skillId_1;
        NetSender.Send<Protocol.game_use_skill>(req, (data) =>
        {
            var resp = data as SprotoType.game_use_skill.response;
            Debug.LogFormat(" game_use_skill response : {0}", resp.e);
            if (resp.e == 0) { }
            PlayAnima(_minroleData.skillAnimaName_1);
            PlayAnima("hurt",false);
            _minroleData.UseSkill1();
            UpdateSkill1Cd();
        });
    }
    void OnSkill2BtnClick()
    {
        if (!_minroleData.Skill_2_CD)
        {
            Debug.LogError("skill_2_cd");
            return;
        }
        Debug.Log("OnSkill2BtnClick");
        Util.PlayClickSound(_setupBtn.gameObject);

        var req = new SprotoType.game_use_skill.request();
        req.skill_id = _minroleData.skillId_2;
        NetSender.Send<Protocol.game_use_skill>(req, (data) =>
        {
            var resp = data as SprotoType.game_use_skill.response;
            Debug.LogFormat(" game_use_skill response : {0}", resp.e);
            if (resp.e == 0) { }
            PlayAnima(_minroleData.skillAnimaName_2);
            PlayAnima("hurt", false);
            _minroleData.UseSkill2();
            _emmyroleData.ChangeHpValue(30);
            UpdateEmmySlider();
            UpdateSkill2Cd();
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
        if (_minroleData.Hp > 0)
            return;

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
        PlayAnima(data.count == 3 ? "atk" : "atk2", false);
        PlayAnima("hurt");

        if (data.count > 3)
        {
            GreatPressureBlock((int)data.count);
            _minroleData.ChangeHpValue((int)(data.count) - 1);
            UpdateMinSlider();
        }
        //_curRowCnt = (int)data.cur_row_cnt;
        //_totalRowCnt = (int)data.total_row_cnt;
    }

    public void Usekill(SprotoType.game_use_skill_broadcast.request data)
    {
        PlayAnima(data.skill_id / 1000 > 10 ? "kill2" : "kill1", false);
        PlayAnima("hurt");

        var value = data.skill_id % 1000;
        if (value < 4)
        {
            MainManager.Ins.DragBlock = false;
            MainManager.Ins.DragTime = MainManager.Ins.Timer + 5;
        }
        else if (value > 3 && value < 10)
        {
            _minroleData.ChangeShieldTime = MainManager.Ins.Timer + 5;
        }
        else if (data.skill_id % 10000 < 4)
        {
            _minroleData.ChangeHpValue(30);
            UpdateMinSlider();
        }
    }

    public void PlayAnima(string animaname,bool ismin = true)
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
    private void ShowRolrModel()
    {
        if (_minRoleTF.childCount == 0)
        {
            var min = Resources.Load<SkeletonDataAsset>(_minroleData.pathName);
            Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
            _minRole = SkeletonGraphic.NewSkeletonGraphicGameObject(min, _minRoleTF, minmaterial);

            _minRole.skeletonDataAsset = min;
            _minRole.initialSkinName = "default";
            _minRole.startingAnimation = "idle";
            _minRole.startingLoop = true;
            _minRole.MatchRectTransformWithBounds();
            _minRole.material = minmaterial;
            _minRole.Initialize(true);

            _minRole.AnimationState.Complete += (a) =>
            {
                PlayAnima(_minroleData.idleAnimaName);
            };
        }
        else
            _minRole = _minRoleTF.GetChild(0).GetComponent<SkeletonGraphic>();

        if (_emmyRoleTF.childCount == 0)
        {
            var emmy = Resources.Load<SkeletonDataAsset>(_emmyroleData.pathName);
            Material emmymaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
            _emmyRole = SkeletonGraphic.NewSkeletonGraphicGameObject(emmy, _emmyRoleTF, emmymaterial);

            _emmyRole.skeletonDataAsset = emmy;
            _emmyRole.initialSkinName = "default";
            _emmyRole.startingAnimation = "idle";
            _emmyRole.startingLoop = true;
            _emmyRole.MatchRectTransformWithBounds();
            _emmyRole.material = emmymaterial;
            _emmyRole.Initialize(true);

            _emmyRole.AnimationState.Complete += (a) =>
            {
                PlayAnima(_emmyroleData.idleAnimaName, false);
            };
        }
        else
            _emmyRole = _minRoleTF.GetChild(0).GetComponent<SkeletonGraphic>();
    }

    private void ShowInitAnima()
    {
        _minRole.AnimationState.SetAnimation(0, "appear_annimation", false);
        _emmyRole.AnimationState.SetAnimation(0, "appear_annimation", false);
    }

    public void UpdateMinSlider()
    {
        float hpValue = (float)_minroleData.Hp;
        float shieldValue = (float)_minroleData.Shield;
        _minHp.value = hpValue;
        _minShield.value = shieldValue;

        _minHpLable.text = string.Format("{0}/{1}", _minroleData.Hp, _minroleData.MaxHp);
        _minShieldLable.text = string.Format("{0}/{1}", _minroleData.Shield, _minroleData.MaxShield);
    }
    public void UpdateEmmySlider()
    {
        float hpValue = (float)_emmyroleData.Hp;
        float shieldValue = (float)_emmyroleData.Shield;
        _emmyHp.value = hpValue;
        _emmyShield.value = shieldValue;

        _emmyHpLable.text = string.Format("{0}/{1}", _emmyroleData.Hp, _emmyroleData.MaxHp);
        _emmyShieldLable.text = string.Format("{0}/{1}", _emmyroleData.Shield, _emmyroleData.MaxShield);
    }

    public void UpdateSkill1Cd()
    {
        if (_minroleData.Skill_1_CD)
        {
            _skill1Cd.gameObject.SetActive(true);
            _skill1Cd.text = string.Format("{0}/{1}", _minroleData.Cd - MainManager.Ins.Timer, 15);
        }
        else
            _skill1Cd.gameObject.SetActive(false);
    }
    public void UpdateSkill2Cd()
    {
        if (!_minroleData.Skill_2_CD)
        {
            _skill2Cd.gameObject.SetActive(true);
            _skill2Cd.text = string.Format("{0}/{1}", _minroleData.Skill_2_Value >= 30 ? 30 : _minroleData.Skill_2_Value, 30);
        }
        else
            _skill2Cd.gameObject.SetActive(false);
    }

    private void StartTimer()
    {
        StopAllCoroutines();
        StartCoroutine(Timer());
    }
    IEnumerator Timer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            MainManager.Ins.Timer++;
            CheckGameOver();
            CheckDrag();
            CheckReCoverHp();
            UpdateSkill1Cd();
        }
    }
    void CheckGameOver()
    {
        if (_curRowCnt > Config.rows || _curMaxRowCnt > Config.rows)
        {
            if (_minroleData.Hp > 0)
            {
                _minroleData.ChangeHpValue2(3);
                UpdateMinSlider();
            }
        }
        if (MainManager.Ins._rivalController._curRowCnt > Config.rows || MainManager.Ins._rivalController._curMaxRowCnt > Config.rows)
        {
            if (_emmyroleData.Hp > 0)
            {
                _emmyroleData.ChangeHpValue2(3);
                UpdateEmmySlider();
            }
        }
    }
    void CheckDrag()
    {
        if (MainManager.Ins.Timer >= MainManager.Ins.DragTime)
            MainManager.Ins.DragBlock = true;
    }
    void CheckReCoverHp()
    {
        if (MainManager.Ins.Timer - 3 > _minroleData.hurtTimer)
        {
            _minroleData.ChangeShildValue(MainManager.Ins.Timer < _minroleData.ChangeShieldTime ? 5 : 2);
        }
        if (MainManager.Ins.Timer - 3 > _emmyroleData.hurtTimer)
        {
            _emmyroleData.ChangeShildValue(2);
        }
    }
}
