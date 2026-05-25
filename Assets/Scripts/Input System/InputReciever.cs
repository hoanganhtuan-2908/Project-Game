using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class InputReciever : MonoBehaviour

{
    protected InputHandler[] inputHandlers;
    public abstract void OnInputRecieved();
    private void Awake()
    {
        inputHandlers = GetComponents<InputHandler>();
    }

}
