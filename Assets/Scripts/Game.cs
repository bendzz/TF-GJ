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

    [Tooltip("How sensitive the girl is to you screwing up and moving too fast; it'll take (1/X) seconds to fully startle her.")]
    public float girlSensitivityVR = 3f;
    public float girlSensitivityKeyboard = 3f;

    [Tooltip("How many degrees per second the joystick has to aim for to arouse but not discomfort her.")]
    public float joystickSwirlSpeed = 360;
    [Tooltip("How many seconds of perfect twirling to get her fully worked up")]
    public float hornyWorkUpTime = 5;
    [Tooltip("It'll take her (1/X) seconds to fully startle if you're joysticking her too hard.")]
    public float joydickSensitivity = 1;

    [Tooltip("Control her emotional state directly with the joystick. For debugging, but also hot.")]
    public bool cheatEmotionsJoystick = false;

    public Girl girl;

    // Working variables
    /// <summary>
    /// For interpolating a virtual trigger press for keyboard users, and tracking speed for VR users and keyboard users
    /// </summary>
    float triggerPress;
    /// <summary>
    /// Holds past joystick values up to "joystickOld1Period" seconds old, to determine how fast the joystick is moving 
    /// </summary>
    Queue<Vector3> joystickOld1;
    const float joystickOld1Period = .5f;

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
        joystickOld1 = new Queue<Vector3>();
        for (int i = 0; i < 10; i++)
        {
            joystickOld1.Enqueue(new Vector3(0,0,Time.time));  // does throwing away Vector3s cause garbage collection?
        }
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

        // update joystick
        Vector2 joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        {
            //joystickOld1
            Vector3 oldJoystick1;
            do
            {
                oldJoystick1 = joystickOld1.Peek();
                if (Time.time - oldJoystick1.z <= joystickOld1Period)
                    break;
                joystickOld1.Dequeue();
            } while (joystickOld1.Count > 0);
        }

        // pump her up
        float trigger = 0;
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0)
                expectingKeyboard = false;
            if (!expectingKeyboard)
            {
                trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                //triggerPress = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                float smoothDelta = triggerPressPower * Time.deltaTime;
                if (triggerPress < trigger)
                    triggerPress += smoothDelta; // TODO Test
                else
                    triggerPress -= smoothDelta;
                if (Mathf.Abs(triggerPress - trigger) * .9f < smoothDelta)
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



        // Joystick arouse and distract her
        {
            //joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            
            if (joystick.magnitude > .1f)
            {
                // Get the joystick rotation distance over "joystickOld1Period" seconds
                float angleDiff = 0;
                Vector3 oldold = joystickOld1.Peek();
                foreach (Vector3 old in joystickOld1)
                {
                    Vector3 oldv = new Vector2(old.x, old.y);
                    angleDiff += Vector2.SignedAngle(oldv, new Vector2(oldold.x, oldold.y)) * oldv.magnitude;
                    oldold = old;
                }
                float angleSpeed = angleDiff / (Time.time - joystickOld1.Peek().z);
                float rating = Mathf.Abs( angleSpeed / joystickSwirlSpeed);

                float overkill = Mathf.Clamp01(rating - 1);

                float hornyIncrease = (rating - overkill) * (1/hornyWorkUpTime) * Time.deltaTime;
                girl.horny += hornyIncrease;
                //girl.concern -= hornyIncrease;

                if (overkill > 0)
                    girl.startle += overkill * joydickSensitivity * Time.deltaTime;

                //print("angleDiff " + angleDiff + " angleSpeed " + angleSpeed + " rating " + rating + " count " + joystickOld1.Count);
            }

            // Lazy shitty keyboard arousal. TODO: Better
            if (Input.GetKey(KeyCode.D))
            {
                girl.horny += .7f * (1 / hornyWorkUpTime) * Time.deltaTime;
            }
        }


        // emote
        {
            if (cheatEmotionsJoystick)
            {
                //Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                girl.horny = Mathf.Clamp01(joystick.x);
                girl.concern = Mathf.Clamp01(joystick.y);
                girl.startle = Mathf.Clamp01(-joystick.y);
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

        //joystickOld = joystick;
        joystickOld1.Enqueue(new Vector3(joystick.x, joystick.y, Time.time));
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

    Transform girlBody;
    SkinnedMeshRenderer girlMesh;

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
        girlBody = Game.findChild(girlModel, "girl");

        girlMesh = girlBody.GetComponent<SkinnedMeshRenderer>();
        //girlMesh.sharedMesh.SetUVs(3, girlMesh.sharedMesh.vertices);
        //girlMesh.sharedMesh.SetUVs(3, girlMesh.sharedMesh.vertices);
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


        float furRamp = Mathf.Clamp01(Mathf.Pow(TF, 3));    // The fur starts fast and ends slow; fix that
        float furTF = Mathf.Lerp(.3f, -3.5f, Mathf.Clamp01(furRamp));    // My fur TF shader has a weird range, idk

        //Debug.Log("girlMesh.materials 0 " + girlMesh.materials[0].name);
        //Debug.Log("girlMesh.materials 1 " + girlMesh.materials[1].name);
        //Debug.Log("girlMesh.materials 2 " + girlMesh.materials[2].name);
        if (girlMesh.materials[0].name.ToLower() != "girl skin")
            Debug.LogWarning("Warning! Skin material not found, for fur TF!");
        girlMesh.materials[0].SetFloat("_FurTF", furTF);

        float blush = Mathf.Clamp(horny, 0, 2);
        blush = Mathf.Lerp(0.711f, 1.5f, blush);
        girlMesh.materials[0].SetFloat("_MakeupMultiplier", blush);
    }

    /// <summary>
    /// Call every update() once Girl's values have been set
    /// </summary>
    public void update()
    {
        // TODO set visual variables that have a max tween speed, ie don't snap to 100% instantly

        animator.SetFloat("PD1Startled", startle);
        animator.SetFloat("PD1Horny", horny);
        animator.SetFloat("PD1Concerned", concern);

        float startleDiff = .35f * Time.deltaTime;
        startle -= startleDiff;
        horny -= .2f * Time.deltaTime;

        concern += startleDiff - Mathf.Clamp01(-startle);

        concern = Mathf.Min(concern, (1 - (horny - .8f) * 2.6f));   // nearly max horny pushes concern out of her mind

        if (startle < .2f)
        {
            if (TF > .1f)
            {
                if (concern < .5f)
                    concern -= .1f * Time.deltaTime;
                else
                    concern += .1f * Time.deltaTime;
            }
            else
                concern -= .1f * Time.deltaTime;
        }

        startle = Mathf.Clamp01(startle);
        horny = Mathf.Clamp01(horny);
        concern = Mathf.Clamp01(concern);
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