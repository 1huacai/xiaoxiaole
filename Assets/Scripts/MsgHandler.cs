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

        NetReceiver.AddHandler<Protocol.match_wrong>((data) =>
        {
            Debug.Log("========= match_wrong");
            _mainInst.MatchWrong(data as SprotoType.match_wrong.request);
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

        NetReceiver.AddHandler<Protocol.game_over>((data) =>
        {
            Debug.Log("========= game_over");
            _mainControllerInst.GameOver(data as SprotoType.game_over.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.sync_swap_block>((data) =>
        {
            Debug.Log("========= sync_swap_block");
            _rivalControllerInst.SyncSwapBlock(data as SprotoType.sync_swap_block.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.sync_up_row>((data) =>
        {
            Debug.Log("========= sync_up_row");
            _rivalControllerInst.SyncUpRow(data as SprotoType.sync_up_row.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.sync_new_row>((data) =>
        {
            Debug.Log("========= sync_new_row");
            _rivalControllerInst.SyncNewRow(data as SprotoType.sync_new_row.request);
            return null;
        });

        NetReceiver.AddHandler<Protocol.sync_score>((data) =>
        {
            Debug.Log("========= sync_score");
            var req = data as SprotoType.sync_score.request;
            if (req.id == Main.rid)
                _rivalControllerInst.SyncScore(data as SprotoType.sync_score.request);
            else
                _mainControllerInst.SyncScore(data as SprotoType.sync_score.request);
            return null;
        });
    }
}