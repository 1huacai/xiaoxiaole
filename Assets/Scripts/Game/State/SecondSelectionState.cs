using System;
using System.Collections.Generic;
using UnityEngine;

// second selection state class
class SecondSelectionState : ControllerStateBase
{
    public SecondSelectionState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    public override void Enter()
    {
        base.Enter();
        _controller._secondSelected.IsSelected = true;
        _controller.ChangeToState(GameBoardState.Swap);
    }
}