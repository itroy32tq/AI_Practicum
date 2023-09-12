using System;
using System.Linq;
using System.Collections.Generic;
using Zenject;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace AI.Units
{
    public class BotComponent : BaseUnit, IPointerClickHandler
    {
        private SteeringData _steeringData;

        [SerializeField, Tooltip("ui скрипт")]
        public HelthBarScrypt _helthBarScrypt;

        [SerializeField, Tooltip("Принадлежность к фабрике")]
        private FabricType _fabricType;

        [SerializeField, Tooltip("Паттерн поведения бота"), ReadOnly(WriteInEditor: true)]
        private AIPattern _pattern = AIPattern.Simple;

        /// <summary>
        /// принадлежность бота к фабрике
        /// </summary>
        public FabricType GetFabric { get => _fabricType; }

        /// <summary>
        /// Текущее действие бота
        /// </summary>
        public ActionType CurrentActionType
        {
            get => _currentActionType;
            set
            {
                var oldState = _currentActionType;
                _currentActionType = value;

#if UNITY_EDITOR
                DebugManager.Log($"{name} start action : {value}", typeof(BaseUnit));
#endif

                if (value != ActionType.MOVE && value != ActionType.IDLE)
                {
                    SetAnimationState(value);
                    transform.LookAt(Target.GetPoint);//todo тоже резкий поворот надо исправить
                }
            }
        }

        /// <summary>
        /// Текущее состояние весов действий
        /// </summary>
        public WeightEnum<ActionType> CurrentActionWeightsAI { get; private set; }
        /// <summary>
        /// Изменения весов действий
        /// </summary>
        public WeightEnum<ActionType> DeltaActionWeightsAI { get; private set; }

        /// <summary>
        /// Активен-ли бот
        /// </summary>
        public bool Activity { get; set; }
        /// <summary>
        /// Преследует-ли бот кого-либо
        /// </summary>
        public bool Hunt { get; set; }
        /// <summary>
        /// Целевая точка в пространстве
        /// </summary>
        public TargetPoint Target {get; set; }
        /// <summary>
        /// Тип перемещения
        /// </summary>
        public SteeringBehavioursType SteeringType { get; set; } = SteeringBehavioursType.Wander;

        /// <summary>
        /// Параметры перемещения
        /// </summary>
        public ref SteeringData GetSteeringData => ref _steeringData;


        /// <summary>
        /// Конфигурирование бота
        /// </summary>
        /// <param name="environments">Данные для окружения</param>
        [Inject]
        public void Construct(Configurations.AIConfiguration[] configs)
        {
            var config = configs.FirstOrDefault(t => t.GetPatternType == _pattern);//todo после старта не обработает без фабрики
            _steeringData = config.GetSteeringData;
#if UNITY_EDITOR

            if (config == null)
            {
                Debug.LogError("Бот не получил конфигурацию своего паттерна : " + _pattern);
                EditorApplication.isPaused = true;
			}
#endif
            CurrentActionWeightsAI = new WeightEnum<ActionType>(config.GetActionWeights);
            DeltaActionWeightsAI = new WeightEnum<ActionType>(config.GetActionWeights);
            
        }

        /// <summary>
        /// Возвращает интервал необходимого расстояния для указанного типа действия
        /// </summary>
        /// <param name="type">Тип действия</param>
        /// <returns>Требуемый интервал расстояния</returns>
        public Interval<float> GetExpectedInterval(ActionType type) => _actionData[type].Interval;


        /// <summary>
        /// Конструирование юнита
        /// </summary>
        /// <param name="signal">Сигнал</param>
        public void Construct(SignalBus signal, Dictionary<UnitType, StatsData> @params)//todo
        {
            _signal = signal;
            _propertiesAssistant = new UnitPropertiesAssistant(@params[_unit]);


            foreach (var element in _elements) element.Construct(this, signal);
        }

        protected override void Start()
        {
            base.Start();

#if UNITY_EDITOR
            _visual = gameObject.AddComponent<AIStateVisualization>();
#endif
        }

        public void Reboot_unit()
        {
            this.Start();
        }


        public Vector3 GetVelocity(IgnoreAxisType ignore = IgnoreAxisType.Y)
        {
            if (_rigidBody == null) base.OnValidate();
            return IgnoreAxisUpdate(ignore, _rigidBody.velocity);
        }

        public void SetVelocity(Vector3 velocity, IgnoreAxisType ignore = IgnoreAxisType.None)
        {
            OnMove(velocity);

            //todo пока что персонаж резко поворачивается в сторону движения
            transform.LookAt(Target.GetPoint);

            _rigidBody.velocity = IgnoreAxisUpdate(ignore, velocity);
        }

        private Vector3 IgnoreAxisUpdate(IgnoreAxisType ignore, Vector3 velocity)
        {
            if (ignore == IgnoreAxisType.None) return velocity;
            if ((ignore & IgnoreAxisType.X) == IgnoreAxisType.X) velocity.x = 0f;
            if ((ignore & IgnoreAxisType.Y) == IgnoreAxisType.Y) velocity.y = 0f;
            if ((ignore & IgnoreAxisType.Z) == IgnoreAxisType.Z) velocity.z = 0f;

            return velocity;
        }
        private AIStateVisualization _visual;

        private void Update()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_helthBarScrypt.gameObject.activeInHierarchy) _helthBarScrypt.gameObject.SetActive(false);
            else _helthBarScrypt.gameObject.SetActive(true);
        }
    }
}
