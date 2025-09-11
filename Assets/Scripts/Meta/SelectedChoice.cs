using UnityEngine;

public static class SelectedChoice
{
    public static GameObject PlayerPrefab;
    public static CharacterData CharacterData;

    public static bool HasSelection => PlayerPrefab != null && CharacterData != null;

    public static void Clear()
    {
        PlayerPrefab = null;
        CharacterData = null;
    }

    public static void LogState(string where)
    {
        Debug.Log($"[SelectedChoice] {where} -> " +
                  $"HasSelection={HasSelection}, " +
                  $"Prefab={(PlayerPrefab ? PlayerPrefab.name : "null")}, " +
                  $"Data={(CharacterData ? CharacterData.name : "null")}");
    }
}
