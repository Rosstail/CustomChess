using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessCase : MonoBehaviour
{
    public static PieceMove pieceMove;
    public Transform currentPieceTransform;
    public Piece currentPiece;
    private Renderer renderer;
    private Material defaultMaterial;

    // Start is called before the first frame update
    void Start()
    {
        pieceMove = GameObject.Find("/GameManager").GetComponent<PieceMove>();
        renderer = GetComponent<Renderer>();
        defaultMaterial = renderer.material;
        CheckPieceTransform();
    }

    // Update is called once per frame
    void Update()
    {
        CheckPieceTransform();
    }

    public void CheckPieceTransform()
    {
        Transform newPiece = null;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.up, 100f);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            Transform raycastTransform = hit.transform;

            Debug.DrawLine(transform.position, raycastTransform.position, Color.magenta);
            if (raycastTransform.gameObject.activeSelf && raycastTransform.tag == "Piece")
            {
                newPiece = raycastTransform;
                break;
            }
        }

        currentPieceTransform = newPiece;
        if (currentPieceTransform != null)
        {
            currentPiece = currentPieceTransform.GetComponent<Piece>();
            currentPiece.chessCase = this;
            currentPiece.possibleMoves = new List<Transform>();
            currentPiece.possibleAttacks = new List<Transform>();
            pieceMove.CheckPossibleMoves(currentPiece);
            pieceMove.CheckPossibleAttacks(currentPiece);

            currentPiece.isChecked = (!currentPiece.canRisk && pieceMove.CheckRisk(currentPiece, transform));
        }
    }

    public void setMaterial(Material material)
    {
        renderer.material = material;
    }

    public void resetMaterial()
    {
        renderer.material = defaultMaterial;
    }
}
