using System;
using TMPro;
using UnityEngine;

public class UICoinInfo : MonoBehaviour
{
    public static UICoinInfo Instance;

    [SerializeField] private TMP_Text coinsText;

    private void Start()
    {
        Instance = this;
        UpdateCoinsText();
    }

    public void UpdateCoinsText()
    {
        var coin = PlayerPrefs.GetInt("coins");
        coinsText.text = coin.ToString();
    }
}