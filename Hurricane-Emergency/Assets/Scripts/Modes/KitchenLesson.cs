using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenLesson : MonoBehaviour, ISimulationMode
{
    public enum KitchenAnimations
    {
        KayTakeChees,
        KayTakeEggs,
        KayTakeChicken,
        KayTakeFish,
        KayTakeCannedFood,
        KayTakeCrackers,
        KayTakeWater,
    }

    [SerializeField] private GameObject kayObj;
    [SerializeField] private GameObject kelanObj;
    [SerializeField] private GameObject dad;
    [SerializeField] private GameObject bag;
    [SerializeField] private Transform kayBagPosition;
    [SerializeField] private GameObject reminderSprite;


    [SerializeField] Transform roomOffset;
    [SerializeField] Transform roomToyOffset;

    private Animator kayAnimator;
    private Animator kelanAnimator;
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

    [SerializeField] private GameObject fridgeSprite;

    [SerializeField] private GameObject kitchenCabinet;

    private bool isFridgeOpen = false; // Track the state of the fridge
    private bool kitchenCabinetIsOpen = false; // Track the state of the fridge


    [Header("Settings")]
    [SerializeField] private float stopDistance = 0.01f;
    [SerializeField] private float pickBagDelay = 0.3f;
    [SerializeField] private float openBagDelay = 2f;
    [SerializeField] private Vector3 finalPoint;
    private Vector3 bagPositionRight = new Vector3(2.5f, -2f, 0f); // Position of the bag
    private Vector3 bagPositionLeft = new Vector3(0.5f, -2f, 0f); // Position of the bag

    private Coroutine KayCoroutine;

    private readonly Queue<KitchenAnimations> animationQueue = new();
    private bool isPlaying;
    private bool currentAnimationFinished;

    private Dictionary<KitchenAnimations, Action<Action>> animationMap;

    public float timer = 0;
    public bool simulationStart = false;



    void Awake()
    {
        dadAnimator = dad.GetComponent<Animator>();
        kayAnimator = kayObj.GetComponent<Animator>();
        kelanAnimator = kelanObj.GetComponent<Animator>();

        animationMap = new Dictionary<KitchenAnimations, Action<Action>>()
        {
            { KitchenAnimations.KayTakeChees, KayTakeChees },
            { KitchenAnimations.KayTakeEggs, KayTakeEggs },
            { KitchenAnimations.KayTakeChicken, KayTakeChicken },
            { KitchenAnimations.KayTakeFish, KayTakeFish },
            { KitchenAnimations.KayTakeCannedFood,KayTakeCannedFood  },
            { KitchenAnimations.KayTakeWater, KayTakeWater },
            { KitchenAnimations.KayTakeCrackers, KayTakeCrackers },
        };
    }



    public void AddGoBagKitchenAnimationFromWeb(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        Debug.Log($"added animation from web: {animationName}");

        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning("Empty animation name");
            return;
        }

        animationName = animationName.Trim();

        if (Enum.TryParse(animationName, true, out KitchenAnimations animation) &&
            Enum.IsDefined(typeof(KitchenAnimations), animation))
        {
            animationQueue.Enqueue(animation);
            Debug.Log($"Animation added to queue: {animation}");

            if (!isPlaying)
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
        isPlaying = true;

        while (animationQueue.Count > 0)
        {
            KitchenAnimations nextAnimation = animationQueue.Dequeue();

            if (animationMap.TryGetValue(nextAnimation, out var startAnimation))
            {
                currentAnimationFinished = false;

                startAnimation(() =>
                {
                    Debug.Log("Animation finished: " + nextAnimation);
                    currentAnimationFinished = true;
                });

                yield return new WaitUntil(() => currentAnimationFinished);
            }
            else
            {
                Debug.LogWarning("No function for animation: " + nextAnimation);
            }
        }

        isPlaying = false;
    }
    void Start()
    {


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
                StopCoroutine(KayCoroutine);
                kayAnimator.SetTrigger("Idle");
                DadEnterTheRoom();
                CheckAmnamitionsInEditor(); // for checking in editor
            }
        }
    }


    public void DadEnterTheRoom()
    {
        StartCoroutine(DadEnterRoomCoroutine());
    }

    private void OpenTheFridge()
    {
        if (isFridgeOpen) return; // If the fridge is already open, do nothing
        isFridgeOpen = true;
        fridgeSprite.SetActive(true); // Show the fridge sprite

    }
    private void OpenkitchenCabinet()
    {
        if (kitchenCabinetIsOpen) return; // If the kitchen cabinet is already open, do nothing
        kitchenCabinetIsOpen = true;
        kitchenCabinet.SetActive(true); // Show the kitchenCabinet sprite

    }



    IEnumerator DadEnterRoomCoroutine()
    {
        bool flipX = true; // Set to true to flip the character when moving left
        dadAnimator.SetTrigger("Walk");
        yield return MoveToTarget(dad.transform, new Vector3(-6f, -2f, 0f), 2f, flipX);
        WebGLBridge.GivesReminder(ObjectsHolder.instance.GetKelanParentsID()); // Call the WebGL function to notify that dad gives reminder to kelen
        dadAnimator.SetTrigger("Idle");
        WebGLBridge.SendEvent(Events.GobagReminder.ToString()); // Send the event to WebGL
        yield return new WaitForSeconds(2f);
        KelanTakeBag();
        dadAnimator.SetTrigger("Walk");
        yield return MoveToTarget(dad.transform, new Vector3(-11f, -2f, 0f), 2f, flipX);
        dadAnimator.SetTrigger("Idle");

    }

    public void KitchenOnGivesReminder() //Called from WebGL when dad gives reminder to kelen
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
            kayAnimator.SetTrigger("Idle"); // for checking in editor, you can comment this line when testing in WebGL
                                            // KeyTakeBag();

            AddGoBagKitchenAnimationFromWeb("KayTakeChees");
            AddGoBagKitchenAnimationFromWeb("KayTakeEggs");
            AddGoBagKitchenAnimationFromWeb("KayTakeChicken");
            AddGoBagKitchenAnimationFromWeb("KayTakeFish");
            AddGoBagKitchenAnimationFromWeb("KayTakeCannedFood");
            AddGoBagKitchenAnimationFromWeb("KayTakeCrackers");
            AddGoBagKitchenAnimationFromWeb("KayTakeWater");
        }
    }





    public void Initialize()
    {

    }

    public void OnSimulationStart()
    {
        playAnimation = true;
        KayPlay();
        // simulationStart = true;

    }

    public void OnSimulationEnd()
    {
        Debug.Log("end ChildrenRoom mode");
    }

    public void Cleanup()
    {
        Debug.Log("Cleanup ChildrenRoom mode");
    }



    #region Right side items: Water, Toy, books, kandels


    public void KayTakeFish(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineRightSide(kayObj, new Vector3(4.5f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Fish", onComplete));
    }

    public void KayTakeChicken(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineRightSide(kayObj, new Vector3(4.5f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Chicken", onComplete));
    }
    public void KayTakeChees(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineRightSide(kayObj, new Vector3(4.5f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Chees", onComplete));
    }
    public void KayTakeEggs(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineRightSide(kayObj, new Vector3(4f, -0.5f, 0f), 2f, ToHandsUper, Events.Empty, "Eggs", onComplete));
    }


    IEnumerator KayTakeObjectsCoroutineRightSide(GameObject objToMove, Vector3 targetPosition, float speed,
     int picUpanimation, Events eventTyp, string objectTotake, Action onComplete)
    {

        Transform objectTransform = objToMove.transform;

        kayAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, targetPosition, speed);
        OpenTheFridge();

        kayAnimator.SetTrigger(picUpanimation);

        yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation
        SetRoomObjectActiveByName(objectTotake, true);

        kayAnimator.SetTrigger("WalkWith");

        yield return MoveToTarget(objectTransform, bagPositionRight, speed);
        kayAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        SetRoomObjectActiveByName(objectTotake, false);
        WebGLBridge.SendEvent(eventTyp.ToString());
        kayAnimator.enabled = true;
        kayAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }

    #endregion


    #region left side items: Crackers, water, canned food
    public void KayTakeCrackers(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineLeftSide(kayObj, new Vector3(-0.4f, -0.2f, 0f), 2f, TakeWithOneHand, Events.PackCrackers, "Crackers", onComplete));
    }
    public void KayTakeWater(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineLeftSide(kayObj, new Vector3(-4.5f, -0.5f, 0f), 2f, TakeWithOneHand, Events.PackWater, "Water", onComplete));
    }

    public void KayTakeCannedFood(Action onComplete)
    {
        StartCoroutine(KayTakeObjectsCoroutineLeftSide(kayObj, new Vector3(-0.4f, -0.2f, 0f), 2f, PicUp, Events.PackCannedFood, "CannedFood", onComplete));
    }


    IEnumerator KayTakeObjectsCoroutineLeftSide(GameObject objToMove, Vector3 targetPosition, float speed,
     int picUpanimation, Events eventTyp, string objectTotake, Action onComplete)
    {

        Transform objectTransform = objToMove.transform;

        kayAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, targetPosition, speed);
        if (objectTotake != "Water")
        {
            OpenkitchenCabinet();
        }

        kayAnimator.SetTrigger(picUpanimation);

        yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation
        SetRoomObjectActiveByName(objectTotake, true);

        kayAnimator.SetTrigger("WalkWith");
        yield return MoveToTarget(objectTransform, roomOffset.position, speed);

        yield return MoveToTarget(objectTransform, bagPositionLeft, speed);
        kayAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        SetRoomObjectActiveByName(objectTotake, false);
        WebGLBridge.SendEvent(eventTyp.ToString());
        kayAnimator.enabled = true;
        kayAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }
    #endregion



    public void KelanTakeBag()
    {
        StartCoroutine(KeyWalkToBag(kelanObj, new Vector3(1.33f, -1.33f, 0f), 2f));
    }

    public void KayPlay()
    {
        KayCoroutine = StartCoroutine(StartKelanPlay(kayObj, new Vector3(-4f, -3f, 0f), 2f));
    }


    private IEnumerator StartKelanPlay(GameObject objToMove, Vector3 endPoint, float speed)
    {
        float startScale = objToMove.transform.localScale.x; // Adjust this value to change the size of the sprite
        while (playAnimation == true)
        {
            Vector3 startPoint = objToMove.transform.position;
            objToMove.transform.localScale = new Vector3(startScale, startScale, startScale); // Set the sprite scale
            kayAnimator.SetTrigger("Play");
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


    public IEnumerator KeyWalkToBag(GameObject objToMove, Vector3 endPoint, float speed)
    {
        if (objToMove == null)
        {
            Debug.LogWarning("KeyWalkPlay: objToMove is null.");
            yield break;
        }

        Transform objectTransform = objToMove.transform;

        SetUniformScale(objectTransform);

        kelanAnimator.SetTrigger(WalkHash);
        yield return MoveToTarget(objectTransform, endPoint, speed);

        kelanAnimator.SetTrigger(GetBagHash);
        yield return new WaitForSeconds(pickBagDelay);

        yield return MoveToTarget(bag.transform, kayBagPosition.position, speed);
        bag.transform.SetParent(kayBagPosition);

        kelanAnimator.SetTrigger(WalkHash);
        yield return MoveToTarget(objectTransform, finalPoint, speed);

        kelanAnimator.SetTrigger(IdleHash);
        bag.GetComponent<Animator>().SetTrigger(OpenBagHash);

        yield return new WaitForSeconds(openBagDelay);

    }

    private IEnumerator MoveToTarget(Transform objectTransform, Vector3 targetPoint, float speed, bool flipX = false)
    {
        FlipObjects(objectTransform.gameObject, targetPoint, flipX);
        while (Vector3.Distance(objectTransform.position, targetPoint) > stopDistance)
        {
            objectTransform.position = Vector3.MoveTowards(
                objectTransform.position,
                targetPoint,
                speed * Time.deltaTime);

            yield return null;
        }

        objectTransform.position = targetPoint;
    }

    private void SetUniformScale(Transform objectTransform)
    {
        float scaleX = objectTransform.localScale.x;
        objectTransform.localScale = new Vector3(scaleX, scaleX, scaleX);
    }

    private void SetRoomObjectActiveByName(string objectName, bool activate)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            Debug.LogWarning("SetRoomObjectActiveByName: objectName is empty.");
            return;
        }

        RoomTakesObjects roomTakesObjects = kayObj.GetComponent<RoomTakesObjects>();
        if (roomTakesObjects == null)
        {
            Debug.LogWarning("SetRoomObjectActiveByName: RoomTakesObjects component not found on kelen.");
            return;
        }

        for (int i = 0; i < roomTakesObjects.objectsToActivateDeactivate.Count; i++)
        {
            ActivateDeactivateObject entry = roomTakesObjects.objectsToActivateDeactivate[i];
            if (entry == null)
                continue;

            if (string.Equals(entry.objectName, objectName, StringComparison.OrdinalIgnoreCase))
            {
                if (activate)
                    entry.ActivateObject();
                else
                    entry.DeactivateObject();

                return;
            }
        }

        Debug.LogWarning($"SetRoomObjectActiveByName: object '{objectName}' was not found in objectsToActivateDeactivate.");
    }


    public void FlipObjects(GameObject objToFlip, Vector3 targetPoint, bool flipX = false)
    {
        if (objToFlip == null)
            return;

        // Compare world-space X coordinates (targetPoint is in world space).
        float currentX = objToFlip.transform.position.x;
        bool movingRight = targetPoint.x > currentX;

        // Flip by setting the sign of localScale.x.
        Vector3 ls = objToFlip.transform.localScale;
        if (!flipX)
        {
            ls.x = Mathf.Abs(ls.x) * (movingRight ? -1f : 1f);
        }
        else
        {
            ls.x = Mathf.Abs(ls.x) * (movingRight ? 1f : -1f);
        }

        objToFlip.transform.localScale = ls;
    }
}
