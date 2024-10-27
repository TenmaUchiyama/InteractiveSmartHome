using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestPointable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("<color=red>Pointer Enter</color>");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("<color=blue>Pointer Exit</color>");
    }
}
