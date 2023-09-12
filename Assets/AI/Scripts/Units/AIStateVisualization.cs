using AI.Managers;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Units
{
    public class AIStateVisualization : MonoBehaviour
    {
#if UNITY_EDITOR
        private Transform _activitySphere;
        private Transform _aggressiveSphere;

        [SerializeField, Space, ReadOnly]
        private bool _activity;
        [SerializeField, ReadOnly]
        private Vector3 _targetPoint;
        [SerializeField, ReadOnly]
        private ActionType _currentActionType;
        [SerializeField, Space, ReadOnly]
        private SteeringBehavioursType _steeringType;
        [SerializeField, ReadOnly, Space]
        private WeightsAI _currentActionWeightsAI;
        [SerializeField, ReadOnly]
        private WeightsAI _deltaActionWeightsAI;


		private void Start()
		{
            //Создаем родительский материал с прозрачностью
            var materialBase = new Material(Shader.Find("Standard"));
            //https://answers.unity.com/questions/1004666/change-material-rendering-mode-in-runtime.html
            materialBase.SetFloat("_Mode", 3);
            materialBase.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            materialBase.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materialBase.SetInt("_ZWrite", 0);
            materialBase.DisableKeyword("_ALPHATEST_ON");
            materialBase.DisableKeyword("_ALPHABLEND_ON");
            materialBase.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            materialBase.renderQueue = 3000;
            materialBase.SetFloat("_Glossiness", 0f);

            System.Func<Color, float, GameObject> create = (color, range) =>
            {
                //Создаем сферу
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphere.GetComponent<SphereCollider>());
                sphere.transform.parent = transform;
                sphere.transform.localPosition = Vector3.zero;
                //Создаем материал
                var material = new Material(materialBase);
                material.color = color;
                
                //Настраиваем материал сферы
                var mesh = sphere.GetComponent<MeshRenderer>();
                mesh.material = material;
                mesh.receiveShadows = false;
                mesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                sphere.transform.localScale = new Vector3(range, range, range);

                return sphere;
            };

            //_activitySphere = create(new Color(0.8f, 0.8f, 0f, 0.15f), 40f * 2).transform;//todo пробросить из ActivityAssistant или где еще будут параметры дистанций
            //_aggressiveSphere = create(new Color(1f, 0.1f, 0f, 0.15f), 15f * 2).transform;

            //Родительский материал больше не нужен
            Destroy(materialBase);
        }

		public void Visualization(ActionType CurrentActionType, bool Activity, SteeringBehavioursType steeringType, WeightEnum<ActionType> CurrentActionWeightsAI, WeightEnum<ActionType> DeltaActionWeightsAI, Vector3 target)
        {
            _currentActionType = CurrentActionType;
            _activity = Activity;
            _steeringType = steeringType;

            _currentActionWeightsAI = new WeightsAI(CurrentActionWeightsAI);
            _deltaActionWeightsAI = new WeightsAI(DeltaActionWeightsAI);

            _targetPoint = target;
        }

        /// <summary>
        /// Веса действий
        /// </summary>
        [System.Serializable]
        public struct WeightsAI
        {
            [Tooltip("Смещение боком")]
            public float Strafe;
            [Tooltip("Прыжок")]
            public float Jump;
            [Tooltip("Перекат")]
            public float Roll;
            [Tooltip("Уклон")]
            public float Evasion;
            [Tooltip("Быстрая атака")]
            public float FastAttack;
            [Tooltip("Сильная атака")]
            public float StrongAttack;
            [Tooltip("Парирование")]
            public float Parry;
            [Tooltip("Дальняя атака")]
            public float RangeAttack;
            [Tooltip("Контратака")]
            public float CounterAttack;
            [Tooltip("Блокирование")]
            public float Block;
            [Tooltip("Атакующая способность")]
            public float AttackAbility;
            [Tooltip("Защитная способность")]
            public float DefenseAbility;
            [Tooltip("Поддерживающая способность")]
            public float SupportAbility;
            [Tooltip("Взаимодействие")]
            public float Interact;


            public WeightsAI(WeightEnum<ActionType> weights)
            {
                weights.TryGetValue(ActionType.Strafe, out Strafe);
                weights.TryGetValue(ActionType.Jump, out Jump);
                weights.TryGetValue(ActionType.Roll, out Roll);
                weights.TryGetValue(ActionType.Evasion, out Evasion);
                weights.TryGetValue(ActionType.FastAttack, out FastAttack);
                weights.TryGetValue(ActionType.StrongAttack, out StrongAttack);
                weights.TryGetValue(ActionType.Parry, out Parry);
                weights.TryGetValue(ActionType.RangeAttack, out RangeAttack);
                weights.TryGetValue(ActionType.CounterAttack, out CounterAttack);
                weights.TryGetValue(ActionType.Block, out Block);
                weights.TryGetValue(ActionType.AttackAbility, out AttackAbility);
                weights.TryGetValue(ActionType.DefenseAbility, out DefenseAbility);
                weights.TryGetValue(ActionType.SupportAbility, out SupportAbility);
                weights.TryGetValue(ActionType.Interact, out Interact);
            }
        }

#endif
    }
}