using UnityEngine;
using UnityEngine.UI;

public delegate void OnCancel();
public delegate void OnSure();

public enum OpenMessageType
{
    Sure,
    SureandCancle
}

public class DialogInfo
{
    public string warnInfo;
    public string sureBtnInfo = "确定";
    public string cancleBtnInfo = "取消";
    public OnCancel onCancel;
    public OnSure onSure;
    public OpenMessageType openType;
}

public class MessageBox
{
    private GameObject messageBox = null;

    private Button m_sureBtn;
    private Button m_cancelBtn;
    private Text m_infoTxt;
    private Text sureBtnTxt;
    private Text cancleBtnTxt;

    private DialogInfo m_dialogInfo;

    public MessageBox(object val)
    {
        messageBox = GameObject.Instantiate(Config._msgBoxObj) as GameObject;
        messageBox.transform.SetParent(Config.mainObj.transform);
        messageBox.transform.localPosition = Vector3.zero;
        messageBox.transform.localScale = Vector3.one;

        m_infoTxt = messageBox.transform.Find("Info").GetComponent<Text>();
        m_sureBtn = messageBox.transform.Find("Sure").GetComponent<Button>();
        m_cancelBtn = messageBox.transform.Find("Cancel").GetComponent<Button>();
        sureBtnTxt = messageBox.transform.Find("Sure/Text").GetComponent<Text>();
        cancleBtnTxt = messageBox.transform.Find("Cancel/Text").GetComponent<Text>();

        m_dialogInfo = (DialogInfo)val;
        if (m_dialogInfo.openType == OpenMessageType.Sure)
        {
            m_cancelBtn.gameObject.SetActive(false);
        }
        if (!string.IsNullOrEmpty(m_dialogInfo.sureBtnInfo))
        {
            sureBtnTxt.text = m_dialogInfo.sureBtnInfo;
        }
        if (!string.IsNullOrEmpty(m_dialogInfo.cancleBtnInfo))
        {
            cancleBtnTxt.text = m_dialogInfo.cancleBtnInfo;
        }
        m_infoTxt.text = m_dialogInfo.warnInfo;

        m_sureBtn.onClick.AddListener(OnSureClick);
        m_cancelBtn.onClick.AddListener(OnCancelClick);
    }

    private void OnSureClick()
    {
        if (m_dialogInfo.onSure != null)
        {
            Util.PlayClickSound(m_sureBtn.gameObject);
            m_dialogInfo.onSure();
        }
        ClosePanel();
    }

    private void OnCancelClick()
    {
        if (m_dialogInfo.onCancel != null)
        {
            Util.PlayClickSound(m_cancelBtn.gameObject);
            m_dialogInfo.onCancel();
        }
        ClosePanel();
    }

    private void ClosePanel()
    {
        GameObject.Destroy(messageBox);
    }
}