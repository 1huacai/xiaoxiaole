//using System;
//using System.Collections.Generic;
//using UnityEngine;

//// reverse swap state class
//class ReverseSwapState : ControllerStateBase
//{
//    public ReverseSwapState(GameController controller) : base(controller)
//    {
//        // Do nothing
//    }

//    public override void Enter()
//    {
//        base.Enter();
//        _controller.SwapTwoBlocks();
//        _controller._firstSelected.SetIsSelected(false);
//        _controller._secondSelected.SetIsSelected(false);
//    }

//    public override void Update()
//    {
//        base.Update();
//        if (_controller._isSwappingDone)
//        {
//            _controller.ChangeToState(GameBoardState.Idle);
//        }
//    }
//}
