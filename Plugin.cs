using BepInEx;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using UnityEngine;
using Utilla;

namespace Sandstorm_AntiCrash
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        bool modDisabled;
        bool fastMode; // makes the game EVEN FASTER, but is bad at optimizing. off by default
        bool lagging;
        bool normalMode; // normal real time optimization!
        string returnThis;

        public void ThrottledLoop(Action action, int cpuPercentageLimit)
        {
            Stopwatch stopwatch = new Stopwatch();

            while (true)
            {
                stopwatch.Reset();
                stopwatch.Start();

                long actionStart = stopwatch.ElapsedTicks;
                action.Invoke();
                long actionEnd = stopwatch.ElapsedTicks;
                long actionDuration = actionEnd - actionStart;

                long relativeWaitTime = (int)(
                    (1 / (double)cpuPercentageLimit) * actionDuration);

                Thread.Sleep((int)((relativeWaitTime / (double)Stopwatch.Frequency) * 1000));
            }
        }

        public void UpdateSettings(string variable, string value)
        {
            string path = @"../SSOptimize/" + variable + ".txt";
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine(value);
                }
            }
        }

        public string ReadSettings(string variable)
        {
            // Open the file to read from.
            string path = @"../SSOptimize/" + variable + ".txt";
            using (StreamReader sr = File.OpenText(path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    returnThis = s;
                }
            }
            return returnThis;
        }

        public void SaveAll()
        {
            normalMode = bool.Parse(ReadSettings("normalMode")); // if you're wondering "parse" just turns the string "true" into the boolean true
            fastMode = bool.Parse(ReadSettings("fastMode"));
            modDisabled = bool.Parse(ReadSettings("modDisabled"));
        }

        public void UpdateAll()
        {
            UpdateSettings("normalMode", normalMode.ToString());
            UpdateSettings("fastMode", fastMode.ToString());
            UpdateSettings("modDisabled", modDisabled.ToString());
        }

        void OnEnable()
        {
            /* Set up your mod here */
            /* Code here runs at the start and whenever your mod is enabled*/
            modDisabled = false;
            HarmonyPatches.ApplyHarmonyPatches();
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnDisable()
        {
            /* Undo mod setup here */
            /* This provides support for toggling mods with ComputerInterface, please implement it :) */
            /* Code here runs whenever your mod is disabled (including if it disabled on startup)*/
            modDisabled = true;
            HarmonyPatches.RemoveHarmonyPatches();
            Utilla.Events.GameInitialized -= OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            if (!modDisabled)
            {
                /* Code here runs after the game initializes (i.e. GorillaLocomotion.Player.Instance != null) */
                if (fastMode == true)
                {
                    using (Process p = Process.GetCurrentProcess())
                        p.PriorityClass = ProcessPriorityClass.High;
                }
                else if (normalMode == true)
                {
                    using (Process p = Process.GetCurrentProcess())
                        p.PriorityClass = ProcessPriorityClass.AboveNormal;

                }
            }
            SaveAll();
        }

        void Update()
        {
            /* Code here runs every frame when the mod is enabled */
            UpdateAll();
        }

        /* This attribute tells Utilla to call this method when a modded room is joined */
        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            /* Activate your mod here */
            /* This code will run regardless of if the mod is enabled*/

            inRoom = true;
        }

        /* This attribute tells Utilla to call this method when a modded room is left */
        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            /* Deactivate your mod here */
            /* This code will run regardless of if the mod is enabled*/

            inRoom = false;
        }
    }
}
