using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Text;
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
                    stringBuilder.AppendLine("VCR.MeleeHitChance".Translate(nonMissChance.ToStringPercent()));
                    stringBuilder.AppendLine("VCR.TargetDodgeChance".Translate(dodgeChance.ToStringPercent()));
                    __result = stringBuilder.ToString();
                }
            }
        }
    }
}
