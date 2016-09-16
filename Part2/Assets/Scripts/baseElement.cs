using UnityEngine;
using System.Collections;

public class baseElement : MonoBehaviour
{
    public bool mine;
    public int x, y;
    
    void Start()
    {
        // An element has 0.1 probabilty to be a mine (a kind of randomness)
        DetermineMine();

        transform.name = string.Format("base({0},{1})", x, y);
    }

    public void DetermineMine()
    {
        mine = Random.value < 0.10;
    }

    // Load a texture
    public void renderTexture(int mineCount)
    {
        if (mine)
            GetComponent<SpriteRenderer>().sprite = Board.mineTexture;
        else
            GetComponent<SpriteRenderer>().sprite = Board.infoTextures[mineCount];
    }

    public void SetSprite(Sprite sprite)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public bool isCovered()
    {
        return GetComponent<SpriteRenderer>().sprite.texture.name == "base";
    }

    void OnMouseUpAsButton()
    {
        if(!Board.gameOver)
        {
            Board.board.SetTimer(true);

            if (mine)
            {
                Board.board.GameOver();
            }
            else
            {
                // using texture show adjacent mine number     

                int surroundingmines = Board.adjacentMines(x, y);

                if (surroundingmines > 0)
                    renderTexture(surroundingmines);
                else
                    Board.uncoverFields(x, y, new bool[Board.w, Board.h]);

                if (Board.isFinished())
                {                    
                    Board.board.GameWin();
                }
            }
        }
    }
}
