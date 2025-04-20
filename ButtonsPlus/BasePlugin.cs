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
    [BepInPlugin("mtm101.rulerp.baldiplus.buttonsplus", "Buttons+", "2.0.0.0")]
    public class ButtonsPlusPlugin : BaseUnityPlugin
    {
        public static ButtonsPlusPlugin Instance;

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.buttonsplus");

            harmony.PatchAllConditionals();
        }


    }
}
