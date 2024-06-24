using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLMs : MonoBehaviour
{
     public static LLMs Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LLMs>();
            }
            return instance;
        }
    }
    private static LLMs instance;
    public CharGLM charGLM;
    public Chatchat chatchat;
    public ChatGLM4Retrieval chatGLM4Retrieval;

}
