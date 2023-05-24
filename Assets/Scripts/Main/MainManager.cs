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


    public static GameObject LoadGameObject(string path)
    {
        Object _obj = Resources.Load(path);
        if (_obj)
        {
            GameObject go = GameObject.Instantiate(_obj) as GameObject;
            return go;
        }
        return null;
    }
}
public enum HudType
{
    shanghai = 1,
    huixue = 2,
    dun = 3,
    jiadun = 4,
}
public enum RoleType
{
    min,
    emmey,
}
public class HudManager
{
    private static HudManager _ins;
    public static HudManager Ins
    {
        get
        {
            if (_ins == null)
                _ins = new HudManager();
            return _ins;
        }
    }

    static Queue<GameObject> hud_pool = new Queue<GameObject>();

    public Transform HudParent;

    public HudManager()
    {
        var go = new GameObject("Hud");
        go.transform.SetParent(GameObject.Find("UIRoot/Game").transform);
        HudParent = go.transform;

        if (hud_pool.Count == 0)
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject obj = MainManager.LoadGameObject("Prefabs/windows/Hud");
                obj.transform.SetParent(HudParent);
                Enqu(obj);
            }
        }
    }

    public void ShowHud(int value, HudType type, RoleType roleType = RoleType.min)
    {
        GameObject obj;
        if (hud_pool.Count < 1)
        {
            obj = MainManager.LoadGameObject("Prefabs/windows/Hud");
            obj.transform.SetParent(HudParent);
        }
        else
        {
            obj =  hud_pool.Dequeue();
        }
        if (obj != null)
        {
            Hud _hud = new Hud(obj);
            _hud.Show(value, type, roleType);
        }
        else
            Debug.LogError("Prefabs/windows/Hud  load fail");
    }

    public void Enqu(GameObject obj)
    {
        hud_pool.Enqueue(obj);
    }
}

public class Hud
{
    private Text valueTxt;
    private GameObject wind;
    public Hud(GameObject go)
    {
        wind = go;
        valueTxt = go.transform.Find("value").GetComponent<Text>();
    }

    public void Show(int value, HudType type, RoleType roleType)
    {        
        Vector3 pos = roleType == RoleType.min ? MainManager.Ins._mainController._minRole.transform.position : MainManager.Ins._mainController._emmyRole.transform.position;
        wind.transform.position = new Vector3(pos.x, pos.y + 360, pos.z);
        valueTxt.color = type == HudType.dun || type == HudType.jiadun ? Color.white : type == HudType.huixue ? Color.green : Color.red;
        valueTxt.text = type == HudType.huixue || type == HudType.jiadun ? "+" + value : "-" + value;
        wind.SetActive(true);
        ShowAnima();
    }

    private void ShowAnima()
    {
        wind.transform.DOLocalMoveY(wind.transform.localPosition.y + 80, 2f).SetEase( Ease.OutExpo);
        valueTxt.DOFade(0, 2f).OnComplete(()=>
        {
            wind.SetActive(false);
            valueTxt.color = new Color(1, 1, 1, 1);
            HudManager.Ins.Enqu(wind);
        });
    }
}