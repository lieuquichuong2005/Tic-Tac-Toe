using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameModeType
{
    Offline,
    Online
}

public class PlayGameManager : MonoBehaviour
{
    public TicTacToe tictactoe;

    public Button replayButton;
    public Button homeButton;

    private void Start()
    {
        replayButton.onClick.AddListener(OnReplayButtonClicked);
        homeButton.onClick.AddListener(OnHomeButtonClicked);
    }


    void OnReplayButtonClicked()
    {
        tictactoe.CreateBoard(PlayerPrefs.GetInt("BoardSize"));
    }
    void OnHomeButtonClicked()
    {
        SceneManager.LoadScene(0);
    }    
}
