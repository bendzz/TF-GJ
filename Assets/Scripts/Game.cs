using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public Transform tailBase;
    public Transform EarBaseL;
    public Transform EarBaseR;

    public float TF;

    public Pump pump;

    float triggerPress;
    bool expectingKeyboard = false;
    [Tooltip("How much the trigger press increases/decreases in 1 second, when using keyboard input")]
    public float keyPressPower = 3f;
    public float earsTailMaxSize = 1.5f;
    public float undoTFRate = .3f;


    // Start is called before the first frame update
    void Start()
    {
        pump = new Pump();

        triggerPress = 0;
    } 

    private void Update()
    {
        OVRInput.Update();


        // pump her up
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0)
                expectingKeyboard = false;
            if (!expectingKeyboard)
            {
                triggerPress = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            }

            if (Input.GetKey(KeyCode.W))
            {
                expectingKeyboard = true;
                triggerPress += keyPressPower * Time.deltaTime;
            }
            else
            {
                if (expectingKeyboard)
                {
                    triggerPress -= keyPressPower * Time.deltaTime;
                }
            }
            triggerPress = Mathf.Clamp01(triggerPress);
            //print("triggerpress " + triggerPress);
        }
        // undo the TF
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0)
                pump.girlFill -= undoTFRate * Time.deltaTime * OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
            if (Input.GetKey(KeyCode.E))
                pump.girlFill -= undoTFRate * Time.deltaTime;
        }

        TF = pump.getTFDisplay(triggerPress);
        //print("TF " + TF);

        TF = Mathf.Clamp(TF, 0, 1.5f);
    }


    //void Update()
    private void LateUpdate()
    {
        Vector3 earsTail = Vector3.one * Mathf.Clamp(TF, 0, earsTailMaxSize);
        tailBase.localScale = earsTail;
        EarBaseL.localScale = earsTail;
        EarBaseR.localScale = earsTail;
    }
}
 
/// <summary>
/// Lets you pump the girl up to TF her, with your trigger
/// </summary>
public class Pump
{
    /// <summary>
    /// How TF'd she appears
    /// </summary>
    float TFDisplay = 0;
    

    /// <summary>
    /// How TF'd she actually is (minus temporary air)
    /// </summary>
    public float girlFill = 0;
    /// <summary>
    /// How much air the pump has pushed into the girl, temporarily. (Much will 'leak back out' when the pump is released)
    /// </summary>
    float pumpFill = 0;


    float oldPumpDepressed = 0;

    /// <summary>
    /// How much a full pump would inflate her
    /// </summary>
    public float pumpVolume = .2f;

    /// <summary>
    /// How much of each pump actually stays in her when released
    /// </summary>
    public float pumpRetained = .4f;


    public Pump()
    {
    }

    /// <summary>
    /// How TF'd to display her
    /// </summary>
    /// <param name="pumpDepressed">Where the pump handle is; 1 is pump is fulled depressed, 0 is the handle's pulled out ready to pump. (Basically, the controller trigger value) </param>
    /// <returns></returns>
    public float getTFDisplay(float pumpDepressed)
    {
        float pumpChange = pumpDepressed - oldPumpDepressed;
        if (pumpChange > 0)
        {
            pumpFill += pumpChange;
        } else if (pumpChange < 0)
        {
            girlFill += -pumpChange * pumpRetained * pumpVolume;
            pumpFill -= -pumpChange;
        }

        TFDisplay = girlFill + pumpFill * pumpVolume;

        oldPumpDepressed = pumpDepressed;
        return TFDisplay;
    }
}