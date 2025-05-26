using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TicTacToe TicTacToe;
    [Space(20)]
    public GameObject optionsPanel;
    public GameObject boardPanel;
    public GameObject homeButtonPanel;
    public GameObject playWithAIPanel;
    [Space(20)]
    public Button playWithAIButton;
    public Slider boardSizeSlider;
    public TMP_Text boardSizeText;
    public Button playButton;
    [Space(20)]
    public Button playWithFriend;
    [Space(20)]
    public Button settingButton;
    public GameObject settingPanel;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Button saveButton;
    [Space(20)]
    public Button quitButton;
    [Space(20)]
    public GameObject notifyImage;
    public TMP_Text notifyText;
    [Space(20)]
    int boardSize = 10; // Default board size

    public Button returnButton;
    private void Start()
    {
        playWithAIButton.onClick.AddListener(OnPlayWithAIButtonClicked);
        boardSizeSlider.onValueChanged.AddListener(OnBoardSizeSliderChanged);
        returnButton.onClick.AddListener(OnReturnButtonClicked);
        playButton.onClick.AddListener(() => PlayGame(boardSize));
        playWithFriend.onClick.AddListener(OnPlayWithFriendButtonClicked);
        settingButton.onClick.AddListener(OnSettingButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);

        saveButton.onClick.AddListener(OnSaveButtonClicked);

    }
    
    void OnPlayWithAIButtonClicked()
    {
        homeButtonPanel.SetActive(false);
        playWithAIPanel.SetActive(true);
        returnButton.gameObject.SetActive(true);
    }
    void OnBoardSizeSliderChanged(float value)
    {
        if(value == 1)
        {
            boardSize = 10;
        }
        else if(value == 2)
        {
            boardSize = 14;
        }
        else if(value == 3)
        {
            boardSize = 16;
        }
        else if(value == 4)
        {
            boardSize = 20;
        }
        boardSizeText.text = boardSize.ToString();
    }
    void PlayGame(int boardSize)
    {
        optionsPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-600, 0);
        AudioManager.instance.PlayMusic(AudioManager.instance.backgroundMusic_02);
        boardPanel.SetActive(true);
        TicTacToe.CreateBoard(boardSize);
    }
    IEnumerator Notify(string message)
    {
        notifyImage.SetActive(true);
        notifyText.text = message;
        yield return new WaitForSeconds(2f);
        notifyImage.SetActive(false);
    }
    void OnReturnButtonClicked()
    {
        playWithAIPanel.SetActive(false);
        returnButton.gameObject.SetActive(false);
        settingPanel.SetActive(false);
        homeButtonPanel.SetActive(true);
    }
    void OnPlayWithFriendButtonClicked()
    {
        StartCoroutine(Notify("Coming Soon"));
    }
    void OnSettingButtonClicked()
    {
        settingPanel.SetActive(true);
        returnButton.gameObject.SetActive(true);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        homeButtonPanel.SetActive(false);
    }
    void OnSaveButtonClicked()
    {
        AudioManager.instance.SetMusicVolume(musicVolumeSlider.value);
        AudioManager.instance.SetSFXVolume(sfxVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        settingPanel.SetActive(false);
        returnButton.gameObject.SetActive(false);
        homeButtonPanel.SetActive(true);
    }
    void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
