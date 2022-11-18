using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VCR
{
	public class DamageWorker_Bullet : DamageWorker_AddInjury
	{
		public static bool active;
		public static bool flanking;
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			if (!active && !flanking)
            {
				return base.ChooseHitPart(dinfo, pawn);
            }
            if (flanking)
            {
				throw new Exception("flanking not implemeted");
            }
			BodyPartRecord randomNotMissingPart = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
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
			if (!active)
            {
				base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
				return;
            }
			totalDamage /= 2;
			base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
			float stoppingPower = (float)(dinfo.Weapon?.Verbs?.FirstOrDefault()?.defaultProjectile?.projectile?.stoppingPower ?? dinfo.Def.defaultStoppingPower);
			List<BodyPartRecord> list = new List<BodyPartRecord>();
			int num = 0;
			if (stoppingPower < 1.0)
            {
				int num2 = ((def.cutExtraTargetsCurve != null) ? GenMath.RoundRandom(def.cutExtraTargetsCurve.Evaluate(Rand.Value)) : 0);
				if (num2 != 0)
				{
					IEnumerable<BodyPartRecord> enumerable = dinfo.HitPart.GetDirectChildParts();
					if (dinfo.HitPart.parent != null)
					{
						enumerable = enumerable.Concat(dinfo.HitPart.parent);
						if (dinfo.HitPart.parent.parent != null)
						{
							enumerable = enumerable.Concat(dinfo.HitPart.parent.GetDirectChildParts());
						}
					}
					list = (from x in enumerable.Except(dinfo.HitPart).InRandomOrder().Take(num2)
							 where !x.def.conceptual
							 select x).ToList();
				}
			}
			else
            {
				totalDamage *= stoppingPower;
				if(stoppingPower <= 1.5)
                {
					for (BodyPartRecord bodyPartRecord = dinfo.HitPart; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
					{
						list.Add(bodyPartRecord);
						if (bodyPartRecord.depth == BodyPartDepth.Outside)
						{
							break;
						}
					}
					totalDamage *= list.Count;
				}
			}
			if (!list.Contains(dinfo.HitPart))
            {
				list.Add(dinfo.HitPart);
            }
			num = list.Count;
			totalDamage /= num;
			for (int j = 0; j < num; j++)
			{
				DamageInfo dinfo3 = dinfo;
				dinfo3.SetHitPart(list[j]);
				if (dinfo3.HitPart.depth == BodyPartDepth.Outside)
				{
					base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo3, result);
				}
				else
				{
					FinalizeAndAddInjury(pawn, totalDamage, dinfo3, result);
				}
			}
			return;
		}
	}

}
