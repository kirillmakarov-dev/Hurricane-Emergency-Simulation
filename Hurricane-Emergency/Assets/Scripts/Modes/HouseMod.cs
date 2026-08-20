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

public class HouseMod : MonoBehaviour, ISimulationMode
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
    private bool suppressAutomaticGardenTransition;

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

        configuredAnimationQueue.Clear();
        suppressAutomaticGardenTransition = false;
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
        firstScineAnim.SetTrigger("House");
        //calendarTimer = 0;
        timerRun = true;
    }

    public void PlayConfiguredSequence(IReadOnlyList<string> animationNames)
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

        configuredQueueCoroutine = StartCoroutine(ProcessConfiguredQueue());
    }

    private IEnumerator ProcessConfiguredQueue()
    {
        while (configuredAnimationQueue.Count > 0)
        {
            HouseAnimations animation = configuredAnimationQueue.Dequeue();
            PlayConfiguredAnimation(animation);
            yield return WaitForConfiguredAnimation(animation);
            WebGLBridge.SendEvent(GetExpectedEvent(animation).ToString());
        }

        configuredQueueCoroutine = null;
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

                yield return new WaitForSeconds(1.25f);
                break;
            }
            case HouseAnimations.CheckGoBag:
                yield return new WaitForSeconds(3.5f);
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
                OnCheckGobag();
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
        calendar.SetTrigger("To June 1");

    }
    public void OnReviewPlan() // called from WebGL
    {
        emergencyPlanMainPlayed = true;
        // emergencyPlanMain.SetActive(true);
        StartCoroutine(OnReviewPlanCoroutine());
    }
    IEnumerator OnReviewPlanCoroutine()
    {
        while (canPlayEmergencyPlan == false)
        {
            yield return null; // Wait until the page animation has been played
        }
        emergencyPlanMain.SetActive(true);
    }

    public void OnCheckGobag() // called from WebGL
    {
        StartCoroutine(ChekedGoBag());
    }

    public void WatchTV()
    {
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
        ScreenFader.Instance.Fade(() =>
        {
            parentsPanic.SetActive(true);
            parentsSittingDown.SetActive(false);
        });
    }
    IEnumerator ChekedGoBag()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log("Checking Go bag");
        checkGobag.SetActive(true);
        // WebGLBridge.SendEvent(Events.CheckGoBag.ToString());
        yield return new WaitForSeconds(5f);
        if (!suppressAutomaticGardenTransition)
        {
            ChangeToClearingGardenMode();
        }
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
