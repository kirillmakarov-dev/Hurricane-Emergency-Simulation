using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Animations
{
    KelenTakeTshirt,
    KelenTakeFruits,
    KelenTakeFlashlight,
    KelenTakeLamp,
    KelenTakeToy,
    KelenTakeAquarium,
    KelenTakeWater,
    KelenTakeScissors,
    KelenTakeChicken,
    kelanGoforWalk,
    kelanTakeBall,
    kelanTaketoys,
    keyTakesBicycle,
    keyPickFlowers,
    HurricaneWatchAnnouncement
}

public class ChildrenRoomMode : MonoBehaviour, ISimulationMode
{
    [SerializeField] private GameObject kelen;
    [SerializeField] private GameObject key;
    [SerializeField] private GameObject kelenMom;
    [SerializeField] private GameObject bag;
    [SerializeField] private Transform kayBagPosition;

    [SerializeField] Transform roomOffset;
    [SerializeField] Transform roomToyOffset;

    private Animator kelenAnimator;
    private Animator keyAnimator;

    private Animator momAnimator;
    public bool checkanimation;
    public Animations animationToPlay; // Variable to specify which animation to play

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

    public bool hurricaneWatch = false;
    [SerializeField] private GameObject hurricaneWatchAnnouncement;

    private Coroutine kelenCoroutine;
    private float kelenStartScale;


    private readonly Queue<Animations> animationQueue = new();
    private bool isPlaying;
    private bool currentAnimationFinished;

    private Dictionary<Animations, Action<Action>> animationMap;

    public float hurricaneWatchTimer = 0;
    public bool timerRun = false;



    void Awake()
    {
        momAnimator = kelenMom.GetComponent<Animator>();

        kelenAnimator = kelen.GetComponent<Animator>();
        keyAnimator = key.GetComponent<Animator>();
        kelenStartScale = kelen.transform.localScale.x;

        animationMap = new Dictionary<Animations, Action<Action>>()
        {
            { Animations.KelenTakeTshirt, KelenTakeTshirt },
            { Animations.KelenTakeFruits, KelenTakeFruits },
            { Animations.KelenTakeFlashlight, KelenTakeFlashlight },
            { Animations.KelenTakeLamp, KelenTakeLamp },
            { Animations.KelenTakeToy, KelenTakeToy },
            { Animations.KelenTakeAquarium, KelenTakeAquarium },
            { Animations.KelenTakeWater, KelenTakeWater },
            { Animations.KelenTakeScissors, KelenTakeScissors },
            { Animations.KelenTakeChicken, KelenTakeChicken },
            { Animations.HurricaneWatchAnnouncement, HurricaneWatchAnnouncement }
        };
    }



    public void AddAnimationFromWeb(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        Debug.Log($"added animation from web: {animationName}");

        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning("Empty animation name");
            return;
        }

        animationName = animationName.Trim();

        if (Enum.TryParse(animationName, true, out Animations animation) &&
            Enum.IsDefined(typeof(Animations), animation))
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
            Animations nextAnimation = animationQueue.Dequeue();

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
        playAnimation = true;
        KelanPlay();
    }

    // Update is called once per frame
    void Update()
    {
        if (timerRun == true)
        {
            if (hurricaneWatchTimer <= 0f)
            {
                SendHurricaneWatch();
                timerRun = false;
            }
            else if (hurricaneWatchTimer > 0f)
            {
                hurricaneWatchTimer -= Time.deltaTime;
            }
        }




        if (hurricaneWatch == true)
        {
            // StartCoroutine(HurricaneWatch()); // check in editor 
            AddAnimationFromWeb(Animations.HurricaneWatchAnnouncement.ToString()); // add to queue
            hurricaneWatch = false; // Reset the flag after handling the announcement
        }

        if (checkanimation == true) // This flag can be set from the web interface to trigger the animation check
        {
            AddAnimationFromWeb(animationToPlay.ToString());
            // RoomAnimations(animationToPlay);
            checkanimation = false;
        }

    }

    public void HurricaneWatchAnnouncement(Action onComplete)
    {
        StartCoroutine(HurricaneWatch(onComplete));
    }

    public void SendHurricaneWatch()
    {
        WebGLBridge.HurricaneWatchOnAnnounced(ObjectsHolder.instance.GetHurricaneWatchID()); // Call the WebGL method to notify that the announcement has been played
    }



    public void Initialize()
    {
        hurricaneWatchAnnouncement.SetActive(false);
    }

    public void OnSimulationStart()
    {
        timerRun = true;
    }

    public void OnSimulationEnd()
    {
        Debug.Log("end ChildrenRoom mode");
    }

    public void Cleanup()
    {
        Debug.Log("Cleanup ChildrenRoom mode");
    }

    private void RoomAnimations(Animations animationToPlay)
    {
        switch (animationToPlay)
        {
            case Animations.KelenTakeTshirt:
                KelenTakeTshirt(() => { });
                break;
            case Animations.KelenTakeFruits:
                KelenTakeFruits(() => { });
                break;
            case Animations.KelenTakeFlashlight:
                KelenTakeFlashlight(() => { });
                break;
            case Animations.KelenTakeLamp:
                KelenTakeLamp(() => { });
                break;
            case Animations.KelenTakeToy:
                KelenTakeToy(() => { });
                break;
            case Animations.KelenTakeAquarium:
                KelenTakeAquarium(() => { });
                break;
            case Animations.KelenTakeWater:
                KelenTakeWater(() => { });
                break;
            case Animations.KelenTakeScissors:
                KelenTakeScissors(() => { });
                break;
            case Animations.KelenTakeChicken:
                KelenTakeChicken(() => { });
                break;
            default:
                Debug.LogWarning("Unknown animation type: " + animationToPlay);
                break;
        }
    }

    public void KelenTakeScissors(Action onComplete)
    {
        StartCoroutine(KelenTakeObjectsFromKitchenCoroutine(kelen, new Vector3(-12f, -3f, 0f), Animations.KelenTakeScissors, 2f, onComplete));
    }
    public void KelenTakeChicken(Action onComplete)
    {
        StartCoroutine(KelenTakeObjectsFromKitchenCoroutine(kelen, new Vector3(-12f, -3f, 0f), Animations.KelenTakeChicken, 2f, onComplete));
    }

    IEnumerator KelenTakeObjectsFromKitchenCoroutine(GameObject objToMove, Vector3 targetPosition, Animations animationToPlay, float speed, Action onComplete)
    {
        GameObject heandGameObject = animationToPlay == Animations.KelenTakeScissors ?
        kelen.GetComponent<RoomTakesObjects>().scissorsHeand :
        kelen.GetComponent<RoomTakesObjects>().chickensHeand;


        // Adjust this value to change the size of the sprite
        Vector3 bagPosition = new Vector3(4f, -2f, 0f); // Position of the bag
        Transform objectTransform = objToMove.transform;

        if (targetPosition.x < 0)
        {
            FlipKelen(false);
        }

        kelenAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, targetPosition, speed);
        heandGameObject.SetActive(true);

        FlipKelen(true);
        kelenAnimator.SetTrigger("WalkWith");

        yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, bagPosition, speed);
        FlipKelen(false);
        kelenAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        yield return new WaitForSeconds(0.5f);
        heandGameObject.SetActive(false); // Call the method to handle the object being taken
        kelenAnimator.enabled = true;
        kelenAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }

    public void KelenTakeWater(Action onComplete)
    {
        StartCoroutine(KelenTakeItemViaRoomOffsetCoroutine(
            kelen, new Vector3(-5.5f, -1.5f, 0f), 2f,
            "Water", kelen.GetComponent<RoomTakesObjects>().waterHeand, onComplete, Events.PackWater));
        // WebGLBridge.SendEvent(Events.PackWater.ToString());
    }

    public void KelenTakeAquarium(Action onComplete)
    {
        StartCoroutine(KelenTakeItemViaToyOffsetCoroutine(
            kelen, new Vector3(4.7f, -0.80f, 0f), 2f,
            "Aquarium", kelen.GetComponent<RoomTakesObjects>().aquriumHeand, onComplete));
    }

    public void KelenTakeToy(Action onComplete)
    {
        StartCoroutine(KelenTakeItemViaToyOffsetCoroutine(
            kelen, new Vector3(4f, -1f, 0f), 2f,
            "Toy", kelen.GetComponent<RoomTakesObjects>().toyHeand, onComplete, Events.PackToys));
        // WebGLBridge.SendEvent(Events.PackToys.ToString());
    }

    public void KelenTakeLamp(Action onComplete)
    {
        StartCoroutine(KelenTakeItemViaRoomOffsetCoroutine(
            kelen, new Vector3(-5.16f, -1.18f, 0f), 2f,
            "Lamp", kelen.GetComponent<RoomTakesObjects>().lampHeand, onComplete));
    }

    private void KelenTakeFlashlight(Action onComplete)
    {
        StartCoroutine(KelenTakeItemViaRoomOffsetCoroutine(
            kelen, new Vector3(-6f, -2f, 0f), 2f,
            "FlashLight", kelen.GetComponent<RoomTakesObjects>().flashLightHeand, onComplete, Events.PackFlashlight));
        //WebGLBridge.SendEvent(Events.PackFlashlight.ToString());
    }

    private void KelenTakeTshirt(Action onComplete)
    {
        StartCoroutine(KelenTakeTshirtCoroutine(kelen, new Vector3(7.4f, -1.75f, 0f), 2f, onComplete));
    }

    IEnumerator KelenTakeTshirtCoroutine(GameObject objToMove, Vector3 targetPosition, float speed, Action onComplete)
    {
        // Adjust this value to change the size of the sprite
        Vector3 bagPosition = new Vector3(4f, -2f, 0f); // Position of the bag
        Transform objectTransform = objToMove.transform;

        if (targetPosition.x > 0)
        {
            FlipKelen(true);
        }

        kelenAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, targetPosition, speed);

        kelenAnimator.SetTrigger("TShirt");

        yield return new WaitForSeconds(1f); // Wait for a moment before starting the take animation

        FlipKelen(false);
        kelenAnimator.SetTrigger("WalkWith");


        yield return MoveToTarget(objectTransform, bagPosition, speed);
        kelenAnimator.enabled = false; // Disable the animator to stop any ongoing animations
        kelen.GetComponent<RoomTakesObjects>().tShirtHeand.SetActive(false); // Call the method to handle the object being taken
        WebGLBridge.SendEvent(Events.PackClothes.ToString());
        kelenAnimator.enabled = true;
        kelenAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f); // Wait for the animation to complete (adjust time as needed)
        onComplete?.Invoke();
    }
    private void KelenTakeFruits(Action onComplete)
    {
        StartCoroutine(KelenTakeItemViaRoomOffsetCoroutine(
            kelen, new Vector3(-4.4f, -1.5f, 0f), 2f,
            "Fruits", kelen.GetComponent<RoomTakesObjects>().fruitsHeand, onComplete));
    }

    public void FlipKelen(bool left)
    {
        if (left)
        {
            kelen.transform.localScale = new Vector3(-kelenStartScale, kelenStartScale, kelenStartScale);
        }
        else
        {
            kelen.transform.localScale = new Vector3(kelenStartScale, kelenStartScale, kelenStartScale);
        }
    }


    public void KeyWalk()
    {
        StartCoroutine(KeyWalkPlay(key, new Vector3(1.33f, -1.33f, 0f), 2f));
    }

    public void KelanPlay()
    {
        kelenCoroutine = StartCoroutine(StartKelanPlay(kelen, new Vector3(-4f, -3f, 0f), 2f));
    }

    IEnumerator HurricaneWatch(Action onComplete = null)
    {
        momAnimator.SetTrigger("Walking");

        Vector3 startPosition = kelenMom.transform.position;
        Vector3 momPosition = new Vector3(startPosition.x + 5, startPosition.y, startPosition.z);
        StartCoroutine(MoveToTarget(kelenMom.transform, momPosition, 2f)); // MoveToTarget

        yield return new WaitForSeconds(2.2f);
        momAnimator.SetTrigger("Standing");


        hurricaneWatchAnnouncement.SetActive(true);
        WebGLBridge.SendEvent(Events.HurricaneWatch.ToString());
        yield return new WaitForSeconds(3f); // Wait for 3 seconds before starting the animation
        hurricaneWatchAnnouncement.SetActive(false);
        StopCoroutine(kelenCoroutine);
        kelenAnimator.SetTrigger("Idle");
        playAnimation = false;
        KeyWalk();

        kelenMom.transform.localScale = new Vector3(-kelenMom.transform.localScale.x,
        kelenMom.transform.localScale.y, kelenMom.transform.localScale.z); // Flip the mom to face left
        momAnimator.SetTrigger("Walking");

        yield return StartCoroutine(MoveToTarget(kelenMom.transform, startPosition, 3f)); // MoveToTarget
        momAnimator.SetTrigger("Standing");
        onComplete?.Invoke();
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


    public IEnumerator KeyWalkPlay(GameObject objToMove, Vector3 endPoint, float speed)
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

    private IEnumerator MoveToTarget(Transform objectTransform, Vector3 targetPoint, float speed)
    {
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

    // Shared coroutine for items on the LEFT side picked up via roomOffset waypoint.
    // Used by: Water, Lamp, Flashlight, Fruits.
    private IEnumerator KelenTakeItemViaRoomOffsetCoroutine(
        GameObject objToMove, Vector3 targetPosition, float speed,
        string pickupTrigger, GameObject handObject, Action onComplete, Events eventname = Events.Empty)
    {
        Vector3 bagPosition = new Vector3(4f, -2f, 0f);
        Transform objectTransform = objToMove.transform;

        if (targetPosition.x < 0)
            FlipKelen(false);

        kelenAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, targetPosition, speed);

        kelenAnimator.SetTrigger(pickupTrigger);
        yield return new WaitForSeconds(1f);

        FlipKelen(true);
        kelenAnimator.SetTrigger("WalkWith");



        yield return MoveToTarget(objectTransform, roomOffset.position, speed);
        yield return MoveToTarget(objectTransform, bagPosition, speed);
        FlipKelen(false);
        kelenAnimator.enabled = false;
        yield return new WaitForSeconds(0.5f);
        handObject.SetActive(false);
        if (eventname != Events.Empty)
        {
            WebGLBridge.SendEvent(eventname.ToString());
        }
        kelenAnimator.enabled = true;
        kelenAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f);
        onComplete?.Invoke();
    }

    // Shared coroutine for items on the RIGHT side picked up via roomToyOffset waypoint.
    // Used by: Aquarium, Toy.
    // Key differences from RoomOffset variant: no waypoint on return, no 0.5f animator-disable wait.
    private IEnumerator KelenTakeItemViaToyOffsetCoroutine(
        GameObject objToMove, Vector3 targetPosition, float speed,
        string pickupTrigger, GameObject handObject, Action onComplete, Events eventname = Events.Empty)
    {
        Vector3 bagPosition = new Vector3(4f, -2f, 0f);
        Transform objectTransform = objToMove.transform;

        if (targetPosition.x > 0)
            FlipKelen(true);

        kelenAnimator.SetTrigger("Walk");
        yield return MoveToTarget(objectTransform, roomToyOffset.position, speed);
        yield return MoveToTarget(objectTransform, targetPosition, speed);

        kelenAnimator.SetTrigger(pickupTrigger);
        yield return new WaitForSeconds(1f);

        FlipKelen(false);
        kelenAnimator.SetTrigger("WalkWith");

        yield return MoveToTarget(objectTransform, bagPosition, speed);
        kelenAnimator.enabled = false;
        handObject.SetActive(false);
        if (eventname != Events.Empty)
        {
            WebGLBridge.SendEvent(eventname.ToString());
        }
        kelenAnimator.enabled = true;
        kelenAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f);
        onComplete?.Invoke();
    }



}
