using UnityEngine;
using System.Collections.Generic;
using System;

public interface InputHandler 
{
    void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action callback);

}
