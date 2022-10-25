using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceHoldersManager : MonoBehaviour
{

    [SerializeField] public Game game;
    [SerializeField] public TextMeshProUGUI textField;
    string defaultText;
    // Start is called before the first frame update
    void Start()
    {
        defaultText = textField.text;
    }

    // Update is called once per frame
    void Update()
    {
        Display();
    }

    void Display()
    {
        string movementText = defaultText;

        movementText = movementText.Replace("%turnCount%", game.turn.ToString());
        movementText = movementText.Replace("%turnPlayer%", game.whoPlays.ToUpper());
        movementText = movementText.Replace("%lightPawnsLeft%", game.lightPawnsCount.ToString());
        movementText = movementText.Replace("%darkPawnsLeft%", game.darkPawnsCount.ToString());

        textField.SetText(movementText);
    } 
}
