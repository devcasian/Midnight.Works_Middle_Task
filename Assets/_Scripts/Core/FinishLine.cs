using System;
using TMPro;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameObject finishMenu;

    [SerializeField] private TMP_Text raceWinner;

    private static string _playerUsername;

    private void Awake()
    {
        _playerUsername = Username.GetName();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Player"))
        {
            Time.timeScale = 0f;
            finishMenu.SetActive(true);
            AudioListener.pause = true;
            raceWinner.text = "Winner is <color=red>" + _playerUsername + "</color>";
        }

        if (col.CompareTag("AI"))
        {
            Time.timeScale = 0f;
            finishMenu.SetActive(true);
            AudioListener.pause = true;
            raceWinner.text = "Winner is <color=red>AI</color>";
        }
    }
}