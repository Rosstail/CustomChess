using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int turn = 1;
    public string whoPlays = "lights";
    public string winner = null;
    [SerializeField] public GameObject lightPawns;
    [SerializeField] public GameObject darkPawns;
    [SerializeField] public PlaceHoldersManager placeHoldersManager;

    public int lightPawnsCount;
    public int darkPawnsCount;

    // Start is called before the first frame update
    void Start()
    {
        lightPawnsCount = lightPawns.transform.childCount;
        darkPawnsCount = darkPawns.transform.childCount;

        CheckGameStatus();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void CheckGameStatus()
    {
        int lights = 0;
        int darks = 0;
        for (int i = 0; i < lightPawns.transform.childCount; i++)
        {
            Transform piece = lightPawns.transform.GetChild(i);
            if (piece.gameObject.activeSelf)
            {
                lights++;
            }
        }

        for (int i = 0; i < darkPawns.transform.childCount; i++)
        {
            Transform piece = darkPawns.transform.GetChild(i);
            if (piece.gameObject.activeSelf)
            {
                darks++;
            }
        }

        lightPawnsCount = lights;
        darkPawnsCount = darks;

        if (lightPawnsCount == 0)
        {
            Victory("darks");
        } else if (darkPawnsCount == 0)
        {
            Victory("lights");
        }
        placeHoldersManager.Check();
    }
    public void SwitchTeam()
    {
        if (whoPlays == "lights")
        {
            whoPlays = "darks";
        } else
        {
            whoPlays = "lights";
            turn++;
        }
    }

    public void Victory(string teamName)
    {
        winner = teamName;
        whoPlays = "End";
    }
}
