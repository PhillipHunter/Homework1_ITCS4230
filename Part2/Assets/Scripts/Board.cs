using System;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public static Board board;

    // The 2D board
    public static int w = 11; // this is the width
    public static int h = 11; // this is the height

    public static Element[,] elements = new Element[w, h];

    public static Sprite baseTexture;
    public static Sprite[] infoTextures;
    public static Sprite mineTexture;

    public static Vector3 elementSize;

    private GameObject gameOverCanvas;
    private GameObject gameWinCanvas;
    private GameObject optionsCanvas;

    private Text txtTime;
    private Slider sliderSize;
    private Text sizeIndicator;

    public bool minesGenerated = false;

    public static bool gamePaused;
    private long gameTime;    
    private bool timeTicking = false;
    public static bool gameOver = false;

    private Transform sun;
    private float sunTime;

    [SerializeField]
    private float testTime = -1;    

    private void Awake()
    {
        board = this;
        GameObject tempBase = GameObject.Find("tempBase");
        elementSize = tempBase.GetComponent<Renderer>().bounds.size;
        GameObject.Destroy(tempBase);        

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

        sizeIndicator = GameObject.Find("lblSizeIndicator").GetComponent<Text>();
        sliderSize = GameObject.Find("sliderSize").GetComponent<Slider>();
        sliderSize.value = 11;

        gameOverCanvas = GameObject.Find("GameOverCanvas");
        gameOverCanvas.SetActive(false);
        gameWinCanvas = GameObject.Find("GameWinCanvas");
        gameWinCanvas.SetActive(false);
        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsCanvas.SetActive(false);

        txtTime = GameObject.Find("txtTime").GetComponent<Text>();
        ResetTimer();

        sun = GameObject.Find("Sun").transform;
        InvokeRepeating("UpdateSun", 0.0f, 1.0f);
    }

    public static Sprite GetSprite(string name)
    {
        return Resources.Load<Sprite>(string.Format("Sprites/{0}", name));
    }

    private void Start()
    {
        GenerateBoard(false);
    }

    private void GenerateBoard(bool clear)
    {
        Camera.main.transform.position = new Vector3((elementSize.x / 2) * w, (elementSize.y / 2) * h, -1.0f);
        Camera.main.orthographicSize = (h * (80.0f / 11.0f)) / 2.0f;

        if(clear)
        {
            foreach (Element elem in elements)
            {
                GameObject.Destroy(elem.gameObject);
            }
        }

        elements = new Element[w, h];

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                GameObject obj = Instantiate(Resources.Load<GameObject>("piece"));
                obj.transform.SetParent(this.transform);
                obj.transform.position = new Vector2(elementSize.x * i, elementSize.y * j);
                Board.elements[i, j] = obj.AddComponent<Element>();
                Board.elements[i, j].x = i;
                Board.elements[i, j].y = j;
            }
        }
    }

    // show mines
    public static void showAllMines()
    {
        foreach (Element elem in elements)
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
        SetTimer(false);
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
        foreach (Element elem in elements)
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

    public void SetPaused(bool gamePaused)
    {
        //TODO: Should options pause timer?
        Board.gamePaused = gamePaused;
        SetTimer(!gamePaused);
    }

    private void UpdateSun()
    {
        float hour = (testTime == -1) ? (DateTime.Now.Hour + (DateTime.Now.Minute / 60.0f) + (DateTime.Now.Second / 3600.0f)) : testTime;
        sunTime = hour / 24.0f;
        //Debug.Log(String.Format("Updating sun with hour {0} with fraction of {1}", hour, sunTime));
        sun.eulerAngles = new Vector3((sunTime * 360) - 90, 0.0f, 0.0f);
    }

    public void sliderSize_OnValueChanged()
    {
        sizeIndicator.text = sliderSize.value.ToString();
    }

    public void btnOptions_Click()
    {        
        optionsCanvas.SetActive(true);
        SetPaused(true);
    }

    public void btnOptionsNewGame_Click()
    {
        optionsCanvas.SetActive(false);
        SetPaused(false);

        w = h = (int)(sliderSize.value);

        GenerateBoard(true);

        RestartGame();
    }

    public void btnOptionsCancel_Click()
    {
        optionsCanvas.SetActive(false);
        SetPaused(false);
    }

    public void btnReplay_Click()
    {
        RestartGame();
    }

    private void RestartGame()
    {
        gameOver = false;
        ResetTimer();

        if (gameOverCanvas.activeInHierarchy)
            gameOverCanvas.SetActive(false);

        if (gameWinCanvas.activeInHierarchy)
            gameWinCanvas.SetActive(false);

        foreach (Element elem in elements)
        {
            elem.SetSprite(baseTexture);
        }

        minesGenerated = false;
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