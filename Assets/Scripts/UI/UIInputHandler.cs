using System;
using UnityEngine;
using UnityEngine.Events;

public class UIInputHandler : MonoBehaviour
{
    public void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action callback)
    {
        callback?.Invoke();
    }
}
