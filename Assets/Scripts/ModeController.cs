using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeController : MonoBehaviour
{
    public GameObject LiveMode;
    public TMP_Text LiveModeTxt;
    public GameObject SayMode;
    public TMP_Text SayModeTxt;
    public ToggleGroup toggleGroup;


    void Start()
    {
        Toggle firstToggle = toggleGroup.transform.Find("Toggle1").GetComponent<Toggle>();
        firstToggle.onValueChanged.AddListener(OnFirstToggleValueChanged);

        Toggle secondToggle = toggleGroup.transform.Find("Toggle2").GetComponent<Toggle>();
        secondToggle.onValueChanged.AddListener(OnSecondToggleValueChanged);
    }
    void Update()
    {

    }

    private void OnFirstToggleValueChanged(bool arg0)
    {
        //切换播报模式
        if (!SayMode.activeInHierarchy)
        {
            SayMode.SetActive(true);
        }
        if (LiveMode.activeInHierarchy)
        {
            LiveMode.SetActive(false);
        }
        ChatSample.Instance.isCreateVoiceMode = true;
        ChatSample.Instance.m_TextBack=SayModeTxt;
    }
    private void OnSecondToggleValueChanged(bool arg0)
    {
        //切换直播模式
        if (SayMode.activeInHierarchy)
        {
            SayMode.SetActive(false);
        }
        if (!LiveMode.activeInHierarchy)
        {
            LiveMode.SetActive(true);
        }
        ChatSample.Instance.isCreateVoiceMode = false;
        ChatSample.Instance.m_TextBack=LiveModeTxt;
    }
}

