using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

public class PieceMove : MonoBehaviour
{
    public Transform selectedCaseTransform;
    public Transform targetedCaseTransform;

    public ChessCase selectedChessCase;
    public ChessCase targetedChessCase;

    public Piece selectedPiece;
    public Piece targetedPiece;

    public int layerMask = ~(1 << 7);

    public bool canSelect = true;
    private Game game;
    private ArduinoManager arduinoManager;

    [SerializeField] public Material selectedPieceMaterial;
    [SerializeField] public Material possibleMoveMaterial;
    [SerializeField] public Material possibleAttackMaterial;

    // Start is called before the first frame update
    void Start()
    {
        game = GetComponent<Game>();
        arduinoManager = GetComponent<ArduinoManager>();
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
        if (!canSelect)
        {
            return;
        }
        ChessCase chessCase = chessCaseTransform.GetComponent<ChessCase>();
        if (!IsSelected())
        {
            if (chessCase.currentPieceTransform == null)
            {
                return;
            }

            selectedCaseTransform = chessCaseTransform;
            selectedChessCase = chessCase;
            selectedPiece = chessCase.currentPieceTransform.GetComponent<Piece>();

            chessCase.setMaterial(selectedPieceMaterial);
            foreach(Transform move in selectedPiece.possibleMoves)
            {
                ChessCase moveChessCase = move.GetComponent<ChessCase>();
                moveChessCase.setMaterial(possibleMoveMaterial);
            }

            foreach (Transform attack in selectedPiece.possibleAttacks)
            {
                ChessCase moveChessCase = attack.GetComponent<ChessCase>();
                moveChessCase.setMaterial(possibleAttackMaterial);
            }
        } else
        {
            if (selectedCaseTransform == chessCaseTransform || selectedPiece.team != game.whoPlays)
            {
                UnselectAll();
                return;
            }

            targetedCaseTransform = chessCaseTransform;
            targetedChessCase = chessCase;
            if (targetedChessCase.currentPieceTransform != null)
            {
                targetedPiece = targetedChessCase.currentPieceTransform.GetComponent<Piece>();
            }

            TryActionPiece();
        }
    }

    void TryActionPiece()
    {
        if (targetedChessCase.currentPieceTransform != null)
        {
            TryAttackPiece(selectedPiece);
        } else
        {
            TryMovePiece();
        }
    }

    void TryMovePiece()
    {
        if (!selectedPiece.possibleMoves.Contains(targetedCaseTransform))
        {
            UnselectTarget();
            return;
        }
        MovePiece();
        UnselectAll();
    }

    public void CheckPossibleMoves(Piece piece)
    {
        foreach (Transform moveRange in GetMoveCase(piece))
        {
            if (CheckObstacle(piece, moveRange) && CheckRisk(piece, moveRange))
            {
                ChessCase possibleChessCase = moveRange.GetComponent<ChessCase>();
                if (possibleChessCase.currentPieceTransform == null)
                {
                    piece.possibleMoves.Add(moveRange);
                }
            }
        }

        foreach (Transform moveRange in GetMoveInfinite(piece))
        {
            if (CheckObstacle(piece, moveRange) && CheckRisk(piece, moveRange))
            {
                ChessCase possibleChessCase = moveRange.GetComponent<ChessCase>();
                if (possibleChessCase.currentPieceTransform == null)
                {
                    piece.possibleMoves.Add(moveRange);
                }
            }
        }
    }
    List<Transform> GetMoveCase(Piece piece)
    {
        List<Transform> ranges = new List<Transform>();
        for (int i = 0; i < piece.simpleMovements.Count; i++)
        {
            Vector2 simpleMovement = piece.simpleMovements[i];
            Vector3 movePos = piece.chessCase.transform.position;

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

    List<Transform> GetMoveInfinite(Piece piece)
    {
        List<Transform> ranges = new List<Transform>();
        for (int i = 0; i < piece.infiniteMovements.Count; i++)
        {
            Vector2 infiniteMovementValue = piece.infiniteMovements[i];
            int multiplier = 1;

            while (true)
            {
                Vector3 movementPos = piece.chessCase.transform.position;
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

    void TryAttackPiece(Piece piece)
    {
        if (piece.team == targetedPiece.team || !piece.possibleAttacks.Contains(targetedCaseTransform))
        {
            UnselectTarget();
            return;
        }
        AttackPiece();
        selectedChessCase.CheckPieceTransform();
        targetedChessCase.CheckPieceTransform();
        UnselectAll();
    }

    public void CheckPossibleAttacks(Piece piece)
    {
        foreach (Transform attackRange in GetAttackCase(piece))
        {
            ChessCase possibleChessCase = attackRange.GetComponent<ChessCase>();

            if (possibleChessCase.currentPieceTransform != null)
            {
                Piece possiblePiece = possibleChessCase.currentPieceTransform.GetComponent<Piece>();
                if (CheckObstacle(piece, attackRange) && possiblePiece.team != piece.team && CheckRisk(piece, attackRange))
                {
                    piece.possibleAttacks.Add(attackRange);
                }
            }
        }

        foreach (Transform attackRange in GetAttackInfinite(piece))
        {
            ChessCase possibleChessCase = attackRange.GetComponent<ChessCase>();

            if (possibleChessCase.currentPieceTransform != null)
            {
                Piece possiblePiece = possibleChessCase.currentPieceTransform.GetComponent<Piece>();
                if (CheckObstacle(piece, attackRange) && possiblePiece.team != piece.team && CheckRisk(piece, attackRange))
                {
                    piece.possibleAttacks.Add(attackRange);
                }
            }
        }
    }
    List<Transform> GetAttackCase(Piece piece)
    {
        List<Transform> ranges = new List<Transform>();
        for (int i = 0; i < piece.simpleAttacks.Count; i++)
        {
            Vector2 simpleAttack = piece.simpleAttacks[i];
            Vector3 attackPos = piece.chessCase.transform.position;

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

    List<Transform> GetAttackInfinite(Piece piece)
    {
        List<Transform> ranges = new List<Transform>();
        for (int i = 0; i < piece.infiniteAttacks.Count; i++)
        {
            Vector2 infiniteAttackValue = piece.infiniteMovements[i];
            int multiplier = 1;

            while (true)
            {
                Vector3 movementPos = piece.chessCase.transform.position;
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

    bool CheckObstacle(Piece piece, Transform destination)
    {
        if (piece.canJumpPieces)
        {
            return true;
        }

        Vector3 direction = destination.position - piece.chessCase.transform.position;
        RaycastHit[] hits = Physics.RaycastAll(piece.chessCase.transform.position, direction);

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
                    if (hit.transform != piece.transform)
                    {
                        if (hit.transform == destination)
                        {
                            return true;
                        }
                        if (chessCase.currentPieceTransform != null)
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

    public bool CheckRisk(Piece piece, Transform destination)
    {
        if (piece.canRisk)
        {
            return true;
        }
        List<GameObject> defenders = new List<GameObject>();
        GameObject defenderTeam;
        List<Transform> riskCaseTransforms = new List<Transform>();
        if (piece.team == "lights")
        {
            defenderTeam = game.darkPawns;
        } else
        {
            defenderTeam = game.lightPawns;
        }
        for (int i = 0; i < defenderTeam.transform.childCount; i++)
        {
            defenders.Add(defenderTeam.transform.GetChild(i).gameObject);
        }

        foreach (GameObject defender in defenders)
        {
            Piece defenderPiece = defender.GetComponent<Piece>();
            foreach (Transform possibleAttack in defenderPiece.possibleAttacks)
            {
                if (riskCaseTransforms.Contains(possibleAttack.transform))
                {
                    riskCaseTransforms.Add(possibleAttack.transform);
                }
            }
        }

        return !riskCaseTransforms.Contains(destination);
    }

    void MovePiece()
    {
        Vector3 startPosition = selectedPiece.transform.position;
        Vector3 destination = new Vector3(targetedCaseTransform.position.x, startPosition.y, targetedCaseTransform.position.z);
        //Vector3 distances = targetedCaseTransform.position - selectedPiece.transform.localPosition;
        Vector2 distances = new Vector2(targetedCaseTransform.localPosition.x, targetedCaseTransform.localPosition.z);
        StartCoroutine(Move(selectedPiece, destination));
        arduinoManager.MoveArms(Vector2.zero, distances);
    }

    IEnumerator Move(Piece piece, Vector3 destination)
    {
        bool value = true;
        canSelect = false;
        piece.agent.SetDestination(destination);
        while (value)
        {
            // Check if we've reached the destination
            if (!piece.agent.pathPending)
            {
                if (piece.agent.remainingDistance <= piece.agent.stoppingDistance)
                {
                    if (!piece.agent.hasPath || piece.agent.velocity.sqrMagnitude == 0f)
                    {
                        canSelect = true;
                        game.SwitchTeam();
                        game.CheckGameStatus();
                        value = false;
                    }
                }
            }
            yield return new WaitForSeconds(.1f);
        }
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
        selectedChessCase.resetMaterial();
        if (selectedCaseTransform != null)
        {
            selectedCaseTransform = null;
            selectedChessCase = null;

            selectedPiece.Unselect();
            selectedPiece = null;
        }

    }

    void UnselectTarget()
    {
        if (targetedCaseTransform != null)
        {
            targetedCaseTransform = null;
            targetedChessCase = null;

            if (targetedPiece != null)
            {
                targetedPiece.Unselect();
                targetedPiece = null;
            }
        }

    }

}
