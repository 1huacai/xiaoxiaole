using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Chain : MonoBehaviour
{
    public int chainNum = 2;

    private Image _image;
    private float _moveDuration = 1.0f;
    private float _moveDis = 80.0f;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        Texture2D _textrue = Resources.Load(Config.textureChainPath + chainNum.ToString("D2")) as Texture2D;
        _image.sprite = Sprite.Create(_textrue, new Rect(0, 0, _textrue.width, _textrue.height), new Vector2(1f, 1f));

        var seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMoveY(transform.localPosition.y + _moveDis, _moveDuration));
        seq.AppendCallback(() =>
        {
            //this.gameObject.SetActive(false);
            GameObject.Destroy(gameObject);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
