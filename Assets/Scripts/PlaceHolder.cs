using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceHolder : MonoBehaviour
{

    [SerializeField] public Game game;
    [SerializeField] public TextMeshProUGUI textField;
    string defaultText;
    // Start is called before the first frame update
    void Awake()
    {
        defaultText = textField.text;
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Display()
    {
        string movementText = defaultText;

        movementText = 
            movementText.Replace("%turnCount%",
            game.turn.ToString());
        movementText = movementText.Replace("%turnPlayer%", game.whoPlays.ToUpper());
        movementText = movementText.Replace("%lightPawnsLeft%", game.lightPawnsCount.ToString());
        movementText = movementText.Replace("%darkPawnsLeft%", game.darkPawnsCount.ToString());

        if (game.winner != null && game.winner != "")
        {
            movementText = movementText.Replace("%lightStatus%", game.winner == "lights" ? "WINNER" : "LOSER");
            movementText = movementText.Replace("%darkStatus%", game.winner == "darks" ? "WINNER" : "LOSER");
        } else
        {
            movementText = movementText.Replace("%lightStatus%", "playing");
            movementText = movementText.Replace("%darkStatus%", "playing");
        }

        textField.SetText(movementText);
    } 
}
