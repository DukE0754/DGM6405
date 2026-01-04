using System;
using System.IO;
using UnityEngine;

public static class SaveUtil
{
    public static Action OnSaveCompleted;
    public static Action OnLoadCompleted;
    public static SavedValues SavedValues;
    
    private static string SaveDataPath => Path.Combine(Application.persistentDataPath, "SaveData.json");
    
    public static void Save()
    {
        var savedJson = JsonUtility.ToJson(SavedValues);
        // todo use async but careful of double operations
        File.WriteAllText(SaveDataPath, savedJson);
        OnSaveCompleted?.Invoke();
        OnSaveCompleted = null;
    }
    
    public static void Load()
    {
        if (File.Exists(SaveDataPath))
        {
            var savedJson = File.ReadAllText(SaveDataPath);
            SavedValues = JsonUtility.FromJson<SavedValues>(savedJson);
        }
        else
        {
            SavedValues = new SavedValues();
        }
        
        OnLoadCompleted?.Invoke();
        OnLoadCompleted = null;
    }

    /// <summary>
    /// Delete the save data
    /// </summary>
    public static void DeleteSaveData()
    {
        if (!File.Exists(SaveDataPath))
            return;
        
        File.Delete(SaveDataPath);
        SavedValues = new SavedValues();
    }
}
