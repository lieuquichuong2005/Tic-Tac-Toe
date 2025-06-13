using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Cell : MonoBehaviour
{
    public TicTacToe gameManager;

    public int row, column;

    public GameObject cellImage;
    public GameObject highlightImage;
    private void Awake()
    {
        highlightImage = this.gameObject.transform.GetChild(0).gameObject;
        cellImage = this.gameObject.transform.GetChild(1).gameObject;
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
        cellImage.SetActive(true);
        this.GetComponent<Button>().interactable = false;
        cellImage.GetComponent<Image>().sprite = spriteToUpdate;
    }

    
}

