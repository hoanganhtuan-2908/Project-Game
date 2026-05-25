using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Serialization;

public class ColliderInputReciever : InputReciever
{
    private Vector3 clickPossition;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                clickPossition = hit.point;
                OnInputRecieved();
            }

        }
    
}
public override void OnInputRecieved()
    {
       foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPossition, null, null);
        }
    }
}
