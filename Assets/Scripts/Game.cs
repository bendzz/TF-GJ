using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Transform tailBase;
    public Transform EarBaseL;
    public Transform EarBaseR;

    public float TF;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        OVRInput.Update();

        TF -= OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) * .1f;     // grip
        TF += OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) * .1f;    // trigger

        if (Input.GetKey(KeyCode.W))
            TF += .05f;
        if (Input.GetKey(KeyCode.E))
            TF -= .05f;

        TF = Mathf.Clamp(TF, 0, 2);
    }

    //void Update()
    private void LateUpdate()
    {
        tailBase.localScale = new Vector3(TF, TF, TF);
        EarBaseL.localScale = new Vector3(TF, TF, TF);
        EarBaseR.localScale = new Vector3(TF, TF, TF);
    }
}
