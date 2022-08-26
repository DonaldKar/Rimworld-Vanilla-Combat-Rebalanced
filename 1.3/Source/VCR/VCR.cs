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
using static Verse.DamageWorker;
using MonoMod.Utils;
using Verse.Sound;

namespace VCR
{
    [StaticConstructorOnStartup]
    public static class VCR
    {
        static VCR()
        {

            //Harmony.DEBUG = true;
            new Harmony("VCR.Mod").PatchAll();
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
            settings.ApplySettings();
        }
    }
    public class VanillaCombatSettings : ModSettings
    {
        public bool AdvancedArmor = false;
        public bool AdvancedAccuracy = false;
        public float AccuracyScale = 1;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AdvancedArmor, "AdvancedArmor", false, true);
            Scribe_Values.Look(ref AdvancedAccuracy, "AdvancedAccuracy", false, true);
            Scribe_Values.Look(ref AccuracyScale,"AccuracyScale", 1, true);
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
            listingStandard.CheckboxLabeled("VCR.AdvanceAccuracy".Translate(), ref AdvancedAccuracy, "VCR.AAcctooltip".Translate());
            listingStandard.GapLine();
            listingStandard.Label("VCR.AccuracyScale".Translate(AccuracyScale));
            string temp= "";
            listingStandard.TextFieldNumericLabeled("VCR.AccScaleTooltip".Translate(AccuracyScale), ref AccuracyScale, ref temp, 1, 60);
            AccuracyScale = listingStandard.Slider(AccuracyScale, 1, 60);
            listingStandard.End();
        }
        public void ApplySettings()
        {
            ArmorUtility_ApplyArmor_Patch.AArmor = AdvancedArmor;
            ShotReport_HitReportFor_Patch.AAccuracy = AdvancedAccuracy;
            ShotReport_HitReportFor_Patch.AccScale = AccuracyScale;
        }
    }
    //advanced armor patches
    [HarmonyPatch(typeof(ArmorUtility), "ApplyArmor")]
    public static class ArmorUtility_ApplyArmor_Patch
    {
        public static bool AArmor;
        public static bool Prefix(ref float armorPenetration, ref float armorRating)
        {
            if (AArmor)
            {
                armorPenetration *= 2;
                armorRating *= 2;
            }
            return true;
        }
    }
    [HarmonyPatch]
    public static class AdvanceArmor_postfix
    {
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
                __result *= 2;
            }
        }
    }
    //advanced accuracy patches (not yet working)
    [HarmonyPatch(typeof(ShotReport), "HitReportFor")]
    public static class ShotReport_HitReportFor_Patch
    {
        public static bool AAccuracy;
        public static float AccScale;
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
        public static float statBump(float factor,Thing caster)
        {
            if (AAccuracy)
            {
                float shootstat = Mathf.Max(1, ((caster is Pawn) ? StatDefOf.ShootingAccuracyPawn.Worker.GetValue(StatRequest.For(caster), false) : StatDefOf.ShootingAccuracyTurret.Worker.GetValue(StatRequest.For(caster), false))/AccScale);//caster.GetStatValue(StatDefOf.ShootingAccuracyPawn, false): caster.GetStatValue(StatDefOf.ShootingAccuracyTurret,false));
                factor = Mathf.Pow(factor, 1 / shootstat);
            }
            return factor;
        }
    }
}
