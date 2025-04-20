using HarmonyLib;
using MTM101BaldAPI.Registers.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ButtonsPlus
{
    [HarmonyPatch]
    internal class BuildButtonPatch
    {
        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethod()
        {
            return typeof(GameButton).GetMethod(
                "BuildInArea",
                new[] {
                typeof(EnvironmentController),
                typeof(IntVector2),
                typeof(int),
                typeof(GameObject),
                typeof(GameButtonBase),
                typeof(System.Random),
                typeof(bool).MakeByRefType()
                }
            );
        }

        [HarmonyPostfix]
        static void Postfix(GameButtonBase __result, System.Random cRng)
        {
            if (__result == null) return;
            string[] possibleColors = ButtonColorManager.definedColors;
            if (ButtonColorManager.TypeSupportsButtonColors(__result.GetType()))
            {
                ButtonColorManager.ApplyButtonMaterials(__result, possibleColors[cRng.Next(0, possibleColors.Length)]);
            }
        }
    }
}
