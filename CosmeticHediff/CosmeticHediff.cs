using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace CosmeticHediff {
    [StaticConstructorOnStartup]
    public class CosmeticHediff {
        static CosmeticHediff() {
            Log.Message("[CosmeticHediff] Now active");

            var harmony = new Harmony("kaitorisenkou.CosmeticHediff");
            //ManualPatch(harmony);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("[CosmeticHediff] Harmony patch complete!");
        }
    }


    [HarmonyPatch(typeof(PawnRenderer), "DrawBodyApparel")]
    public static class Patch_DrawBodyApparel {
        [HarmonyPrefix]
        static void Prefix(Vector3 shellLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags,Pawn ___pawn) {
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            foreach(var i in ___pawn.health.hediffSet.hediffs) {
                var comp = i.TryGetComp<HediffComp_Cosmetic>();
                if (comp == null) continue;
                var props = comp.PropsCosmetic;
                if ((byte)props.layer > 1) continue;
                Vector3 loc = shellLoc;
                if (props.layer == CosmeticHediffLayer.skin) {
                    loc.y += skinHeight;
                } else {
                    loc.y += apparelHeight;
                }
                if (bodyFacing==Rot4.North) {
                    loc.y += northHeight;
                }
                var graphic = comp.GetGraphic();
                Material material = graphic.MatAt(bodyFacing, null);
                GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
            }
        }
        const float skinHeight = -0.011f;
        const float apparelHeight = 0f;
        const float northHeight = -0.003f;
    }
    [HarmonyPatch(typeof(PawnRenderer), "DrawHeadHair")]
    public static class Patch_DrawHeadHair {
        [HarmonyPrefix]
        static void Prefix(Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, bool bodyDrawn,Pawn ___pawn, PawnGraphicSet ___graphics) {
            hideHairInt = false;
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 locBase = rootLoc + headOffset;
            Mesh mesh = ___graphics.HairMeshSet.MeshAt(headFacing);
            ___pawn.health.hediffSet.hediffs.Select(t => t.TryGetComp<HediffComp_Cosmetic>()).Where(t => t != null);
            foreach (var i in ___pawn.health.hediffSet.hediffs) {
                var comp = i.TryGetComp<HediffComp_Cosmetic>();
                if (comp == null) continue;
                var props = comp.PropsCosmetic;
                if ((byte)props.layer < 2) continue;
                Vector3 loc = locBase;
                if (props.layer == CosmeticHediffLayer.head) {
                    loc.y += headHeight;
                } else {
                    loc.y += overheadHeight;
                }
                var graphic = comp.GetGraphic();
                Material material = graphic.MatAt(bodyFacing, null);
                GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
                if (props.hideHair) hideHairInt = true;
            }
        }

        static bool hideHairInt = false;
        const float headHeight = 0.0285f;
        const float overheadHeight = 0.035f;

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var instructionList = instructions.ToList();
            int stage = 0;
            for (int i = 0; i < instructionList.Count; i++) {
                if (instructionList[i].opcode == OpCodes.Ldloc_2) {
                    instructionList.Insert(
                        i + 1,
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_DrawHeadHair), nameof(AndHHI)))
                    );
                    stage++;
                    break;
                }
            }
            if (stage < 1) {
                Log.Error("[CosmeticHediff] Patch_DrawHeadHair failed (stage:" + stage + ")");
            }
            return instructionList;
        }
        public static bool AndHHI(bool flag3) {
            return flag3 && !hideHairInt;
        }
    }
}
