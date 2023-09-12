using AI.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace AI
{
    public class PropertiesMenuComponent : MonoBehaviour
    {
        [Tooltip("ссылка на панель с кнопками"), SerializeField]
        private PropertiesMenuController _propertiesMenuPanel;
        private ZigguratComponent _zigguratComponent;


        private PlayerControls _controls;

        private void Awake()
        {
            _controls = new PlayerControls();
            _zigguratComponent = GetComponentInParent<ZigguratComponent>();
            _zigguratComponent.OnClickEventHandler += ZigguratClicKHandler;
        }

        private void ZigguratClicKHandler(ZigguratComponent ziggurat, PointerEventData eventData)
        {
           if  (!_propertiesMenuPanel.isActiveAndEnabled) _propertiesMenuPanel.OpenDialog(ziggurat, eventData);
           else _propertiesMenuPanel.CloseDialog(ziggurat, eventData);
        }

    }
}
