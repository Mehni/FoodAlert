using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace FoodAlert
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.FoodAlert.Main");

            harmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoDate)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FoodCounter_NearDatePostfix)), null);
        }

        private static void FoodCounter_NearDatePostfix(float leftX, float width, ref float curBaseY)
        {
            {
                var map = Find.VisibleMap;
                if (map == null || !map.IsPlayerHome) return;
                if (Find.TickManager.TicksGame < 150000) return;

                float totalHumanEdibleNutrition = map.resourceCounter.TotalHumanEdibleNutrition;

                if (totalHumanEdibleNutrition < 4f * (float)map.mapPawns.FreeColonistsSpawnedCount) return;

                int humansGettingFood = map.mapPawns.FreeColonistsSpawnedCount + (from pr in map.mapPawns.PrisonersOfColony
                                                                    where pr.guest.GetsFood
                                                                    select pr).Count<Pawn>();

                int totalDaysOfFood = Mathf.FloorToInt(totalHumanEdibleNutrition / humansGettingFood);
                string daysWorthOfHumanFood = $"{totalDaysOfFood}" + "FoodAlert_DaysOfFood".Translate();
                string addendumForFlavour = string.Empty;

                switch (totalDaysOfFood)
                {
                    case int n when (n >= 100):
                        addendumForFlavour = "FoodAlert_Ridiculous".Translate();
                        break;

                    case int n when (n >= 60):
                        addendumForFlavour = "FoodAlert_Solid".Translate();
                        break;

                    case int n when (n >= 30):
                        addendumForFlavour = "FoodAlert_Bunch".Translate();
                        break;

                    case int n when (n >= 10):
                        addendumForFlavour = "FoodAlert_Decent".Translate();
                        break;

                    default:
                        addendumForFlavour = "FoodAlert_Decent".Translate();
                        break;
                }

                float rightMargin = 7f;
                Rect zlRect = new Rect(UI.screenWidth - Alert.Width, curBaseY - 24f, Alert.Width, 24f);
                Text.Font = GameFont.Small;

                if (Mouse.IsOver(zlRect))
                {
                    Widgets.DrawHighlight(zlRect);
                }

                GUI.BeginGroup(zlRect);
                Text.Anchor = TextAnchor.UpperRight;
                Rect rect = zlRect.AtZero();
                rect.xMax -= rightMargin;
                if (Event.current.capsLock)
                {
                    rect.yMin -= 50f;
                }
                Widgets.Label(rect, daysWorthOfHumanFood);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();

                TooltipHandler.TipRegion(zlRect, new TipSignal(delegate
                {

                    return string.Format("SomeFoodDesc".Translate(), totalHumanEdibleNutrition.ToString("F0"), humansGettingFood.ToStringCached(), totalDaysOfFood.ToStringCached() + addendumForFlavour);
                }, 76515));

                curBaseY -= zlRect.height;
            }
        }
    }
}
