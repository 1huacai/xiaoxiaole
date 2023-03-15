using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    void Awake()
    {
        //������ʾ
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = true;

        //�����ʼ��
        NetCore.Init();
        NetSender.Init();
        NetReceiver.Init();
        NetCore.enabled = true;

        //�����ʼ��
        Config.rootObj = this.gameObject;
        Config.mainObj = this.transform.Find("Main").gameObject;
        Config.gameObj = this.transform.Find("Game").gameObject;
        Config.mainObj.SetActive(true);
        Config.gameObj.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
