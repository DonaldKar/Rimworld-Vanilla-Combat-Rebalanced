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
        }
        public static void ApplySettings()
        {
            ArmorUtility_ApplyArmor_Patch.AArmor = VanillaCombatMod.settings.AdvancedArmor;
            ArmorUtility_ApplyArmor_Patch.AArmorScale = VanillaCombatMod.settings.ArmorScale;
            AdvanceArmor_postfix.APScale = VanillaCombatMod.settings.PenetrationScale;
            ShotReport_HitReportFor_Patch.AAccuracy = VanillaCombatMod.settings.AdvancedAccuracy;
            ShotReport_HitReportFor_Patch.AccScale = VanillaCombatMod.settings.AccuracyScale;
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
        public bool HandFeetPatch = false;
        public bool BulletsWorker = false;
        public bool ArrowsWorker = false;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AdvancedArmor, "AdvancedArmor", false);
            Scribe_Values.Look(ref ArmorScale, "ArmorScale", 2f);
            Scribe_Values.Look(ref armtemp, "armtemp", "2");
            Scribe_Values.Look(ref PenetrationScale, "PenetrationScale", 2f);
            Scribe_Values.Look(ref pentemp, "pentemp", "2");
            Scribe_Values.Look(ref AdvancedAccuracy, "AdvancedAccuracy", false);
            Scribe_Values.Look(ref AccuracyScale,"AccuracyScale", 5f);
            Scribe_Values.Look(ref acctemp, "acctemp", "5");
            Scribe_Values.Look(ref HandFeetPatch, "HandFeetPatch", false);
            Scribe_Values.Look(ref BulletsWorker, "BulletsWorker", false);
            Scribe_Values.Look(ref ArrowsWorker, "ArrowsWorker", false);
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
            listingStandard.SliderLabeled("VCR.ArmorScale".Translate(200/value), ref value, value.ToString(), 0.001f, 5, "VCR.ArmScaleTooltip".Translate());
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
            listingStandard.CheckboxLabeled("VCR.BulletsWorker".Translate(), ref BulletsWorker, "VCR.BWorkerTooltip".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.ArrowsWorker".Translate(), ref ArrowsWorker, "VCR.AWorkerTooltip".Translate());
            listingStandard.GapLine();
            listingStandard.GapLine();
            listingStandard.Label("VCR.XmlSettings".Translate());
            listingStandard.GapLine();
            listingStandard.CheckboxLabeled("VCR.HandFeetPatch".Translate(), ref HandFeetPatch, "VCR.HandFeetTooltip".Translate());

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
