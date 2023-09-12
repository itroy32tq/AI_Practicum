using OneLine;
using System;
using UnityEngine;

namespace AI
{
    /// <summary>
    /// Класс, описывающий параметры сущностей
    /// </summary>
    [Serializable]
    public class StatsData
    {
        public BaseParamsData BaseParams = BaseParamsData.Empty;
        public MobilityParamsData MobilityParams = MobilityParamsData.Empty;
        public BattleParamsData BattleParams = BattleParamsData.Empty;
        public ProbabilityParamsData ProbabilityParams = ProbabilityParamsData.Empty;

        public static StatsData operator +(StatsData a, StatsData b)
        {
            var sum = new StatsData();

            var baseParams = a.BaseParams;
            baseParams.MaxHealth += b.BaseParams.MaxHealth;
            baseParams.MaxMana += b.BaseParams.MaxMana;
            baseParams.HPRegenPerSec += b.BaseParams.HPRegenPerSec;
            baseParams.MPRegenPerSec += b.BaseParams.MPRegenPerSec;
            sum.BaseParams = baseParams;

            var mobilityParams = a.MobilityParams;
            mobilityParams.MoveSpeed = Mathf.Clamp(mobilityParams.MoveSpeed + b.MobilityParams.MoveSpeed, 0f, 100f);
            mobilityParams.JumpForce = Mathf.Clamp(mobilityParams.JumpForce + b.MobilityParams.JumpForce, 0f, 100f);
            sum.MobilityParams = mobilityParams;

            var battleParams = a.BattleParams;
            battleParams.Armor += b.BattleParams.Armor;
            battleParams.CriticalMultiplier = Mathf.Clamp(battleParams.CriticalMultiplier + b.BattleParams.CriticalMultiplier, 0f, 0.95f);
            battleParams.FastAttackDamage += b.BattleParams.FastAttackDamage;
            //battleParams.FastAttackSpeed = Mathf.Clamp(battleParams.FastAttackSpeed - b.BattleParams.FastAttackSpeed, 0.1f, 5f);
            battleParams.StrongAttackDamage += b.BattleParams.StrongAttackDamage;
            //battleParams.StrongAttackSpeed = Mathf.Clamp(battleParams.StrongAttackSpeed - b.BattleParams.StrongAttackSpeed, 0.1f, 15f);
            sum.BattleParams = battleParams;

            var probabilityParamsData = a.ProbabilityParams;
            probabilityParamsData.CriticalChance = Mathf.Clamp(probabilityParamsData.CriticalChance + b.ProbabilityParams.CriticalChance, 0f, 1f);
            probabilityParamsData.EvadeChance = Mathf.Clamp(probabilityParamsData.EvadeChance + b.ProbabilityParams.EvadeChance, 0f, 1f);
            probabilityParamsData.MissChance = Mathf.Clamp(probabilityParamsData.MissChance - b.ProbabilityParams.MissChance, 0f, 1f);
            sum.ProbabilityParams = probabilityParamsData;

            return sum;
        }
    }

    /// <summary>
    /// Класс, описывающий параметры игрока
    /// </summary>
    [Serializable]
    public class PlayerStatsData : StatsData
    {
        /// <summary>
        /// Роль персонажа
        /// </summary>
        [Tooltip("Роль персонажа"), OneLine, Highlight(0.05f, 0.72f, 0.62f)]
        public RoleType Type;
        /// <summary>
        /// Базовые параметры персонажа
        /// </summary>
        [OneLineWithHeader, HideLabel, Highlight(0, 0, 1), Separator("[ Базовые параметры персонажа ]")]
        public BaseStatsData BaseStats = BaseStatsData.Empty;
        /// <summary>
        /// Диалоговые таланты
        /// </summary>
        [OneLineWithHeader, HideLabel, Highlight(0.72f, 0.65f, 0.05f), Separator("[ Диалоговые таланты ]")]
        public DialogTalentsData DialogTalents = DialogTalentsData.Empty;

        public static PlayerStatsData operator +(PlayerStatsData a, PlayerStatsData b)
        {
            var sum = (PlayerStatsData)(a + (StatsData)b);

            var dialogTalents = a.DialogTalents;
            dialogTalents.Bluff += a.DialogTalents.Bluff;
            dialogTalents.Commerce += a.DialogTalents.Commerce;
            dialogTalents.Fortunate += a.DialogTalents.Fortunate;
            dialogTalents.Sympathy += a.DialogTalents.Sympathy;
            dialogTalents.Terror += a.DialogTalents.Terror;
            sum.DialogTalents = dialogTalents;

            var baseStats = a.BaseStats;
            baseStats.Accuracy += a.BaseStats.Accuracy;
            baseStats.Agility += a.BaseStats.Agility;
            baseStats.Intelligence += a.BaseStats.Intelligence;
            baseStats.Strength += a.BaseStats.Strength;
            sum.DialogTalents = dialogTalents;

            return sum;
        }
    }

    [Serializable]
    public struct BaseParamsData
    {
        /// <summary>
        /// Здоровье, сколько урона выдержит персонаж
        /// </summary>
        [Tooltip("Здоровье, сколько урона выдержит персонаж")]
        public float MaxHealth;
        /// <summary>
        /// Мана, сколько особых способностей персонаж может выполнить
        /// </summary>
        [Tooltip("Мана, сколько особых способностей персонаж может выполнить")]
        public float MaxMana;
        /// <summary>
        /// Скорость восстановления здоровья в секунду
        /// </summary>
        [Tooltip("Скорость восстановления здоровья в секунду")]
        public float HPRegenPerSec;
        /// <summary>
        /// Скорость восстановления маны в секунду
        /// </summary>
        [Tooltip("Скорость восстановления маны в секунду")]
        public float MPRegenPerSec;

        public static BaseParamsData Empty => new BaseParamsData()
        {
            MaxHealth = 3f,
            MaxMana = 0f,
            MPRegenPerSec = 0f,
            HPRegenPerSec = 0f
        };

		public override bool Equals(object obj)
		{
            if (!(obj is BaseParamsData)) return false;

            var data = (BaseParamsData)obj;

            return data.MaxHealth == MaxHealth && 
                data.MaxMana == MaxMana && 
                data.HPRegenPerSec == HPRegenPerSec && 
                data.MPRegenPerSec == MPRegenPerSec;
		}
	}

    /// <summary>
    /// Параметры мобильности юнитов
    /// </summary>
    [Serializable]
    public struct MobilityParamsData
    {
        /// <summary>
        /// Скорость перемещения
        /// </summary>
        [Tooltip("Скорость перемещения")]
        public float MoveSpeed;
        /// <summary>
        /// Скорость поворота
        /// </summary>
        [Tooltip("Скорость поворота")]
        public float RotateSpeed;
        /// <summary>
        /// Сила прыжка
        /// </summary>
        [Tooltip("Сила прыжка")]
        public float JumpForce;

        public static MobilityParamsData Empty => new MobilityParamsData()
        {
            MoveSpeed = 1f,
            RotateSpeed = 1f,
            JumpForce = 5f
        };

        public override bool Equals(object obj)
        {
            if (!(obj is MobilityParamsData)) return false;

            var data = (MobilityParamsData)obj;

            return data.MoveSpeed == MoveSpeed && 
                data.JumpForce == JumpForce &&
                data.RotateSpeed == RotateSpeed;
        }
    }

    /// <summary>
    /// Боевые параметры юнитов
    /// </summary>
    [Serializable]
    public struct BattleParamsData
    {
        /// <summary>
        /// Сколько урона будет поглощено
        /// </summary>
        [Tooltip("Сколько урона будет поглощено")]
        public float Armor;
        /// <summary>
        /// Урон при быстрых атаках
        /// </summary>
        [Tooltip("Урон при быстрых атаках")]
        public float FastAttackDamage;
        /// <summary>
        /// Урон при сильных атаках
        /// </summary>
        [Tooltip("Урон при сильных атаках")]
        public float StrongAttackDamage;
        /// <summary>
        /// Множитель критического урона
        /// </summary>
        [Tooltip("Множитель критического урона")]
        public float CriticalMultiplier;

        public static BattleParamsData Empty => new BattleParamsData()
        {
            Armor = 0f,
            FastAttackDamage = 5f,
            StrongAttackDamage = 10f,
            CriticalMultiplier = 1f
        };

        public override bool Equals(object obj)
        {
            if (!(obj is BattleParamsData)) return false;

            var data = (BattleParamsData)obj;

            return data.Armor == Armor && 
                data.FastAttackDamage == FastAttackDamage && 
                data.StrongAttackDamage == StrongAttackDamage && 
                data.CriticalMultiplier == CriticalMultiplier;
        }
    }

    /// <summary>
    /// Вероятности событий у юнитов
    /// </summary>
    [Serializable]
    public struct ProbabilityParamsData
    {
        /// <summary>
        /// Шанс критической атаки
        /// </summary>
        [Tooltip("Шанс критической атаки")]
        public float CriticalChance;
        /// <summary>
        /// Шанс уворота
        /// </summary>
        [Tooltip("Шанс уворота")]
        public float EvadeChance;
        /// <summary>
        /// Шанс промаха
        /// </summary>
        [Tooltip("Шанс промаха")]
        public float MissChance;

        public static ProbabilityParamsData Empty => new ProbabilityParamsData()
        {
            CriticalChance = 0.05f,
            EvadeChance = 0.05f,
            MissChance = 0.25f
        };

        public override bool Equals(object obj)
        {
            if (!(obj is ProbabilityParamsData)) return false;

            var data = (ProbabilityParamsData)obj;

            return data.CriticalChance == CriticalChance &&
                data.EvadeChance == EvadeChance &&
                data.MissChance == MissChance;
        }
    }

    /// <summary>
    /// Базовые параметры персонажа
    /// </summary>
    [Serializable]
    public struct BaseStatsData
    {
        /// <summary>
        /// Сила отвечает за урон в ближнем бой и броню
        /// </summary>
        [Tooltip("Сила отвечает за урон в ближнем бой и броню")]
        public int Strength;
        /// <summary>
        /// Ловкость отвечает за меткость и увороты
        /// </summary>
        [Tooltip("Ловкость отвечает за меткость и увороты")]
        public int Agility;
        /// <summary>
        /// Интеллект отвечает за ману и диалоговые параметры
        /// </summary>
        [Tooltip("Интеллект отвечает за ману и диалоговые параметры")]
        public int Intelligence;
        /// <summary>
        /// Точность отвечает за критические попадания и сплеш
        /// </summary>
        [Tooltip("Точность отвечает за критические попадания и сплеш")]
        public int Accuracy;

        public static BaseStatsData Empty => new BaseStatsData()
        {
            Strength = 1,
            Agility = 1,
            Intelligence = 1,
            Accuracy = 1
        };

        public override bool Equals(object obj)
        {
            if (!(obj is BaseStatsData)) return false;

            var data = (BaseStatsData)obj;

            return data.Strength == Strength &&
                data.Agility == Agility &&
                data.Intelligence == Intelligence &&
                data.Accuracy == Accuracy;
        }
    }

    /// <summary>
    /// Диалоговые таланты
    /// </summary>
    [Serializable]
    public struct DialogTalentsData
    {
        /// <summary>
        /// Устрашение
        /// </summary>
        [Tooltip("Устрашение")]
        public float Terror;
        /// <summary>
        /// Обман
        /// </summary>
        [Tooltip("Обман")]
        public float Bluff;
        /// <summary>
        /// Сочувствие
        /// </summary>
        [Tooltip("Сочувствие")]
        public float Sympathy;
        /// <summary>
        /// Торговля
        /// </summary>
        [Tooltip("Торговля")]
        public float Commerce;
        /// <summary>
        /// Удачливость
        /// </summary>
        [Tooltip("Удачливость")]
        public float Fortunate;

        public static DialogTalentsData Empty => new DialogTalentsData()
        {
            Terror = 0,
            Bluff = 0,
            Sympathy = 0,
            Commerce = 0,
            Fortunate = 0
        };

        public override bool Equals(object obj)
        {
            if (!(obj is DialogTalentsData)) return false;

            var data = (DialogTalentsData)obj;

            return data.Terror == Terror &&
                data.Bluff == Bluff &&
                data.Sympathy == Sympathy &&
                data.Commerce == Commerce &&
                data.Fortunate == Fortunate;
        }
    }
}
