using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Chatchat : LLM
{

    private void Awake()
    {
        OnInit();
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <returns></returns>
    public override void PostMsg(string _msg, Action<string> _callback)
    {
        base.PostMsg(_msg, _callback);
    }


    /// <summary>
    /// 发送数据
    /// </summary> 
    /// <param name="_postWord"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public override IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        stopwatch.Restart();
        string jsonPayload = JsonConvert.SerializeObject(new RequestData
        {
            query = _postWord,
            history = m_DataList
        });

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");


            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;
                
                // 找到子字符串的索引
                int startIndex = _msg.IndexOf("data")+5;

                // 从startIndex开始截取剩余的字符串
                string resultString = _msg[startIndex..];
                Debug.Log(resultString);
                ResponseData response = JsonConvert.DeserializeObject<ResponseData>(resultString);
                string _msgBack = response.answer;

                //添加记录
                m_DataList.Add(new SendData("assistant", _msgBack));
                //回调
                _callback(_msgBack);

            }

        }

        stopwatch.Stop();
        Debug.Log("chatchat耗时：" + stopwatch.Elapsed.TotalSeconds);
    }



    /// <summary>
    /// 初始化
    /// </summary>
    private void OnInit()
    {
        GetEndPointUrl();
    }
    /// <summary>
    /// 获取资源路径
    /// </summary>
    private void GetEndPointUrl()
    {
        url = "http://127.0.0.1:7861/chat/knowledge_base_chat";
    }




    /// <summary>
    /// 模型类型
    /// </summary>
    public enum ModelType
    {
        chatglm_turbo,
        charglm3,
    }


    private class RequestData
    {
        public string query;
        public string knowledge_base_name = "huohuo";
        public int top_k = 3;
        public int score_threshold = 1;
        public List<SendData> history;
        public bool stream = false;
        public string model_name = "Qwen-1_8B-Chat";
        public float temperature = 0.7f;
        public int max_tokens = 0;
        public string prompt_name = "huohuo";
    }


    private class ResponseData
    {
        public string answer;
        public List<string> docs;

    }



}
