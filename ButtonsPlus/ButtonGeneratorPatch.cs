using BepInEx;
using HarmonyLib;
using MTM101BaldAPI.AssetManager;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Assertions;
using System.Reflection.Emit;
using System.Reflection;

namespace ButtonsPlus
{
    [HarmonyPatch(typeof(GameButton))]
    [HarmonyPatch("BuildInArea")]
    class ButtonGeneratorPatch
    {
        static void Postfix(ref GameButton __result, ref System.Random cRng)
        {
            if (!ButtonsPlusPlugin.Instance.DoRandomButtonColors) return;
            if (__result.gameObject.name.Contains("New Game Object")) return;
            // I wanted to create a seperate seed consistent RNG so custom button colors don't change all seeds, but I can't find a good way of pulling that off...
            Dictionary<string, ButtonMaterials>.ValueCollection values = ButtonsPlusPlugin.buttonColors.Values;
            ButtonMaterials bM = values.ToList()[cRng.Next(values.Count)];
            ButtonsPlusPlugin.ApplyButtonMaterials(__result, bM);
        }
    }

    [ConditionalPatchLockdownLevers]
    [HarmonyPatch(typeof(LockdownDoorBuilder))]
    [HarmonyPatch("Build")]
    class LockdownBuilderPatch
    {

        static MethodInfo buttonBuild = AccessTools.Method(typeof(GameButton), "BuildInArea");
        static MethodInfo leverBuild = AccessTools.Method(typeof(Extensions), "BuildInAreaPatchableStartOn");

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int changed = 0;
            //instructions.MethodReplacer(buttonBuild, leverBuild);
            foreach (CodeInstruction instruction in instructions)
            {
                if ((object)(instruction.operand as MethodBase) == buttonBuild)
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = leverBuild;
                    changed++;
                }

                yield return instruction;
            }
            if (changed == 0)
            {
                throw new Exception("Failed to change anything in LockdownDoorBuilder!");
            }
        }

        //after everything has been build
        static void Postfix(RoomController room)
        {
            if (!ButtonsPlusPlugin.Instance.DoLockdownLevers) return;
            LockdownDoor[] doors = room.transform.GetComponentsInChildren<LockdownDoor>();
            foreach (LockdownDoor door in doors) 
            { 
                if (door.gameObject.GetComponent<LockdownHandledByLever>() == null)
                {
                    door.gameObject.AddComponent<LockdownHandledByLever>();
                }
            }
            /*LockdownDoor d = doors.Last();
            if (d.gameObject.GetComponent<LockdownHandledByLever>()) throw new Exception("Attempted to add LockdownHandledByLever to a door that already had it!");
            d.gameObject.AddComponent<LockdownHandledByLever>();*/
        }
    }
}
