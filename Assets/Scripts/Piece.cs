using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Piece : MonoBehaviour
{
    public static PieceMove pieceMove;

    public ChessCase chessCase;
    [SerializeField] public List<Vector2> simpleMovements = new List<Vector2>();
    [SerializeField] public List<Vector2> simpleAttacks = new List<Vector2>();
    [SerializeField] public List<Vector2> infiniteMovements = new List<Vector2>();
    [SerializeField] public List<Vector2> infiniteAttacks = new List<Vector2>();
    [SerializeField] public bool canJumpPieces = false;
    [SerializeField] public bool canRisk = true;
    public bool isChecked = false;
    [SerializeField] public string team = "lights";
    public NavMeshAgent agent;

    public List<Transform> possibleMoves = new List<Transform>();
    public List<Transform> possibleAttacks = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pieceMove = GameObject.Find("/GameManager").GetComponent<PieceMove>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Taken()
    {
        gameObject.SetActive(false);
    }

    public void Unselect()
    {
        foreach (Transform move in possibleMoves)
        {
            ChessCase chessCase = move.GetComponent<ChessCase>();
            chessCase.resetMaterial();
        }
        foreach (Transform attacks in possibleAttacks)
        {
            ChessCase chessCase = attacks.GetComponent<ChessCase>();
            chessCase.resetMaterial();
        }
    }
}