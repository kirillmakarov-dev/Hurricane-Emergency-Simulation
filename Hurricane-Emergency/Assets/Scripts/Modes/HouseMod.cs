using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HouseAnimations
{
    RadioAnnouncement,
    ReviewEmergencyPlan,
    CheckGoBag,
    PlayRadioSong,
    ParentsPanic,
    WatchTV
}

public class HouseMod : MonoBehaviour, IConfiguredSequenceMode
{
    public Animator firstScineAnim;
    public Animator radioAnimator;
    public Animator calendar;
    public GameObject tvAnimation;

    public float calendarTimer;
    public GameObject emergencyPlanMain;
    public GameObject checkGobag;
    public bool timerRun = false;

    [SerializeField] private bool isWatchingTV = false;
    [SerializeField] private bool isParentsPanic = false;

    [SerializeField] private GameObject parentsPanic;
    [SerializeField] private GameObject parentsSittingDown;

    [SerializeField] private GameObject kayAndKelanPlaying;

    [SerializeField] private GameObject announcementBubble; // Reference to the GameObject containing the first scene objects

    private bool panicAnimationPlayed = false; // Flag to track if the panic animation has been played

    private bool emergencyPlanMainPlayed = false; // Flag to track if the emergency plan main has been played

    public bool canPlayEmergencyPlan = false; // Flag to track if the page animation has been played

    [SerializeField] private GameObject pageAnimation; // Reference to the text canvas GameObject

    private readonly Queue<HouseAnimations> configuredAnimationQueue = new();
    private Coroutine configuredQueueCoroutine;
    private Coroutine emergencyPlanCoroutine;
    private Coroutine checkGoBagCoroutine;
    private bool suppressAutomaticGardenTransition;
    private bool june1OnArriveAnimationCompleted;

    void Update()
    {
        if (timerRun == true)
        {
            if (calendarTimer <= 0f)
            {
                timerRun = false;
                June1onArrivesAnim();
            }
            else if (calendarTimer > 0)
            {
                calendarTimer -= Time.deltaTime;
            }
        }

        if (isWatchingTV)
        {
            isWatchingTV = false;
            WatchTV();
        }

        if (isParentsPanic)
        {
            isParentsPanic = false;
            ParentsPanic();
        }
    }
    public void Cleanup()
    {
        if (configuredQueueCoroutine != null)
        {
            StopCoroutine(configuredQueueCoroutine);
            configuredQueueCoroutine = null;
        }

        StopHouseAnimationCoroutine(ref emergencyPlanCoroutine);
        StopHouseAnimationCoroutine(ref checkGoBagCoroutine);

        configuredAnimationQueue.Clear();
        suppressAutomaticGardenTransition = false;

        ResetHouseAnimationObjects(emergencyPlanMain);
        ResetHouseAnimationObjects(checkGobag);
    }

    public void Initialize()
    {
        //Debug.Log("Initialize house mod");
        //firstScineAnim.SetBool("House", true);
        //calendarTimer = 0;
    }

    public void OnSimulationEnd()
    {
        // throw new System.NotImplementedException();
    }

    public void ContinueCalendarAnimation()
    {
        StartCoroutine(ContinueCalendarAnimationCoroutine());
    }
    IEnumerator ContinueCalendarAnimationCoroutine()
    {
        yield return new WaitForSeconds(4f);
        if (panicAnimationPlayed == false && emergencyPlanMainPlayed == false)
        {
            calendar.enabled = true;
        }
    }

    public void OnSimulationStart()
    {
        Debug.Log("start house mod");
        june1OnArriveAnimationCompleted = false;
        firstScineAnim.SetTrigger("House");
        emergencyPlanMainPlayed = false;
        canPlayEmergencyPlan = false;
        panicAnimationPlayed = false;

        ResetHouseAnimationObjects(emergencyPlanMain);
        ResetHouseAnimationObjects(checkGobag);

        //calendarTimer = 0;
        timerRun = true;
    }

    public void PlayConfiguredSequence(IReadOnlyList<string> animationNames, Action onCompleted = null)
    {
        if (configuredQueueCoroutine != null)
        {
            StopCoroutine(configuredQueueCoroutine);
        }

        configuredAnimationQueue.Clear();
        suppressAutomaticGardenTransition = true;

        for (int i = 0; i < animationNames.Count; i++)
        {
            if (System.Enum.TryParse(animationNames[i], true, out HouseAnimations animation))
            {
                configuredAnimationQueue.Enqueue(animation);
            }
            else
            {
                Debug.LogWarning($"Unknown House lesson command: {animationNames[i]}");
            }
        }

        configuredQueueCoroutine = StartCoroutine(ProcessConfiguredQueue(onCompleted));
    }

    private IEnumerator ProcessConfiguredQueue(Action onCompleted)
    {
        // The House lesson must start only after the calendar has finished
        // playing the June 1 on-arrive animation.
        yield return new WaitUntil(() => june1OnArriveAnimationCompleted);

        while (configuredAnimationQueue.Count > 0)
        {
            HouseAnimations animation = configuredAnimationQueue.Dequeue();
            PlayConfiguredAnimation(animation);
            yield return WaitForConfiguredAnimation(animation);
            WebGLBridge.SendEvent(GetExpectedEvent(animation).ToString());
        }

        configuredQueueCoroutine = null;
        onCompleted?.Invoke();
    }

    private IEnumerator WaitForConfiguredAnimation(HouseAnimations animation)
    {
        switch (animation)
        {
            case HouseAnimations.RadioAnnouncement:
                yield return new WaitForSeconds(3.1f);
                break;
            case HouseAnimations.ReviewEmergencyPlan:
            {
                float timeout = 20f;
                while (!emergencyPlanMain.activeSelf && timeout > 0f)
                {
                    timeout -= Time.deltaTime;
                    yield return null;
                }

                // Emergency plan transition (3.1 s) switches to the separate
                // "Looking at emergency plan" animation (2.58 s).
                yield return new WaitForSeconds(6f);
                break;
            }
            case HouseAnimations.CheckGoBag:
                // The Go Bag transition (3.07 s) switches to the separate
                // family animation (2.4 s), after a 2 s presentation delay.
                yield return new WaitForSeconds(7.5f);
                break;
            case HouseAnimations.WatchTV:
                yield return new WaitForSeconds(4f);
                break;
            default:
                yield return new WaitForSeconds(2f);
                break;
        }
    }

    private void PlayConfiguredAnimation(HouseAnimations animation)
    {
        switch (animation)
        {
            case HouseAnimations.RadioAnnouncement:
                RadioOnAnnouncement();
                break;
            case HouseAnimations.ReviewEmergencyPlan:
                OnReviewPlan();
                break;
            case HouseAnimations.CheckGoBag:
                PlayConfiguredGoBag();
                break;
            case HouseAnimations.PlayRadioSong:
                RadioPlayingSong();
                break;
            case HouseAnimations.ParentsPanic:
                ParentsPanic();
                break;
            case HouseAnimations.WatchTV:
                WatchTV();
                break;
        }
    }

    private static Events GetExpectedEvent(HouseAnimations animation)
    {
        return animation switch
        {
            HouseAnimations.RadioAnnouncement => Events.RadioBroadcast,
            HouseAnimations.ReviewEmergencyPlan => Events.ReviewEmergencyPlan,
            HouseAnimations.CheckGoBag => Events.CheckGoBag,
            _ => Events.Empty
        };
    }

    public void RadioOnAnnouncement() //called from WebGL
    {
        ModeName mode = SimulationManager.Instance.CurrentMode;
        if (mode == ModeName.House)
        {
            Debug.Log("Radio Announcement Playing");
            radioAnimator.SetTrigger(AnimationsInSuper.Announcement.ToString());
            announcementBubble.SetActive(true);
        }
        else
        {
            SuperMarketMode superMarketMode = SimulationManager.Instance.GetMode<SuperMarketMode>();
            if (superMarketMode != null)
            {
                Debug.Log("Radio Announcement Playing in SuperMarket");
                superMarketMode.SuperQueueAnimation(AnimationsInSuper.Announcement.ToString());
            }
        }
        AudioManager.Instance.PlayAnnouncement();
        Invoke(nameof(SendRadioBroadcast), 3f); // Send the event after 3 seconds
    }

    private void SendRadioBroadcast()
    {
        WebGLBridge.SendEvent(Events.RadioBroadcast.ToString());
    }

    public void RadioPlayingSong() //called from WebGL
    {
        Debug.Log("Radio Playing Song");
        AudioManager.Instance.PlaySong();
    }

    public void June1onArrivesAnim() //called from WebGL
    {
        Debug.Log("June1onArrives");
        june1OnArriveAnimationCompleted = false;
        calendar.SetTrigger("To June 1");

    }

    public void HandleJune1AnimationCompleted()
    {
        june1OnArriveAnimationCompleted = true;
    }
    public void OnReviewPlan() // called from WebGL
    {
        emergencyPlanMainPlayed = true;
        StopHouseAnimationCoroutine(ref emergencyPlanCoroutine);
        emergencyPlanCoroutine = StartCoroutine(OnReviewPlanCoroutine());
    }
    IEnumerator OnReviewPlanCoroutine()
    {
        while (canPlayEmergencyPlan == false)
        {
            yield return null; // Wait until the page animation has been played
        }

        StopHouseAnimationCoroutine(ref checkGoBagCoroutine);
        ResetHouseAnimationObjects(checkGobag);

        emergencyPlanMain.SetActive(true);
        emergencyPlanCoroutine = null;
    }

    private void PlayConfiguredGoBag()
    {
        StopHouseAnimationCoroutine(ref checkGoBagCoroutine);
        checkGoBagCoroutine = StartCoroutine(ChekedGoBag());
    }

    public void OnCheckGobag() // legacy WebGL entry point; Go Bag is queue-driven now
    {
        Debug.LogWarning("Check Go Bag can only be started by the configured House rule queue.");
    }

    public void WatchTV()
    {
        DisableOtherHouseAnimation(tvAnimation);
        ScreenFader.Instance.Fade(() =>
        {
            kayAndKelanPlaying.SetActive(false);
            tvAnimation.SetActive(true);
        });
        if (!suppressAutomaticGardenTransition)
        {
            Invoke(nameof(ChangeToClearingGardenMode), 4f); // End TV after 4 seconds
        }
    }

    public void ParentsPanic()
    {
        panicAnimationPlayed = true;
        DisableOtherHouseAnimation(parentsPanic);
        ScreenFader.Instance.Fade(() =>
        {
            parentsPanic.SetActive(true);
            parentsSittingDown.SetActive(false);
        });
    }

    private void DisableOtherHouseAnimation(GameObject animationToKeepActive)
    {
        if (emergencyPlanMain != null && emergencyPlanMain != animationToKeepActive)
        {
            ResetHouseAnimationObjects(emergencyPlanMain);
        }

        if (checkGobag != null && checkGobag != animationToKeepActive)
        {
            ResetHouseAnimationObjects(checkGobag);
        }

        if (parentsPanic != null && parentsPanic != animationToKeepActive && parentsPanic.activeSelf)
        {
            parentsPanic.SetActive(false);
        }

        if (tvAnimation != null && tvAnimation != animationToKeepActive && tvAnimation.activeSelf)
        {
            tvAnimation.SetActive(false);
        }
    }

    IEnumerator ChekedGoBag()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Checking Go bag");

        StopHouseAnimationCoroutine(ref emergencyPlanCoroutine);
        ResetHouseAnimationObjects(emergencyPlanMain);

        ResetHouseAnimationObjects(checkGobag);
        checkGobag.SetActive(true);
        // WebGLBridge.SendEvent(Events.CheckGoBag.ToString());
        yield return new WaitForSeconds(5f);
        if (!suppressAutomaticGardenTransition)
        {
            ChangeToClearingGardenMode();
        }

        checkGoBagCoroutine = null;
    }

    private void StopHouseAnimationCoroutine(ref Coroutine coroutine)
    {
        if (coroutine == null)
        {
            return;
        }

        StopCoroutine(coroutine);
        coroutine = null;
    }

    private static void ResetHouseAnimationObjects(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        ScineSwicher switcher = root.GetComponent<ScineSwicher>();
        if (switcher != null && switcher.nextObjectToSwitch != null)
        {
            switcher.nextObjectToSwitch.SetActive(false);
        }

        root.SetActive(false);
    }

    public void ChangeToClearingGardenMode()
    {
        SimulationManager.Instance.SwitchMode(ModeName.ClearingGarden);
    }

    public void May1onArrivesAnim() //called from WebGL
    {
        Debug.Log("May1onArrives");
        calendar.enabled = false;
        // pageAnimationPlayed = true;


        if (pageAnimation != null)
        {
            pageAnimation.SetActive(true);
        }
        ContinueCalendarAnimation();
    }

    public void HandleMayArrivalForConfiguredLesson()
    {
        if (suppressAutomaticGardenTransition)
        {
            May1onArrivesAnim();
        }
    }
}
