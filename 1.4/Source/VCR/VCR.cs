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
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            //Harmony.DEBUG = true;
            new Harmony("VCR.Mod").PatchAll();
        }
    }
    [StaticConstructorOnStartup]
    public static class VCR
    {
        static VCR()
        {
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
            if (VanillaCombatMod.settings.AcidHeatPatch)
            {
                XmlSettings.Add("AcidHeatPatch");
            }
            if (VanillaCombatMod.settings.ThumpBluntPatch)
            {
                XmlSettings.Add("ThumpBluntPatch");
            }
            if (VanillaCombatMod.settings.GlassesHelmetPatch)
            {
                XmlSettings.Add("GlassesHelmetPatch");
            }
            if (VanillaCombatMod.settings.NoseMouthPatch)
            {
                XmlSettings.Add("NoseMouthPatch");
            }
            if (VanillaCombatMod.settings.MaskPatch)
            {
                XmlSettings.Add("MaskPatch");
            }
            if (VanillaCombatMod.settings.HeadsetPatch)
            {
                XmlSettings.Add("HeadsetPatch");
            }
            if (VanillaCombatMod.settings.ArrayHeadsetPatch)
            {
                XmlSettings.Add("ArrayHeadsetPatch");
            }
        }
        public static void ApplySettings()
        {
            ArmorUtility_ApplyArmor_Patch.AArmor = VanillaCombatMod.settings.AdvancedArmor;
            ArmorUtility_ApplyArmor_Patch.AArmorScale = VanillaCombatMod.settings.ArmorScale;
            AdvanceArmor_postfix.APScale = VanillaCombatMod.settings.PenetrationScale;
            
            ShotReport_HitReportFor_Patch.AAccuracy = VanillaCombatMod.settings.AdvancedAccuracy;
            ShotReport_HitReportFor_Patch.AccScale = VanillaCombatMod.settings.AccuracyScale;
            
            ShotReport_AimOnTargetChance_IgnoringPosture_Patch.Eva = VanillaCombatMod.settings.Evasion;
            ShotReport_AimOnTargetChance_IgnoringPosture_Patch.baseEvasion = VanillaCombatMod.settings.EvasionScale;
            ShotReport_AimOnTargetChance_IgnoringPosture_Patch.minSpeed = VanillaCombatMod.settings.minSpeed;
            ShotReport_AimOnTargetChance_IgnoringPosture_Patch.EvaAcc = VanillaCombatMod.settings.EvaAcc;

            Parrying.active = VanillaCombatMod.settings.Parry;
            Parrying.front = VanillaCombatMod.settings.ParryFront;
            Parrying.side = VanillaCombatMod.settings.ParrySide;

            DamageWorker_Bullet.active = VanillaCombatMod.settings.BulletsWorker;
            DamageWorker_Bullet.flanking = VanillaCombatMod.settings.Flanking;
            DamageWorker_Bullet.SPmax = VanillaCombatMod.settings.SPLimit;
            DamageWorker_Arrow.active = VanillaCombatMod.settings.ArrowsWorker;
            DamageWorker_Arrow.flanking = VanillaCombatMod.settings.Flanking;
            Flanking.MeleeFlanking = VanillaCombatMod.settings.MeleeFlanking;

            PawnRenderer_DrawApparel_Patch.setting = VanillaCombatMod.settings.ApparelTweaks;
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
        public Tabs curTab = Tabs.armor;

        public bool AdvancedArmor = false;
        public float ArmorScale = 2f;
        public string armtemp = "2";
        public float PenetrationScale = 2f;
        public string pentemp = "2";

        public bool AdvancedAccuracy = false;
        public float AccuracyScale = 5f;
        public string acctemp = "5";

        public bool UseFiringArc = false;
        public float FiringArc = 45;
        public string arctemp = "45";
        public int ArcType = 0;

        public bool Evasion = false;
        public float EvasionScale = 0.8f;
        public string evatemp = "0.8";
        public float minSpeed = 2.5f;
        public string minSpeedtemp = "2.5";
        public bool EvaAcc = true;

        public bool Parry = false;
        public float ParryFront = 1.5f;
        public string pFrontTemp = "1.5";
        public float ParrySide = 1.5f;
        public string pSideTemp = "1.5";

        public bool BulletsWorker = false;
        public float SPLimit = 10f;
        public string SPLimitTemp = "10";
        public bool ArrowsWorker = false;
        public bool Flanking = false;
        public bool MeleeFlanking = false;

        public bool HandFeetPatch = false;
        public bool AcidHeatPatch = false;
        public bool ThumpBluntPatch = false;
        public bool GlassesHelmetPatch = false;
        public bool NoseMouthPatch = false;
        public bool MaskPatch = false;
        public bool HeadsetPatch = false;
        public bool ArrayHeadsetPatch = false;
        public bool ApparelTweaks = false;


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

            Scribe_Values.Look(ref UseFiringArc, "UseFiringArc", false);
            Scribe_Values.Look(ref FiringArc, "FiringArc", 45f);
            Scribe_Values.Look(ref arctemp, "arctemp", "45");
            Scribe_Values.Look(ref ArcType, "ArcType", 0);


            Scribe_Values.Look(ref Evasion, "Evasion", false);
            Scribe_Values.Look(ref EvasionScale, "EvasionScale", 0.8f);
            Scribe_Values.Look(ref evatemp, "evatemp", "0.8");
            Scribe_Values.Look(ref minSpeed, "minSpeed", 2.5f);
            Scribe_Values.Look(ref minSpeedtemp, "minSpeedtemp", "2.5");
            Scribe_Values.Look(ref EvaAcc, "EvaAcc", true);
            
            Scribe_Values.Look(ref Parry, "Parry", false);
            Scribe_Values.Look(ref ParrySide, "ParrySide", 1.5f);
            Scribe_Values.Look(ref ParryFront, "ParryFront", 1.5f);

            Scribe_Values.Look(ref BulletsWorker, "BulletsWorker", false);
            Scribe_Values.Look(ref SPLimit, "SPLimit", 10f);
            Scribe_Values.Look(ref ArrowsWorker, "ArrowsWorker", false);
            Scribe_Values.Look(ref Flanking, "Flanking", false);
            Scribe_Values.Look(ref MeleeFlanking, "MeleeFlanking", false);

            Scribe_Values.Look(ref HandFeetPatch, "HandFeetPatch", false);
            Scribe_Values.Look(ref AcidHeatPatch, "AcidHeatPatch", false);
            Scribe_Values.Look(ref ThumpBluntPatch, "ThumpBluntPatch", false);
            Scribe_Values.Look(ref GlassesHelmetPatch, "GlassesHelmetPatch", false);
            Scribe_Values.Look(ref NoseMouthPatch, "NoseMouthPatch", false);
            Scribe_Values.Look(ref MaskPatch, "MaskPatch", false);
            Scribe_Values.Look(ref HeadsetPatch, "HeadsetPatch", false);
            Scribe_Values.Look(ref ArrayHeadsetPatch, "ArrayHeadsetPatch", false);
            Scribe_Values.Look(ref ApparelTweaks, "ApparelTweaks", false);


        }
        public enum Tabs
        {
            armor,
            accuracy,
            evasion,
            parry,
            damage,
            xml
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Rect tabtop = new Rect(0, TabDrawer.TabHeight, inRect.width, 0);
            Rect settingSection = new Rect(0, TabDrawer.TabHeight, inRect.width, inRect.height-TabDrawer.TabHeight);

            Widgets.DrawMenuSection(settingSection);
            List<TabRecord> tablist = new List<TabRecord>();
            tablist.Add(new TabRecord("VCR.AdvanceArmorTab".Translate(), delegate { curTab = Tabs.armor; }, curTab == Tabs.armor));
            tablist.Add(new TabRecord("VCR.AdvanceAccuracyTab".Translate(), delegate { curTab = Tabs.accuracy; }, curTab == Tabs.accuracy));
            tablist.Add(new TabRecord("VCR.EvasionTab".Translate(), delegate { curTab = Tabs.evasion; }, curTab == Tabs.evasion));
            tablist.Add(new TabRecord("VCR.ParryTab".Translate(), delegate { curTab = Tabs.parry; }, curTab == Tabs.parry));
            tablist.Add(new TabRecord("VCR.DamageWorkerTab".Translate(), delegate { curTab = Tabs.damage; }, curTab == Tabs.damage));
            tablist.Add(new TabRecord("VCR.Xml".Translate(), delegate { curTab = Tabs.xml; }, curTab == Tabs.xml));
            TabDrawer.DrawTabs(tabtop, tablist);

            switch (curTab)
            {
                case Tabs.armor:
                    Armor(inRect.ContractedBy(20));
                    break;
                case Tabs.accuracy:
                    Accuracy(inRect.ContractedBy(20));
                    break;
                case Tabs.evasion:
                    EvasionTab(inRect.ContractedBy(20));
                    break;
                case Tabs.parry:
                    ParryTab(inRect.ContractedBy(20));
                    break;
                case Tabs.damage:
                    Damage(inRect.ContractedBy(20));
                    break;
                case Tabs.xml:
                    XmlSettings(inRect.ContractedBy(20));
                    break;
                default:
                    Armor(inRect.ContractedBy(20));
                    break;
            }
            GUI.EndGroup();
        }
        public void Armor(Rect inRect)
        {

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("VCR.AdvanceArmor".Translate(), ref AdvancedArmor, "VCR.AAtooltip".Translate());
            listingStandard.Gap();
            var value = ArmorScale;
            listingStandard.SliderLabeled("VCR.ArmorScale".Translate(200 / value), ref value, value.ToString(), 0.001f, 5, "VCR.ArmScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value, ref armtemp, 0.001f, 5);
            ArmorScale = value;
            listingStandard.Gap();
            var value2 = PenetrationScale;
            listingStandard.SliderLabeled("VCR.PenetrationScale".Translate(value2.ToString()), ref value2, value2.ToString(), 0.001f, 5, "VCR.PenScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value2, ref pentemp, 0.001f, 5);
            PenetrationScale = value2;
            listingStandard.End();
        }
        public void Accuracy(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("VCR.AdvanceAccuracy".Translate(), ref AdvancedAccuracy, "VCR.AAcctooltip".Translate());
            listingStandard.Gap();
            var value3 = AccuracyScale;
            listingStandard.SliderLabeled("VCR.AccuracyScale".Translate(value3), ref value3, value3.ToString(), 1, 60, "VCR.AccScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value3, ref acctemp, 1, 60);
            AccuracyScale = value3;
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.UseFiringArc".Translate(), ref UseFiringArc, "VCR.UseArctooltip".Translate());
            listingStandard.Gap();
            var value4 = FiringArc;
            listingStandard.SliderLabeled("VCR.FiringArc".Translate(value4), ref value4, value4.ToString(), 1, 179, "VCR.FiringArcTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value4, ref arctemp, 1, 179);
            FiringArc = value4;
            listingStandard.Gap();
            var value5 = ArcType;
            listingStandard.SliderLabeled("VCR.ArcType".Translate(value5), ref value5, value5.ToString(), 0, 5, "VCR.ArcTypeTooltip".Translate());
            ArcType = value5;
            listingStandard.End();
        }
        public void EvasionTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("VCR.Evasion".Translate(), ref Evasion, "VCR.Evatooltip".Translate());
            listingStandard.Gap();
            var value4 = EvasionScale;
            listingStandard.SliderLabeled("VCR.EvasionScale".Translate(value4.ToString()), ref value4, value4.ToString(), 0, 1, "VCR.EvaScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value4, ref evatemp, 0, 1);
            EvasionScale = value4;
            listingStandard.Gap();
            var value5 = minSpeed;
            listingStandard.SliderLabeled("VCR.MinSpeed".Translate(value5.ToString()), ref value5, value5.ToString(), 0, 30, "VCR.MinSpeedTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value5, ref minSpeedtemp, 0, 30);
            minSpeed = value5;
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.EvasionAccuracy".Translate(), ref EvaAcc, "VCR.EvaAcctooltip".Translate());
            listingStandard.End();
        }
        public void ParryTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("VCR.Parry".Translate(), ref Parry, "VCR.Parrytooltip".Translate());
            listingStandard.Gap();
            var value6 = ParryFront;
            listingStandard.SliderLabeled("VCR.ParryFront".Translate(value6.ToString()), ref value6, value6.ToString(), 0, 5, "VCR.ParryFrontTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value6, ref pFrontTemp, 0, 5);
            ParryFront = value6;
            listingStandard.Gap();
            var value7 = ParrySide;
            listingStandard.SliderLabeled("VCR.ParrySide".Translate(value7.ToString()), ref value7, value7.ToString(), 0, 5, "VCR.ParrySideTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value7, ref pSideTemp, 0, 5);
            ParrySide = value7;
            listingStandard.End();
        }
        public void Damage(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("VCR.BulletsWorker".Translate(), ref BulletsWorker, "VCR.BWorkerTooltip".Translate());
            listingStandard.Gap();
            listingStandard.Label("VCR.SPLimit".Translate(), -1, "VCR.SPTooltip".Translate());
            var value8 = SPLimit;
            listingStandard.TextFieldNumeric(ref value8, ref SPLimitTemp, 1);
            SPLimit = value8;
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.ArrowsWorker".Translate(), ref ArrowsWorker, "VCR.AWorkerTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.Flanking".Translate(), ref Flanking, "VCR.FlankingTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.MeleeFlank".Translate(), ref MeleeFlanking, "VCR.MeleeFlankTooltip".Translate());
            listingStandard.End();
        }
        public void XmlSettings(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("VCR.XmlSettings".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.HandFeetPatch".Translate(), ref HandFeetPatch, "VCR.HandFeetTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.AcidHeatPatch".Translate(), ref AcidHeatPatch, "VCR.AcidHeatTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.ThumpBluntPatch".Translate(), ref ThumpBluntPatch, "VCR.ThumpBluntTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.GlassesHelmetPatch".Translate(), ref GlassesHelmetPatch, "VCR.GlassesHelmetTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.NoseMouthPatch".Translate(), ref NoseMouthPatch, "VCR.NoseMouthTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.MaskPatch".Translate(), ref MaskPatch, "VCR.MaskTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.HeadsetPatch".Translate(), ref HeadsetPatch, "VCR.HeadsetTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.ArrayHeadsetPatch".Translate(), ref ArrayHeadsetPatch, "VCR.ArrayHeadsetTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.ApparelTweaks".Translate(), ref ApparelTweaks, "VCR.ApparelTweaksTooltip".Translate());
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

}
