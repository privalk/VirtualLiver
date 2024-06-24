using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class ChatGLM4Retrieval : LLM
{


    /// <summary>
    /// apikey
    /// </summary>
    [SerializeField] private string m_Key = string.Empty;
    //api key
    private string m_ApiKey = string.Empty;
    //secret key
    private string m_SecretKey = string.Empty;
    private List<Tools> m_tools = new();
    public  string retrievalID;
    public  string prompt ;


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
        m_tools.Clear();
        m_tools.Add(new("retrieval",retrievalID,prompt));
        string jsonPayload = JsonConvert.SerializeObject(new RequestData
        {
            model = "glm-4",
            messages = m_DataList,
            tools = m_tools
        });
        //Debug.Log(jsonPayload);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", GetToken());

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;

                ResponseData response = JsonConvert.DeserializeObject<ResponseData>(_msg);

                if (response.choices.Count > 0)
                {
                    string _msgBack = response.choices[0].message.content;

                    //添加记录
                    m_DataList.Add(new SendData("assistant", _msgBack));
                    //回调
                    _callback(_msgBack);
                }
                else
                {
                    Debug.Log(_msg);
                }
            }

        }

        stopwatch.Stop();
        Debug.Log("ChatGLM4耗时：" + stopwatch.Elapsed.TotalSeconds);
    }



    /// <summary>
    /// 初始化
    /// </summary>
    private void OnInit()
    {
        SplitKey();
        GetEndPointUrl();

        LoadSettings();


    }
    /// <summary>
    /// 保存配置
    /// </summary>
    public void SaveSettings(){
        Debug.Log("ChatGLM4配置已保存");
        m_tools.Clear();
        m_tools.Add(new("retrieval",retrievalID,prompt));
        string js = JsonConvert.SerializeObject(new RequestData
        {
            tools = m_tools
        });
            string fileUrl = Application.persistentDataPath + "\\ChatGLM4Settings.json";
             Debug.Log(fileUrl);
            using (StreamWriter sw = new StreamWriter(fileUrl))
            {
                sw.Write(js);
                sw.Close();
                sw.Dispose();
            }
    }
    /// <summary>
    /// 读取配置
    /// </summary>
    public void LoadSettings()
    {
        string fileUrl = Application.persistentDataPath + "\\ChatGLM4Settings.json";
        if (!File.Exists(fileUrl)) return;
        using (StreamReader sr = new StreamReader(fileUrl))
        {
            string line = sr.ReadLine();
            Debug.Log(line);
            RequestData Request = JsonConvert.DeserializeObject<RequestData>(line);
            prompt=Request.tools[0].retrieval.prompt_template;
            retrievalID=Request.tools[0].retrieval.knowledge_id;
        }

    }
    /// <summary>
    /// 获取url
    /// </summary>
    private void GetEndPointUrl()
    {
        url = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

    }
    /// <summary>
    /// 处理key
    /// </summary>
    private void SplitKey()
    {
        try
        {
            if (m_Key == "")
                return;

            string[] _split = m_Key.Split('.');
            m_ApiKey = _split[0];
            m_SecretKey = _split[1];
        }
        catch { }


    }


    /// <summary>
    /// 生成api鉴权 token
    /// </summary>
    /// <returns></returns>
    private string GetToken()
    {
        long expirationMilliseconds = DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds();
        long timestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        string jwtToken = GenerateJwtToken(m_ApiKey, expirationMilliseconds, timestampMilliseconds);
        return jwtToken;
    }
    //获取token
    private string GenerateJwtToken(string apiKeyId, long expirationMilliseconds, long timestampMilliseconds)
    {
        // 构建Header
        string _headerJson = "{\"alg\":\"HS256\",\"sign_type\":\"SIGN\"}";

        string encodedHeader = Base64UrlEncode(_headerJson);

        // 构建Payload
        string _playLoadJson = string.Format("{{\"api_key\":\"{0}\",\"exp\":{1}, \"timestamp\":{2}}}", apiKeyId, expirationMilliseconds, timestampMilliseconds);

        string encodedPayload = Base64UrlEncode(_playLoadJson);

        // 构建签名
        string signature = HMACsha256(m_SecretKey, $"{encodedHeader}.{encodedPayload}");
        // 组合Header、Payload和Signature生成JWT令牌
        string jwtToken = $"{encodedHeader}.{encodedPayload}.{signature}";

        return jwtToken;
    }
    // Base64 URL编码
    private string Base64UrlEncode(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        string base64 = Convert.ToBase64String(inputBytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
    // 使用HMAC SHA256生成签名
    private string HMACsha256(string apiSecretIsKey, string buider)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(apiSecretIsKey);
        HMACSHA256 hMACSHA256 = new System.Security.Cryptography.HMACSHA256(bytes);
        byte[] date = Encoding.UTF8.GetBytes(buider);
        date = hMACSHA256.ComputeHash(date);
        hMACSHA256.Clear();

        return Convert.ToBase64String(date);

    }

    public string GetKey()
    {
        return m_Key;
    }
    public void SetKey(String _key)
    {
        m_Key=_key;
    }
    public  string GetPrompt()
    {
        return prompt;
    }
    public void SetPrompt(String _prompt)
    {
        prompt=_prompt;
    }
    public string GetRetrievalID()
    {
        return retrievalID;
    }
    public void SetRetrievalID(String _retrievalID)
    {
        retrievalID=_retrievalID;
    }

    private class RequestData
    {
        public string model;
        public List<SendData> messages;
        public List<Tools> tools;
    }



    private class ResponseData
    {
        public List<Choice> choices = new();

    }
    private class Choice
    {
        public Message message;
    }
    private class Message
    {
        public string content;
    }

    public class Tools
    {
        public string type;

        public struct Retrieval
        {
            public string knowledge_id;
            public string prompt_template;
        }

        public Retrieval retrieval;
        public Tools(string _tpye, string _knowledge_id, string _prompt_template)
        {
            type = _tpye;
            retrieval.knowledge_id = _knowledge_id;
            retrieval.prompt_template = _prompt_template;
        }

    }


}
