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
using System.Reflection;
using BepInEx.Configuration;

namespace ButtonsPlus
{
    public struct ButtonMaterials
    {
        public Material buttonPressed;
        public Material buttonUnpressed;
        public Material leverUp;
        public Material leverDown;
        public Color color;
        public string name;
    }


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

        private static Dictionary<string, ButtonMaterials> _buttonColors = new Dictionary<string, ButtonMaterials>();

        public static Dictionary<string, ButtonMaterials> buttonColors
        {
            get
            {
                if (_buttonColors.Count == 0)
                {
                    throw new NullReferenceException("Attempted to access buttonColors before it is defined!"); //give mods an error if they try to access button colors before they are ready
                }
                return _buttonColors;
            }
        }

        public static Material BaseButtonMaterial_Unpressed;
        public static Material BaseButtonMaterial_Pressed;
        public static Material BaseLeverMaterial_Up;
        public static Material BaseLeverMaterial_Down;

        public static GameLever leverPrefab;

        static FieldInfo buttonPressedF = AccessTools.Field(typeof(GameButton), "pressed");
        static FieldInfo buttonUnpressedF = AccessTools.Field(typeof(GameButton), "unPressed");
        static FieldInfo buttonMeshRenderer = AccessTools.Field(typeof(GameButton), "meshRenderer");

        static FieldInfo leverMeshRenderer = AccessTools.Field(typeof(GameLever), "meshRenderer");
        static FieldInfo leverOffMat = AccessTools.Field(typeof(GameLever), "offMat");
        static FieldInfo leverOnMat = AccessTools.Field(typeof(GameLever), "onMat");

        public static void ApplyButtonMaterials(GameButton applyTo, ButtonMaterials toApply)
        {
            MeshRenderer mr = ((MeshRenderer)buttonMeshRenderer.GetValue(applyTo));
            Material oldPressed = (Material)buttonPressedF.GetValue(applyTo);
            buttonPressedF.SetValue(applyTo, toApply.buttonPressed);
            buttonUnpressedF.SetValue(applyTo, toApply.buttonUnpressed);
            mr.sharedMaterial = (mr.sharedMaterial == oldPressed ? toApply.buttonPressed : toApply.buttonUnpressed);
        }

        public static void ApplyButtonMaterials(GameButton applyTo, string colorName)
        {
            ApplyButtonMaterials(applyTo, buttonColors[colorName]);
        }

        public static void ApplyLeverMaterials(GameLever applyTo, ButtonMaterials toApply)
        {
            // why is the lever down mat the off mat? that's weird but. whatever
            MeshRenderer mr = ((MeshRenderer)leverMeshRenderer.GetValue(applyTo));
            Material oldOff = (Material)leverOffMat.GetValue(applyTo);
            leverOnMat.SetValue(applyTo, toApply.leverUp);
            leverOffMat.SetValue(applyTo, toApply.leverDown);
            mr.sharedMaterial = (mr.sharedMaterial == oldOff ? toApply.leverDown : toApply.leverUp);
        }

        public static void ApplyLeverMaterials(GameLever applyTo, string colorName)
        {
            ApplyLeverMaterials(applyTo, buttonColors[colorName]);
        }

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

        public static void AddRed()
        {
            if (_buttonColors.Count != 0) throw new Exception("AddRed called twice!");
            _buttonColors.Add("Red", new ButtonMaterials()
            {
                buttonUnpressed = BaseButtonMaterial_Unpressed,
                buttonPressed = BaseButtonMaterial_Pressed,
                leverUp = BaseLeverMaterial_Up,
                leverDown = BaseLeverMaterial_Down,
                color = new Color(1f, 0f, 0f, 0f),
                name = "Red"
            });
        }

        public static ButtonMaterials CreateButtonMaterial(string key, Color color)
        {
            if (buttonColors.ContainsKey(key))
            {
                Debug.LogWarningFormat("Attempted to add already existing button color: {0}!", key);
                return buttonColors[key];
            }
            Material leverUpMaterial = new Material(BaseLeverMaterial_Up);
            Material leverDownMaterial = new Material(BaseLeverMaterial_Down);
            Material pressedMaterial = new Material(BaseButtonMaterial_Pressed);
            Material unpressedMaterial = new Material(BaseButtonMaterial_Unpressed);
            color = new Color(color.r, color.g, color.b, 0f); //make sure alpha is 0
            // button material creation
            pressedMaterial.name = String.Format("Button_{0}_Pressed", key);
            pressedMaterial.SetColor("_TextureColor", color);
            unpressedMaterial.name = String.Format("Button_{0}_Unpressed", key);
            unpressedMaterial.SetColor("_TextureColor", color);
            // lever material creation
            leverUpMaterial.name = String.Format("Lever_{0}_Up", key);
            leverUpMaterial.SetColor("_TextureColor", color);
            leverDownMaterial.name = String.Format("Lever_{0}_Down", key);
            leverDownMaterial.SetColor("_TextureColor", color);
            ButtonMaterials newBut = new ButtonMaterials()
            {
                buttonPressed = pressedMaterial,
                buttonUnpressed = unpressedMaterial,
                color = color,
                name = key,
                leverUp = leverUpMaterial,
                leverDown = leverDownMaterial
            };
            buttonColors.Add(key, newBut);
            return newBut;
        }

    }

    [HarmonyPatch(typeof(NameManager))]
    [HarmonyPatch("Awake")]
    class NameAwakePatch
    {
        static void Prefix()
        {
            List<Material> materials = Resources.FindObjectsOfTypeAll<Material>().ToList();
            ButtonsPlusPlugin.BaseButtonMaterial_Unpressed = materials.Find(x => x.name == "Button_Red_Unpressed");
            ButtonsPlusPlugin.BaseButtonMaterial_Pressed = materials.Find(x => x.name == "Button_Red_Pressed");
            ButtonsPlusPlugin.BaseLeverMaterial_Down = materials.Find(x => x.name == "Lever_Red_Down");
            ButtonsPlusPlugin.BaseLeverMaterial_Up = materials.Find(x => x.name == "Lever_Red_Up");
            // make sure we crash HERE if any of these are null(makes things easier to debug if mystman12 renames these)
            Assert.IsNotNull(ButtonsPlusPlugin.BaseButtonMaterial_Unpressed);
            Assert.IsNotNull(ButtonsPlusPlugin.BaseButtonMaterial_Pressed);
            Assert.IsNotNull(ButtonsPlusPlugin.BaseLeverMaterial_Down);
            Assert.IsNotNull(ButtonsPlusPlugin.BaseLeverMaterial_Up);
            // handle all the basic colors people may want so we dont have a million mods trying to create the same colors
            ButtonsPlusPlugin.AddRed();
            ButtonsPlusPlugin.CreateButtonMaterial("Orange", new Color(1f, 1f, 0f));
            ButtonsPlusPlugin.CreateButtonMaterial("Yellow", new Color(1f, 1f, 0f));
            ButtonsPlusPlugin.CreateButtonMaterial("Green", Color.green);
            ButtonsPlusPlugin.CreateButtonMaterial("Cyan", new Color(0f, 1f, 1f));
            ButtonsPlusPlugin.CreateButtonMaterial("Blue", Color.blue);
            ButtonsPlusPlugin.CreateButtonMaterial("Purple", new Color(0.5f, 1f, 0f));
            ButtonsPlusPlugin.CreateButtonMaterial("Magenta", new Color(1f, 0f, 1f));
            ButtonsPlusPlugin.CreateButtonMaterial("Pink", new Color(1f, 0.5f, 1f));
            ButtonsPlusPlugin.CreateButtonMaterial("White", Color.white);
            // assign the lever prefab for easy use
            ButtonsPlusPlugin.leverPrefab = Resources.FindObjectsOfTypeAll<GameLever>().First();
            Assert.IsNotNull(ButtonsPlusPlugin.leverPrefab);
        }
    }
}
