using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using OpenBLive.Runtime;
using OpenBLive.Runtime.Data;
using OpenBLive.Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;
using UnityEngine.Networking;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Tool;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Physics;
using DG.Tweening;

public class LinkToBili : MonoBehaviour
{
    public static LinkToBili Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LinkToBili>();
            }
            return instance;
        }
    }
    private static LinkToBili instance;
    public WebSocketBLiveClient m_WebSocketBLiveClient;
    private InteractivePlayHeartBeat m_PlayHeartBeat;
    private string gameId;
    public string accessKeySecret;
    public string accessKeyId;
    public string appId;
    public string codeId;
    public Action ConnectSuccess;
    public Action ConnectFailure;
    /// <summary>
    ///读弹幕
    /// </summary>
    public SpeechText speechText;
    /// <summary>
    ///弹幕UI预制体
    /// </summary>
    public GameObject DMUI;
    /// <summary>
    ///弹幕UI父节点
    /// </summary>
    public Transform p_DMUI;
    /// <summary>
    ///弹幕msg列表
    /// </summary>
    public List<string> l_dmMsg = new();
    /// <summary>
    ///弹幕UI实例列表
    /// </summary>
    public List<GameObject> l_DMUI = new();
    /// <summary>
    ///Live2D表情控制
    /// </summary>
    public CubismExpressionController cubismExpressionController;
    /// <summary>
    ///看的方向
    /// </summary>
    public Transform LookTarget;
    private readonly List<Vector3> lookTargetpositions = new(){
        new Vector3(0,0.47f,0),//前
        new Vector3(-0.7f,0.47f,0),//左
        new Vector3(-0.7f,0.47f-0.7f,0),//左下
        new Vector3(0,0.47f-0.7f,0),//下
        new Vector3(0.7f,0.47f-0.7f,0),//右下
        new Vector3(0.7f,0.47f,0),//右
        new Vector3(0.7f,0.47f+0.7f,0),//右上
        new Vector3(0,0.47f+0.7f,0),//上
        new Vector3(-0.7f,0.47f+0.7f,0),//左上
    };
    /// <summary>
    ///风
    /// </summary>
    public CubismPhysicsController wind;

    public void Start()
    {
        LinkStart(codeId);
    }
    public async void LinkStart(string code)
    {
        //测试的密钥
        SignUtility.accessKeySecret = accessKeySecret;
        //测试的ID
        SignUtility.accessKeyId = accessKeyId;
        var ret = await BApi.StartInteractivePlay(code, appId);
        //打印到控制台日志
        var gameIdResObj = JsonConvert.DeserializeObject<AppStartInfo>(ret);
        if (gameIdResObj.Code != 0)
        {
            Debug.LogError(gameIdResObj.Message);
            ConnectFailure?.Invoke();
            return;
        }

        m_WebSocketBLiveClient = new WebSocketBLiveClient(gameIdResObj.GetWssLink(), gameIdResObj.GetAuthBody());
        m_WebSocketBLiveClient.OnDanmaku += WebSocketBLiveClientOnDanmaku;
        m_WebSocketBLiveClient.OnGift += WebSocketBLiveClientOnGift;
        m_WebSocketBLiveClient.OnGuardBuy += WebSocketBLiveClientOnGuardBuy;
        m_WebSocketBLiveClient.OnSuperChat += WebSocketBLiveClientOnSuperChat;

        try
        {
            m_WebSocketBLiveClient.Connect(TimeSpan.FromSeconds(1), 1000000);
            ConnectSuccess?.Invoke();
            Debug.Log("连接成功");
        }
        catch (Exception)
        {
            ConnectFailure?.Invoke();
            Debug.Log("连接失败");
            throw;
        }

        gameId = gameIdResObj.GetGameId();
        m_PlayHeartBeat = new InteractivePlayHeartBeat(gameId);
        m_PlayHeartBeat.HeartBeatError += M_PlayHeartBeat_HeartBeatError;
        m_PlayHeartBeat.HeartBeatSucceed += M_PlayHeartBeat_HeartBeatSucceed;
        m_PlayHeartBeat.Start();

    }


    public async Task LinkEnd()
    {
        m_WebSocketBLiveClient.Dispose();
        m_PlayHeartBeat.Dispose();
        await BApi.EndInteractivePlay(appId, gameId);
        Debug.Log("游戏关闭");
    }

    private void WebSocketBLiveClientOnSuperChat(SuperChat superChat)
    {
        StringBuilder sb = new StringBuilder("收到SC!");
        sb.AppendLine();
        sb.Append("来自用户：");
        sb.AppendLine(superChat.userName);
        sb.Append("留言内容：");
        sb.AppendLine(superChat.message);
        sb.Append("金额：");
        sb.Append(superChat.rmb);
        sb.Append("元");
        Debug.Log(sb);
    }

    private void WebSocketBLiveClientOnGuardBuy(Guard guard)
    {
        StringBuilder sb = new StringBuilder("收到大航海!");
        sb.AppendLine();
        sb.Append("来自用户：");
        sb.AppendLine(guard.userInfo.userName);
        sb.Append("赠送了");
        sb.Append(guard.guardUnit);
        Debug.Log(sb);
    }

    private void WebSocketBLiveClientOnGift(SendGift sendGift)
    {
        StringBuilder sb = new StringBuilder("收到礼物!");
        sb.AppendLine();
        sb.Append("来自用户：");
        sb.AppendLine(sendGift.userName);
        sb.Append("赠送了");
        sb.Append(sendGift.giftNum);
        sb.Append("个");
        sb.Append(sendGift.giftName);
        Debug.Log(sb);
    }

    private void WebSocketBLiveClientOnDanmaku(Dm dm)
    {
        StringBuilder sb = new StringBuilder("收到弹幕!");
        sb.AppendLine();
        sb.Append("用户：");
        sb.AppendLine(dm.userName);
        sb.Append("弹幕内容：");
        sb.Append(dm.msg);
        Debug.Log(sb);

        //显示弹幕
        //如果弹幕数量超过5则摧毁队首ui实例，并将其在列表中移除
        if (l_DMUI.Count > 5)
        {
            Destroy(l_DMUI[0]);
            l_DMUI.RemoveAt(0);

        }
        GameObject t_DMUI = Instantiate(DMUI, p_DMUI);//创建弹幕UI实例
        t_DMUI.transform.Find("msg").GetComponent<TMP_Text>().text = dm.msg;
        t_DMUI.transform.Find("userName").GetComponent<TMP_Text>().text = dm.userName;
        StartCoroutine(DownSprite(dm.userFace, t_DMUI.transform.Find("userFace").GetComponent<RawImage>()));//协程 下载用户头像图片
        //将UI实例添加到列表中
        l_DMUI.Add(t_DMUI);
        //将弹幕添加到列表中并去除两边空格
        l_dmMsg.Add(dm.msg.Trim());
        //读弹幕
        if (!ChatSample.Instance.isChating)
        {
            speechText.Text = l_dmMsg[0];
            speechText.Speak();
            speechText.OnSpeechTextComplete += SpeechText_OnCompleted;
        }


    }

    public void SpeechText_OnCompleted()
    {
        if (l_dmMsg.Count > 0)
        {
            if (!ChatSample.Instance.isChating)
            {
                ChatSample.Instance.isChating = true;
                switch (l_dmMsg[0])
                {
                    case string s when s.StartsWith("点歌 "):
                        ChatSample.Instance.isChating = true;
                        l_dmMsg[0] = l_dmMsg[0][3..];
                        SongList.Instance.PlaySong(l_dmMsg[0]);
                        l_dmMsg.RemoveAt(0);
                        break;
                    case "正常":
                        cubismExpressionController.CurrentExpressionIndex = 0;
                        NextDm();
                        break;
                    case "抱枕":
                        cubismExpressionController.CurrentExpressionIndex = 1;
                        NextDm();
                        break;
                    case "掉小珍珠":
                        cubismExpressionController.CurrentExpressionIndex = 2;
                        NextDm();
                        break;
                    case "生气":
                        cubismExpressionController.CurrentExpressionIndex = 3;
                        NextDm();
                        break;
                    case "举旗":
                        cubismExpressionController.CurrentExpressionIndex = 4;
                        NextDm();
                        break;
                    case "白眼":
                        cubismExpressionController.CurrentExpressionIndex = 5;
                        NextDm();
                        break;
                    case "歪嘴":
                        cubismExpressionController.CurrentExpressionIndex = 6;
                        NextDm();
                        break;
                    case "向前看":
                        LookTarget.position = lookTargetpositions[0];
                        NextDm();
                        break;
                    case "向后看":
                        ChatSample.Instance.m_TextToSpeech.Speak("抱歉，我做不到啊",  ChatSample.Instance.PlayVoice);
                        NextDm();
                        break;
                    case "向左看":
                        LookTarget.position = lookTargetpositions[1];
                        NextDm();
                        break;
                    case "向左下看":
                        LookTarget.position = lookTargetpositions[2];
                        NextDm();
                        break;
                    case "向下看":
                        LookTarget.position = lookTargetpositions[3];
                        NextDm();
                        break;
                    case "向右下看":
                        LookTarget.position = lookTargetpositions[4];
                        NextDm();
                        break;
                    case "向右看":
                        LookTarget.position = lookTargetpositions[5];
                        NextDm();
                        break;
                    case "向右上看":
                        LookTarget.position = lookTargetpositions[6];
                        NextDm();
                        break;
                    case "向上看":
                        LookTarget.position = lookTargetpositions[7];
                        NextDm();
                        break;
                    case "向左上看":
                        LookTarget.position = lookTargetpositions[8];
                        NextDm();
                        break;
                    case string s when s.StartsWith("风 "):
                        l_dmMsg[0] = l_dmMsg[0][2..];
                        string[] w = l_dmMsg[0].Split(' ');
                        Vector2 windforce = new(float.Parse(w[0]), 0);
                        float duration = float.Parse(w[1]);
                        DOTween.To(() => windforce, x => wind.SetWind(x), Vector2.zero, duration);
                        NextDm();
                        break;
                    default:
                        ChatSample.Instance.SendData(l_dmMsg[0]);
                        l_dmMsg.RemoveAt(0);
                        break;
                }
            }
        }

    }


    private void NextDm()
    {
        l_dmMsg.RemoveAt(0);
        ChatSample.Instance.isChating = false;
        SpeechText_OnCompleted();
    }

    private static void M_PlayHeartBeat_HeartBeatSucceed()
    {
        Debug.Log("心跳成功");
    }

    private static void M_PlayHeartBeat_HeartBeatError(string json)
    {
        Debug.Log("心跳失败" + json);
    }



    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (m_WebSocketBLiveClient is { ws: { State: WebSocketState.Open } })
        {
            m_WebSocketBLiveClient.ws.DispatchMessageQueue();
        }
#endif
    }

    private void OnDestroy()
    {
        if (m_WebSocketBLiveClient == null)
            return;

        m_PlayHeartBeat.Dispose();
        m_WebSocketBLiveClient.Dispose();
    }
    private IEnumerator DownSprite(string url, RawImage rawImage)
    {
        using UnityWebRequest request = new UnityWebRequest(url);
        DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
        request.downloadHandler = texDl;
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(request.error);
        }
        else
        {
            rawImage.texture = texDl.texture;
        }
    }

}
