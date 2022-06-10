using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Transform girl;
    public Transform girlHead;
    public Transform hair;

    // Start is called before the first frame update
    void Start()
    {
        hair.parent = girlHead;
        //print("hair parent " + hair.parent);
        hair.transform.localPosition = Vector3.zero;
        hair.transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
