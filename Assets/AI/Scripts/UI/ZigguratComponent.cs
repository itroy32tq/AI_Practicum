using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZigguratComponent : MonoBehaviour, IPointerClickHandler
{
    
    /// <summary>
    /// Событие клика на игровом объекте
    /// </summary>
    public event ClickEventHandler OnClickEventHandler;

    //При нажатии мышкой по объекту, вызывается данный метод
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEventHandler?.Invoke(this, eventData);
    }

    public delegate void ClickEventHandler(ZigguratComponent ziggurat, PointerEventData eventData);
}
