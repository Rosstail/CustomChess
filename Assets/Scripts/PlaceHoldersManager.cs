using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceHoldersManager : MonoBehaviour
{

    [SerializeField] List<Transform> placeHolderTransforms = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Check()
    {
        foreach(Transform transform in placeHolderTransforms)
        {
            transform.GetComponent<PlaceHolder>().Display();
        }
    }
}
