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
    private Animator _prepareAnim; // 准备开始动画
    private Text _timer; // 倒计时
    private Button _upBtn; // 方块上升一行按钮
    private Button _setupBtn; // 设置按钮
    private GameObject _resultObj; // 结果UI
    private GameObject _rivalObj; // 对手展示区
    private GameObject _comingSoonObj;
    private Tweener _comingSoonTw;
    private Button _skill1Btn; // 技能1释放按钮
    private Button _skill2Btn; // 技能2释放按钮

    private Transform _minRoleTF;
    private Transform _emmyRoleTF;

    private Transform _minRoleeffect;
    private Transform _emmyRoleeffect;

    public SkeletonGraphic _minRole;
    public SkeletonGraphic _emmyRole;

    public Role _minRoleData;
    public Role _emmyRoleData;

    private Text _minName;
    private Slider _minHp;
    private Slider _minShield;
    private Text _minHpLable;
    private Text _minShieldLable;

    private Text _emmyName;
    private Slider _emmyHp;
    private Slider _emmyShield;
    private Text _emmyHpLable;
    private Text _emmyShieldLable;

    private Text _skill1Cd;
    private Text _skill2Cd;

    private Image _skill_Mask;
    private Image _skill_MaskBg;
    private Image _skill2_Slider;

    private Image _skil2_Mask;
    private Image _skil2_MaskBg;

    private GameObject _skilleffect;

    public GameObject _DragBlockIma;


    void Awake()
    {
        _controller = this;
        Config.msgHandler._mainControllerInst = this;
        StartTimer();
    }

    void Start()
    {
        // unity component init
        _prepareAnim = GameObject.Find(Config.preparePath).GetComponent<Animator>();
        _prepareAnim.gameObject.SetActive(true);

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

        _rivalObj = GameObject.Find(Config.rivalShowPath);
        _rivalObj.SetActive(IsMultiPlayer());

        _minRoleTF = GameObject.Find(Config.uiGameRoot + "/Player1/role").transform;
        _emmyRoleTF = GameObject.Find(Config.uiGameRoot + "/Player2/role").transform;

        _minRoleeffect = GameObject.Find(Config.uiGameRoot + "/Player1/effect").transform;
        _emmyRoleeffect = GameObject.Find(Config.uiGameRoot + "/Player2/effect").transform;

        _minName = GameObject.Find(Config.uiGameRoot + "/Player1/Name").GetComponent<Text>();
        _minHp = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Hp").GetComponent<Slider>();
        _minShield = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Shield").GetComponent<Slider>();
        _minHpLable = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Hp/Text").GetComponent<Text>();
        _minShieldLable = GameObject.Find(Config.uiGameRoot + "/Player1/HpBar/Shield/Text").GetComponent<Text>();

        _emmyName = GameObject.Find(Config.uiGameRoot + "/Player2/Name").GetComponent<Text>();
        _emmyHp = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Hp").GetComponent<Slider>();
        _emmyShield = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Shield").GetComponent<Slider>();
        _emmyHpLable = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Hp/Text").GetComponent<Text>();
        _emmyShieldLable = GameObject.Find(Config.uiGameRoot + "/Player2/HpBar/Shield/Text").GetComponent<Text>();

        _DragBlockIma = GameObject.Find("DragBlock");
        _DragBlockIma.SetActive(false);

        _skill1Cd = _skill1Btn.transform.Find("Text").GetComponent<Text>();
        _skill2Cd = _skill2Btn.transform.Find("Text").GetComponent<Text>();

        _skill_MaskBg = _skill1Btn.transform.Find("CD/mask").GetComponent<Image>();
        _skill_Mask = _skill1Btn.transform.Find("CD").GetComponent<Image>();
        _skill2_Slider = _skill2Btn.transform.Find("CDmask").GetComponent<Image>();
        _skill1Cd.gameObject.SetActive(false);
        _skill_Mask.gameObject.SetActive(false);

        _skil2_Mask = _skill2Btn.transform.Find("CDmask").GetComponent<Image>();
        _skil2_MaskBg = _skill2Btn.transform.Find("CDmask/mask").GetComponent<Image>();
        _skil2_Mask.gameObject.SetActive(false);

        _skilleffect = _skill2Btn.transform.Find("skilleffect").gameObject;
        _skilleffect.SetActive(false);



        InitRoleData(MainManager.Ins.players);

        // state handler init        
        InitStateHandlers();

        // variable init     
        InitMembers();

        // initial block init
        // InitBlocks();
        _gameInit = true;
    }

    public void InitRoleData(List<SprotoType.player_info> infos)
    {
        for (int i = 0; i < infos.Count; i++)
        {
            if (infos[i].rid == MainManager.Ins.Uid)
            {
                _minRoleData = new Role(infos[i].rname);
                _minRoleData.side = (int)infos[i].render;
                _minRoleData.isMin = true;
                _minName.text = "玩家";// infos[i].rname;
            }
            else
            {
                _emmyRoleData = new Role(infos[i].rname);
                _emmyRoleData.side = (int)infos[i].render;
                _emmyRoleData.isMin = false;
                _emmyName.text = "对手";// infos[i].rname;
            }
        }

        _minHp.maxValue = _minRoleData.MaxHp;
        _minShield.maxValue = _minRoleData.MaxShield;
        UpdateMinSlider();

        _emmyHp.maxValue = _emmyRoleData.MaxHp;
        _emmyShield.maxValue = _emmyRoleData.MaxShield;
        UpdateEmmySlider();

        ShowRoleModel();

        ShowInitAnima();

        UpdateSkill2Cd();
    }

    int cntInit = 0;
    void FixedUpdate()
    {
        if (_gameOver)
            return;

        cntInit++;
        _delta += Time.deltaTime;

        if (cntInit % 50 == 0)
        {
            PrintMatrix();
        }

        UpdateBlockArea();

        if (_gameInit)
        {
            if (cntInit % 5 == 0)
            {
                InitBlocks();
            }
            return;
        }

        if (_gameReady)
        {
            var stateInfo = _prepareAnim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                _prepareAnim.gameObject.SetActive(false);
                _timer.gameObject.SetActive(true);
                _gameReady = false;
                if (!IsMultiPlayer())
                    _gameStart = true;
            }
            return;
        }

        if (!_gameStart)
            return;

        UpdateState();

        if (_curRowCnt > Config.rows || _curMaxRowCnt > Config.rows)
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

        if (_suspendRaise <= 0 && _delta * 1000 >= _curRaiseTime)
        {
            RaiseOneStep();
        }
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

        if (IsMultiPlayer())
        {
            var req = new SprotoType.game_up_row.request();
            NetSender.Send<Protocol.game_up_row>(req, (data) =>
            {
                var resp = data as SprotoType.game_up_row.response;
                Debug.LogFormat("{0} -- up_row response: {1}", _boardType, resp.e);
                if (resp.e == 0)
                {
                    _raiseOneRow = true;
                }
            });
        }
    }

    void OnOtherBtnClick()
    {
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
        if (_minRoleData.Skill_1_CD)
        {
            Debug.LogError("skill _ cd");
            return;
        }
        Util.PlayClickSound(_setupBtn.gameObject);

        var req = new SprotoType.game_use_skill.request();
        req.skill_id = _minRoleData.skillId_1;
        NetSender.Send<Protocol.game_use_skill>(req, (data) =>
        {
            var resp = data as SprotoType.game_use_skill.response;
            Debug.LogFormat("{0} -- game_use_skill response: {1}", _boardType, resp.e);
            if (resp.e == 0) { }
            PlayAnima(_minRoleData.skillAnimaName_1);
            PlayAnima("hurt", false);
            _minRoleData.UseSkill1();
            UpdateSkill1Cd();
        });
    }

    void OnSkill2BtnClick()
    {
        if (!_minRoleData.Skill_2_CD)
        {
            Debug.LogError("skill_2_cd");
            return;
        }
        Util.PlayClickSound(_setupBtn.gameObject);

        var req = new SprotoType.game_use_skill.request();
        req.skill_id = _minRoleData.skillId_2;
        NetSender.Send<Protocol.game_use_skill>(req, (data) =>
        {
            var resp = data as SprotoType.game_use_skill.response;
            Debug.LogFormat("{0} -- game_use_skill response: {1}", _boardType, resp.e);
            if (resp.e == 0) { }
            PlayAnima(_minRoleData.skillAnimaName_2);
            PlayAnima("hurt", false);
            _minRoleData.UseSkill2();
            if (_minRoleData.side == 1)
            {
                _emmyRoleData.ChangeHpValue(30);
                UpdateEmmySlider();
            }
            UpdateSkill2Cd();
            if (_minRoleData.side == 1)
                ShowEmmeyEffect(SKillType.toushiche);

            if (_minRoleData.side == 2)
                ShowMinEffect(SKillType.huixue);
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
    void SyncScore(int score, int combo_cnt)
    {
        //var req = new SprotoType.score.request();
        //req.score = score;
        //req.combo_cnt = combo_cnt;
        //NetSender.Send<Protocol.score>(req, (data) =>
        //{
        //    var resp = data as SprotoType.score.response;
        //    Debug.LogFormat("{0} -- score response: {1}", _boardType, resp.e);
        //    if (resp.e == 0) { }
        //});
    }

    void ShowResult()
    {
        _resultObj.SetActive(true);
    }

    void TouchTop()
    {
        // Debug.Log(_boardType + " -- touch top hp:" + _minRoleData.Hp);
        if (_minRoleData.Hp > 0)
            return;

        _gameOver = true;
        if (IsMultiPlayer())
        {
            var req = new SprotoType.game_over.request();
            NetSender.Send<Protocol.game_over>(req, (data) =>
            {
                var resp = data as SprotoType.game_over.response;
                Debug.LogFormat("{0} -- game_over : {1}", _boardType, resp.e);
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
        Debug.Log(_boardType + " -- data.count:" + data.count);
        PlayAnima(data.count == 3 ? "atk" : data.count == 4 ? "atk2" : "atk3", false);
        PlayAnima("hurt");

        // if (data.count > 3 && _curRowCnt < Config.rows)
        if (data.count > 3)
        {
            GreatPressureBlock((int)data.count);
            _minRoleData.ChangeHpValue((int)(data.count) - 1);
            UpdateMinSlider();


            _emmyRoleData.UpdateSkill2((int)data.count);
        }
    }

    public void Usekill(SprotoType.game_use_skill_broadcast.request data)
    {
        PlayAnima(data.skill_id / 1000 > 10 ? "kill2" : "kill1", false);
        PlayAnima("hurt");

        var value = data.skill_id % 1000;
        if (value == 1)//女
        {
            MainManager.Ins.DragBlock = false;
            MainManager.Ins.DragTime = MainManager.Ins.Timer + 5;
        }
        else if (value == 2 )//男
        {
            _emmyRoleData.ChangeShieldTime = MainManager.Ins.Timer + 5;
        }
        else if (data.skill_id % 1000 == 10)//女
        {
            _minRoleData.ChangeHpValue(30);
            UpdateMinSlider();
            ShowMinEffect(SKillType.toushiche);
        }
        else if (data.skill_id % 1000 == 20)//男
        {
            _emmyRoleData.UseSkill2();
            ShowEmmeyEffect(SKillType.huixue);
        }
    }

    public void PlayAnima(string animaname, bool ismin = true)
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
    private void ShowRoleModel()
    {
        if (_minRoleTF.childCount == 0)
        {
            var min = Resources.Load<SkeletonDataAsset>(_minRoleData.pathName);
            Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
            _minRole = SkeletonGraphic.NewSkeletonGraphicGameObject(min, _minRoleTF, minmaterial);

            _minRole.skeletonDataAsset = min;
            _minRole.initialSkinName = "default";
            _minRole.startingAnimation = "idle";
            _minRole.startingLoop = false;
            _minRole.MatchRectTransformWithBounds();
            _minRole.material = minmaterial;
            _minRole.Initialize(true);

            _minRole.AnimationState.Complete += (a) =>
            {
                PlayAnima(_minRoleData.idleAnimaName);
            };
        }
        else
            _minRole = _minRoleTF.GetChild(0).GetComponent<SkeletonGraphic>();

        if (_emmyRoleTF.childCount == 0)
        {
            var emmy = Resources.Load<SkeletonDataAsset>(_emmyRoleData.pathName);
            Material emmymaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
            _emmyRole = SkeletonGraphic.NewSkeletonGraphicGameObject(emmy, _emmyRoleTF, emmymaterial);

            _emmyRole.skeletonDataAsset = emmy;
            _emmyRole.initialSkinName = "default";
            _emmyRole.startingAnimation = "idle";
            _emmyRole.startingLoop = false;
            _emmyRole.MatchRectTransformWithBounds();
            _emmyRole.material = emmymaterial;
            _emmyRole.Initialize(true);

            _emmyRole.AnimationState.Complete += (a) =>
            {
                PlayAnima(_emmyRoleData.idleAnimaName, false);
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
        float hpValue = (float)_minRoleData.Hp;
        float shieldValue = (float)_minRoleData.Shield;
        _minHp.value = hpValue;
        _minShield.value = shieldValue;

        _minHpLable.text = string.Format("{0}/{1}", _minRoleData.Hp, _minRoleData.MaxHp);
        _minShieldLable.text = string.Format("{0}/{1}", _minRoleData.Shield, _minRoleData.MaxShield);
    }
    public void UpdateEmmySlider()
    {
        float hpValue = (float)_emmyRoleData.Hp;
        float shieldValue = (float)_emmyRoleData.Shield;
        _emmyHp.value = hpValue;
        _emmyShield.value = shieldValue;

        _emmyHpLable.text = string.Format("{0}/{1}", _emmyRoleData.Hp, _emmyRoleData.MaxHp);
        _emmyShieldLable.text = string.Format("{0}/{1}", _emmyRoleData.Shield, _emmyRoleData.MaxShield);
    }

    public void UpdateSkill1Cd()
    {
        if (_minRoleData.Skill_1_CD)
        {
            _skill_Mask.gameObject.SetActive(true);
            _skill1Cd.gameObject.SetActive(true);
            _skill1Cd.text = string.Format("{0}", _minRoleData.Cd - MainManager.Ins.Timer);
            _skill_Mask.fillAmount = 1 -  (float)(_minRoleData.Cd - MainManager.Ins.Timer) / 15;
            _skill_MaskBg.fillAmount = (float)(_minRoleData.Cd - MainManager.Ins.Timer) / 15;
        }
        else
        {
            _skill1Cd.gameObject.SetActive(false);
            _skill_Mask.gameObject.SetActive(false);
        }
    }
    public void UpdateSkill2Cd()
    {
        if (!_minRoleData.Skill_2_CD)
        {
            _skill2Cd.gameObject.SetActive(false);
            //_skill2Cd.text = string.Format("{0}/{1}", _minRoleData.Skill_2_Value >= 30 ? 30 : _minRoleData.Skill_2_Value, 30);
            // _skill2_Slider.fillAmount = _minRoleData.Skill_2_Value >= 30 ? 1 : (float)_minRoleData.Skill_2_Value / 30;

            _skil2_Mask.gameObject.SetActive(true);
            _skil2_Mask.fillAmount = _minRoleData.Skill_2_Value >= 30 ? 1 : (float)_minRoleData.Skill_2_Value / 30;
            _skil2_MaskBg.fillAmount = _minRoleData.Skill_2_Value >= 30 ? 0 :  1 - (float)_minRoleData.Skill_2_Value / 30;
            _skilleffect.SetActive(false);
        }
        else
        {
            //_skill2_Slider.fillAmount = 0;
            _skill2Cd.gameObject.SetActive(false);
            _skil2_Mask.gameObject.SetActive(false);
            _skilleffect.SetActive(true);
        }
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
            CheckRecoverShield();
            UpdateSkill1Cd();
            CheckShowDrag();
        }
    }
    void CheckShowDrag()
    {
        if (MainManager.Ins.Timer > MainManager.Ins.DragTime && MainManager.Ins.DragTime != 0)
            MainManager.Ins.DragBlock = true;
        if (_DragBlockIma.activeSelf != !MainManager.Ins.DragBlock)
            _DragBlockIma.SetActive(!MainManager.Ins.DragBlock);
    }
    void CheckGameOver()
    {
        if (_curRowCnt > Config.rows || _curMaxRowCnt > Config.rows)
        {
            if (_minRoleData.Hp > 0)
            {
                _minRoleData.ChangeHpValue2(3);
                UpdateMinSlider();
            }
        }
        if (MainManager.Ins._rivalController._curRowCnt > Config.rows || MainManager.Ins._rivalController._curMaxRowCnt > Config.rows)
        {
            if (_emmyRoleData.Hp > 0)
            {
                _emmyRoleData.ChangeHpValue2(3);
                UpdateEmmySlider();
            }
        }
    }

    void CheckRecoverShield()
    {
        if (MainManager.Ins.Timer < _minRoleData.ChangeShieldTime && _minRoleData.ChangeShieldTime != 0)
            MainManager.Ins._mainController.ShowMinEffect(SKillType.dun);
        if (MainManager.Ins.Timer < _emmyRoleData.ChangeShieldTime && _emmyRoleData.ChangeShieldTime != 0)
            MainManager.Ins._mainController.ShowEmmeyEffect(SKillType.dun);

        if (MainManager.Ins.Timer - 6 > _minRoleData.hurtTimer && _minRoleData.hurtTimer != 0 )
        {
            _minRoleData.ChangeShieldValue(MainManager.Ins.Timer < _minRoleData.ChangeShieldTime && _minRoleData.ChangeShieldTime != 0 ? 5 : 2);
            UpdateMinSlider();
        }
        if (MainManager.Ins.Timer - 6 > _emmyRoleData.hurtTimer && _emmyRoleData.hurtTimer != 0)
        {
            _emmyRoleData.ChangeShieldValue(MainManager.Ins.Timer < _emmyRoleData.ChangeShieldTime && _emmyRoleData.ChangeShieldTime != 0 ? 5 : 2);
            UpdateEmmySlider();
        }
    }
    public void ShowMinEffect(SKillType type)
    {
        string path = type == SKillType.toushiche ? "spineArt/effect/huoqiu/skeleton_SkeletonData" :
            type == SKillType.dun ? "spineArt/effect/dun/shandian_SkeletonData" :
            "spineArt/effect/huixue/huixue_SkeletonData";


        var effectData = Resources.Load<SkeletonDataAsset>(path);
        Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
        SkeletonGraphic Minffect = SkeletonGraphic.NewSkeletonGraphicGameObject(effectData, _minRoleeffect, minmaterial);
        Minffect.transform.localPosition = type != SKillType.huixue ? new Vector3(Minffect.transform.localPosition.x, Minffect.transform.localPosition.y + 160, Minffect.transform.localPosition.z) : Minffect.transform.localPosition;
        Minffect.transform.localScale = type != SKillType.toushiche ? new Vector3(3, 3, 3) : Vector3.one;

        Minffect.skeletonDataAsset = effectData;
        Minffect.initialSkinName = "default";
        Minffect.startingAnimation = "animation";
        Minffect.startingLoop = false;
        Minffect.MatchRectTransformWithBounds();
        Minffect.material = minmaterial;
        Minffect.Initialize(true);

        Minffect.AnimationState.Complete += (a) =>
        {
            Destroy(Minffect.gameObject);
        };
    }

    public void ShowEmmeyEffect(SKillType type)
    {
        string path = type == SKillType.toushiche ? "spineArt/effect/huoqiu/skeleton_SkeletonData" :
            type == SKillType.dun ? "spineArt/effect/dun/shandian_SkeletonData" :
            "spineArt/effect/huixue/huixue_SkeletonData";


        var effectData = Resources.Load<SkeletonDataAsset>(path);
        Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
        SkeletonGraphic Emmeyeffect = SkeletonGraphic.NewSkeletonGraphicGameObject(effectData, _emmyRoleeffect, minmaterial);
        Emmeyeffect.transform.localPosition = type != SKillType.huixue ? new Vector3(Emmeyeffect.transform.localPosition.x, Emmeyeffect.transform.localPosition.y + 160, Emmeyeffect.transform.localPosition.z) : Emmeyeffect.transform.localPosition;
        Emmeyeffect.transform.localScale = type != SKillType.toushiche ? new Vector3(3, 3, 3) : Vector3.one;

        Emmeyeffect.skeletonDataAsset = effectData;
        Emmeyeffect.initialSkinName = "default";
        Emmeyeffect.startingAnimation = "animation";
        Emmeyeffect.startingLoop = false;
        Emmeyeffect.MatchRectTransformWithBounds();
        Emmeyeffect.material = minmaterial;
        Emmeyeffect.Initialize(true);

        Emmeyeffect.AnimationState.Complete += (a) =>
        {
            Destroy(Emmeyeffect.gameObject);
        };
    }
}
public enum SKillType
{
    toushiche,
    dun,
    huixue,
}
