using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class GameController : MonoBehaviour
{
    public GameController _controller;

    public GameObject _blockBoardObj; // 方块面板区
    public GameObject _blockAreaObj; // 方块面板区底图

    public Dictionary<GameBoardState, ControllerStateBase> _states;
    public GameBoardState _curGameBoardState = GameBoardState.Idle;

    public Block _firstSelected = null;
    public Block _secondSelected = null;

    public static List<SprotoType.block_info> _initMatrix;
    public static bool _gameReady = true;
    public static bool _gameStart = false;
    public static bool _gameOver = false;

    public Block[,] _blockMatrix; // 方块行列矩阵
    public List<PressureBlock> _PressureMatrixList = new List<PressureBlock>();
    public List<Dictionary<int, Vector3>> _rowY_pos = new List<Dictionary<int, Vector3>>();

    public bool _multiPlayer = false;
    public bool isExMode = false;

    public bool _isSwappingDone = false;
    public bool _isFallingDone = false;
    public bool _isPressureFallingDone = false;
    public int _curRowCnt = 0;
    public int _totalRowCnt = 0;

    public int _curMaxRowCnt = 0;

    public bool _alarmSet = false; // 危险警报
    public bool _suspendRaise = false;

    public int _totalRemoveCnt = 0;
    public int _speedRemoveCnt = 0;
    public int _curSpeed = 0;
    public int _curRaiseTime;

    public int _chainCnt = 0; //连消计数

    public bool _raiseOneRow = false;
    public bool _addNewRow = false;
    public bool _addNewPressureRow = false;
    public int _score = 0;

    public float _delta = 0;

    void Awake()
    { }

    void Start()
    { }

    void Update()
    { }

    void OnDestroy()
    { }

    public void InitStateHandlers()
    {
        _states = new Dictionary<GameBoardState, ControllerStateBase>();

        _states.Add(GameBoardState.Idle, new IdleState(this));
        //_states.Add(GameBoardState.Spawn, new SpawnState(this));
        _states.Add(GameBoardState.FirstSelection, new FirstSelectionState(this));
        _states.Add(GameBoardState.SecondSelection, new SecondSelectionState(this));
        _states.Add(GameBoardState.Swap, new SwapState(this));
        //_states.Add(GameBoardState.ReverseSwap, new ReverseSwapState(this));
        _states.Add(GameBoardState.Fall, new FallState(this));
        _states.Add(GameBoardState.Blank, new BlankState(this));
        //_states.Add(GameBoardState.Destroy, new DestroyState(this));
    }

    public void InitMembers()
    {
        _blockBoardObj = transform.Find("BlockBoard").gameObject;
        _blockAreaObj = transform.Find("BlockBoard/AreaBottom").gameObject;

        _gameReady = true;
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
        Debug.Log(string.Format("Change to new state:{0}", newState.ToString()));
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

    virtual public bool IsMultiPlayer()
    {
        return false;
    }

    virtual public void ChangeScore(int score, int combo_cnt)
    {
        _score += score;

        _totalRemoveCnt++;
        _speedRemoveCnt++;
        if (_speedRemoveCnt >= Config.panelsToNextSpeed[_curSpeed])
        {
            _speedRemoveCnt = 0;
            _curSpeed++;
            _curRaiseTime = Config.speedToRaiseTime[_curSpeed];
        }
    }

    public void CheckAlarm()
    {
        for (int i = 0; i < _PressureMatrixList.Count; i++)
        {
            if (_PressureMatrixList[i].Row_y >= Config.alarmRow)
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
                    if (block.IsTrembled == false)
                        block.TrembleChange(true);
                }
            }
            else
            {
                for (int row = 0; row < _curRowCnt; row++)
                {
                    var item = _blockMatrix[row, col];
                    if (item.IsTrembled)
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
        _suspendRaise = true;
        var moveDis = (_totalRowCnt - Config.initRows + 1) * Config.blockHeight;
        _blockBoardObj.transform.DOLocalMoveY(moveDis, 0.2f).OnComplete(() =>
        {
            _blockAreaObj.transform.DOLocalMoveY(_blockAreaObj.transform.localPosition.y - Config.blockHeight, 0);
            _addNewRow = true;
            _suspendRaise = false;
        });
    }

    public void RaiseOneStep()
    {
        _blockBoardObj.transform.DOLocalMoveY(_blockBoardObj.transform.localPosition.y + Config.raiseDis, 0);

        if (_blockBoardObj.transform.localPosition.y > (_totalRowCnt - Config.initRows + 1) * Config.blockHeight - 15)
        {
            _blockAreaObj.transform.DOLocalMoveY(_blockAreaObj.transform.localPosition.y - Config.blockHeight, 0);
            _addNewRow = true;
        }
        _delta = 0;
    }

    public void UpdateState()
    {
        if (_states != null && _states.ContainsKey(_curGameBoardState))
        {
            _states[_curGameBoardState].Update();
        }
    }

    //生成初始的方块
    public void InitBlocks()
    {
        _blockMatrix = new Block[Config.matrixRows, Config.columns];
        foreach (SprotoType.block_info data in _initMatrix)
        {
            int row = (int)data.row;
            int col = (int)data.col;
            var block = Block.CreateBlockObject(row, col, (int)data.type, _blockBoardObj.transform);
            block.transform.localPosition = new Vector3(Config.blockXPosShit + col * Config.blockWidth, -(Config.initRows - 1 - row) * Config.blockHeight, 0);
            block.BlockOperationEvent += OnBlockOperation;

            _blockMatrix[row, col] = block;
        }
        _curRowCnt = Config.initRows;
        _totalRowCnt = Config.initRows;
    }
    public void GreatPressureBlock(int count)
    {
        SetPressureInfo(count);
        for (int i = 0; i < _pressureInfo.Count; i++)
        {
            var block = PressureBlock.CreateBlockObject(Config.rows, 0, (int)PressureBlockType.D1, _blockBoardObj.transform);
            block.GetComponent<RectTransform>().sizeDelta = new Vector2(_pressureInfo[i].Key * Config.blockWidth, _pressureInfo[i].Value * Config.blockHeight - 1);
            block.transform.localPosition = new Vector3((Config.blockXPosShit - Config.blockWidth / 2 + block.xNum * Config.blockWidth), (block.Row_y - 1) * Config.blockHeight + Config.StartPosY + _blockAreaObj.transform.localPosition.y, 0);//block.GetComponent<RectTransform>().rect.width / 2  // + block.Column_x * Config.blockWidth
            //block.BlockOperationEvent += OnBlockOperation;
            block.gameObject.name = "1111";
            block.xNum = _pressureInfo[i].Key;
            int _row = 0;
            _PressureMatrixList.Add(block);
        }
        _PressureMatrixList.Sort((a, b) => a.Row_y - b.Row_y);
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
            BlockType aboveType = _blockMatrix[1, col].Type;
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
                }
            }
            opRow -= 1;
        }
        for (int i = 0; i < _PressureMatrixList.Count; i++)
        {
            var item = _PressureMatrixList[i];
            item.Row_y++;
        }
    }

    public void AddNewRow(List<BlockData> newRow)
    {
        //已有方块整体上移一行
        BringForward();

        // 多人模式同步数据
        if (IsMultiPlayer())
        {
            var req = new SprotoType.new_row.request();
            req.row_data = new List<SprotoType.block_info>();
            foreach (BlockData data in newRow)
            {
                req.row_data.Add(new SprotoType.block_info
                {
                    row = data.row,
                    col = data.col,
                    type = (int)data.type,
                });
            }
            req.cur_row_cnt = _curRowCnt;
            req.total_row_cnt = _totalRowCnt;
            NetSender.Send<Protocol.new_row>(req, (data) =>
            {
                var resp = data as SprotoType.new_row.response;
                Debug.LogFormat(" new_row response : {0}", resp.e);
                if (resp.e == 0)
                {

                }
            });
        }
        foreach (BlockData data in newRow)
        {
            var block = Block.CreateBlockObject(0, data.col, (int)data.type, _blockBoardObj.transform);
            block.transform.localPosition = new Vector3(Config.blockXPosShit + data.col * Config.blockWidth, -_totalRowCnt * Config.blockHeight, 0);
            block.BlockOperationEvent += OnBlockOperation;

            _blockMatrix[0, data.col] = block;
        }
        _curRowCnt += 1;
        _totalRowCnt += 1;

        CheckAlarm();

        if (CalculateSwappedBlocks())
        {
            ChangeToState(GameBoardState.Blank);
        }
        foreach (var item in _blockMatrix)
        {
            if (item != null)
                item.gameObject.name = item.Row + "+ " + item.Column;
        }

        _addNewRow = false;
        _delta = 0;
    }

    public bool CalculateSwappedBlocks()
    {
        var minRowIndex = 0;
        var maxRowIndex = _curRowCnt;
        var minColumnIndex = 0;
        var maxColumnIndex = Config.columns;

        var blockToCheckList = new List<Block>();
        blockToCheckList.Add(_firstSelected);
        blockToCheckList.Add(_secondSelected);

        var hasMatchedBlocks = false;
        foreach (var block in _blockMatrix)
        {
            if (block == null)
                continue;

            // Horizontal
            var leftMostColumnIndex = block.Column;
            var rightMostColumnIndex = block.Column;
            // Left
            for (int col = block.Column; col >= minColumnIndex; col--)
            {
                var leftBlock = _controller._blockMatrix[block.Row, col];
                var currentBlock = _controller._blockMatrix[block.Row, leftMostColumnIndex];
                if (leftBlock.IsMatched(currentBlock))
                {
                    leftMostColumnIndex = col;
                }
                else
                {
                    break;
                }
            }
            // Right
            for (int col = block.Column; col < maxColumnIndex; col++)
            {
                var rightBlock = _controller._blockMatrix[block.Row, col];
                var currentBlock = _controller._blockMatrix[block.Row, rightMostColumnIndex];
                if (rightBlock.IsMatched(currentBlock))
                {
                    rightMostColumnIndex = col;
                }
                else
                {
                    break;
                }
            }

            // Vertical
            var upperMostRowIndex = block.Row;
            var lowestRowIndex = block.Row;
            // Up
            for (int row = block.Row; row >= minRowIndex; row--)
            {
                var upperBlock = _controller._blockMatrix[row, block.Column];
                var currentBlock = _controller._blockMatrix[upperMostRowIndex, block.Column];
                if (upperBlock.IsMatched(currentBlock))
                {
                    upperMostRowIndex = row;
                }
                else
                {
                    break;
                }
            }
            // Low
            for (int row = block.Row; row < maxRowIndex; row++)
            {
                var lowerBlock = _controller._blockMatrix[row, block.Column];
                var currentBlock = _controller._blockMatrix[lowestRowIndex, block.Column];
                if (lowerBlock.IsMatched(currentBlock))
                {
                    lowestRowIndex = row;
                }
                else
                {
                    break;
                }
            }

            if (rightMostColumnIndex - leftMostColumnIndex >= 2)
            {
                hasMatchedBlocks = true;
                for (int i = leftMostColumnIndex; i <= rightMostColumnIndex; i++)
                {
                    var current = _controller._blockMatrix[block.Row, i];
                    current.IsTagged = true;

                    for (int j = 0; j < _PressureMatrixList.Count; j++)
                    {
                        var item = current;
                        if (item.Row + 1 == _PressureMatrixList[j].Row_y && _PressureMatrixList[j].Column_x <= item.Column && item.Column <= _PressureMatrixList[j].Column_x + _PressureMatrixList[j].xNum - 1)
                            _PressureMatrixList[j].IsTagged = true;
                    }
                }
            }
            if (lowestRowIndex - upperMostRowIndex >= 2)
            {
                hasMatchedBlocks = true;
                for (int i = upperMostRowIndex; i <= lowestRowIndex; i++)
                {
                    var current = _controller._blockMatrix[i, block.Column];
                    current.IsTagged = true;

                    for (int j = 0; j < _PressureMatrixList.Count; j++)
                    {
                        if (current.Row + 1 == _PressureMatrixList[j].Row_y && _PressureMatrixList[j].Column_x <= current.Column && current.Column <= _PressureMatrixList[j].Column_x + _PressureMatrixList[j].xNum - 1)
                            _PressureMatrixList[j].IsTagged = true;
                    }
                }
            }
        }
        return hasMatchedBlocks;
    }

    public void DestroyBlock(int row, int column)
    {
        var item = _controller._blockMatrix[row, column];
        if (item != null)
        {
            item.BlockOperationEvent -= OnBlockOperation;
            item.DoDestroy();
            item = null;
        }
    }

    public void DestroyBlankRow()
    {
        for (int row = _curRowCnt - 1; row > 0; row--)
        {
            for (int col = 0; col < Config.columns; col++)
            {
                var item = _controller._blockMatrix[row, col];
                if (item != null  && _controller._blockMatrix[row, col].Type != BlockType.None)
                {
                    return;
                }
            }
            for (int col = 0; col < Config.columns; col++)
            {
                DestroyBlock(row, col);
            }
            CheckAlarm();
            _curRowCnt -= 1;
        }
    }
    public void DestroyPBlockRow()
    {
        for (int i = 0; i < _controller._PressureMatrixList.Count; i++)
        {
            if (_controller._PressureMatrixList[i].IsTagged)
            {
                var pblok = _controller._PressureMatrixList[i];
                _controller._PressureMatrixList.Remove(pblok);
                pblok.DoDestroy();
                Debug.LogError("destroy pblok");
                _controller._curMaxRowCnt -= 1;
            }
        }
    }
    public void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        if (_states.ContainsKey(_curGameBoardState))
        {
            _states[_curGameBoardState].OnBlockOperation(row, column, operation);
        }
    }

    public Block GetBlockByX_Y(int row, int col)
    {
        foreach (var item in _blockMatrix)
        {
            if (item != null)
                if (item.Row == row && item.Column == col)
                    return item;
        }
        return null;
    }

}