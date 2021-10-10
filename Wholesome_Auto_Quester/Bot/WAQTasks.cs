﻿using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Wholesome_Auto_Quester.Database.Models;
using Wholesome_Auto_Quester.Database.Objectives;
using Wholesome_Auto_Quester.Helpers;
using wManager;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static wManager.Wow.Helpers.Quest.PlayerQuest;

namespace Wholesome_Auto_Quester.Bot {
    public class WAQTasks {
        private static int _tick;
        public static List<WAQTask> TasksPile { get; set; } = new List<WAQTask>();
        public static List<ModelQuest> Quests { get; set; } = new List<ModelQuest>();
        public static WAQTask TaskInProgress { get; set; }
        public static WoWObject TaskInProgressWoWObject { get; set; }

        public static void AddQuests(List<ModelQuest> quests) {
            quests.ForEach(newQuest => {
                if (!Quests.Exists(quest => quest.Id == newQuest.Id))
                    Quests.Add(newQuest);
            });
        }

        public static void UpdateTasks() {
            if (Quests.Count <= 0 
                || !ObjectManager.Me.IsAlive 
                || !ObjectManager.Me.IsValid 
                || Fight.InFight)
                return;

            //Logger.Log("Update tasks");
            var generatedTasks = new List<WAQTask>();
            int myContinent = Usefuls.ContinentId;
            int myLevel = (int)ObjectManager.Me.Level;
            Vector3 myPosition = ObjectManager.Me.Position;
            ToolBox.UpdateObjectiveCompletionDict(Quests.Where(quest => quest.Status == QuestStatus.InProgress)
                .Select(quest => quest.Id).ToArray());
            foreach (ModelQuest quest in Quests) {
                // Completed
                if (quest.Status == QuestStatus.Completed || quest.Status == QuestStatus.Blacklisted) {
                    TasksPile.RemoveAll(t => t.Quest.Id == quest.Id);
                    continue;
                }

                // Turn in quest
                if (quest.Status == QuestStatus.ToTurnIn) {
                    TasksPile.RemoveAll(t => t.Quest.Id == quest.Id && t.TaskType != TaskType.TurnInQuestToNpc);
                    quest.NpcQuestTurners.ForEach(npcQt => {
                        if (npcQt.Map == myContinent
                            && !TasksPile.Exists(t =>
                                t.IsSameTask(TaskType.TurnInQuestToNpc, quest.Id, 5, () => npcQt.Guid)))
                            generatedTasks.Add(new WAQTask(TaskType.TurnInQuestToNpc, npcQt, null, quest, 5));
                    });
                    quest.WorldObjectQuestTurners.ForEach(wQt => {
                        if (wQt.Map == myContinent
                            && !TasksPile.Exists(t =>
                                t.IsSameTask(TaskType.TurnInQuestToGameObject, quest.Id, 5, () => wQt.Guid)))
                            generatedTasks.Add(new WAQTask(TaskType.TurnInQuestToGameObject, null, wQt, quest, 5));
                    });
                    continue;
                }

                // Pick up quest
                if (quest.Status == QuestStatus.ToPickup) {
                    TasksPile.RemoveAll(t => t.Quest.Id == quest.Id && t.TaskType != TaskType.PickupQuestFromNpc);
                    quest.NpcQuestGivers.ForEach(qg => {
                        if (qg.Map == myContinent
                            && !TasksPile.Exists(t =>
                                t.IsSameTask(TaskType.PickupQuestFromNpc, quest.Id, 6, () => qg.Guid)))
                            generatedTasks.Add(new WAQTask(TaskType.PickupQuestFromNpc, qg, null, quest, 6));
                    });
                    quest.WorldObjectQuestGivers.ForEach(qg => {
                        if (qg.Map == myContinent
                            && !TasksPile.Exists(t =>
                                t.IsSameTask(TaskType.PickupQuestFromGameObject, quest.Id, 6, () => qg.Guid)))
                            generatedTasks.Add(new WAQTask(TaskType.PickupQuestFromGameObject, null, qg, quest, 6));
                    });
                    continue;
                }

                if (quest.Status == QuestStatus.InProgress) {
                    TasksPile.RemoveAll(t => t.Quest.Id == quest.Id
                                             && (t.TaskType == TaskType.PickupQuestFromNpc ||
                                                 t.TaskType == TaskType.TurnInQuestToNpc));

                    // Explore
                    foreach (ExplorationObjective obje in quest.ExplorationObjectives)
                        if (!ToolBox.GetObjectiveCompletion(obje.ObjectiveIndex, quest.Id)) {
                            if (obje.Area.ContinentId == myContinent
                                && !TasksPile.Exists(t =>
                                    t.IsSameTask(TaskType.Explore, quest.Id,
                                        obje.ObjectiveIndex)))
                                generatedTasks.Add(new WAQTask(TaskType.Explore, obje.Area, quest,
                                    obje.ObjectiveIndex));
                        } else {
                            TasksPile.RemoveAll(t => t.Quest.Id == quest.Id
                                                     && t.ObjectiveIndex == obje.ObjectiveIndex
                                                     && t.TaskType == TaskType.Explore);
                        }

                    // Kill & Loot
                    foreach (KillLootObjective obje in quest.KillLootObjectives)
                        if (!ToolBox.GetObjectiveCompletion(obje.ObjectiveIndex, quest.Id))
                            obje.WorldCreatures.ForEach(wc => {
                                if (wc.Map == myContinent
                                    && wc.MaxLevel <= myLevel + 2
                                    && !TasksPile.Exists(t =>
                                        t.IsSameTask(TaskType.KillAndLoot, quest.Id,
                                            obje.ObjectiveIndex, () => wc.Guid)))
                                    generatedTasks.Add(new WAQTask(TaskType.KillAndLoot, wc, null, quest,
                                        obje.ObjectiveIndex));
                            });
                        else
                            TasksPile.RemoveAll(t => t.Quest.Id == quest.Id
                                                     && t.ObjectiveIndex == obje.ObjectiveIndex
                                                     && t.TaskType == TaskType.KillAndLoot);

                    // Kill
                    foreach (KillObjective obje in quest.KillObjectives)
                        if (!ToolBox.GetObjectiveCompletion(obje.ObjectiveIndex, quest.Id))
                            obje.WorldCreatures.ForEach(wc => {
                                if (wc.Map == myContinent
                                    && wc.MaxLevel <= myLevel + 2
                                    && !TasksPile.Exists(t =>
                                        t.IsSameTask(TaskType.Kill, quest.Id,
                                            obje.ObjectiveIndex, () => wc.Guid)))
                                    generatedTasks.Add(new WAQTask(TaskType.Kill, wc, null, quest,
                                        obje.ObjectiveIndex));
                            });
                        else
                            TasksPile.RemoveAll(t => t.Quest.Id == quest.Id
                                                     && t.ObjectiveIndex == obje.ObjectiveIndex
                                                     && t.TaskType == TaskType.Kill);

                    // Gather object
                    foreach (GatherObjective obje in quest.GatherObjectives)
                        if (!ToolBox.GetObjectiveCompletion(obje.ObjectiveIndex, quest.Id))
                            obje.WorldObjects.ForEach(wo => {
                                if (wo.Map == myContinent
                                    && !TasksPile.Exists(t =>
                                        t.IsSameTask(TaskType.GatherObject, quest.Id,
                                            obje.ObjectiveIndex, () => wo.Guid)))
                                    generatedTasks.Add(new WAQTask(TaskType.GatherObject, wo, quest,
                                        obje.ObjectiveIndex));
                            });
                        else
                            TasksPile.RemoveAll(t => t.Quest.Id == quest.Id
                                                     && t.ObjectiveIndex == obje.ObjectiveIndex
                                                     && t.TaskType == TaskType.GatherObject);

                    // Interact with object
                    foreach (InteractObjective obje in quest.InteractObjectives)
                        if (!ToolBox.GetObjectiveCompletion(obje.ObjectiveIndex, quest.Id))
                            obje.WorldObjects.ForEach(wo => {
                                if (wo.Map == myContinent
                                    && !TasksPile.Exists(t =>
                                        t.IsSameTask(TaskType.InteractWithWorldObject, quest.Id,
                                            obje.ObjectiveIndex, () => wo.Guid)))
                                    generatedTasks.Add(new WAQTask(TaskType.InteractWithWorldObject, wo, quest,
                                        obje.ObjectiveIndex));
                            });
                        else
                            TasksPile.RemoveAll(t => t.Quest.Id == quest.Id
                                                     && t.ObjectiveIndex == obje.ObjectiveIndex
                                                     && t.TaskType == TaskType.GatherObject);
                }
            }

            TasksPile.AddRange(generatedTasks);

            if (TasksPile.Count <= 0)
                return;
            
            // Filter far away new quests if we still have quests to turn in
            // if (TasksPile.Any(task => task.Quest.ShouldQuestBeFinished())) {
            //     TasksPile.RemoveAll(task => task.TaskType == TaskType.PickupQuest && !Quests
            //         .Where(quest => quest.ShouldQuestBeFinished())
            //         .Any(questToBeFinished => questToBeFinished.QuestGivers
            //             .Any(questToFinishGiver => task.Quest.QuestGivers.Any(taskQuestGiver =>
            //                 taskQuestGiver.Position().DistanceTo(questToFinishGiver.Position()) < 250))));
            // }
            
            WAQTask.UpdatePriorityData();
            TasksPile = TasksPile.Where(task => !wManagerSetting.IsBlackListedZone(task.Location))
                .OrderBy(t => t.Priority).ToList();

            WAQTask closestTask = TasksPile.Find(t => !t.IsTimedOut);

            // Check if pathing distance of first entries is not too far (big detour)
            var watchTaskLong = Stopwatch.StartNew();
            int nbPathFinds = 0;
            float closestTaskDistance = ToolBox.CalculatePathTotalDistance(myPosition, closestTask.Location);
            nbPathFinds++;
            if (closestTaskDistance > closestTask.GetDistance * 2)
            {
                Logger.LogError($"Detour detected for task {closestTask.TaskName}");
                int nbTasks = TasksPile.Count;
                for (int i = 0; i < nbTasks - 1; i++)
                {
                    if (!TasksPile[i].IsTimedOut)
                    {
                        float walkDistanceToTask = ToolBox.CalculatePathTotalDistance(myPosition, TasksPile[i].Location);
                        nbPathFinds++;
                        float nextFlyDistanceToTask = TasksPile[i + 1].GetDistance;
                        //Logger.Log($"Task {i} is {walkDistanceToTask} yards away (walk)");
                        //Logger.Log($"Task {i + 1} is {nextFlyDistanceToTask} yards away (fly)");

                        if (walkDistanceToTask < closestTaskDistance)
                        {
                            closestTaskDistance = walkDistanceToTask;
                            closestTask = TasksPile[i];
                        }

                        if (closestTaskDistance < nextFlyDistanceToTask)
                            break;
                    }
                }
            }
            //Logger.Log($"TASK: [{nbPathFinds}] [{watchTaskLong.ElapsedMilliseconds}ms] [{closestTask?.TaskName}]");

            // Get unique POIs
            var researchedTasks = new List<WAQTask>();
            var wantedUnitEntries = new List<int>();
            var wantedObjectEntries = new List<int>();
            TasksPile.ForEach(pileTask => {
                if (!researchedTasks.Exists(poiTasks => poiTasks.POIEntry == pileTask.POIEntry) &&
                    !pileTask.IsTimedOut) {
                    if (pileTask.Npc != null)
                        wantedUnitEntries.Add(pileTask.POIEntry);
                    if (pileTask.Item != null || pileTask.WorldObject != null)
                        wantedObjectEntries.Add(pileTask.POIEntry);

                    researchedTasks.Add(pileTask);
                }
            });

            // Look for surrounding POIs
            List<WoWObject> surroundingWoWObjects = ObjectManager.GetObjectWoW();

            var watchObjectsShort = Stopwatch.StartNew();
            List<WoWObject> filteredSurroundingObjects = surroundingWoWObjects.FindAll(o => {
                int entry = o.Entry;
                WoWObjectType type = o.Type;
                return (type == WoWObjectType.Unit && wantedUnitEntries.Contains(entry)
                        || type == WoWObjectType.GameObject && wantedObjectEntries.Contains(entry))
                       && o.GetRealDistance() < 60
                       && IsObjectValidForTask(o, researchedTasks.Find(task => task.POIEntry == entry));
            }).OrderBy(o => o.GetDistance).ToList();

            // Get objects real distance
            var watchObjectShort = Stopwatch.StartNew();
            nbPathFinds = 0;
            if (filteredSurroundingObjects.Count > 0)
            {
                WoWObject closestObject = filteredSurroundingObjects[0];
                float closestObjectDistance = ToolBox.CalculatePathTotalDistance(myPosition, closestObject.Position);
                nbPathFinds++;

                if (closestObjectDistance > closestObject.GetDistance * 2)
                {
                    Logger.LogError($"Detour detected for object {closestObject.Name}");
                    int nbObject = filteredSurroundingObjects.Count;
                    //Logger.Log($"There are {nbObject} objects");
                    for (int i = 0; i < nbObject - 1; i++)
                    {
                        float walkDistanceToObject = ToolBox.CalculatePathTotalDistance(myPosition, filteredSurroundingObjects[i].Position);
                        nbPathFinds++;
                        float nextFlyDistanceToObject = filteredSurroundingObjects[i + 1].GetDistance;
                        //Logger.Log($"Object {i} is {walkDistanceToObject} yards away (walk)");
                        //Logger.Log($"Object {i + 1} is {nextFlyDistanceToObject} yards away (fly)");

                        if (walkDistanceToObject < closestObjectDistance)
                        {
                            closestObjectDistance = walkDistanceToObject;
                            closestObject = filteredSurroundingObjects[i];
                        }

                        if (closestObjectDistance < nextFlyDistanceToObject)
                            break;
                    }
                    /*
                    TaskInProgressWoWObject = filteredSurroundingObjects.TakeHighest(
                        o => (int) -o.GetRealDistance());*/
                }

                if (closestObjectDistance > closestTaskDistance + 20)
                    TaskInProgressWoWObject = null;
                else
                {
                    TaskInProgressWoWObject = closestObject;
                    closestTask = researchedTasks.Find(task => task.POIEntry == TaskInProgressWoWObject.Entry);
                }
            } else {
                TaskInProgressWoWObject = null;
            }
            //Logger.Log($"OBJECT: [{nbPathFinds}] [{watchObjectsShort.ElapsedMilliseconds}ms] [{TaskInProgressWoWObject?.Name}]");
            //Logger.Log($"**************************************");

            TaskInProgress = closestTask;

            /*Logger.Log($"********************************************");
            Logger.Log($"TaskInProgress: {TaskInProgress?.TaskName}");
            Logger.Log($"TaskInProgressWoWObject: {TaskInProgressWoWObject?.Name}");*/

            if (_tick++ % 5 == 0) Main.QuestTrackerGui.UpdateTasksList();
        }

        private static bool IsObjectValidForTask(WoWObject wowObject, WAQTask task) {
            if (task.TaskType == TaskType.KillAndLoot) {
                var unit = (WoWUnit) wowObject;
                if (!unit.IsAlive && !unit.IsLootable)
                    return false;
            }

            if (task.TaskType == TaskType.Kill) {
                var unit = (WoWUnit) wowObject;
                if (!unit.IsAlive)
                    return false;
            }

            return true;
        }

        public static void UpdateStatuses() {
            ToolBox.UpdateCompletedQuests();
            Dictionary<int, Quest.PlayerQuest> currentQuests = Quest.GetLogQuestId().ToDictionary(quest => quest.ID);
            ModelQuest[] completedQuests =
                Quests.Where(q => q.Status == QuestStatus.Completed && q.PreviousQuestsIds.Count > 0).ToArray();
            // Update quests statuses
            foreach (ModelQuest quest in Quests)
            {
                // Quest blacklisted
                if (WholesomeAQSettings.CurrentSetting.BlacklistesQuests.Contains(quest.Id)) {
                    quest.RemoveQuestItemsFromDoNotSellList();
                    quest.Status = QuestStatus.Blacklisted;
                    continue;
                }

                // Quest completed
                if (quest.IsCompleted
                    || completedQuests.Any(q => q.PreviousQuestsIds.Contains(quest.Id))) {
                    quest.RemoveQuestItemsFromDoNotSellList();
                    quest.Status = QuestStatus.Completed;
                    continue;
                }

                // Quest to pickup
                if (quest.IsPickable()
                    && !currentQuests.ContainsKey(quest.Id)) {
                    quest.AddQuestItemsToDoNotSellList();
                    quest.Status = QuestStatus.ToPickup;
                    continue;
                }

                // Log quests
                if (currentQuests.TryGetValue(quest.Id, out Quest.PlayerQuest foundQuest)) {
                    // Quest to turn in
                    if (foundQuest.State == StateFlag.Complete) {
                        quest.Status = QuestStatus.ToTurnIn;
                        quest.AddQuestItemsToDoNotSellList();
                        continue;
                    }

                    // Quest failed
                    if (foundQuest.State == StateFlag.Failed) {
                        quest.Status = QuestStatus.Failed;
                        quest.RemoveQuestItemsFromDoNotSellList();
                        continue;
                    }

                    // Quest in progress
                    // if (Quest.HasQuest(quest.Id)) {
                    quest.Status = QuestStatus.InProgress;
                    quest.AddQuestItemsToDoNotSellList();
                    continue;
                    // }
                }

                quest.Status = QuestStatus.None;
            }

            if (_tick++ % 5 == 0) Main.QuestTrackerGui.UpdateQuestsList();
        }
    }
}