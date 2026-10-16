using System.Collections.Generic;
using System.Reflection;
using Harmony;
using MSCLoader;
using OASIS;

namespace BoltSizeIndicatorOASIS {
    [HarmonyPatch(typeof(Bolt))]
    [HarmonyPatch("tryResetMaterial")]
    internal class OASIS_Bolt_tryResetMaterial_Patch {
        private static readonly HashSet<Bolt> CannotBeBolted = new HashSet<Bolt>();
        private static readonly FieldInfo CanBeBolted = typeof(Bolt).GetField("canBeBolted", BindingFlags.Instance | BindingFlags.NonPublic);

        private static bool Prefix(Bolt __instance) {
            if (__instance.isMouseOver) {
                if (!BoltSizeIndicator.BoltSizeIndicator.HoldingRatchet || __instance.canUseRatchet) {
                    if (CannotBeBolted.Add(__instance)) {
                        CanBeBolted.SetValue(__instance, false);
                        __instance.materialCache = __instance.renderer.sharedMaterial;
                        __instance.renderer.material = BoltSizeIndicator.BoltSizeIndicator.HeldToolSize < __instance.size ? BoltSizeIndicator.BoltSizeIndicator.BigBoltMaterial : BoltSizeIndicator.BoltSizeIndicator.SmallBoltMaterial;
                    }
                    return false;
                }
            }

            if (CannotBeBolted.Remove(__instance)) {
                __instance.renderer.material = __instance.materialCache;
                __instance.materialCache = null;

                return false;
            }

            return true;
        }
    }

    public class BoltSizeIndicatorOASIS : Mod {
        public override string ID => "BoltSizeIndicatorOASIS"; // Your (unique) mod ID 
        public override string Name => "Bolt Size Indicator Support for OASIS"; // Your mod name
        public override string Author => "particleicicle"; // Name of the Author (your name)
        public override string Version => "1.0.1"; // Version
        public override string Description => "Adds support for OASIS bolts to Bolt Size Indicator."; // Short description of your mod
        public override Game SupportedGames =>  Game.MySummerCar_And_MyWinterCar;

        public override void ModSetup() {
            SetupFunction(Setup.PostLoad, Mod_PostLoad);
            SetupFunction(Setup.OnModDisabled, Mod_Disable);
        }

        private void Mod_Disable() {
            if (!harmonyPatched || ModLoader.CurrentScene != CurrentScene.MainMenu) return;

            harmony?.UnpatchAll(ID);
            harmonyPatched = false;
        }

        private static HarmonyInstance harmony;
        private static bool harmonyPatched;

        private void Mod_PostLoad() {
            if (!ModLoader.IsModPresent("BoltSizeIndicator")) {

                if (harmonyPatched) {
                    harmony?.UnpatchAll(ID);
                    harmonyPatched = false;
                }

                return;
            }

            if (!ModLoader.IsReferencePresent("OASIS") || harmonyPatched) return;

            if (harmony == null) harmony = HarmonyInstance.Create(ID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmonyPatched = true;
        } 
    }
}
