using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
            IEnumerable<BodyPartRecord> enumerable = null;
            if (GetNotMissingPartsWithGroup(target, height, depth, tag, partParent, side).Any((BodyPartRecord p) => p.coverageAbs > 0f))//check for with height able to hit side
            {
                enumerable = GetNotMissingPartsWithGroup(target, height, depth, tag, partParent, side);
            }
            else
            {
                return 0;
            }
            float a = 0;
            foreach (BodyPartRecord x in enumerable)
            {
                a += x.coverageAbs * x.def.GetHitChanceFactorFor(damDef);
            }
            return a;
        }
        public static float relBodychance(Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            return Bodychance(target, side, damDef, height, tag, depth, partParent)/ Bodychance(target, side, damDef, BodyPartHeight.Undefined, tag, depth, partParent);
        }
        public static float ChanceWithPawn(Thing caster,Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            float a = relBodychance(target, side, damDef, height, tag, depth, partParent);
            return ShotReport_HitReportFor_Patch.statpush(a, caster);
        }
        public static BodyPartRecord flank(Thing caster, Pawn target, BodyPartGroupDef side, DamageDef damDef, BodyPartHeight height = BodyPartHeight.Undefined, BodyPartTagDef tag = null, BodyPartDepth depth = BodyPartDepth.Undefined, BodyPartRecord partParent = null)
        {
            IEnumerable<BodyPartRecord> enumerable = null;
            float a = ChanceWithPawn(caster, target, side, damDef, height, tag, depth, partParent);
            if (Rand.Chance(a))
            {
                enumerable = GetNotMissingPartsWithGroup(target, height, depth, tag, partParent, side);
            }
            else
            {
                enumerable = GetNotMissingPartsWithGroup(target, BodyPartHeight.Undefined, depth, tag, partParent, side);
            }
            if (enumerable.TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs * x.def.GetHitChanceFactorFor(damDef), out var result))
            {
                return result;
            }
            if (enumerable.TryRandomElementByWeight((BodyPartRecord x) => x.coverageAbs, out result))
            {
                return result;
            }
            return null;
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
    }

}
