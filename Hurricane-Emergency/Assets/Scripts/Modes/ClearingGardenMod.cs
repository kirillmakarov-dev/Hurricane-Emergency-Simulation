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

public class ClearingGardenMod : MonoBehaviour, IConfiguredSequenceMode
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


    private SequentialAnimationQueue<Anim> momQueueState;
    private SequentialAnimationQueue<Anim> dadQueueState;

    private Dictionary<Anim, Action<Action>> momAnimationMap;
    private Dictionary<Anim, Action<Action>> dadAnimationMap;
    private Dictionary<ClearingGardenAnimations, Action<Action>> configuredAnimationMap;
    private SequentialAnimationQueue<ClearingGardenAnimations> configuredAnimationQueue;
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

        configuredAnimationMap = new Dictionary<ClearingGardenAnimations, Action<Action>>()
        {
            { ClearingGardenAnimations.ClearYard, StartMomCleaning },
            { ClearingGardenAnimations.GatherPlywood, StartGatherPlywood },
            { ClearingGardenAnimations.WaterFlowers, StartWateringFlowers },
            { ClearingGardenAnimations.GoForWalk, StartDadWalk },
        };

        momQueueState = new SequentialAnimationQueue<Anim>(momAnimationMap);
        dadQueueState = new SequentialAnimationQueue<Anim>(dadAnimationMap);
        configuredAnimationQueue =
            new SequentialAnimationQueue<ClearingGardenAnimations>(configuredAnimationMap);
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
            AnimationEnqueueResult result =
                configuredAnimationQueue.Enqueue(animationNames[i], out _);
            if (result == AnimationEnqueueResult.InvalidName)
            {
                Debug.LogWarning($"Unknown Cleaning Garden lesson command: {animationNames[i]}");
            }
        }

        configuredQueueCoroutine = StartCoroutine(ProcessConfiguredLessonQueue());
    }

    private IEnumerator ProcessConfiguredLessonQueue()
    {
        yield return configuredAnimationQueue.Process(
            animation => WebGLBridge.SendEvent(GetConfiguredEvent(animation).ToString()),
            animation => Debug.LogWarning($"No function for Cleaning Garden animation: {animation}"));

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
    private void EnqueueAnimation(SequentialAnimationQueue<Anim> queueState, Anim animation)
    {
        if (queueState == null)
        {
            Debug.LogWarning($"Queue state is not initialized for animation {animation}");
            return;
        }

        queueState.Enqueue(animation);

        if (queueState.NeedsProcessing)
        {
            string playerName = ReferenceEquals(queueState, momQueueState) ? "Mom" : "Dad";
            StartCoroutine(ProcessQueue(queueState, playerName));
        }
    }


    private IEnumerator ProcessQueue(SequentialAnimationQueue<Anim> queueState, string playerName)
    {
        yield return queueState.Process(
            next => Debug.Log($"{playerName} animation finished: {next}"),
            next => Debug.LogWarning($"No function for {playerName} animation: {next}"));
    }

}

