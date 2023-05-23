using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgHandler : MonoBehaviour
{
    public Main _mainInst;
    public MainController _mainControllerInst;
    public RivalController _rivalControllerInst;

    private void Awake()
    {
        Config.msgHandler = this;
    }

    private void Start()
    {
        InitHandler();
    }

    private void Update()
    {
        NetCore.Dispatch();
    }

    private void InitHandler()
    {
        NetReceiver.AddHandler<Protocol.match_timeout>((data) =>
        {
            Debug.Log("========= match_timeout");
            _mainInst.MatchTimeout(data as SprotoType.match_timeout.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.match_success>((data) =>
        {
            Debug.Log("========= match_success");
            _mainInst.MatchSuccess(data as SprotoType.match_success.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.match_error>((data) =>
        {
            Debug.Log("========= match_wrong");
            _mainInst.MatchWrong(data as SprotoType.match_error.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_ready>((data) =>
        {
            Debug.Log("========= game_ready");
            _mainInst.GameReady(data as SprotoType.game_ready.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_start>((data) =>
        {
            Debug.Log("========= game_start");
            _mainControllerInst.GameStart(data as SprotoType.game_start.request);
            return null;
        });


        NetReceiver.AddHandler<Protocol.game_over_broadcast>((data) =>
        {
            Debug.Log("========= game_over");
            _mainControllerInst.GameOver(data as SprotoType.game_over_broadcast.request);
            return null;
        });


        //对手


        NetReceiver.AddHandler<Protocol.game_use_skill_broadcast>((data) =>
        {
            Debug.Log("========= game_use_skill_broadcast");
            _mainControllerInst.Usekill(data as SprotoType.game_use_skill_broadcast.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_swap_broadcast>((data) =>
        {
            Debug.Log("========= game_swap_broadcast");
            _rivalControllerInst.SyncSwapBlock(data as SprotoType.game_swap_broadcast.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_up_row_broadcast>((data) =>
        {
            Debug.Log("========= game_up_row_broadcast");
            _rivalControllerInst.SyncUpRow(data as SprotoType.game_up_row_broadcast.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_new_row_broadcast>((data) =>
        {
            Debug.Log("========= game_new_row_broadcast");
            _rivalControllerInst.SyncNewRow(data as SprotoType.game_new_row_broadcast.request);
            return null;
        });
        NetReceiver.AddHandler<Protocol.eliminate_broadcast>((data) =>
        {
            Debug.Log("========= eliminate_broadcast");
            _mainControllerInst.SyncNewpreBlock(data as SprotoType.eliminate_broadcast.request);
            return null;
        });
        NetReceiver.AddHandler<Protocol.createBlock_broadcast>((data) =>
        {
            Debug.Log("========= createBlock_broadcast");
            _rivalControllerInst.CreateBlock(data as SprotoType.createBlock_broadcast.request);
            return null;
        });
    }
}