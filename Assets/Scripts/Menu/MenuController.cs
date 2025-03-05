using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class MenuController : MonoBehaviour
{
    [Header("Volume setting")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private float defaultVolume = 1.0f;

    [Header("Gameplay setting")]
    [SerializeField] private TMP_Text controllerSenTextValue = null;
    [SerializeField] private Slider controllerSenSlider = null;
    [SerializeField] private float defaultSen = 4.0f;
    public int mainControllerSen = 4;

    [Header("Toggle setting")]
    [SerializeField] private Toggle invertYToggle = null;

    [Header("Graphics setting")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [SerializeField] private float defaultBrightness = 1f;

    [Space(10)]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullScreenToggle;

    private float _qualityLevel;
    private bool _isFullScreen;
    private float _brightnessLevel;
    
    [Header("Confirmation")]
    [SerializeField] private GameObject confirmationPrompt = null; 

    [Header("Levels to load")]
    [SerializeField] private GameObject noSaveGameDialog = null;
    public string newGameLevel;
    private string leveltoLoad;

    [Header("Resolution setting")]   
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private void Start()
    {
        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();

            int currentResolutionIndex = 0;
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + "x" + resolutions[i].height;
                options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options); // Cách này vẫn đúng
            resolutionDropdown.value = currentResolutionIndex; // Cách này vẫn đúng
            resolutionDropdown.RefreshShownValue(); // Cách này vẫn đúng
        }

    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }



    public void NewGameDialogYes()
    {
        SceneManager.LoadScene(newGameLevel);
    }
    public void LoadGameDialogYes()
    {
        if(PlayerPrefs.HasKey("SavedLevel"))
        {
            leveltoLoad = PlayerPrefs.GetString("SavedLevel");
            SceneManager.LoadScene(leveltoLoad);
        }
        else
        {

           noSaveGameDialog.SetActive(true);
        }
    }
    public void  ExitButton()
    {
        Application.Quit();
    }
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        volumeTextValue.text = volume.ToString("0.0");
    }
    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }
    public void SetControllerSen(float sen)
    {
        controllerSenTextValue.text = sen.ToString("0.0");
        mainControllerSen = (int)sen;
    }
    public void GameplayApply()
    {
        if(invertYToggle.isOn)
        {
            PlayerPrefs.SetInt("invertY", 1);
        }
        else
        {
            PlayerPrefs.SetInt("invertY", 0);
        }
        PlayerPrefs.SetInt("masterSen", mainControllerSen);
        StartCoroutine(ConfirmationBox());
    }
    public void SetBrightness(float brightness)
    {
        brightnessTextValue.text = brightness.ToString("0.0");
        _brightnessLevel = brightness;
    }
    public void SetFullScreen(bool isFullScreen)
    {
        _isFullScreen = isFullScreen;
    }
    public void SetQuality(float qualityIndex)
    {
        _qualityLevel = qualityIndex;
    }
    public void GraphicsApply()
    {

       PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);
        PlayerPrefs.SetInt("masterQuality", (int)_qualityLevel);
        QualitySettings.SetQualityLevel((int)_qualityLevel);
        PlayerPrefs.SetInt("masterFullScreen", _isFullScreen ? 1 : 0);
        Screen.fullScreen = _isFullScreen;
        StartCoroutine(ConfirmationBox());
    }
    public void ResetButton(string MenuType)
    {
        if(MenuType == "Graphics")
        {
            // Reset brightness value
            brightnessSlider.value = defaultBrightness;
            brightnessTextValue.text = defaultBrightness.ToString("0.0");

            qualityDropdown.value = 1;
            QualitySettings.SetQualityLevel(1);

            fullScreenToggle.isOn = true;
            Screen.fullScreen = false;

            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
            resolutionDropdown.value = resolutions.Length;
            GraphicsApply();
        }
        if(MenuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("0.0");
            VolumeApply();
        }
        if(MenuType == "Gameplay")
        {
            controllerSenSlider.value = defaultSen;
            controllerSenTextValue.text = defaultSen.ToString("0.0");
            mainControllerSen = (int)defaultSen;
            invertYToggle.isOn = false;
            GameplayApply();
        }
    }
    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }
}
