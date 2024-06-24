using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;


public class GPTSoVITSTextToSpeech : TTS
{

    [SerializeField] private AudioClip m_ReferenceClip = null;//参考音频

    [SerializeField] private string m_ReferenceText = "";//参考音频文本

    [SerializeField] private Language m_ReferenceTextLan = Language.中文;//参考音频的语言

    [SerializeField] private Language m_TargetTextLan = Language.中文;//合成音频的语言
    private string m_AudioBase64String = "";//参考音频的base64编码
    private void Awake()
    {
        AudioTurnToBase64();
    }

    /// <summary>
    /// 语音合成，返回合成文本
    /// </summary>
    /// <param name="_msg"></param>
    /// <param name="_callback"></param>
    public override void Speak(string _msg, Action<AudioClip, string> _callback)
    {
        StartCoroutine(GetVoice(_msg, _callback));
    }

    /// <summary>
    /// 合成音频
    /// </summary>
    /// <param name="_msg"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetVoice(string _msg, Action<AudioClip, string> _callback)
    {
        //发送报文
        string _postJson = GetPostJson(_msg);

        using (UnityWebRequest request = new UnityWebRequest(m_PostURL, "POST"))
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_postJson);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                 string _text = request.downloadHandler.text;
                Response _response=JsonConvert.DeserializeObject<Response>(_text);
                if (_response.data[0]!=null){
                    string _wavPath = _response.data[0].name;
                    StartCoroutine(GetAudioFromFile(_wavPath, _msg, _callback));
                }
                else
                {
                    //如果合成失败，再尝试一次
                    StartCoroutine(GetVoice(_msg, _callback));
                }


            }
            else
            {
                Debug.LogError("语音合成失败: " + request.error);
            }
        }


    }


    /// <summary>
    /// 处理发送的Json报文
    /// </summary>
    /// <param name="_msg"></param>
    /// <returns></returns>
    private string GetPostJson(string _msg)
    {

        if (m_ReferenceText == "" || m_ReferenceClip == null)
        {
            Debug.LogError("GPT-SoVITS未配置参考音频或参考文本");
            return null;
        }


        // 创建数据结构
        var jsonData = new
        {
            data = new List<object>
            {
                new { name = "audio.wav", data = "data:audio/wav;base64,"+m_AudioBase64String },
                m_ReferenceText,
                m_ReferenceTextLan.ToString(),
                _msg,
                m_TargetTextLan.ToString(),
                "凑四句一切",
                5,
                1,
                1,
                false
            }
        };

        // 将数据转换为JSON格式
        string jsonString = JsonConvert.SerializeObject(jsonData, Formatting.Indented);

        return jsonString;
    }

    /// <summary>
    /// 将音频转为base64
    /// </summary>
    private void AudioTurnToBase64()
    {
        if (m_ReferenceClip == null)
        {
            Debug.LogError("GPT-SoVITS未配置参考音频");
            return;
        }
        byte[] audioData = ACToBae64(m_ReferenceClip);
        string base64String = Convert.ToBase64String(audioData);
        m_AudioBase64String = base64String;
    }
    public static byte[] ACToBae64(AudioClip clip)
    {
        // Create a new WAV file
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        // Write the WAV header
        writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + clip.samples * 2);
        writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
        writer.Write(new char[4] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((ushort)1);
        writer.Write((ushort)clip.channels);
        writer.Write(clip.frequency);
        writer.Write(clip.frequency * clip.channels * 2);
        writer.Write((ushort)(clip.channels * 2));
        writer.Write((ushort)16);
        writer.Write(new char[4] { 'd', 'a', 't', 'a' });
        writer.Write(clip.samples * 2);

        // Write the audio data
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        int intMax = 32767; // max value for a 16-bit signed integer
        for (int i = 0; i < clip.samples; i++)
        {
            writer.Write((short)(samples[i] * intMax));
        }

        // Clean up
        writer.Close();
        byte[] wavBytes = stream.ToArray();
        stream.Close();
        return wavBytes;
    }

    /// <summary>
    /// 从本地获取合成后的音频文件
    /// </summary>
    /// <param name="_path"></param>
    /// <param name="_msg"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private IEnumerator GetAudioFromFile(string _path, string _msg, Action<AudioClip, string> _callback)
    {
        string filePath = "file://" + _path;
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.WAV))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                _callback(audioClip, _msg);
            }
            else
            {
                Debug.LogError("音频读取失败 ：" + request.error);
            }
        }


    }




    public class Response
    {
        public List<AudioBack> data;
        public bool is_generating = true;
        public float duration;
        public float average_duration;
    }

    public class AudioBack
    {
        public string name;
        public string data;
        public bool is_file;
    }

    public enum Language
    {
        中文,
        英文,
        日文,
        中英混合,
        日英混合,
        多语种混合,
    }




}
