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
    void DoMovePressureBlock(PressureBlock pressureBlock)
    {
        //int valueStep = CheckDownHasPblok(pressureBlock);

        int fallStep = 0;// valueStep == 0 ? 0 : valueStep;
        int maxY = GetMaxYIndex(pressureBlock);
        fallStep = maxY <= 0 ? pressureBlock.Row_y : pressureBlock.Row_y - maxY <= 0 ? pressureBlock.Row_y : pressureBlock.Row_y - maxY;
        Debug.LogError(pressureBlock.Row_y  + "__________________________" + maxY);
        PressureBlock block = pressureBlock;
        //if (block.transform.localPosition.y / Config.blockHeight != pressureBlock.Row_y)
        //{
        //    _controller._curMaxRowCnt = _controller._curRowCnt + _controller._PressureMatrixList.Count;
        //    Debug.LogError("FallBlocks");
        //    return;
        //}
        fallStep =-( block.Row_y - maxY);
        block.Row_y = maxY;// fallStep;
        float fallDis = (block.Row_y) * Config.blockHeight + Config.StartPosY;// - ((Config.initRows - block.Row_y - 1) * Config.blockHeight);// block.transform.localPosition.y - );//       fallStep * Config.blockHeight;
        block.transform.localPosition = new Vector3(block.transform.localPosition.x, block.transform.localPosition.y + fallStep * Config.blockHeight, block.transform.localPosition.z);
        //block.transform.DOLocalMoveY(fallDis, 0).OnComplete(() =>
        //{
            _controller._isPressureFallingDone = true;
        //});
        _controller._curMaxRowCnt = _controller._curRowCnt + _controller._PressureMatrixList.Count;
        _controller.CheckAlarm();
    }
    private int CheckDownHasPblok(PressureBlock pressureBlock)
    {
        int step = 0;
        foreach (var item in _controller._PressureMatrixList)
        {
            if (item.Row_y >= pressureBlock.Row_y)
                continue;
            int vale = pressureBlock.Row_y - item.Row_y - 1;
            if (step == 0 || step > vale)
                step = vale;
        }
        return step;
    }
    private int GetMaxX(PressureBlock pressureBlock)
    {
        int maxX = 0;
        bool isChekOk = true;
        foreach (var item in _controller._blockMatrix)
        {
            if (item != null)
                if (pressureBlock.Column_x <= item.Column && item.Column <= pressureBlock.Column_x + pressureBlock.xNum - 1)
                {
                    var currBlock = _controller._blockMatrix[item.Row + 1, item.Column];
                    if (currBlock == null || currBlock.Type == BlockType.None)
                    {
                        Block checkItem;
                        checkItem = _controller._blockMatrix[item.Row + 1, 0];
                        int i = 0;
                        int rowY = item.Row + 1;
                        do
                        {
                            checkItem = _controller._blockMatrix[rowY, i];
                            if (checkItem != null && checkItem.Row == pressureBlock.Row_y)
                            {
                                isChekOk = false;
                                maxX = 0;
                                break;
                            }
                            if (checkItem != null && checkItem.Type != BlockType.None)
                            {
                                i = 0;
                                rowY++;
                                checkItem = _controller._blockMatrix[rowY, 0];
                                isChekOk = true;
                                continue;
                            }
                            i++;
                        } while (i < pressureBlock.xNum);
                        Debug.LogError("over");
                        if (isChekOk && checkItem != null)
                        {
                            maxX = checkItem.Row;
                            pressureBlock.localPos = checkItem.transform.localPosition.y;
                            Debug.LogError("pressureBlock333____" + maxX + "+++++" + pressureBlock.Row_y);
                        }
                        break;
                    }
                }
        }
        //if (!isChekOk && maxX == 0)
        //{
        //    maxX = _controller._curRowCnt + _controller._PressureMatrixList.Count - 1;
        //}
        return maxX;
    }
    private int GetMaxYIndex(PressureBlock pressureBlock)
    {
        int index = 0;
        foreach (var item in _controller._blockMatrix)
        {
            if (item != null)
                if (pressureBlock.Column_x <= item.Column && item.Column <= pressureBlock.Column_x + pressureBlock.xNum - 1)
                {
                    if (index < item.Row && item.Type != BlockType.None)
                        index = item.Row;
                }
        }
        foreach (var item in _controller._PressureMatrixList)
        {
            if (item.GetHashCode() != pressureBlock.GetHashCode())
            {
                if (item.Row_y < pressureBlock.Row_y)
                    index = item.Row_y;
            }
        }
        if (index + 1 == pressureBlock.Row_y)
            return pressureBlock.Row_y;
        return index + 1;
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
                DoMovePressureBlock(_controller._PressureMatrixList[i]);
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
        _controller._PressureMatrixList.Sort((a, b) => a.Row_y - b.Row_y);
    }

    public override void Update()
    {
        base.Update();
        _time += Time.deltaTime;
        if (_controller._isFallingDone)
        {
            _controller.DestroyBlankRow();

        }
        //if (_controller._isPressureFallingDone)
        { 
            //_controller._isPressureFallingDone = false;
            
            //if (_controller._PressureMatrixList.Count > 0)
            //{
                //for (int i = 0; i < _controller._PressureMatrixList.Count; i++)
                 //   DoMovePressureBlock(_controller._PressureMatrixList[i]);
            //}
        }
        if (_controller._isFallingDone)// || _controller._isPressureFallingDone
        {
            bool hasMatchedGames = _controller.CalculateSwappedBlocks();
            if (hasMatchedGames)
                _controller.ChangeToState(GameBoardState.Blank);
            else
                _controller.ChangeToState(GameBoardState.Idle);

            //_controller._isFallingDone = false;
            //_controller._isPressureFallingDone = false;
        }
    }
    public override void Exit()
    {
        base.Exit();
        Debug.Log("Exit");
    }
}
