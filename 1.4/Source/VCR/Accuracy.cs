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
    }
}
