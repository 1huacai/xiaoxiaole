using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseUp()
    {
        Debug.Log("result down");
        this.gameObject.SetActive(false);

        Config.gameObj.SetActive(false);
        Config.mainObj.SetActive(true);

        Main._reset = true;
    }
}
