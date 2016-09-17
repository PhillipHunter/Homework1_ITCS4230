using UnityEngine;

public class Element : MonoBehaviour
{
    public bool mine = false;
    public int x, y;

    private SpriteRenderer _SpriteRenderer;

    void Awake()
    {
        this._SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.name = string.Format("base({0},{1})", x, y);
    }

    public void DetermineMine()
    {
        mine = Random.value < Board.mineFreq;
    }

    public void RenderTexture(int mineCount)
    {
        if (mine)
            _SpriteRenderer.sprite = Board.mineTexture;
        else
            _SpriteRenderer.sprite = Board.infoTextures[mineCount];
    }

    public void SetSprite(Sprite sprite)
    {
        _SpriteRenderer.sprite = sprite;
    }

    public bool IsCovered()
    {
        return _SpriteRenderer.sprite.texture.name == "base";
    }

    void OnMouseUpAsButton()
    {
        if (!Board.gameOver && !Board.gamePaused)
        {
            Board.board.SetTimer(true);

            if (!Board.minesGenerated)
            {
                foreach (Element elem in Board.elements)
                {
                    if (elem != this)
                    {
                        elem.DetermineMine();
                    }
                    else
                    {
                        elem.mine = false;
                    }
                }
                Board.minesGenerated = true;
            }

            if (mine)
            {
                Board.board.GameOver();
            }
            else
            {
                // Using texture show adjacent mine number.

                int surroundingmines = Board.AdjacentMines(x, y);

                if (surroundingmines > 0)
                    RenderTexture(surroundingmines);
                else
                    Board.UncoverFields(x, y, new bool[Board.boardWidth, Board.boardHeight]);

                if (Board.IsFinished())
                {
                    Board.board.GameWin();
                }
            }
        }
    }
}
