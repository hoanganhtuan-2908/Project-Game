using UnityEngine;

public abstract class InputReciever : MonoBehaviour
{
    protected InputHandler[] inputHandlers;

    public abstract void OnInputRecieved();

    private void Awake()
    {
        inputHandlers = GetComponents<InputHandler>();

        Debug.Log("Handlers Found: " + inputHandlers.Length);
    }
}