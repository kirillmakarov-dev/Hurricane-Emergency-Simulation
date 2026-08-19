using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GoBagLesson : MonoBehaviour, ISimulationMode
{
    public enum GaBagAnimations
    {
        KelenTakeTshirt,
        KelenTakeFlashlight,
        KelenTakeLamp,
        KelenTakeToy,
        KelenTakeWater,
        KelanTakeBall,
        KelenTakeBooks,
        KelenTakeCandels,
        ColoringBook,
        KelenTakeScissors
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


    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int GetBagHash = Animator.StringToHash("GetBag");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int OpenBagHash = Animator.StringToHash("OpenBag");

    [Header("Settings")]
    [SerializeField] private float stopDistance = 0.01f;
    [SerializeField] private float pickBagDelay = 0.3f;
    [SerializeField] private float openBagDelay = 2f;
    [SerializeField] private Vector3 finalPoint = new Vector3(3f, -2f, 0f);
    private Vector3 bagPositionRight = new Vector3(4f, -2f, 0f); // Position of the bag
    private Vector3 bagPositionLeft = new Vector3(2.5f, -2f, 0f); // Position of the bag

    private Coroutine kelenCoroutine;

    private readonly Queue<GaBagAnimations> animationQueue = new();
    private bool isPlaying;
    private bool currentAnimationFinished;

    private Dictionary<GaBagAnimations, Action<Action>> animationMap;

    public float timer = 0;
    public bool simulationStart = false;



    void Awake()
    {
        dadAnimator = dad.GetComponent<Animator>();
        kelenAnimator = kelen.GetComponent<Animator>();
        keyAnimator = key.GetComponent<Animator>();

        animationMap = new Dictionary<GaBagAnimations, Action<Action>>()
        {
            { GaBagAnimations.KelenTakeTshirt, KelenTakeTshirt },
            { GaBagAnimations.KelenTakeFlashlight, KelenTakeFlashlight },
            { GaBagAnimations.KelenTakeLamp, KelenTakeLamp },
            { GaBagAnimations.KelenTakeToy, KelenTakeToy },
            { GaBagAnimations.KelenTakeWater, KelenTakeWater },
            { GaBagAnimations.KelenTakeBooks, KelenTakeBooks },
            { GaBagAnimations.KelenTakeCandels, KelenTakeKandels },
            { GaBagAnimations.KelanTakeBall, KelanTakeBall },
            { GaBagAnimations. ColoringBook, KelanTakeColoringBook },
            { GaBagAnimations. KelenTakeScissors, KelenTakeScissors }, // To:Do needto add the function in Web
        };
    }



    public void AddGoBagAnimationFromWeb(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        Debug.Log($"added animation from web: {animationName}");

        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning("Empty animation name");
            return;
        }

        animationName = animationName.Trim();

        if (Enum.TryParse(animationName, true, out GaBagAnimations animation) &&
            Enum.IsDefined(typeof(GaBagAnimations), animation))
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
            GaBagAnimations nextAnimation = animationQueue.Dequeue();

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
                StopCoroutine(kelenCoroutine);
                kelenAnimator.SetTrigger("Idle");
                DadEnterTheRoom();
                CheckAmnamitionsInEditor(); // for checking in editor
            }
        }
    }


    public void DadEnterTheRoom()
    {
        StartCoroutine(DadEnterRoomCoroutine());
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
        KeyTakeBag();
        dadAnimator.SetTrigger("Walk");
        yield return MoveToTarget(dad.transform, new Vector3(-11f, -2f, 0f), 2f, flipX);
        dadAnimator.SetTrigger("Idle");

    }

    public void OnGivesReminder() //Called from WebGL when dad gives reminder to kelen
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
            kelenAnimator.SetTrigger("Idle"); // for checking in editor, you can comment this line when testing in WebGL

            AddGoBagAnimationFromWeb("KelenTakeWater");
            AddGoBagAnimationFromWeb("KelenTakeToy");
            AddGoBagAnimationFromWeb("KelenTakeBooks");
            AddGoBagAnimationFromWeb("KelenTakeCandels");

            // AddGoBagAnimationFromWeb("KelanTakeBall");
            // AddGoBagAnimationFromWeb("KelenTakeLamp");
            // AddGoBagAnimationFromWeb("ColoringBook");
            // AddGoBagAnimationFromWeb("KelenTakeScissors");
            // AddGoBagAnimationFromWeb("KelenTakeTshirt");
            // AddGoBagAnimationFromWeb("KelenTakeFlashlight");
        }
    }


    public void Initialize()
    {

    }

    public void OnSimulationStart()
    {
        playAnimation = true;
        KelanPlay();
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
    public void KelenTakeWater(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(6.5f, -1f, 0f), 2f, "TakeWithOneHand", Events.PackWater, "Water", onComplete));
    }
    public void KelenTakeToy(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(5f, -1f, 0f), 2f, "PicUp", Events.PackToys, "Toy", onComplete));
    }
    public void KelenTakeBooks(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(7f, -1f, 0f), 2f, "PicUp", Events.PackBook, "Books", onComplete));
    }
    public void KelenTakeKandels(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineRightSide(kelen, new Vector3(7f, -1f, 0f), 2f, "ToHandsUper", Events.Empty, "Candels", onComplete));
    }


    IEnumerator KelanTakeObjectsCoroutineRightSide(GameObject objToMove, Vector3 targetPosition, float speed,
     string picUpanimation, Events eventTyp, string objectTotake, Action onComplete)
    {

        Transform objectTransform = objToMove.transform;

        kelenAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, targetPosition, speed);

        kelenAnimator.SetTrigger(picUpanimation);

        yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation
        SetRoomObjectActiveByName(objectTotake, true);

        kelenAnimator.SetTrigger("WalkWith");

        yield return MoveToTarget(objectTransform, bagPositionRight, speed);
        kelenAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        SetRoomObjectActiveByName(objectTotake, false);
        WebGLBridge.SendEvent(eventTyp.ToString());
        kelenAnimator.enabled = true;
        kelenAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }

    #endregion


    #region left side items: ball, lamp, flashlight, tshirt coloringBook, paint
    public void KelanTakeBall(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(kelen, new Vector3(1f, -2.5f, 0f), 2f, "PicUp", Events.Empty, "Ball", onComplete));
    }
    public void KelenTakeLamp(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(kelen, new Vector3(-1.5f, -0.6f, 0f), 2f, "ToHandsUper", Events.Empty, "Lamp", onComplete));
    }
    public void KelanTakeColoringBook(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(kelen, new Vector3(-0.5f, -0.6f, 0f), 2f, "ToHandsUper", Events.Empty, "ColoringBook", onComplete));
    }
    public void KelenTakeScissors(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(kelen, new Vector3(-11f, -2f, 0f), 2f, "ToHandsUper", Events.Empty, "Scissors", onComplete));
    }
    public void KelenTakeTshirt(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(kelen, new Vector3(-6f, -0.7f, 0f), 2f, "TakeWithToHands", Events.PackClothes, "Tshirt", onComplete));
    }
    public void KelenTakeFlashlight(Action onComplete)
    {
        StartCoroutine(KelanTakeObjectsCoroutineLeftSide(kelen, new Vector3(-6f, -2f, 0f), 2f, "PicUp", Events.PackFlashlight, "FlashLight", onComplete));
    }





    IEnumerator KelanTakeObjectsCoroutineLeftSide(GameObject objToMove, Vector3 targetPosition, float speed,
     string picUpanimation, Events eventTyp, string objectTotake, Action onComplete)
    {

        Transform objectTransform = objToMove.transform;

        kelenAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, targetPosition, speed);

        kelenAnimator.SetTrigger(picUpanimation);

        yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation
        SetRoomObjectActiveByName(objectTotake, true);

        kelenAnimator.SetTrigger("WalkWith");
        yield return MoveToTarget(objectTransform, roomOffset.position, speed);

        yield return MoveToTarget(objectTransform, bagPositionLeft, speed);
        kelenAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        SetRoomObjectActiveByName(objectTotake, false);
        WebGLBridge.SendEvent(eventTyp.ToString());
        kelenAnimator.enabled = true;
        kelenAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }
    #endregion



    public void KeyTakeBag()
    {
        StartCoroutine(KeyWalkToBag(key, new Vector3(1.33f, -1.33f, 0f), 2f));
    }

    public void KelanPlay()
    {
        kelenCoroutine = StartCoroutine(StartKelanPlay(kelen, new Vector3(-4f, -3f, 0f), 2f));
    }


    private IEnumerator StartKelanPlay(GameObject objToMove, Vector3 endPoint, float speed)
    {
        float startScale = objToMove.transform.localScale.x; // Adjust this value to change the size of the sprite
        while (playAnimation == true)
        {
            Vector3 startPoint = objToMove.transform.position;
            objToMove.transform.localScale = new Vector3(startScale, startScale, startScale); // Set the sprite scale
            kelenAnimator.SetTrigger("Play");
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

        keyAnimator.SetTrigger(WalkHash);
        yield return MoveToTarget(objectTransform, endPoint, speed);

        keyAnimator.SetTrigger(GetBagHash);
        yield return new WaitForSeconds(pickBagDelay);

        yield return MoveToTarget(bag.transform, kayBagPosition.position, speed);
        bag.transform.SetParent(kayBagPosition);

        keyAnimator.SetTrigger(WalkHash);
        yield return MoveToTarget(objectTransform, finalPoint, speed);

        keyAnimator.SetTrigger(IdleHash);
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

        RoomTakesObjects roomTakesObjects = kelen.GetComponent<RoomTakesObjects>();
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
