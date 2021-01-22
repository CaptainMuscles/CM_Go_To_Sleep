using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Go_To_Sleep
{
    [StaticConstructorOnStartup]
    public static class FloatMenuMakerMap_Patches
    {
        [HarmonyPatch(typeof(FloatMenuMakerMap))]
        [HarmonyPatch("AddHumanlikeOrders", MethodType.Normal)]
        public static class FloatMenuMakerMap_AddHumanlikeOrders
        {
            [HarmonyPostfix]
            public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
            {
                if (pawn.needs == null || pawn.needs.rest == null)
                    return;

                foreach (LocalTargetInfo bed in GenUI.TargetsAt_NewTemp(clickPos, ForSleeping(pawn), thingsOnly: true))
                {
                    if (pawn.needs.rest.CurLevel > RestUtility.FallAsleepMaxLevel(pawn))
                    {
                        opts.Add(new FloatMenuOption("CM_Go_To_Sleep_Cannot_Sleep".Translate() + ": " + "CM_Go_To_Sleep_Not_Tired".Translate().CapitalizeFirst(), null));
                    }
                    else if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
                    {
                        opts.Add(new FloatMenuOption("CM_Go_To_Sleep_Cannot_Sleep".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null));
                    }
                    else
                    {
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CM_Go_To_Sleep_GoToSleep".Translate(), delegate
                        {
                            Job job = JobMaker.MakeJob(JobDefOf.LayDown, bed.Thing);

                            pawn.jobs.TryTakeOrderedJob(job);
                        }, MenuOptionPriority.High), pawn, bed.Thing));
                    }
                }
            }

            private static TargetingParameters ForSleeping(Pawn sleeper)
            {
                return new TargetingParameters
                {
                    canTargetPawns = false,
                    canTargetBuildings = true,
                    mapObjectTargetsMustBeAutoAttackable = false,
                    validator = delegate (TargetInfo targ)
                    {
                        if (!targ.HasThing)
                        {
                            return false;
                        }
                        Building_Bed bed = targ.Thing as Building_Bed;
                        if (bed == null)
                        {
                            return false;
                        }
                        return (!bed.ForPrisoners && !bed.Medical);
                        //return (bed.AnyUnownedSleepingSlot || bed.CompAssignableToPawn.AssignedPawns.Contains(sleeper));
                    }
                };
            }
        }
    }
}
