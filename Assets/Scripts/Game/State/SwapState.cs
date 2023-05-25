using System;
using System.Collections.Generic;
using UnityEngine;

// swap state class
class SwapState : StateBase
{
    public SwapState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    public override void Enter()
    {
        base.Enter();
        SwapBlock();
    }

    void SwapBlock()
    {
        var first = _controller._firstSelected;
        var second = _controller._secondSelected;
        // 多人模式需要同步交换操作
        if (_controller.IsMultiPlayer())
        {
            var req = new SprotoType.game_swap.request();
            req.block1 = new SprotoType.block_info
            {
                row = first.Row,
                col = first.Column,
                type = (int)first.Type
            };
            req.block2 = new SprotoType.block_info
            {
                row = second.Row,
                col = second.Column,
                type = (int)second.Type
            };
            Debug.Log(_controller._boardType + "-- do swap first[" + first.Row + "," + first.Column + "] - second[" + second.Row + "," + second.Column + "]");
            NetSender.Send<Protocol.game_swap>(req, (data) =>
            {
                var resp = data as SprotoType.game_swap.response;
                Debug.LogFormat("{0} -- swap_block response : {1}", _controller._boardType, resp.e);
                if (resp.e == 0)
                {
                    Garbage garbage = _controller.GetGarbageInst();
                    garbage.Reset();
                    first._garbage = garbage;
                    first.ComboTrans = 1;
                    second._garbage = garbage;
                    second.ComboTrans = 1;

                    _controller.DoSwap(first, second);
                }
                else
                {
                    Debug.LogError(_controller._boardType + " -- swap response err");
                    _controller.ChangeToState(GameBoardState.Idle);
                }
            });
        }
    }
}
