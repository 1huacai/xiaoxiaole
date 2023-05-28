using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CheckerboardType { mine, emmy };
public enum BlockType { None, B1, B2, B3, B4, B5, Count };
public enum BlockState { None, Selected, Swaped, Moved, Tagged, Blanked, Trembled, Locked };
public enum BlockOperation { TouchEnter, TouchDown, TouchUp, TouchExit, DragHalf };
public enum GameBoardState { Idle, Selection, Swap, StateCount };
public enum GameStatus { Ready, Start, Alarm, Over };

public class Config : ScriptableObject
{
    //
    public const string serverIP = "124.221.89.86";
    public const int serverPort = 23001;

    //Root
    public static GameObject rootObj;
    public static GameObject mainObj;
    public static GameObject gameObj;
    public static MsgHandler msgHandler;

    public const string uiRoot = "UIRoot";

    //Main
    public const string uiMainRoot = uiRoot + "/Main";

    public const string singleModePath = uiMainRoot + "/SingleModeBtn";
    public const string multiModePath = uiMainRoot + "/MultiModeBtn";
    public const string matchingPath = uiMainRoot + "/Matching";
    public const string matchingTextPath = matchingPath + "/MatchText";
    public const string matchingDotTextPath = matchingPath + "/DotText";
    public const string cancelMatchPath = matchingPath + "/CancelMatchBtn";

    //Game
    public const string uiGameRoot = uiRoot + "/Game";

    public const string bgPath = uiGameRoot + "/Background/Character";
    public const string mainBlockPath = uiGameRoot + "/MainGameArea/BlockBoard";
    public const string mainAreaPath = uiGameRoot + "/MainGameArea/BlockBoard/AreaBottom";
    public const string rivalBlockPath = uiGameRoot + "/RivalGameArea/BlockBoard";
    public const string rivalAreaPath = uiGameRoot + "/RivalGameArea/BlockBoard/AreaBottom";
    public const string rivalShowPath = uiGameRoot + "/Player2";
    public const string timerPath = uiGameRoot + "/Timer/Timer(Clone)";
    public const string timerTextPath = uiGameRoot + "/Timer/Timer(Clone)/Text";
    public const string mainScorePath = uiGameRoot + "/Score";
    public const string resultPath = uiGameRoot + "/Result";
    public const string upButtonPath = uiGameRoot + "/UpButton";
    public const string setupButtonPath = uiGameRoot + "/Setup";
    public const string preparePath = uiGameRoot + "/Prepare";
    public const string skill1Path = uiGameRoot + "/skill1";
    public const string skill2Path = uiGameRoot + "/skill2";

    public const string MsgBoxPrefabPath = "Prefabs/MessageBox";
    public static Object _msgBoxObj = Resources.Load(MsgBoxPrefabPath);

    public const string ComingSoonPrefabPath = "Prefabs/ComingSoon";
    public static Object _comingSoonObj = Resources.Load(ComingSoonPrefabPath);

    public const string mainGamerPrefabPath = "Prefabs/MainGameArea";
    public static Object _mainGamerObj = Resources.Load(mainGamerPrefabPath);

    public const string rivalGamerPrefabPath = "Prefabs/RivalGameArea";
    public static Object _rivalGamerObj = Resources.Load(rivalGamerPrefabPath);

    public const string timerPrefabPath = "Prefabs/Timer";
    public static Object _timerObj = Resources.Load(timerPrefabPath);

    public const string blockPrefabPath = "Prefabs/Block";
    public static Object _blockObj = Resources.Load(blockPrefabPath);

    public const string comboPrefabPath = "Prefabs/Combo";
    public static Object _comboObj = Resources.Load(comboPrefabPath);

    public const string chainPrefabPath = "Prefabs/Chain";
    public static Object _chainObj = Resources.Load(chainPrefabPath);

    public const string pressureBlockPrefabPath = "Prefabs/PressureBlock";
    public static Object _pressureBlockObj = Resources.Load(pressureBlockPrefabPath);

    public const string starPrefabPath = "Prefabs/Star";
    public static Object _starObj = Resources.Load(starPrefabPath);

    public static List<Object> obstacleObjs = new List<Object>();

    public const string textureBlockPath = "Texture/block/panel";
    public const string textureComboPath = "Texture/combo/combo";
    public const string textureChainPath = "Texture/chain/chain";

    public const string animDestroyPath = "Animation/Destroy/Block";
    public const string animTremblePath = "Animation/Tremble/Block";
    public const string animPressurePath = "Animation/Pressure/Pressure";

    private static List<Texture2D> _texture = new List<Texture2D>{
        Resources.Load(Config.textureBlockPath+"01") as Texture2D,
        Resources.Load(Config.textureBlockPath+"11") as Texture2D,
        Resources.Load(Config.textureBlockPath+"21") as Texture2D,
        Resources.Load(Config.textureBlockPath+"31") as Texture2D,
        Resources.Load(Config.textureBlockPath+"41") as Texture2D,
        Resources.Load(Config.textureBlockPath+"51") as Texture2D,
    };
    public static Dictionary<int, Sprite> _sprites = new Dictionary<int, Sprite>
    {
        {0, Sprite.Create(_texture[0], new Rect(0, 0, _texture[0].width, _texture[0].height), new Vector2(1f, 1f)) },
        {1, Sprite.Create(_texture[1], new Rect(0, 0, _texture[1].width, _texture[1].height), new Vector2(1f, 1f)) },
        {2, Sprite.Create(_texture[2], new Rect(0, 0, _texture[2].width, _texture[2].height), new Vector2(1f, 1f)) },
        {3, Sprite.Create(_texture[3], new Rect(0, 0, _texture[3].width, _texture[3].height), new Vector2(1f, 1f)) },
        {4, Sprite.Create(_texture[4], new Rect(0, 0, _texture[4].width, _texture[4].height), new Vector2(1f, 1f)) },
        {5, Sprite.Create(_texture[5], new Rect(0, 0, _texture[5].width, _texture[5].height), new Vector2(1f, 1f)) },
    };
    public static List<Texture2D> _lockTexture = new List<Texture2D>{
        Resources.Load(Config.textureBlockPath+"07") as Texture2D,
        Resources.Load(Config.textureBlockPath+"17") as Texture2D,
        Resources.Load(Config.textureBlockPath+"27") as Texture2D,
        Resources.Load(Config.textureBlockPath+"37") as Texture2D,
        Resources.Load(Config.textureBlockPath+"47") as Texture2D,
        Resources.Load(Config.textureBlockPath+"57") as Texture2D,
    };
    public static Dictionary<int, Sprite> _lockSprites = new Dictionary<int, Sprite>
    {
        {0, Sprite.Create(_lockTexture[0], new Rect(0, 0, _lockTexture[0].width, _lockTexture[0].height), new Vector2(1f, 1f)) },
        {1, Sprite.Create(_lockTexture[1], new Rect(0, 0, _lockTexture[1].width, _lockTexture[1].height), new Vector2(1f, 1f)) },
        {2, Sprite.Create(_lockTexture[2], new Rect(0, 0, _lockTexture[2].width, _lockTexture[2].height), new Vector2(1f, 1f)) },
        {3, Sprite.Create(_lockTexture[3], new Rect(0, 0, _lockTexture[3].width, _lockTexture[3].height), new Vector2(1f, 1f)) },
        {4, Sprite.Create(_lockTexture[4], new Rect(0, 0, _lockTexture[4].width, _lockTexture[4].height), new Vector2(1f, 1f)) },
        {5, Sprite.Create(_lockTexture[5], new Rect(0, 0, _lockTexture[5].width, _lockTexture[5].height), new Vector2(1f, 1f)) },
    };

    public static List<RuntimeAnimatorController> _animDestroy = new List<RuntimeAnimatorController>
    {
        Resources.Load<RuntimeAnimatorController>(animDestroyPath+0),
        Resources.Load<RuntimeAnimatorController>(animDestroyPath+1),
        Resources.Load<RuntimeAnimatorController>(animDestroyPath+2),
        Resources.Load<RuntimeAnimatorController>(animDestroyPath+3),
        Resources.Load<RuntimeAnimatorController>(animDestroyPath+4),
        Resources.Load<RuntimeAnimatorController>(animDestroyPath+5),
    };

    public static List<RuntimeAnimatorController> _animTremble = new List<RuntimeAnimatorController>
    {
        Resources.Load<RuntimeAnimatorController>(animTremblePath+0),
        Resources.Load<RuntimeAnimatorController>(animTremblePath+1),
        Resources.Load<RuntimeAnimatorController>(animTremblePath+2),
        Resources.Load<RuntimeAnimatorController>(animTremblePath+3),
        Resources.Load<RuntimeAnimatorController>(animTremblePath+4),
        Resources.Load<RuntimeAnimatorController>(animTremblePath+5),
    };

    // 压力块解锁动画
    public static List<RuntimeAnimatorController> _animPressure = new List<RuntimeAnimatorController>
    {
        Resources.Load<RuntimeAnimatorController>(animPressurePath+3),
        Resources.Load<RuntimeAnimatorController>(animPressurePath+4),
        Resources.Load<RuntimeAnimatorController>(animPressurePath+5),
        Resources.Load<RuntimeAnimatorController>(animPressurePath+6),
    };

    // Coming soon
    public const string comingSoonPath = uiRoot + "/ComingSoon";

    // Audio
    public const string buttonClickClip = "Audio/menu_move";


    // Game Configuration
    public const string matchFailureMsgBoxTitle = "匹配失败";
    public const string matchErrorMsgBoxTitle = "匹配异常";
    public const string matchMsgBoxSure = "重试";
    public const string matchMsgBoxCancel = "取消";

    public const int rows = 11; // 总行数
    public const int matrixRows = rows + 1; // 方块矩阵行数
    public const int alarmRow = rows - 1; // 报警行数
    public const int columns = 6; // 总列数
    public const int initRows = 7; // 初始行数
    public const int initCols = 6; // 初始列数

    public const float blockWidth = 126.0f; // 方块宽度
    public const float blockHeight = 126.0f; // 方块高度

    public const float XPosShift = -315.0f; // 方块位置x坐标偏移值
    public const float YPosShift = 756.0f; // 方块位置y坐标偏移值
    public const int raiseSteps = 11; // 上升一行需要的步数
    public const float raiseDis = blockHeight / raiseSteps; // 每次上升的距离
    public const float moveDuration = 0.075f; // 方块滑动移动时间
    public const float fallDuration = 0.075f; // 方块下落移动时间

    public const int maxSpeed = 50;

    // speed to raise time
    public static int[] speedToRaiseTime =
    {
        942, 983, 838, 790, 755, 695, 649, 604, 570, 515,
        474, 444, 394, 370, 347, 325, 306, 289, 271, 256,
        240, 227, 213, 201, 189, 178, 169, 158, 148, 138,
        129, 120, 112, 105, 99,  92,  86,  82,  77,  73,
        69,  66,  62,  59,  56,  54,  52,  50,  48,  47,
    };

    // endless and 1P time attack use a speed system in which
    // speed increases based on the number of panels you clear.
    // For example, to get from speed 1 to speed 2, you must clear 9 panels.
    public static int[] panelsToNextSpeed =
    {
        9, 12, 12, 12, 12, 12, 15, 15, 18, 18,
        24, 24, 24, 24, 24, 24, 21, 18, 18, 18,
        36, 36, 36, 36, 36, 36, 36, 36, 36, 36,
        39, 39, 39, 39, 39, 39, 39, 39, 39, 39,
        45, 45, 45, 45, 45, 45, 45, 45, 45, 45,
    };

    // combo score lookup tables
    public static int[] scoreCombo =
    {
        0,      0,      0,      20,     30,
        50,     60,     70,     80,     100,
        140,    170,    210,    250,    290,
        340,    390,    440,    490,    550,
        610,    680,    750,    820,    900,
        980,    1060,   1150,   1240,   1330,
    };

    // chain score lookup tables
    public static int[] scoreChain = {
        0,      50,     80,     150,    300,
        400,    500,    700,    900,    1100,
        1300,   1500,   1800,
    };
}