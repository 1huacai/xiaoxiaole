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
            return;

        if (!_gameStart)
            return;

        if (_raiseOneRow)
        {
            RaiseOneRow();
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

    public override bool IsMultiPlayer()
    {
        return false;
    }

    public override void ChangeScore(int score, int combo_cnt)
    {
        base.ChangeScore(score, combo_cnt);
    }

    public void SyncSwapBlock(SprotoType.sync_swap_block.request data)
    {
        Debug.LogFormat("-------- SyncSwapBlock block1[{0},{1}], block2[{2},{3}]", data.block1.row, data.block1.col, data.block2.row, data.block2.col);
        OnBlockOperation((int)data.block1.row, (int)data.block1.col, BlockOperation.TouchDown);
        OnBlockOperation((int)data.block2.row, (int)data.block2.col, BlockOperation.TouchDown);
    }

    public void SyncUpRow(SprotoType.sync_up_row.request data)
    {
        _raiseOneRow = true;
    }

    public void SyncNewRow(SprotoType.sync_new_row.request data)
    {
        List<BlockData> newRow = new List<BlockData>();
        foreach(SprotoType.block_info info in data.row_data)
        {
            newRow.Add(new BlockData
            {
               row = (int)info.row,
               col = (int)info.col,
               type = (BlockType)info.type,
            });
            Debug.LogFormat("--- syncNewRow, [{0},{1},{2}]", info.row, info.col, info.type);
        }
        AddNewRow(newRow);
        //_curRowCnt = (int)data.cur_row_cnt;
        //_totalRowCnt = (int)data.total_row_cnt;
    }

    public void SyncScore(SprotoType.sync_score.request data)
    {
        int comboCnt = (int)data.combo_cnt;
        ChangeScore((int)data.score, comboCnt);
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
