using TMPro;
using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown qualityDropdown;

    private void Awake()
    {
        LoadQualitySettings();
    }


    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SaveQualitySettings()
    {
        PlayerPrefs.SetInt("QualitySettingsPreference", qualityDropdown.value);
    }

    private void LoadQualitySettings()
    {
        qualityDropdown.value = PlayerPrefs.HasKey("QualitySettingsPreference")
            ? PlayerPrefs.GetInt("QualitySettingsPreference")
            : 3;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveQualitySettings();
        }
    }

    public void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        UICoinInfo.Instance.UpdateCoinsText();
        Username.Instance.UpdateName();
        PlayerPrefs.SetFloat("carAcceleration", 1f);
        PlayerPrefs.SetFloat("carMaxSpeed", 4f);
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs Deleted");
    }

    public void EnforceCar()
    {
        var acceleration = PlayerPrefs.GetFloat("carAcceleration");
        var maxSpeed = PlayerPrefs.GetFloat("carMaxSpeed");


        acceleration += 0.1f;
        maxSpeed -= 0.3f;

        PlayerPrefs.SetFloat("carAcceleration", acceleration);
        PlayerPrefs.SetFloat("carMaxSpeed", maxSpeed);

        Debug.Log("Car acceleration enforced!" + PlayerPrefs.GetFloat("carAcceleration"));
        Debug.Log("Car max speed enforced!" + PlayerPrefs.GetFloat("carMaxSpeed"));
    }
}