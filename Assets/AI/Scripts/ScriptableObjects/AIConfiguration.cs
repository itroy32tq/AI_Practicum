using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Configurations
{
    [CreateAssetMenu(fileName = "NewAIConfiguration", menuName = "Configurations/AI Config", order = 1)]
    public class AIConfiguration : ScriptableObject
    {
        [SerializeField, Tooltip("Паттерн поведения")]
        private AIPattern _pattern;
        [SerializeField, Tooltip("Веса действий"), Space]
        private ActionTypeWeightDictionary _actionWeights;
        [SerializeField, Tooltip("Параметры движения")]
        private SteeringData _steering;

        public AIPattern GetPatternType => _pattern;
        public IReadOnlyDictionary<ActionType, float> GetActionWeights => _actionWeights.Clone();
        public SteeringData GetSteeringData => _steering;
    }
}
