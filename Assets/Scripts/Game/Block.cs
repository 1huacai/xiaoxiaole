using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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

    private Vector3 _dragOffset;
    private Vector3 _dragBeginPos;

    public delegate void BlockOperationHandler(int row, int column, BlockOperation operation);
    public event BlockOperationHandler BlockOperationEvent;


    void Awake() { }

    // Use this for initialization
    void Start() {
        heightOffset = Config.blockHeight / 3;
    }

    void OnDestroy() { }

    private void OnMouseEnter1()
    {
        if (BlockOperationEvent != null)
        {
            BlockOperationEvent(_row, _column, BlockOperation.TouchEnter);
        }
    }

    private void OnMouseExit2()
    {
        if (BlockOperationEvent != null)
        {
            BlockOperationEvent(_row, _column, BlockOperation.TouchExit);
        }
    }

    private void OnMouseDown()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        swapDirection = Vector3.zero;
        drag = true;

        if (BlockOperationEvent != null)
        {
            BlockOperationEvent(_row, _column, BlockOperation.TouchDown);
        }
    }

    private void OnMouseUp()
    {
        drag = false;
        swapDirection = Vector3.zero;
        if (BlockOperationEvent != null)
        {
            BlockOperationEvent(_row, _column, BlockOperation.TouchUp);
        }
    }

    private float heightOffset;
    private Vector3 mousePosition;
    private bool drag;
    public Vector3 swapDirection = Vector3.zero;

    // Update is called once per frame
    void Update() {
        if (drag)
        {
            _dragOffset = mousePosition - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            moveEvent(_dragOffset);
        }
    }
    private void moveEvent(Vector3 _Offset)
    {
        if (BlockOperationEvent != null)
        {
            if (Vector3.Magnitude(_Offset) > 0.1f)
            {
                if (Mathf.Abs(_Offset.x) > Mathf.Abs(_Offset.y) && _Offset.x > 0)
                    swapDirection.x = 1;
                else if (Mathf.Abs(_Offset.x) > Mathf.Abs(_Offset.y) && _Offset.x < 0)
                    swapDirection.x = -1;
                else if (Mathf.Abs(_Offset.x) < Mathf.Abs(_Offset.y) && _Offset.y > 0)
                    swapDirection.y = 1;
                else if (Mathf.Abs(_Offset.x) < Mathf.Abs(_Offset.y) && _Offset.y < 0)
                    swapDirection.y = -1;


                if (swapDirection.x > 0)
                {
                    BlockOperationEvent(_row, _column - 1, BlockOperation.DragHalf);
                }
                else if (swapDirection.x < 0)
                {
                    BlockOperationEvent(_row, _column + 1, BlockOperation.DragHalf);
                }
                else if (swapDirection.y > 0)
                {
                    BlockOperationEvent(_row - 1, _column, BlockOperation.DragHalf);
                }
                else if (swapDirection.y < 0)
                {
                    BlockOperationEvent(_row + 1, _column, BlockOperation.DragHalf);
                }
            }
        }
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
                IsLocked = false;
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
                _state |= 1 << (int)BlockState.Selected;
            else
                _state &= ~(1 << (int)BlockState.Selected);
        }
    }

    public bool IsTagged // 标记
    {
        get { return (_state & 1 << (int)BlockState.Tagged) != 0; }
        set
        {
            if (value)
                _state |= 1 << (int)BlockState.Tagged;
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
                _state |= 1 << (int)BlockState.Blanked;
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
                _state |= 1 << (int)BlockState.Locked;
            else
                _state &= ~(1 << (int)BlockState.Locked);
        }
    }

    // 判断两个方块是否相邻
    public bool IsAdjacent(Block other)
    {
        return other == null ? false : IsLocked || other.IsLocked ? false :
            Row != other.Row ? 
            true : System.Math.Abs(Column - other.Column) > 1 ?
            false : true;
    }

    // 判断两个方块类型是否相同
    public bool IsMatched(Block other)
    {
        bool ret = false;
        do
        {
            if (IsLocked || other.IsLocked)
                break;
            if (Type == BlockType.None || Type != other.Type)
                break;
            ret = true;
        } while (false);
        return ret;
    }

    // 播放置空动画
    public void PlayBlankAnim()
    {
        if (_row == 0)
            return;

        _anim.runtimeAnimatorController = Config._animDestroy[(int)_type];
    }

    // 置空动画播放完成后回调
    public void FinishBlankAnim()
    {
        _type = BlockType.None;
        _image.sprite = Config._sprites[(int)_type];
        IsBlanked = true;
    }

    // 警报颤抖状态切换
    public void TrembleChange(bool danger)
    {
        if (IsLocked)
            return;

        if (danger)
        {
            _anim.runtimeAnimatorController = Config._animTremble[(int)_type];
            IsTrembled = true;
        }
        else
        {
            _anim.runtimeAnimatorController = null;
            _image.sprite = Config._sprites[(int)_type];
            IsTrembled = false;
        }
    }

    public static Block CreateBlockObject(int row, int col, int type, Transform parent)
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
        block.Type = (BlockType)type;
        block.Row = row;
        block.Column = col;
        block._image.sprite = Config._sprites[(int)block._type];

        return block;
    }

    public void DoDestroy()
    {
        Destroy(gameObject);
    }
}