using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class GameController : MonoBehaviour
{
    public GameController _controller;
    public CheckerboardType _boardType;

    public GameObject _blockBoardObj; // 方块面板区
    public GameObject _blockAreaObj; // 方块面板区底图
    public GameObject _pressureBoardObj; // 压力块面板区
    public GameObject _effectObj; // 特效

    public Dictionary<GameBoardState, StateBase> _states;
    public GameBoardState _curGameBoardState = GameBoardState.Idle;

    public bool _gameInit = false;
    public static bool _gameReady = false;
    public static bool _gameStart = false;
    public static bool _gameOver = false;
    public bool _multiPlayer = false;

    public List<SprotoType.block_info> _initMatrix = new List<SprotoType.block_info>(); // 方块初始数据
    public Block[,] _blockMatrix = new Block[Config.matrixRows, Config.columns]; // 方块行列矩阵
    public List<PressureBlock> _PressureMatrix = new List<PressureBlock>(); // 压力块列表

    public Block _firstSelected = null;
    public Block _secondSelected = null;

    public int _curRowCnt = 0;
    public int _curMaxRowCnt = 0;
    public int _totalRowCnt = 0;

    public int _curSpeed = 0;
    public int _curRaiseTime;
    public bool _raiseOneRow = false;
    public bool _addNewRow = false;
    public int _suspendRaise = 0;
    public bool _alarmSet = false; // 危险警报

    Garbage[] _garbagePool = new Garbage[50];
    int _poolIdx = 0;

    public float _delta = 0;


    void Awake()
    { }

    void Start()
    { }

    void Update()
    { }

    void OnDestroy()
    { }

    public void PrintMatrix()
    {
        if (_gameInit)
            return;
        string str = _boardType + "\n";
        for (int row = _curMaxRowCnt - 1; row >= 0; row--)
        {
            int col = 0;
            PressureBlock pressure = GetPressureByRow(row);
            if (pressure != null)
            {
                for (int i = 0; i < pressure.xNum; i++)
                {
                    str = str + "\t[" + pressure.Row + "," + i + " - @@ ]";
                    col++;
                }
            }
            for (; col < Config.columns; col++)
            {
                if (row <= Config.rows)
                {
                    Block block = _blockMatrix[row, col];
                    if (block)
                        str = str + "\t[" + block.Row + "," + block.Column + " - " + block.Type + "]";
                    else
                    {
                        str = str + "\t[" + row + "," + col + " - " + BlockType.None + "]";
                    }
                }
                else
                {
                    str = str + "\t[" + row + "," + col + " - " + BlockType.None + "]";
                }
            }
            str = str + "\n";
        }
        Debug.Log(str);
    }

    public void InitStateHandlers()
    {
        _states = new Dictionary<GameBoardState, StateBase>();
        _states.Add(GameBoardState.Idle, new IdleState(this));
        _states.Add(GameBoardState.Selection, new SelectionState(this));
        _states.Add(GameBoardState.Swap, new SwapState(this));
    }

    public void InitMembers()
    {
        _blockBoardObj = transform.Find("BlockBoard").gameObject;
        _blockAreaObj = _blockBoardObj.transform.Find("AreaBottom").gameObject;
        _pressureBoardObj = transform.Find("PressureBoard").gameObject;
        _effectObj = transform.Find("Effect").gameObject;

        // _gameReady = true;
        _gameStart = false;
        _gameOver = false;

        _curRaiseTime = Config.speedToRaiseTime[_curSpeed];
    }

    public void DestroyObj()
    {
        _blockMatrix = null;
        if (_states != null)
        {
            foreach (var state in _states.Values)
            {
                state.OnDestroy();
            }
            _states = null;
        }
    }

    public void ChangeToState(GameBoardState newState)
    {
        if (!MainManager.Ins.DragBlock) return;
        if (newState == _curGameBoardState) return;
        Debug.Log(string.Format("{0} -- Change to new state:{1}", _boardType, newState.ToString()));
        if (_states.ContainsKey(_curGameBoardState))
        {
            _states[_curGameBoardState].Exit();
        }
        _curGameBoardState = newState;
        if (_states.ContainsKey(_curGameBoardState))
        {
            _states[_curGameBoardState].Enter();
        }
    }

    public void UpdateState()
    {
        if (_states != null && _states.ContainsKey(_curGameBoardState))
        {
            _states[_curGameBoardState].Update();
        }
    }

    public void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        if (_states.ContainsKey(_curGameBoardState))
        {
            _states[_curGameBoardState].OnBlockOperation(row, column, operation);
        }
    }

    virtual public bool IsMultiPlayer()
    {
        return false;
    }

    public void CheckAlarm()
    {
        for (int i = 0; i < _PressureMatrix.Count; i++)
        {
            if (_PressureMatrix[i].Row >= Config.alarmRow)
            {
                _alarmSet = true;
            }
        }
        if (_curRowCnt >= Config.alarmRow)
        {
            _alarmSet = true;
        }
    }

    public void SoundAlarm()
    {
        for (int col = 0; col < Config.columns; col++)
        {
            var topBlock = _blockMatrix[Config.alarmRow, col];
            if (topBlock != null && topBlock.Type != BlockType.None)
            {
                for (int row = 0; row < _curRowCnt; row++)
                {
                    var block = _blockMatrix[row, col];
                    if (block != null && block.IsTrembled == false)
                        block.TrembleChange(true);
                }
            }
            else
            {
                for (int row = 0; row < _curRowCnt; row++)
                {
                    var item = _blockMatrix[row, col];
                    if (item != null && item.IsTrembled)
                        item.TrembleChange(false);
                }
            }
        }
        _alarmSet = false;
    }

    // 提升一行
    public void RaiseOneRow()
    {
        _raiseOneRow = false;
        if (_curMaxRowCnt >= Config.matrixRows)
            return;

        _suspendRaise++;
        var moveDis = (_totalRowCnt - Config.initRows + 1) * Config.blockHeight;
        _blockBoardObj.transform.DOLocalMoveY(moveDis, 0.2f).OnComplete(() =>
        {
            Transform _areaTran = _blockAreaObj.transform;
            _areaTran.localPosition = new Vector3(_areaTran.localPosition.x, _areaTran.localPosition.y - Config.blockHeight, 0);
            _addNewRow = true;
            _suspendRaise--;
        });

        _pressureBoardObj.transform.DOLocalMoveY(moveDis, 0.2f);
        _effectObj.transform.DOLocalMoveY(moveDis, 0.2f);
    }

    public void RaiseOneStep()
    {
        Transform _tran = _blockBoardObj.transform;
        _tran.localPosition = new Vector3(_tran.localPosition.x, _tran.localPosition.y + Config.raiseDis, 0);
        Transform _pressureTrans = _pressureBoardObj.transform;
        _pressureTrans.localPosition = new Vector3(_pressureTrans.localPosition.x, _pressureTrans.localPosition.y + Config.raiseDis, 0);
        Transform _effectTrans = _effectObj.transform;
        _effectTrans.localPosition = new Vector3(_effectTrans.localPosition.x, _effectTrans.localPosition.y + Config.raiseDis, 0);
        if (_tran.localPosition.y > (_totalRowCnt - Config.initRows + 1) * Config.blockHeight - 11)
        {
            Transform _areaTran = _blockAreaObj.transform;
            _areaTran.localPosition = new Vector3(_areaTran.localPosition.x, _areaTran.localPosition.y - Config.blockHeight, 0);
            _addNewRow = true;
        }
        _delta = 0;
    }

    //生成初始的方块
    public void InitBlocks()
    {
        if (_initMatrix.Count == 0)
        {
            _gameInit = false;
            _gameReady = true;
            UpdateMaxCnt();
            return;
        }

        SprotoType.block_info data = _initMatrix[0];
        int row = (int)data.row;
        int col = (int)data.col;
        int type = (int)data.type;
        _initMatrix.Remove(data);

        // 空方块不生成对象
        if ((BlockType)type == BlockType.None)
            return;

        var block = Block.CreateBlockObject(row, col, type, _blockBoardObj.transform, this);
        block.transform.localPosition = new Vector3(Config.XPosShift + col * Config.blockWidth, Config.YPosShift, 0);
        block.BlockOperationEvent += OnBlockOperation;
        block.IsIniting = true;
        block.NeedFall = true;
        block.fallCnt = row - 12;

        _blockMatrix[row, col] = block;

        _curRowCnt = row + 1;
        _curMaxRowCnt = _curRowCnt;
        _totalRowCnt = row + 1;
    }

    public void GreatPressureBlock(int count)
    {
        SetPressureInfo(count);
        int initRow = Config.matrixRows;
        for (int i = 0; i < _pressureInfo.Count; i++)
        {
            int yOffset = 0;
            if (_curMaxRowCnt > Config.matrixRows)
            {
                initRow = _curMaxRowCnt;
                yOffset = _curMaxRowCnt - Config.matrixRows;
            }
            Debug.Log(_boardType + "-- create pressure _curMaxRowCnt:" + _curMaxRowCnt + " - initRow:" + initRow + " - yOffset:" + yOffset);
            var pressure = PressureBlock.CreatePressureObject(initRow, _pressureInfo[i].Key, _pressureBoardObj.transform, this);
            pressure.GetComponent<RectTransform>().sizeDelta = new Vector2(_pressureInfo[i].Key * Config.blockWidth, _pressureInfo[i].Value * Config.blockHeight - 15);
            float posx = Config.XPosShift - Config.blockWidth / 2;
            float posy = Config.YPosShift - (_totalRowCnt - Config.initRows - yOffset) * Config.blockHeight;
            pressure.transform.localPosition = new Vector3(posx, posy, 0);
            pressure.gameObject.name = "pressure + " + pressure.Row;
            // if (_curMaxRowCnt < Config.matrixRows)
            // {
            //     pressure.NeedFall = true;
            //     pressure.fallCnt = -1;
            // }
            // else
            // {
            pressure.IsMoved = true;
            // }
            _PressureMatrix.Add(pressure);
            UpdateMaxCnt();
        }
    }

    public Garbage GetGarbageInst()
    {
        if (_poolIdx >= 50)
            _poolIdx = 0;
        var garbage = _garbagePool[_poolIdx];
        if (garbage == null)
        {
            garbage = new Garbage();
            garbage.ID = _poolIdx;
            _garbagePool[_poolIdx] = garbage;
        }
        _poolIdx++;
        return garbage;
    }

    List<KeyValuePair<int, int>> _pressureInfo = new List<KeyValuePair<int, int>>();
    int PressureWith, PressureHeight;
    private void SetPressureInfo(int count)
    {
        _pressureInfo.Clear();
        KeyValuePair<int, int> item;
        KeyValuePair<int, int> item2;
        switch (count)
        {
            case 4:
                item = new KeyValuePair<int, int>(3, 1);
                _pressureInfo.Add(item);
                break;
            case 5:
                item = new KeyValuePair<int, int>(4, 1);
                _pressureInfo.Add(item);
                break;
            case 6:
                item = new KeyValuePair<int, int>(5, 1);
                _pressureInfo.Add(item);
                break;
            case 7:
                item = new KeyValuePair<int, int>(6, 1);
                _pressureInfo.Add(item);
                break;
            case 8:
                item = new KeyValuePair<int, int>(3, 1);
                item2 = new KeyValuePair<int, int>(4, 1);
                _pressureInfo.Add(item);
                _pressureInfo.Add(item2);
                break;
            case 9:
                item = new KeyValuePair<int, int>(4, 1);
                item2 = new KeyValuePair<int, int>(4, 1);
                _pressureInfo.Add(item);
                _pressureInfo.Add(item2);
                break;
            case 10:
                item = new KeyValuePair<int, int>(5, 1);
                item2 = new KeyValuePair<int, int>(5, 1);
                _pressureInfo.Add(item);
                _pressureInfo.Add(item2);
                break;
            case 11:
                item = new KeyValuePair<int, int>(5, 1);
                item2 = new KeyValuePair<int, int>(6, 1);
                _pressureInfo.Add(item);
                _pressureInfo.Add(item2);
                break;
            case 12:
                item = new KeyValuePair<int, int>(6, 2);
                _pressureInfo.Add(item);
                break;
            case 13:
                item = new KeyValuePair<int, int>(6, 3);
                _pressureInfo.Add(item);
                break;
            default:
                {
                    if (count >= 14 && count <= 19)
                    {
                        item = new KeyValuePair<int, int>(6, 4);
                        _pressureInfo.Add(item);
                    }
                    else if (count >= 20 && count <= 24)
                    {
                        item = new KeyValuePair<int, int>(6, 6);
                        _pressureInfo.Add(item);
                    }
                    else
                        return;
                    break;
                }
        }
    }

    //检查第1行新的方块类型是否会与已生成的方块三连
    public bool CheckRowType(List<BlockData> data, int col, BlockType type)
    {
        if (col < 2)
            return true;
        if (type == data[col - 1].type && data[col - 2].type == data[col - 1].type)
        {
            return false;
        }
        return true;
    }

    //生产一行方块
    public List<BlockData> SpawnBlock()
    {
        List<BlockData> newRow = new List<BlockData>();
        var rand = new System.Random(Util.GetRandomSeed());
        for (int col = 0; col < Config.columns; col++)
        {
            BlockType newType = (BlockType)rand.Next((int)BlockType.B1, (int)BlockType.Count);
            Block aboveBlock = _blockMatrix[1, col];
            BlockType aboveType = aboveBlock != null ? aboveBlock.Type : BlockType.None;
            while (aboveType == newType || !CheckRowType(newRow, col, newType))
            {
                newType = (BlockType)rand.Next((int)BlockType.B1, (int)BlockType.Count);
            }
            BlockData data = new BlockData
            {
                row = 0,
                col = col,
                type = newType,
            };
            newRow.Add(data);
        }
        return newRow;
    }

    public List<BlockData> SpawnBlock(int blockCount, int _row, int index)
    {
        List<BlockData> newRow = new List<BlockData>();
        var rand = new System.Random(Util.GetRandomSeed());
        for (int col = index; col < blockCount + index; col++)
        {
            BlockType newType = (BlockType)rand.Next((int)BlockType.B1, (int)BlockType.Count);
            while (!CheckRowType(newRow, col, newType))
            {
                newType = (BlockType)rand.Next((int)BlockType.B1, (int)BlockType.Count);
            }
            BlockData data = new BlockData
            {
                row = _row,
                col = col,
                type = newType,
            };
            newRow.Add(data);
        }
        return newRow;
    }

    //所有方块上移一行
    public void BringForward()
    {
        int opRow = _curRowCnt;
        while (opRow > 0)
        {
            for (int col = 0; col < Config.initCols; col++)
            {
                var block = _blockMatrix[opRow - 1, col];
                if (block != null)
                {
                    _blockMatrix[opRow, col] = block;
                    _blockMatrix[opRow, col].Row += 1;
                    _blockMatrix[opRow - 1, col] = null;

                    if (_blockMatrix[opRow, col].Row == 1)
                    {
                        _blockMatrix[opRow, col].IsLocked = false;
                        _blockMatrix[opRow, col].IsMoved = true;
                        _blockMatrix[opRow, col].MoveStay = 3;
                    }
                }
            }
            opRow -= 1;
        }
        for (int i = 0; i < _PressureMatrix.Count; i++)
        {
            var item = _PressureMatrix[i];
            item.Row++;
            item.gameObject.name = "pressure + " + item.Row;
        }
    }

    public void AddNewRow(List<BlockData> newRow)
    {
        // 多人模式同步数据
        if (_boardType == CheckerboardType.mine)
        {
            if (IsMultiPlayer())
            {
                var req = new SprotoType.game_new_row.request();
                req.matrix = new List<SprotoType.block_info>();
                foreach (BlockData data in newRow)
                {
                    req.matrix.Add(new SprotoType.block_info
                    {
                        row = data.row,
                        col = data.col,
                        type = (int)data.type,
                    });
                }
                NetSender.Send<Protocol.game_new_row>(req, (data) =>
                {
                    var resp = data as SprotoType.game_new_row.response;
                    Debug.LogFormat("{0} -- new_row response : {1}", _boardType, resp.e);
                    if (resp.e == 0)
                    {
                        DoAddNewRow(newRow);
                    }
                });
            }
            else
            {
                // 己方
                DoAddNewRow(newRow);

                // 对手
                MainManager.Ins._rivalController.DoAddNewRow(newRow);
            }
        }
        _addNewRow = false;
        _delta = 0;
    }

    public PressureBlock GetPressureByRow(int row)
    {
        foreach (var pressure in _PressureMatrix)
        {
            if (pressure.Row == row)
                return pressure;
        }
        return null;
    }

    public void UpdateMaxCnt()
    {
        _curRowCnt = 0;
        foreach (var item in _blockMatrix)
        {
            if (item != null)
                item.gameObject.name = item.Row + "+ " + item.Column;
            if (item != null && item.Type != BlockType.None && item.Row > _curRowCnt)
                _curRowCnt = item.Row;
        }
        _curMaxRowCnt = _curRowCnt;
        foreach (var item in _PressureMatrix)
        {
            if (item.Row > _curMaxRowCnt && item.fallCnt == 0)
                _curMaxRowCnt = item.Row;
        }
        _curRowCnt += 1;
        _curMaxRowCnt += 1;
        Debug.Log(_boardType + " -- _curRowCnt:" + _curRowCnt + " - _curMaxRowCnt:" + _curMaxRowCnt);
    }

    public void AddNewBlock(List<BlockData> newRow, PressureBlock pressure)
    {
        pressure._genBlocks.Clear();
        foreach (BlockData data in newRow)
        {
            var block = Block.CreateBlockObject(data.row, data.col, (int)data.type, _blockBoardObj.transform, this);
            block.transform.localPosition = new Vector3(Config.XPosShift + data.col * Config.blockWidth, (-_totalRowCnt + data.row + 1) * Config.blockHeight, 0);
            block.BlockOperationEvent += OnBlockOperation;
            block.IsLocked = true;
            pressure._genBlocks.Add(block);

            _blockMatrix[data.row, data.col] = block;
        }

        UpdateMaxCnt();
        CheckAlarm();
    }

    // 交换方块处理
    public void DoSwap(Block first, Block second)
    {
        Debug.Log(_boardType + " -- swap first[" + first.Row + "," + first.Column + " - " + first.Type + "] - second[" + second.Row + "," + second.Column + " - " + second.Type + "]");
        if (first.Row == second.Row)
        {
            if (first.moveCnt != 0 || second.moveCnt != 0)
            {
                Debug.LogError(_boardType + " -- swap run error! first.moveCnt=" + first.moveCnt + " - second.moveCnt=" + second.moveCnt);
                ChangeToState(GameBoardState.Idle);
                return;
            }
            first.NeedMove = true;
            first.moveCnt = second.Column - first.Column;
            second.NeedMove = true;
            second.moveCnt = first.Column - second.Column;
        }
        if (first.Column == second.Column)
        {
            if (first.fallCnt != 0 || second.fallCnt != 0)
            {
                Debug.LogError(_boardType + " -- swap run error! first.fallCnt=" + first.fallCnt + " - second.fallCnt=" + second.fallCnt);
                ChangeToState(GameBoardState.Idle);
                return;
            }
            first.NeedFall = true;
            first.fallCnt = second.Row - first.Row;
            second.NeedFall = true;
            second.fallCnt = first.Row - second.Row;
        }
    }

    // 在底部新加一行方块
    public void DoAddNewRow(List<BlockData> newRow)
    {
        // 已有方块整体上移一行
        BringForward();

        foreach (BlockData item in newRow)
        {
            var block = Block.CreateBlockObject(0, item.col, (int)item.type, _blockBoardObj.transform, this);
            block.transform.localPosition = new Vector3(Config.XPosShift + item.col * Config.blockWidth, -_totalRowCnt * Config.blockHeight, 0);
            block.BlockOperationEvent += OnBlockOperation;

            _blockMatrix[0, item.col] = block;
        }
        _totalRowCnt += 1;

        UpdateMaxCnt();
        CheckAlarm();
    }

    // 方块移动结束处理逻辑
    public void BlockMoved(Block block)
    {
        UpdateMaxCnt();

        int row = block.Row;
        int col = block.Column;
        BlockType type = block.Type;
        Garbage garbage = block._garbage;
        int trans = block.ComboTrans;
        Debug.Log(_boardType + " -- block[" + row + "," + col + " - " + block.Type + "] move proc");

        // 空方块，上方方块下落
        if (type == BlockType.None)
        {
            block.IsBlanked = true;
            row = row + 1;
            if (row > Config.rows)
                return;
            Block above = _blockMatrix[row, col];
            while (above != null && above.IsTagged == false && above.moveCnt == 0 && above.fallCnt == 0 && above.IsLocked == false)
            {
                Debug.Log(_boardType + " -- above block[" + above.Row + "," + above.Column + " - " + above.Type + "] fall");
                above.fallCnt = -1;
                above.NeedFall = true;
                // garbage
                if (garbage != null)
                {
                    above._garbage = garbage;
                    above.ComboTrans = block.ComboTrans;
                }
                row = row + 1;
                if (row > Config.rows)
                    return;
                above = _blockMatrix[row, col];
            }

            bool logFlag = false;
            // 处理压力块
            for (int i = 0; i < _PressureMatrix.Count; i++)
            {
                var pressure = _PressureMatrix[i];
                if (pressure.Row > row)
                {
                    if (logFlag)
                        Debug.LogError(_boardType + " -- row:" + row + " -- Row:" + pressure.Row + " -- i:" + i);
                    break;
                }
                bool underFall = true;
                for (int j = 0; j < pressure.xNum && pressure.Row < Config.matrixRows; j++)
                {
                    var item = _blockMatrix[pressure.Row - 1, j];
                    if (item != null && item.Type != BlockType.None && item.fallCnt == 0)
                    {
                        underFall = false;
                        break;
                    }
                }
                if (i > 0)
                {
                    var underPress = _PressureMatrix[i - 1];
                    if (underPress != null && underPress.Row == pressure.Row - 1 && _PressureMatrix[i - 1].fallCnt == 0)
                        underFall = false;
                }
                if (underFall == false)
                    continue;

                if (pressure.fallCnt == 0 && pressure.IsLocked == false)
                {
                    Debug.Log(_boardType + " -- above pressure[" + pressure.Row + " - " + pressure.xNum + "] fall");
                    pressure.NeedFall = true;
                    pressure.fallCnt = -1;
                    logFlag = true;

                    // 处理压力块上的方块
                    row = pressure.Row + 1;
                    if (row < Config.rows)
                    {
                        for (int idx = 0; idx < pressure.xNum; idx++)
                        {
                            above = _blockMatrix[row, idx];
                            if (above != null)
                            {
                                if (above.IsLocked == false && above.fallCnt == 0)
                                {
                                    Debug.Log(_boardType + " -- above block[" + above.Row + "," + above.Column + " - " + above.Type + "] fall");
                                    above.fallCnt = -1;
                                    above.NeedFall = true;
                                    // garbage
                                    if (garbage != null)
                                    {
                                        above._garbage = garbage;
                                        above.ComboTrans = block.ComboTrans;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        // 方块下落
        while (row > 1)
        {
            if (block.fallCnt != 0)
                return;

            PressureBlock underPressure = GetPressureByRow(row - 1);
            if (underPressure != null && underPressure.xNum >= col + 1 && underPressure.fallCnt == 0)
                break;

            Block under = _blockMatrix[row - 1, col];
            if (under != null && under.fallCnt == 0)
                break;

            Debug.Log(_boardType + " -- self block[" + row + "," + col + " - " + type + "] fall");
            block.fallCnt = -1;
            block.NeedFall = true;
            if (_firstSelected == block)
                ChangeToState(GameBoardState.Idle);

            row = row + 1;
            if (row > Config.rows)
                return;
            Block above = _blockMatrix[row, col];
            while (above != null && above.IsTagged == false && above.moveCnt == 0 && above.fallCnt == 0 && above.IsLocked == false)
            {
                Debug.Log(_boardType + " -- above block[" + above.Row + "," + above.Column + " - " + above.Type + "] fall");
                above.fallCnt = -1;
                above.NeedFall = true;
                // garbage
                if (garbage != null)
                {
                    above._garbage = garbage;
                    above.ComboTrans = block.ComboTrans;
                }
                row = row + 1;
                if (row > Config.rows)
                    return;
                above = _blockMatrix[row, col];
            }

            // 处理压力块
            for (int i = 0; i < _PressureMatrix.Count; i++)
            {
                var pressure = _PressureMatrix[i];
                if (pressure.Row > row)
                    break;
                bool underFall = true;
                for (int j = 0; j < pressure.xNum && pressure.Row < Config.matrixRows; j++)
                {
                    var item = _blockMatrix[pressure.Row - 1, j];
                    if (item != null && item.Type != BlockType.None && item.fallCnt == 0)
                    {
                        underFall = false;
                        break;
                    }
                }
                if (i > 0)
                {
                    var underPress = _PressureMatrix[i - 1];
                    if (underPress != null && underPress.Row == pressure.Row - 1 && _PressureMatrix[i - 1].fallCnt == 0)
                        underFall = false;
                }
                if (underFall == false)
                    continue;

                if (pressure.fallCnt == 0 && pressure.IsLocked == false)
                {
                    Debug.Log(_boardType + " -- above pressure[" + pressure.Row + " - " + pressure.xNum + "] fall");
                    pressure.NeedFall = true;
                    pressure.fallCnt = -1;

                    // 处理压力块上的方块
                    row = pressure.Row + 1;
                    if (row < Config.rows)
                    {
                        for (int idx = 0; idx < pressure.xNum; idx++)
                        {
                            above = _blockMatrix[row, idx];
                            if (above != null)
                            {
                                if (above.IsLocked == false && above.fallCnt == 0)
                                {
                                    Debug.Log(_boardType + " -- above block[" + above.Row + "," + above.Column + " - " + above.Type + "] fall");
                                    above.fallCnt = -1;
                                    above.NeedFall = true;
                                    // garbage
                                    if (garbage != null)
                                    {
                                        above._garbage = garbage;
                                        above.ComboTrans = block.ComboTrans;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        if (block.IsTagged)
            return;

        // 匹配消除
        // 左侧
        List<Block> h_matchList = new List<Block>();
        List<Block> v_matchList = new List<Block>();
        for (int i = col - 1; i >= 0; i--)
        {
            var checkBlock = _blockMatrix[row, i];
            if (checkBlock != null && checkBlock.IsLocked == false && checkBlock.Type == type && checkBlock != block && checkBlock.fallCnt == 0)
                h_matchList.Add(checkBlock);
            else
                break;
        }
        // 右侧
        for (int i = col + 1; i < Config.columns; i++)
        {
            var checkBlock = _blockMatrix[row, i];
            if (checkBlock != null && checkBlock.IsLocked == false && checkBlock.Type == type && checkBlock != block && checkBlock.fallCnt == 0)
                h_matchList.Add(checkBlock);
            else
                break;
        }
        if (h_matchList.Count >= 2)
        {
            block.IsTagged = true;
            if (garbage != null)
            {
                if (garbage.EliminateCnt.ContainsKey(trans))
                    garbage.EliminateCnt[trans] += 1;
                else
                    garbage.EliminateCnt.Add(trans, 1);

                if (garbage.Parent.ContainsKey(trans) == false)
                    garbage.Parent.Add(trans, block.transform);
            }
            foreach (var matchedBlock in h_matchList)
            {
                matchedBlock.IsTagged = true;

                // Garbage
                if (garbage != null)
                {
                    matchedBlock._garbage = garbage;
                    matchedBlock.ComboTrans = trans;

                    garbage.EliminateCnt[trans] += 1;
                }
            }
        }
        // 上方
        for (int i = row + 1; i <= _curRowCnt; i++)
        {
            if (i > Config.rows)
                break;
            var checkBlock = _blockMatrix[i, col];
            if (checkBlock != null && checkBlock.IsLocked == false && checkBlock.Type == type && checkBlock != block && checkBlock.fallCnt == 0)
                v_matchList.Add(checkBlock);
            else
                break;
        }
        // 下方
        for (int i = row - 1; i > 0; i--)
        {
            var checkBlock = _blockMatrix[i, col];
            if (checkBlock != null && checkBlock.IsLocked == false && checkBlock.Type == type && checkBlock != block && checkBlock.fallCnt == 0)
                v_matchList.Add(checkBlock);
            else
                break;
        }
        if (v_matchList.Count >= 2)
        {
            if (h_matchList.Count < 2)
            {
                block.IsTagged = true;
                if (garbage != null)
                {
                    if (garbage.EliminateCnt.ContainsKey(trans))
                        garbage.EliminateCnt[trans] += 1;
                    else
                        garbage.EliminateCnt.Add(trans, 1);

                    if (garbage.Parent.ContainsKey(trans) == false)
                        garbage.Parent.Add(trans, block.transform);
                }
            }
            foreach (var matchedBlock in v_matchList)
            {
                matchedBlock.IsTagged = true;

                // Garbage
                if (garbage != null)
                {
                    matchedBlock._garbage = garbage;
                    matchedBlock.ComboTrans = trans;

                    garbage.EliminateCnt[trans] += 1;
                }
            }
        }
        if (garbage != null && (!garbage.EliminateCnt.ContainsKey(trans) || garbage.EliminateCnt[trans] < 3))
        {
            Debug.Log(_boardType + " -- block[" + block.Row + "," + block.Column + " - " + block.Type + "] garbage(" + garbage.ID + ") set null");
            block._garbage = null;
        }

        if (_firstSelected == block && block.IsTagged == false)
            _controller.ChangeToState(GameBoardState.Selection);
    }

    // 压力块移动结束处理逻辑
    public void PressureMoved(PressureBlock pressure)
    {
        UpdateMaxCnt();

        Debug.Log(_boardType + " -- pressure[" + pressure.Row + " - " + pressure.xNum + "] move proc");
        bool underFall = true;
        // 判断下方的压力块
        foreach (var item in _PressureMatrix)
        {
            if ((pressure.Row == item.Row + 1) && (item.fallCnt == 0))
            {
                Debug.Log(_boardType + " -- pressure.Row:" + pressure.Row + " - item.Row:" + item.Row + " - item.fallCnt:" + item.fallCnt);
                underFall = false;
                break;
            }
        }
        if (underFall == false)
            return;

        // 判断下方的方块
        if (pressure.Row <= Config.matrixRows)
        {
            for (int j = 0; j < pressure.xNum; j++)
            {

                var item = _blockMatrix[pressure.Row - 1, j];
                if (item != null && item.Type != BlockType.None && item.fallCnt == 0)
                {
                    underFall = false;
                    break;
                }
            }
        }
        if (underFall == false)
            return;

        // 压力块下落
        Debug.Log(_boardType + " -- self pressure[" + pressure.Row + " - " + pressure.xNum + "] fall");
        if (pressure.fallCnt == 0 && pressure.IsLocked == false)
        {
            pressure.NeedFall = true;
            pressure.fallCnt = -1;
        }

        // 处理压力块上方的方块
        int row = pressure.Row + 1;
        if (row <= Config.rows)
        {
            for (int i = 0; i < pressure.xNum; i++)
            {
                Block above = _blockMatrix[row, i];
                if (above != null && above.IsLocked == false)
                {
                    Debug.Log(_boardType + " -- above block[" + above.Row + "," + above.Column + " - " + above.Type + "] fall");
                    above.fallCnt = -1;
                    above.NeedFall = true;
                }
            }
        }
    }

    // 方块被消除时处理压力块
    public void BlockTagged(Block block)
    {
        int row = block.Row;
        int col = block.Column;
        foreach (var pressure in _PressureMatrix)
        {
            if ((pressure.Row == row + 1) && (pressure.xNum > col) && (pressure.IsLocked == false))
            {
                pressure.IsTagged = true;
                break;
            }
        }
    }

    // 解锁压力块逻辑
    public void PressureTagged(PressureBlock pressure)
    {
        Debug.Log(_boardType + " -- pressure[" + pressure.Row + " - " + pressure.xNum + "] tag proc");
        if (_boardType == CheckerboardType.mine)
        {
            var newBlocks = SpawnBlock(pressure.xNum, pressure.Row, 0);
            var req = new SprotoType.createBlock.request();
            req.matrix = new List<SprotoType.block_info>();
            foreach (BlockData data in newBlocks)
            {
                req.matrix.Add(new SprotoType.block_info
                {
                    row = data.row,
                    col = data.col,
                    type = (int)data.type,
                });
            }
            if (IsMultiPlayer())
            {
                NetSender.Send<Protocol.createBlock>(req, (data) =>
                {
                    var resp = data as SprotoType.createBlock.response;
                    Debug.LogFormat("{0} -- new_row response : {1}", _boardType, resp.e);
                    if (resp.e == 0)
                    {
                        pressure.PlayUnlockAnim();
                        Debug.Log(_boardType + " -- createBlock response pressure[" + pressure.Row + " - " + pressure.xNum + "]");
                        AddNewBlock(newBlocks, pressure);
                    }
                });
            }
            else
            {
                // 自己
                pressure.PlayUnlockAnim();
                AddNewBlock(newBlocks, pressure);

                // 对手
                var rival = MainManager.Ins._rivalController;
                foreach (PressureBlock item in rival._PressureMatrix)
                {
                    if (item.Row == pressure.Row)
                    {
                        item.PlayUnlockAnim();
                        rival.AddNewBlock(newBlocks, item);
                        break;
                    }
                }
            }
        }
    }

    private void SendNet(List<BlockData> _list, PressureBlock pressure)
    {
        var req = new SprotoType.createBlock.request();
        req.matrix = new List<SprotoType.block_info>();
        foreach (BlockData data in _list)
        {
            req.matrix.Add(new SprotoType.block_info
            {
                row = data.row,
                col = data.col,
                type = (int)data.type,
            });
        }
        NetSender.Send<Protocol.createBlock>(req, (data) =>
        {
            var resp = data as SprotoType.createBlock.response;
            Debug.LogFormat("{0} -- new_row response : {1}", _boardType, resp.e);
            if (resp.e == 0)
            {
                pressure.PlayUnlockAnim();
                Debug.Log(_boardType + " -- createBlock response pressure[" + pressure.Row + " - " + pressure.xNum + "]");
                AddNewBlock(_list, pressure);
            }
        });
    }

    public void UpdateBlockArea()
    {
        Dictionary<Garbage, int> showGarbage = new Dictionary<Garbage, int>();

        // 方块
        for (int row = 0; row < Config.matrixRows; row++)
        {
            for (int col = 0; col < Config.columns; col++)
            {
                var block = _blockMatrix[row, col];
                if (block != null)
                {
                    var garbage = block._garbage;
                    if (garbage != null && !showGarbage.ContainsKey(garbage))
                    {
                        showGarbage.Add(garbage, block.ComboTrans);
                    }

                    block.LogicUpdate();
                }
            }
        }

        // 压力块
        foreach (var pressure in _PressureMatrix)
        {
            pressure.LogicUpdate();
        }

        // Garbage
        foreach (var item in showGarbage)
        {
            var garbage = item.Key;
            var trans = item.Value;
            if (!garbage.EliminateCnt.ContainsKey(trans))
                continue;

            int cnt = garbage.EliminateCnt[trans];
            // Debug.Log(_boardType + " -- garbage:" + garbage.ID + " - trans:" + trans + " - cnt:" + cnt);
            // Combo
            if (cnt > 3 && !garbage.ComboShowed.Contains(trans))
            {
                garbage.ComboShowed.Add(trans);
                if (_boardType == CheckerboardType.mine)
                {
                    if (IsMultiPlayer())
                    {
                        var req = new SprotoType.eliminate.request();
                        req.count = cnt;
                        Debug.Log(_boardType + " -- send eliminate count: " + cnt);
                        NetSender.Send<Protocol.eliminate>(req, (data) =>
                        {
                            var resp = data as SprotoType.eliminate.response;
                            Debug.LogFormat("{0} -- eliminate response: {1}", _boardType, resp.e);
                            if (resp.e == 0)
                            {
                                Debug.Log("_boardType +  -- show combo - garbage:" + garbage.ID + " - trans:" + trans);
                                // 己方显示combo
                                GameObject obj = GameObject.Instantiate(Config._comboObj, _effectObj.transform) as GameObject;
                                Vector3 relative = garbage.Parent[trans].localPosition;
                                obj.transform.localPosition = new Vector3(relative.x, relative.y, 0);
                                Combo comb = obj.gameObject.GetComponent<Combo>();
                                comb.Show(cnt, this);

                                // 对手生成压力块
                                MainManager.Ins._rivalController.GreatPressureBlock(cnt);
                            }
                            else
                            {
                                Debug.LogError(_boardType + " -- eliminate response failed!");
                            }
                        });
                    }
                    else
                    {
                        Debug.Log(_boardType + " -- show combo - garbage:" + garbage.ID + " - trans:" + trans);
                        // 己方显示combo
                        GameObject obj = GameObject.Instantiate(Config._comboObj, _effectObj.transform) as GameObject;
                        Vector3 relative = garbage.Parent[trans].localPosition;
                        obj.transform.localPosition = new Vector3(relative.x, relative.y, 0);
                        Combo comb = obj.gameObject.GetComponent<Combo>();
                        comb.Show(cnt, this);

                        // 自己生成压力块
                        GreatPressureBlock(cnt);

                        // 对手生成压力块
                        MainManager.Ins._rivalController.GreatPressureBlock(cnt);
                    }
                }
            }
            //Chain
            if (cnt >= 3 && !garbage.ChainShowed.Contains(trans))
            {
                garbage.ChainShowed.Add(trans);
                garbage.ChainCnt += 1;
                if (garbage.ChainCnt >= 2)
                {
                    Debug.Log(_boardType + " -- show chain - garbage:" + garbage.ID + " - trans:" + trans);
                    GameObject obj = GameObject.Instantiate(Config._chainObj, _effectObj.transform) as GameObject;
                    Vector3 relative = garbage.Parent[trans].localPosition;
                    obj.transform.localPosition = new Vector3(relative.x, relative.y, 0);
                    Chain chain = obj.gameObject.GetComponent<Chain>();
                    chain.chainNum = garbage.ChainCnt;
                }
            }
            // star
            if (cnt >= 3 && !garbage.StarShowed.Contains(trans))
            {
                garbage.StarShowed.Add(trans);
                GameObject star = GameObject.Instantiate(Config._starObj, _effectObj.transform) as GameObject;
                Vector3 relative = garbage.Parent[trans].localPosition;
                star.transform.localPosition = new Vector3(relative.x, relative.y, 0);
                Transform role = Config.gameObj.transform.Find("Player1/role");
                star.transform.DOMove(role.position, 1.0f).OnComplete(() =>
                {
                    GameObject.Destroy(star);
                });
            }
            if (cnt >= 3)
            {
                if (this is MainController)
                    MainManager.Ins._mainController._minRoleData.UpdateSkill2(cnt);
                if (MainManager.Ins._mainController._minRoleData.IsRecoverHpSKill && this is RivalController)
                {
                    if (cnt > 3 && cnt - 3 <= 8 && MainManager.Ins.Timer < MainManager.Ins._mainController._minRoleData.UseRecoverSkillTime)
                        MainManager.Ins._mainController.ShowMinEffect(SKillType.huixue);
                }
                else if (MainManager.Ins._mainController._emmyRoleData.IsRecoverHpSKill && this is MainController)
                {
                    if (cnt > 3 && cnt - 3 <= 8 && MainManager.Ins.Timer < MainManager.Ins._mainController._emmyRoleData.UseRecoverSkillTime)
                        MainManager.Ins._mainController.ShowEmmeyEffect(SKillType.huixue);
                }
                MainManager.Ins._mainController._minRoleData.UpdateSkill2(cnt);
                MainManager.Ins._mainController.UpdateSkill2Cd();
            }
        }
    }
}