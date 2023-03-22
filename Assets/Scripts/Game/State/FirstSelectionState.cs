using System;
using System.Collections.Generic;
using UnityEngine;

// first selection state class
class FirstSelectionState : ControllerStateBase
{
    public FirstSelectionState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    public override void Enter()
    {
        base.Enter();
        _controller._chainCnt = 0;
        _controller._firstSelected.IsSelected = true;
    }

    public override void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        base.OnBlockOperation(row, column, operation);
        if (operation == BlockOperation.TouchDown || operation == BlockOperation.DragHalf)
        {
            // check if the two selected block is adjacent
            if(row >= 0 && row< _controller._curRowCnt && column>=0 && column< Config.columns)
            {
                var selectedBlock = _controller._blockMatrix[row, column];
                bool isAdjacent = _controller._firstSelected.IsAdjacent(selectedBlock);
                if (isAdjacent)
                {
                    _controller._secondSelected = selectedBlock;
                    _controller.ChangeToState(GameBoardState.SecondSelection);
                }
                else
                {
                    if (selectedBlock != null && selectedBlock.Type != BlockType.None)
                    {
                        Debug.LogError("selected" + row + "+" + column);
                        _controller._firstSelected.IsSelected = false;
                        _controller._firstSelected = selectedBlock;
                        _controller._firstSelected.IsSelected = true;
                    }
                }
            }
        }
        else
        {
            _controller.ChangeToState(GameBoardState.Idle);
        }
    }
    private bool CheckHasPblock(int row_y,int col_x)
    {
        var block = _controller._blockMatrix[row_y, col_x];
        foreach (var item in _controller._PressureMatrixList)
        {
            if (block == null || block.Type !=  BlockType.None)
            {
                if (item.Column_x <= col_x - 1 && col_x >= item.Column_x + item.xNum)
                    return false;
                if (item.Column_x <= col_x + 1 && col_x >= item.Column_x + item.xNum - 1)
                    return false;
                if (item.Row_y <= row_y + 1 && item.Column_x <= col_x && col_x >= item.Column_x + item.xNum - 1)
                    return false;
            }
        }
        return true;
    }
}