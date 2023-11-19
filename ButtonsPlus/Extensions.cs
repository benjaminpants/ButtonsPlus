using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ButtonsPlus
{
    public static class Extensions
    {
        public static GameButton BuildInAreaWithColor(EnvironmentController ec, IntVector2 posA, IntVector2 posB, int buttonRange, GameObject receiver, GameButton buttonPre, string colorKey, System.Random cRng)
        {
            GameButton gb = GameButton.BuildInArea(ec,posA,posB,buttonRange,receiver,buttonPre,cRng);
            ButtonsPlusPlugin.ApplyButtonMaterials(gb, colorKey);
            return gb;
        }

        public static void ChangeColor(this GameButton me, ButtonMaterials bm)
        {
            ButtonsPlusPlugin.ApplyButtonMaterials(me, bm);
        }

        public static void ChangeColor(this GameLever me, ButtonMaterials bm)
        {
            ButtonsPlusPlugin.ApplyLeverMaterials(me, bm);
        }

        public static void ChangeColor(this GameButton me, string colorKey)
        {
            ButtonsPlusPlugin.ApplyButtonMaterials(me, colorKey);
        }

        public static void ChangeColor(this GameLever me, string colorKey)
        {
            ButtonsPlusPlugin.ApplyLeverMaterials(me,colorKey);
        }

        private static void ChangeLeverColorRandom(GameLever lvr, System.Random cRng)
        {
            if (!ButtonsPlusPlugin.Instance.DoRandomButtonColors) return;
            // I wanted to create a seperate seed consistent RNG so custom button colors don't change all seeds, but I can't find a good way of pulling that off...
            Dictionary<string, ButtonMaterials>.ValueCollection values = ButtonsPlusPlugin.buttonColors.Values;
            ButtonMaterials bM = values.ToList()[cRng.Next(values.Count)];
            ButtonsPlusPlugin.ApplyLeverMaterials(lvr, bM);
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
            GameLever lever = Extensions.BuildLeverInArea(ec, posA, posB, buttonRange, receiver, ButtonsPlusPlugin.leverPrefab, cRng);
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

        public static GameLever BuildLeverInArea(EnvironmentController ec, IntVector2 posA, IntVector2 posB, int buttonRange, GameObject receiver, GameLever leverPre, System.Random cRng)
        {
            GameLever lever;
            GameButton fakePrefab = new GameObject().AddComponent<GameButton>();
            GameButton gb = GameButton.BuildInArea(ec, posA, posB, buttonRange, receiver, fakePrefab, cRng); //create a new gamebutton object because we'll be deleting it anyway
            GameObject.Destroy(fakePrefab.gameObject); //don't need this anymore
            if (gb == null) return null; //the button didn't succesfully spawn so we have nowhere to put the lever
            lever = UnityEngine.Object.Instantiate(leverPre, gb.transform.parent); // parent to the tile controller
            lever.transform.rotation = gb.transform.rotation; // rotate to whatever the button was rotated to
            lever.SetUp(receiver.GetComponent<IButtonReceiver>());
            GameObject.Destroy(gb.gameObject);
            ChangeLeverColorRandom(lever, cRng);
            return lever;

        }
    }
}
