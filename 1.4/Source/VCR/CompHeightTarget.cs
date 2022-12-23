using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VCR
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Pawn_SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && __instance.IsColonist is false)
            {
                __instance.TryAssignRandomTargetingMode();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DeSpawn))]
    public static class Pawn_DeSpawn_Patch
    {
        public static void Prefix(Pawn __instance)
        {
            if (__instance.IsColonist is false && __instance.MapHeld is null)
            {
                __instance.TryResetTargetingMode();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public static class Pawn_DraftController_Drafted_Patch
    {
        public static void Postfix(Pawn_DraftController __instance, bool value)
        {
            if (value is false)
            {
                __instance.pawn.TryResetTargetingMode();
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class Pawn_HealthTracker_MakeDowned_Patch
    {
        private static void Postfix(Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
        {
            if (___pawn.Downed)
            {
                ___pawn.TryResetTargetingMode();
            }
        }
    }

    public class CompHeightTarget : ThingComp, ITargetModeSettable
    {
        public Pawn Pawn => parent as Pawn;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (VanillaCombatMod.settings.Flanking && parent.Faction == Faction.OfPlayer && (Pawn == null || Pawn.Drafted))
                yield return TargetingModesUtility.SetTargetModeCommand(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _targetingMode, "heightTarget");
        }

        public override string ToString() =>
            $"CompTargetingMode for {parent} :: _targetingMode={_targetingMode.ToStringHuman()};";

        public BodyPartHeight GetTargetingMode() => _targetingMode;

        public void SetTargetingMode(BodyPartHeight targetMode)
        {
            // Actually set the targeting mode
            _targetingMode = targetMode;
        }

        private BodyPartHeight _targetingMode = TargetingModesUtility.defaultHeight;
    }

    [StaticConstructorOnStartup]
    public static class TargetingModesUtility
    {
        private static Texture2D SetTargetingModeTex = ContentFinder<Texture2D>.Get("UI/SetTargetingMode");

        public static readonly List<BodyPartHeight> allTargetingModes = Enum.GetValues(typeof(BodyPartHeight)).Cast<BodyPartHeight>().ToList();

        public static readonly BodyPartHeight defaultHeight = BodyPartHeight.Undefined;

        static TargetingModesUtility()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.race != null && (def.race.Humanlike || def.race.ToolUser))
                {
                    if (def.comps is null)
                        def.comps = new List<CompProperties>();

                    def.comps.Add(new CompProperties
                    {
                        compClass = typeof(CompHeightTarget)
                    });
                }
            }
        }
        public static Command_SetTargetingMode SetTargetModeCommand(ITargetModeSettable settable) =>
            new Command_SetTargetingMode
            {
                icon = SetTargetingModeTex,
                defaultLabel = "VCR.CommandSetTargetingMode".Translate(settable.GetTargetingMode().ToStringHuman()),
                defaultDesc = "VCR.CommandSetTargetingModeDesc".Translate(),
                settable = settable,
            };

        public static BodyPartHeight GetTargetHeight(this Thing instigator)
        {
            if (instigator.CanUseTargetingModes())
            {
                var comp = instigator.TryGetComp<CompHeightTarget>();
                if (comp != null)
                {
                    return comp.GetTargetingMode();
                }
            }
            return defaultHeight;
        }
        public static bool CanUseTargetingModes(this Thing instigator)
        {
            // No instigator or instigator doesn't have CompTargetingMode
            if (instigator == null || !instigator.def.HasComp(typeof(CompHeightTarget)))
                return false;

            // Melee attack
            //if (typeof(Pawn).IsAssignableFrom(weapon.thingClass) || weapon.IsMeleeWeapon)
            //    return true;

            if (instigator is Pawn pawn)
            {
                // Explosive
                if (pawn.CurrentEffectiveVerb.verbProps.CausesExplosion)
                    return false;
            }

            if (instigator is Building_Turret turret)
            {
                // Explosive
                if (turret.CurrentEffectiveVerb.verbProps.CausesExplosion)
                    return false;
            }

            return true;
        }

        public static void TryAssignRandomTargetingMode(this Pawn pawn)
        {
            if (pawn.TryGetComp<CompHeightTarget>() is CompHeightTarget Comp)
            {
                BodyPartHeight height = (BodyPartHeight)Rand.RangeInclusive(0,3);
                Comp.SetTargetingMode(height);
            }
        }
        public static void TryResetTargetingMode(this Pawn pawn)
        {
            if (pawn.TryGetComp<CompHeightTarget>() is CompHeightTarget Comp)
            {
                Comp.SetTargetingMode(defaultHeight);
            }
        }
        public static string ToStringHuman(this BodyPartHeight targetingMode)
        {
            return ("VCR." + targetingMode.ToString()).Translate();
        }
    }
    public interface ITargetModeSettable
    {
        BodyPartHeight GetTargetingMode();

        void SetTargetingMode(BodyPartHeight height);
    }


    [StaticConstructorOnStartup]
    public class Command_SetTargetingMode : Command
    {
        public ITargetModeSettable settable;
        public List<ITargetModeSettable> settables;
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            if (settables == null)
                settables = new List<ITargetModeSettable>();
            if (!settables.Contains(settable))
                settables.Add(settable);

            List<FloatMenuOption> targetingModeOptions = new List<FloatMenuOption>();
            foreach (var targetMode in TargetingModesUtility.allTargetingModes)
                targetingModeOptions.Add(new FloatMenuOption(FloatMenuLabel(targetMode),
                    () =>
                    {
                        for (int i = 0; i < settables.Count; i++)
                            settables[i].SetTargetingMode(targetMode);
                    }));
            Find.WindowStack.Add(new FloatMenu(targetingModeOptions));
        }

        private string FloatMenuLabel(BodyPartHeight targetingMode)
        {
            return targetingMode.ToStringHuman();
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (settables == null)
                settables = new List<ITargetModeSettable>();
            settables.Add(((Command_SetTargetingMode)other).settable);
            return false;
        }
    }
}