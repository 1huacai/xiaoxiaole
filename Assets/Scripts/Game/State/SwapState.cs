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

    public override void OnBlockOperation(int row, int column, BlockOperation operation)
    {
        base.OnBlockOperation(row, column, operation);
        if (operation == BlockOperation.TouchUp)
        {
            _controller.ChangeToState(GameBoardState.Idle);
            return;
        }
    }

    void DoSwap(Block first, Block second)
    {
        Garbage garbage = _controller.GetGarbageInst();
        garbage.Reset();
        first._garbage = garbage;
        first.ComboTrans = 1;
        second._garbage = garbage;
        second.ComboTrans = 1;

        _controller.DoSwap(first, second);
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
                    DoSwap(first, second);
                }
                else
                {
                    Debug.LogError(_controller._boardType + " -- swap response err");
                    _controller.ChangeToState(GameBoardState.Idle);
                }
            });
        }
        else
        {
            // 己方
            DoSwap(first, second);

            // 对手
            var rival = MainManager.Ins._rivalController;
            Block rival_first = rival._controller._blockMatrix[first.Row, first.Column];
            Block rival_second = rival._controller._blockMatrix[second.Row, second.Column];
            if (rival_second == null)
            {
                Block block = Block.CreateBlockObject(second.Row, second.Column, (int)second.Type, rival._controller._blockBoardObj.transform, rival._controller);
                block.transform.localPosition = new Vector3(0, 0, -1);
                rival._controller._blockMatrix[second.Row, second.Column] = block;
                rival_second = block;
            }
            rival.DoSwap(rival_first, rival_second);
        }
    }
}
