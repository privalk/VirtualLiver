using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SongList : MonoBehaviour
{
    public static SongList Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SongList>();
            }
            return instance;
        }
    }
    private static SongList instance;
    public Dictionary<string, AudioClip> Song = new();
    public Transform p_transform;
    public GameObject prefab;
    public AudioSource audioSource;
    public SpriteRenderer micPhone;
    public void Awake()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Song");
        foreach (AudioClip clip in clips)
        {
            Song.Add(clip.name, clip);
        }

    }
    public void Start()
    {
        SongListAddView();
    }
    public void SongListAddView()
    {
        foreach (string item in Song.Keys)
        {
            GameObject ins = Instantiate(prefab, p_transform);
            ins.GetComponent<TMP_Text>().text = item;
        }
    }
    public void PlaySong(string songname)
    {
        if (Song.TryGetValue(songname, out AudioClip foundClip))
        {
            audioSource.clip = foundClip;
            MicInAni();
            audioSource.Play();
            StartCoroutine(AudioCallBack(audioSource, AudioCallBackAction));
        }
        else
        {
            ChatSample.Instance.m_TextToSpeech.Speak("抱歉，我现在不会唱这首歌", ChatSample.Instance.PlayVoice);
        }
    }
    private IEnumerator AudioCallBack(AudioSource AudioObject, Action action)
    {
        while (AudioObject.isPlaying)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        action();
    }
    private void AudioCallBackAction()
    {
        ChatSample.Instance.isChating = false;
        MicOutAni();
        if (LinkToBili.Instance.l_dmMsg.Count > 0)
        {
            LinkToBili.Instance.speechText.Text = LinkToBili.Instance.l_dmMsg[0];
            LinkToBili.Instance.speechText.Speak();
        }

    }
    private void MicInAni()
    {
        micPhone.gameObject.SetActive(true);
        float moveDuration = 1.0f; // 移动动画的持续时间
        float fadeDuration = 1.0f; // 透明度变化的持续时间
        Vector3 moveDistance = new Vector3(0, 0.1f, 0); // 向上移动的距离
        float endOpacity = 1.0f; // 最终的不透明度
        Color initialColor = micPhone.material.color;
        initialColor.a = 0;
        micPhone.material.color = initialColor;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(micPhone.transform.DOMove(micPhone.transform.position + moveDistance, moveDuration)).SetEase(Ease.OutQuad);
        mySequence.Join(micPhone.material.DOFade(endOpacity, fadeDuration));

    }
    private void MicOutAni()
    {
        micPhone.gameObject.SetActive(true);
        float moveDuration = 1.0f; // 移动动画的持续时间
        float fadeDuration = 1.0f; // 透明度变化的持续时间
        Vector3 moveDistance = new Vector3(0, -0.1f, 0); // 向上移动的距离
        float endOpacity = 0; // 最终的不透明度
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(micPhone.transform.DOMove(micPhone.transform.position + moveDistance, moveDuration)).SetEase(Ease.OutQuad);
        mySequence.Join(micPhone.material.DOFade(endOpacity, fadeDuration));

    }
}
