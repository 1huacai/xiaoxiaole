using System;
using System.Collections.Generic;
using UnityEngine;

// blank state classes
class BlankState : ControllerStateBase
{
    int _blankCnt = 0; // 一次消除方块数
    List<Block> _wait2Blank = new List<Block> { };
    bool _blankDone = false;

    int _addScore = 0; // 本次消除得分

    public BlankState(GameController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _controller._suspendRaise = true;
        if (_controller._firstSelected)
            _controller._firstSelected.IsSelected = false;
        if (_controller._secondSelected)
            _controller._secondSelected.IsSelected = false;

        _blankCnt = 0;
        _addScore = 0;
        _wait2Blank.Clear();

        List<Block> readyCombo = new List<Block>();
        Dictionary<BlockType, int> comboCnt = new Dictionary<BlockType, int>();
        for (int row = _controller._curRowCnt - 1; row > 0; row--)
        {
            for (int col = 0; col < Config.columns; col++)
            {
                var current = _controller._blockMatrix[row, col];
                if (current != null && current.IsTagged)
                {
                    current.IsTagged = false;
                    current.PlayBlankAnim();
                    _wait2Blank.Add(current);
                    _blankCnt += 1;
                    if (comboCnt.ContainsKey(current.Type))
                    {
                        comboCnt[current.Type] += 1;
                    }
                    else
                    {
                        comboCnt.Add(current.Type, 1);
                        readyCombo.Add(current);
                    }
                }
            }
        }

        var showCombo = true;
        foreach (var block in readyCombo)
        {
            // var cnt = comboCnt[block.Type];
            if (_blankCnt > 3 && showCombo) // 大消
            {
                GameObject obj = GameObject.Instantiate(Config._comboObj, block.transform) as GameObject;
                obj.transform.localPosition = new Vector3(0, 0, -1);
                Combo combo = obj.gameObject.GetComponent<Combo>();
                combo.comboNum = _blankCnt;
                _addScore += Config.scoreCombo[_blankCnt - 1];
                showCombo = false;
            }
            _controller._chainCnt++;
            if (_controller._chainCnt > 1) // 连消
            {
                GameObject obj = GameObject.Instantiate(Config._chainObj, block.transform) as GameObject;
                obj.transform.localPosition = new Vector3(0, 0, -1);
                Chain chain = obj.gameObject.GetComponent<Chain>();
                chain.chainNum = _controller._chainCnt;
            }
        }
        if (_blankCnt > 3)
        {
            

            //_controller.GreatPressureBlock(_blankCnt);
            //技能释放控制器逻辑添加

            if (_controller is MainController)
            {
                MainManager.Ins._rivalController.GreatPressureBlock(_blankCnt);
                //var main = _controller as MainController;
                //main.PlayAnima(main._minroleData.specialAtkAnimaName);
            }
        }
        else
        {
            //if (_controller is MainController)
            //{
            //    var main = _controller as MainController;
            //    main.PlayAnima(main._minroleData.atkAnimaName);
            //}
        }
        if (_controller is MainController)
        {
            SendNet(_blankCnt);

            MainManager.Ins._mainController._minroleData.UpdateSkill2(_blankCnt);
        }
        _controller.DestroyPBlockRow();
    }
    private void SendNet(int _count)
    {
        var req = new SprotoType.eliminate.request();
        req.count = _count;
        NetSender.Send<Protocol.eliminate>(req, (data) =>
        {
            var resp = data as SprotoType.eliminate.response;
            Debug.LogFormat("eliminate : {0}", resp.e);
            if (resp.e == 0) { }
        });
    }

    public override void Update()
    {
        base.Update();

        foreach (var block in _wait2Blank)
        {
            if (block.IsBlanked == false)
                return;
            _blankDone = true;
        }
        if (_blankDone && _blankCnt > 0)
        {
            _blankDone = false;
            if (_blankCnt > 0)
                _addScore = _addScore + 10 + Config.scoreChain[_controller._chainCnt -1];
            _controller.ChangeScore(_addScore, _blankCnt);
            _controller.ChangeToState(GameBoardState.Fall);
        }
    }

    public override void Exit()
    {
        _controller._suspendRaise = false;
        base.Exit();
    }
}