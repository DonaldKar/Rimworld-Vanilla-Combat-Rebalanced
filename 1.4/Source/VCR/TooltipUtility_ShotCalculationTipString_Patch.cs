using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace VCR
{
    [HarmonyPatch(typeof(TooltipUtility), "ShotCalculationTipString")]
    public static class TooltipUtility_ShotCalculationTipString_Patch
    {
        public delegate float GetNonMissChance(Verb_MeleeAttack verb_MeleeAttack, LocalTargetInfo target);
        public static readonly GetNonMissChance getNonMissChance =
            AccessTools.MethodDelegate<GetNonMissChance>(AccessTools.Method(typeof(Verb_MeleeAttack), "GetNonMissChance"));

        public delegate float GetDodgeChance(Verb_MeleeAttack verb_MeleeAttack, LocalTargetInfo target);
        public static readonly GetDodgeChance getDodgeChance =
            AccessTools.MethodDelegate<GetDodgeChance>(AccessTools.Method(typeof(Verb_MeleeAttack), "GetDodgeChance"));



		public static void NonMissChanceReadout(Pawn CasterPawn, LocalTargetInfo target, float nonMissChance, ref StringBuilder stringBuilder)
		{
			stringBuilder.AppendLine("   " + "VCR.MeleeHitChance".Translate(nonMissChance.ToStringPercent()));//Melee Hit Chance:

			float num = CasterPawn.GetStatValue(StatDefOf.MeleeHitChance);
            if (nonMissChance == num)
            {
				return;
            }
			stringBuilder.AppendLine("      "+"VCR.MeleeHitChanceBase".Translate(num.ToStringPercent()));// Base Melee Hit Chance:

			if (ModsConfig.IdeologyActive && target.HasThing)
			{
				if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))// Ideology Offset (outdoors lit):
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.OutdoorsLit".Translate(CasterPawn.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsLitOffset).ToStringPercent()));
				}
				else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.OutdoorsDark".Translate(CasterPawn.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsDarkOffset).ToStringPercent()));
				}
				else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.IndoorsDark".Translate(CasterPawn.GetStatValue(StatDefOf.MeleeHitChanceIndoorsDarkOffset).ToStringPercent()));
				}
				else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.IndoorsLit".Translate(CasterPawn.GetStatValue(StatDefOf.MeleeHitChanceIndoorsLitOffset).ToStringPercent()));
				}
			}
			return;
		}
		public static void DodgeChanceReadout(LocalTargetInfo target, ref StringBuilder stringBuilder, float dodgeChance)
		{
			if (!(target.Thing is Pawn pawn))// no output, no dodge
			{
				return;
			}
			if (pawn.stances.curStance is Stance_Busy stance_Busy && stance_Busy.verb != null && !stance_Busy.verb.verbProps.IsMeleeAttack)
			{
				stringBuilder.AppendLine("   " + "VCR.TargetCannnotDodgeChance".Translate());// Target Dodge Chance: 0% (not in melee)
				return;
			}
			stringBuilder.AppendLine("   " + "VCR.TargetDodgeChance".Translate(dodgeChance.ToStringPercent()));// Target Dodge Chance:


			float num = pawn.GetStatValue(StatDefOf.MeleeDodgeChance);
			if (dodgeChance == num)
			{
				return;
			}
			stringBuilder.AppendLine("      " + "VCR.MeleeDodgeChanceBase".Translate(num.ToStringPercent()));// Base Dodge Chance
			if (ModsConfig.IdeologyActive)
			{
				if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))//ideology offset
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.OutdoorsLit".Translate(pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsLitOffset).ToStringPercent()));
				}
				else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.OutdoorsDark".Translate(pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset).ToStringPercent()));
				}
				else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.IndoorsDark".Translate(pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset).ToStringPercent()));
				}
				else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
				{
					stringBuilder.AppendLine("      " + "VCR.MeleeOffset" + "VCR.IndoorsLit".Translate(pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset).ToStringPercent()));
				}
			}
			return;
		}

		public static void Postfix(ref string __result, Thing target)
        {
            if (Find.Selector.SingleSelectedThing is Pawn pawn && pawn.equipment != null)
            {
                var meleeVerb = pawn.equipment.PrimaryEq?.AllVerbs.OfType<Verb_MeleeAttack>().FirstOrDefault();
                if (meleeVerb != null)
                {
                    StringBuilder stringBuilder = new StringBuilder(__result);
                    var nonMissChance = getNonMissChance(meleeVerb, target);
                    var dodgeChance = getDodgeChance(meleeVerb, target);
					float angle = Quaternion.LookRotation((target.Position.ToVector3() - pawn.Position.ToVector3()).Yto0()).eulerAngles.y;
					var parryChance = Parrying.parryChance(meleeVerb, angle);
					if (Parrying.surpriseattack(meleeVerb))
					{
						stringBuilder.AppendLine("VCR.SurpriseAttack".Translate(1f.ToStringPercent()));//Melee Hit Chance: 100% (suprise attack)
						__result = stringBuilder.ToString();
						return;
					}
					if (Parrying.istargetimmobile(meleeVerb, target))
					{
						stringBuilder.AppendLine("VCR.ImmobileAttack".Translate(1f.ToStringPercent()));//Melee Hit Chance: 100% (Target Immobile)
						__result = stringBuilder.ToString();
						return;
					}
					float hitChance = nonMissChance * (1 - dodgeChance) * (1 - parryChance);
					stringBuilder.AppendLine("VCR.MeleeHitTotal".Translate(hitChance.ToStringPercent()));//Final Melee Hit Chance:
					if(nonMissChance != hitChance)
                    {
						NonMissChanceReadout(pawn, target, nonMissChance, ref stringBuilder);
						DodgeChanceReadout(target, ref stringBuilder, dodgeChance);
						if (parryChance > 0)
						{
						stringBuilder.AppendLine("   "+"VCR.TargetParryChance".Translate(parryChance.ToStringPercent()));
						}
                    }
					if (VanillaCombatMod.settings.MeleeFlanking)
					{
						BodyPartGroupDef side = Flanking.Side(angle, target);
						if (side != null)
						{
							stringBuilder.AppendLine("   " + "VCR.TargetSide".Translate() + ": " + side.LabelShort);
						}
						BodyPartHeight height = pawn.GetTargetHeight();
						if (height != BodyPartHeight.Undefined && target is Pawn)
						{
							stringBuilder.AppendLine("   " + "VCR.HeightChance".Translate() + height.ToStringHuman() + ": " + Flanking.ChanceWithPawnMelee(pawn, (Pawn)target, side, meleeVerb.GetDamageDef(), height, null,BodyPartDepth.Outside).ToStringPercent());
						}
					}

					__result = stringBuilder.ToString();
                }
            }
        }
    }
}
