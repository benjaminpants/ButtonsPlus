using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MTM101BaldAPI.Registers.Buttons;
using MTM101BaldAPI;
using HarmonyLib;

namespace ButtonsPlus
{
    public static class Extensions
    {
        internal static void ChangeLeverColorRandom(GameLever lvr, System.Random cRng)
        {
            if (!ButtonsPlusPlugin.Instance.DoRandomButtonColors) return;
            // I wanted to create a seperate seed consistent RNG so custom button colors don't change all seeds, but I can't find a good way of pulling that off...
            Dictionary<string, ButtonMaterials>.ValueCollection values = ButtonColorManager.buttonColors.Values;
            ButtonMaterials bM = values.ToList()[cRng.Next(values.Count)];
            ButtonColorManager.ApplyLeverMaterials(lvr, bM);
        }


        // incase a mod disables this mid-game, i do an extra check here
        // DONT USE THE PATCHABLE VARIANT UNLESS YOU ARE PATCHING SOMETHING
        public static GameLever BuildInAreaPatchable(EnvironmentController ec, IntVector2 posA, IntVector2 posB, int buttonRange, GameObject receiver, GameButton buttonPre, System.Random cRng)
        {
            if (!ButtonsPlusPlugin.Instance.DoLockdownLevers)
            {
                GameLever fakelever = new GameObject().AddComponent<GameLever>();
                ec.StartCoroutine(Destroy(fakelever.gameObject));
                GameButton.BuildInArea(ec, posA, posB, buttonRange, receiver, buttonPre, cRng);
                return fakelever;
            }
            GameLever lever = GeneratorHelpers.BuildLeverInArea(ec, posA, posB, buttonRange, receiver, GeneratorHelpers.leverPrefab, cRng);
            return lever;
        }

        public static GameLever BuildInAreaPatchableStartOn(EnvironmentController ec, IntVector2 posA, IntVector2 posB, int buttonRange, GameObject receiver, GameButton buttonPre, System.Random cRng)
        {
            GameLever l = BuildInAreaPatchable(ec, posA, posB, buttonRange, receiver, null, cRng);
            if (l == null) return null;
            l.Set(true);
            return l;
        }

        static IEnumerator Destroy(GameObject go) 
        {
            yield return null;
            yield return null;
            yield return null;
            GameObject.Destroy(go);
            yield break;
        }
    }

    //i cant believe im patching MY OWN FUNCTION
    [HarmonyPatch(typeof(GeneratorHelpers))]
    [HarmonyPatch("BuildLeverInArea")]
    static class LevelBuilderPatch
    {
        static void Postfix(ref GameLever __result, ref System.Random cRng)
        {
            Extensions.ChangeLeverColorRandom(__result, cRng);
        }
    }
}
