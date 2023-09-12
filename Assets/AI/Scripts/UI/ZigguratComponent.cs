using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZigguratComponent : MonoBehaviour, IPointerClickHandler
{
    
    /// <summary>
    /// ������� ����� �� ������� �������
    /// </summary>
    public event ClickEventHandler OnClickEventHandler;

    //��� ������� ������ �� �������, ���������� ������ �����
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickEventHandler?.Invoke(this, eventData);
    }

    public delegate void ClickEventHandler(ZigguratComponent ziggurat, PointerEventData eventData);
}
