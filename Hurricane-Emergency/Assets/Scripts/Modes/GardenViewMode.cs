using System.Collections;
using System;
using UnityEngine;
using System.Collections.Generic;

public class GardenViewMode : MonoBehaviour, ISimulationMode
{

    private sealed class AnimationQueueState
    {
        public AnimationQueueState(string playerName, Dictionary<Animations, Action<Action>> animationMap)
        {
            PlayerName = playerName;
            AnimationMap = animationMap;
        }

        public string PlayerName { get; }
        public Dictionary<Animations, Action<Action>> AnimationMap { get; }
        public Queue<Animations> Queue { get; } = new();
        public bool IsPlaying { get; set; }
    }

    public static GardenViewMode Instance { get; private set; }
    public bool checkanimation = false;

    public Animator radioAnimator;
    public GameObject momMain;
    public GameObject momWateringAnimator;
    [SerializeField] GameObject waterParticles;
    public Animator kelanAnimator;
    public Animator keyAnimator;
    public GameObject kelanCanvas;

    private Dictionary<Animations, Action<Action>> kelanAnimationMap;
    private Dictionary<Animations, Action<Action>> keyAnimationMap;
    private AnimationQueueState kelanQueueState;
    private AnimationQueueState keyQueueState;
    private Coroutine configuredSequenceCoroutine;
    public Animations animationToPlay; // Variable to specify which animation to play

    public Action onKyelanAnimationComplete; // Event to signal when an animation is complete

    public Action onKeyAnimationComplete; // Event to signal when an animation is complete

    public float hurricaneWarningTimer = 0;
    public bool timerRun = false;
    [SerializeField] private GameObject hurricaneWarning;


    public void Cleanup()
    {
        configuredSequenceCoroutine = null;
        StopAllCoroutines();
        if (kelanQueueState != null)
        {
            kelanQueueState.Queue.Clear();
            kelanQueueState.IsPlaying = false;
        }
        if (keyQueueState != null)
        {
            keyQueueState.Queue.Clear();
            keyQueueState.IsPlaying = false;
        }

        timerRun = false;
        hurricaneWarningTimer = 0f;
        checkanimation = false;
        if (momMain != null) momMain.SetActive(false);
        if (momWateringAnimator != null) momWateringAnimator.SetActive(false);
        if (kelanAnimator != null) kelanAnimator.gameObject.SetActive(false);
        if (kelanCanvas != null) kelanCanvas.SetActive(false);
        if (keyAnimator != null) keyAnimator.gameObject.SetActive(false);
        if (hurricaneWarning != null) hurricaneWarning.SetActive(false);
        WaterParticlesEnabled(false);
    }

    public void Initialize()
    {
        // momMain.SetActive(false);
    }

    public void OnSimulationEnd()
    {
        timerRun = false;
        hurricaneWarningTimer = 0f;
    }

    public void OnSimulationStart()
    {
        timerRun = true;
        momMain.SetActive(false);
        momWateringAnimator.SetActive(false);
        kelanAnimator.gameObject.SetActive(false);
        kelanCanvas.SetActive(false);
        keyAnimator.gameObject.SetActive(false);
    }

    public void PlayConfiguredSequence(IReadOnlyList<string> animationNames)
    {
        if (animationNames == null || animationNames.Count == 0)
        {
            Debug.LogWarning("PlayConfiguredSequence requires at least one garden view animation.");
            return;
        }

        StopAllCoroutines();
        if (kelanQueueState != null)
        {
            kelanQueueState.Queue.Clear();
            kelanQueueState.IsPlaying = false;
        }
        if (keyQueueState != null)
        {
            keyQueueState.Queue.Clear();
            keyQueueState.IsPlaying = false;
        }

        timerRun = false;
        hurricaneWarningTimer = 0f;
        checkanimation = false;

        configuredSequenceCoroutine = StartCoroutine(PlayConfiguredSequenceCoroutine(animationNames));
    }

    private IEnumerator PlayConfiguredSequenceCoroutine(IReadOnlyList<string> animationNames)
    {
        yield return PlayConfiguredAnimation(Animations.HurricaneWatchAnnouncement);

        for (int i = 0; i < animationNames.Count; i++)
        {
            if (!Enum.TryParse(animationNames[i], true, out Animations animation) ||
                !Enum.IsDefined(typeof(Animations), animation))
            {
                Debug.LogWarning($"Unknown garden view animation: {animationNames[i]}");
                continue;
            }

            if (animation == Animations.HurricaneWatchAnnouncement)
            {
                continue;
            }

            yield return PlayConfiguredAnimation(animation);
        }

        configuredSequenceCoroutine = null;
    }

    private IEnumerator PlayConfiguredAnimation(Animations animation)
    {
        bool finished = false;
        switch (animation)
        {
            case Animations.HurricaneWatchAnnouncement:
                HurricaneWarningAnnouncement();
                yield return new WaitForSeconds(3.1f);
                yield break;
            case Animations.kelanTakeBall:
                KelanTakesBall(() => finished = true);
                break;
            case Animations.kelanGoforWalk:
                KelanGoForWalk(() => finished = true);
                break;
            case Animations.kelanTaketoys:
                KelanTakesToys(() => finished = true);
                break;
            case Animations.keyPickFlowers:
                KeyPickFlowers(() => finished = true);
                break;
            case Animations.keyTakesBicycle:
                KeyTakesBicycle(() => finished = true);
                break;
            default:
                Debug.LogWarning("Unsupported configured garden animation: " + animation);
                yield break;
        }

        yield return new WaitUntil(() => finished);
    }



    private void RoomAnimations(Animations animationToPlay)
    {
        switch (animationToPlay)
        {
            case Animations.kelanTakeBall:
                KelanTakesBall(() => { });
                break;
            case Animations.kelanGoforWalk:
                KelanGoForWalk(() => { });
                break;
            case Animations.kelanTaketoys:
                KelanTakesToys(() => { });
                break;

            case Animations.keyPickFlowers:
                KeyPickFlowers(() => { });
                break;
            case Animations.keyTakesBicycle:
                KeyTakesBicycle(() => { });
                break;
            default:
                Debug.LogWarning("Unknown animation type: " + animationToPlay);
                break;
        }
    }

    public void AddKelanAnimationFromWeb(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        EnqueueAnimationFromWeb(animationName, kelanQueueState);
    }


    public void AddKeyAnimationFromWeb(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        EnqueueAnimationFromWeb(animationName, keyQueueState);
    }

    private void EnqueueAnimationFromWeb(string animationName, AnimationQueueState queueState)
    {
        Debug.Log($"added animation from web for {queueState.PlayerName}: {animationName}");

        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning($"Empty animation name for {queueState.PlayerName}");
            return;
        }

        animationName = animationName.Trim();

        if (!Enum.TryParse(animationName, true, out Animations animation) ||
            !Enum.IsDefined(typeof(Animations), animation))
        {
            Debug.LogWarning($"No such animation in enum Animations: {animationName}");
            return;
        }

        if (!queueState.AnimationMap.ContainsKey(animation))
        {
            Debug.LogWarning($"Animation {animation} is not supported for {queueState.PlayerName}");
            return;
        }

        queueState.Queue.Enqueue(animation);
        Debug.Log($"Animation added to {queueState.PlayerName} queue: {animation}");

        if (!queueState.IsPlaying)
        {
            StartCoroutine(ProcessQueue(queueState));
        }
    }

    private IEnumerator ProcessQueue(AnimationQueueState queueState)
    {
        queueState.IsPlaying = true;

        while (queueState.Queue.Count > 0)
        {
            Animations nextAnimation = queueState.Queue.Dequeue();

            if (queueState.AnimationMap.TryGetValue(nextAnimation, out var startAnimation))
            {
                bool animationFinished = false;

                startAnimation(() =>
                {
                    Debug.Log($"{queueState.PlayerName} animation finished: {nextAnimation}");
                    animationFinished = true;
                });

                yield return new WaitUntil(() => animationFinished);
            }
            else
            {
                Debug.LogWarning($"No function for {queueState.PlayerName} animation: {nextAnimation}");
            }
        }

        queueState.IsPlaying = false;
    }




    void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        kelanAnimationMap = new Dictionary<Animations, Action<Action>>()
        {
            { Animations.kelanTakeBall, KelanTakesBall },
            { Animations.kelanGoforWalk, KelanGoForWalk },
            { Animations.kelanTaketoys, KelanTakesToys },
        };

        keyAnimationMap = new Dictionary<Animations, Action<Action>>()
        {
            { Animations.keyPickFlowers, KeyPickFlowers },
            { Animations.keyTakesBicycle, KeyTakesBicycle },
        };

        kelanQueueState = new AnimationQueueState("Kelan", kelanAnimationMap);
        keyQueueState = new AnimationQueueState("Key", keyAnimationMap);
    }

    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {

        if (timerRun == true)
        {
            if (hurricaneWarningTimer <= 0)
            {
                SendHurricaneWarning();
                timerRun = false;
            }
            else if (hurricaneWarningTimer > 0)
            {
                hurricaneWarningTimer -= Time.deltaTime;
            }
        }

        if (checkanimation == true) // This flag can be set from the web interface to trigger the animation check
        {
            RoomAnimations(animationToPlay);
            checkanimation = false;
        }

    }

    public void SendHurricaneWarning() //called from WebGL
    {
        RadioOnAnnouncement();
        WebGLBridge.HurricaneWarningOnAnnounced(ObjectsHolder.instance.GetHurricaneWarningID()); // Call the WebGL method to notify that the announcement has been played

    }


    public void HurricaneWarningAnnouncement() //called from WebGL
    {
        StartCoroutine(HurricaneWarning());
    }

    IEnumerator HurricaneWarning()
    {
        hurricaneWarning.SetActive(true);
        WebGLBridge.SendEvent(Events.HurricaneWarning.ToString());
        yield return new WaitForSeconds(3f); // Wait for 3 seconds before starting the animation
        hurricaneWarning.SetActive(false);
    }





    public void RadioOnAnnouncement() //called from WebGL
    {
        Debug.Log("Radio Announcement Playing");
        radioAnimator.SetTrigger("Announcement");
    }
    #region Mom Animations

    public void OnCoversWindow() //called from WebGL
    {

        Vector3 startPos = new Vector3(-7f, 0.8f, 0);

        momMain.transform.position = startPos;
        momMain.SetActive(true);
        Animator momAnimator = momMain.GetComponent<Animator>();
        momAnimator.SetTrigger("WalkHammer");

        StartCoroutine(MoveToPoint(momMain, new Vector3(-3.73f, 0.2f, 0), 2f, "TakePlanks"));

        Vector3 planksPos = new Vector3(-3.73f, 0.2f, 0);
    }

    public void OnWaterGarden() //called from WebGL
    {
        StartCoroutine(WateringFlowers());
    }

    IEnumerator WateringFlowers()
    {
        // while (!startAnimations)
        // {
        //     yield return null; // Wait until the simulation starts
        // }
        momWateringAnimator.transform.position = new Vector3(-6f, 2f, 0f);
        momWateringAnimator.SetActive(true);
        momWateringAnimator.GetComponent<Animator>().SetTrigger("Walking");

        StartCoroutine(MoveToPoint(momWateringAnimator, new Vector3(-3f, -2f, 0f), 2f, "Watering"));

        WaterParticlesEnabled(true);

        yield return new WaitForSeconds(5f);
        WaterParticlesEnabled(false);
        momWateringAnimator.GetComponent<Animator>().SetTrigger("Walking");
        WaterParticlesEnabled(true);
        StartCoroutine(MoveToPoint(momWateringAnimator, new Vector3(1.7f, -2f, 0f), 2f, "Watering"));

        yield return new WaitForSeconds(8f);
        momWateringAnimator.GetComponent<Animator>().SetTrigger("Standing");

    }

    public void WaterParticlesEnabled(bool value)
    {
        if (waterParticles != null)
        {
            waterParticles.SetActive(value);
        }
    }


    #endregion

    #region kelanAnimations

    public void KelanGoForWalk(Action onComplete) //called from WebGL
    {
        StartCoroutine(KelanGoForWalkCoroutine(onComplete));
    }

    private IEnumerator KelanGoForWalkCoroutine(Action onComplete)
    {
        Vector3 startPos = new Vector3(-7f, 0.8f, 0);
        FlipObject(kelanAnimator.gameObject, true);

        kelanAnimator.transform.position = startPos;
        kelanAnimator.gameObject.SetActive(true);
        kelanCanvas.SetActive(true);
        kelanAnimator.SetTrigger("Walk");

        yield return MoveToPoint(kelanAnimator.gameObject, new Vector3(-6f, -3f, 0), 2f, "");

        yield return MoveToPoint(kelanAnimator.gameObject, new Vector3(12f, -3f, 0), 2f, "");
        kelanAnimator.gameObject.SetActive(false);
        kelanCanvas.SetActive(false);
        onComplete?.Invoke();
    }
    public void FlipObject(GameObject objToflip, bool left)
    {
        if (left)
        {
            objToflip.transform.localScale = new Vector3(objToflip.transform.localScale.x * -1, objToflip.transform.localScale.y, objToflip.transform.localScale.z);
        }
        else
        {
            objToflip.transform.localScale = new Vector3(Mathf.Abs(objToflip.transform.localScale.x), objToflip.transform.localScale.y, objToflip.transform.localScale.z);
        }
    }



    public void KelanTakesToys(Action onComplete) //called from WebGL
    {
        StartCoroutine(KelanTakesSceneObjectsCoroutine(new Vector3(1.5f, -1.5f, 0), "TakeToys", onComplete));
    }


    IEnumerator KelanTakesSceneObjectsCoroutine(Vector3 targetPosition, string triggerName, Action onComplete = null)
    {
        Vector3 startPos = new Vector3(-7f, 0.8f, 0);
        FlipObject(kelanAnimator.gameObject, true);

        kelanAnimator.transform.position = startPos;
        kelanAnimator.gameObject.SetActive(true);

        yield return MoveToPoint(kelanAnimator.gameObject, new Vector3(-6f, -1.5f, 0), 2f, "");

        yield return MoveToPoint(kelanAnimator.gameObject, targetPosition, 2f, triggerName);
        yield return new WaitForSeconds(1f);
        FlipObject(kelanAnimator.gameObject, false);
        yield return MoveToPoint(kelanAnimator.gameObject, new Vector3(-6f, -1.5f, 0), 2f, "");
        yield return MoveToPoint(kelanAnimator.gameObject, startPos, 2f, "");
        kelanAnimator.gameObject.SetActive(false);
        onKyelanAnimationComplete?.Invoke();
        onComplete?.Invoke();
    }


    public void KelanTakesBall(Action onComplete) //called from WebGL
    {
        StartCoroutine(KelanTakesSceneObjectsCoroutine(new Vector3(3.8f, -2.5f, 0), "Ball", onComplete));
    }

    #endregion

    #region Key Animations

    public void KeyTakesBicycle(Action onComplete) //called from WebGL
    {
        StartCoroutine(KeyTakesSceneObjectsCoroutine(new Vector3(5f, 0.35f, 0), "TakeBicycle", onComplete));
    }

    public void KeyPickFlowers(Action onComplete)
    {
        StartCoroutine(KeyTakesSceneObjectsCoroutine(new Vector3(6.5f, -1f, 0), "PickFlowers", onComplete));
    }

    IEnumerator KeyTakesSceneObjectsCoroutine(Vector3 targetPosition, string triggerName, Action onComplete = null)
    {
        Vector3 startPos = new Vector3(-7f, 0.8f, 0);
        FlipObject(keyAnimator.gameObject, true);

        keyAnimator.transform.position = startPos;
        keyAnimator.gameObject.SetActive(true);

        yield return MoveToPoint(keyAnimator.gameObject, new Vector3(3f, -2f, 0), 2f, "");

        yield return MoveToPoint(keyAnimator.gameObject, targetPosition, 2f, triggerName);
        yield return new WaitForSeconds(1f);
        FlipObject(keyAnimator.gameObject, false);
        yield return MoveToPoint(keyAnimator.gameObject, new Vector3(3f, -2f, 0), 2f, "");
        yield return MoveToPoint(keyAnimator.gameObject, startPos, 2f, "");
        keyAnimator.gameObject.SetActive(false);
        onKeyAnimationComplete?.Invoke();
        onComplete?.Invoke();
    }

    #endregion




    private IEnumerator MoveToPoint(GameObject objToMove, Vector3 endPoint, float speed, string triggerName = "")
    {
        Vector3 targetPoint = endPoint;
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
        if (!string.IsNullOrEmpty(triggerName))
        {
            objToMove.GetComponent<Animator>().SetTrigger(triggerName);
        }
    }

}
