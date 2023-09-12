using AI.Units;
using System;
using UnityEngine;

namespace AI
{
    /// <summary>
    /// Данные статуса
    /// </summary>
    [Serializable]
    public class StatusDataArgs
    {
        /// <summary>
        /// Название статуса
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Длительность в секундах
        /// </summary>
        public float Duraction { get; }
        /// <summary>
        /// Целевой объект статуса
        /// </summary>
        public BaseUnit Target{ get; }
        /// <summary>
        /// Источник статуса
        /// </summary>
        public BaseUnit Source { get; }
        /// <summary>
        /// Количество циклов
        /// </summary>
        public int Count;
        /// <summary>
        /// Тип статуса
        /// </summary>
        public StatusType Type { get; }

        /// <summary>
        /// Текущее оставшееся время действия статуса
        /// </summary>
        public float CurrentDuraction { get; set; }

        public StatusDataArgs(string name, float duraction, BaseUnit target, BaseUnit source, StatusType type)
        {
            Name = name; Duraction = CurrentDuraction = duraction; Target = target; Source = source; Type = type;
		}
    }

    public struct WeightData
    {
        public ActionType[] PlayerActions;
        public ActionType[] BotActions;
        public ActionResultType[] Results;
        public float Coef;
    }

    /// <summary>
    /// Структура, описывающая результат действия юнита
    /// </summary>
    public readonly struct ActionResultInfo
    {
        public readonly ActionType Type;
        public readonly BaseUnit Source;
        public readonly BaseUnit Target;
        public readonly ActionResultType Result;

        public ActionResultInfo(ActionType type, BaseUnit source, BaseUnit target, ActionResultType result)
        {
            Type = type; Source = source; Target = target; Result = result;
		}

		public override bool Equals(object obj)
		{
            if (!(obj is ActionResultInfo)) return false;

            var str = (ActionResultInfo)obj;

            return str.Type == Type &&
                str.Source.Equals(Source) &&
                str.Target.Equals(Target) &&
                str.Result == Result;
		}
	}

    [Serializable]
    public struct SteeringData
    {
        [Tooltip("Максимальная сила смещения"), Range(0f, 100f)]
        public float MaxVelocity;
        [Tooltip("Максимальная скорость перемещения"), Range(0f, 100f)]
        public float Speed;
        [Tooltip("Радиус прибытия к цели"), Range(0f, 100f)]
        public float DistanceArrival;
        [Tooltip("Радиус отбытия от цели"), Range(0f, 100f)]
        public float DistanceAvoid;
    }

    public readonly struct UnitActionData
    {
        public readonly string Key;
        public readonly Interval<float> Interval;

        public UnitActionData(string key, float minDistance = -1f, float maxDistance = -1f)
        {
            Key = key; Interval = new Interval<float>(minDistance, maxDistance);
        }

        public UnitActionData(string key, Interval<float> interval)
        {
            Key = key; Interval = interval;
        }
    }

    /// <summary>
    /// Точка в пространстве
    /// </summary>
    [Serializable]
    public struct TargetPoint
    {
        private readonly Transform _transform;
        private readonly Vector3 _position;

        /// <summary>
        /// Возвращает целевую точку в пространстве
        /// </summary>
        public Vector3 GetPoint => _transform != null ? _transform.position : _position;

        public TargetPoint(Transform transform)
        {
            _transform = transform;
            _position = Vector3.zero;
		}

        public TargetPoint(Vector3 position)
        {
            _transform = null;
            _position = position;
		}

        /// <summary>
        /// Проверка, является-ли цель заданным объектом
        /// </summary>
        /// <param name="target">Объект</param>
        /// <returns>Равны-ли цель и объект</returns>
        public bool CheckTransformEquals(Transform target)
        {
			return _transform != null && target == _transform;
		}

        /// <summary>
        /// Неопределенное значение структуры по-умолчанию
        /// </summary>
        public static readonly TargetPoint Indefinite = 
            new TargetPoint(new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity));

		public override bool Equals(object obj)
		{
            if (obj is TargetPoint point)
            {
                if (point._position == _position && point._transform == _transform) return true;
            }
            return false;
		}

        public static bool operator ==(TargetPoint a, TargetPoint b)
        {
            return a._position == b._position && a._transform == b._transform;
		}
        public static bool operator !=(TargetPoint a, TargetPoint b)
        {
            return a._position != b._position || a._transform != b._transform;
        }

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

    public struct Interval<T> where T : struct
    {
        public static readonly Interval<float> FloatIndefinite = new Interval<float>(-1f, -1f);
        public static readonly Interval<int> IntIndefinite = new Interval<int>(-1, -1);

        public T Min;
        public T Max;

        public Interval(T min, T max)
        {
            Min = min; Max = max;
		}



		public static bool operator ==(Interval<T> a, Interval<T> b)
        {
            return a.Min.Equals(b.Min) && a.Max.Equals(b.Max);
		}

        public static bool operator !=(Interval<T> a, Interval<T> b)
        {
            return !a.Min.Equals(b.Min) || !a.Max.Equals(b.Max);
        }

		public override bool Equals(object obj)
		{
            var type = obj.GetType();

            //Получаем информацию о полях
            var minField = type.GetField("Min");
            if (minField == null) return false;
            var maxField = type.GetField("Max");
            if (maxField == null) return false;

            //Получаем из сущности значение ее полей
            var value1 = minField.GetValue(obj);
            var value2 = maxField.GetValue(obj);
            //Если : тип наших полей не соответствует типу полей сущности или типы полей сущностей не одинаковы - она нам не идентична
            if (value1.GetType() != typeof(T) || value1.GetType() == value2.GetType()) return false;
            //
            return value1.Equals(Min) && value2.Equals(Max);

            //Не проходят: классы, не шаблоны и примитивы
            //if (!type.IsValueType || !type.IsGenericType || type.IsPrimitive) return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return $"Min: {Min} Max: {Max}";
        }
	}
}