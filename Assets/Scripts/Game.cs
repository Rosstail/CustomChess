using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{

    public int turn = 0;
    public string whoPlays = "light";
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
}
