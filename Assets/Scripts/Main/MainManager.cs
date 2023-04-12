using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using SprotoType;

public class MainManager
{
    private static MainManager _ins;
    public static MainManager Ins
    {
        get {
            if (_ins == null)
                _ins = new MainManager();
            return _ins;
        } 
    }

    public List<SprotoType.player_info>  players;

    public string Uid;


    public MainController _mainController;

    public RivalController _rivalController;

    public int Timer;

    public bool DragBlock = true;

    public int DragTime;
}