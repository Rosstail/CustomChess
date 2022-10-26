using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessCase : MonoBehaviour
{
    public Transform currentPiece;
    // Start is called before the first frame update
    void Start()
    {
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

        currentPiece = newPiece;
    }
}
