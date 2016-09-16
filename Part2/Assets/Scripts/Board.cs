using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public static Board board;

    // The 2D board
    public static int w = 10; // this is the width
    public static int h = 11; // this is the height

    public static baseElement[,] elements = new baseElement[w, h];

    public static Sprite baseTexture;
    public static Sprite[] infoTextures;
    public static Sprite mineTexture;

    public static Vector3 elementSize;

    private GameObject gameOverCanvas;
    private GameObject gameWinCanvas;
    private Text txtTime;

    private long gameTime;
    private bool timeTicking = false;
    public static bool gameOver = false;

    private void Awake()
    {
        board = this;
        GameObject tempBase = GameObject.Find("tempBase");
        elementSize = tempBase.GetComponent<Renderer>().bounds.size;
        GameObject.Destroy(tempBase);

        Camera.main.transform.position = new Vector3((elementSize.x / 2) * w, (elementSize.y / 2) * h, -1.0f);
        Camera.main.orthographicSize = (h * (80.0f / 11.0f)) / 2.0f;

        baseTexture = GetSprite("base");

        infoTextures = new Sprite[]
        {
            GetSprite("empty"),
            GetSprite("one"),
            GetSprite("two"),
            GetSprite("three"),
            GetSprite("four"),
            GetSprite("five"),
            GetSprite("six"),
            GetSprite("seven"),
            GetSprite("eight"),
        };

        mineTexture = GetSprite("mine");

        gameOverCanvas = GameObject.Find("GameOverCanvas");
        gameOverCanvas.SetActive(false);
        gameWinCanvas = GameObject.Find("GameWinCanvas");
        gameWinCanvas.SetActive(false);

        txtTime = GameObject.Find("txtTime").GetComponent<Text>();
        ResetTimer();
    }

    public static Sprite GetSprite(string name)
    {
        return Resources.Load<Sprite>(string.Format("Sprites/{0}", name));
    }

    private void Start()
    {
        for(int i = 0; i < w; i++)
        {
            for(int j = 0; j < h; j++)
            {
                GameObject obj = Instantiate(Resources.Load<GameObject>("piece"));
                obj.transform.SetParent(this.transform);
                obj.transform.position = new Vector2(elementSize.x * i, elementSize.y * j);                
                Board.elements[i, j] = obj.AddComponent<baseElement>();
                Board.elements[i, j].x = i;
                Board.elements[i, j].y = j;
            }
        }
    }

    // show mines
    public static void showAllMines()
    {
        foreach (baseElement elem in elements)
            if (elem.mine)
                elem.renderTexture(-1);
    }

    // check whether a base element has a mine
    public static bool mineAt(int i, int j)
    {
        if (i >= 0 && j >= 0 && i < w && j < h)
            return elements[i, j].mine;
        else return false;
    }

    // Count adjacent mines for an element
    public static int adjacentMines(int i, int j)
    {
        int count = 0;

        if (mineAt(i - 1, j)) ++count; // left
        if (mineAt(i + 1, j)) ++count; // right

        if (mineAt(i, j + 1)) ++count; // top
        if (mineAt(i, j - 1)) ++count; // bottom

        if (mineAt(i + 1, j + 1)) ++count; // top-right
        if (mineAt(i + 1, j - 1)) ++count; // bottom-right

        if (mineAt(i - 1, j - 1)) ++count; // bottom-left
        if (mineAt(i - 1, j + 1)) ++count; // top-left

        return count;
    }

    public static void uncoverFields(int x, int y, bool[,] visited)
    {
        int minesAround;
        // Coordinates in Range?
        if (x >= 0 && x < w && y >= 0 && y < h)
        {
            if (visited[x, y])
                return;
            else
                visited[x, y] = true;

            minesAround = adjacentMines(x, y);

            // show the number
            elements[x, y].renderTexture(minesAround);

            // game logic
            if (minesAround > 0) return;

            // repeat the process recursively
            uncoverFields(x - 1, y, visited);
            uncoverFields(x + 1, y, visited);
            uncoverFields(x, y - 1, visited);
            uncoverFields(x, y + 1, visited);
        }
    }

    public void ResetTimer()
    {
        timeTicking = false;
        gameTime = 0L;
        txtTime.text = string.Format("Time: {0}", gameTime);
    }

    public void SetTimer(bool state)
    {
        if (state && !timeTicking)
        {
            InvokeRepeating("UpdateTimer", 1.0f, 1.0f);
        }

        timeTicking = state;

        if(!timeTicking)
        {
            CancelInvoke("UpdateTimer");
        }
        
    }

    private void UpdateTimer()
    {
        gameTime++;
        txtTime.text = string.Format("Time: {0}", gameTime);
    }

    public static bool isFinished()
    {
        // Try to find a covered element that is no mine
        foreach (baseElement elem in elements)
            if (elem.isCovered() && !elem.mine)
                return false;
        // There are none => all are mines => game won.
        return true;
    }

    public void GameWin()
    {
        gameOver = true;
        Board.board.SetTimer(false);
        Debug.Log("Congratulation! You Won");

        gameWinCanvas.SetActive(true);
    }

    public void GameOver()
    {
        gameOver = true;
        Board.showAllMines();
        Board.board.SetTimer(false);
        Debug.Log("Sorry, try again.");

        gameOverCanvas.SetActive(true);
    }

    public void btnReplay_Click()
    {
        gameOver = false;
        ResetTimer();

        if (gameOverCanvas.activeInHierarchy)
            gameOverCanvas.SetActive(false);

        if (gameWinCanvas.activeInHierarchy)
            gameWinCanvas.SetActive(false);

        foreach (baseElement elem in elements)
        {
            elem.SetSprite(baseTexture);
            elem.DetermineMine();
        }
    }

    public void btnQuit_Click()
    {
        if(Application.isEditor)
        {
            Debug.Log("Can not close game in an editor run!");
            return;
        }

        Application.Quit();
    }
}