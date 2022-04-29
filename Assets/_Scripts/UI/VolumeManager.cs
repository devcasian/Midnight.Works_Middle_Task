using System;
using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    private static readonly string FirstPlay = "FirstPlay";

    private static readonly string MusicPref = "MusicPref";

    private static readonly string SoundEffectsPref = "SoundsEffectsPref";

    [SerializeField] private Slider musicSlider;

    [SerializeField] private Slider soundEffectsSlider;

    [SerializeField] private AudioSource musicAudio;

    [SerializeField] private AudioSource[] soundEffectsAudio;

    private int _firstPlayInt;

    private float _musicFloat;

    private float _soundEffectsFloat;

    private void Start()
    {
        _firstPlayInt = PlayerPrefs.GetInt(FirstPlay);

        if (_firstPlayInt == 0)
        {
            _musicFloat = 0.25f;
            _soundEffectsFloat = 0.75f;
            musicSlider.value = _musicFloat;
            soundEffectsSlider.value = _soundEffectsFloat;

            PlayerPrefs.SetFloat(MusicPref, _musicFloat);
            PlayerPrefs.SetFloat(SoundEffectsPref, _soundEffectsFloat);
            PlayerPrefs.SetInt(FirstPlay, -1);
        }
        else
        {
            _musicFloat = PlayerPrefs.GetFloat(MusicPref);
            musicSlider.value = _musicFloat;

            _soundEffectsFloat = PlayerPrefs.GetFloat(SoundEffectsPref);
            soundEffectsSlider.value = _soundEffectsFloat;
        }
    }

    public void SaveSoundSettings()
    {
        PlayerPrefs.SetFloat(MusicPref, musicSlider.value);
        PlayerPrefs.SetFloat(SoundEffectsPref, soundEffectsSlider.value);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveSoundSettings();
        }
    }

    public void UpdateSound()
    {
        musicAudio.volume = musicSlider.value;

        foreach (var effect in soundEffectsAudio)
        {
            effect.volume = soundEffectsSlider.value;
        }
    }
}