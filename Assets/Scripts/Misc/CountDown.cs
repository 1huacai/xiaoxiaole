using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;


public class CountDown : MonoBehaviour
{
    public GameObject TextObj;
    public int TotalTime = 180;

    private int _run_time = 0;
    public bool _countDown = true;

    void Start()
    {
        StartCoroutine(Time());
    }

    IEnumerator Time()
    {
        while (true)
        {
            if (TextObj.activeSelf)
            {
                if (_countDown)
                {
                    TextObj.gameObject.GetComponent<Text>().text = string.Format("{0:D2}:{1:D2}", (int)TotalTime / 60, (int)TotalTime % 60);
                    TotalTime--;
                }
                else
                {
                    TextObj.gameObject.GetComponent<Text>().text = string.Format("{0:D2}:{1:D2}", (int)_run_time / 60, (int)_run_time % 60);
                    _run_time++;
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    void update()
    {

    }
}