using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Zenject;

namespace AI.Units
{
	public class UnitElement : MonoBehaviour
	{
		private BaseUnit _parent;
		private ActionType _action;
		private SignalBus _signal;
		private List<string> _contacts = new List<string>();

		[SerializeField, Tooltip("Идентификатор элемента")]
		private int _id;
		[SerializeField, ReadOnly]
		private Collider _collider;

		public int GetID => _id;

		public bool ColliderActivity 
		{
			get => _collider.enabled;
			set
			{
				_collider.enabled = value;
				if (!value) _contacts.Clear();
			}
		}

		public ActionType CurrentActionType 
		{
			get => _action;
			set
			{
				_action = value;
				//Пока активен защитный коллайдер - персонаж не получает урон
				_parent.GetProperties.Immortal = value == ActionType.Block;
			}
		}

		internal void Construct(BaseUnit unit, SignalBus signal)
		{
			_parent = unit; _signal = signal;
		}

		private void OnValidate()
		{
			_collider = GetComponent<Collider>();

#if UNITY_EDITOR
			
			if(_collider == null)
			{
				Debug.Log("Элемент не содержит коллайдер: " + name);
			}

#endif

			if (_collider == null) return;
			_collider.isTrigger = true;
			_collider.enabled = false;
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
			Gizmos.DrawCube(_collider.bounds.center, _collider.bounds.size);
		}

		private void OnTriggerEnter(Collider other)
		{
			var unit = other.GetComponent<BaseUnit>();

			//Коллизия засчитывается один раз за включение коллайдера
			if (unit == null || _contacts.Contains(unit.name)) return;

			_contacts.Add(unit.name);

			var success = false;

			switch(CurrentActionType)
			{
				case ActionType.FastAttack:
					success = unit.GetProperties.SetDamage(_parent.GetProperties.FastAttackDamage);
					break;
				case ActionType.StrongAttack:
					success = unit.GetProperties.SetDamage(_parent.GetProperties.StrongAttackDamage);
					break;
			}


			//Оповещение о том, что урон нанесен/другое действие успешно
			_signal.FireId("Unit", new ActionResultInfo(_action, _parent, unit, success ? ActionResultType.Successfully : ActionResultType.Failed));
			//todo никогда не будет фейла потому что персонаж не попадает же. Метод не вызывается

			if (unit.GetProperties.Health <= 0)
			{
				//Сброс любой анимации и отыгрывание смерти
				if (success) unit.SetDieAnimation();
			}
			else
			{
				//Сброс любой анимации и отыгрывание импакта
				if (success) unit.SetImpactAnimation();
			}
			
			

#if UNITY_EDITOR

			if (unit.GetProperties.Immortal) Debug.Log($"{unit.name} blocked damage");
			else Debug.Log($"{unit.name} received damage. Current points : {unit.GetProperties.Health}");
#endif
		}
	}
}
