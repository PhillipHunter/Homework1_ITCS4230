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

    private int currTurn;
    private int[] playerScores = new int[2];
    private readonly char[] PLAYER_LETTERS = new char[] { 'X', 'O' };

    enum WinConditionType { ROW, COLUMN, DIAGONAL };

    void Awake()
    {
        boardStatus = new char[9]
        {
            'N','N','N',
            'N','N','N',
            'N','N','N'
        };
    }

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].text = string.Empty;
        }

        SetScore(0, 0);
        SetScore(1, 0);
        SetCurrentTurn(Random.Range(0, 2));
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
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

        switch (type)
        {
            case WinConditionType.ROW:
                for (int player = 0; player < 2; player++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        canWin = (boardStatus[c + (param * 3)] == PLAYER_LETTERS[player]); // param = curr row

                        if (!canWin)
                            break;
                    }

                    if (canWin)
                        return player;
                }
                break;

            case WinConditionType.COLUMN:
                for (int player = 0; player < 2; player++)
                {
                    for (int r = 0; r < 3; r++)
                    {
                        int curr = r * 3 + param;

                        canWin = (boardStatus[curr] == PLAYER_LETTERS[player]); // param = curr col

                        if (!canWin)
                            break;
                    }

                    if (canWin)
                        return player;
                }
                break;

            case WinConditionType.DIAGONAL:
                for (int player = 0; player < 2; player++)
                {
                    if (param == 0)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int curr = i * 3 + i;

                            canWin = (boardStatus[curr] == PLAYER_LETTERS[player]); // param = diagonal

                            if (!canWin)
                                break;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            int curr = i * 3 + ((3 - 1) - i);

                            canWin = (boardStatus[curr] == PLAYER_LETTERS[player]); // param = diagonal

                            if (!canWin)
                                break;
                        }
                    }

                    if (canWin)
                        return player;
                }
                break;
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
            }
        }

        // Check Columns
        for (int c = 0; c < 3; c++)
        {
            int cond = CheckWinCondition(WinConditionType.COLUMN, c);
            if (cond != -1)
            {
                Debug.Log(string.Format("Player {0} has won via column {1}!", PLAYER_LETTERS[cond], c));
            }
        }

        // Check Diagonals
        for (int d = 0; d < 2; d++)
        {
            int cond = CheckWinCondition(WinConditionType.DIAGONAL, d);
            if (cond != -1)
            {
                Debug.Log(string.Format("Player {0} has won via diagonal {1}!", PLAYER_LETTERS[cond], d));
            }
        }
    }

    private static int GetOtherPlayer(int player)
    {
        return (player == 0) ? 1 : 0;
    }

    private void GameButton_Click(int button)
    {
        if (boardStatus[button] == 'N')
        {
            boardStatus[button] = PLAYER_LETTERS[currTurn];
            buttons[button].text = boardStatus[button].ToString();
            CheckAllWinConditions();
            SetCurrentTurn(GetOtherPlayer(currTurn));
        }
    }
}
