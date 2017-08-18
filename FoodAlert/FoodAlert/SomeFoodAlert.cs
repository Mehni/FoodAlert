using System;
using System.Linq;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace FoodAlert
{
    public class Alert_SomeFood : Alert
    {
        private const float NutritionThresholdPerColonist = 4f;

        public Alert_SomeFood()
        {
            this.defaultLabel = "SomeFood".Translate();
            this.defaultPriority = AlertPriority.Medium;
        }

        public override string GetExplanation()
        {
            Map map = this.MapWithSomeFood();
            if (map == null)
            {
                return string.Empty;
            }
            float totalHumanEdibleNutrition = map.resourceCounter.TotalHumanEdibleNutrition;
            int num = map.mapPawns.FreeColonistsSpawnedCount + (from pr in map.mapPawns.PrisonersOfColony
                                                                where pr.guest.GetsFood
                                                                select pr).Count<Pawn>();
            int num2 = Mathf.FloorToInt(totalHumanEdibleNutrition / (float)num);
            return string.Format("SomeFoodDesc".Translate(), totalHumanEdibleNutrition.ToString("F0"), num.ToStringCached(), num2.ToStringCached());
        }

        public override AlertReport GetReport()
        {
            if (Find.TickManager.TicksGame < 150000)
            {
                return false;
            }
            return this.MapWithSomeFood() != null;
        }

        private Map MapWithSomeFood()
        {
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map.IsPlayerHome)
                {
                    int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
                    if (map.resourceCounter.TotalHumanEdibleNutrition > 4f * (float)freeColonistsSpawnedCount)
                    {
                        return map;
                    }
                }
            }
            return null;
        }
    }
}
