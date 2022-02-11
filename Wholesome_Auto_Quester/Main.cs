﻿using robotManager.Events;
using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wholesome_Auto_Quester;
using Wholesome_Auto_Quester.Bot;
using Wholesome_Auto_Quester.GUI;
using Wholesome_Auto_Quester.Helpers;
using wManager.Plugin;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : IProduct
{
    public static readonly string ProductVersion = "0.1.04"; // Must match version in Version.txt
    public static readonly string ProductName = "Wholesome Auto Quester";
    public static readonly string FileName = "Wholesome_Auto_Quester";
    private ProductSettingsControl _settingsUserControl;
    private readonly QuestsTrackerGUI tracker = new QuestsTrackerGUI();
    private readonly Bot _bot = new Bot();

    public bool IsStarted { get; private set; }

    public void Initialize()
    {
        try
        {
            WholesomeAQSettings.Load();
            Logger.Log($"{ProductName} version {ProductVersion} loaded");
        }
        catch (Exception e)
        {
            Logging.WriteError("Main > Initialize(): " + e);
        }
    }

    public void Start()
    {
        try
        {
            if (AutoUpdater.CheckUpdate(ProductVersion))
            {
                return;
            }

            if (!AutoUpdater.CheckDbDownload())
            {
                return;
            }

            if (!WholesomeAQSettings.CurrentSetting.RecordUnreachables)
            {
                WholesomeAQSettings.CurrentSetting.RecordedUnreachables.Clear();
                WholesomeAQSettings.CurrentSetting.Save();
            }

            IsStarted = true;
            LoggingEvents.OnAddLog += AddLogHandler;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsWithArgsHandler;
            EventsLua.AttachEventLua("PLAYER_DEAD", e => PlayerDeadHandler(e));

            if (!Products.IsStarted)
            {
                IsStarted = false;
                return;
            }

            Task.Run(async () =>
            {
                while (IsStarted)
                {
                    try
                    {
                        if (Conditions.InGameAndConnectedAndProductStartedNotInPause)
                        {
                            Quest.RequestQuestsCompleted();
                            Quest.ConsumeQuestsCompletedRequest();
                        }
                        await Task.Delay(5 * 1000);
                    }
                    catch (Exception arg)
                    {
                        Logging.WriteError(string.Concat(arg));
                    }
                }
            });

            if (_bot.Pulse(tracker))
            {
                PluginsManager.LoadAllPlugins();

                Logging.Status = "Start Product Complete";
                Logging.Write("Start Product Complete");
            }
            else
            {
                IsStarted = false;
                Logging.Status = "Start Product failed";
                Logging.Write("Start Product failed");
            }
        }
        catch (Exception e)
        {
            IsStarted = false;
            Logging.WriteError("Main > Start(): " + e);
        }
    }

    public void Dispose()
    {
        try
        {
            Stop();
            Logging.Status = "Dispose Product Complete";
            Logging.Write("Dispose Product Complete");
        }
        catch (Exception e)
        {
            Logging.WriteError("Main > Dispose(): " + e);
        }
    }

    public void Stop()
    {
        try
        {
            Lua.RunMacroText("/stopcasting");
            MoveHelper.StopAllMove(true);
            LoggingEvents.OnAddLog -= AddLogHandler;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsWithArgsHandler;
            _bot.Dispose();
            IsStarted = false;
            PluginsManager.DisposeAllPlugins();
            Logging.Status = "Stop Product Complete";
            Logging.Write("Stop Product Complete");
        }
        catch (Exception e)
        {
            Logging.WriteError("Main > Stop(): " + e);
        }
    }

    // LOG EVENTS
    private void AddLogHandler(Logging.Log log)
    {
        if (log.Text == "[Fight] Mob seem bugged" && ObjectManager.Target.Guid > 0)
        {
            BlacklistHelper.AddNPC(ObjectManager.Target.Guid, "Mob seem bugged");
        }
        else if (log.Text == "PathFinder server seem down, use offline pathfinder.")
        {
            Logger.LogError($"The pathfinder server is down, please close and resart WRobot");
            Stop();
        }
    }

    // GUI
    public UserControl Settings
    {
        get
        {
            try
            {
                if (_settingsUserControl == null)
                {
                    _settingsUserControl = new ProductSettingsControl();
                }
                return _settingsUserControl;
            }
            catch (Exception e)
            {
                Logger.Log("> Main > Settings(): " + e);
            }

            return null;
        }
    }

    private void PlayerDeadHandler(object context)
    {
        BlacklistHelper.AddZone(ObjectManager.Me.Position, 20, "Death");
    }

    private void EventsWithArgsHandler(string id, List<string> args)
    {
        if (id == "UI_ERROR_MESSAGE" && args[0] == "You cannot attack that target.")
        {
            if (ObjectManager.Target != null)
            {
                BlacklistHelper.AddNPC(ObjectManager.Target.Guid, $"Can't attack this target");
            }
        }
    }
}