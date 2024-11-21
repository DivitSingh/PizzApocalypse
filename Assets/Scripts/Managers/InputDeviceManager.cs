using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputDeviceManager : MonoBehaviour
{
    public static InputDeviceManager Instance { get; private set; }
    public bool IsGamepad { get; private set; }

    public event Action<bool> OnGamepadStatusChanged;
    
    private void Awake()
    {
        Instance = this;
        InputSystem.onAnyButtonPress.Call(control =>
        {
            var newStatus = control.device is Gamepad;
            if (newStatus != IsGamepad)
            {
                IsGamepad = newStatus;
                OnGamepadStatusChanged?.Invoke(IsGamepad);    
            }
            
        });
    }
}