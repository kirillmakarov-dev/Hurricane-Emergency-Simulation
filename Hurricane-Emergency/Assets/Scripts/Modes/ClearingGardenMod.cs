using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClearingGardenAnimations
{
    ClearYard,
    GatherPlywood,
    WaterFlowers,
    GoForWalk
}

public class ClearingGardenMod : MonoBehaviour, ISimulationMode
{
    public GameObject dadAnimator;
    public GameObject momAnimator;

    public bool weteringFlowers = false;
    [SerializeField] GameObject waterParticles;

    private bool startAnimations = false;

    private enum Anim
    {
        Walk,
        PlyWood,
        Cleaning,
        Watering
    }


    private sealed class AnimationQueueState
    {
        public AnimationQueueState(string playerName, Dictionary<Anim, Action<Action>> animationMap)
        {
            PlayerName = playerName;
            AnimationMap = animationMap;
        }

        public string PlayerName { get; }
        public Dictionary<Anim, Action<Action>> AnimationMap { get; }
        public Queue<Anim> Queue { get; } = new();
        public bool IsPlaying { get; set; }
    }

    private AnimationQueueState momQueueState;
    private AnimationQueueState dadQueueState;

    private Dictionary<Anim, Action<Action>> momAnimationMap;
    private Dictionary<Anim, Action<Action>> dadAnimationMap;
    private readonly Queue<ClearingGardenAnimations> configuredAnimationQueue = new();
    private Coroutine configuredQueueCoroutine;

    private void Awake()
    {
        momAnimationMap = new Dictionary<Anim, Action<Action>>()
        {
            { Anim.Cleaning, StartMomCleaning },
            { Anim.Watering, StartWateringFlowers },
        };

        dadAnimationMap = new Dictionary<Anim, Action<Action>>()
        {
            { Anim.Walk, StartDadWalk },
            { Anim.PlyWood, StartGatherPlywood },
        };

        momQueueState = new AnimationQueueState("Mom", momAnimationMap);
        dadQueueState = new AnimationQueueState("Dad", dadAnimationMap);
    }

    public void Cleanup()
    {
        if (configuredQueueCoroutine != null)
        {
            StopCoroutine(configuredQueueCoroutine);
            configuredQueueCoroutine = null;
        }

        configuredAnimationQueue.Clear();
    }

    public void Initialize()
    {

    }
    void Update()
    {
        if (weteringFlowers)
        {
            weteringFlowers = false;
            GoForWalk();
            OnWateringTheFlowers();
            OnClearYard();

            OnGatherPlywood();
        }
    }

    public void OnSimulationEnd()
    {
        // throw new System.NotImplementedException();
    }

    public void OnSimulationStart()
    {
        startAnimations = true;

        if (Application.isEditor)
        {
            // OnClearYard();

            // OnGatherPlywood();

        }
    }

    public void PlayConfiguredSequence(IReadOnlyList<string> animationNames)
    {
        if (configuredQueueCoroutine != null)
        {
            StopCoroutine(configuredQueueCoroutine);
        }

        configuredAnimationQueue.Clear();
        for (int i = 0; i < animationNames.Count; i++)
        {
            if (Enum.TryParse(animationNames[i], true, out ClearingGardenAnimations animation))
            {
                configuredAnimationQueue.Enqueue(animation);
            }
            else
            {
                Debug.LogWarning($"Unknown Cleaning Garden lesson command: {animationNames[i]}");
            }
        }

        configuredQueueCoroutine = StartCoroutine(ProcessConfiguredLessonQueue());
    }

    private IEnumerator ProcessConfiguredLessonQueue()
    {
        while (configuredAnimationQueue.Count > 0)
        {
            ClearingGardenAnimations animation = configuredAnimationQueue.Dequeue();
            bool finished = false;
            Action onComplete = () => finished = true;

            switch (animation)
            {
                case ClearingGardenAnimations.ClearYard:
                    StartMomCleaning(onComplete);
                    break;
                case ClearingGardenAnimations.GatherPlywood:
                    StartGatherPlywood(onComplete);
                    break;
                case ClearingGardenAnimations.WaterFlowers:
                    StartWateringFlowers(onComplete);
                    break;
                case ClearingGardenAnimations.GoForWalk:
                    StartDadWalk(onComplete);
                    break;
            }

            yield return new WaitUntil(() => finished);
            WebGLBridge.SendEvent(GetConfiguredEvent(animation).ToString());
        }

        configuredQueueCoroutine = null;
    }

    private static Events GetConfiguredEvent(ClearingGardenAnimations animation)
    {
        return animation switch
        {
            ClearingGardenAnimations.ClearYard => Events.CleanYard,
            ClearingGardenAnimations.GatherPlywood => Events.CollectPlywood,
            _ => Events.Empty
        };
    }

    public void OnClearYard() // mother //called from WebGL
    {
        Debug.Log("OnClearYard called - from unity");
        EnqueueAnimation(momQueueState, Anim.Cleaning);
    }

    public void OnGatherPlywood()// father //called from WebGL
    {
        Debug.Log("OnGatherPlywood called - from unity");
        EnqueueAnimation(dadQueueState, Anim.PlyWood);
    }


    public void GoForWalk() // called from WebGL
    {
        EnqueueAnimation(dadQueueState, Anim.Walk);
    }

    private void StartDadWalk(Action onComplete)
    {
        StartCoroutine(GoForWalkCoroutine(onComplete));
    }

    IEnumerator GoForWalkCoroutine(Action onComplete)
    {
        while (!startAnimations)
        {
            yield return null; // Wait until the simulation starts
        }
        dadAnimator.SetActive(true);
        dadAnimator.transform.position = new Vector3(-6f, 2f, 0f);
        Animator animator = dadAnimator.GetComponent<Animator>();
        animator.enabled = true;
        animator.SetTrigger("OnWalk");
        yield return new WaitForSeconds(8f);
        onComplete?.Invoke();
    }



    public void OnWateringTheFlowers() // called from WebGL
    {
        EnqueueAnimation(momQueueState, Anim.Watering);
    }

    private void StartMomCleaning(Action onComplete)
    {
        StartCoroutine(MomCleaning(onComplete));
    }

    IEnumerator MomCleaning(Action onComplete)
    {
        while (!startAnimations)
        {
            yield return null; // Wait until the simulation starts
        }
        yield return new WaitForSeconds(3f);
        momAnimator.SetActive(true);
        momAnimator.GetComponent<Animator>().SetTrigger("Walking");
        momAnimator.transform.position = new Vector3(-6f, 2f, 0f);

        // WebGLBridge.SendEvent(Events.CleanYard.ToString());

        StartCoroutine(MoveToPoint(momAnimator, new Vector3(-5f, -1.5f, 0f), 2f, "Cleaning"));
        yield return new WaitForSeconds(7);
        momAnimator.GetComponent<Animator>().SetTrigger("Standing");
        yield return new WaitForSeconds(1);

        momAnimator.GetComponent<Animator>().SetTrigger("Walking");
        StartCoroutine(MoveToPoint(momAnimator, new Vector3(0f, -1.5f, 0f), 2f, "Cleaning"));
        yield return new WaitForSeconds(10f);
        onComplete?.Invoke();
    }

    private void StartGatherPlywood(Action onComplete)
    {
        StartCoroutine(GatherPlywood(onComplete));
    }

    IEnumerator GatherPlywood(Action onComplete)
    {
        while (!startAnimations)
        {
            yield return null; // Wait until the simulation starts
        }
        yield return new WaitForSeconds(3f);
        dadAnimator.transform.position = new Vector3(-6f, 2f, 0f);
        dadAnimator.SetActive(true);
        Animator animator = dadAnimator.GetComponent<Animator>();
        animator.enabled = true;
        animator.SetTrigger("Walk");
        // WebGLBridge.SendEvent(Events.CollectPlywood.ToString());
        while (animator.enabled != false)
        {
            yield return null; // Wait until the simulation starts
        }
        onComplete?.Invoke();
    }


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

    private void StartWateringFlowers(Action onComplete)
    {
        StartCoroutine(WateringFlowers(onComplete));
    }

    IEnumerator WateringFlowers(Action onComplete)
    {
        while (!startAnimations)
        {
            yield return null; // Wait until the simulation starts
        }
        momAnimator.transform.position = new Vector3(-6f, 2f, 0f);
        momAnimator.SetActive(true);
        momAnimator.GetComponent<Animator>().SetTrigger("Walking");

        StartCoroutine(MoveToPoint(momAnimator, new Vector3(-3f, -2f, 0f), 2f, "Watering"));

        WaterParticlesEnabled(true);

        yield return new WaitForSeconds(5f);
        WaterParticlesEnabled(false);
        momAnimator.GetComponent<Animator>().SetTrigger("Walking");
        WaterParticlesEnabled(true);
        StartCoroutine(MoveToPoint(momAnimator, new Vector3(1.7f, -2f, 0f), 2f, "Watering"));

        yield return new WaitForSeconds(8f);
        momAnimator.GetComponent<Animator>().SetTrigger("Standing");
        yield return new WaitForSeconds(1f);
        onComplete?.Invoke();

    }

    public void WaterParticlesEnabled(bool value)
    {
        if (waterParticles != null)
        {
            waterParticles.SetActive(value);
        }
    }



    // public void AddMomAnimationFromWeb(string animationName) // called from WebGL
    // {
    //     EnqueueAnimationFromWeb(animationName, momQueueState);
    // }


    // public void AddDadAnimationFromWeb(string animationName) // called from WebGL
    // {
    //     EnqueueAnimationFromWeb(animationName, dadQueueState);
    // }



    private void EnqueueAnimation(AnimationQueueState queueState, Anim animation)
    {
        if (queueState == null)
        {
            Debug.LogWarning($"Queue state is not initialized for animation {animation}");
            return;
        }

        queueState.Queue.Enqueue(animation);

        if (!queueState.IsPlaying)
        {
            StartCoroutine(ProcessQueue(queueState));
        }
    }


    // private void EnqueueAnimationFromWeb(string animationName, AnimationQueueState queueState)
    // {
    //     Debug.Log($"added animation from web for {queueState.PlayerName}: {animationName}");

    //     if (string.IsNullOrWhiteSpace(animationName))
    //     {
    //         Debug.LogWarning($"Empty animation name for {queueState.PlayerName}");
    //         return;
    //     }

    //     animationName = animationName.Trim();

    //     if (!Enum.TryParse(animationName, true, out Anim animation) ||
    //         !Enum.IsDefined(typeof(Anim), animation))
    //     {
    //         Debug.LogWarning($"No such animation in enum Anim: {animationName}");
    //         return;
    //     }

    //     if (!queueState.AnimationMap.ContainsKey(animation))
    //     {
    //         Debug.LogWarning($"Animation {animation} is not supported for {queueState.PlayerName}");
    //         return;
    //     }

    //     queueState.Queue.Enqueue(animation);
    //     Debug.Log($"Animation added to {queueState.PlayerName} queue: {animation}");

    //     if (!queueState.IsPlaying)
    //     {
    //         StartCoroutine(ProcessQueue(queueState));
    //     }
    // }


    private IEnumerator ProcessQueue(AnimationQueueState queueState)
    {
        queueState.IsPlaying = true;

        while (queueState.Queue.Count > 0)
        {
            Anim nextAnimation = queueState.Queue.Dequeue();

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

}

