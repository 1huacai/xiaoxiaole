using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

// swap state class
class SwapState : ControllerStateBase
{
    public SwapState(GameController controller) : base(controller)
    {
        // Do nothing
    }

    void SwapBlock()
    {
        var first = _controller._firstSelected;
        var second = _controller._secondSelected;
        // 多人模式需要同步交换操作
        if (_controller.IsMultiPlayer())
        {
            var req = new SprotoType.swap_block.request();
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
            NetSender.Send<Protocol.swap_block>(req, (data) =>
            {
                var resp = data as SprotoType.swap_block.response;
                Debug.LogFormat(" swap_block response : {0}", resp.e);
                if (resp.e == 0) { }
            });
        }

        var mySequence = DOTween.Sequence();
        float moveDuration = 0.18f;
        mySequence.Append(first.transform.DOLocalMove(second.transform.localPosition, moveDuration));
        mySequence.Join(second.transform.DOLocalMove(first.transform.localPosition, moveDuration));
        mySequence.AppendCallback(() =>
        {
            _controller._blockMatrix[first.Row, first.Column] = second;
            _controller._blockMatrix[second.Row, second.Column] = first;
            var row = first.Row;
            var column = first.Column;
            first.Row = second.Row;
            first.Column = second.Column;
            second.Row = row;
            second.Column = column;
            _controller._isSwappingDone = true;

            _controller.CheckAlarm();
        });
    }

    public override void Enter()
    {
        base.Enter();

        _controller._isSwappingDone = false;
        SwapBlock();
    }

    public override void Update()
    {
        base.Update();
        if (_controller._isSwappingDone)
        {
            _controller.ChangeToState(GameBoardState.Fall);
        }
    }
}
