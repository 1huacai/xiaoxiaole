using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Main : MonoBehaviour
{
    public static string rid = "";

    private Button _singleBtn;
    private Button _multiBtn;
    private Button _multiExBtn;

    private GameObject _matchingUI;
    private Text _matchingText;
    private Text _matchingDotText;

    private GameObject _matchSuccessUI;
    private Text _matchSuccessText;

    private GameObject _mainGamerObj;
    private GameObject _rivalGamerObj;

    private GameObject _comingSoonObj;
    private Tweener _comingSoonTw;
    private GameObject _timerObj;

    private Button _playerInfoBtn;
    private Button _signBtn;
    private Button _giftBtn;
    private Button _fetterBtn;
    private Button _mailBtn;
    private Button _friendsBtn;
    private Button _chargeBtn;
    private Button _mallBtn;
    private Button _diaryBtn;


    private bool _enterGame = false; // ������Ϸ����
    private bool _multiPlayer = false; // ����ģʽ����
    private bool _isExMode = false; // ����EXģʽ����
    private bool _server_logind = false;
    private float _delta = 0;

    private List<SprotoType.block_info> _initMatrix; // ��Ϸ��ʼ��������

    public static bool _reset = false;



    // Start is called before the first frame update
    void Start()
    {
        Config.msgHandler._mainInst = this;
        Application.runInBackground = true;

        _singleBtn = GameObject.Find(Config.singleStartPath).GetComponent<Button>();
        _singleBtn.onClick.AddListener(OnSingleBtnClick);

        _multiBtn = GameObject.Find(Config.multiStartPath).GetComponent<Button>();
        _multiBtn.onClick.AddListener(OnMultiBtnClick);

        _multiExBtn = GameObject.Find(Config.multiExStartPath).GetComponent<Button>();
        _multiExBtn.onClick.AddListener(OnMultiExBtnClick);

        _matchingUI = GameObject.Find(Config.matchingPath);
        _matchingText = GameObject.Find(Config.matchingTextPath).GetComponent<Text>();
        _matchingDotText = GameObject.Find(Config.matchingDotTextPath).GetComponent<Text>();
        _matchingUI.SetActive(false);

        _matchSuccessUI = GameObject.Find(Config.matchSuccessPath);
        _matchSuccessText = GameObject.Find(Config.matchSuccessTextPath).GetComponent<Text>();
        _matchSuccessUI.SetActive(false);

        _playerInfoBtn = GameObject.Find(Config.playereInfoBtnPath).GetComponent<Button>();
        _playerInfoBtn.onClick.AddListener(OnOtherBtnClick);
        _signBtn = GameObject.Find(Config.signBtnPath).GetComponent<Button>();
        _signBtn.onClick.AddListener(OnOtherBtnClick);
        _giftBtn = GameObject.Find(Config.giftBtnPath).GetComponent<Button>();
        _giftBtn.onClick.AddListener(OnOtherBtnClick);
        _fetterBtn = GameObject.Find(Config.fetterBtnPath).GetComponent<Button>();
        _fetterBtn.onClick.AddListener(OnOtherBtnClick);
        _mailBtn = GameObject.Find(Config.mailBtnPath).GetComponent<Button>();
        _mailBtn.onClick.AddListener(OnOtherBtnClick);
        _friendsBtn = GameObject.Find(Config.friendsBtnPath).GetComponent<Button>();
        _friendsBtn.onClick.AddListener(OnOtherBtnClick);
        _chargeBtn = GameObject.Find(Config.chargeBtnPath).GetComponent<Button>();
        _chargeBtn.onClick.AddListener(OnOtherBtnClick);
        _mallBtn = GameObject.Find(Config.mallBtnPath).GetComponent<Button>();
        _mallBtn.onClick.AddListener(OnOtherBtnClick);
        _diaryBtn = GameObject.Find(Config.diaryBtnPath).GetComponent<Button>();
        _diaryBtn.onClick.AddListener(OnOtherBtnClick);

        Config.gameObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        NetCore.Dispatch();

        if (_reset)
        {
            if (_mainGamerObj != null)
            {
                GameObject.Destroy(_mainGamerObj);
                _mainGamerObj = null;
            }
            if (_rivalGamerObj != null)
            {
                GameObject.Destroy(_rivalGamerObj);
                _rivalGamerObj = null;
            }
            if (_timerObj != null)
            {
                GameObject.Destroy(_timerObj);
                _timerObj = null;
            }
            _reset = false;
        }

        if (_enterGame)
        {
            _delta += Time.deltaTime;
            if (_delta < 0.2) return;

            Debug.Log("entering game");
            _delta = 0;
            _enterGame = false;

            DestroyComingSoonObj();
            Config.mainObj.SetActive(false);
            Config.gameObj.SetActive(true);
            _matchingUI.SetActive(false);
            _matchSuccessUI.SetActive(false);

            var parent = Config.gameObj.transform.Find("GameArea");
            GameController._initMatrix = _initMatrix;
            _mainGamerObj = Instantiate(Config._mainGamerObj, parent) as GameObject;
            var mainController = _mainGamerObj.GetComponent<MainController>();
            mainController._multiPlayer = _multiPlayer;
            mainController.isExMode = _isExMode;
            if (_multiPlayer)
            {
                _rivalGamerObj = Instantiate(Config._rivalGamerObj, parent) as GameObject;
                var rivalController = _rivalGamerObj.GetComponent<RivalController>();
                rivalController.isExMode = _isExMode;
            }

            var timerParent = Config.gameObj.transform.Find("Timer");
            _timerObj = Instantiate(Config._timerObj, timerParent) as GameObject;
            _timerObj.GetComponent<CountDown>()._countDown = _multiPlayer;
            
            _multiPlayer = false;
        }
    }

    // ����ģʽ��ʼ
    void OnSingleBtnClick()
    {
        Util.PlayClickSound(_singleBtn.gameObject);
        _initMatrix = GenInitBlocks(false);
        _enterGame = true;
    }

    // ����ģʽ��ʼ
    void OnMultiBtnClick()
    {
        Util.PlayClickSound(_multiBtn.gameObject);
        _isExMode = false;
        GameBattle();
    }

    void OnMultiExBtnClick()
    {
        Util.PlayClickSound(_multiBtn.gameObject);
        _isExMode = true;
        GameBattle();
    }

    // ƥ��ʧ��ȷ�Ͽ�ȡ��
    void OnMatchCancelClick()
    {
        _matchingUI.SetActive(false);
    }

    // ƥ��ʧ��ȷ�Ͽ�����
    void OnMatchRetryClick()
    {
        SwitchMatchingText(true);
        MatchReq();
    }

    void OnOtherBtnClick()
    {
        Debug.Log("OnOtherBtnClick");
        Util.PlayClickSound(_multiBtn.gameObject);
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

    void GameBattle()
    {
        if (NetCore.connected == false)
        {
            _server_logind = false;
            NetCore.Connect(Config.serverIP, Config.serverPort, () =>
            {
                Debug.Log("connect server success");
            });
        }
        if (_server_logind)
            MatchReq();
        else
            GameAuthReq();
    }

    void MatchSuccessTexTWFinish()
    {
    }

    void SwitchMatchingText(bool signal)
    {
        _matchingText.gameObject.SetActive(signal);
        _matchingDotText.gameObject.SetActive(signal);
    }

    void GameAuthReq()
    {
        var req = new SprotoType.game_auth.request()
        {
            uid = SystemInfo.deviceUniqueIdentifier,
            version = "0.1",
        };
        NetSender.Send<Protocol.game_auth>(req, (data) =>
        {
            var resp = data as SprotoType.game_auth.response;
            Debug.LogFormat("game_auth response : {0}, rid={1}", resp.e, resp.rid);
            if (resp.e == 0)
            {
                rid = resp.rid;
                LoginReq(rid);
            }
        });
    }

    void LoginReq(string rid)
    {
        var req = new SprotoType.login.request()
        {
            rid = rid,
        };
        NetSender.Send<Protocol.login>(req, (data) =>
        {
            var resp = data as SprotoType.login.response;
            Debug.LogFormat("login response : {0}", resp.e);
            if (resp.e == 0)
            {
                _server_logind = true;
                MatchReq();
            }
        });
    }

    void MatchReq()
    {
        var req = new SprotoType.match.request();
        NetSender.Send<Protocol.match>(req, (data) =>
        {
            var resp = data as SprotoType.match.response;
            Debug.LogFormat("match response : {0}", resp.e);
            if (resp.e == 0)
            {
                _matchingUI.SetActive(true);
                SwitchMatchingText(true);
                _matchingDotText.text = "";
                Tweener textTW = _matchingDotText.DOText("...", 3.0f);
                textTW.SetLoops(4);
            }
        });
    }

    void InitDataReq(List<SprotoType.block_info> pack)
    {
        var req = new SprotoType.init_data.request()
        {
            matrix = pack,
        };
        NetSender.Send<Protocol.init_data>(req, (data) =>
        {
            var resp = data as SprotoType.init_data.response;
            Debug.LogFormat(" init_data response : {0}", resp.e);
            if (resp.e == 0)
            {

            }
        });
    }

    void ShowMatchMsgBox(string title)
    {
        SwitchMatchingText(false);
        var dialog = new DialogInfo
        {
            warnInfo = title,
            sureBtnInfo = Config.matchMsgBoxSure,
            cancleBtnInfo = Config.matchMsgBoxCancel,
            onCancel = OnMatchCancelClick,
            onSure = OnMatchRetryClick,
            openType = OpenMessageType.SureandCancle,
        };
        MessageBox msgBox = new MessageBox(dialog);
    }

    // ƥ�䳬ʱ����
    public void MatchTimeout(SprotoType.match_timeout.request data)
    {
        ShowMatchMsgBox(Config.matchFailureMsgBoxTitle);
    }

    // ƥ��ɹ�����
    public void MatchSuccess(SprotoType.match_success.request data)
    {
        _multiPlayer = true;
        GenInitBlocks(true);

        var colorTW = _matchSuccessText.DOColor(Color.red, 0.1f);
        colorTW.SetLoops(3);
        colorTW.OnComplete(MatchSuccessTexTWFinish);
    }

    // ƥ���쳣����
    public void MatchWrong(SprotoType.match_wrong.request data)
    {
        ShowMatchMsgBox(Config.matchErrorMsgBoxTitle);
    }

    // ����׼���׶�
    public void GameReady(SprotoType.game_ready.request data)
    {
        _initMatrix = data.init_matrix;
        _enterGame = true;
    }

    public void GenBringForward(BlockType[,] matrix, int rowCnt)
    {
        int opRow = rowCnt;
        while (opRow > 0)
        {
            for (int col = 0; col < Config.initCols; col++)
            {
                matrix[opRow, col] = matrix[opRow - 1, col];
                matrix[opRow - 1, col] = 0;
            }
            opRow -= 1;
        }
    }

    public bool GenCheckRowType(BlockType[,] matrix, int opCol, BlockType newType)
    {
        if (opCol < 2)
            return true;
        if (matrix[0, opCol - 2] == matrix[0, opCol - 1] && matrix[0, opCol - 1] == newType)
        {
            return false;
        }
        return true;
    }

    private List<SprotoType.block_info> GenInitBlocks(bool needSync = false)
    {
        List<SprotoType.block_info> pack = new List<SprotoType.block_info>();
        var blockMatrix = new BlockType[Config.initRows, Config.initCols];
        var initSymb = new Dictionary<int, List<int>>()
        {
            [0] = new List<int>() { },
            [1] = new List<int>() { },
            [2] = new List<int>() { },
            [3] = new List<int>() { },
            [4] = new List<int>() { },
            [5] = new List<int>() { },
            [6] = new List<int>() { 0, 0, 0, 0, 0, 0 },
            [7] = new List<int>() { 0, 0, 0, 0, 0, 0 },
        };
        for (int row = 0; row < Config.initRows; row++)
        {
            var rand = new System.Random(Util.GetRandomSeed());
            GenBringForward(blockMatrix, row);
            for (int col = 0; col < Config.initCols; col++)
            {
                if (row < 6)
                {
                    if (row > 0 && initSymb[row - 1][col] == 0)
                    {
                        initSymb[row].Add(0);
                    }
                    else
                    {
                        initSymb[row].Add(rand.Next(0, 3));
                    }
                }
            }
            for (int col = 0; col < Config.initCols; col++)
            {
                int newType = (int)BlockType.None;
                if (initSymb[row] == null || initSymb[row][col] == 0) // û��ָ������0��ʶ�����ɷ��飬��0�����ɷ���
                {
                    newType = rand.Next((int)BlockType.B1, (int)BlockType.Count);
                    var aboveType = BlockType.None;
                    if (row > 0)
                    {
                        aboveType = blockMatrix[1, col];
                    }
                    while (!GenCheckRowType(blockMatrix, col, (BlockType)newType) || aboveType == (BlockType)newType)
                    {
                        newType = rand.Next((int)BlockType.B1, (int)BlockType.Count);
                    }
                }

                blockMatrix[0, col] = (BlockType)newType;
                SprotoType.block_info item = new SprotoType.block_info
                {
                    row = Config.initRows - 1 - row,
                    col = col,
                    type = newType,
                };
                pack.Add(item);
            }
        }
        if (needSync)
        {
            InitDataReq(pack);
        }
        return pack;
    }
}