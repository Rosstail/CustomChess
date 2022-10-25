using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{

    Transform currentCaseLocation;
    [SerializeField] public List<Vector2> movements;


    // Start is called before the first frame update
    void Start()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100f))
        {
            Transform raycastTransform = hit.transform;

            if (raycastTransform.tag == "Case")
            {
                currentCaseLocation = raycastTransform;
                Debug.Log(transform.name + " Current location is " + currentCaseLocation.name);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
