using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class InstantTweener : MonoBehaviour, IObjectTweener
{
    public void MoveTo(Transform transform, Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }
}
