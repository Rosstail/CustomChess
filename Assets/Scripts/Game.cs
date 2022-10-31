using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int turn = 0;
    public string whoPlays = "lights";
    [SerializeField] public GameObject lightPawns;
    [SerializeField] public GameObject darkPawns;

    public int lightPawnsCount;
    public int darkPawnsCount;

    // Start is called before the first frame update
    void Start()
    {
        lightPawnsCount = lightPawns.transform.childCount;
        darkPawnsCount = darkPawns.transform.childCount;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CheckGameStatus()
    {
        if (lightPawnsCount == 0)
        {
            Victory("darks");
        } else if (darkPawnsCount == 0)
        {
            Victory("lights");
        }
    }
    public void SwitchTeam()
    {
        if (whoPlays == "lights")
        {
            whoPlays = "darks";
        } else
        {
            whoPlays = "lights";
        }
    }

    public void Victory(string teamName)
    {
        Debug.Log(teamName + "  is victorious !");
        whoPlays = null;
    }
}
