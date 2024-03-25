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
    //advanced accuracy patches
    [HarmonyPatch(typeof(ShotReport), "HitReportFor")]
    public static class ShotReport_HitReportFor_Patch
    {
        public static bool AAccuracy;
        public static float AccScale;
        public static Thing shotCaster;
        public static Verb shotVerb;
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodBase from = AccessTools.Method(typeof(VerbProperties), "GetHitChanceFactor");
            MethodBase from1 = AccessTools.PropertyGetter(typeof(WeatherManager), "CurWeatherAccuracyMultiplier");
            MethodBase to = AccessTools.Method(typeof(ShotReport_HitReportFor_Patch), "statBump");

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand as MethodBase == from || instruction.operand as MethodBase == from1)//find method to replace
                {
                    yield return instruction;//add first instruction back
                    yield return new CodeInstruction(OpCodes.Ldarg_0);//load in next argument
                    yield return new CodeInstruction(OpCodes.Call, to);//execute new method
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        public static bool Prefix(Thing caster, Verb verb)
        {
            shotCaster = caster;
            shotVerb = verb;
            return true;
        }
        public static float statBump(float factor, Thing caster)
        {
            if (AAccuracy)
            {
                float shootstat = Mathf.Max(1, ((caster is Pawn) ? StatDefOf.ShootingAccuracyPawn.Worker.GetValue(StatRequest.For(caster), false) : StatDefOf.ShootingAccuracyTurret.Worker.GetValue(StatRequest.For(caster), false)) / AccScale);//caster.GetStatValue(StatDefOf.ShootingAccuracyPawn, false): caster.GetStatValue(StatDefOf.ShootingAccuracyTurret,false));
                factor = Mathf.Pow(factor, 1 / shootstat);
            }
            return factor;
        }
        public static float statpush(float factor, Thing caster)
        {
            if (VanillaCombatMod.settings.Flanking)
            {
                float shootstat = Mathf.Max(1, ((caster is Pawn) ? StatDefOf.ShootingAccuracyPawn.Worker.GetValue(StatRequest.For(caster), false) : StatDefOf.ShootingAccuracyTurret.Worker.GetValue(StatRequest.For(caster), false)) / AccScale);//caster.GetStatValue(StatDefOf.ShootingAccuracyPawn, false): caster.GetStatValue(StatDefOf.ShootingAccuracyTurret,false));
                factor = 1-Mathf.Pow(1-factor, shootstat);
            }
            return factor;
        }
        public static float statpushMelee(float factor, Thing caster)
        {
            if (VanillaCombatMod.settings.Flanking)
            {
                float shootstat = Mathf.Max(1, (StatDefOf.MeleeHitChance.Worker.GetValue(StatRequest.For(caster), false)) / AccScale);//caster.GetStatValue(StatDefOf.ShootingAccuracyPawn, false): caster.GetStatValue(StatDefOf.ShootingAccuracyTurret,false));
                factor = 1 - Mathf.Pow(1 - factor, shootstat);
            }
            return factor;
        }
    }


    //firing arc patches
    [HarmonyPatch(typeof(ShootLine), "ChangeDestToMissWild")]
    public static class ShootLine_ChangeDestToMissWild_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodBase from = AccessTools.Method(typeof(SimpleSurface), "Evaluate");
            MethodBase to = AccessTools.Method(typeof(ShootLine_ChangeDestToMissWild_Patch), "arcAdjust");
            FieldInfo field1 = AccessTools.Field(typeof(ShootLine), "dest");
            FieldInfo field2 = AccessTools.Field(typeof(ShootLine), "source");

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand as MethodBase == from)//find method to replace
                {
                    yield return instruction;//add first instruction back
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field1);//load in next argument
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, field2);
                    yield return new CodeInstruction(OpCodes.Call, to);//execute new method
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static float arcAdjust(float num, IntVec3 dest, IntVec3 source)
        {
            if (!VanillaCombatMod.settings.UseFiringArc)
            {
                return num;
            }
            float angle = VanillaCombatMod.settings.FiringArc*Mathf.PI/360;
            float num2 = Vector3.Magnitude((dest - source).ToVector3()) * Mathf.Tan(angle);
            switch (VanillaCombatMod.settings.ArcType)
            {
                case 0:
                    return num * num2 / 10f; //initial scales, scales with distance
                case 1:
                    return Mathf.Min(num * num2 / 10f, 10f);//initial scales, final value capped and scales with distance
                case 2:
                    return num * Mathf.Min(num2 / 10f, 1f);//initial scales with distance, final does not scale with distance
                case 3:
                    return Mathf.Max(Mathf.Min(num, num2), num * num2 / 10f);//initial capped, final scales with distance
                case 4:
                    return Mathf.Max(Mathf.Min(num, num2), Mathf.Min(num * num2 / 10f, 10f));// initial capped, final value capped and scales with distance
                case 5:
                    return Mathf.Min(num, num2); //initial capped, does not scale with distance
                default:
                    return num;
            }
        }
    }

}
