using System.Collections.Generic;
using UnityEngine;

namespace AI.Configurations
{
	[CreateAssetMenu(fileName = "NewAIManagerConfiguration", menuName = "Configurations/AI Manager Configuration")]
	public class AIManagerConfiguration : ScriptableObject
	{
    	[SerializeField, Tooltip("Расстояние до игрока для активации логики бота")]
        private float _distanceActivation = 40f;
        [SerializeField, Tooltip("Интервал проверки активности ботов")]
        private float _activityCheckDelay = 2f;
        [SerializeField, Tooltip("Расстояние триггера бота на игрока")]
        private float _targetFocusDistance = 15f;

        [SerializeField, Space, Tooltip("Расстояние выбора точки при блуждании")]
        private float _rangeWander = 10f;
        [SerializeField, Tooltip("Время действия статуса блуждания")]
        private float _wanderStatusDuration = 7f;
        [SerializeField, Tooltip("Минимальная сила стремления, ниже которой бот останавливается")]
        private float _minVelocity = 0.2f;
        [SerializeField, Tooltip("Дистанция прибытия к точки стремления")]
        private float _sqrArrivalDistance = 3f;
        [SerializeField, Space, Tooltip("Коэффициенты окончания действий")]
        private ActionResultWeightDictionary _actionResultWeights;

        /// <summary>
        /// Возвращает Расстояние до игрока для активации логики бота
        /// </summary>
        public float GetDistanceActivation => _distanceActivation;
        /// <summary>
        /// Возвращает Интервал проверки активности ботов
        /// </summary>
        public float GetActivityCheckDelay => _activityCheckDelay;
        /// <summary>
        /// Возвращает Расстояние триггера бота на игрока
        /// </summary>
        public float GetTargetFocusDistance => _targetFocusDistance;
        /// <summary>
        /// Расстояние выбора точки при блуждании
        /// </summary>
        public float GetRangeWander => _rangeWander;
        /// <summary>
        /// Время действия статуса блуждания
        /// </summary>
        public float GetWanderStatusDuration => _wanderStatusDuration;
        /// <summary>
        /// Минимальная сила стремления, ниже которой бот останавливается
        /// </summary>
        public float GetMinVelocity => _minVelocity;
        /// <summary>
        /// Дистанция прибытия к точки стремления
        /// </summary>
        public float GetSqrArrivalDistance => _sqrArrivalDistance;

        /// <summary>
        /// Возвращает словарь коэффициентов изменения весов
        /// </summary>
        public IReadOnlyDictionary<ActionResultType, float> GetActionResultWeights => _actionResultWeights.Clone();
    }
}