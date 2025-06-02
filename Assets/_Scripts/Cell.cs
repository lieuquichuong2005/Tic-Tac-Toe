using System.Collections;
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
        cellImage = this.gameObject.transform.GetChild(0).gameObject;
    }

    public void OnMouseDown()
    {
        if (gameManager.isGameActive)
        {

            gameManager.HandlePlayerMove(this.row, this.column);
        }
    }
    public IEnumerator HandleCellClick(Sprite sprite)
    {
        // Hiệu ứng nhấp nháy
        Color originalColor = GetComponent<Image>().color;
        GetComponent<Image>().color = Color.white; // Màu sáng lên
        yield return new WaitForSeconds(0.1f);
        GetComponent<Image>().color = Color.blue;
        yield return new WaitForSeconds(0.1f);
        GetComponent<Image>().color = originalColor; // Trở lại màu ban đầu

        SetCellState(sprite);
        //gameManager.HandlePlayerMove(this.row, this.column);
    }
    public void SetCellState(Sprite spriteToUpdate)
    {
        cellImage.SetActive(true);
        this.GetComponent<Button>().interactable = false;
        cellImage.GetComponent<Image>().sprite = spriteToUpdate;
    }

    
}

