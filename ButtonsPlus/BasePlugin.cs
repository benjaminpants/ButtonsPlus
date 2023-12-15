using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Assertions;
using System.Reflection;
using BepInEx.Configuration;

namespace ButtonsPlus
{
    [BepInPlugin("mtm101.rulerp.baldiplus.buttonsplus", "Buttons+", "1.0.0.1")]
    public class ButtonsPlusPlugin : BaseUnityPlugin
    {

        public ConfigEntry<bool> configRandomColors;
        public ConfigEntry<bool> configLockdownLevers;
        public ConfigEntry<bool> configLockSwitch;
        private bool modRandomColors = true;
        private bool modLockdownLevers = true;
        private bool modLockSwitch = false;

        // Both the player and any mods that set this must agree(both must be true) in order for random button colors to be enabled
        public bool DoRandomButtonColors
        {
            get
            {
                return modRandomColors && configRandomColors.Value;
            }
            set
            {
                modRandomColors = value;
            }
        }

        // Only the player or the mod itself will have to agree for this to work.
        public bool DoLockSwitch
        {
            get
            {
                return modLockSwitch || configLockSwitch.Value;
            }
            set
            {
                modLockSwitch = value;
            }
        }

        // Both the player and any mods that set this must agree(both must be true) in order for lockdown levers to be enabled
        public bool DoLockdownLevers
        {
            get
            {
                return modLockdownLevers && configLockdownLevers.Value;
            }
            set
            {
                modLockdownLevers = value;
            }
        }

        public static ButtonsPlusPlugin Instance;

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.buttonsplus");

            configRandomColors = Config.Bind(
                "General",
                "RandomColors",
                true,
                "Whether or not randomly generated levels should get randomized color buttons. Mods can forecfully disable this."
                );

            configLockdownLevers = Config.Bind(
                "General",
                "LockdownLevers",
                true,
                "Whether or not lockdown doors use levers instead of buttons. Mods can forcefully disable this."
                );

            configLockSwitch = Config.Bind(
                "General",
                "LockLeversWhenDoorMove",
                true,
                "Whether or not levers will be unusable until all lockdown doors connected to them are finished moving. Mods can forcefully enable this."
                );

            harmony.PatchAllConditionals();
        }


    }
}
