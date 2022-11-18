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

            Parrying.active = VanillaCombatMod.settings.Parry;
            Parrying.front = VanillaCombatMod.settings.ParryFront;
            Parrying.side = VanillaCombatMod.settings.ParrySide;

            DamageWorker_Bullet.active = VanillaCombatMod.settings.BulletsWorker;
            DamageWorker_Bullet.flanking = VanillaCombatMod.settings.Flanking;
            DamageWorker_Arrow.active = VanillaCombatMod.settings.ArrowsWorker;
            DamageWorker_Arrow.flanking = VanillaCombatMod.settings.Flanking;
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

        public bool Evasion = false;
        public float EvasionScale = 0.5f;
        public string evatemp = "0.5";
        public float minSpeed = 2.5f;
        public string minSpeedtemp = "2.5";
        public bool EvaAcc = true;

        public bool Parry = false;
        public float ParryFront = 1f;
        public string pFrontTemp = "1";
        public float ParrySide = 1f;
        public string pSideTemp = "1";

        public bool BulletsWorker = false;
        public bool ArrowsWorker = false;
        public bool Flanking = false;

        public bool HandFeetPatch = false;
        public bool AcidHeatPatch = false;
        public bool ThumpBluntPatch = false;
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
            
            Scribe_Values.Look(ref Parry, "Parry", false);
            Scribe_Values.Look(ref ParrySide, "ParrySide", 1);
            Scribe_Values.Look(ref ParryFront, "ParryFront", 1);

            Scribe_Values.Look(ref BulletsWorker, "BulletsWorker", false);
            Scribe_Values.Look(ref ArrowsWorker, "ArrowsWorker", false);
            Scribe_Values.Look(ref Flanking, "Flanking", false);

            Scribe_Values.Look(ref HandFeetPatch, "HandFeetPatch", false);
            Scribe_Values.Look(ref AcidHeatPatch, "AcidHeatPatch", false);
            Scribe_Values.Look(ref ThumpBluntPatch, "ThumpBluntPatch", false);
            Scribe_Values.Look(ref GlassesHelmetPatch, "GlassesHelmetPatch", false);
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
            listingStandard.SliderLabeled("VCR.PenetrationScale".Translate(value2), ref value2, value2.ToString(), 0.001f, 5, "VCR.PenScaleTooltip".Translate());
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
            listingStandard.End();
        }
        public void EvasionTab(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("VCR.Evasion".Translate(), ref Evasion, "VCR.Evatooltip".Translate());
            listingStandard.Gap();
            var value4 = EvasionScale;
            listingStandard.SliderLabeled("VCR.EvasionScale".Translate(value4), ref value4, value4.ToString(), 0, 1, "VCR.EvaScaleTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value4, ref evatemp, 0, 1);
            EvasionScale = value4;
            listingStandard.Gap();
            var value5 = minSpeed;
            listingStandard.SliderLabeled("VCR.MinSpeed".Translate(value5), ref value5, value5.ToString(), 0, 30, "VCR.MinSpeedTooltip".Translate());
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
            listingStandard.SliderLabeled("VCR.ParryFront".Translate(value6), ref value6, value6.ToString(), 0, 5, "VCR.ParryFrontTooltip".Translate());
            listingStandard.TextFieldNumeric(ref value6, ref pFrontTemp, 0, 5);
            ParryFront = value6;
            listingStandard.Gap();
            var value7 = ParrySide;
            listingStandard.SliderLabeled("VCR.ParrySide".Translate(value7), ref value7, value7.ToString(), 0, 5, "VCR.ParrySideTooltip".Translate());
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
            listingStandard.CheckboxLabeled("VCR.ArrowsWorker".Translate(), ref ArrowsWorker, "VCR.AWorkerTooltip".Translate());
            listingStandard.Gap();
            listingStandard.CheckboxLabeled("VCR.Flanking".Translate(), ref Flanking, "VCR.FlankingTooltip".Translate());
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
