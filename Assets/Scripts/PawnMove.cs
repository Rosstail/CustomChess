using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform != null)
                {
                    Transform clickedTransform = raycastHit.transform;
                    //Our custom method.

                    if (clickedTransform.tag != "Untagged")
                    {
                        Debug.Log(clickedTransform.name + " " + clickedTransform.tag);
                    }
                    //CurrentClickedGameObject(raycastHit.transform.gameObject);
                }
            }
        }
    }
}
