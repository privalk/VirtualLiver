using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CharGLM : LLM
{


    /// <summary>
    /// 智普AI的apikey
    /// </summary>
    [Header("填写智普AI的apikey")]
    [SerializeField] private string m_Key = string.Empty;
    //api key
    private string m_ApiKey = string.Empty;
    //secret key
    private string m_SecretKey = string.Empty;
    private string prompt = "藿藿，米哈游出品的游戏《崩坏：星穹铁道》的角色。性格内向，说话略有一些口吃。可怜又弱小的狐人小姑娘，也是怕鬼捉鬼的罗浮十王司见习判官。名为“尾巴”的岁阳被十王司的判官封印在她的颀尾上，使她成为了招邪的“贞凶之命”。害怕妖魔邪物，却总是受命捉拿邪祟，完成艰巨的除魔任务；自认能力不足，却无法鼓起勇气辞职，只好默默害怕地继续下去。新上任的十王司判官，仙舟罗浮的十王司见习判官，被岁阳附身的狐人小女孩。性格怯懦，弱小可怜，害怕种种怪异之事却肩负起勾摄邪魔的职责。藿藿小时候因一场变故，被长生种妖物“岁阳”寄生在了尾巴上，十王司将妖物封印后，也将藿藿纳入麾下成为了见习判官 。虽然名为“尾巴”的岁阳给藿藿添加了许多麻烦，但因为平时被符咒封印着，尾巴并不会对藿藿的身体造成什么影响 。因为尾巴的存在，她从小就备受瞩目 。景元是仙舟罗浮的将军。";
    private string bot_name = "藿藿";

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
            model = "charglm-3",
            meta ={
                user_info="看直播的用户",
                bot_info=prompt,
                bot_name=bot_name,
                user_name="user"
            },
            prompt = m_DataList
        });

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
                if (response.data.choices.Count > 0)
                {
                    //Debug.Log(response.data.choices[0].content);
                    string _msgBack = response.data.choices[0].content.Substring(1, response.data.choices[0].content.Length - 4);

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
        Debug.Log("charGLM耗时：" + stopwatch.Elapsed.TotalSeconds);
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
    /// 获取资源路径
    /// </summary>
    private void GetEndPointUrl()
    {
        url = "https://open.bigmodel.cn/api/paas/v3/model-api/charglm-3/invoke";
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public void SaveSettings()
    {
        Debug.Log("CharGLM配置已保存");
        string js = JsonConvert.SerializeObject(new RequestData
        {
            model = "charglm-3",
            meta ={
                user_info="看直播的用户",
                bot_info=prompt,
                bot_name=bot_name,
                user_name="user"
            }
        });
        string fileUrl = Application.persistentDataPath + "\\CharGLMSettings.json";
        // Debug.Log(fileUrl);
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
        string fileUrl = Application.persistentDataPath + "\\CharGLMSettings.json";
        if (!File.Exists(fileUrl)) return;
        using (StreamReader sr = new StreamReader(fileUrl))
        {
            string line = sr.ReadLine();
            RequestData Request = JsonConvert.DeserializeObject<RequestData>(line);
            prompt=Request.meta.bot_info;
            bot_name=Request.meta.bot_name;
        }



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
        //Debug.Log(jwtToken);
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
        m_Key = _key;
    }
    public string GetPrompt()
    {
        return prompt;
    }
    public void SetPrompt(String _prompt)
    {
        prompt = _prompt;
    }
    public string GetBotName()
    {
        return bot_name;
    }
    public void SetBotName(String _bot_name)
    {
        bot_name = _bot_name;
    }




    private class RequestData
    {
        public string model;
        public List<SendData> prompt;
        public struct Meta
        {
            public string user_info;
            public string bot_info;
            public string bot_name;
            public string user_name;
        }
        public Meta meta;
        public float temperature = 0.7f;
    }


    private class ResponseData
    {
        public int code;
        public string msg = string.Empty;
        public string success = string.Empty;
        public ReData data = new ReData();

    }


    private class ReData
    {
        public string task_id = string.Empty;
        public string request_id = string.Empty;
        public string task_status = string.Empty;
        public List<SendData> choices = new List<SendData>();
    }



}
