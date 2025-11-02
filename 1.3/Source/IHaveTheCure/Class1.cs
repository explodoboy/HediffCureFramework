using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IHaveTheCure
{
    public class IHaveTheCureMod : Mod
    {
        public IHaveTheCureMod(ModContentPack pack) : base(pack)
        {
            new Harmony("IHaveTheCure.Mod").PatchAll();
        }
    }

	[HarmonyPatch(typeof(TickManager), "DoSingleTick")]
	public static class DoSingleTick_Patch
	{
		public static List<Hediff> hediffsToRemove = new List<Hediff>();
		public static void Postfix()
		{
			for (var i = 0; i < hediffsToRemove.Count; i++)
            {
				var hediff = hediffsToRemove[i];
				var pawn = hediff.pawn;
				if (pawn != null)
                {
					pawn.health.RemoveHediff(hediff);
                }
			}
			hediffsToRemove.Clear();
		}
	}

	[HarmonyPatch(typeof(HediffDef), "SpecialDisplayStats")]
	public static class HediffDef_SpecialDisplayStats_Patch
	{
		public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, HediffDef __instance)
		{
			foreach (var r in __result)
			{
				yield return r;
			}

			var compProps = __instance.CompProps<HediffCompProperties_CuresHediffs>();
			if (compProps != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "IHTC.CuresHediff".Translate(), compProps.hediffsToCure.Select((HediffDef im) => im.label).ToCommaList().CapitalizeFirst(),
					"IHTC.Stat_Hediff_CuresHediff_Desc".Translate(), 4051);
			}
		}
	}


	[HarmonyPatch(typeof(HediffStatsUtility), "SpecialDisplayStats")]
	public static class HediffStatsUtility_SpecialDisplayStats_Patch
	{
		public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> __result, HediffStage stage, Hediff instance)
		{
			foreach (var r in __result)
			{
				yield return r;
			}
			var comp = instance.TryGetComp<HediffComp_CuresHediffs>();
			if (comp != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.CapacityEffects, "IHTC.CuresHediff".Translate(), comp.Props.hediffsToCure.Select((HediffDef im) => im.label).ToCommaList().CapitalizeFirst(),
					"IHTC.Stat_Hediff_CuresHediff_Desc".Translate(), 4051);
			}
		}
	}

	public class HediffCompProperties_CuresHediffs : HediffCompProperties
	{
		public List<HediffDef> hediffsToCure;
		public HediffCompProperties_CuresHediffs()
		{
			compClass = typeof(HediffComp_CuresHediffs);
		}
	}
	public class HediffComp_CuresHediffs : HediffComp
    {
		public HediffCompProperties_CuresHediffs Props => (HediffCompProperties_CuresHediffs)props;
        public override void CompPostMake()
        {
            base.CompPostMake();
			TryRemoveHediffs();
		}

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
			TryRemoveHediffs();
		}

		private void TryRemoveHediffs()
        {
			foreach (var hediff in Pawn.health.hediffSet.hediffs)
            {
				if (Props.hediffsToCure.Contains(hediff.def) && !DoSingleTick_Patch.hediffsToRemove.Contains(hediff))
                {
					DoSingleTick_Patch.hediffsToRemove.Add(hediff);
				}
            }
        }
    }
}
