//using RimWorld;
//using System.Collections.Generic;
//using UnityEngine;
//using Verse;

//namespace VCR
//{
//    public class CompHeightTarget:ThingComp, ITargetModeSettable
//    {
//        private const int TargetModeResetCheckInterval = 60;

//        public Pawn Pawn => parent as Pawn;

//        public override IEnumerable<Gizmo> CompGetGizmosExtra()
//        {
//            if (parent.Faction == Faction.OfPlayer && (Pawn == null && Pawn.Drafted))
//                yield return TargetingModesUtility.SetTargetModeCommand(this);
//        }

//        public override void CompTick()
//        {
//            base.CompTick();

//            // For compatibility with existing saves
//            if (_targetingMode == null)
//                SetTargetingMode(BodyPartHeight.Undefined);

//            // Check if targeting mode should be reset every 60 ticks
//            else if (parent.IsHashIntervalTick(TargetModeResetCheckInterval))
//            {
//                if (CanResetTargetingMode(out bool updateResetTick))
//                    SetTargetingMode(TargetingModesUtility.defaultHeight);
//                else if (updateResetTick)
//                    _resetTargetingModeTick = Find.TickManager.TicksGame + TargModeResetAttemptInterval();
//            }
//        }

//        public bool CanResetTargetingMode(out bool updateResetTick)
//        {
//            updateResetTick = false;

//            // World pawns are exempt to player restrictions
//            if (parent.Map == null)
//                return Find.TickManager.TicksGame < _resetTargetingModeTick;

//            // If player's set it to never reset, if the mode is already default or if it isn't time to update
//            if (TargetingModesSettings.TargModeResetFrequencyInt == 0 || _targetingMode == TargetingModesUtility.defaultHeight || Find.TickManager.TicksGame < _resetTargetingModeTick)
//                return false;

//            // If the parent pawn is drafted or considered in dangerous combat
//            if (Pawn != null && (Pawn.Drafted || GenAI.InDangerousCombat(Pawn)))
//            {
//                updateResetTick = true;
//                return false;
//            }

//            // If the parent is a turret and is targeting something
//            if (parent is Building_Turret turret && turret.CurrentTarget != null)
//            {
//                updateResetTick = true;
//                return false;
//            }

//            return true;
//        }

//        private int TargModeResetAttemptInterval()
//        {
//            switch (parent.Faction == Faction.OfPlayer ? VanillaCombatSettings.TargModeResetFrequencyInt : 3)
//            {
//                // 1 = 1d
//                case 1:
//                    return GenDate.TicksPerDay;
//                // 2 = 12h
//                case 2:
//                    return GenDate.TicksPerHour * 12;
//                // 3 = 6h
//                case 3:
//                    return GenDate.TicksPerHour * 6;
//                // 4 = 3h
//                case 4:
//                    return GenDate.TicksPerHour * 3;
//                // other = 1h
//                default:
//                    return GenDate.TicksPerHour;
//            };
//        }

//        public override void PostExposeData()
//        {
//            base.PostExposeData();
//            Scribe_Values.Look(ref _targetingMode, "heightTarget");
//            Scribe_Values.Look(ref _resetTargetingModeTick, "resetTargetingModeTick", -1);
//        }

//        public override string ToString() =>
//            $"CompTargetingMode for {parent} :: _targetingMode={_targetingMode.LabelCap}; _resetTargetingModeTick={_resetTargetingModeTick}; (TicksGame={Find.TickManager.TicksGame})";

//        public BodyPartHeight GetTargetingMode() => _targetingMode;

//        public void SetTargetingMode(BodyPartHeight targetMode)
//        {
//            // Actually set the targeting mode
//            _targetingMode = targetMode;

//            // Set which tick the game will try to reset the mode at
//            _resetTargetingModeTick = Find.TickManager.TicksGame + TargModeResetAttemptInterval();
//        }

//        private BodyPartHeight _targetingMode = TargetingModesUtility.defaultHeight;
//        private int _resetTargetingModeTick = -1;

//    }
//    public static class TargetingModesUtility
//    {

//        public static readonly BodyPartHeight defaultHeight = BodyPartHeight.Undefined;
//        public static Command_SetTargetingMode SetTargetModeCommand(ITargetModeSettable settable) =>
//            new Command_SetTargetingMode
//            {
//                defaultDesc = "CommandSetTargetingModeDesc".Translate(),
//                settable = settable
//            };

//        public static bool CanUseTargetingModes(this Thing instigator, ThingDef weapon)
//        {
//            // No instigator or instigator doesn't have CompTargetingMode
//            if (instigator == null || !instigator.def.HasComp(typeof(CompHeightTarget)) || weapon == null)
//                return false;

//            // Melee attack
//            //if (typeof(Pawn).IsAssignableFrom(weapon.thingClass) || weapon.IsMeleeWeapon)
//            //    return true;

//            if (instigator is Pawn pawn)
//            {
//                // Explosive
//                if (pawn.CurrentEffectiveVerb.verbProps.CausesExplosion)
//                    return false;
//            }

//            if (instigator is Building_Turret turret)
//            {
//                // Explosive
//                if (turret.CurrentEffectiveVerb.verbProps.CausesExplosion)
//                    return false;
//            }

//            return true;
//        }

//        public static void TryAssignRandomTargetingMode(this Pawn pawn)
//        {
//            if (VanillaCombatSettings.raidersCanTarget && pawn.TryGetComp<CompHeightTarget>() is CompHeightTarget Comp)
//            {
//                BodyPartHeight height = (BodyPartHeight)Rand.RangeInclusive(0,3);
//                Comp.SetTargetingMode(height);
//            }
//        }
//    }
//    public interface ITargetModeSettable
//    {

//        BodyPartHeight GetTargetingMode();

//        void SetTargetingMode(BodyPartHeight height);

//    }


//    [StaticConstructorOnStartup]
//    public class Command_SetTargetingMode : Command
//    {

//        private static Texture2D SetTargetingModeTex = ContentFinder<Texture2D>.Get("UI/TargetingModes/MultipleModes");
//        public ITargetModeSettable settable;
//        public List<ITargetModeSettable> settables;

//        public Command_SetTargetingMode()
//        {
//            BodyPartHeight height = BodyPartHeight.Undefined;
//            bool multiplePawnsSelected = false;
//            foreach (object obj in Find.Selector.SelectedObjects)
//            {
//                if (obj is ThingWithComps thing && thing.TryGetComp<CompHeightTarget>() is ITargetModeSettable targetModeSettable)
//                {
//                    if (targetingMode != null && targetingMode != targetModeSettable.GetTargetingMode())
//                    {
//                        multiplePawnsSelected = true;
//                        break;
//                    }
//                    targetingMode = targetModeSettable.GetTargetingMode();
//                }
//            }
//            icon = (multiplePawnsSelected) ? SetTargetingModeTex : targetingMode.uiIcon;
//            defaultLabel = (multiplePawnsSelected) ? "CommandSetTargetingModeMulti".Translate() : "CommandSetTargetingMode".Translate(targetingMode.LabelCap);
//        }

//        public override void ProcessInput(Event ev)
//        {
//            base.ProcessInput(ev);
//            if (settables == null)
//                settables = new List<ITargetModeSettable>();
//            if (!settables.Contains(settable))
//                settables.Add(settable);
//            TargetingModes.SortBy(t => -t.displayOrder, t => t.LabelCap);
//            List<FloatMenuOption> targetingModeOptions = new List<FloatMenuOption>();
//            foreach (TargetingModeDef targetMode in TargetingModes)
//                targetingModeOptions.Add(new FloatMenuOption(FloatMenuLabel(targetMode),
//                    () =>
//                    {
//                        for (int i = 0; i < settables.Count; i++)
//                            settables[i].SetTargetingMode(targetMode);
//                    }));
//            Find.WindowStack.Add(new FloatMenu(targetingModeOptions));
//        }

//        private string FloatMenuLabel(TargetingModeDef targetingMode)
//        {
//            string label = targetingMode.LabelCap;
//            if (targetingMode.HitChanceFactor != 1f)
//                label += $" (x{targetingMode.HitChanceFactor.ToStringPercent()} Acc)";
//            return label;
//        }

//        public override bool InheritInteractionsFrom(Gizmo other)
//        {
//            if (settables == null)
//                settables = new List<ITargetModeSettable>();
//            settables.Add(((Command_SetTargetingMode)other).settable);
//            return false;
//        }

//        public List<BodyPartHeight> TargetingModes => BodyPartHeight;

//    }
//}