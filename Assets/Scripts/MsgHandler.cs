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

        NetReceiver.AddHandler<Protocol.match_start>((data) =>
        {
            Debug.Log("========= game_start");
            _mainControllerInst.GameStart(data as SprotoType.game_start.request);
            return null;
        });


        NetReceiver.AddHandler<Protocol.game_over>((data) =>
        {
            Debug.Log("========= game_over");
            _mainControllerInst.GameOver(data as SprotoType.game_over.request);
            return null;
        });


        //����


        NetReceiver.AddHandler<Protocol.game_use_skill_broadcast>((data) =>
        {
            Debug.Log("========= game_use_skill_broadcast");
            _rivalControllerInst.Usekill(data as SprotoType.game_use_skill_broadcast.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_swap_broadcast>((data) =>
        {
            Debug.Log("========= sync_swap_block");
            _rivalControllerInst.SyncSwapBlock(data as SprotoType.game_swap_broadcast.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_up_row_broadcast>((data) =>
        {
            Debug.Log("========= sync_up_row");
            _rivalControllerInst.SyncUpRow(data as SprotoType.game_up_row_broadcast.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.game_new_row_broadcast>((data) =>
        {
            Debug.Log("========= sync_new_row");
            _rivalControllerInst.SyncNewRow(data as SprotoType.game_new_row_broadcast.request);
            return null;
        });
        NetReceiver.AddHandler<Protocol.eliminate_broadcast>((data) =>
        {
            Debug.Log("========= SyncNewpreBlock");
            _rivalControllerInst.SyncNewpreBlock(data as SprotoType.eliminate_broadcast.request);
            return null;
        });

        //NetReceiver.AddHandler<Protocol.sync_score>((data) =>
        //{
        //    Debug.Log("========= sync_score");
        //    var req = data as SprotoType.sync_score.request;
        //    if (req.id == Main.rid)
        //        _rivalControllerInst.SyncScore(data as SprotoType.sync_score.request);
        //    else
        //        _mainControllerInst.SyncScore(data as SprotoType.sync_score.request);
        //    return null;
        //});
    }
}