using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessCase : MonoBehaviour
{
    public Transform currentPiece;
    // Start is called before the first frame update
    void Start()
    {
        CheckPawnTransform();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckPawnTransform()
    {
        Transform newPiece = null;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.up, 100f);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            Transform raycastTransform = hit.transform;

            if (raycastTransform.gameObject.activeSelf && raycastTransform.tag == "Piece")
            {
                newPiece = raycastTransform;
                break;
            }
        }

        currentPiece = newPiece;
    }
}
