using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Text[] buttons;
    private char[] boardStatus;
    [SerializeField]
    private Text[] txtPlayerScores;
    [SerializeField]
    private Image[] pnlPlayerBackgrounds;

    private bool gameWon = false;
    private int nextFirstPlayer;
    private int currTurn;
    private int[] playerScores = new int[2] { 0, 0 };
    private readonly char[] PLAYER_LETTERS = new char[] { 'X', 'O' };
    private readonly Color WIN_COLOR = new Color32(255, 128, 0, 255);

    enum WinConditionType { ROW, COLUMN, DIAGONAL };

    void Start()
    {
        nextFirstPlayer = Random.Range(0, 2);
        ResetBoard();

        SetScore(0, 0);
        SetScore(1, 0);
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
        gameWon = false;

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

        ResetButtonColors();
        SetCurrentTurn(nextFirstPlayer);
        nextFirstPlayer = GetOtherPlayer(nextFirstPlayer);
    }

    private void SetScore(int player, int score)
    {
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
        // Check Rows
        for (int r = 0; r < 3; r++)
        {
            int cond = CheckWinCondition(WinConditionType.ROW, r);
            if (cond != -1)
            {
                Debug.Log(string.Format("Player {0} has won via row {1}!", PLAYER_LETTERS[cond], r));
                gameWon = true;
                SetScore(cond, playerScores[cond] + 1);
                return;
            }
        }

        // Check Columns
        for (int c = 0; c < 3; c++)
        {
            int cond = CheckWinCondition(WinConditionType.COLUMN, c);
            if (cond != -1)
            {
                Debug.Log(string.Format("Player {0} has won via column {1}!", PLAYER_LETTERS[cond], c));
                SetScore(cond, playerScores[cond] + 1);
                gameWon = true;
                return;
            }
        }

        // Check Diagonals
        for (int d = 0; d < 2; d++)
        {
            int cond = CheckWinCondition(WinConditionType.DIAGONAL, d);
            if (cond != -1)
            {
                Debug.Log(string.Format("Player {0} has won via diagonal {1}!", PLAYER_LETTERS[cond], d));
                gameWon = true;
                SetScore(cond, playerScores[cond] + 1);
                return;
            }
        }

        if (!gameWon)
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
                Debug.Log("DRAW");
            }
        }
    }

    public void ResetButtonColors()
    {
        foreach (Text curr in buttons)
        {
            curr.color = Color.green;
        }
    }

    private static int GetOtherPlayer(int player)
    {
        return (player == 0) ? 1 : 0;
    }

    private void GameButton_Click(int button)
    {
        if (boardStatus[button] == 'N' && !gameWon)
        {
            boardStatus[button] = PLAYER_LETTERS[currTurn];
            buttons[button].text = boardStatus[button].ToString();
            CheckAllWinConditions();
           
            if(!gameWon)
                SetCurrentTurn(GetOtherPlayer(currTurn));
        }
    }
}
