using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elements : MonoBehaviour
{
    public Vector3 rotationPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Rotation
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);

        }
    }
}
