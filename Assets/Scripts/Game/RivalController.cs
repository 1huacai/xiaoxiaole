using System;
using System.Collections.Generic;
using UnityEngine;


public class RivalController : GameController
{
    void Awake()
    {
        _controller = this;
        Config.msgHandler._rivalControllerInst = this;
    }

    void Start()
    {
        // state handler init        
        InitStateHandlers();

        // variable init     
        InitMembers();

        _gameInit = true;
    }

    int cntInit = 0;
    void FixedUpdate()
    {
        if (_gameInit)
        {
            if (cntInit % 5 == 0)
            {
                InitBlocks();
            }
            cntInit++;
            return;
        }
    }

    void Update()
    {
        _delta += Time.deltaTime;
        if (_gameInit || _gameOver)
            return;

        // if (_gameReady)
        //     return;

        if (!_gameStart)
            return;

        UpdateState();

        if (_curRowCnt > Config.rows || _curMaxRowCnt > Config.rows)
        {
            // TouchTop();
            return;
        }

        if (_raiseOneRow)
        {
            RaiseOneRow();
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

    public override bool IsMultiPlayer()
    {
        return _multiPlayer;
    }

    public void SyncSwapBlock(SprotoType.game_swap_broadcast.request data)
    {
        Debug.LogFormat("{0} -- SyncSwapBlock block1[{1},{2}], block2[{3},{4}]", _boardType, data.block1.row, data.block1.col, data.block2.row, data.block2.col);
        int row1 = (int)data.block1.row;
        int col1 = (int)data.block1.col;
        int row2 = (int)data.block2.row;
        int col2 = (int)data.block2.col;
        Block first = _controller._blockMatrix[row1, col1];
        Block second = _controller._blockMatrix[row2, col2];
        if (second == null)
        {
            Block block = Block.CreateBlockObject(row2, col2, (int)data.block2.type, _controller._blockBoardObj.transform, _controller);
            block.transform.localPosition = new Vector3(0, 0, -1);
            _controller._blockMatrix[row2, col2] = block;
            second = block;
        }
        DoSwap(first, second);
    }

    public void SyncUpRow(SprotoType.game_up_row_broadcast.request data)
    {
        _raiseOneRow = true;
    }

    public void SyncNewRow(SprotoType.game_new_row_broadcast.request data)
    {
        List<BlockData> newRow = new List<BlockData>();
        foreach (SprotoType.block_info info in data.matrix)
        {
            newRow.Add(new BlockData
            {
                row = (int)info.row,
                col = (int)info.col,
                type = (BlockType)info.type,
            });
            Debug.LogFormat("{0} -- syncNewRow, [{1},{2} - {3}]", _boardType, info.row, info.col, info.type);
        }
        DoAddNewRow(newRow);
    }

    public void CreateBlock(SprotoType.createBlock_broadcast.request data)
    {
        int row = -1;
        List<BlockData> newRow = new List<BlockData>();
        foreach (SprotoType.block_info info in data.matrix)
        {
            row = (int)info.row;
            newRow.Add(new BlockData
            {
                row = (int)info.row,
                col = (int)info.col,
                type = (BlockType)info.type,
            });
            Debug.LogFormat("{0} -- createBlock_broadcast, [{1},{2},{3}]", _boardType, info.row, info.col, info.type);
        }
        foreach (PressureBlock pressure in _PressureMatrix)
        {
            if (pressure.Row_y == row)
            {
                pressure.PlayUnlockAnim();
                AddNewBlock(newRow, pressure);
                break;
            }
        }
    }
}
