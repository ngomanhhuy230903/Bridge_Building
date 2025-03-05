using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadPrefs : MonoBehaviour
{
    [Header("General setting")]
    [SerializeField] private bool canUse = false;
    [SerializeField] private MenuController menuController;
    [Header("Volume setting")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [Header("Brightness setting")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [Header("Quality level setting")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [Header("FullScreen setting")]
    [SerializeField] private Toggle fullScreenToggle;
    [Header("Sentivity setting")]
    [SerializeField] private TMP_Text controllerSenTextValue = null;
    [SerializeField] private Slider controllerSenSlider = null;
    [Header("Invert Y setting")]
    [SerializeField] private Toggle invertYToggle = null;
    public void Awake()
    {
        if (canUse)
        {
            if(PlayerPrefs.HasKey("masterVolume"))
            {
                float localVolume = PlayerPrefs.GetFloat("masterVolume");
                volumeTextValue.text = localVolume.ToString("0.0");
                volumeSlider.value = localVolume;
                AudioListener.volume = localVolume;
            }
            else
            {
                menuController.ResetButton("Audio");
            }
            if(PlayerPrefs.HasKey("masterQuality"))
            {
                int localQuality = PlayerPrefs.GetInt("masterQuality");
                qualityDropdown.value = localQuality;
                QualitySettings.SetQualityLevel(localQuality);

            }
            if(PlayerPrefs.HasKey("masterFullscreen"))
            {
                bool localFullscreen = PlayerPrefs.GetInt("masterFullscreen") == 1 ? true : false;  
                fullScreenToggle.isOn = true;
                Screen.fullScreen = localFullscreen;
            }
            else
            {
                menuController.ResetButton("Graphics");
            }
            if(PlayerPrefs.HasKey("masterBrightness"))
            {
                float localBrightness = PlayerPrefs.GetFloat("masterBrightness");
                brightnessTextValue.text = localBrightness.ToString("0.0");
                brightnessSlider.value = localBrightness;
            }
            if(PlayerPrefs.HasKey("masterSen"))
            {
                float localSen = PlayerPrefs.GetFloat("masterSen");
                controllerSenTextValue.text = localSen.ToString("0.0");
                controllerSenSlider.value = localSen;
                menuController.mainControllerSen = (int)localSen;
            }
            if(PlayerPrefs.HasKey("masterInvertY"))
            {
                if(PlayerPrefs.GetInt("masterInvertY") == 1)
                {
                    invertYToggle.isOn = true;
                }
                else
                {
                    invertYToggle.isOn = false;
                }
            }
            else
            {
                
            }
        }
    }
}
