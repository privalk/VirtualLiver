using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using UnityEngine;

public class LLM:MonoBehaviour
{
    /// <summary>
    /// api地址
    /// </summary>
    [SerializeField] protected string url;
   
    /// <summary>
    /// 语言
    /// </summary
    //[Header("设置回复的语言")]
    //[SerializeField] protected string lan="中文";
    /// <summary>
    /// 上下文保留条数
    /// </summary>
    [Header("上下文保留条数")]
    [SerializeField] protected int m_HistoryKeepCount = 30;
    /// <summary>
    /// 缓存对话
    /// </summary>
    [SerializeField] public List<SendData> m_DataList = new();
    /// <summary>
    /// 计算方法调用的时间
    /// </summary>
    [SerializeField] protected Stopwatch stopwatch=new();
    /// <summary>
    /// 发送消息
    /// </summary>
    public virtual void PostMsg(string _msg,Action<string> _callback) {
        //上下文条数设置
        CheckHistory();


        string message = _msg;
        //缓存发送的信息列表
        m_DataList.Add(new SendData("user", message));

        StartCoroutine(Request(message, _callback));
    }

    public virtual IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        yield return new WaitForEndOfFrame();
          
    }

    /// <summary>
    /// 设置保留的上下文条数，防止太长
    /// </summary>
    public virtual void CheckHistory()
    {
        if(m_DataList.Count> m_HistoryKeepCount)
        {
            m_DataList.RemoveAt(0);
        }
    }

    [Serializable]
    public class SendData
    {
        [SerializeField] public string role;
        [SerializeField] public string content;
        public SendData() { }
        public SendData(string _role, string _content)
        {
            role = _role;
            content = _content;
        }

    }

}
