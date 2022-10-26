using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceMove : MonoBehaviour
{
    public Transform selectedCaseTransform;
    public Transform targetedCaseTransform;
    public int layerMask = ~(1 << 7);

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f, layerMask))
            {
                Transform clickedTransform = hit.transform;
                //Our custom method.

                if (clickedTransform.tag == "Case")
                {
                    Debug.Log(clickedTransform.name);
                    ChessCaseSelect(clickedTransform);
                }
                //CurrentClickedGameObject(raycastHit.transform.gameObject);
            }
        }
    }

    void ChessCaseSelect(Transform chessCaseTransform)
    {
        ChessCase chessCase = chessCaseTransform.GetComponent<ChessCase>();
        if (!IsSelected())
        {
            if (chessCase.currentPiece == null)
            {
                return;
            }
            Debug.Log(chessCase.currentPiece.name);

            selectedCaseTransform = chessCaseTransform;
        } else
        {
            if (selectedCaseTransform == chessCaseTransform)
            {
                Unselect();
                return;
            }

            targetedCaseTransform = chessCaseTransform;
            TryMovePiece();
        }
    }

    void TryMovePiece()
    {
        ChessCase targetChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        if (targetChessCase.currentPiece != null)
        {
            TryAttackPiece();
        } else
        {
            //Move piece if possible
        }
    }

    void TryAttackPiece()
    {
        ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
        ChessCase targetedChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        Piece selectedPawn = selectedChessCase.currentPiece.GetComponent<Piece>();
        Piece targetPawn = targetedChessCase.currentPiece.GetComponent<Piece>();

        if (selectedPawn.team == targetPawn.team)
        {
            targetedCaseTransform = null;
            return;
        }
        selectedChessCase.currentPiece.position = targetedChessCase.currentPiece.position;
        targetPawn.Taken();
        selectedChessCase.CheckPawnTransform();
        targetedChessCase.CheckPawnTransform();
        Unselect();
    }

    private bool IsSelected()
    {
        return selectedCaseTransform != null;
    }

    private void Unselect()
    {
        selectedCaseTransform = null;
        targetedCaseTransform = null;
    }

}
