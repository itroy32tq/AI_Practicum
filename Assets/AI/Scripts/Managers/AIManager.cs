using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using AI.Assistants;
using Zenject;
using AI.Units;
using AI.Configurations;
using UnityEngine.EventSystems;

namespace AI.Managers
{
    public class AIManager : MonoBehaviour
    {
        //Ассистент, обновляющий поведение бота
        private StateAIAssistant _stateAssist;
        //Ассистент, проверяющий активность ботов
        private ActivityAssistant _activityAssistant;
        //Ассистент, рассчитывающий передвижение ботов
        private MoveAssistant _moveAssistant;

        //Пул для ботов
        private Transform _enemiesPool;

        [SerializeField, Tooltip("префаб бота для фабрики")]
        private GameObject _pref;

        [Inject]
        private LinkedList<BotComponent> _enemies;
        [Inject]
        private Dictionary<UnitType, StatsData> _params;
        [Inject]
        private SignalBus _actionSignal;

        [SerializeField, Tooltip("Конфигурация настроек менеджера и его ассистентов")]
        private AIManagerConfiguration _configs;

        [Inject]
        private AIConfiguration[] configs;

        [SerializeField, Tooltip("одновременное количество ботов на сцене")]
        private int _countBot;



        private float time = 0f;

        private void Start()
		{
            foreach (var enemy in _enemiesPool.GetComponentsInChildren<BotComponent>(true))
            {
                _enemies.AddLast(enemy);
                _stateAssist.ClearStateAI(enemy);
                enemy.Construct(_actionSignal, _params);
            }
 
            _moveAssistant.SetParams(_stateAssist, _configs.GetDistanceActivation, _configs.GetRangeWander, _configs.GetWanderStatusDuration, _configs.GetMinVelocity, _configs.GetSqrArrivalDistance);
            _activityAssistant.Execute(_configs.GetActivityCheckDelay, _configs.GetDistanceActivation, _configs.GetTargetFocusDistance);
            _stateAssist.SetConfiguration(_configs.GetActionResultWeights);
        }

		void Update()
        {
            time -= Time.deltaTime;

            if (time < 0f & _enemies.Count <= _countBot)
            {
                time = 10;
                CreateNewEnemy(_pref);

            }

            _moveAssistant.OnMoves();

            OnStatusUpdate();

#if UNITY_EDITOR
            UpdateEditorInfo();
            #endif
        }
        public void CreateNewEnemy(GameObject prefab)
        {
            
            GameObject bot = Instantiate(prefab, _enemiesPool);
            bot.transform.position = _enemiesPool.position;
            bot.transform.eulerAngles = _enemiesPool.eulerAngles;
            BotComponent new_bot = bot.GetComponent<BotComponent>();

            //костыль по причине того, что при создании из префаба отваливается rigidbady
            new_bot.Reboot_unit();

            _enemies.AddLast(new_bot);

            //Инъекция сущностей в обект
            new_bot.Construct(_actionSignal, _params);
            new_bot.Construct(configs);
            Debug.Log(new_bot.GetProperties.Health);
        }

        public void KillAllBotInFabric()
        {
            foreach (var bot in _enemies)
            {
                bot.SetDieAnimation();
            }

        }

        public void ShowAllHelth()
        {
            foreach (var bot in _enemies)
            {
                if (bot._helthBarScrypt.gameObject.activeInHierarchy) bot._helthBarScrypt.gameObject.SetActive(false);
                else bot._helthBarScrypt.gameObject.SetActive(true);
            }
        }

        public void RemoveEnemies(IEnumerable<BotComponent> targets)
        {
            foreach (var enemy in targets)
            {
                _enemies.Remove(enemy);
                Destroy(enemy.gameObject);//todo сделать карутину
            }
                
		}

        //Обновление состояния статусов ботов
        private void OnStatusUpdate()
        {
            foreach (var bot in _enemies)
            {
                var removeStatuses = new LinkedList<StatusDataArgs>();

                foreach (var status in bot.GetStatuses)
                {
                    //Перманентные статусы не трогаются
                    if (status.Type == StatusType.Persistance) continue;

                    status.CurrentDuraction -= Time.deltaTime;
                    //Если : действие статуса закончено
                    if (status.CurrentDuraction > 0f) continue;

                    //Убираем закончившиеся статусы
                    switch (status.Type)
                    {
                        //Одиночный просто удаляется
                        case StatusType.Single:
                            removeStatuses.AddLast(status);
                            break;
                    }
                }
                //Сообщаем боту о том, какие статусы нужно удалить
                bot.RemoveStatuses(removeStatuses);
            }
        }

        [Inject]
        private void AssistantsConstruct(StateAIAssistant stateAIAssistant, ActivityAssistant activityAssistant, MoveAssistant moveAssistant)
        {
            _stateAssist = stateAIAssistant; _activityAssistant = activityAssistant; _moveAssistant = moveAssistant;
        }

        #region Editor
#if UNITY_EDITOR

        [Space, Header("---Debug data---"), ReadOnly, SerializeField]
        private int _allBotsCount;

        [ReadOnly, SerializeField]
        private int _activityBotsCount;

        private void UpdateEditorInfo()
        {
            _allBotsCount = _enemies.Count;
            _activityBotsCount = _enemies.Count(t => t.Activity);
        }

        private void OnValidate()
        {
            _enemiesPool = this.GetComponentInChildren<Transform>().Find("EnemiesPool");
        }

#endif
        #endregion
    }
}