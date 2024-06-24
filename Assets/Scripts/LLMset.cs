using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
public class LLMset : MonoBehaviour
{
    public TMP_Dropdown tMP_Dropdown;
    public List<GameObject> UIs = new();
    public TMP_InputField tMP_InputField0_API;
    public TMP_InputField tMP_InputField0_retrievalID;
    public TMP_InputField tMP_InputField0_prompt;
    public TMP_InputField tMP_InputField1_API;
    public TMP_InputField tMP_InputField1_botName;
    public TMP_InputField tMP_InputField1_prompt;

    public Button save;
    public Button close;
    public GameObject SettingUI;
    public static event Action OnGameSettingChanged;

    void Start()
    {
        OnInit();
        tMP_Dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        save.onClick.AddListener(OnClickToSave);
        close.onClick.AddListener(OnClickToClose);
    }

    private void OnDropdownValueChanged(int value)
    {
        SetUIActive(tMP_Dropdown.options[value].text);
        OnGameSettingChanged=null;
        switch (value)
        {
            case 0: ChatSample.Instance.m_ChatModel = LLMs.Instance.chatGLM4Retrieval; OnGameSettingChanged+=LLMs.Instance.chatGLM4Retrieval.SaveSettings;break;
            case 1: ChatSample.Instance.m_ChatModel = LLMs.Instance.charGLM;OnGameSettingChanged+=LLMs.Instance.charGLM.SaveSettings; break;
            case 2: ChatSample.Instance.m_ChatModel = LLMs.Instance.chatchat; break;
            default: break;
        }
        OnInit();
    }
    public void OnInit()
    {
        tMP_InputField0_API.text = LLMs.Instance.chatGLM4Retrieval.GetKey();
        tMP_InputField0_retrievalID.text = LLMs.Instance.chatGLM4Retrieval.GetRetrievalID();
        tMP_InputField0_prompt.text = LLMs.Instance.chatGLM4Retrieval.GetPrompt();
        tMP_InputField1_API.text = LLMs.Instance.charGLM.GetKey();
        tMP_InputField1_prompt.text = LLMs.Instance.charGLM.GetPrompt();
        tMP_InputField1_botName.text = LLMs.Instance.charGLM.GetBotName();
        
        OnGameSettingChanged+=LLMs.Instance.chatGLM4Retrieval.SaveSettings;
    }
    public void SetUIActive(string UIname)
    {
        foreach (GameObject ui in UIs)
        {
            if (ui.name == UIname)
            {
                ui.SetActive(true);
            }
            else
            {
                ui.SetActive(false);
            }
        }
    }
    public void OnClickToSave()
    {
        LLMs.Instance.chatGLM4Retrieval.SetKey(tMP_InputField0_API.text);
        LLMs.Instance.chatGLM4Retrieval.SetRetrievalID(tMP_InputField0_retrievalID.text);
        LLMs.Instance.chatGLM4Retrieval.SetPrompt(tMP_InputField0_prompt.text);
        LLMs.Instance.charGLM.SetKey(tMP_InputField1_API.text);
        LLMs.Instance.charGLM.SetPrompt(tMP_InputField1_prompt.text);
        LLMs.Instance.charGLM.SetBotName(tMP_InputField1_botName.text);
        OnGameSettingChanged?.Invoke();

    }
    public void OnClickToClose(){
        SettingUI.SetActive(false);
    }
}
