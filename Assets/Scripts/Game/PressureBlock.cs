using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PressureBlock : MonoBehaviour
{
    public int comboNum = 4;

    private Image _image;
    private float _moveDuration = 1.0f;
    private float _moveDis = 80.0f;

    public PressureBlockType Type;
    [SerializeField]
    private PressureBlockType _type = PressureBlockType.D1;
    public int Row_y;//y
    public int Column_x;//x
    public int xNum;

    [SerializeField]
    private int _state = (int)BlockState.None;
    public bool IsTagged // ±ê¼Ç
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

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        //Texture2D _textrue = Resources.Load(Config.textureComboPath + comboNum.ToString("D2")) as Texture2D;
        //_image.sprite = Sprite.Create(_textrue, new Rect(0, 0, _textrue.width, _textrue.height), new Vector2(1f, 1f));

        //var seq = DOTween.Sequence();
        //seq.Append(transform.DOLocalMoveY(transform.localPosition.y + _moveDis, _moveDuration));
        //seq.AppendCallback(() =>
        //{
        //    //this.gameObject.SetActive(false);
        //    GameObject.Destroy(gameObject);

        //});
    }
    public static PressureBlock CreateBlockObject(int row, int col, int type, Transform parent)
    {
        GameObject obj = Instantiate(Config._pressureBlockObj, parent) as GameObject;
        if (obj == null)
        {
            Debug.Assert(false, "Instantiate Block failed.");
            return null;
        }

        PressureBlock block = obj.GetComponent<PressureBlock>();
        block._image = obj.GetComponent<Image>();
        block.Type = (PressureBlockType)type;
        block.Row_y = row;
        block.Column_x = col;
        //block._image.sprite = Config._sprites[(int)block._type];

        return block;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
