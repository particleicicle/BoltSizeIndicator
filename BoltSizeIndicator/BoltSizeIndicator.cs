using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using UnityEngine;

namespace BoltSizeIndicator {
    public class BoltSizeIndicator : Mod {
        public override string ID => "BoltSizeIndicator"; // Your (unique) mod ID 
        public override string Name => "Bolt Size Indicator"; // Your mod name
        public override string Author => "particleicicle"; // Name of the Author (your name)
        public override string Version => "1.1.2"; // Version
        public override string Description => "Indicates if the selected tool is the wrong size with a different color."; // Short description of your mod
        public override Game SupportedGames => Game.MySummerCar_And_MyWinterCar;

        public override void ModSetup() {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        private static readonly FsmBool playerHasRatchet = PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerHasRatchet");
        private static readonly FsmFloat toolWrenchSize = PlayMakerGlobals.Instance.Variables.FindFsmFloat("ToolWrenchSize");

        public static bool HoldingRatchet {
            get => playerHasRatchet.Value;
        }
        public static float HeldToolSize {
            get => toolWrenchSize.Value;
        }

        private static Material _smallBoltMat;
        private static Material _bigBoltMat;

        public static Material SmallBoltMaterial => _smallBoltMat;
        public static Material BigBoltMaterial => _bigBoltMat;

        private void Mod_OnLoad() {
            // Called once, when mod is loading after game is fully loaded

            var playerCam = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("SavePlayerCam").Value.transform;

            var spannerBoltCheckFSM = playerCam.Find("2Spanner/Raycast").GetPlayMaker("Check");

            spannerBoltCheckFSM.enabled = false;

            Shader shader = Shader.Find("GUI/Text Shader");

            if (_bigBoltMat == null) {
                _bigBoltMat = new Material(shader) {
                    name = "bigbolt",
                    color = Color.red
                };
            }

            if (_smallBoltMat == null) {
                _smallBoltMat = new Material(shader) {
                    name = "smallbolt",
                    color = Color.yellow
                };
            }  

            var stateToolTooBig = spannerBoltCheckFSM.GetState("State 1");

            stateToolTooBig.Actions[0].Enabled = true;
            stateToolTooBig.Actions[1].Enabled = true;
            ((SetMaterial)stateToolTooBig.Actions[1]).material = new FsmMaterial() { Value = _bigBoltMat };
            stateToolTooBig.Transitions[0].ToState = "Wait";

            var stateToolTooSmall = spannerBoltCheckFSM.GetState("State 2");

            var boltGameObject = spannerBoltCheckFSM.FsmVariables.FindFsmGameObject("Bolt");
            var boltFsmOwnerDefault = new FsmOwnerDefault() { GameObject = boltGameObject, OwnerOption = OwnerDefaultOption.SpecifyGameObject };

            if (stateToolTooSmall == null) {
                var meshGameObject = spannerBoltCheckFSM.FsmVariables.FindFsmGameObject("Mesh");
                var meshFsmOwnerDefault = new FsmOwnerDefault() { GameObject = meshGameObject, OwnerOption = OwnerDefaultOption.SpecifyGameObject };

                stateToolTooSmall = spannerBoltCheckFSM.AddState("State 2");
                stateToolTooSmall.AddAction(new GetChild() {
                    Fsm = spannerBoltCheckFSM.Fsm,
                    childName = new FsmString(),
                    gameObject = boltFsmOwnerDefault,
                    withTag = new FsmString() { Value = "Untagged" },
                    storeResult = meshGameObject
                });
                stateToolTooSmall.AddAction(new SetMaterial() {
                    Fsm = spannerBoltCheckFSM.Fsm,
                    gameObject = meshFsmOwnerDefault,
                    material = new FsmMaterial() { Value = _smallBoltMat },
                    materialIndex = new FsmInt() { Value = 0 }
                });
                stateToolTooSmall.AddTransition("FINISHED", "Wait");
            }
            else {
                stateToolTooSmall.Actions[0].Enabled = true;
                stateToolTooSmall.Actions[1].Enabled = true;
                ((SetMaterial)stateToolTooSmall.Actions[1]).material = new FsmMaterial() { Value = _smallBoltMat };
                stateToolTooSmall.Transitions[0].ToState = "Wait";
            }

            //for MWC where the transition from the YELLOW-event no longer goes to State 2 as it was removed
            var checkState = spannerBoltCheckFSM.GetState("Check tool");

            if (checkState != null) {
                var trans = checkState.Transitions.FirstOrDefault(x => x.EventName == "YELLOW");
                if (trans != null && trans.ToState != stateToolTooSmall.Name) trans.ToState = stateToolTooSmall.Name;
            }

            var waitForBoltChange = spannerBoltCheckFSM.AddState("Wait");
            waitForBoltChange.AddTransition(FsmEvent.Finished.Name, "Set Material back");
            waitForBoltChange.AddAction(new GameObjectCompare
            {
                gameObjectVariable = boltFsmOwnerDefault,
                everyFrame = false,
                compareTo = new FsmGameObject() { Value = null },
                equalEvent = FsmEvent.Finished,
                notEqualEvent = null,
                storeResult = new FsmBool()
            });
            waitForBoltChange.AddAction(new GameObjectChanged
            {
                changedEvent = FsmEvent.Finished,
                gameObjectVariable = boltGameObject,
                storeResult = new FsmBool()
            });
            /*
            waitForBoltChange.AddAction(new FloatChanged
            {
                floatVariable = PlayMakerGlobals.Instance.Variables.FindFsmFloat("ToolWrenchSize"),
                changedEvent = FsmEvent.Finished,
                storeResult = new FsmBool()
            });
            */

            spannerBoltCheckFSM.enabled = true;

        }
    }
}
