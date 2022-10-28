using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

                if (clickedTransform.tag == "Case")
                {
                    ChessCaseSelect(clickedTransform);
                }
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

            selectedCaseTransform = chessCaseTransform;
        } else
        {
            if (selectedCaseTransform == chessCaseTransform)
            {
                Unselect();
                return;
            }

            targetedCaseTransform = chessCaseTransform;
            TryActionPiece();
        }
    }

    void TryActionPiece()
    {
        ChessCase targetChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        if (targetChessCase.currentPiece != null)
        {
            TryAttackPiece();
        } else
        {
            TryMovePiece();
        }
    }

    void TryMovePiece()
    {
        if (!CheckMove())
        {
            targetedCaseTransform = null;
            return;
        }
        MovePiece();
        ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
        ChessCase targetedChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        selectedChessCase.CheckPieceTransform();
        targetedChessCase.CheckPieceTransform();
        Unselect();
    }

    bool CheckMove()
    {
        foreach (Transform moveRange in GetMoveCase())
        {
            if (targetedCaseTransform == moveRange && CheckObstacle())
            {
                return true;
            }
        }

        foreach (Transform moveRange in GetMoveInfinite())
        {
            if (targetedCaseTransform == moveRange && CheckObstacle())
            {
                return true;
            }
        }
        return false;
    }
    List<Transform> GetMoveCase()
    {
        List<Transform> ranges = new List<Transform>();
        Transform currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece;
        Piece selectedPiece = currentPiece.GetComponent<Piece>();
        for (int i = 0; i < selectedPiece.simpleMovements.Count; i++)
        {
            Vector2 simpleMovement = selectedPiece.simpleMovements[i];
            Vector3 movePos = selectedCaseTransform.position;

            if (currentPiece.localEulerAngles.y == 180)
            {
                simpleMovement.x = -simpleMovement.x;
            }

            movePos.x = movePos.x + simpleMovement.x;
            movePos.z = movePos.z + simpleMovement.y;

            Collider[] hitColliders = Physics.OverlapSphere(movePos, 0.1f, layerMask);
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

    List<Transform> GetMoveInfinite()
    {
        List<Transform> ranges = new List<Transform>();
        Transform currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece;
        Piece selectedPiece = currentPiece.GetComponent<Piece>();
        for (int i = 0; i < selectedPiece.infiniteMovements.Count; i++)
        {
            Vector2 infiniteMovementValue = selectedPiece.infiniteMovements[i];
            int multiplier = 1;

            if (currentPiece.localEulerAngles.y == 180)
            {
                infiniteMovementValue.x = -infiniteMovementValue.x;
            }

            while (true)
            {
                Vector3 movementPos = selectedCaseTransform.position;
                int caseCounter = 0;
                {
                    movementPos.x = movementPos.x + infiniteMovementValue.x * multiplier;
                    movementPos.z = movementPos.z + infiniteMovementValue.y * multiplier;

                    Collider[] hitColliders = Physics.OverlapSphere(movementPos, 0.1f, layerMask);
                    foreach (var hitCollider in hitColliders)
                    {
                        if (hitCollider.tag == "Case")
                        {
                            ranges.Add(hitCollider.transform);
                            caseCounter++;
                        }
                    }
                }
                multiplier++;
                if (caseCounter == 0)
                {
                    break;
                }
            }
        }

        return ranges;

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
        selectedChessCase.CheckPieceTransform();
        targetedChessCase.CheckPieceTransform();
        Unselect();
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

        foreach (Transform attackRange in GetMoveInfinite())
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
                simpleAttack.x = -simpleAttack.x;
            }

            attackPos.x = attackPos.x + simpleAttack.x;
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
            if (hit.transform != selectedCaseTransform)
            {
                if (hit.transform == targetedCaseTransform)
                {
                    return true;
                }
                if (chessCase.currentPiece != null)
                {
                    return false;
                }
            }

        }

        return true;
    }

    void MovePiece()
    {
        ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
        ChessCase targetedChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        Transform selectedPieceTransform = selectedChessCase.currentPiece;
        Piece selectedPiece = selectedPieceTransform.GetComponent<Piece>();

        Vector3 startPosition = selectedChessCase.currentPiece.position;
        Vector3 destination = new Vector3(targetedCaseTransform.position.x, startPosition.y, targetedCaseTransform.position.z);
        selectedPiece.agent.SetDestination(destination);
    }

    void AttackPiece()
    {
        MovePiece();
        ChessCase targetedChessCase = targetedCaseTransform.GetComponent<ChessCase>();
        Piece targetPiece = targetedChessCase.currentPiece.GetComponent<Piece>();
        targetPiece.Taken();
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
