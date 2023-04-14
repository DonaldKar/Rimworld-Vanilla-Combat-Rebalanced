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

namespace VCR
{
    [HarmonyPatch]
    public static class PawnRenderer_DrawApparel_Patch
    {
        public static bool setting = false;
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var targetMethod = typeof(PawnRenderer).GetNestedTypes(AccessTools.all).SelectMany(innerType => AccessTools.GetDeclaredMethods(innerType))
                    .FirstOrDefault(method => method.Name.Contains("DrawApparel") && method.ReturnType == typeof(void) && method.GetParameters().Length == 1);
            yield return targetMethod;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo from = typeof(PawnRenderer).GetNestedTypes(AccessTools.all).SelectMany(innerType => AccessTools.GetDeclaredFields(innerType))
                    .FirstOrDefault(field => field.Name.Contains("onHeadLoc"));

            MethodBase to = AccessTools.Method(typeof(PawnRenderer_DrawApparel_Patch), "notFrontOfFaceOffset");
            MethodBase to2 = AccessTools.Method(typeof(PawnRenderer_DrawApparel_Patch), "FrontOfFaceOffset");
            bool first = true;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode==OpCodes.Ldfld && instruction.operand as FieldInfo == from && first == true)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, to);
                    first = false;
                    continue;
                }
                if (instruction.opcode==OpCodes.Ldc_R4 && instruction.OperandIs(0.03185328f))
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, to2);
                    continue;
                }
                yield return instruction;
            }
        }
        public static Vector3 notFrontOfFaceOffset(Vector3 vect, ApparelGraphicRecord apparelRecord)
        {
            if (setting && !apparelRecord.sourceApparel.def.apparel.hatRenderedAboveBody)
            {
                vect.y += 0.00289575267f;
            }
            return vect;
        }
        public static float FrontOfFaceOffset(float f, ApparelGraphicRecord apparelRecord)
        {
            if (setting && apparelRecord.sourceApparel.def.apparel.forceRenderUnderHair)
            {
                f = 0.0289575271f;
            }
            return f;
        }
    }
}