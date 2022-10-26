using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        Piece selectedPiece = selectedChessCase.currentPiece.GetComponent<Piece>();
        Piece targetPiece = targetedChessCase.currentPiece.GetComponent<Piece>();

        if (selectedPiece.team == targetPiece.team || !CheckAttackMove())
        {
            targetedCaseTransform = null;
            return;
        }
        AttackPiece();
    }

    bool CheckAttackMove()
    {
        foreach (Transform attackRange in GetAttackCase())
        {
            if (targetedCaseTransform == attackRange && CheckObstacle())
            {
                return true;
            }
        }
        return false;
    }
    List<Transform> GetAttackCase()
    {
        List<Transform> ranges = new List<Transform>();
        Transform currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece;
        Piece selectedPiece = currentPiece.GetComponent<Piece>();
        for (int i = 0; i < selectedPiece.simpleAttacks.Count; i++)
        {
            Vector2 simpleAttack = selectedPiece.simpleAttacks[i];
            Vector3 attackPos = selectedCaseTransform.position;

            if (currentPiece.localEulerAngles.y == 180)
            {
                attackPos.x = attackPos.x - simpleAttack.x;
            } else
            {
                attackPos.x = attackPos.x + simpleAttack.x;
            }

            attackPos.z = attackPos.z + simpleAttack.y;

            Collider[] hitColliders = Physics.OverlapSphere(attackPos, 0.1f, layerMask);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.tag == "Case")
                {
                    ranges.Add(hitCollider.transform);
                }
            }
        }

        return ranges;
    }

    bool CheckObstacle()
    {
        Transform currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece;
        Piece selectedPiece = currentPiece.GetComponent<Piece>();
        if (selectedPiece.canJumpPieces)
        {
            return true;
        }

        Vector3 direction = targetedCaseTransform.position - selectedCaseTransform.position;
        RaycastHit[] hits = Physics.RaycastAll(selectedCaseTransform.position, direction);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            ChessCase chessCase = hit.transform.GetComponent<ChessCase>();
            if (hit.transform != selectedCaseTransform && hit.transform != targetedCaseTransform)
            {
                if (chessCase.currentPiece != null)
                {
                    return false;
                }
            }

        }

        return true;
    }

    void AttackPiece()
    {
        ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
        ChessCase targetedChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        Piece targetPiece = targetedChessCase.currentPiece.GetComponent<Piece>();
        selectedChessCase.currentPiece.position = targetedChessCase.currentPiece.position;
        targetPiece.Taken();
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
