using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Cell : MonoBehaviour
{
    public TicTacToe gameManager;

    public int row, column;

    public GameObject cellImage;
    private void Awake()
    {
        gameManager = GameObject.FindWithTag("GameManager").GetComponent<TicTacToe>();
        cellImage = this.gameObject.transform.GetChild(0).gameObject;
    }

    public void OnMouseDown()
    {
        if (gameManager.isGameActive)
        {

            gameManager.HandlePlayerMove(this.row, this.column);
        }
    }
    public void SetCellState(Sprite spriteToUpdate)
    {
        this.GetComponent<Button>().interactable = false;
        cellImage.SetActive(true);
        cellImage.GetComponent<Image>().sprite = spriteToUpdate;
    }
    public string GetCellState()
    {
        return this.gameObject.GetComponentInChildren<TMP_Text>().text;
    }
}

