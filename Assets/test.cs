using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Tool;

public class test : MonoBehaviour
{
    public Button button;
    public SpeechText speechText;
    void Start()
    {   
        button=this.GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    public void OnButtonClick()
    {
        speechText.Text="啊啊啊啊啊";
        speechText.Speak();
        
    } 
}
