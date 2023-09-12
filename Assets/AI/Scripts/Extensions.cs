using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AI
{
    public static class AIUtility
    {
        private static readonly string c_ConfigPath = "//AI//Resources//Config.xml";
        private static readonly string c_WeightPath = "//AI//Resources//Weights.xml";

        private static Dictionary<DistanceType, List<ActionType>> _distance = new Dictionary<DistanceType, List<ActionType>>();
        private static Dictionary<UnitType, Dictionary<ActionType, UnitActionData>> _animationKeys = new Dictionary<UnitType, Dictionary<ActionType, UnitActionData>>();
        private static List<WeightData> _playerCoefs = new List<WeightData>();

        //Запускается автоматически
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Configuration()
        {
            var id = 0;
            try
            {
                var root = XDocument.Load(Application.dataPath + c_ConfigPath).Root;
                id = 1;
                ConfigurationPlayerKeys(root);
                id = 2;
                root = XDocument.Load(Application.dataPath + c_WeightPath).Root;
                ConfigurationPlayerActions(root);
                id = 3;
            }
            //Обработка исключения
            catch (Exception e)
            {
                Debug.LogError($"Конфигурационный данные содержат ошибку. Парсинг остановился с идентификатором :{id}");

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
                throw e;
            }
        }

        private static void ConfigurationPlayerKeys(XElement root)
        {
            //Проходка по всем условиям машины анимации
            foreach (var element in root.Element("Units").Elements("Unit"))
            {
                var name = (UnitType)Enum.Parse(typeof(UnitType), element.Attribute("Name").Value);
                var actionsDic = new Dictionary<ActionType, UnitActionData>();
                foreach(var pairs in element.Elements("UnitAction"))
                {
                    var action = (ActionType)Enum.Parse(typeof(ActionType), pairs.Attribute("Action").Value);
                    var key = pairs.Attribute("Key").Value;

                    var interval = pairs.Attribute("Min");
                    if (interval != null)
                    {
                        actionsDic.Add(
                            action, 
                            new UnitActionData(
                                key, 
                                float.Parse(interval.Value), 
                                float.Parse(pairs.Attribute("Max").Value)
                        ));
                    }
                    else actionsDic.Add(action, new UnitActionData(key));
                }

                _animationKeys.Add(name, actionsDic);
            }
        }
        private static void ConfigurationPlayerActions(XElement root)
        {
            //Проходка по группам действий
            foreach (var element in root.Element("Player").Elements("Weight"))
            {

                //Получение значения перечисления для игрока
                var playerActionsStr = element.Attribute("Actions").Value.Split(' ');
                var botActionsStr = element.Attribute("Params").Value.Split(' ');
                var resultsStr = element.Attribute("Results").Value.Split(' ');
                var coef = float.Parse(element.Attribute("Coef").Value);

                var playerActions = new ActionType[playerActionsStr.Length];
                var botActions = new ActionType[botActionsStr.Length];
                var results = new ActionResultType[resultsStr.Length];

                for (var i = 0; i < playerActionsStr.Length; i++) playerActions[i] = (ActionType)Enum.Parse(typeof(ActionType), playerActionsStr[i]);
                for (var i = 0; i < botActionsStr.Length; i++) botActions[i] = (ActionType)Enum.Parse(typeof(ActionType), botActionsStr[i]);
                for (var i = 0; i < resultsStr.Length; i++) results[i] = (ActionResultType)Enum.Parse(typeof(ActionResultType), resultsStr[i]);

                _playerCoefs.Add(new WeightData { PlayerActions = playerActions, BotActions = botActions, Results = results, Coef = coef });
            }
        }


        /// <summary>
        /// Возвращает словарь анимационных ключей, связанных с действиями
        /// </summary>
        /// <param name="unitName">Имя юнита</param>
        /// <returns>Словарь ключей</returns>
        public static IReadOnlyDictionary<ActionType, UnitActionData> GetKeysForActionTypes(UnitType unit) => _animationKeys[unit];
        /// <summary>
        /// Возвращает коллекцию для чтения с коэффициентами изменения приоритетов действий бота по действиям игрока
        /// </summary>
        public static IReadOnlyList<WeightData> GetPlayerActionCoefs => _playerCoefs;
    }
    [Serializable]
    public class ActionTypeWeightDictionary : SerializableDictionaryBase<ActionType, float> { }
    [Serializable]
    public class ActionResultWeightDictionary : SerializableDictionaryBase<ActionResultType, float> { }

    public delegate void ActionEventHandler(ActionType newType, bool isLoop);

    public class WeightEnum<T> where T : struct
    {
        //Внутренние сущности класса
        private IReadOnlyDictionary<T, int> _keys;
        private float[] _values;

        //Индексатор, позволяет обращаться к весам по значению перечисления
        public float this[T key]
        {
            get
            {
                return _values[_keys[key]];
            }
            set
            {
                _values[_keys[key]] = value;
            }
        }

        public int Count => _keys.Count;

        public IEnumerable<T> Keys => _keys.Keys;

        public WeightEnum()
        {
            var array = Enum.GetValues(typeof(T));

            var keys = new Dictionary<T, int>(array.Length);
            _values = new float[array.Length];

            int i = 0;
            foreach (var en in array)
            {
                keys.Add((T)en, i);
                i++;
            }

            _keys = keys;
        }

        public WeightEnum(IReadOnlyDictionary<T, float> weights)
        {
            var keys = new Dictionary<T, int>(weights.Count);
            _values = new float[weights.Count];

            int i = 0;
            foreach (var pair in weights)
            {
                keys.Add(pair.Key, i);
                _values[i] = pair.Value;
                i++;
            }

            _keys = keys;
        }

        //Перечислитель, с его помощью можно пройти по классу через foreach
        public IEnumerator<KeyAndWeightPair<T, float>> GetEnumerator()
        {
            return new GroupEnumerator(_keys, _values);
        }

        /// <summary>
        /// Попытка получить значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение или default значение</param>
        /// <returns>Успешно-ли получено значение по ключу</returns>
        public bool TryGetValue(T key, out float value)
        {
            var res = _keys.TryGetValue(key, out var i);
            value = res ? _values[i] : default(float);
            return res;
		}

        /// <summary>
        /// Проверяет, содержится-ли данный ключ в сущности
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Истина, если ключ найден</returns>
        public bool ContainsKey(T key)
        {
            return _keys.ContainsKey(key);
		}

        public List<KeyAndWeightPair<T, float>> ToList()
        {
            var list = new List<KeyAndWeightPair<T, float>>(_keys.Count);

            foreach(var el in this)
            {
                list.Add(el);
			}
            return list;
		}
		//Пара ключ и вес, по аналогии со словарной парой
		public struct KeyAndWeightPair<T1, T2> where T1 : struct where T2 : struct
        {
            public T1 Key;
            public T2 Weight;

            public KeyAndWeightPair(T1 key, T2 weight)
            {
                Key = key; Weight = weight;
            }

            //Для удобства вывода текстовой версии
            public override string ToString()
            {
                return Key + " : " + Weight;
            }

            //Для избегания рефлексии, если потребуется сравнивать пары
            public override bool Equals(object obj)
            {
                if (!(obj is KeyAndWeightPair<T1, T2>)) return false;

                var pair = (KeyAndWeightPair<T1, T2>)obj;
                return Key.Equals(pair.Key) && Weight.Equals(pair.Weight);
            }
        }

        //Внутренний класс, определяющий логику перемещения в foreach
        private class GroupEnumerator : IEnumerator<KeyAndWeightPair<T, float>>
        {
            //Индекс итерации по классу и удобное представление структуры
            private List<KeyAndWeightPair<T, float>> _list;
            private int _current = -1;

            public GroupEnumerator(IReadOnlyDictionary<T, int> keys, float[] values)
            {
                _list = new List<KeyAndWeightPair<T, float>>(keys.Count);

                foreach (var el in keys)
                {
                    _list.Add(new KeyAndWeightPair<T, float>(el.Key, values[el.Value]));
                }
            }

            //Возвращает значение из текущей итерации
            public KeyAndWeightPair<T, float> Current
            {
                get
                {
                    return _list[_current];
                }
            }

            //Возвращает значение из текущей итерации, если foreach работает с обобщеной коллекцией
            object IEnumerator.Current => Current;

            //Выделение памяти в данном классе отсутствует, очистка не требуется
            public void Dispose() { }

            //Смещает итератор на следующий элемент
            //Если успешно - итерирование продолжается
            public bool MoveNext()
            {
                if (_current < _list.Count - 1)
                {
                    _current++;
                    return true;

                    List<int> r = null;
                }

                return false;
            }

            //Сброс итерирования на начальное значение
            public void Reset()
            {
                _current = -1;
            }
        }

    }

    public static class Extension//show
    {
        public static Interval<float> Pow(this Interval<float> interval, float value = 2)
        {
            return new Interval<float>(Mathf.Pow(interval.Min, value), Mathf.Pow(interval.Max, value));
        }
        public static Interval<int> Pow(this Interval<int> interval, float value = 2)
        {
            return new Interval<int>(Mathf.RoundToInt(Mathf.Pow(interval.Min, value)), Mathf.RoundToInt(Mathf.Pow(interval.Max, value)));
        }

        /// <summary>
        /// Конвертация двумерного вектора в вектор трехмерного перемещения
        /// </summary>
        /// <param name="vector">Двумерный вектор</param>
        /// <returns>Трехмерных вектор</returns>
        public static Vector3 ConvertToMoveVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, 0f, vector.y);
		}
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute 
    {
        public bool WriteInEditor { get; }

        public ReadOnlyAttribute(bool WriteInEditor = false)
        {
            this.WriteInEditor = WriteInEditor;
		}
    }

    //public class ReadOnlyInRuntimeAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var atr = (ReadOnlyAttribute)attribute;

            GUI.enabled = atr.WriteInEditor && !EditorApplication.isPlaying;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    /*
    [CustomPropertyDrawer(typeof(ReadOnlyInRuntimeAttribute))]
    public class ReadInRuntimeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = !EditorApplication.isPlaying;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }*/
#endif
}
