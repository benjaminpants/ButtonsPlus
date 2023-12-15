using BepInEx;
using HarmonyLib;
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

    // This class does...... NOTHING!!
    // It's just to let the lockdown door easily know that it is being controlled by a lever.
    public class LockdownHandledByLever : MonoBehaviour
    {

    }

    [HarmonyPatch(typeof(LockdownDoor))]
    [HarmonyPatch("ButtonPressed")]
    class LockdownButtonPressedPatch
    {
        static bool Prefix(LockdownDoor __instance, bool val)
        {
            if (!__instance.gameObject.GetComponent<LockdownHandledByLever>()) return true;
            if (val)
            {
                __instance.Open(true,false);
            }
            else
            {
                __instance.Shut();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(GameLever))]
    [HarmonyPatch("Pressed")]
    class LeverPressedPatch
    {

        static FieldInfo lockdownMoving = AccessTools.Field(typeof(LockdownDoor), "moving");

        static bool Prefix(GameLever __instance, ref List<IButtonReceiver> ___buttonReceivers, bool ___on)
        {
            if (!ButtonsPlusPlugin.Instance.DoLockSwitch) return true;
            foreach (IButtonReceiver ib in ___buttonReceivers)
            {
                if (ib is LockdownDoor)
                {
                    if ((bool)lockdownMoving.GetValue((LockdownDoor)ib))
                    {
                        __instance.Set(___on); //dont toggle
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
