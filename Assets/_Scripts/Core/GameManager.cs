using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    private void Awake()
    {
        _instance = this;
        LoadQualitySettings();
    }

    private static void LoadQualitySettings()
    {
        var qualityIndex = PlayerPrefs.GetInt("QualitySettingsPreference");
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}