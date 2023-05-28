using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class PressureBlock : MonoBehaviour
{
    [SerializeField]
    private int _row;
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


    void Start()
    {
        _image = GetComponent<Image>();
    }

    public void LogicUpdate()
    {
        if (IsMoved)
        {
            IsMoved = false;
            _controller.PressureMoved(this);
        }
        if (IsTagged)
        {
            IsTagged = false;
            _controller.PressureTagged(this);
        }
        if (NeedFall)
        {
            float duration = Mathf.Abs(fallCnt) * Config.fallDuration;
            float yDis = transform.localPosition.y + fallCnt * (Config.blockHeight);
            Debug.Log(_controller._boardType + " -- before fall Pressure[" + Row + " - " + xNum + "] - fallCnt:" + fallCnt + " - y:" + transform.localPosition.y);
            transform.DOLocalMoveY(yDis, duration).OnComplete(() =>
            {
                Row = Row + fallCnt;
                gameObject.name = "pressure + " + Row;
                Debug.Log(_controller._boardType + " -- after fall Pressure[" + Row + " - " + xNum + "] - fallCnt:" + fallCnt + " - y:" + transform.localPosition.y);
                fallCnt = 0;
                IsMoved = true;
            });
            NeedFall = false;
        }
    }

    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
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
                    Debug.LogError(_controller._boardType + " -- pressure[" + Row + " - " + xNum + "] already tagged");
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
        // _anim.runtimeAnimatorController = Config._animPressure[xNum - 3];
        ShowBlankAnima(1);
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
        Debug.Log(_controller._boardType + " -- pressure[" + Row + " - " + xNum + "] destroyed");
        DoDestroy();
    }

    private void ShowBlankAnima(int shift)
    {
        string path = "spineArt/pressure/boom/yalikuai_SkeletonData";

        var effectData = Resources.Load<SkeletonDataAsset>(path);
        Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
        SkeletonGraphic effect = SkeletonGraphic.NewSkeletonGraphicGameObject(effectData, this.transform, minmaterial);
        effect.transform.localPosition = new Vector3(55, effect.transform.localPosition.y, effect.transform.localPosition.z);
        effect.transform.localScale = new Vector3(3.8f, 3.5f, 3.5f);

        effect.skeletonDataAsset = effectData;
        effect.initialSkinName = "default";
        effect.startingAnimation = "animation";
        effect.startingLoop = false;
        effect.MatchRectTransformWithBounds();
        effect.material = minmaterial;
        effect.Initialize(true);

        effect.AnimationState.Complete += (a) =>
        {
            if (shift < xNum)
            {
                float posx = transform.localPosition.x + Config.blockWidth;
                transform.localPosition = new Vector3(posx, transform.localPosition.y, 0);
                RectTransform rect = GetComponent<RectTransform>();
                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (xNum - shift) * Config.blockWidth);
                ShowBlankAnima(++shift);
            }
            else
            {
                Destroy(effect.gameObject);
                FinishUnlockAnim();
            }
        };
    }

    public static PressureBlock CreatePressureObject(int row, int xNum, Transform parent, GameController controller)
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
        pressure.Row = row;
        pressure.xNum = xNum;
        pressure._controller = controller;
        Debug.Log(controller._boardType + " -- new pressure[" + pressure.Row + " - " + pressure.xNum + "]");
        return pressure;
    }

    public void DoDestroy()
    {
        Destroy(gameObject);
    }
}
