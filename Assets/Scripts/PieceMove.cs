using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class PieceMove : MonoBehaviour
{
    public Transform selectedCaseTransform;
    public Transform targetedCaseTransform;
    public int layerMask = ~(1 << 7);
    private Game game;
    
    List<Transform> possibleMoves = new List<Transform>();
    List<Transform> possibleAttacks = new List<Transform>();

    [SerializeField] public Material selectedPieceMaterial;
    [SerializeField] public Material possibleMoveMaterial;
    [SerializeField] public Material possibleAttackMaterial;

    // Start is called before the first frame update
    void Start()
    {
        game = GetComponent<Game>();
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

            chessCase.setMaterial(selectedPieceMaterial);
            CheckPossibleMoves();
            CheckPossibleAttacks();
        } else
        {
            Piece currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece.GetComponent<Piece>();
            if (selectedCaseTransform == chessCaseTransform || currentPiece.team != game.whoPlays)
            {
                UnselectAll();
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
        if (!possibleMoves.Contains(targetedCaseTransform))
        {
            UnselectTarget();
            return;
        }
        MovePiece();
        UnselectAll();
    }

    void CheckPossibleMoves()
    {
        foreach (Transform moveRange in GetMoveCase())
        {
            if (CheckObstacle(moveRange))
            {
                ChessCase possibleChessCase = moveRange.GetComponent<ChessCase>();
                if (possibleChessCase.currentPiece == null)
                {
                    possibleMoves.Add(moveRange);
                    possibleChessCase.setMaterial(possibleMoveMaterial);
                }
            }
        }

        foreach (Transform moveRange in GetMoveInfinite())
        {
            if (CheckObstacle(moveRange))
            {
                ChessCase possibleChessCase = moveRange.GetComponent<ChessCase>();
                if (possibleChessCase.currentPiece == null)
                {
                    possibleMoves.Add(moveRange);
                    possibleChessCase.setMaterial(possibleMoveMaterial);
                }
            }
        }
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

        if (selectedPiece.team == targetPiece.team || !possibleAttacks.Contains(targetedCaseTransform))
        {
            UnselectTarget();
            return;
        }
        AttackPiece();
        selectedChessCase.CheckPieceTransform();
        targetedChessCase.CheckPieceTransform();
        UnselectAll();
    }

    void CheckPossibleAttacks()
    {
        foreach (Transform attackRange in GetAttackCase())
        {
            ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
            ChessCase possibleChessCase = attackRange.GetComponent<ChessCase>();
            Piece selectedPiece = selectedChessCase.currentPiece.GetComponent<Piece>();

            if (possibleChessCase.currentPiece != null)
            {
                Piece possiblePiece = possibleChessCase.currentPiece.GetComponent<Piece>();
                if (CheckObstacle(attackRange) && possiblePiece.team != selectedPiece.team)
                {
                    possibleAttacks.Add(attackRange);
                    possibleChessCase.setMaterial(possibleAttackMaterial);
                }
            }
        }

        foreach (Transform attackRange in GetAttackInfinite())
        {
            ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
            ChessCase possibleChessCase = attackRange.GetComponent<ChessCase>();
            Piece selectedPiece = selectedChessCase.currentPiece.GetComponent<Piece>();

            if (possibleChessCase.currentPiece != null)
            {
                Piece possiblePiece = possibleChessCase.currentPiece.GetComponent<Piece>();
                if (CheckObstacle(attackRange) && possiblePiece.team != selectedPiece.team)
                {
                    possibleAttacks.Add(attackRange);
                    possibleChessCase.setMaterial(possibleAttackMaterial);
                }
            }
        }
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

    List<Transform> GetAttackInfinite()
    {
        List<Transform> ranges = new List<Transform>();
        Transform currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece;
        Piece selectedPiece = currentPiece.GetComponent<Piece>();
        for (int i = 0; i < selectedPiece.infiniteAttacks.Count; i++)
        {
            Vector2 infiniteAttackValue = selectedPiece.infiniteMovements[i];
            int multiplier = 1;

            if (currentPiece.localEulerAngles.y == 180)
            {
                infiniteAttackValue.x = -infiniteAttackValue.x;
            }

            while (true)
            {
                Vector3 movementPos = selectedCaseTransform.position;
                int caseCounter = 0;
                {
                    movementPos.x = movementPos.x + infiniteAttackValue.x * multiplier;
                    movementPos.z = movementPos.z + infiniteAttackValue.y * multiplier;

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

    bool CheckObstacle(Transform targetTransform)
    {
        Transform currentPiece = selectedCaseTransform.GetComponent<ChessCase>().currentPiece;
        Piece selectedPiece = currentPiece.GetComponent<Piece>();
        if (selectedPiece.canJumpPieces)
        {
            return true;
        }

        Vector3 direction = targetTransform.position - selectedCaseTransform.position;
        RaycastHit[] hits = Physics.RaycastAll(selectedCaseTransform.position, direction);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            ChessCase chessCase = hit.transform.GetComponent<ChessCase>();
            if (hit.transform != selectedCaseTransform)
            {
                if (hit.transform == targetTransform)
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
        game.SwitchTeam();
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

    private void UnselectAll()
    {
        UnselectFirst();
        UnselectTarget();
    }

    void UnselectFirst()
    {
        if (selectedCaseTransform != null)
        {
            ChessCase selectedChessCase = selectedCaseTransform.GetComponent<ChessCase>();
            selectedChessCase.resetMaterial();
            selectedCaseTransform = null;
        }

        foreach (Transform move in possibleMoves)
        {
            ChessCase chessCase = move.GetComponent<ChessCase>();
            chessCase.resetMaterial();
        }
        possibleMoves = new List<Transform>();
    }

    void UnselectTarget()
    {
        if (targetedCaseTransform != null)
        {
            ChessCase targetChessCase = targetedCaseTransform.GetComponent<ChessCase>();
            targetChessCase.resetMaterial();
            targetedCaseTransform = null;
        }

        foreach (Transform attacks in possibleAttacks)
        {
            ChessCase chessCase = attacks.GetComponent<ChessCase>();
            chessCase.resetMaterial();
        }
        possibleAttacks = new List<Transform>();
    }

}
