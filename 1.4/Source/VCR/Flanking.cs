using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VCR
{

    public static class Flanking
    {
        public static RotationDirection getdirection(Thing caster, Thing target)
        {
            Rot4 c;
            if(target is Pawn && (target as Pawn).Downed)
            {
                return RotationDirection.Opposite;
            }
            if(caster is Building_TurretGun)
            {
                c = Rot4.FromAngleFlat((caster as Building_TurretGun).Top.CurRotation);
            }
            else
            {
                c = caster.Rotation;
            }
            Rot4 t = target.Rotation;
            return Rot4.GetRelativeRotation(c, t);//none=rear, opposite=front, clockwise=right, counterclockwise=left
        }
        public static RotationDirection getdirection(float angle, Thing target)
        {
            Rot4 c;
            if (target is Pawn && (target as Pawn).Downed)
            {
                return RotationDirection.Opposite;
            }
            c = Rot4.FromAngleFlat(angle);
            Rot4 t = target.Rotation;
            return Rot4.GetRelativeRotation(c, t);//none=rear, opposite=front, clockwise=right, counterclockwise=left
        }

        public static BodyPartGroupDef Side(Thing Caster, Thing Target)
        {
            RotationDirection a = getdirection(Caster, Target);
            switch (a)
            {
                case RotationDirection.Clockwise:
                    return SideGroupOf.Right;//contains all right and center
                case RotationDirection.Counterclockwise:
                    return SideGroupOf.Left;//contains all left and center
                case RotationDirection.None:
                    return SideGroupOf.Center;//contains all that cover left and right or torso
                case RotationDirection.Opposite:
                    return null;
                default:
                    return null;
            }
        }//grab side to use
        public static BodyPartGroupDef Side(float angle, Thing Target)
        {
            RotationDirection a = getdirection(angle, Target);
            switch (a)
            {
                case RotationDirection.Clockwise:
                    return SideGroupOf.Right;//contains all right and center
                case RotationDirection.Counterclockwise:
                    return SideGroupOf.Left;//contains all left and center
                case RotationDirection.None:
                    return SideGroupOf.Center;//contains all that cover left and right or torso
                case RotationDirection.Opposite:
                    return null;
                default:
                    return null;
            }
        }//grab side to use
        public static IEnumerable<BodyPartRecord> GetNotMissingPartsWithGroup(Pawn pawn, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartTagDef tag = null, BodyPartRecord partParent = null, BodyPartGroupDef group = null)
        {
            if(group == null)
            {
                return pawn.health.hediffSet.GetNotMissingParts(height, depth, tag, partParent);
            }
            return pawn.health.hediffSet.GetNotMissingParts(height, depth, tag, partParent).Where((BodyPartRecord p) => p.groups.Contains(group));
        }//list of parts to hit with that side and height
        public static float Bodychance(Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            var list = GetNotMissingPartsWithGroup(target, height, depth, tag, partParent, side).ToList();
            if (list.Any((BodyPartRecord p) => p.coverageAbs > 0f) is false)
            {
                return 0;
            }
            float a = 0;
            foreach (BodyPartRecord x in list)
            {
                a += x.coverageAbs * x.def.GetHitChanceFactorFor(damDef);
            }
            return a;
        }//calculate chance total coverage of that height and group
        public static float relBodychance(Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            return Bodychance(target, side, damDef, height, tag, depth, partParent)/ Bodychance(target, side, damDef, BodyPartHeight.Undefined, tag, depth, partParent);
        }//calculate hit chance of height relative to the total leftover coverage of hittable parts on that side
        public static float ChanceWithPawn(Thing caster,Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            float a = relBodychance(target, side, damDef, height, tag, depth, partParent);
            return ShotReport_HitReportFor_Patch.statpush(a, caster);
        }//adjustment of hit chance according to pawn's skill
        public static float ChanceWithPawnMelee(Thing caster, Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            float a = relBodychance(target, side, damDef, height, tag, depth, partParent);
            return ShotReport_HitReportFor_Patch.statpushMelee(a, caster);
        }//adjustment of hit chance according to pawn's skill
        public static BodyPartRecord flank(Thing caster, float angle, Pawn target, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            BodyPartGroupDef side = Side(angle, target);
            if (Rand.Chance(ChanceWithPawn(caster, target, side, damDef, height, tag, depth, partParent)))
            {
                if (GetNotMissingPartsWithGroup(target, height, depth, tag, partParent, side).TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs * x.def.GetHitChanceFactorFor(damDef), out var result))
                {
                    return result;
                }
            }
            else if (GetNotMissingPartsWithGroup(target, BodyPartHeight.Undefined, depth, tag, partParent, side).TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out var result))
            {
                return result;
            }
            return null;
        }//"worker" hooks into damage worker and outputs the hit target, if you fail hit of target, it rolls once more to hit a part on that side with any height

        public static BodyPartRecord flankMelee(Thing caster, float angle, Pawn target, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            BodyPartGroupDef side = Side(angle, target);
            if (Rand.Chance(ChanceWithPawnMelee(caster, target, side, damDef, height, tag, depth, partParent)))
            {
                if (GetNotMissingPartsWithGroup(target, height, depth, tag, partParent, side).TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs * x.def.GetHitChanceFactorFor(damDef), out var result))
                {
                    return result;
                }
            }
            else if (GetNotMissingPartsWithGroup(target, BodyPartHeight.Undefined, depth, tag, partParent, side).TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out var result))
            {
                return result;
            }
            return null;
        }
        public static bool MeleeFlanking;
        public static BodyPartRecord MeleeFlankHandler(HediffSet hediffset, DamageDef def, BodyPartHeight height, BodyPartDepth depth, BodyPartRecord parentpart, ref DamageInfo dinfo, Pawn pawn)
        {
            if (MeleeFlanking)
            {
                var targetHeight = dinfo.Instigator.GetTargetHeight();
                return flankMelee(dinfo.Instigator, dinfo.Angle, pawn, def, targetHeight, null, depth, parentpart);
            }
            return hediffset.GetRandomNotMissingPart(def, height, depth, parentpart);
        }
    }
    [HarmonyPatch]
    public static class MeleeFlankingTranspiler
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(DamageWorker_Bite), "ChooseHitPart");
            yield return AccessTools.Method(typeof(DamageWorker_Blunt), "ChooseHitPart");
            yield return AccessTools.Method(typeof(DamageWorker_Cut), "ChooseHitPart");
            yield return AccessTools.Method(typeof(DamageWorker_Scratch), "ChooseHitPart");
            yield return AccessTools.Method(typeof(DamageWorker_Stab), "ChooseHitPart");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodBase from = AccessTools.Method(typeof(HediffSet), "GetRandomNotMissingPart");
            MethodBase to = AccessTools.Method(typeof(Flanking), "MeleeFlankHandler");

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.operand as MethodBase == from)//find method to replace
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, to);
                    continue;
                }
                yield return instruction;
            }
        }
    }
    [DefOf]
    public static class SideGroupOf
    {
        public static BodyPartGroupDef Left;
        public static BodyPartGroupDef Right;
        public static BodyPartGroupDef Center;
        static SideGroupOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SideGroupOf));
        }
    }//body group tags, xml patch is configured to always output something to hit for center (all torso parts are included in center)

}
