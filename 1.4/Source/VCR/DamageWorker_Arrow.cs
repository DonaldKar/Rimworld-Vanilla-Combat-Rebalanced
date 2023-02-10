using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VCR
{
    public class DamageWorker_Arrow : DamageWorker_AddInjury
    {
        public static bool active;
		public static bool flanking;
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			if (!active && !flanking)
			{
				return base.ChooseHitPart(dinfo, pawn);
			}
			BodyPartRecord randomNotMissingPart;
			if (flanking)
			{
				var targetHeight = dinfo.Instigator.GetTargetHeight();
				randomNotMissingPart = Flanking.flank(dinfo.Instigator, dinfo.Angle, pawn, dinfo.Def, targetHeight, null, dinfo.Depth, null);
				if (randomNotMissingPart.depth != BodyPartDepth.Inside && Rand.Chance(def.stabChanceOfForcedInternal))
				{
					BodyPartRecord randomNotMissingPart2 = Flanking.flank(dinfo.Instigator, dinfo.Angle, pawn, dinfo.Def, BodyPartHeight.Undefined, null, BodyPartDepth.Inside, randomNotMissingPart);
					if (randomNotMissingPart2 != null)
					{
						return randomNotMissingPart2;
					}
				}
				return randomNotMissingPart;
			}
			randomNotMissingPart = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
			if (randomNotMissingPart.depth != BodyPartDepth.Inside && Rand.Chance(def.stabChanceOfForcedInternal))
			{
				BodyPartRecord randomNotMissingPart2 = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, BodyPartHeight.Undefined, BodyPartDepth.Inside, randomNotMissingPart);
				if (randomNotMissingPart2 != null)
				{
					return randomNotMissingPart2;
				}
			}
			return randomNotMissingPart;
		}
		protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
		{
			if (!active || result.diminished || dinfo.HitPart.depth == BodyPartDepth.Inside)
			{
				base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
				return;
			}
			IEnumerable<BodyPartRecord> enumerable = dinfo.HitPart.GetDirectChildParts();
			if (dinfo.HitPart.parent != null)
			{
				enumerable = enumerable.Concat(dinfo.HitPart.parent);
				if (dinfo.HitPart.parent.parent != null)
				{
					enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
				}
			}
			enumerable = enumerable.Where((BodyPartRecord target) => target != dinfo.HitPart && !target.def.conceptual && target.depth == BodyPartDepth.Outside && !pawn.health.hediffSet.PartIsMissing(target));
			BodyPartRecord bodyPartRecord2 = enumerable.RandomElementWithFallback();
			if (bodyPartRecord2 == null)
			{
				FinalizeAndAddInjury(pawn, ReduceDamageToPreserveOutsideParts(totalDamage, dinfo, pawn), dinfo, result);
				return;
			}
			FinalizeAndAddInjury(pawn, ReduceDamageToPreserveOutsideParts(totalDamage * def.scratchSplitPercentage, dinfo, pawn), dinfo, result);
			DamageInfo dinfo3 = dinfo;
			dinfo3.SetHitPart(bodyPartRecord2);
			FinalizeAndAddInjury(pawn, ReduceDamageToPreserveOutsideParts(totalDamage * def.scratchSplitPercentage, dinfo3, pawn), dinfo3, result);
		}
	}
}
