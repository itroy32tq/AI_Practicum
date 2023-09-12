using AI.Assistants;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace AI
{
	public class DebugManager : MonoBehaviour
	{
#if UNITY_EDITOR
		private static DebugManager _self;

		private static Dictionary<Type, FieldInfo> _fields = new Dictionary<Type, FieldInfo>();

		[SerializeField, DebugType(typeof(ActivityAssistant))]
		private bool _activityAssistant = true;
		[SerializeField, DebugType(typeof(MoveAssistant))]
		private bool _moveAssistant = true;
		[SerializeField, DebugType(typeof(StateAIAssistant))]
		private bool _stateAssistant = true;
		[SerializeField, DebugType(typeof(Units.BaseUnit))]
		private bool _baseUnits = true;

		public static void Log(string text, Type sourceType)
		{
			if (!_fields.TryGetValue(sourceType, out var field)) return;

			if ((bool)field.GetValue(_self)) Debug.Log(text);
		}

		private void Start()
		{
			if(_self != null)
			{
				Destroy(this);
				return;
			}
			_self = this;

			var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

			foreach(var field in fields)
			{
				var atr = field.GetCustomAttribute<DebugTypeAttribute>();

				if (atr == null) continue;

				_fields.Add(atr.Type, field);
			}
		}


		private class DebugTypeAttribute : Attribute
		{
			public Type Type;

			public DebugTypeAttribute(Type type)
			{
				Type = type;
			}
		}
#endif
	}
}
