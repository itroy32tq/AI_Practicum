using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace AI
{
    /// <summary>
    /// Перечисление видов действий
    /// </summary>
    public enum ActionType : byte
    {
        /// <summary>
        /// Ожидание
        /// </summary>
        IDLE = 0,
        /// <summary>
        /// Хотьба
        /// </summary>
        MOVE = 1,
        /// <summary>
        /// Смещение боком
        /// </summary>
        Strafe = 2,
        /// <summary>
        /// Прыжок
        /// </summary>
        Jump = 3,
        /// <summary>
        /// Перекат
        /// </summary>
        Roll = 4,
        /// <summary>
        /// Уклон
        /// </summary>
        Evasion = 5,
        /// <summary>
        /// Быстрая атака
        /// </summary>
        FastAttack = 20,
        /// <summary>
        /// Сильная атака
        /// </summary>
        StrongAttack = 21,
        /// <summary>
        /// Парирование
        /// </summary>
        Parry = 22,
        /// <summary>
        /// Дальняя атака
        /// </summary>
        RangeAttack = 23,
        /// <summary>
        /// Контратака
        /// </summary>
        CounterAttack = 24,
        /// <summary>
        /// Блокирование
        /// </summary>
        Block = 40,
        /// <summary>
        /// Атакующая способность
        /// </summary>
        AttackAbility = 60,
        /// <summary>
        /// Защитная способность
        /// </summary>
        DefenseAbility = 61,
        /// <summary>
        /// Поддерживающая способность
        /// </summary>
        SupportAbility = 62,
        IMPACT = 70,
        /// <summary>
        /// Взаимодействие
        /// </summary>
        Interact = 100,
        /// <summary>
        /// смерть
        /// </summary>
        Die = 101
    }

    /// <summary>
    /// Перечисление типов перемещения
    /// </summary>
    public enum SteeringBehavioursType : byte
    {
        Idle = 0,
        Seek = 1,
        Flee = 2,
        Wander = 3
    }

    /// <summary>
    /// Перечисление типа дистанции
    /// </summary>
    public enum DistanceType : byte//todo parse
    {
        /// <summary>
        /// Неопределенная
        /// </summary>
        Indefinite = 0,
        /// <summary>
        /// Тесное взаимодействие
        /// </summary>
        Melee = 1,
        /// <summary>
        /// Дальняя конфронтация
        /// </summary>
        Ranged = 2,
        /// <summary>
        /// Свободное расстояние
        /// </summary>
        Spell = 3
    }

    /// <summary>
    /// Перечисление паттернов поведения ботов
    /// </summary>
    public enum AIPattern : byte
    {
        /// <summary>
        /// Простой
        /// </summary>
        Simple = 0,
        /// <summary>
        /// Защищающийся
        /// </summary>
        Defender = 10,
        /// <summary>
        /// Контратакующий
        /// </summary>
        CounterDefender = 11,
        /// <summary>
        /// Быстро атакующий
        /// </summary>
        FastAttacker = 20,
        /// <summary>
        /// Сокрушающий
        /// </summary>
        StrongAttacker = 21,
        /// <summary>
        /// Дальний
        /// </summary>
        Further = 30,
        /// <summary>
        /// Поддерживающий
        /// </summary>
        Support = 40,
        /// <summary>
        /// Маг
        /// </summary>
        Mage = 50,
    }

    /// <summary>
    /// Тип действия статуса
    /// </summary>
    public enum StatusType : byte
    {
        /// <summary>
        /// Одноразовый
        /// </summary>
        Single = 0,
        /// <summary>
        /// Постоянный
        /// </summary>
        Persistance = 1,
        /// <summary>
        /// Зацикленный
        /// </summary>
        Looping = 2,
        /// <summary>
        /// Серийный
        /// </summary>
        Series = 3
    }

    /// <summary>
    /// Состояние статуса
    /// </summary>
    public enum StateStatusType : byte
    {
        /// <summary>
        /// Активация
        /// </summary>
        Create,
        /// <summary>
        /// Начало работы
        /// </summary>
        Start,
        /// <summary>
        /// Завершение
        /// </summary>
        Finish
    }

    /// <summary>
    /// Типы юнитов
    /// </summary>
    public enum UnitType : byte
    {
        /// <summary>
        /// Неопределенный тип юнита
        /// </summary>
        None = 0,
        /// <summary>
        /// Игрок
        /// </summary>
        Player = 1,
        /// <summary>
        /// Ближник
        /// </summary>
        RedMelee = 10,
        BluMelee = 13,
        GreenMelee = 14,
        /// <summary>
        /// Удаленщик
        /// </summary>
        Ranger = 11,
        /// <summary>
        /// Маг
        /// </summary>
        Mage = 12
    }

    /// <summary>
    /// Перечисление типов результатов действия
    /// </summary>
    public enum ActionResultType : byte
    {
        /// <summary>
        /// Нет результата
        /// </summary>
        None = 0,
        /// <summary>
        /// Прервано
        /// </summary>
        Interrupted = 1,
        /// <summary>
        /// Провально
        /// </summary>
        Failed = 2,
        /// <summary>
        /// Успешно
        /// </summary>
        Successfully = 3,
        /// <summary>
        /// Завершен
        /// </summary>
        Completed = 4,
    }

    /// <summary>
    /// Роль персонажа игрока
    /// </summary>
    public enum RoleType : byte
    {
        None = 0,
        Warrior = 1,
        Archer = 2,
        Mage = 3,
    }

    [Flags]
    public enum IgnoreAxisType : byte
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4
    }
    [Flags]
    public enum FabricType : byte
    {
        Red = 0,
        Green = 1,
        Blue = 2,
    }
}
