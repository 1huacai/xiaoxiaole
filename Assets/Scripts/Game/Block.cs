using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class Block : MonoBehaviour
{
    [SerializeField]
    private int _row = 0;
    [SerializeField]
    private int _column = 0;
    [SerializeField]
    private BlockType _type = BlockType.Count;
    [SerializeField]
    private int _state = (int)BlockState.None;

    private Image _image;
    private Animator _anim;
    private Vector3 _dragBeginPos;
    private GameObject _selectImage;

    public bool NeedFall = false;
    // 下落距离
    public int fallCnt = 0;

    public bool NeedMove = false;
    // 水平移动距离（包括左移和右移）
    public int moveCnt = 0;
    public int MoveStay = 0;

    public bool IsIniting = false;
    public GameController _controller;
    public Garbage _garbage;
    public int ComboTrans = 0;

    public delegate void BlockOperationHandler(int row, int column, BlockOperation operation);
    public event BlockOperationHandler BlockOperationEvent;


    void Awake() { }

    void Start() { }

    void OnDestroy() { }

    private void OnMouseEnter()
    {
        if (IsTagged == false && BlockOperationEvent != null)
        {
            // BlockOperationEvent(_row, _column, BlockOperation.TouchEnter);
        }
    }

    private void OnMouseExit()
    {
        if (IsTagged == false && BlockOperationEvent != null)
        {
            // BlockOperationEvent(_row, _column, BlockOperation.TouchExit);
        }
    }

    private void OnMouseDown()
    {
        if (IsTagged == false && BlockOperationEvent != null)
        {
            BlockOperationEvent(_row, _column, BlockOperation.TouchDown);
            _dragBeginPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (IsTagged == false && BlockOperationEvent != null)
        {
            BlockOperationEvent(_row, _column, BlockOperation.TouchUp);
        }
    }

    private void OnMouseDrag()
    {
        if (IsTagged == false && BlockOperationEvent != null)
        {
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float xOffset = Mathf.Abs(curPosition.x - _dragBeginPos.x);
            float yOffset = Mathf.Abs(curPosition.y - _dragBeginPos.y);
            if (xOffset > Config.blockWidth / 5)
            {
                if (xOffset > Config.blockWidth)
                    return;
                if (curPosition.x > _dragBeginPos.x && _column < Config.columns - 1)
                    BlockOperationEvent(_row, _column + 1, BlockOperation.DragHalf);
                if (curPosition.x < _dragBeginPos.x && _column > 0)
                    BlockOperationEvent(_row, _column - 1, BlockOperation.DragHalf);
            }
            else if (yOffset > Config.blockHeight / 5)
            {
                if (yOffset > Config.blockHeight)
                    return;
                if (curPosition.y > _dragBeginPos.y && _row < Config.rows - 1)
                    BlockOperationEvent(_row + 1, _column, BlockOperation.DragHalf);
                if (curPosition.y < _dragBeginPos.y && _row > 1)
                    BlockOperationEvent(_row - 1, _column, BlockOperation.DragHalf);
            }
        }
    }

    public void ResetDragPos()
    {
        _dragBeginPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void LogicUpdate()
    {
        if (IsMoved)
        {
            IsMoved = false;
            _controller.BlockMoved(this);
        }
        if (IsTagged)
        {
            IsTagged = false;
            PlayBlankAnim();
            _controller.BlockTagged(this);
        }
        if (NeedFall)
        {
            NeedFall = false;
            if (IsIniting)
            {
                IsIniting = false;
                float duration = Mathf.Abs(fallCnt) * Config.fallDuration;
                float yDis = transform.localPosition.y + fallCnt * (Config.blockHeight);
                transform.DOLocalMoveY(yDis, duration).OnComplete(() =>
                {
                    fallCnt = 0;
                    ShowBlockMoveAnima((int)Type);
                });
            }
            else
            {
                float duration = Mathf.Abs(fallCnt) * Config.fallDuration;
                float yDis = transform.localPosition.y + fallCnt * (Config.blockHeight);
                Debug.Log(_controller._boardType + " -- before fall block[" + Row + "," + Column + " - " + Type + "] - fallCnt:" + fallCnt + " - y:" + transform.localPosition.y);
                transform.DOLocalMoveY(yDis, duration).OnComplete(() =>
                {
                    if (_controller._blockMatrix[Row, Column] == this)
                        _controller._blockMatrix[Row, Column] = null;
                    Row = Row + fallCnt;
                    gameObject.name = Row + " + " + Column;
                    _controller._blockMatrix[Row, Column] = this;
                    Debug.Log(_controller._boardType + " -- after fall block[" + Row + "," + Column + " - " + Type + "] - fallCnt:" + fallCnt + " - y:" + transform.localPosition.y);
                    fallCnt = 0;
                    IsMoved = true;
                    if (_garbage != null && this != _controller._firstSelected && this != _controller._secondSelected)
                        ComboTrans += 1;
                    MoveStay = 3;
                    if (this != _controller._firstSelected && this != _controller._secondSelected)
                        ShowBlockMoveAnima((int)Type);
                });
            }
        }
        if (NeedMove)
        {
            NeedMove = false;
            float xDis = transform.localPosition.x + moveCnt * Config.blockWidth;
            Debug.Log(_controller._boardType + " -- before move block[" + Row + "," + Column + " - " + Type + "] - moveCnt:" + moveCnt + " - x:" + transform.localPosition.x);
            transform.DOLocalMoveX(xDis, Config.moveDuration).OnComplete(() =>
            {
                Column = Column + moveCnt;
                gameObject.name = Row + " + " + Column;
                _controller._blockMatrix[Row, Column] = this;
                Debug.Log(_controller._boardType + " -- after move block[" + Row + "," + Column + " - " + Type + "] - moveCnt:" + moveCnt + " - x:" + transform.localPosition.x);
                moveCnt = 0;
                IsMoved = true;
                MoveStay = 3;
                //ShowBlockMoveAnima((int)Type);
            });
        }
        if (MoveStay > 0)
            MoveStay--;
    }

    public enum BlockEffectType
    {
        red = 1,
        green = 2,
        blue = 3,
        yellow = 4,
        purple = 5,
    }
    private void ShowBlockMoveAnima(int _type)
    {
        _image.enabled = false;

        BlockEffectType type = (BlockEffectType)_type;

        string path = string.Format("spineArt/block/{0}/{1}_SkeletonData", type.ToString(), type.ToString());

        var effectData = Resources.Load<SkeletonDataAsset>(path);
        Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
        SkeletonGraphic effect = SkeletonGraphic.NewSkeletonGraphicGameObject(effectData, this.transform, minmaterial);
        effect.transform.localPosition = new Vector3(effect.transform.localPosition.x, effect.transform.localPosition.y - 70, effect.transform.localPosition.z);

        effect.skeletonDataAsset = effectData;
        effect.initialSkinName = "default";
        effect.startingAnimation = "animation";
        effect.startingLoop = false;
        effect.MatchRectTransformWithBounds();
        effect.material = minmaterial;
        effect.Initialize(true);

        effect.AnimationState.Complete += (a) =>
        {
            Destroy(effect.gameObject);
            _image.enabled = true;
        };
    }
    private void ShowBlockZhikongAnima()
    {
        string path = "spineArt/block/boom/qizi_SkeletonData";

        var effectData = Resources.Load<SkeletonDataAsset>(path);
        Material minmaterial = new Material(Shader.Find("Spine/SkeletonGraphic"));
        SkeletonGraphic effect = SkeletonGraphic.NewSkeletonGraphicGameObject(effectData, this.transform, minmaterial);
        //effect.transform.localPosition = new Vector3(effect.transform.localPosition.x, effect.transform.localPosition.y - 70, effect.transform.localPosition.z);
        effect.transform.localScale = new Vector3(3, 3, 3);

        effect.skeletonDataAsset = effectData;
        effect.initialSkinName = "default";
        effect.startingAnimation = "animation";
        effect.startingLoop = false;
        effect.MatchRectTransformWithBounds();
        effect.material = minmaterial;
        effect.Initialize(true);

        effect.AnimationState.Complete += (a) =>
        {
            Destroy(effect.gameObject);
            FinishBlankAnim();
        };
    }

    public int Row
    {
        get { return _row; }
        set
        {
            _row = value;
            if (_row == 0)
            {
                IsLocked = true;
                _image.sprite = Config._lockSprites[(int)_type];
            }
            else
            {
                _image.sprite = Config._sprites[(int)_type];
            }
        }
    }

    public int Column
    {
        get { return _column; }
        set { _column = value; }
    }

    public BlockType Type
    {
        get { return _type; }
        set { _type = value; }
    }

    public bool IsSelected // 选中
    {
        get { return (_state & 1 << (int)BlockState.Selected) != 0; }
        set
        {
            if (value)
            {
                _state |= 1 << (int)BlockState.Selected;
                _selectImage.SetActive(true);
            }
            else
            {
                _state &= ~(1 << (int)BlockState.Selected);
                _selectImage.SetActive(false);
            }
        }
    }

    public bool IsMoved // 移动
    {
        get { return (_state & 1 << (int)BlockState.Moved) != 0; }
        set
        {
            if (value)
            {
                Debug.Log(_controller._boardType + " -- block[" + Row + "," + Column + " - " + Type + "] Moved");
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
                    Debug.LogError(_controller._boardType + " -- block[" + Row + "," + Column + " - " + Type + "] already tagged");
                    return;
                }
                Debug.Log(_controller._boardType + " -- block[" + Row + "," + Column + " - " + Type + "] Tagged");
                _state |= 1 << (int)BlockState.Tagged;
                IsLocked = true;
                _controller._suspendRaise++;
                if (_controller._firstSelected == this)
                    _controller.ChangeToState(GameBoardState.Idle);
            }
            else
                _state &= ~(1 << (int)BlockState.Tagged);
        }
    }

    public bool IsBlanked // 置空
    {
        get { return (_state & 1 << (int)BlockState.Blanked) != 0; }
        set
        {
            if (value)
            {
                Debug.Log(_controller._boardType + " -- block[" + Row + "," + Column + " - " + Type + "] destroyed");
                _state |= 1 << (int)BlockState.Blanked;
                DoDestroy();
            }
            else
                _state &= ~(1 << (int)BlockState.Blanked);
        }
    }

    public bool IsTrembled // 颤抖
    {
        get { return (_state & 1 << (int)BlockState.Trembled) != 0; }
        set
        {
            if (value)
                _state |= 1 << (int)BlockState.Trembled;
            else
                _state &= ~(1 << (int)BlockState.Trembled);
        }
    }

    public bool IsLocked // 锁定
    {
        get { return (_state & 1 << (int)BlockState.Locked) != 0; }
        set
        {
            if (value)
            {
                _state |= 1 << (int)BlockState.Locked;
                _image.sprite = Config._lockSprites[(int)_type];
            }
            else
            {
                _state &= ~(1 << (int)BlockState.Locked);
                _image.sprite = Config._sprites[(int)_type];
            }
        }
    }

    // 判断两个方块是否相邻
    public bool CheckAdjacent(Block other)
    {
        if (IsLocked || other.IsLocked)
            return false;
        if ((Row == other.Row && System.Math.Abs(Column - other.Column) == 1)
            || (Column == other.Column && System.Math.Abs(Row - other.Row) == 1))
            return true;
        return false;
    }

    // 播放置空动画
    public void PlayBlankAnim()
    {
        if (_row == 0)
            return;
        IsBlanked = false;
        //_anim.runtimeAnimatorController = Config._animDestroy[(int)_type];
        ShowBlockZhikongAnima();
    }

    // 置空动画播放完成后回调
    public void FinishBlankAnim()
    {
        _type = BlockType.None;
        _image.sprite = Config._sprites[(int)_type];
        _controller._suspendRaise--;
        _controller.BlockMoved(this);
    }

    // 警报颤抖状态切换
    public void TrembleChange(bool danger)
    {
        if (IsLocked)
            return;

        if (danger)
        {
            //_anim.runtimeAnimatorController = Config._animTremble[(int)_type];
            IsTrembled = true;
        }
        else
        {
            //_anim.runtimeAnimatorController = null;
            _image.sprite = Config._sprites[(int)_type];
            IsTrembled = false;
        }
    }

    public static Block CreateBlockObject(int row, int col, int type, Transform parent, GameController ctrl)
    {
        GameObject obj = Instantiate(Config._blockObj, parent) as GameObject;
        if (obj == null)
        {
            Debug.Assert(false, "Instantiate Block failed.");
            return null;
        }

        Block block = obj.GetComponent<Block>();
        block._image = obj.GetComponent<Image>();
        block._anim = obj.GetComponent<Animator>();
        block._selectImage = obj.transform.Find("Select").gameObject;
        block.Type = (BlockType)type;
        block._image.sprite = Config._sprites[(int)block._type];
        block.Row = row;
        block.Column = col;
        block.gameObject.name = row + " + " + col;
        block._controller = ctrl;
        return block;
    }

    public void DoDestroy()
    {
        _controller._blockMatrix[Row, Column] = null;
        Destroy(gameObject);
    }
}