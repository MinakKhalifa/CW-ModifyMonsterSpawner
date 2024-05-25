using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;

namespace ModifyMonsterSpawner.Patches
{
    [HarmonyPatch(typeof(RoundSpawner))]
    public class MonsterSpawnerPatch
    {
        // Патч для метода Start в классе RoundSpawner
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void RoundSpawnerStartPatch(RoundSpawner __instance)
        {
            int Modify = ModifySpawnerConfig.SpawnModify;
            float secondsDivider = ModifySpawnerConfig.SpawnDuration;
            Debug.Log($"Modify: {Modify}");
            // Удвоение размера первой волны
            FieldInfo testFirstWaveSizeField = typeof(RoundSpawner).GetField("testFirstWaveSize", BindingFlags.Instance | BindingFlags.Public);
            if (testFirstWaveSizeField != null)
            {
                Vector2 originalFirstWaveSize = (Vector2)testFirstWaveSizeField.GetValue(__instance);
                testFirstWaveSizeField.SetValue(__instance, new Vector2(originalFirstWaveSize.x * Modify, originalFirstWaveSize.y * Modify));
                Debug.Log($"Modified first wave size: {originalFirstWaveSize} -> {(Vector2)testFirstWaveSizeField.GetValue(__instance)}");
            }
            else
            {
                Debug.LogError("Failed to find testFirstWaveSize field.");
            }

            // Удвоение бюджета для спавна
            FieldInfo testBudgetField = typeof(RoundSpawner).GetField("testBudget", BindingFlags.Instance | BindingFlags.Public);
            if (testBudgetField != null)
            {
                int originalBudget = (int)testBudgetField.GetValue(__instance);
                testBudgetField.SetValue(__instance, originalBudget * Modify);
                Debug.Log($"Modified spawn budget: {originalBudget} -> {originalBudget * Modify}");
            }
            else
            {
                Debug.LogError("Failed to find testBudget field.");
            }

            // Уменьшение времени ожидания между волнами
            FieldInfo testMinMaxSecondWaveWaitTimeField = typeof(RoundSpawner).GetField("testMinMaxSecondWaveWaitTime", BindingFlags.Instance | BindingFlags.Public);
            if (testMinMaxSecondWaveWaitTimeField != null)
            {
                Vector2 originalWaitTime = (Vector2)testMinMaxSecondWaveWaitTimeField.GetValue(__instance);
                testMinMaxSecondWaveWaitTimeField.SetValue(__instance, new Vector2(originalWaitTime.x / secondsDivider, originalWaitTime.y / secondsDivider));
                Debug.Log($"Modified second wave wait time: {originalWaitTime} -> {(Vector2)testMinMaxSecondWaveWaitTimeField.GetValue(__instance)}");
            }
            else
            {
                Debug.LogError("Failed to find testMinMaxSecondWaveWaitTime field.");
            }

            // Обеспечим запуск второй волны с уменьшенным временем ожидания
            MethodInfo spawnSecondRunMethod = typeof(RoundSpawner).GetMethod("SpawnSecondRun", BindingFlags.NonPublic | BindingFlags.Instance);
            if (spawnSecondRunMethod != null)
            {
                int budgetForSecondWave = (int)testBudgetField.GetValue(__instance); // Используем удвоенный бюджет для второй волны
                FieldInfo testBiggestPurchaseField = typeof(RoundSpawner).GetField("testBiggestPurchase", BindingFlags.Instance | BindingFlags.Public);
                if (testBiggestPurchaseField != null)
                {
                    int biggestPurchase = (int)testBiggestPurchaseField.GetValue(__instance);
                    float newWaitTime = secondsDivider; // Уменьшенное время ожидания
                    __instance.StartCoroutine(DelayedSecondWaveSpawn(__instance, spawnSecondRunMethod, newWaitTime, budgetForSecondWave, biggestPurchase));
                }
                else
                {
                    Debug.LogError("Failed to find testBiggestPurchase field.");
                }
            }
            else
            {
                Debug.LogError("Failed to find SpawnSecondRun method.");
            }
        }

        private static System.Collections.IEnumerator DelayedSecondWaveSpawn(RoundSpawner instance, MethodInfo spawnSecondRunMethod, float waitTime, int budgetForSpawn, int biggestPurchase)
        {
            yield return new WaitForSeconds(1f); // Ждем немного перед запуском второй волны
            try
            {
                spawnSecondRunMethod.Invoke(instance, new object[] { waitTime, budgetForSpawn, biggestPurchase });
                Debug.Log("Spawned second wave with modified settings.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error invoking SpawnSecondRun: {ex}");
            }
        }
    }
}