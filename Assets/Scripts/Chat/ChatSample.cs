using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WebGLSupport;
using TMPro;
using Live2D.Cubism.Framework.MouthMovement;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Tool;


public class ChatSample : MonoBehaviour
{
    public static ChatSample Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ChatSample>();
            }
            return instance;
        }
    }
    /// <summary>
    /// 单例
    /// </summary>
    private static ChatSample instance;
    /// <summary>
    ///读弹幕
    /// </summary>
    public SpeechText speechText;
    /// <summary>
    /// 是否对话中
    /// </summary>
    public bool isChating = false;
    /// <summary>
    /// 聊天模型
    /// </summary>
    [Header("llm脚本")]
    public LLM m_ChatModel;
    /// <summary>
    /// 语音合成服务
    /// </summary>
    [Header("语音合成脚本")]
    public TTS m_TextToSpeech;

    /// <summary>
    /// 聊天气泡
    /// </summary>
    [SerializeField] private GameObject m_ChatBubble;
    /// <summary>
    /// 返回的信息
    /// </summary>
    public TMP_Text m_TextBack;
    /// <summary>
    /// 播放声音
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;
    /// <summary>
    /// 发送信息按钮
    /// </summary>
    [SerializeField] private Button m_CommitMsgBtn;

    /// <summary>
    /// 动画控制器
    /// </summary>
    [SerializeField] private Animator m_Animator;
    /// <summary>
    /// 语音模式，设置为false,则不合成语音
    /// </summary>
    [SerializeField] private bool m_IsVoiceMode = true;
    /// <summary>
    /// 直接合成文字语音
    /// </summary>
    public bool isCreateVoiceMode = false;


    /// <summary>
    /// 发送信息
    /// </summary>
    /// <param name="_postWord"></param>
    public void SendData(string _postWord)
    {
        if (_postWord.Equals(""))
            return;

        if (isCreateVoiceMode)//合成输入为语音
        {
            CallBack(_postWord);
            return;
        }


        //添加记录聊天
        m_ChatHistory.Add(_postWord);
        //输入内容
        string _msg = _postWord;

        //发送数据
        m_ChatModel.PostMsg(_msg, CallBack);



        //切换思考动作
        SetAnimator("state", 1);
    }

    /// <summary>
    /// AI回复的信息的回调
    /// </summary>
    /// <param name="_response"></param>
    private void CallBack(string _response)
    {
        _response = _response.Trim();
        m_TextBack.text = "";


        Debug.Log("收到AI回复：" + _response);

        //记录聊天
        m_ChatHistory.Add(_response);

        if (!m_IsVoiceMode || m_TextToSpeech == null)
        {
            //开始逐个显示返回的文本

            StartTypeWords(_response);
            return;
        }

        //合成语音
        m_TextToSpeech.Speak(_response, PlayVoice);
    }



    public void PlayVoice(AudioClip _clip, string _response)
    {
        m_AudioSource.clip = _clip;

        m_AudioSource.Play();
        Debug.Log("音频时长：" + _clip.length);
        m_clipLenth=_clip.length;
        //开始逐个显示返回的文本
        StartTypeWords(_response);
        //切换到说话动作
        SetAnimator("state", 2);
    }

    //
    private float m_clipLenth;
    //逐字显示的时间间隔
     private float m_WordWaitTime = 0.3f;
    //是否显示完成
     private bool m_WriteState = false;

    /// <summary>
    /// 开始逐个打印
    /// </summary>
    /// <param name="_msg"></param>
    private void StartTypeWords(string _msg)
    {
        if (_msg == "")
            return;
        m_WordWaitTime=m_clipLenth/_msg.Length;
        print(m_WordWaitTime);
        m_ChatBubble.SetActive(true);
        m_WriteState = true;
        StartCoroutine(SetTextPerWord(_msg));
    }

    private IEnumerator SetTextPerWord(string _msg)
    {
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //更新显示的内容
            m_TextBack.text = _msg.Substring(0, currentPos);

            m_WriteState = currentPos < _msg.Length;

        }

        //切换到等待动作

        SetAnimator("state", 0);
        yield return new WaitForSeconds(3f);
        m_ChatBubble.SetActive(false);
        //结束对话
        isChating = false;
        //检测弹幕队列
        if (LinkToBili.Instance.l_dmMsg.Count > 0)
        {
            speechText.Text = LinkToBili.Instance.l_dmMsg[0];
            speechText.Speak();
            speechText.OnSpeechTextComplete += LinkToBili.Instance.SpeechText_OnCompleted;
        }


    }

    //保存聊天记录
    [SerializeField] private List<string> m_ChatHistory;


    private void SetAnimator(string _para, int _value)
    {
        if (m_Animator == null)
            return;

        m_Animator.SetInteger(_para, _value);
    }
}
