using System.Collections;
using UnityEngine;
using System;

public class ShelterMod : MonoBehaviour, ISimulationMode
{
    public enum ShelterAnimations
    {
        ColoursABook,
        PlaysWithToy,
        PlaysOutside,
        TalksToAStranger

    }
    [SerializeField] private Animator kayakAnimator;
    [SerializeField] private Animator strangerAnimator;
    [SerializeField] private Animator talkAnimator;

    public float timer = 5f;
    public bool simulationStart = false;

    private readonly System.Collections.Generic.Queue<ShelterAnimations> animationQueue = new();
    private bool isPlaying;
    private bool currentAnimationFinished;
    private ShelterAnimations? currentQueuedAnimation;
    private Coroutine configuredSequenceCoroutine;

    private System.Collections.Generic.Dictionary<ShelterAnimations, Action<Action>> animationMap;

    public void Cleanup()
    {
        // cleanup shelter mode state
        configuredSequenceCoroutine = null;
        StopAllCoroutines();
        animationQueue.Clear();
        isPlaying = false;
        currentAnimationFinished = false;
        currentQueuedAnimation = null;
        simulationStart = false;
        if (kayakAnimator != null) kayakAnimator.gameObject.SetActive(false);
        if (strangerAnimator != null) strangerAnimator.gameObject.SetActive(false);
        if (talkAnimator != null) talkAnimator.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (simulationStart)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0f;
                simulationStart = false;

                WebGLBridge.OnInShelter(ObjectsHolder.instance.GetKayID());
                // for checking the shelter animations, uncomment the following lines to see them in action
                if (Application.isEditor)
                {
                    ShelterQueueAnimation("ColoursABook");
                    ShelterQueueAnimation("TalksToAStranger");
                    // ShelterQueueAnimation("PlaysWithToy");


                    ShelterQueueAnimation("PlaysOutside");
                }
            }
        }
    }

    public void Initialize()
    {
        // initialize shelter mode
        if (kayakAnimator != null) kayakAnimator.gameObject.SetActive(true);
        simulationStart = false;
    }

    public void OnSimulationEnd()
    {
        // simulation ended
        simulationStart = false;
    }

    public void OnSimulationStart()
    {
        simulationStart = true;

    }

    public void PlayConfiguredSequence(System.Collections.Generic.IReadOnlyList<string> animationNames)
    {
        if (animationNames == null)
        {
            Debug.LogWarning("PlayConfiguredSequence requires a shelter animation list.");
            return;
        }

        StopAllCoroutines();
        animationQueue.Clear();
        isPlaying = false;
        currentAnimationFinished = false;
        currentQueuedAnimation = null;
        simulationStart = false;

        if (kayakAnimator != null) kayakAnimator.gameObject.SetActive(true);
        if (strangerAnimator != null) strangerAnimator.gameObject.SetActive(false);
        if (talkAnimator != null) talkAnimator.gameObject.SetActive(false);

        configuredSequenceCoroutine = StartCoroutine(PlayConfiguredSequenceCoroutine(animationNames));
    }

    private IEnumerator PlayConfiguredSequenceCoroutine(System.Collections.Generic.IReadOnlyList<string> animationNames)
    {
        for (int i = 0; i < animationNames.Count; i++)
        {
            ShelterQueueAnimation(animationNames[i]);
        }

        configuredSequenceCoroutine = null;
        yield break;
    }



    private void Awake()
    {
        animationMap = new System.Collections.Generic.Dictionary<ShelterAnimations, Action<Action>>()
        {
            { ShelterAnimations.ColoursABook, OnColoursABook },
            { ShelterAnimations.PlaysWithToy, OnPlaysWithToy },
            { ShelterAnimations.PlaysOutside, OnPlaysOutside },
            { ShelterAnimations.TalksToAStranger, OnTalksToAStranger }
        };
    }

    public void ShelterQueueAnimation(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName)) return;
        animationName = animationName.Trim();
        if (Enum.TryParse(animationName, true, out ShelterAnimations animation) &&
            Enum.IsDefined(typeof(ShelterAnimations), animation))
        {
            if (currentQueuedAnimation == animation || animationQueue.Contains(animation)) return;
            animationQueue.Enqueue(animation);
            if (!isPlaying) StartCoroutine(ProcessQueue());
        }
        else
        {
            Debug.LogWarning($"No such shelter animation: {animationName}");
        }
    }


    private IEnumerator ProcessQueue()
    {
        Debug.Log("Processing shelter animation queue...");
        isPlaying = true;
        while (animationQueue.Count > 0)
        {
            ShelterAnimations next = animationQueue.Dequeue();
            currentQueuedAnimation = next;
            if (animationMap.TryGetValue(next, out var startAnimation))
            {
                currentAnimationFinished = false;
                startAnimation(() => { currentAnimationFinished = true; });
                yield return new WaitUntil(() => currentAnimationFinished);
            }
            else
            {
                Debug.LogWarning("No function for shelter animation: " + next);
            }
            currentQueuedAnimation = null;
        }
        isPlaying = false;
    }

    public void OnColoursABook(Action onComplete)
    {
        StartCoroutine(ColoursABook(kayakAnimator, "ColoursABook", onComplete));
    }
    IEnumerator ColoursABook(Animator animator, string triggerName, Action onComplete)
    {
        animator.SetTrigger(triggerName);
        timer = 0f;
        while (timer < 3f) // wait for 3 seconds or until the animation is done
        {
            timer += Time.deltaTime;
            yield return null;
        }
        WebGLBridge.SendEvent(Events.ColorBook.ToString());

        onComplete?.Invoke();
    }

    public void OnPlaysWithToy(Action onComplete)
    {
        StartCoroutine(PlaysWithToy(kayakAnimator, "PlaysWithToy", onComplete));
    }
    IEnumerator PlaysWithToy(Animator animator, string triggerName, Action onComplete)
    {
        animator.SetTrigger(triggerName);
        timer = 0f;
        while (timer < 3f) // wait for 3 seconds or until the animation is done
        {
            timer += Time.deltaTime;
            yield return null;
        }
        WebGLBridge.SendEvent(Events.PlayToy.ToString());
        onComplete?.Invoke();
    }

    public void OnPlaysOutside(Action onComplete)
    {
        StartCoroutine(PlaysOutside(kayakAnimator, "PlaysOutside", onComplete));
    }
    IEnumerator PlaysOutside(Animator animator, string triggerName, Action onComplete)
    {
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(2f); // wait for 2 seconds before moving the kayak


        GameObject objToMove = kayakAnimator.gameObject;
        Vector3 targetPoint = new Vector3(-14f, kayakAnimator.transform.position.y, kayakAnimator.transform.position.z);
        kayakAnimator.SetTrigger("Run");
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {

            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, 2 * Time.deltaTime);
            yield return null;
        }
        onComplete?.Invoke();
    }


    public void OnTalksToAStranger(Action onComplete)
    {
        StartCoroutine(TalksToAStranger(kayakAnimator, "TalksToAStranger", onComplete));
    }
    IEnumerator TalksToAStranger(Animator animator, string triggerName, Action onComplete)
    {

        strangerAnimator.gameObject.SetActive(true);
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(4f); // wait for 4 seconds before showing the talk animation
        talkAnimator.gameObject.SetActive(true);

        timer = 0f;
        while (timer < 5f) // wait for 5 seconds or until the animation is done
        {
            timer += Time.deltaTime;
            yield return null;
        }
        talkAnimator.gameObject.SetActive(false);
        onComplete?.Invoke();
    }




}


