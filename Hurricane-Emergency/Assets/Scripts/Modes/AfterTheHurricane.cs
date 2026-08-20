using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ActivateDeactivateObject
{
    public string objectName;

    public GameObject objectInHand;
    public GameObject objectInScine;


    public void ActivateObject()
    {
        if (objectInHand != null)
        {
            objectInHand.SetActive(true);
        }
        if (objectInScine != null)
        {
            objectInScine.SetActive(false);
        }
    }
    public void DeactivateObject()
    {
        if (objectInHand != null)
        {
            objectInHand.SetActive(false);
        }
    }

    public void ObjectToActivateOff()
    {
        if (objectInHand != null)
        {
            objectInHand.SetActive(false);
        }
    }
}

public enum AfterHurricaneAnimations
{
    picksUpBranches,
    picksUpBottles,
    picksUpBrockenGlass,
    picksUpElectricWires,
    FatherPicksUpBrockenGlass,
    FatherPicksUpElectricWires,
    GoForWalk,
    MotherCutWood
}

public class AfterTheHurricane : MonoBehaviour, IConfiguredSequenceMode
{
    [SerializeField] private Animator fatherAnimator;
    [SerializeField] private Animator kelanAnimator;
    [SerializeField] private Animator motherAnimator;
    public List<ActivateDeactivateObject> branchesList = new List<ActivateDeactivateObject>();
    public List<ActivateDeactivateObject> bottlesList = new List<ActivateDeactivateObject>();
    public List<ActivateDeactivateObject> brockenGlassList = new List<ActivateDeactivateObject>();
    public List<ActivateDeactivateObject> treeList = new List<ActivateDeactivateObject>();
    public List<GameObject> bubbleReactions = new List<GameObject>();

    public GameObject motherCanvas;
    public GameObject allClear;




    public float timer = 5f;
    public bool simulationStart = false;

    private SequentialAnimationQueue<AfterHurricaneAnimations> animationQueue;
    private Coroutine configuredSequenceCoroutine;

    private Dictionary<AfterHurricaneAnimations, Action<Action>> animationMap;

    public void Cleanup()
    {
        // cleanup after the hurricane mode state
        configuredSequenceCoroutine = null;
        StopAllCoroutines();
        animationQueue?.Clear();
        simulationStart = false;
        if (fatherAnimator != null) fatherAnimator.gameObject.SetActive(false);
        if (kelanAnimator != null) kelanAnimator.gameObject.SetActive(false);
        if (motherAnimator != null) motherAnimator.gameObject.SetActive(false);
        if (motherCanvas != null) motherCanvas.SetActive(false);
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

                WebGLBridge.AllClear(ObjectsHolder.instance.GetAllClearID());
            }
        }
    }

    public void Initialize()
    {

        // initialize after the hurricane mode
        if (fatherAnimator != null) fatherAnimator.gameObject.SetActive(true);
        if (kelanAnimator != null) kelanAnimator.gameObject.SetActive(true);
        if (motherAnimator != null) motherAnimator.gameObject.SetActive(true);
        if (motherCanvas != null) motherCanvas.SetActive(false);

    }

    public void OnSimulationEnd()
    {
        // simulation ended
        simulationStart = false;
    }

    public void OnSimulationStart()
    {
        // simulationStart = true;
        if (Application.isEditor)
        {
            // AfterHurricaneQueueAnimation("picksUpBranches");
            // AfterHurricaneQueueAnimation("picksUpBottles");
            // AfterHurricaneQueueAnimation("picksUpBrockenGlass");
            // AfterHurricaneQueueAnimation("picksUpElectricWires");
            // AfterHurricaneQueueAnimation("MotherCutWood");
            // AfterHurricaneQueueAnimation("GoForWalk");
            // AfterHurricaneQueueAnimation("FatherPicksUpBrockenGlass");
            // AfterHurricaneQueueAnimation("FatherPicksUpElectricWires");
        }
    }

    public void PlayConfiguredSequence(IReadOnlyList<string> animationNames)
    {
        if (animationNames == null)
        {
            Debug.LogWarning("PlayConfiguredSequence requires an after the hurricane animation list.");
            return;
        }

        StopAllCoroutines();
        animationQueue.Clear();
        simulationStart = false;

        if (fatherAnimator != null) fatherAnimator.gameObject.SetActive(true);
        if (kelanAnimator != null) kelanAnimator.gameObject.SetActive(true);
        if (motherAnimator != null) motherAnimator.gameObject.SetActive(true);
        if (motherCanvas != null) motherCanvas.SetActive(false);
        if (allClear != null) allClear.SetActive(false);

        configuredSequenceCoroutine = StartCoroutine(PlayConfiguredSequenceCoroutine(animationNames));
    }

    private IEnumerator PlayConfiguredSequenceCoroutine(IReadOnlyList<string> animationNames)
    {
        for (int i = 0; i < animationNames.Count; i++)
        {
            AfterHurricaneQueueAnimation(animationNames[i]);
        }

        configuredSequenceCoroutine = null;
        yield break;
    }

    public void AllClearAnimation()
    {
        if (allClear != null)
        {
            StartCoroutine(AllclearCoroutine());
        }
    }


    private void Awake()
    {
        animationMap = new Dictionary<AfterHurricaneAnimations, Action<Action>>()
        {
            { AfterHurricaneAnimations.picksUpBranches, PicksUpBranches },
            { AfterHurricaneAnimations.picksUpBottles, PicksUpBottles },
            { AfterHurricaneAnimations.picksUpBrockenGlass, PicksUpBrockenGlass },
            { AfterHurricaneAnimations.picksUpElectricWires, PicksUpElectricWires },
            { AfterHurricaneAnimations.GoForWalk, GoForWalk },
            { AfterHurricaneAnimations.MotherCutWood, MotherCutWood },
            { AfterHurricaneAnimations.FatherPicksUpBrockenGlass, FatherPicksUpBrockenGlass },
            { AfterHurricaneAnimations.FatherPicksUpElectricWires, FatherPicksUpElectricWires },
        };
        animationQueue = new SequentialAnimationQueue<AfterHurricaneAnimations>(animationMap, true);
    }


    public void AfterHurricaneQueueAnimation(string animationName)
    {
        if (string.IsNullOrWhiteSpace(animationName)) return;
        AnimationEnqueueResult result = animationQueue.Enqueue(animationName, out _);
        if (result == AnimationEnqueueResult.Added)
        {
            if (animationQueue.NeedsProcessing) StartCoroutine(ProcessQueue());
        }
        else if (result == AnimationEnqueueResult.InvalidName)
        {
            Debug.LogWarning($"No such after hurricane animation: {animationName}");
        }
    }


    private IEnumerator ProcessQueue()
    {
        Debug.Log("Processing after hurricane animation queue...");
        yield return animationQueue.Process(
            onMissingAction: next => Debug.LogWarning("No function for after hurricane animation: " + next));
    }

    public void PicksUpBranches(Action onComplete)
    {
        StartCoroutine(PicksUpBranchesCoroutine(kelanAnimator, kelanAnimator.gameObject, onComplete, Events.PickBranches));
    }

    IEnumerator PicksUpBranchesCoroutine(Animator animator, GameObject objToMove, Action onComplete, Events eventType)
    {
        string walking = "Walking";
        string picksUp = "picksUp";
        string WalkingWith = "WalkingWith";

        Vector3 targetPoint = new Vector3(5f, -1.5f, kelanAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(walking);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));


        animator.SetTrigger(picksUp);
        yield return new WaitForSeconds(0.3f);
        branchesList[0].ActivateObject();

        animator.SetTrigger(WalkingWith);
        targetPoint = new Vector3(7f, -1f, kelanAnimator.transform.position.z);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));

        animator.SetTrigger(picksUp);
        yield return new WaitForSeconds(0.3f);
        branchesList[1].ActivateObject();
        WebGLBridge.SendEvent(eventType.ToString());

        animator.SetTrigger(WalkingWith);
        FlipObjects(objToMove, targetPoint);
        targetPoint = new Vector3(-11f, -1f, kelanAnimator.transform.position.z);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));

        branchesList[1].DeactivateObject();
        branchesList[0].DeactivateObject();

        targetPoint = new Vector3(-4f, -1f, kelanAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(walking);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));

        onComplete?.Invoke();
    }

    IEnumerator MoveTo(GameObject objToMove, Vector3 targetPoint)
    {
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, 2 * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
    }




    public void PicksUpBottles(Action onComplete)
    {
        StartCoroutine(PicksUpBottlesCoroutine(kelanAnimator, kelanAnimator.gameObject, onComplete, Events.PickBottles));
    }

    IEnumerator PicksUpBottlesCoroutine(Animator animator, GameObject objToMove, Action onComplete, Events eventType)
    {
        string walking = "Walking";
        string picksUp = "picksUp";
        string WalkingWith = "WalkingWith";
        string idle = "Idle";


        Vector3 targetPoint = new Vector3(-3, -2f, kelanAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(walking);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));


        animator.SetTrigger(picksUp);
        yield return new WaitForSeconds(0.3f);
        bottlesList[0].ActivateObject();

        animator.SetTrigger(WalkingWith);
        targetPoint = new Vector3(0f, -2.5f, kelanAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));

        animator.SetTrigger(picksUp);
        yield return new WaitForSeconds(0.3f);
        bottlesList[1].ActivateObject();
        WebGLBridge.SendEvent(eventType.ToString());

        animator.SetTrigger(WalkingWith);
        FlipObjects(objToMove, targetPoint);
        targetPoint = new Vector3(-5f, -0.5f, kelanAnimator.transform.position.z);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));
        animator.SetTrigger(idle);

        bottlesList[1].ObjectToActivateOff();
        bottlesList[0].ObjectToActivateOff();


        onComplete?.Invoke();
    }


    public void PicksUpBrockenGlass(Action onComplete)// kelan
    {
        StartCoroutine(PicksUpBrockenGlassCoroutine(kelanAnimator, "picksUp", onComplete));
    }
    IEnumerator PicksUpBrockenGlassCoroutine(Animator animator, string triggerName, Action onComplete)
    {
        string walking = "Walking";
        GameObject objToMove = kelanAnimator.gameObject;
        Vector3 targetPoint = new Vector3(-0.6f, -1.53f, kelanAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(walking);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(0.3f); // wait for 0.3 seconds or until the animation is done

        yield return BubbleReactionCoroutine();
        animator.SetTrigger("GettingHurt");

        yield return new WaitForSeconds(2f); // wait for 2 seconds or until the animation is done 
        onComplete?.Invoke();
    }

    public void PicksUpElectricWires(Action onComplete)
    {
        StartCoroutine(PicksUpElectricWiresCoroutine(kelanAnimator, "picksUp", onComplete));
    }
    IEnumerator PicksUpElectricWiresCoroutine(Animator animator, string triggerName, Action onComplete)
    {

        GameObject objToMove = kelanAnimator.gameObject;
        Vector3 targetPoint = new Vector3(-2f, -3.5f, kelanAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger("Run");
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {

            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, 2 * Time.deltaTime);
            yield return null;
        }
        animator.SetTrigger(triggerName);


        animator.SetTrigger("Shoked");

        yield return new WaitForSeconds(2f); // wait for 2 seconds or until the animation is done 
        bubbleReactions[1].gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    public void MotherCutWood(Action onComplete)
    {
        StartCoroutine(MotherCutWoodCoroutine(motherAnimator, "cutting", onComplete, Events.CutBranches));
    }
    IEnumerator MotherCutWoodCoroutine(Animator animator, string triggerName, Action onComplete, Events eventType)
    {
        GameObject objToMove = motherAnimator.gameObject;
        Vector3 targetPoint = new Vector3(6.5f, -2.5f, motherAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger("Walk");
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, 2 * Time.deltaTime);
            yield return null;
        }
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(2f);
        treeList[0].ActivateObject();
        WebGLBridge.SendEvent(eventType.ToString());
        onComplete?.Invoke();
    }

    public void GoForWalk(Action onComplete)
    {
        StartCoroutine(GoForWalkCoroutine(motherAnimator, "Walk", onComplete));
    }
    IEnumerator GoForWalkCoroutine(Animator animator, string triggerName, Action onComplete)
    {
        GameObject objToMove = motherAnimator.gameObject;
        motherCanvas.SetActive(true);
        Vector3 targetPoint = new Vector3(12f, -2f, motherAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(triggerName);
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {

            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, 2 * Time.deltaTime);
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void FatherPicksUpBrockenGlass(Action onComplete) // father
    {
        StartCoroutine(FatherPicksUpBrockenGlassCoroutine(fatherAnimator, "picksUp", Events.PickGlass, onComplete));
    }
    IEnumerator FatherPicksUpBrockenGlassCoroutine(Animator animator, string triggerName, Events eventType, Action onComplete)
    {
        string walking = "Walking";
        string idle = "Idle";
        GameObject objToMove = fatherAnimator.gameObject;
        Vector3 targetPoint = new Vector3(2f, -0.5f, fatherAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(walking);

        yield return StartCoroutine(MoveTo(objToMove, targetPoint));
        objToMove.GetComponent<SpriteRenderer>().flipX = false;
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(0.8f); // wait for 0.3 seconds or until the animation is done

        brockenGlassList[0].ActivateObject();
        WebGLBridge.SendEvent(eventType.ToString());

        animator.SetTrigger(walking);
        targetPoint = new Vector3(-11f, -1f, fatherAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));



        brockenGlassList[0].DeactivateObject();
        targetPoint = new Vector3(-6.5f, -0f, fatherAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        yield return StartCoroutine(MoveTo(objToMove, targetPoint));
        animator.SetTrigger(idle);

        onComplete?.Invoke();
    }

    public void FatherPicksUpElectricWires(Action onComplete)
    {
        StartCoroutine(FatherPicksUpElectricWiresCoroutine(fatherAnimator, "picksUp", onComplete));
    }
    IEnumerator FatherPicksUpElectricWiresCoroutine(Animator animator, string triggerName, Action onComplete)
    {
        string walking = "Walking";
        string shock = "DadShock";
        GameObject objToMove = fatherAnimator.gameObject;
        Vector3 targetPoint = new Vector3(-1f, -2.5f, fatherAnimator.transform.position.z);
        FlipObjects(objToMove, targetPoint);
        animator.SetTrigger(walking);

        yield return StartCoroutine(MoveTo(objToMove, targetPoint));
        objToMove.GetComponent<SpriteRenderer>().flipX = false;
        animator.SetTrigger(triggerName);
        yield return new WaitForSeconds(0.4f); // wait for 0.3 seconds or until the animation is done

        animator.SetTrigger(shock);
        yield return new WaitForSeconds(1f); // wait for 0.3 seconds or until the animation is done

        onComplete?.Invoke();
    }

    IEnumerator AllclearCoroutine()
    {
        allClear.SetActive(true);
        yield return new WaitForSeconds(2f); // wait for 3 seconds or until the animation is done 
        WebGLBridge.SendEvent(Events.AllClear.ToString());
        allClear.SetActive(false);
    }




    IEnumerator BubbleReactionCoroutine(int index = 0)
    {
        bubbleReactions[index].gameObject.SetActive(true);
        yield return new WaitForSeconds(1f); // wait for 1 second or until the animation is done 
        bubbleReactions[index].gameObject.SetActive(false);
    }

    public void FlipObjects(GameObject objToFlip, Vector3 targetPoint)
    {
        // Ensure we compare world-space X coordinates (targetPoint is in world space).
        float currentX = objToFlip.transform.position.x;
        bool movingRhight = targetPoint.x > currentX;

        // Try to get SpriteRenderer on the object or its children.
        var sr = objToFlip.GetComponent<SpriteRenderer>();
        if (sr == null) sr = objToFlip.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = movingRhight;
            return;
        }

        // Fallback: flip by inverting localScale.x on the root transform.
        Vector3 ls = objToFlip.transform.localScale;
        ls.x = Math.Abs(ls.x) * (movingRhight ? -1f : 1f);
        objToFlip.transform.localScale = ls;
    }





}
