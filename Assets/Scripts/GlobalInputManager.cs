using TMPro;
using UnityEngine;

public class GlobalInputManager : MonoBehaviour
{
    // 控制是否启用输入检测
    public static bool inputEnabled = true;

    public static bool KeyDown(KeyCode key)
    {
        if (!inputEnabled) return false;
        return Input.GetKeyDown(key);
    }
    public static bool KeyReturnDown(){
        if (inputEnabled) return false;
        return Input.GetKeyDown(KeyCode.Return);
    }

    public static bool KeyPressed(KeyCode key)
    {
        if (!inputEnabled) return false;
        return Input.GetKey(key);
    }

    public static bool KeyUp(KeyCode key)
    {
        if (!inputEnabled) return false;
        return Input.GetKeyUp(key);
    }

    public static bool MouseButtonDown(int button)
    {
        if (!inputEnabled) return false;
        return Input.GetMouseButtonDown(button);
    }

    public static bool MouseButtonPressed(int button)
    {
        if (!inputEnabled) return false;
        return Input.GetMouseButton(button);
    }

    public static bool MouseButtonUp(int button)
    {
        if (!inputEnabled) return false;
        return Input.GetMouseButtonUp(button);
    }

    public static Vector3 MousePosition()
    {
        if (!inputEnabled) return Vector3.zero;
        return Input.mousePosition;
    }
    
    public static void SwitchInputWhenTmpInputSelected(TMP_InputField tMP_InputField){
        if (tMP_InputField == null) return;
        tMP_InputField.onSelect.AddListener((string arg0) => inputEnabled = false);
        tMP_InputField.onDeselect.AddListener((string arg0) => inputEnabled = true);
    }
}
