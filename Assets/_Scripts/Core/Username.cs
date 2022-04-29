using TMPro;
using UnityEngine;

public class Username : MonoBehaviour
{
    public static Username Instance;

    [SerializeField] private TMP_Text inputText;

    [SerializeField] private TMP_Text loadedName;

    private string _nameOfPlayer;

    private string _saveName;

    private void Awake()
    {
        Instance = this;
        _nameOfPlayer = PlayerPrefs.GetString("username", "Default");
        loadedName.text = _nameOfPlayer;
    }

    public void UpdateName()
    {
        if (!PlayerPrefs.HasKey("username"))
        {
            PlayerPrefs.SetString("username", "Default");
            loadedName.text = PlayerPrefs.GetString("username");
        }
        else
        {
            _saveName = inputText.text;
            PlayerPrefs.SetString("username", _saveName);
            loadedName.text = _saveName;
        }
    }

    public static string GetName()
    {
        return PlayerPrefs.GetString("username");
    }
}