using System;
using System.Collections.Generic;
using UnityEngine;

// idle state class
class IdleState : StateBase
{
    public IdleState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    public override void Enter()
    {
        base.Enter();
        if (_controller._firstSelected != null)
        {
            _controller._firstSelected.IsSelected = false;
            _controller._firstSelected = null;
        }
        if (_controller._secondSelected != null)
        {
            _controller._secondSelected = null;
        }
    }

    public override void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        base.OnBlockOperation(row, column, operation);
        if (operation == BlockOperation.TouchDown)
        {
            var selectedBlock = _controller._blockMatrix[row, column];
            if (selectedBlock.Type != BlockType.None
                && selectedBlock.moveCnt == 0
                && selectedBlock.fallCnt == 0
                && selectedBlock.IsLocked == false)
            {
                _controller._firstSelected = selectedBlock;
                _controller.ChangeToState(GameBoardState.Selection);
            }
        }
    }
}