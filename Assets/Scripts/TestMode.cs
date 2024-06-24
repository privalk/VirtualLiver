
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using OpenBLive.Runtime.Data;

public class TestMode : MonoBehaviour
{
    public GameObject input;
    public TMP_InputField tMP_InputField;
    public Button submit;

    void Start()
    {
        submit.onClick.AddListener(OnButtonClick);
        GlobalInputManager.SwitchInputWhenTmpInputSelected(tMP_InputField);
    }


    void Update()
    {
        if (GlobalInputManager.KeyDown(KeyCode.T))
        {
            if (input.activeInHierarchy)
            {
                input.SetActive(false);
            }
            else
            {
                input.SetActive(true);
                tMP_InputField.text = "";
            }
        }
        if(GlobalInputManager.KeyReturnDown()){
            OnButtonClick();
            tMP_InputField.text = "";
        }
    }

    public void OnButtonClick()
    {
        //模拟接收到弹幕
        Dm dm = new()
        {
            userName = "test",
            msg = tMP_InputField.text,
            userFace="https://bpic.588ku.com/element_pic/01/88/76/6057555a64bff31.jpg"
        };
        LinkToBili.Instance.m_WebSocketBLiveClient.OnDmSubmit(dm);
        tMP_InputField.text="";
    }

}
