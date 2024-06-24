using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMode : MonoBehaviour
{
    public GameObject SettingUI;
    
    void Update()
    {
        if (GlobalInputManager.KeyDown(KeyCode.Escape))
        {
             
            if (SettingUI.activeInHierarchy)
            {
                SettingUI.SetActive(false);
            }
            else
            {
                SettingUI.SetActive(true);
            }
        }
    }
}
