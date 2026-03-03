
using System;

/// <summary>
/// Class for save data that is locally retained by the <see cref="SaveUtil"/>
/// </summary>
[Serializable]
public class SavedValues
{
    //public bool ToggleValue;
    //public int IntValue;
    //public string StringValue;
    public int HighestLevelCompleted = -1;
    public int[] BestTimeMs;

    public float GlobalVolume = 0.5f;
    public float MusicVolume = 0.5f;
    public float SfxVolume = 0.5f;
}
