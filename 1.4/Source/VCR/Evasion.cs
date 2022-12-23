using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VCR
{
    //evasion patches
    [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_IgnoringPosture", MethodType.Getter)]
    public static class ShotReport_AimOnTargetChance_IgnoringPosture_Patch
    {
        public static bool Eva;
        public static float baseEvasion;
        public static float minSpeed;
        public static bool EvaAcc;
        public static float CurEvasion = 1;
        public static float FinalEvasion = 1;
        //add into calc
        public static void Postfix(ref float __result, TargetInfo ___target)
        {
            if (!Eva)
            {
                CurEvasion = 1;
                return;
            }
            if (___target.HasThing && ___target.Thing is Pawn p)
            {
                FinalEvasion = Evasion(p, ShotReport_HitReportFor_Patch.shotCaster);
                __result *= FinalEvasion;
            }
        }
        //calc evasion chance
        public static float Evasion(Pawn targetPawn, Thing caster)
        {

            CurEvasion = (float)Math.Pow(baseEvasion, Math.Max(Velocity(targetPawn) - minSpeed, 0));
            if (!EvaAcc)
            {
                return CurEvasion;
            }
            return ShotReport_HitReportFor_Patch.statBump(CurEvasion, caster);
        }
        //calc exact velocity
        public static float Velocity(Pawn pawn)
        {
            float v = 0;
            if (pawn != null && pawn.pather.MovingNow)
            {
                float path = pawn.pather.nextCellCostTotal;
                float num = 1f;
                if (pawn.stances.stagger.Staggered)
                {
                    num *= pawn.stances.stagger.StaggerMoveSpeedFactor;
                }
                if (num < path / 450f)
                {
                    num = path / 450f;
                }
                v = 60 * num / path;
            }
            return v;
        }
    }
    //custom readout
    [HarmonyPatch(typeof(ShotReport), "GetTextReadout")]
    public static class ShotReport_GetTextReadout_Patch
    {
        public static bool Prefix(ref string __result, ShotReport __instance, TargetInfo ___target, float ___distance, List<CoverInfo> ___covers,
                                    float ___factorFromShooterAndDist, float ___factorFromEquipment, float ___factorFromTargetSize, float ___factorFromWeather,
                                    float ___forcedMissRadius, float ___offsetFromDarkness, float ___factorFromCoveringGas,ShootLine ___shootLine)
        {
            float angle = Quaternion.LookRotation((___shootLine.Dest.ToVector3() - ___shootLine.Source.ToVector3()).Yto0()).eulerAngles.y;
            //Log.Message(angle.ToString());
            if (ShotReport_HitReportFor_Patch.AAccuracy || ShotReport_AimOnTargetChance_IgnoringPosture_Patch.Eva||VanillaCombatMod.settings.Flanking)
            {
                __result = CustomTextReadout(__instance, ___target, ___distance, ___covers,
                                    ___factorFromShooterAndDist, ___factorFromEquipment, ___factorFromTargetSize, ___factorFromWeather,
                                    ___forcedMissRadius, ___offsetFromDarkness, ___factorFromCoveringGas, FactorFromPosture(___target, ___distance),
                                    FactorFromExecution(___target, ___distance), angle);
                return false;
            }
            return true;
        }
        public static float temp;
        public static float temp2;

        private static float FactorFromPosture(TargetInfo target, float distance)
        {
            if (target.HasThing && target.Thing is Pawn p && distance >= 4.5f && p.GetPosture() != 0)
            {
                return 0.2f;
            }
            return 1f;
        }

        public static float FactorFromExecution(TargetInfo target, float distance)
        {
            if (target.HasThing && target.Thing is Pawn p && distance <= 3.9f && p.GetPosture() != 0)
            {
                return 7.5f;
            }
            return 1f;
        }

        public static string CustomTextReadout(ShotReport instance, TargetInfo target, float distance, List<CoverInfo> covers,
                                    float factorFromShooterAndDist, float factorFromEquipment, float factorFromTargetSize, float factorFromWeather,
                                    float forcedMissRadius, float offsetFromDarkness, float factorFromCoveringGas, float FactorFromPosture,
                                    float FactorFromExecution, float angle)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (forcedMissRadius > 0.5f)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("WeaponMissRadius".Translate() + ": " + forcedMissRadius.ToString("F1"));
                stringBuilder.AppendLine("DirectHitChance".Translate() + ": " + (1f / (float)GenRadial.NumCellsInRadius(forcedMissRadius)).ToStringPercent());
            }
            else
            {
                temp = ShotReport_HitReportFor_Patch.shotVerb.verbProps.GetHitChanceFactor(ShotReport_HitReportFor_Patch.shotVerb.EquipmentSource, distance);
                stringBuilder.AppendLine(instance.TotalEstimatedHitChance.ToStringPercent());
                stringBuilder.AppendLine("   " + "ShootReportShooterAbility".Translate() + ": " + factorFromShooterAndDist.ToStringPercent());
                stringBuilder.AppendLine("   " + "ShootReportWeapon".Translate() + ": " + temp.ToStringPercent());
                if (temp != factorFromEquipment)
                {
                    stringBuilder.AppendLine("      " + "VCR.AdjWeapon".Translate() + ": " + factorFromEquipment.ToStringPercent());

                }

                if (target.HasThing && factorFromTargetSize != 1f)
                {
                    stringBuilder.AppendLine("   " + "TargetSize".Translate() + ": " + factorFromTargetSize.ToStringPercent());
                }
                temp2 = ShotReport_HitReportFor_Patch.shotCaster.Map.weatherManager.CurWeatherAccuracyMultiplier;

                if (temp2 < 0.99f)
                {
                    stringBuilder.AppendLine("   " + "Weather".Translate() + ": " + temp2.ToStringPercent());
                    if (temp2 != factorFromWeather)
                    {
                        stringBuilder.AppendLine("      " + "VCR.AdjWeather".Translate() + ": " + factorFromWeather.ToStringPercent());
                    }
                }
                if (factorFromCoveringGas < 0.99f)
                {
                    stringBuilder.AppendLine("   " + "BlindSmoke".Translate().CapitalizeFirst() + ": " + factorFromCoveringGas.ToStringPercent());
                }
                if (FactorFromPosture < 0.9999f)
                {
                    stringBuilder.AppendLine("   " + "TargetProne".Translate() + ": " + FactorFromPosture.ToStringPercent());
                }
                if (FactorFromExecution != 1f)
                {
                    stringBuilder.AppendLine("   " + "Execution".Translate() + ": " + FactorFromExecution.ToStringPercent());
                }
                if (ModsConfig.IdeologyActive && target.HasThing && offsetFromDarkness != 0f)
                {
                    if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
                    {
                        stringBuilder.AppendLine("   " + StatDefOf.ShootingAccuracyOutdoorsLitOffset.LabelCap + ": " + offsetFromDarkness.ToStringPercent());
                    }
                    else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
                    {
                        stringBuilder.AppendLine("   " + StatDefOf.ShootingAccuracyOutdoorsDarkOffset.LabelCap + ": " + offsetFromDarkness.ToStringPercent());
                    }
                    else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
                    {
                        stringBuilder.AppendLine("   " + StatDefOf.ShootingAccuracyIndoorsDarkOffset.LabelCap + ": " + offsetFromDarkness.ToStringPercent());
                    }
                    else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
                    {
                        stringBuilder.AppendLine("   " + StatDefOf.ShootingAccuracyIndoorsLitOffset.LabelCap + "   " + offsetFromDarkness.ToStringPercent());
                    }
                }
                float eva1 = ShotReport_AimOnTargetChance_IgnoringPosture_Patch.CurEvasion;
                float eva2 = ShotReport_AimOnTargetChance_IgnoringPosture_Patch.FinalEvasion;
                if (eva1 < 0.99f)
                {
                    stringBuilder.AppendLine("   " + "VCR.EvasionRead".Translate() + ": " + eva1.ToStringPercent());
                    if (eva1 != eva2)
                    {
                        stringBuilder.AppendLine("      " + "VCR.AdjEvasionRead".Translate() + ": " + eva2.ToStringPercent());
                    }
                }

                if (instance.PassCoverChance < 1f)
                {
                    stringBuilder.AppendLine("   " + "ShootingCover".Translate() + ": " + instance.PassCoverChance.ToStringPercent());
                    for (int i = 0; i < covers.Count; i++)
                    {
                        CoverInfo coverInfo = covers[i];
                        if (coverInfo.BlockChance > 0f)
                        {
                            stringBuilder.AppendLine("     " + "CoverThingBlocksPercentOfShots".Translate(coverInfo.Thing.LabelCap, coverInfo.BlockChance.ToStringPercent(), new NamedArgument(coverInfo.Thing.def, "COVER")).CapitalizeFirst());
                        }
                    }
                }
                else
                {
                    stringBuilder.AppendLine("   (" + "NoCoverLower".Translate() + ")");
                }
                if (VanillaCombatMod.settings.Flanking)
                {
                    BodyPartGroupDef side = Flanking.Side(angle, target.Thing);
                    if (side != null)
                    {
                        stringBuilder.AppendLine("   " + "VCR.TargetSide".Translate() + ": " + side.LabelShort);
                    }
                    BodyPartHeight height = ShotReport_HitReportFor_Patch.shotCaster.GetTargetHeight();
                    if (height != BodyPartHeight.Undefined && target.Thing is Pawn)
                    {
                        stringBuilder.AppendLine("   " + "VCR.HeightChance".Translate() + height.ToStringHuman() + ": " + Flanking.ChanceWithPawn(ShotReport_HitReportFor_Patch.shotCaster, (Pawn)target.Thing, side, DamageDefOf.Bullet, height).ToStringPercent());
                    }
                }
            }
            return stringBuilder.ToString();
        }
    }
}
