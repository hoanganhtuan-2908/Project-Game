using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInputReciever : InputReciever
{
    private Vector3 clickPosition;
    void Update()
    {
        // If the pointer is over a UI element (like the pause menu), ignore board input
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[Input] Camera.main is null. Ensure the camera has tag MainCamera.");
                return;
            }
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                Debug.Log(string.Format("[Input] Raycast hit {0} at {1}", hit.collider != null ? hit.collider.name : "null", clickPosition));
                OnInputRecieved();
            }
            else
            {
                Debug.Log("[Input] Raycast missed.");
            }
        }
    }

    public override void OnInputRecieved()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPosition, null, null);
        }
    }
}