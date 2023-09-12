using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Zenject;

namespace AI.Units
{
    public abstract class BaseUnit : MonoBehaviour
    {
        protected ActionType _currentActionType;
        private AudioSource _audioSource;
        private Animator _animator;

        protected UnitPropertiesAssistant _propertiesAssistant;
        protected SignalBus _signal;

        protected IReadOnlyDictionary<ActionType, UnitActionData> _actionData;
        protected Rigidbody _rigidBody;

        [SerializeField, Tooltip("Тип юнита")]
        protected UnitType _unit;

        [SerializeField, ReadOnly]
        protected UnitElement[] _elements;

        public bool InAnimation { get; private set; }
        public UnitPropertiesAssistant GetProperties => _propertiesAssistant;

        public float Mass => _rigidBody.mass;



        #region Status

        private LinkedList<StatusDataArgs> _statuses = new LinkedList<StatusDataArgs>();

        public IReadOnlyCollection<StatusDataArgs> GetStatuses => _statuses;

        public StateStatusType AddStatus(StatusDataArgs status)
        {
            var oldStatus = _statuses.FirstOrDefault(t => t.Name == status.Name);

            //Если еще нет аналогичного статуса
            if (oldStatus == null)
            {
                status.CurrentDuraction = status.Duraction;
                _statuses.AddLast(status);
                return StateStatusType.Create;
            }
            //Обновляем статус
            else
            {
                oldStatus.CurrentDuraction = status.Duraction;
                return StateStatusType.Start;
            }
        }

        /// <summary>
        /// Попытка получить статус юнита
        /// </summary>
        /// <param name="name">Имя статуса</param>
        /// <returns>Искомый статус</returns>
        public StatusDataArgs TryToGetStatus(string name)
        {
            return _statuses.FirstOrDefault(t => t.Name == name);
		}

        public void RemoveStatuses(IEnumerable<StatusDataArgs> statuses)
        {
            foreach(var status in statuses) _statuses.Remove(status);
		}

        /// <summary>
        /// Удаляет статус по имени
        /// </summary>
        /// <param name="name">Имя статуса</param>
        public void RemoveStatusByName(string name)
        {
            var status = _statuses.FirstOrDefault(t => t.Name == name);
            _statuses.Remove(status);
		}

        #endregion

        //Метод ключа анимации
        private void OnAnimationEvent_UnityEditor(AnimationEvent data)
        {
            //Проверяем активацию коллайдера
            if (data.stringParameter.Contains("Collider"))
            {
                var element = _elements.First(t => t.GetID == data.intParameter);
                element.ColliderActivity = !element.ColliderActivity;
                element.CurrentActionType = _currentActionType;
            }

            var obj = data.objectReferenceParameter;

            if (obj == null) return;

            if (obj is AudioClip clip)
            {
                _audioSource.PlayOneShot(clip);
            }
            else if (obj is ParticleSystem system)
            {
                system.Play();
            }
        }

        private void OnActionEnd_UnityEditor(AnimationEvent data)
        {
            InAnimation = false;
            var param = data.stringParameter;
            var obj = data.objectReferenceParameter;
            
            //Оповещение о том, что анимация завершена
           
            _signal.FireId("Unit", new ActionResultInfo(param.Contains("IMPACT") ? ActionType.IMPACT : _currentActionType, this, null, ActionResultType.Completed));
            
            if (param.Contains("Die"))
            {
                Debug.Log(this.gameObject.name);
                _signal.FireId("Die", new ActionResultInfo(param.Contains("Die") ? ActionType.Die : _currentActionType, this, null, ActionResultType.Completed));
            }
        }



        #region Обновление состояния машины

        public void SetAnimationState(ActionType newType)
        {
            InAnimation = true;
            _animator.SetTrigger(_actionData[newType].Key);
        }

        public void SetDieAnimation()
        {

            InAnimation = true;
            _animator.enabled = false;
            _animator.enabled = true;
            _animator.SetTrigger(_actionData[ActionType.Die].Key);
            _signal.FireId("Unit", new ActionResultInfo(_currentActionType, this, null, ActionResultType.Interrupted));
        }

        public void SetImpactAnimation()
        {
#if UNITY_EDITOR
            DebugManager.Log($"{name} playing impact animation", typeof(BaseUnit));
#endif
            InAnimation = true;
            _animator.enabled = false;
            _animator.enabled = true;
            _animator.SetTrigger(_actionData[ActionType.IMPACT].Key);
            _signal.FireId("Unit", new ActionResultInfo(_currentActionType, this, null, ActionResultType.Interrupted)); 
        }

        protected void SetAnimationState<KeyType>(ActionType newType, KeyType value) where KeyType : struct
        {
            InAnimation = true;

            if (value is bool b) _animator.SetBool(_actionData[newType].Key, b);
            else if (value is float f) _animator.SetFloat(_actionData[newType].Key, f);
            else if (value is int i) _animator.SetInteger(_actionData[newType].Key, i);
#if UNITY_EDITOR
            else throw new ApplicationException("Попытка установить в качестве ключа машины анимации недоступный тип : " + value.GetType());
#endif
        }

        #endregion

        /// <summary>
        /// Перемещение персонажа
        /// </summary>
        protected virtual void OnMove(Vector3 direction)
        {
            _animator.SetFloat("Forward_Move", direction.z);
            _animator.SetFloat("Right_Move", direction.x);
        }


		protected virtual void Start()
        {
            _actionData = AIUtility.GetKeysForActionTypes(_unit);
        }

        protected virtual void OnValidate()
		{
            _rigidBody = FindComponent<Rigidbody>();
            _animator = FindComponent<Animator>();
            _audioSource = FindComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;

            _elements = GetComponentsInChildren<UnitElement>(true).ToArray();
        }

#if UNITY_EDITOR
        
        private T FindComponent<T>() where T : Component
        {
            var component = GetComponentInChildren<T>(true);

            if (component == null) component = GetComponentInParent<T>();

            if(component == null)
            {
                Debug.Log($"У {name} не обнаружен компонент {typeof(T)}");
			}

            return component;
		}

#endif
	}
}
