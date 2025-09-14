using UnityEngine;
using UnityEngine.InputSystem;

public class InputMgr
{
    //单例模式
    private static InputMgr _instance = new InputMgr();
    public static InputMgr Instance => _instance;
    public InputSystem_Actions InputSystem;

    //私有化构造函数，只允许自己实例化单例
    private InputMgr()
    {
        Debug.Log("InputMgr");
        InputSystem = new InputSystem_Actions();
        InputSystem.Enable();
    }
    
    public void Enable(InputActionMap map)
    {
        map.Enable();
        if (map == InputSystem.PlantMode.Get())
        {
            Debug.Log("激活种植模式控制系统");
            InputSystem.PlantMode.PlantAction.performed += StartScript.Instance.PlantActionCallBack;
        }
        // if (map == InputSystem.Camera.Get())
        // {
        //     Debug.Log("激活camera旋转控制系统");
        //     InputSystem.Camera.Rotate.performed += _mainCamera.RotatePerformed;
        //     InputSystem.Camera.Rotate.canceled += _mainCamera.RotateCanceled;
        // }
        // if (map == InputSystem.Combat.Get())
        // {
        //     Debug.Log("激活战斗控制系统");
        //     InputSystem.Combat.ReleaseSkill.performed += SkillArea.Instance.ReleaseSkill;
        // }
        // if (map == InputSystem.UI.Get())
        // {
        //     Debug.Log("激活UI按键");
        //     InputSystem.UI.Click.performed += DialogueMgr.Instance.NextSentence;
        // }
    }

    // public void Enable(InputActionMap map, InteractiveTrigger trigger)
    // {
    //     map.Enable();
    //     if (map == InputSystem.Interact.Get())
    //     {
    //         InputSystem.Interact.Interact.performed += trigger.TriggerDialogue;
    //         InputSystem.Interact.StartCombat.performed += trigger.TriggerCombat;
    //     }
    // }
    //
    // public void Disable(InputActionMap map, InteractiveTrigger trigger)
    // {
    //     map.Disable();
    //     if (map == InputSystem.Interact.Get())
    //     {
    //         InputSystem.Interact.Interact.performed -= trigger.TriggerDialogue;
    //         InputSystem.Interact.StartCombat.performed -= trigger.TriggerCombat;
    //     }
    // }
    //
    public void Disable(InputActionMap map)
    {
        map.Disable();
        if (map == InputSystem.PlantMode.Get())
        {
            Debug.Log("禁用种植模式控制系统");
            InputSystem.PlantMode.PlantAction.performed -= StartScript.Instance.PlantActionCallBack;
        }
        // if (map == InputSystem.Camera.Get())
        // {
        //     Debug.Log("禁用camera旋转控制系统");
        //     InputSystem.Camera.Rotate.performed -= _mainCamera.RotatePerformed;
        //     InputSystem.Camera.Rotate.canceled -= _mainCamera.RotateCanceled;
        // }
        // if (map == InputSystem.Combat.Get())
        // {
        //     Debug.Log("禁用战斗控制系统");
        //     InputSystem.Combat.ReleaseSkill.performed -= SkillArea.Instance.ReleaseSkill;
        // }
        // if (map == InputSystem.UI.Get())
        // {
        //     Debug.Log("禁用UI按键");
        //     InputSystem.UI.Click.performed -= DialogueMgr.Instance.NextSentence;
        // }
    }
    
    public void Restart()
    {
        InputSystem.Dispose();
        _instance = new InputMgr(); // 等待 GC 回收
    }
}