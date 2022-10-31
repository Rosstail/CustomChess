using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PieceMove : MonoBehaviour
{
    public Transform selectedCaseTransform;
    public Transform targetedCaseTransform;

    public ChessCase selectedChessCase;
    public ChessCase targetedChessCase;

    public Piece selectedPiece;
    public Piece targetedPiece;

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
            selectedChessCase = chessCase;
            selectedPiece = chessCase.currentPiece.GetComponent<Piece>();

            chessCase.setMaterial(selectedPieceMaterial);
            CheckPossibleMoves();
            CheckPossibleAttacks();
        } else
        {
            if (selectedCaseTransform == chessCaseTransform || selectedPiece.team != game.whoPlays)
            {
                UnselectAll();
                return;
            }

            targetedCaseTransform = chessCaseTransform;
            targetedChessCase = chessCase;
            if (targetedChessCase.currentPiece != null)
            {
                targetedPiece = targetedChessCase.currentPiece.GetComponent<Piece>();
            }

            TryActionPiece();
        }
    }

    void TryActionPiece()
    {
        if (targetedChessCase.currentPiece != null)
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
        for (int i = 0; i < selectedPiece.simpleMovements.Count; i++)
        {
            Vector2 simpleMovement = selectedPiece.simpleMovements[i];
            Vector3 movePos = selectedCaseTransform.position;

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
        for (int i = 0; i < selectedPiece.infiniteMovements.Count; i++)
        {
            Vector2 infiniteMovementValue = selectedPiece.infiniteMovements[i];
            int multiplier = 1;

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
        if (selectedPiece.team == targetedPiece.team || !possibleAttacks.Contains(targetedCaseTransform))
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
            ChessCase possibleChessCase = attackRange.GetComponent<ChessCase>();

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
            ChessCase possibleChessCase = attackRange.GetComponent<ChessCase>();

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
        for (int i = 0; i < selectedPiece.simpleAttacks.Count; i++)
        {
            Vector2 simpleAttack = selectedPiece.simpleAttacks[i];
            Vector3 attackPos = selectedCaseTransform.position;

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
        for (int i = 0; i < selectedPiece.infiniteAttacks.Count; i++)
        {
            Vector2 infiniteAttackValue = selectedPiece.infiniteMovements[i];
            int multiplier = 1;

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
        if (selectedPiece.canJumpPieces)
        {
            return true;
        }

        Vector3 direction = targetTransform.position - selectedCaseTransform.position;
        RaycastHit[] hits = Physics.RaycastAll(selectedCaseTransform.position, direction);

        List<float> distances = new List<float>();

        for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
        {
            distances.Add(hits[hitIndex].distance);
        }

        distances.Sort();

        int distanceIndex = 0;

        while (distanceIndex < distances.Count)
        {
            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                RaycastHit hit = hits[hitIndex];
                if (hit.distance == distances[distanceIndex])
                {
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

                    distanceIndex++;
                }

            }
        }

        return true;
    }

    void MovePiece()
    {
        Vector3 startPosition = selectedChessCase.currentPiece.position;
        Vector3 destination = new Vector3(targetedCaseTransform.position.x, startPosition.y, targetedCaseTransform.position.z);
        selectedPiece.agent.SetDestination(destination);
        game.SwitchTeam();
    }

    void AttackPiece()
    {
        MovePiece();
        targetedPiece.Taken();
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
            selectedChessCase.resetMaterial();
            selectedCaseTransform = null;
            selectedChessCase = null;
            selectedPiece = null;
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
            targetedChessCase.resetMaterial();
            targetedCaseTransform = null;
            targetedChessCase = null;
            targetedPiece = null;
        }

        foreach (Transform attacks in possibleAttacks)
        {
            ChessCase chessCase = attacks.GetComponent<ChessCase>();
            chessCase.resetMaterial();
        }
        possibleAttacks = new List<Transform>();
    }

}
