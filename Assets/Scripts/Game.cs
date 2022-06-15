using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game instance;
    public Transform girlModel;


    //public float TF;

    public Pump pump;

    bool expectingKeyboard = false;
    [Tooltip("How much the trigger press increases/decreases in 1 second, when using keyboard input")]
    public float keyPressPower = 2f;
    [Tooltip("For VR trigger")]
    public float triggerPressPower = 2f;
    public float earsTailMaxSize = 1.5f;
    [Tooltip("Undoing the TF is more of a debug/for fun thing...")]
    public float undoTFRate = .5f;

    [Tooltip("How sensitive the girl is to you screwing up and moving too fast.")]
    public float girlSensitivityVR = .5f;
    public float girlSensitivityKeyboard = .5f;


    [Tooltip("Control her emotional state directly with the joystick. For debugging, but also hot.")]
    public bool cheatEmotionsJoystick = false;

    public Girl girl;

    // Working variables
    /// <summary>
    /// For interpolating a virtual trigger press for keyboard users, and tracking speed for VR users and keyboard users
    /// </summary>
    public float triggerPress;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("Only one instance of Game.cs allowed!");

        pump = new Pump();
        girl = new Girl(girlModel);

        triggerPress = 0;
    } 

    /// <summary>
    /// Recursively searched all children for item. Ignores capitalization 
    /// </summary>
    public static Transform findChild(Transform parent, string name)
    {
        foreach(Transform child in parent)
        {
            if (child.name.ToLower() == name.ToLower())
                return child;
            Transform result = findChild(child, name);
            if (result)
                return result;
        }
        return null;
    }

    private void Update()
    {
        OVRInput.Update();

        // pump her up
        float trigger = 0;
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0)
                expectingKeyboard = false;
            if (!expectingKeyboard)
            {
                trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                //triggerPress = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                if (triggerPress < trigger)
                    triggerPress += triggerPressPower * Time.deltaTime; // TODO Test
                else
                    triggerPress -= triggerPressPower * Time.deltaTime;
                if (Mathf.Abs(triggerPress - trigger) < triggerPressPower * Time.deltaTime)
                    triggerPress = trigger;
            }

            bool keyWEPressed = false;
            float keypp = keyPressPower * Time.deltaTime;
            if (Input.GetKey(KeyCode.W))
            {
                expectingKeyboard = true;
                if (pump.inflating)
                {
                    triggerPress += keypp;
                } else
                {
                    triggerPress -= keypp;
                    girl.startle += girlSensitivityKeyboard * Time.deltaTime;
                }
                keyWEPressed = true;
            }
            else
            {
                if (Input.GetKey(KeyCode.E))
                {
                    expectingKeyboard = true;
                    if (!pump.inflating)
                    {
                        triggerPress -= keypp;
                    }
                    else
                    {
                        triggerPress += keypp;
                        girl.startle += girlSensitivityKeyboard * Time.deltaTime;
                    }
                keyWEPressed = true;
                }

                //if (expectingKeyboard)
                //{
                //    triggerPress -= keyPressPower * Time.deltaTime;
                //}
            }

            if (expectingKeyboard)
            {
                if (!keyWEPressed)
                {
                    if (pump.pumpFill >= 1)
                    {
                        pump.inflating = false;
                    }
                    else if (pump.pumpFill <= 0)
                    {
                        pump.inflating = true;
                    }
                }
            }

            triggerPress = Mathf.Clamp01(triggerPress);
            //print("triggerpress " + triggerPress);
        }
        // undo the TF
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0)
                pump.girlFill -= undoTFRate * Time.deltaTime * OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
            if (Input.GetKey(KeyCode.Q))
                pump.girlFill -= undoTFRate * Time.deltaTime;
            if (pump.girlFill < 0)
                pump.girlFill = 0;
        }

        girl.TF = pump.getTFDisplay(triggerPress);
        //print("TF " + TF);

        girl.TF = Mathf.Clamp(girl.TF, 0, 1.5f);

        // emote
        {
            if (cheatEmotionsJoystick)
            {
                Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                girl.horny = Mathf.Clamp01(stick.x);
                girl.concern = Mathf.Clamp01(stick.y);
                girl.startle = Mathf.Clamp01(-stick.y);
            }

            // check if VR player is too enthusiastic
            if (!expectingKeyboard)
            {
                if (trigger != triggerPress)
                {
                    float diff = Mathf.Abs(trigger - triggerPress);
                    print("diff " + diff);

                    girl.startle += diff * girlSensitivityVR * Time.deltaTime;
                }
            }

        }

        girl.update();
    }


    //void Update()
    private void LateUpdate()
    {
        girl.lateUpdate();
    }
}
 
public class Girl
{
    public Transform girl;
    
    // emotional state; top items overrule lower items.
    public float startle;
    public float horny;
    public float concern;
    
    public float TF;


    Transform tailBase;  // TODO grab these automatically
    Transform EarBaseL;
    Transform EarBaseR;

    Animator animator;
    RuntimeAnimatorController controller;

    public Girl(Transform girlModel)
    {
        girl = girlModel;

        animator = girlModel.GetComponent<Animator>();
        controller = animator.runtimeAnimatorController;
        tailBase = Game.findChild(girlModel, "tail1");
        EarBaseL = Game.findChild(girlModel, "ear1.L");
        EarBaseR = Game.findChild(girlModel, "ear1.R");
    }

    /// <summary>
    /// Used to update the bone scaling
    /// </summary>
    public void lateUpdate()
    {
        Vector3 earsTail = Vector3.one * Mathf.Clamp(TF, 0, Game.instance.earsTailMaxSize);
        tailBase.localScale = earsTail;
        EarBaseL.localScale = earsTail;
        EarBaseR.localScale = earsTail;
    }

    /// <summary>
    /// Call every update() once Girl's values have been set
    /// </summary>
    public void update()
    {
        animator.SetFloat("PD1Startled", startle);
        animator.SetFloat("PD1Horny", horny);
        animator.SetFloat("PD1Concerned", concern);
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
    public float pumpFill = 0;


    float oldPumpDepressed = 0;

    /// <summary>
    /// How much a full pump would inflate her
    /// </summary>
    public float pumpVolume = .2f;

    /// <summary>
    /// How much of each pump actually stays in her when released
    /// </summary>
    public float pumpRetained = .4f;

    /// <summary>
    /// Keyboard users have to alternate presses to inflate her
    /// </summary>
    public bool inflating = true;

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