using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse.AI.Group;
using MonoMod.Utils;
using Verse.Sound;
using System.Xml;

namespace VCR
{
    [StaticConstructorOnStartup]
    public static class VCR
    {
        static VCR()
        {

            //Harmony.DEBUG = true;
            new Harmony("VCR.Mod").PatchAll();
            SetXmlSettings();
            ApplySettings();
        }
        public static List<string> XmlSettings = new List<string>();
        public static void SetXmlSettings()
        {
            if (VanillaCombatMod.settings.HandFeetPatch)
            {
                XmlSettings.Add("HandFeetPatch");
            }
            if (VanillaCombatMod.settings.GlassesHelmetPatch)
            {
                XmlSettings.Add("GlassesHelmetPatch");
            }
        }
        public static void ApplySettings()
        {
            ArmorUtility_ApplyArmor_Patch.AArmor = VanillaCombatMod.settings.AdvancedArmor;
            ArmorUtility_ApplyArmor_Patch.AArmorScale = VanillaCombatMod.settings.ArmorScale;
            AdvanceArmor_postfix.APScale = VanillaCombatMod.settings.PenetrationScale;
            ShotReport_HitReportFor_Patch.AAccuracy = VanillaCombatMod.settings.AdvancedAccuracy;
            ShotReport_HitReportFor_Patch.AccScale = VanillaCombatMod.settings.AccuracyScale;
            ShotReport_AimOnTargetChance_StandardTarget_Patch.Eva = VanillaCombatMod.settings.Evasion;
            ShotReport_AimOnTargetChance_StandardTarget_Patch.baseEvasion = VanillaCombatMod.settings.EvasionScale;
            ShotReport_AimOnTargetChance_StandardTarget_Patch.minSpeed = VanillaCombatMod.settings.minSpeed;
            ShotReport_AimOnTargetChance_StandardTarget_Patch.EvaAcc = VanillaCombatMod.settings.EvaAcc;
            DamageWorker_Bullet.active = VanillaCombatMod.settings.BulletsWorker;
            DamageWorker_Arrow.active = VanillaCombatMod.settings.ArrowsWorker;
        }
    }
    //settings
    public class VanillaCombatMod : Mod
    {
        public static VanillaCombatSettings settings;
        public VanillaCombatMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<VanillaCombatSettings>();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
            VCR.ApplySettings();
        }
    }
    public class VanillaCombatSettings : ModSettings
    {
        public bool AdvancedArmor = false;
        public float ArmorScale = 2f;
        public string armtemp = "2";
        public float PenetrationScale = 2f;
        public string pentemp = "2";

        public bool AdvancedAccuracy = false;
        public float AccuracyScale = 5f;
        public string acctemp = "5";

        public bool Evasion = false;
        public float EvasionScale = 0.5f;
        public string evatemp = "0.5";
        public float minSpeed = 2.5f;
        public string minSpeedtemp = "2.5";
        public bool EvaAcc = true;

        public bool HandFeetPatch = false;
        public bool BulletsWorker = false;
        public bool ArrowsWorker = false;
        public bool GlassesHelmetPatch = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AdvancedArmor, "AdvancedArmor", false);
            Scribe_Values.Look(ref ArmorScale, "ArmorScale", 2f);
            Scribe_Values.Look(ref armtemp, "armtemp", "2");
            Scribe_Values.Look(ref PenetrationScale, "PenetrationScale", 2f);
            Scribe_Values.Look(ref pentemp, "pentemp", "2");
            Scribe_Values.Look(ref AdvancedAccuracy, "AdvancedAccuracy", false);
            Scribe_Values.Look(ref AccuracyScale, "AccuracyScale", 5f);
            Scribe_Values.Look(ref acctemp, "acctemp", "5");
            Scribe_Values.Look(ref Evasion, "Evasion", false);
            Scribe_Values.Look(ref EvasionScale, "EvasionScale", 0.5f);
            Scribe_Values.Look(ref evatemp, "evatemp", "0.5");
            Scribe_Values.Look(ref minSpeed, "minSpeed", 2.5f);
            Scribe_Values.Look(ref minSpeedtemp, "minSpeedtemp", "2.5");
            Scribe_Values.Look(ref EvaAcc, "EvaAcc", true);
            Scribe_Values.Look(ref HandFeetPatch, "HandFeetPatch", false);
            Scribe_Values.Look(ref BulletsWorker, "BulletsWorker", false);
            Scribe_Values.Look(ref ArrowsWorker, "ArrowsWorker", false);
            Scribe_Values.Look(ref GlassesHelmetPatch, "GlassesHelmetPatch", false);
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.GapLine();
            listingStandard.Label("VCR.generaltab".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.AdvanceArmor".Translate(), ref AdvancedArmor, "VCR.AAtooltip".Translate());
            listingStandard.GapLine();
            var value = ArmorScale;
            listingStandard.SliderLabeled("VCR.ArmorScale".Translate(200 / value), ref value, value.ToString(), 0.001f, 5, "VCR.ArmScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value, ref armtemp, 0.001f, 5);
            ArmorScale = value;
            listingStandard.GapLine();
            var value2 = PenetrationScale;
            listingStandard.SliderLabeled("VCR.PenetrationScale".Translate(value2), ref value2, value2.ToString(), 0.001f, 5, "VCR.PenScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value2, ref pentemp, 0.001f, 5);
            PenetrationScale = value2;
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.AdvanceAccuracy".Translate(), ref AdvancedAccuracy, "VCR.AAcctooltip".Translate());
            listingStandard.GapLine();
            var value3 = AccuracyScale;
            listingStandard.SliderLabeled("VCR.AccuracyScale".Translate(value3), ref value3, value3.ToString(), 1, 60, "VCR.AccScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value3, ref acctemp, 1, 60);
            AccuracyScale = value3;
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.Evasion".Translate(), ref Evasion, "VCR.Evatooltip".Translate());
            listingStandard.GapLine();
            var value4 = EvasionScale;
            listingStandard.SliderLabeled("VCR.EvasionScale".Translate(value4), ref value4, value4.ToString(), 0, 1, "VCR.EvaScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value4, ref evatemp, 0, 1);
            EvasionScale = value4;
            listingStandard.GapLine();
            var value5 = minSpeed;
            listingStandard.SliderLabeled("VCR.MinSpeed".Translate(value5), ref value5, value5.ToString(), 0, 30, "VCR.MinSpeedTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value5, ref minSpeedtemp, 0, 30);
            minSpeed = value5;
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.EvasionAccuracy".Translate(), ref EvaAcc, "VCR.EvaAcctooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.BulletsWorker".Translate(), ref BulletsWorker, "VCR.BWorkerTooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.ArrowsWorker".Translate(), ref ArrowsWorker, "VCR.AWorkerTooltip".Translate());
            listingStandard.GapLine();
            listingStandard.GapLine();
            listingStandard.Label("VCR.XmlSettings".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.HandFeetPatch".Translate(), ref HandFeetPatch, "VCR.HandFeetTooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.GlassesHelmetPatch".Translate(), ref GlassesHelmetPatch, "VCR.GlassesHelmetTooltip".Translate());

            listingStandard.End();
        }
    }

    public class PatchOperationXmlSetting : PatchOperation
    {
        private string setting;

        private PatchOperation match;

        private PatchOperation nomatch;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (VCR.XmlSettings.Contains(setting))
            {
                if (match != null)
                {
                    return match.Apply(xml);
                }
            }
            else if (nomatch != null)
            {
                return nomatch.Apply(xml);
            }
            if (match == null)
            {
                return nomatch != null;
            }
            return true;
        }
        public override string ToString()
        {
            return $"{base.ToString()}({setting})";
        }
    }

    //advanced armor patches
    [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
    public static class ArmorUtility_ApplyArmor_Patch
    {
        public static bool AArmor;
        public static float AArmorScale;
        public static bool Prefix(ref float armorPenetration, ref float armorRating)
        {
            if (AArmor)
            {
                armorPenetration *= AArmorScale;
                armorRating *= AArmorScale;
            }
            return true;
        }
    }
    [HarmonyPatch]
    public static class AdvanceArmor_postfix
    {
        public static float APScale;
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration), parameters: new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver) });
            yield return AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration), parameters: new Type[] { typeof(Tool), typeof(Pawn), typeof(ThingDef), typeof(ThingDef), typeof(HediffComp_VerbGiver) });
            yield return AccessTools.Method(typeof(ExtraDamage), nameof(ExtraDamage.AdjustedArmorPenetration), parameters: new Type[] { });
            yield return AccessTools.Method(typeof(ExtraDamage), nameof(ExtraDamage.AdjustedArmorPenetration), parameters: new Type[] { typeof(Verb), typeof(Pawn) });
            yield return AccessTools.Method(typeof(ProjectileProperties), nameof(ProjectileProperties.GetArmorPenetration), parameters: new Type[] { typeof(float), typeof(StringBuilder) });
        }
        public static void Postfix(ref float __result)
        {
            if (ArmorUtility_ApplyArmor_Patch.AArmor)
            {
                __result *= APScale;
            }
        }
    }
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
    }
    [HarmonyPatch(typeof(ShotReport), "AimOnTargetChance_StandardTarget", MethodType.Getter)]
    public static class ShotReport_AimOnTargetChance_StandardTarget_Patch
    {
        public static bool Eva;
        public static float baseEvasion;
        public static float minSpeed;
        public static bool EvaAcc;
        public static float CurEvasion = 1;
        public static float FinalEvasion = 1;
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
        public static float Evasion(Pawn targetPawn, Thing caster)
        {

            CurEvasion = (float)Math.Pow(baseEvasion, Math.Max(Velocity(targetPawn) - minSpeed, 0));
            if (!EvaAcc)
            {
                return CurEvasion;
            }
            return ShotReport_HitReportFor_Patch.statBump(CurEvasion, caster);
        }
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
    [HarmonyPatch(typeof(ShotReport), "GetTextReadout")]
    public static class ShotReport_GetTextReadout_Patch
    {
        public static bool Prefix(ref string __result,ShotReport __instance, TargetInfo ___target, float ___distance, List<CoverInfo> ___covers,
                                    float ___factorFromShooterAndDist, float ___factorFromEquipment, float ___factorFromTargetSize, float ___factorFromWeather,
                                    float ___forcedMissRadius, float ___offsetFromDarkness, float ___factorFromCoveringGas)
        {
            if (ShotReport_HitReportFor_Patch.AAccuracy || ShotReport_AimOnTargetChance_StandardTarget_Patch.Eva)
            {
                __result = CustomTextReadout(__instance ,___target, ___distance, ___covers,
                                    ___factorFromShooterAndDist, ___factorFromEquipment, ___factorFromTargetSize, ___factorFromWeather,
                                    ___forcedMissRadius, ___offsetFromDarkness, ___factorFromCoveringGas, FactorFromPosture(___target,___distance),
                                    FactorFromExecution(___target, ___distance));
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
                                    float FactorFromExecution)
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
                float eva1 = ShotReport_AimOnTargetChance_StandardTarget_Patch.CurEvasion;
                float eva2 = ShotReport_AimOnTargetChance_StandardTarget_Patch.FinalEvasion;
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
            }
            return stringBuilder.ToString();
        }
    }
}
