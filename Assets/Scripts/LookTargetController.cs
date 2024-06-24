using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LookTargetController : MonoBehaviour
{
    public GameObject Looktarget;
    public ToggleGroup toggleGroup;
    private bool isFollowMouse = false;
    private Vector3 orginPositon;
    void Start()
    {   
        orginPositon = transform.position;
        Toggle firstToggle = toggleGroup.transform.Find("Toggle1").GetComponent<Toggle>();
        firstToggle.onValueChanged.AddListener(OnFirstToggleValueChanged);

        Toggle secondToggle = toggleGroup.transform.Find("Toggle2").GetComponent<Toggle>();
        secondToggle.onValueChanged.AddListener(OnSecondToggleValueChanged);
    }
    void Update()
    {
        if (isFollowMouse){
            Vector3 mousePosition = GlobalInputManager.MousePosition();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Looktarget.transform.position=worldPosition;
        }
    }

    private void OnFirstToggleValueChanged(bool arg0)
    {
        isFollowMouse = true;
    }
    private void OnSecondToggleValueChanged(bool arg0)
    {
        isFollowMouse = false;
        Looktarget.transform.position=orginPositon;
    }

}
