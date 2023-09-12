using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using System.Collections.Generic;
using AI.Units;
using System.Collections;
using UnityEngine.EventSystems;

namespace AI.Managers
{
    public class PropertiesMenuController : MonoBehaviour
    {
        [Tooltip("ссылка на  лабел с количеством жизней"), SerializeField]
        public Text _propertiesHeltsLabel;

        [Tooltip("ссылка на  слайдер с количеством жизней"), SerializeField]
        public Slider _propertiesHeltsSlider;

        [Tooltip("ссылка на  лабел со скоростью юнита"), SerializeField]
        public Text _propertiesSpeedLabel;

        [Tooltip("ссылка на  слайдер со скоростью юнита"), SerializeField]
        public Slider _propertiesSpeedSlider;

        [Tooltip("ссылка на  лабел с уроном от быстрой атаки"), SerializeField]
        public Text _propertiesFastAttackDamageLabel;

        [Tooltip("ссылка на  слайдер с уроном от быстрой атаки"), SerializeField]
        public Slider _propertiesFastAttackDamageSlider;

        [Tooltip("ссылка на  лабел с уроном от сильной атаки"), SerializeField]
        public Text _propertiesStrongAttackDamageLabel;

        [Tooltip("ссылка на  слайдер с уроном от сильной атаки"), SerializeField]
        public Slider _propertiesStrongAttackDamageSlider;

        [Tooltip("ссылка на  лабел с шансом крит урона"), SerializeField]
        public Text _propertiesCriticalChanceLabel;

        [Tooltip("ссылка на  слайдер с шансом крит урона"), SerializeField]
        public Slider _propertiesCriticalChanceSlider;

        [Tooltip("ссылка на  кнопку массовго убийства"), SerializeField]
        public Button _propertiesKillEmAllButton;

        [Tooltip("ссылка на  кнопку отображения здоровья"), SerializeField]
        public Button _propertiesShowAllHelthButton;



        [Inject]
        private Dictionary<UnitType, StatsData> _params;
        [Inject]
        private AIManager _manager;
        private StatsData _data;

        protected UnitPropertiesAssistant _propertiesAssistant;

        [SerializeField, Tooltip("Тип юнита")]
        protected UnitType _unittype;

        private BaseParamsData _baseParams;

        private MobilityParamsData _mobilityParams;

        private BattleParamsData _battleParams;

        private ProbabilityParamsData _probabilityParams;

        private RectTransform _recttransform;
        private RectTransform _recttransformcanvas;

        public bool isActivCoroutine = false;

        private List<GameObject> _elements = new List<GameObject>();

        private Vector2 _startpos;
        private Vector2 _endpos;


        [Tooltip("время трансформации на закрытие, открытие диалога"), SerializeField]
        float _speedTransform = 0.5f;


        void Awake()
        {
            this.gameObject.SetActive(false);
            _recttransform = this.gameObject.GetComponent<RectTransform>();
            _recttransformcanvas = GetComponentInParent<RectTransform>();
        }

        void Start()
        {
            _data = _params[_unittype];

            _propertiesHeltsSlider.maxValue = _data.BaseParams.MaxHealth*2;
            _propertiesHeltsSlider.value = _data.BaseParams.MaxHealth;
            _propertiesSpeedSlider.maxValue = _data.MobilityParams.MoveSpeed*2;
            _propertiesSpeedSlider.value = _data.MobilityParams.MoveSpeed;
            _propertiesFastAttackDamageSlider.maxValue = _data.BattleParams.FastAttackDamage*2;
            _propertiesFastAttackDamageSlider.value = _data.BattleParams.FastAttackDamage;
            _propertiesStrongAttackDamageSlider.maxValue = _data.BattleParams.StrongAttackDamage*2;
            _propertiesStrongAttackDamageSlider.value = _data.BattleParams.StrongAttackDamage;
            _propertiesCriticalChanceSlider.maxValue = _data.BattleParams.CriticalMultiplier*2;
            _propertiesCriticalChanceSlider.value = _data.BattleParams.CriticalMultiplier;
        }

        void Update()
        {
            if (_propertiesHeltsLabel != null)
            {
                _propertiesHeltsLabel.text = "Здоровье юнита \n" + _data.BaseParams.MaxHealth.ToString();
            }
            if (_propertiesSpeedLabel != null)
            {
                _propertiesSpeedLabel.text = "Скорость передвижения юнита \n" + _data.MobilityParams.MoveSpeed.ToString();
            }
            if (_propertiesFastAttackDamageLabel != null)
            {
                _propertiesFastAttackDamageLabel.text = "Урон быстрой атаки \n" + _data.BattleParams.FastAttackDamage.ToString();
            }
            if (_propertiesStrongAttackDamageLabel != null)
            {
                _propertiesStrongAttackDamageLabel.text = "Урон сильной атаки \n" + _data.BattleParams.StrongAttackDamage.ToString();
            }
            if (_propertiesCriticalChanceLabel != null)
            {
                _propertiesCriticalChanceLabel.text = "Шанс критического урона \n" + _data.BattleParams.CriticalMultiplier.ToString();
            }

        }
        public void OnHeltsSliderValueChanged(float newValue)
        {
            _data.BaseParams.MaxHealth = newValue;
        }
        public void OnSpeedSliderValueChanged(float newValue)
        {
            _data.MobilityParams.MoveSpeed = newValue;
        }
        public void OnFASliderValueChanged(float newValue)
        {
            _data.BattleParams.FastAttackDamage = newValue;

        }
        public void OnSASliderValueChanged(float newValue)
        {
            _data.BattleParams.StrongAttackDamage = newValue;
        }
        public void OnCriticalChanceSliderValueChanged(float newValue)
        {
            _data.BattleParams.CriticalMultiplier = newValue;
        }

        public void OnKillEmAllButtonClick()
        {
            _manager.KillAllBotInFabric();   
        }

        public void OnShowAllHelthButtonClick()
        {
            _manager.ShowAllHelth();
        }


        public void OpenDialog(ZigguratComponent ziggurat, PointerEventData eventData)
        {
            this.gameObject.SetActive(true);

            GetPos(eventData);
            if (!isActivCoroutine)
            {
                StartCoroutine(OpenPanel(_recttransform, _startpos, _endpos, _speedTransform));
            }
        }

        public void CloseDialog(ZigguratComponent ziggurat, PointerEventData eventData)
        {
 

            if (!isActivCoroutine)
            {
                StartCoroutine(ClosePanel(_recttransform, _endpos, _startpos, _speedTransform));
            }

            this.gameObject.SetActive(true);
        }
        private void GetPos(PointerEventData eventData)
        {
            Vector2 pos = eventData.pointerCurrentRaycast.screenPosition;

            if (pos.x <= _recttransformcanvas.rect.width / 2 & pos.y <= _recttransformcanvas.rect.height / 2)
            {
                _startpos = new Vector2(0f, 0f);
                _endpos = new Vector2(_recttransform.rect.width, 0f);
            }
            else if (pos.x <= _recttransformcanvas.rect.width / 2 & pos.y > _recttransformcanvas.rect.height / 2)
            {
                _startpos = new Vector2(0f, _recttransformcanvas.rect.height - _recttransform.rect.height);
                _endpos = new Vector2(_recttransform.rect.width, _recttransformcanvas.rect.height - _recttransform.rect.height);
            }
            else if (pos.x > _recttransformcanvas.rect.width / 2 & pos.y <= _recttransformcanvas.rect.height / 2)
            {
                _startpos = new Vector2(_recttransformcanvas.rect.width + _recttransform.rect.width, 0f);
                _endpos = new Vector2(_recttransformcanvas.rect.width, 0f);
            }
            else
            {
                _startpos = new Vector2(_recttransformcanvas.rect.width + _recttransform.rect.width, _recttransformcanvas.rect.height - _recttransform.rect.height);
                _endpos = new Vector2(_recttransformcanvas.rect.width, _recttransformcanvas.rect.height - _recttransform.rect.height);
            }
        }

        public void OnExitPanel()
        {
            this.gameObject.SetActive(false);
            Debug.Log("Exit Game");
            UnityEditor.EditorApplication.isPaused = true;
        }

        public IEnumerator OpenPanel(RectTransform recttransform, Vector2 startpos, Vector2 endpos, float time)
        {
            isActivCoroutine = true;
            DeActivationUI();
            float currentTime = 0f;
            while (currentTime < time)
            {
                recttransform.anchoredPosition = Vector2.Lerp(startpos, endpos, 1 - (time - currentTime) / time);
                currentTime += Time.deltaTime;
                yield return null;
            }
            yield return null;
            recttransform.anchoredPosition = endpos;
            ActivationUI();
            isActivCoroutine = false;
        }

        public IEnumerator ClosePanel(RectTransform recttransform, Vector2 startpos, Vector2 endpos, float time)
        {
            isActivCoroutine = true;
            DeActivationUI();
            float currentTime = 0f;
            startpos = recttransform.anchoredPosition;
            while (currentTime < time)
            {
                recttransform.anchoredPosition = Vector2.Lerp(startpos, endpos, 1 - (time - currentTime) / time);
                currentTime += Time.deltaTime;
                yield return null;
            }
            yield return null;
            recttransform.anchoredPosition = endpos;
            ActivationUI();
            isActivCoroutine = false;
            recttransform.gameObject.SetActive(false);
        }

        private void DeActivationUI()
        {
            foreach (Selectable element in Selectable.allSelectablesArray)
            {
                element.interactable = false;
            }
        }

        private void ActivationUI()
        {
            foreach (Selectable element in Selectable.allSelectablesArray)
            {
                element.interactable = true;
            }
        }

    }
}
