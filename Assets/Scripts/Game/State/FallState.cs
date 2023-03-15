using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// fall state class
class FallState : ControllerStateBase
{
    private float _time = 0;
    public FallState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    bool FallBlocks(Block blankBlock)
    {
        bool ret = false;

        var col = blankBlock.Column;
        float moveDuration = 0.1f;
        int fallCnt = 0;
        int fallStep = 0;
        var raiseBlock = new Dictionary<int, Block>();
        for (int i = blankBlock.Row + 1; i < _controller._curRowCnt; i++)
        {
            var fallBlock = _controller._blockMatrix[i, col];
            if (fallBlock.Type != BlockType.None)
            {
                if (fallStep == 0)
                    fallStep = fallBlock.Row - blankBlock.Row;

                float fallDis = fallBlock.transform.localPosition.y - fallStep * Config.blockHeight;
                fallBlock.transform.DOLocalMoveY(fallDis, moveDuration);

                if (_controller._blockMatrix[fallBlock.Row - fallStep, col].Type == BlockType.None)
                    raiseBlock[fallCnt] = _controller._blockMatrix[fallBlock.Row - fallStep, col];
                _controller._blockMatrix[fallBlock.Row - fallStep, col] = fallBlock;
                fallBlock.Row = blankBlock.Row + fallCnt;

                fallCnt++;
                ret = true;
            }
        }

        for (int i = fallStep; i > 0; i--)
        {
            Block block;
            if (i <= raiseBlock.Count)
            {
                block = raiseBlock[i - 1];
            }
            else
            {
                block = _controller._blockMatrix[blankBlock.Row + i - 1, col];
            }

            _controller._blockMatrix[block.Row + fallCnt, col] = block;
            block.Row += fallCnt;
            float fallDis = block.transform.localPosition.y + fallCnt * Config.blockHeight;
            if (i == 1)
            {
                block.transform.DOLocalMoveY(fallDis, moveDuration).OnComplete(() =>
                {
                    _controller._isFallingDone = true;
                });
            }
            else
            {
                block.transform.DOLocalMoveY(fallDis, moveDuration);
            }
        }
        _controller.CheckAlarm();
        return ret;
    }
    bool FallBlocks(PressureBlock blankBlock)
    {
        bool ret = false;

        var col = blankBlock.Column;
        float moveDuration = 0.1f;
        int fallCnt = 0;
        int fallStep = 0;
        var raiseBlock = new Dictionary<int, PressureBlock>();
        if (_controller._curMaxRowCnt == 0) _controller._curMaxRowCnt = _controller._curRowCnt;
         fallStep = blankBlock.Row - _controller._curMaxRowCnt <= 1 ? 0 :   Config.rows - _controller._curRowCnt;
        if (fallStep == 0)
        {
            Debug.LogError("FallBlocks");
            return ret;
        }
        fallCnt = 1;
        PressureBlock block = blankBlock;

        block.Row -= fallStep;
        float fallDis = block.transform.localPosition.y - fallStep * Config.blockHeight;
        block.transform.DOLocalMoveY(fallDis, moveDuration).OnComplete(() =>
        {
            _controller._isPressureFallingDone = true;
        });
        foreach (var item in _controller._PressureMatrixList)
        {
            if (item.Row > _controller._curMaxRowCnt)
                _controller._curMaxRowCnt = item.Row;
        }
        _controller.CheckAlarm();
        return ret;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("Enter");
        _time = 0;

        bool hasFalled = false;
        bool hasPBlockFalled = false;
        for (int row = 1; row < _controller._curRowCnt; row++)
        {
            for (int col = 0; col < Config.columns; col++)
            {
                var underBlock = _controller._blockMatrix[row - 1, col];
                if (underBlock.Type == BlockType.None)
                {
                    hasFalled = FallBlocks(underBlock) || hasFalled;
                }
            }
        }

        if (_controller._PressureMatrixList.Count > 0)
        {
            for (int i = 0; i < _controller._PressureMatrixList.Count; i++)
                hasPBlockFalled = FallBlocks(_controller._PressureMatrixList[i]) || hasPBlockFalled;
        }

        {
            //for (int row = 1; row < _controller._curRowCnt; row++)
            //{
            //    for (int col = 0; col < Config.columns; col++)
            //    {
            //        var underBlock = _controller._blockMatrix[row - 1, col];//_controller._PressureMatrixList[i].Row
            //        if (underBlock == null || underBlock.Type == BlockType.None)
            //        {
            //            for (int i = 0; i < _controller._PressureMatrixList.Count; i++)
            //            {
            //                //var _underBlock = _controller._PressureMatrixList[i];
            //                if (underBlock.Row + 1 == _controller._PressureMatrixList[i].Row  && _controller._PressureMatrixList[i].Column <= underBlock.Column && _controller._PressureMatrixList[i].Column + _controller._PressureMatrixList[i].xNum - 1 >= underBlock.Column)
            //                {
            //                    hasFalled = FallBlocks(_controller._PressureMatrixList[i]) || hasFalled;
            //                    Debug.LogError("_PressureMatrixList");
            //                }
            //            }
            //        }
            //    }
            //}
        }

        if (hasFalled)
        {
            if (_controller._firstSelected)
                _controller._firstSelected.IsSelected = false;
            if (_controller._secondSelected)
                _controller._secondSelected.IsSelected = false;
        }
        else
        {
            bool hasMatched = _controller.CalculateSwappedBlocks();
            if (hasMatched)
                _controller.ChangeToState(GameBoardState.Blank);
            else
                _controller.ChangeToState(GameBoardState.Idle);
        }
    }

    public override void Update()
    {
        base.Update();
        _time += Time.deltaTime;
        if (_controller._isFallingDone)
        {
            Debug.LogError("DestroyBlankRow");
            _controller.DestroyBlankRow();

        }
        if (_controller._isPressureFallingDone)
        { 
            //_controller._isPressureFallingDone = false;
            _controller.DestroyPBlockRow();
            bool hasFalled = false;
            if (_controller._PressureMatrixList.Count > 0)
            {
                for (int i = 0; i < _controller._PressureMatrixList.Count; i++)
                    hasFalled = FallBlocks(_controller._PressureMatrixList[i]) || hasFalled;
            }
        }
        if (_controller._isFallingDone || _controller._isPressureFallingDone)
        {
            bool hasMatchedGames = _controller.CalculateSwappedBlocks();
            if (hasMatchedGames)
                _controller.ChangeToState(GameBoardState.Blank);
            else
                _controller.ChangeToState(GameBoardState.Idle);
        }
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit");
    }
}
