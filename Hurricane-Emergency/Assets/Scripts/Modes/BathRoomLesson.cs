using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathRoomLesson : MonoBehaviour, ISimulationMode
{
    public enum BathRoomAnimations
    {
        PackFirstAid,
        PackToothbrush,
        PackWipes,
        PackSoap,
        PuckHairDryer, // wrong Things
        PackPump,
        PackWashingGel,
        PackCleaningSpray
    }

    [SerializeField] private GameObject kelen;
    [SerializeField] private GameObject key;
    [SerializeField] private GameObject dad;
    [SerializeField] private GameObject bag;
    [SerializeField] private Transform kayBagPosition;
    [SerializeField] private GameObject reminderSprite;


    [SerializeField] Transform roomOffset;
    [SerializeField] Transform roomToyOffset;

    private Animator kelenAnimator;
    private Animator keyAnimator;
    private Animator dadAnimator;

    public bool playAnimation; // Variable to trigger the animation


    //kay animation
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int GetBagHash = Animator.StringToHash("GetBag");
    private static readonly int OpenBagHash = Animator.StringToHash("OpenBag");
    private static readonly int IdleHash = Animator.StringToHash("Idle"); // kelan idle animation

    // Kelan Pick Up animations

    private static readonly int PicUp = Animator.StringToHash("PicUp");
    private static readonly int ToHandsUper = Animator.StringToHash("ToHandsUper");
    private static readonly int TakeWithToHands = Animator.StringToHash("TakeWithToHands");
    private static readonly int TakeWithOneHand = Animator.StringToHash("TakeWithOneHand");


    [SerializeField] private GameObject bathRoomCabinet;

    private bool bathRoomCabinetIsOpen = false; // Track the state of the fridge








    [Header("Settings")]
    [SerializeField] private float stopDistance = 0.01f;
    [SerializeField] private float pickBagDelay = 0.3f;
    [SerializeField] private float openBagDelay = 2f;
    [SerializeField] private Vector3 finalPoint;
    private Vector3 bagPositionRight = new Vector3(2.5f, -2f, 0f); // Position of the bag
    private Vector3 bagPositionLeft = new Vector3(4.6f, -1.5f, 0f); // Position of the bag

    private Coroutine KeyCoroutine;

    private SequentialAnimationQueue<BathRoomAnimations> animationQueue;

    private Dictionary<BathRoomAnimations, Action<Action>> animationMap;

    public float timer = 0;
    public bool simulationStart = false;



    void Awake()
    {
        dadAnimator = dad.GetComponent<Animator>();
        kelenAnimator = kelen.GetComponent<Animator>();
        keyAnimator = key.GetComponent<Animator>();

        animationMap = new Dictionary<BathRoomAnimations, Action<Action>>()
        {
            { BathRoomAnimations.PackFirstAid, PackFirstAid },
            { BathRoomAnimations.PackToothbrush, PackToothbrush },
            { BathRoomAnimations.PackWipes, PackWipes },
            { BathRoomAnimations.PackSoap, PackSoap },
            { BathRoomAnimations.PuckHairDryer, PuckHairDryer },
            { BathRoomAnimations.PackPump, PackPump },
            { BathRoomAnimations.PackWashingGel, PackWashingGel },
            { BathRoomAnimations.PackCleaningSpray, PackCleaningSpray }
        };
        animationQueue = new SequentialAnimationQueue<BathRoomAnimations>(animationMap);
    }



    public void AddGoBagBathroomAnimationFromWeb(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        Debug.Log($"added animation from web: {animationName}");

        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning("Empty animation name");
            return;
        }

        AnimationEnqueueResult result = animationQueue.Enqueue(animationName, out BathRoomAnimations animation);
        if (result == AnimationEnqueueResult.Added)
        {
            Debug.Log($"Animation added to queue: {animation}");

            if (animationQueue.NeedsProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        else
        {
            Debug.LogWarning($"No such animation in enum Animations: {animationName}");
        }
    }

    private IEnumerator ProcessQueue()
    {
        yield return animationQueue.Process(
            next => Debug.Log("Animation finished: " + next),
            next => Debug.LogWarning("No function for animation: " + next));
    }
    void Start()
    {
        // keyAnimator.SetTrigger("Idle");


    }

    // Update is called once per frame
    void Update()
    {
        if (simulationStart)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0f;
                simulationStart = false;
                StopCoroutine(KeyCoroutine);
                keyAnimator.SetTrigger("Idle");
                DadEnterTheRoom();
                CheckAmnamitionsInEditor(); // for checking in editor
            }
        }
    }


    public void DadEnterTheRoom()
    {
        StartCoroutine(DadEnterRoomCoroutine());
    }

    private void OpenBathRoomCabinet()
    {
        if (bathRoomCabinetIsOpen) return; // If the kitchen cabinet is already open, do nothing
        bathRoomCabinetIsOpen = true;
        bathRoomCabinet.SetActive(true); // Show the kitchenCabinet sprite

    }



    IEnumerator DadEnterRoomCoroutine()
    {
        bool flipX = true; // Set to true to flip the character when moving left
        dadAnimator.SetTrigger("Walk");
        yield return MoveToTarget(dad.transform, new Vector3(-6f, -1.5f, 0f), 2f, flipX);
        WebGLBridge.GivesReminder(ObjectsHolder.instance.GetKelanParentsID()); // Call the WebGL function to notify that dad gives reminder to kelen
        dadAnimator.SetTrigger("Idle");
        WebGLBridge.SendEvent(Events.GobagReminder.ToString()); // Send the event to WebGL
        KelanTakeBag();

        yield return new WaitForSeconds(2f);
        dadAnimator.SetTrigger("Walk");
        yield return MoveToTarget(dad.transform, new Vector3(-11f, -2f, 0f), 2f, flipX);
        dadAnimator.SetTrigger("Idle");

    }

    public void BathroomOnGivesReminder() //Called from WebGL when dad gives reminder to kelen
    {
        StartCoroutine(OnGivesReminderCorutine());

    }
    IEnumerator OnGivesReminderCorutine()
    {
        dadAnimator.SetTrigger("Idle");
        reminderSprite.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        reminderSprite.gameObject.SetActive(false);
    }

    public void CheckAmnamitionsInEditor()
    {
        if (Application.isEditor)
        {
            keyAnimator.SetTrigger("Idle"); // for checking in editor, you can comment this line when testing in WebGL
                                            // KeyTakeBag();

            AddGoBagBathroomAnimationFromWeb("PackFirstAid");
            AddGoBagBathroomAnimationFromWeb("PackToothbrush");
            AddGoBagBathroomAnimationFromWeb("PackWipes");
            AddGoBagBathroomAnimationFromWeb("PackSoap");
            AddGoBagBathroomAnimationFromWeb("PuckHairDryer");
            AddGoBagBathroomAnimationFromWeb("PackPump");
            AddGoBagBathroomAnimationFromWeb("PackWashingGel");
            AddGoBagBathroomAnimationFromWeb("PackCleaningSpray");
        }
    }



    public void Initialize()
    {

    }

    public void OnSimulationStart()
    {
        playAnimation = true;
        KeyPlay();
    }

    public void OnSimulationEnd()
    {
        Debug.Log("end ChildrenRoom mode");
    }

    public void Cleanup()
    {
        Debug.Log("Cleanup ChildrenRoom mode");
    }



    // #region Right side items: Water, Toy, books, kandels


    // public void KelenTakeFish(Action onComplete)
    // {
    //     StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(4.5f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Fish", onComplete));
    // }

    // public void KelenTakeChicken(Action onComplete)
    // {
    //     StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(4.5f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Chicken", onComplete));
    // }
    // public void KelenTakeChees(Action onComplete)
    // {
    //     StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(4.5f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Chees", onComplete));
    // }
    // public void KelenTakeEggs(Action onComplete)
    // {
    //     StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(4f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Eggs", onComplete));
    // }


    // IEnumerator KelanTakeObjectsCoroutineRightSide(GameObject objToMove, Vector3 targetPosition, float speed,
    //  int picUpanimation, Events eventTyp, string objectTotake, Action onComplete)
    // {

    //     Transform objectTransform = objToMove.transform;

    //     kelenAnimator.SetTrigger("Walk");
    //     yield return MoveToTarget(objectTransform, targetPosition, speed);

    //     kelenAnimator.SetTrigger(picUpanimation);

    //     yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation
    //     SetRoomObjectActiveByName(objectTotake, true);

    //     kelenAnimator.SetTrigger("WalkWith");

    //     yield return MoveToTarget(objectTransform, bagPositionRight, speed);
    //     kelenAnimator.enabled = false; // Disable the animator to stop any ongoing animations
    //     SetRoomObjectActiveByName(objectTotake, false);
    //     WebGLBridge.SendEvent(eventTyp.ToString());
    //     kelenAnimator.enabled = true;
    //     kelenAnimator.SetTrigger("Idle");
    //     yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
    //     onComplete?.Invoke();
    // }

    // #endregion


    #region left side items: Crackers, water, canned food
    public void PackFirstAid(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(-4.5f, -1.2f, 0f), 2f, PicUp, Events.PackFirstAid, "FirstAid", onComplete));
    }
    public void PackToothbrush(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(3f, -1.3f, 0f), 2f, TakeWithOneHand, Events.PackToothbrush, "Toothpaste", onComplete));
    }

    public void PackWipes(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(-4.5f, -1.2f, 0f), 2f, TakeWithOneHand, Events.PackWipes, "Wipers", onComplete));
    }

    public void PackSoap(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(2f, -1.2f, 0f), 2f, TakeWithOneHand, Events.PackSoap, "Soup", onComplete));
    }
    public void PuckHairDryer(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(-3f, -1.2f, 0f), 2f, TakeWithOneHand, Events.PackWater, "Hairdryer", onComplete));
    }

    public void PackPump(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(-4.5f, -1.2f, 0f), 2f, TakeWithToHands, Events.PackCannedFood, "Pump", onComplete));
    }
    public void PackWashingGel(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(-3f, -1.2f, 0f), 2f, TakeWithToHands, Events.PackWater, "WashingGel", onComplete));
    }

    public void PackCleaningSpray(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(key, new Vector3(-4f, -1.2f, 0f), 2f, TakeWithToHands, Events.PackCannedFood, "CleaningSpray", onComplete));
    }



    IEnumerator KelanTakeObjectsCoroutineLeftSide(GameObject objToMove, Vector3 targetPosition, float speed,
     int picUpanimation, Events eventTyp, string objectTotake, Action onComplete)
    {

        Transform objectTransform = objToMove.transform;

        keyAnimator.SetTrigger("Walk");
        // yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, targetPosition, speed);
        if (objectTotake != "Wipers" || objectTotake != "Hairdryer" || objectTotake != "Toothpaste" || objectTotake != "Soup")
        {
            OpenBathRoomCabinet();
        }

        keyAnimator.SetTrigger(picUpanimation);

        yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation
        SetRoomObjectActiveByName(objectTotake, true);

        keyAnimator.SetTrigger("WalkWith");
        //  yield return MoveToTarget(objectTransform, roomOffset.position, speed);

        yield return MoveToTarget(objectTransform, bagPositionLeft, speed);
        keyAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        SetRoomObjectActiveByName(objectTotake, false);
        WebGLBridge.SendEvent(eventTyp.ToString());
        keyAnimator.enabled = true;
        keyAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }
    #endregion



    public void KelanTakeBag()
    {
        StartCoroutine(KelanWalkToBag(kelen, new Vector3(10.5f, -2f, 0f), 2f));
    }

    public void KeyPlay()
    {
        KeyCoroutine = StartCoroutine(StartKeyPlay(key, new Vector3(-4f, -2f, 0f), 2f));
    }


    private IEnumerator StartKeyPlay(GameObject objToMove, Vector3 endPoint, float speed)
    {
        float startScale = objToMove.transform.localScale.x; // Adjust this value to change the size of the sprite
        while (playAnimation == true)
        {
            Vector3 startPoint = objToMove.transform.position;
            objToMove.transform.localScale = new Vector3(startScale, startScale, startScale); // Set the sprite scale
            keyAnimator.SetTrigger("Play");
            Vector3 targetPoint = endPoint;
            while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
            {
                objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
                yield return null;
            }
            objToMove.transform.localScale = new Vector3(startScale * -1, startScale, startScale); // Set the sprite scale
            while (Vector3.Distance(objToMove.transform.position, startPoint) > 0.01f)
            {
                objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, startPoint, speed * Time.deltaTime);
                yield return null;
            }
        }
    }


    public IEnumerator KelanWalkToBag(GameObject objToMove, Vector3 bagPosition, float speed)
    {
        if (objToMove == null)
        {
            Debug.LogWarning("KeyWalkPlay: objToMove is null.");
            yield break;
        }

        Transform objectTransform = objToMove.transform;

        SetUniformScale(objectTransform);

        kelenAnimator.SetTrigger(WalkHash);
        yield return MoveToTarget(objectTransform, bagPosition, speed);

        kelenAnimator.SetTrigger(GetBagHash);
        yield return new WaitForSeconds(pickBagDelay);

        yield return MoveToTarget(bag.transform, kayBagPosition.position, speed);
        bag.transform.SetParent(kayBagPosition);

        kelenAnimator.SetTrigger(WalkHash);
        yield return MoveToTarget(objectTransform, finalPoint, speed);

        kelenAnimator.SetTrigger(IdleHash);
        bag.GetComponent<Animator>().SetTrigger(OpenBagHash);

        yield return new WaitForSeconds(openBagDelay);

        // Vector3 ls = objToMove.transform.localScale;
        // ls.x = objToMove.transform.localScale.x * -1f;
        // objToMove.transform.localScale = ls;

    }

    private IEnumerator MoveToTarget(Transform objectTransform, Vector3 targetPoint, float speed, bool flipX = false)
    {
        return CharacterMotion.MoveToTarget(objectTransform, targetPoint, speed, stopDistance, flipX);
    }

    private void SetUniformScale(Transform objectTransform)
    {
        CharacterMotion.SetUniformScale(objectTransform);
    }

    private void SetRoomObjectActiveByName(string objectName, bool activate)
    {
        RoomObjectActivation.SetActive(key, objectName, activate);
    }


    public void FlipObjects(GameObject objToFlip, Vector3 targetPoint, bool flipX = false)
    {
        CharacterMotion.FaceTarget(objToFlip != null ? objToFlip.transform : null, targetPoint, flipX);
    }
}
