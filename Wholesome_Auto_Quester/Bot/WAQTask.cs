﻿using robotManager.Helpful;
using System;
using Wholesome_Auto_Quester.Database.Models;
using Wholesome_Auto_Quester.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace Wholesome_Auto_Quester.Bot
{
    public class WAQTask
    {
        public ModelAreaTrigger Area { get; }
        public ModelGameObject GameObject { get; }
        public ModelCreature Creature { get; }

        public Vector3 Location { get; set; }
        public int Continent { get; }
        public uint ObjectDBGuid { get; }
        public int ObjectiveIndex { get; }
        public int TargetEntry { get; }
        public string TargetName { get; }
        public string TaskName { get; }
        public TaskType TaskType { get; }
        public float GameObjectSize { get; }

        public string QuestTitle { get; }
        public int QuestId { get; }

        private Timer _timeOutTimer = new Timer();
        private int _timeoutMultiplicator = 1;

        // Creatures
        public WAQTask(TaskType taskType, string creatureName, int creatureEntry, string questTitle, int questId,
            ModelCreature creature, int objectiveIndex)
        {
            TaskType = taskType;
            Creature = creature;
            Continent = creature.map;
            Location = creature.GetSpawnPosition;
            ObjectDBGuid = creature.guid;
            QuestId = questId;
            TargetEntry = creatureEntry;
            TargetName = creatureName;
            ObjectiveIndex = objectiveIndex;
            QuestTitle = questTitle;

            if (taskType == TaskType.PickupQuestFromCreature)
                TaskName = $"Pick up {QuestTitle} from {TargetName}";
            if (taskType == TaskType.TurnInQuestToCreature)
                TaskName = $"Turn in {QuestTitle} at {TargetName}";
            if (taskType == TaskType.Kill)
                TaskName = $"Kill {TargetName} for {QuestTitle}";
            if (taskType == TaskType.KillAndLoot)
                TaskName = $"Kill and Loot {TargetName} for {QuestTitle}";
        }

        // Grind
        public WAQTask(TaskType taskType, string creatureName, int creatureEntry, ModelCreature creature)
        {
            TaskType = taskType;
            Creature = creature;
            Continent = creature.map;
            Location = creature.GetSpawnPosition;
            ObjectDBGuid = creature.guid;
            TargetEntry = creatureEntry;
            TargetName = creatureName;
            TaskName = $"Grind {creatureName}";
        }

        // Game Objects
        public WAQTask(TaskType taskType, string gameObjectName, int gameObjectEntry, string questTitle, int questId,
            ModelGameObject gameObject, int objectiveIndex)
        {
            TaskType = taskType;
            GameObject = gameObject;
            Continent = gameObject.map;
            Location = gameObject.GetSpawnPosition;
            ObjectDBGuid = gameObject.guid;
            QuestId = questId;
            TargetEntry = gameObjectEntry;
            TargetName = gameObjectName;
            ObjectiveIndex = objectiveIndex;
            QuestTitle = questTitle;

            if (taskType == TaskType.PickupQuestFromGameObject)
                TaskName = $"Pick up {QuestTitle} from {TargetName}";
            if (taskType == TaskType.TurnInQuestToGameObject)
                TaskName = $"Turn in {QuestTitle} at {TargetName}";
            if (taskType == TaskType.GatherGameObject)
                TaskName = $"Gather item {TargetName} for {QuestTitle}";
            if (taskType == TaskType.InteractWithWorldObject)
                TaskName = $"Interact with object {TargetName} for {QuestTitle}";
        }

        // Explore
        public WAQTask(TaskType taskType, string questTitle, int questId,
            ModelAreaTrigger modelArea, int objectiveIndex)
        {
            TaskType = taskType;
            Area = modelArea;
            Location = modelArea.GetPosition;
            ObjectiveIndex = objectiveIndex;
            Continent = modelArea.ContinentId;
            QuestTitle = questTitle;
            QuestId = questId;

            TaskName = $"Explore {Location} for {QuestTitle}";
        }

        public void PutTaskOnTimeout(string reason, bool exponentiallyLonger = false)
        {
            if (!IsTimedOut)
            {
                int timeInSeconds = Creature?.spawnTimeSecs ?? GameObject.spawntimesecs;
                if (timeInSeconds < 30) timeInSeconds = 60 * 5;
                Logger.Log($"Putting task {TaskName} on time out for {timeInSeconds * _timeoutMultiplicator} seconds. Reason: {reason}");
                _timeOutTimer = new Timer(timeInSeconds * 1000 * _timeoutMultiplicator);
                if (exponentiallyLonger)
                    _timeoutMultiplicator *= 2;
            }
        }

        public void PutTaskOnTimeout(int timeInSeconds, string reason, bool exponentiallyLonger = false)
        {
            if (!IsTimedOut)
            {
                Logger.Log($"Putting task {TaskName} on time out for {timeInSeconds * _timeoutMultiplicator} seconds. Reason: {reason}");
                _timeOutTimer = new Timer(timeInSeconds * 1000 * _timeoutMultiplicator);
                if (exponentiallyLonger)
                    _timeoutMultiplicator *= 2;
            }
        }
        public bool IsSameTask(TaskType taskType, int questEntry, int objIndex, Func<int> getUniqueId = null)
        {
            return TaskType == taskType && QuestId == questEntry && ObjectiveIndex == objIndex
                   && ObjectDBGuid == (getUniqueId?.Invoke() ?? 0);
        }

        public bool IsTimedOut => !_timeOutTimer.IsReady;
        public string TrackerColor => GetTrackerColor();
        public float GetDistance => ObjectManager.Me.PositionWithoutType.DistanceTo(Location);

        private static Vector3 _myPos = ObjectManager.Me.PositionWithoutType;
        private static uint _myLevel = ObjectManager.Me.Level;

        public static void UpdatePriorityData()
        {
            _myPos = ObjectManager.Me.PositionWithoutType;
            _myLevel = ObjectManager.Me.Level;
        }

        public int Priority
        {
            get
            {
                // Lowest priority == do first
                return CalculatePriority(_myPos.DistanceTo(Location));
            }
        }
        public int CalculatePriority(float taskDistance)
        {
            var priority = (int)System.Math.Pow(taskDistance, 1.64);
            if (TaskType == TaskType.Grind)
            {
                return priority;
            };

            var quest = WAQTasks.Quests.Find(q => q.Id == QuestId);
            const double powerBase = 1.16;

            if (priority > 0) // path found
            {
                if (TaskType == TaskType.PickupQuestFromCreature || TaskType == TaskType.PickupQuestFromGameObject)
                    priority = (int)(priority * System.Math.Pow(powerBase, WAQTasks.NbQuestsInProgress));
                if (TaskType == TaskType.TurnInQuestToCreature || TaskType == TaskType.TurnInQuestToGameObject)
                    priority = (int)(priority / System.Math.Pow(powerBase, WAQTasks.NbQuestsToTurnIn));
                if (quest.QuestAddon?.AllowableClasses > 0)
                    priority /= 8;
                if (quest.TimeAllowed > 0 && TaskType != TaskType.PickupQuestFromCreature && TaskType != TaskType.PickupQuestFromGameObject)
                    priority /= 128;
            }

            if (Continent != Usefuls.ContinentId)
                priority += 1 << 20;

            return priority;
        }

        /*
        public int CalculatePriority(float taskDistance)
        {
            if (TaskType == TaskType.Grind) return (int)taskDistance;
            ModelQuestTemplate quest = WAQTasks.Quests.Find(q => q.Id == QuestId);
            if (taskDistance > 0) // path found
            {
                if (TaskType == TaskType.PickupQuestFromCreature || TaskType == TaskType.PickupQuestFromGameObject)
                    taskDistance *= System.Math.Max(1.0f, WAQTasks.NbQuestsInProgress << 1);
                if (TaskType == TaskType.TurnInQuestToCreature || TaskType == TaskType.TurnInQuestToGameObject)
                    taskDistance /= WAQTasks.NbQuestsToTurnIn << 1;
                if (quest.QuestAddon?.AllowableClasses > 0)
                    taskDistance /= 5;
                if (quest.TimeAllowed > 0 && TaskType != TaskType.PickupQuestFromCreature && TaskType != TaskType.PickupQuestFromGameObject)
                    taskDistance /= 100;
            }

            if (Continent != Usefuls.ContinentId)
                taskDistance += 1000000;

            return (int)taskDistance;
        }
        */
        private string GetTrackerColor()
        {
            if (IsTimedOut)
                return "Gray";
            if (WAQTasks.TaskInProgress == this)
                return "Gold";
            if (TaskType == TaskType.Explore)
                return "Linen";
            if (TaskType == TaskType.GatherGameObject)
                return "Cyan";
            if (TaskType == TaskType.Grind)
                return "PaleGreen";
            if (TaskType == TaskType.InteractWithWorldObject)
                return "Aqua";
            if (TaskType == TaskType.Kill)
                return "OrangeRed";
            if (TaskType == TaskType.KillAndLoot)
                return "Orange";
            if (TaskType == TaskType.PickupQuestFromCreature || TaskType == TaskType.PickupQuestFromGameObject)
                return "DodgerBlue";
            if (TaskType == TaskType.TurnInQuestToCreature || TaskType == TaskType.TurnInQuestToGameObject)
                return "Lime";

            return "Beige";
        }

        public bool IsOnMyContinent => Continent == Usefuls.ContinentId;
    }
}