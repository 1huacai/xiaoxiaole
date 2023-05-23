using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PressureBlock : MonoBehaviour
{
    [SerializeField]
    public int Row_y;
    [SerializeField]
    public int xNum;
    [SerializeField]
    private int _state = (int)BlockState.None;

    private Image _image;
    private Animator _anim;

    public bool NeedFall = false;
    public int fallCnt = 0;

    public List<Block> _genBlocks = new List<Block>();
    public GameController _controller;


    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
    }

    //
    void FixedUpdate()
    {
        if (IsMoved)
        {
            IsMoved = false;
            _controller.PressureMoved(this);
        }
        if (IsTagged)
        {
            IsTagged = false;
            // PlayUnlockAnim();
            _controller.PressureTagged(this);
        }
        if (NeedFall)
        {
            float moveDuration = Mathf.Abs(fallCnt) * 0.08f;
            float yDis = transform.localPosition.y + fallCnt * (Config.blockHeight);
            Debug.Log(_controller._boardType + " -- before fall Pressure[" + Row_y + " - " + xNum + "] - fallCnt:" + fallCnt + " - y:" + transform.localPosition.y);
            transform.DOLocalMoveY(yDis, moveDuration).OnComplete(() =>
            {
                Row_y = Row_y + fallCnt;
                gameObject.name = "pressure + " + Row_y;
                Debug.Log(_controller._boardType + " -- after fall Pressure[" + Row_y + " - " + xNum + "] - fallCnt:" + fallCnt + " - y:" + transform.localPosition.y);
                fallCnt = 0;
                IsMoved = true;
            });
            NeedFall = false;
        }
    }

    public bool IsMoved // 移动
    {
        get { return (_state & 1 << (int)BlockState.Moved) != 0; }
        set
        {
            if (value)
            {
                _state |= 1 << (int)BlockState.Moved;
            }
            else
                _state &= ~(1 << (int)BlockState.Moved);
        }
    }

    public bool IsTagged // 标记
    {
        get { return (_state & 1 << (int)BlockState.Tagged) != 0; }
        set
        {
            if (value)
            {
                if (IsTagged)
                {
                    Debug.LogError(_controller._boardType + " -- pressure[" + Row_y + " - " + xNum + "] already tagged");
                    return;
                }
                _state |= 1 << (int)BlockState.Tagged;
                IsLocked = true;
            }
            else
                _state &= ~(1 << (int)BlockState.Tagged);
        }
    }

    public bool IsLocked // 锁定
    {
        get { return (_state & 1 << (int)BlockState.Locked) != 0; }
        set
        {
            if (value)
                _state |= 1 << (int)BlockState.Locked;
            else
                _state &= ~(1 << (int)BlockState.Locked);
        }
    }

    // 播放解锁动画
    public void PlayUnlockAnim()
    {
        _anim.runtimeAnimatorController = Config._animPressure[xNum - 3];
    }

    // 解锁动画播放完成后回调
    public void FinishUnlockAnim()
    {
        foreach (Block block in _genBlocks)
        {
            block.IsLocked = false;
            block.IsMoved = true; // 生成的方块开始下落
        }

        _controller._PressureMatrix.Remove(this);
        Debug.Log(_controller._boardType + " -- pressure[" + Row_y + " - " + xNum + "] destroyed");
        DoDestroy();
    }

    public static PressureBlock CreatePressureObject(int row, int xNum, Transform parent, GameController ctrl)
    {
        GameObject obj = Instantiate(Config._pressureBlockObj, parent) as GameObject;
        if (obj == null)
        {
            Debug.Assert(false, "Instantiate Block failed.");
            return null;
        }

        PressureBlock pressure = obj.GetComponent<PressureBlock>();
        pressure._image = obj.GetComponent<Image>();
        pressure._anim = obj.GetComponent<Animator>();
        pressure.Row_y = row;
        pressure.xNum = xNum;
        pressure._controller = ctrl;
        Debug.Log(ctrl._boardType + " -- new pressure[" + pressure.Row_y + " - " + pressure.xNum + "]");

        return pressure;
    }

    public void DoDestroy()
    {
        Destroy(gameObject);
    }
}
