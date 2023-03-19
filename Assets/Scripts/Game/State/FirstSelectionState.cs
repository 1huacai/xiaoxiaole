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
}
