using System;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public static Board board;

    // Board Parameters
    private static readonly DifficultyPreset DIFFICULTY_BEGINNER = new DifficultyPreset(11, 0.10f); // Default Difficulty.
    private static readonly DifficultyPreset DIFFICULTY_INTERMEDIATE = new DifficultyPreset(22, 0.20f);
    private static readonly DifficultyPreset DIFFICULTY_EXPERT = new DifficultyPreset(28, 0.25f);

    public static int boardWidth = DIFFICULTY_BEGINNER.boardSize;
    public static int boardHeight = DIFFICULTY_BEGINNER.boardSize;
    public static float mineFreq = DIFFICULTY_BEGINNER.mineFreq;

    public static Element[,] elements = new Element[boardWidth, boardHeight];
    public static Vector3 elementSize;

    // Game State
    public static bool gamePaused;
    public static bool gameOver = false;
    public static bool minesGenerated = false;

    // Content
    public static Sprite baseTexture;
    public static Sprite[] infoTextures;
    public static Sprite mineTexture;

    // UI Objects
    private GameObject gameOverCanvas;
    private GameObject gameWinCanvas;
    private GameObject optionsCanvas;
    private Text txtTime;
    private Slider sliderSize;
    private Text lblSizeIndicator;
    private Slider sliderMine;
    private Text lblMineIndicator;

    // Timer
    private long gameTime;
    private bool timeTicking = false;

    // Sun Real World Synchronization
    [SerializeField]
    private float testTime = -1;
    private Transform sun;
    private float sunTime;

    private void Awake()
    {
        board = this;
        GameObject tempBase = GameObject.Find("tempBase"); // Used to get the size of the sprite in game.
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

        lblSizeIndicator = GameObject.Find("lblSizeIndicator").GetComponent<Text>();
        sliderSize = GameObject.Find("sliderSize").GetComponent<Slider>();
        sliderSize.value = boardHeight;

        lblMineIndicator = GameObject.Find("lblMineIndicator").GetComponent<Text>();
        sliderMine = GameObject.Find("sliderMine").GetComponent<Slider>();
        sliderMine.value = mineFreq * 100;

        gameOverCanvas = GameObject.Find("GameOverCanvas");
        gameOverCanvas.SetActive(false);
        gameWinCanvas = GameObject.Find("GameWinCanvas");
        gameWinCanvas.SetActive(false);
        optionsCanvas = GameObject.Find("OptionsCanvas");
        optionsCanvas.SetActive(false);

        txtTime = GameObject.Find("txtTime").GetComponent<Text>();
        ResetTimer();

        sun = GameObject.Find("Sun").transform;
        InvokeRepeating("UpdateSun", 0.0f, 1.0f); // Start sun sync.
    }

    private void Start()
    {
        GenerateBoard(false);
    }

    private void GenerateBoard(bool clear)
    {
        // Set camera position and size to correctly fit the board in the frame. This is why only square boards are allowed.
        Camera.main.transform.position = new Vector3((elementSize.x / 2) * boardWidth, (elementSize.y / 2) * boardHeight, -1.0f);
        Camera.main.orthographicSize = (boardHeight * (80.0f / 11.0f)) / 2.0f;

        if (clear)
        {
            foreach (Element elem in elements)
            {
                GameObject.Destroy(elem.gameObject);
            }
        }

        elements = new Element[boardWidth, boardHeight];

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
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

    public static Sprite GetSprite(string name)
    {
        return Resources.Load<Sprite>(string.Format("Sprites/{0}", name));
    }

    public static void ShowAllMines()
    {
        foreach (Element elem in elements)
        {
            if (elem.mine)
            {
                elem.RenderTexture(-1);
            }
        }
    }

    public static bool MineAt(int i, int j)
    {
        // Check whether a base element has a mine.
        return (i >= 0 && j >= 0 && i < boardWidth && j < boardHeight) ? elements[i, j].mine : false;
    }

    public static int AdjacentMines(int i, int j)
    {
        int count = 0;

        if (MineAt(i - 1, j)) ++count; // left
        if (MineAt(i + 1, j)) ++count; // right

        if (MineAt(i, j + 1)) ++count; // top
        if (MineAt(i, j - 1)) ++count; // bottom

        if (MineAt(i + 1, j + 1)) ++count; // top-right
        if (MineAt(i + 1, j - 1)) ++count; // bottom-right

        if (MineAt(i - 1, j - 1)) ++count; // bottom-left
        if (MineAt(i - 1, j + 1)) ++count; // top-left

        return count;
    }

    public static void UncoverFields(int x, int y, bool[,] visited)
    {
        int minesAround;
        // Coordinates in Range?
        if (x >= 0 && x < boardWidth && y >= 0 && y < boardHeight)
        {
            if (visited[x, y])
                return;
            else
                visited[x, y] = true;

            minesAround = AdjacentMines(x, y);

            // Show the number.
            elements[x, y].RenderTexture(minesAround);

            // Game logic.
            if (minesAround > 0) return;

            // Repeat the process recursively.
            UncoverFields(x - 1, y, visited);
            UncoverFields(x + 1, y, visited);
            UncoverFields(x, y - 1, visited);
            UncoverFields(x, y + 1, visited);
        }
    }

    public static bool IsFinished()
    {
        // Try to find a covered element that is no mine.
        foreach (Element elem in elements)
        {
            if (elem.IsCovered() && !elem.mine)
            {
                return false;
            }
        }

        // There are none => all are mines => game won.
        return true;
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

        if (!timeTicking)
        {
            CancelInvoke("UpdateTimer");
        }
    }

    private void UpdateTimer()
    {
        gameTime++;
        txtTime.text = string.Format("Time: {0}", gameTime);
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
        Board.ShowAllMines();
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
        float hour = (testTime == -1) ? (DateTime.Now.Hour + (DateTime.Now.Minute / 60.0f) + (DateTime.Now.Second / 3600.0f)) : testTime; // Get the time of the day as a decimal.
        sunTime = hour / 24.0f; // The fraction of the current decimal time of the day.
        sun.eulerAngles = new Vector3((sunTime * 360) - 90, 0.0f, 0.0f); // Set sun angle to realistically match decimal time.
    }

    private void sliderSize_OnValueChanged()
    {
        lblSizeIndicator.text = sliderSize.value.ToString();
    }

    private void sliderMine_OnValueChanged()
    {
        lblMineIndicator.text = String.Format("{0}%", sliderMine.value);
    }

    private void btnDifficultyPreset_Click(int diff)
    {
        DifficultyPreset[] presets = new DifficultyPreset[] { DIFFICULTY_BEGINNER, DIFFICULTY_INTERMEDIATE, DIFFICULTY_EXPERT };
        sliderSize.value = presets[diff].boardSize;
        sliderMine.value = presets[diff].mineFreq * 100.0f;
    }

    private void btnOptions_Click()
    {
        optionsCanvas.SetActive(true);
        SetPaused(true);
    }

    private void btnOptionsNewGame_Click()
    {
        optionsCanvas.SetActive(false);
        SetPaused(false);

        boardWidth = boardHeight = (int)(sliderSize.value);
        mineFreq = sliderMine.value / 100.0f;

        GenerateBoard(true);

        RestartGame();
    }

    private void btnOptionsCancel_Click()
    {
        optionsCanvas.SetActive(false);
        SetPaused(false);
    }

    private void btnReplay_Click()
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

    private void btnQuit_Click()
    {
        if (Application.isEditor)
        {
            Debug.Log("Can not close game in an editor run!");
            return;
        }

        Application.Quit();
    }

    private struct DifficultyPreset
    {
        public int boardSize;
        public float mineFreq;

        public DifficultyPreset(int boardSize, float mineFreq)
        {
            this.boardSize = boardSize;
            this.mineFreq = mineFreq;
        }
    }
}