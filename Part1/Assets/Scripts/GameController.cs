using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Text[] buttons;
    [SerializeField]
    private Text[] txtPlayerTitles;
    [SerializeField]
    private Text[] txtPlayerScores;
    [SerializeField]
    private Image[] pnlPlayerBackgrounds;
    [SerializeField]
    private GameObject[] pnlPlayerWinBackgrounds;
    [SerializeField]
    private GameObject txtDraw;

    private char[] boardStatus;

    private bool gameFinished = false;
    private int nextFirstPlayer;
    private int currTurn;
    private int[] playerScores = new int[2] { 0, 0 };
    private readonly char[] PLAYER_LETTERS = new char[] { 'X', 'O' };
    private readonly Color WIN_COLOR = new Color32(255, 255, 0, 255);

    enum WinConditionType { ROW, COLUMN, DIAGONAL };

    void Start()
    {
        nextFirstPlayer = Random.Range(0, 2);

        for (int i = 0; i < txtPlayerTitles.Length; i++)
        {
            txtPlayerTitles[i].text = string.Format("Player {0}", PLAYER_LETTERS[i]);
        }

        SetScore(0, 0);
        SetScore(1, 0);

        ResetBoard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
    }

    public void ResetBoard()
    {
        gameFinished = false;

        boardStatus = new char[9]
        {
            'N','N','N',
            'N','N','N',
            'N','N','N'
        };

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].text = string.Empty;
        }

        foreach (GameObject curr in pnlPlayerWinBackgrounds)
            curr.SetActive(false);

        txtDraw.SetActive(false);

        ResetButtonColors();
        SetCurrentTurn(nextFirstPlayer);
        nextFirstPlayer = GetOtherPlayer(nextFirstPlayer);
    }

    private void SetScore(int player, int score)
    {
        pnlPlayerWinBackgrounds[player].SetActive(true);
        playerScores[player] = score;
        txtPlayerScores[player].text = string.Format("Score: {0}", score);
    }

    private void SetCurrentTurn(int player)
    {
        currTurn = player;
        pnlPlayerBackgrounds[player].color = new Color32(64, 64, 0, 255);
        pnlPlayerBackgrounds[GetOtherPlayer(player)].color = new Color32(21, 21, 21, 255);
    }

    private int CheckWinCondition(WinConditionType type, int param)
    {
        bool canWin = false;

        int testPos = 0;

        for (int player = 0; player < 2; player++)
        {
            switch (type)
            {
                case WinConditionType.ROW:
                    for (int c = 0; c < 3; c++)
                    {
                        testPos = c + (param * 3);
                        canWin = (boardStatus[testPos] == PLAYER_LETTERS[player]); // param = curr row
                        buttons[testPos].color = WIN_COLOR;

                        if (!canWin)
                        {
                            ResetButtonColors();
                            break;
                        }
                    }

                    if (canWin)
                        return player;
                    break;

                case WinConditionType.COLUMN:
                    for (int r = 0; r < 3; r++)
                    {
                        testPos = r * 3 + param;

                        canWin = (boardStatus[testPos] == PLAYER_LETTERS[player]); // param = curr col
                        buttons[testPos].color = WIN_COLOR;

                        if (!canWin)
                        {
                            ResetButtonColors();
                            break;
                        }
                    }

                    if (canWin)
                        return player;
                    break;

                case WinConditionType.DIAGONAL:
                    if (param == 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            testPos = i * 3 + i;

                            canWin = (boardStatus[testPos] == PLAYER_LETTERS[player]); // param = diagonal
                            buttons[testPos].color = WIN_COLOR;

                            if (!canWin)
                            {
                                ResetButtonColors();
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            testPos = i * 3 + ((3 - 1) - i);

                            canWin = (boardStatus[testPos] == PLAYER_LETTERS[player]); // param = diagonal
                            buttons[testPos].color = WIN_COLOR;

                            if (!canWin)
                            {
                                ResetButtonColors();
                                break;
                            }
                        }
                    }

                    if (canWin)
                        return player;
                    break;
            }
        }
        return -1;
    }

    private void CheckAllWinConditions()
    {
        int winner = -1;

        // Check Rows
        for (int r = 0; r < 3; r++)
        {
            winner = CheckWinCondition(WinConditionType.ROW, r);
            if (winner != -1)
            {
                gameFinished = true;
                SetScore(winner, playerScores[winner] + 1);
                return;
            }
        }

        // Check Columns
        for (int c = 0; c < 3; c++)
        {
            winner = CheckWinCondition(WinConditionType.COLUMN, c);
            if (winner != -1)
            {
                gameFinished = true;
                SetScore(winner, playerScores[winner] + 1);
                return;
            }
        }

        // Check Diagonals
        for (int d = 0; d < 2; d++)
        {
            winner = CheckWinCondition(WinConditionType.DIAGONAL, d);
            if (winner != -1)
            {
                gameFinished = true;
                SetScore(winner, playerScores[winner] + 1);
                return;
            }
        }

        // Check For Draw
        if (!gameFinished)
        {
            bool draw = true;
            foreach (char curr in boardStatus)
            {
                if (curr == 'N')
                {
                    draw = false;
                }
            }

            if (draw)
            {
                gameFinished = true;
                txtDraw.SetActive(true);
            }
        }
    }

    public void ResetButtonColors()
    {
        foreach (Text curr in buttons)
        {
            curr.color = Color.green;
            if (curr.text.Contains(PLAYER_LETTERS[0].ToString()))
            {
                curr.color = Color.red;
            }

            if (curr.text.Contains(PLAYER_LETTERS[1].ToString()))
            {
                curr.color = new Color32(0, 11, 255, 255);
            }
        }
    }

    private static int GetOtherPlayer(int player)
    {
        return (player == 0) ? 1 : 0;
    }

    private void GameButton_Click(int button)
    {
        if (boardStatus[button] == 'N' && !gameFinished)
        {
            boardStatus[button] = PLAYER_LETTERS[currTurn];
            buttons[button].text = boardStatus[button].ToString();
            CheckAllWinConditions();
            SetCurrentTurn(GetOtherPlayer(currTurn));
        }
    }
}
