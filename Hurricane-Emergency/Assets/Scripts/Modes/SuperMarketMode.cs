using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationsInSuper
{
    Tosupermarket,
    GetCannedFood,
    GetCrackers,
    GetWater,
    GetCheese,
    GetEggs,
    GetChicken,
    GetFish,
    Announcement,
    WayToSupermarketAnimation
}

public class SuperMarketMode : MonoBehaviour, ISimulationMode
{
    [SerializeField] private GameObject wayToSupermarket; // Reference to the second animation GameObject  
    [SerializeField] private GameObject supermarketMainObject; // Reference to the second animation GameObject
    [SerializeField] private GameObject momAnimator; // Reference to another GameObject
    [SerializeField] private Animator carAnimator; // Reference to another GameObject
    [SerializeField] private Animator carAnimationInSuper; // Reference to another GameObject
    public bool checkanimation;
    float scale;
    private Animator animator;
    public AnimationsInSuper animationToPlay; // Variable to specify which animation to play

    private readonly Queue<AnimationsInSuper> animationQueue = new();
    private bool isPlaying;
    private bool currentAnimationFinished;
    private AnimationsInSuper? currentQueuedAnimation;

    private Dictionary<AnimationsInSuper, Action<Action>> animationMap;


    public bool timerRun = false;
    public float calendarTimer = 5f;



    private void Awake()
    {
        animator = momAnimator.GetComponent<Animator>();

        animationMap = new Dictionary<AnimationsInSuper, Action<Action>>()
        {
            { AnimationsInSuper.GetChicken, OnGetChicken },
            { AnimationsInSuper.GetCrackers, OnGetCrackers },
            { AnimationsInSuper.GetCannedFood, OnGetCannedFood },
            { AnimationsInSuper.GetWater, OnGetWater },
            { AnimationsInSuper.GetCheese, OnGetCheese },
            { AnimationsInSuper.GetEggs, OnGetEggs },
            { AnimationsInSuper.GetFish, OnGetFish },
            { AnimationsInSuper.Tosupermarket, OnGoToSupermarket },
            { AnimationsInSuper.Announcement, PlayRadioAnnouncement },
            { AnimationsInSuper.WayToSupermarketAnimation, WayToSupermarketAnimation }
        };
    }
    public void SuperQueueAnimation(string animationName) // This method can be called to add animations to the queue from the web interface
    {
        Debug.Log($"Unity - added animation from web: {animationName}");

        if (string.IsNullOrWhiteSpace(animationName))
        {
            Debug.LogWarning("Empty animation name");
            return;
        }

        animationName = animationName.Trim();

        if (Enum.TryParse(animationName, true, out AnimationsInSuper animation) &&
            Enum.IsDefined(typeof(AnimationsInSuper), animation))
        {
            if (currentQueuedAnimation == animation || animationQueue.Contains(animation))
            {
                Debug.Log($"Animation already queued or playing: {animation}");
                return;
            }

            animationQueue.Enqueue(animation);
            Debug.Log($"Animation added to queue: {animation}");

            if (!isPlaying)
            {
                StartCoroutine(ProcessQueue());
            }
        }
        else
        {
            Debug.LogWarning($"Unity - No such animation in enum Animations: {animationName}");
        }
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (animationQueue.Count > 0)
        {
            AnimationsInSuper nextAnimation = animationQueue.Dequeue();
            currentQueuedAnimation = nextAnimation;

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

            currentQueuedAnimation = null;
        }

        isPlaying = false;
    }



    void Update()
    {
        if (timerRun == true)
        {
            if (calendarTimer <= 0f)
            {
                timerRun = false;
                WebGLBridge.OnJuneArrives(ObjectsHolder.instance.GetJune1ID());
                WebGLBridge.SendEvent(Events.JuneFirst.ToString());

                // SuperQueueAnimation(AnimationsInSuper.Announcement.ToString());

            }
            else if (calendarTimer > 0)
            {
                calendarTimer -= Time.deltaTime;
            }
        }



        if (checkanimation == true)
        {
            FoodAnimation(animationToPlay);
            checkanimation = false;
        }

    }
    void Start()
    {
        scale = momAnimator.transform.localScale.x;
    }
    public void Initialize()
    {
        Debug.Log("Initialize SuperMarket mod");
    }


    public void OnSimulationStart()
    {
        Debug.Log("start SuperMarket mod");
        carAnimator.gameObject.SetActive(true);
        // wayToSupermarket.SetActive(true);
        // Invoke(nameof(ActivateSuperMarketMainObject), 11f); // Call the method after a delay of 15 seconds
    }

    public void PlayRadioAnnouncement(Action onComplete)
    {
        //SuperQueueAnimation(AnimationsInSuper.WayToSupermarketAnimation.ToString());
        Debug.Log("Playing radio announcement in SuperMarketMode");
        StartCoroutine(AnouncementCoroutine(onComplete));
        // SuperQueueAnimation(AnimationsInSuper.GetCannedFood.ToString()); // check ///
    }
    IEnumerator AnouncementCoroutine(Action onComplete)
    {
        carAnimator.SetTrigger("Announcement");

        yield return StartCoroutine(WaitForStateToFinish(carAnimator, "Car Announcement"));
        Debug.Log("Car announcement animation finished");
        onComplete?.Invoke();
    }


    public void WayToSupermarketAnimation(Action onComplete)
    {
        StartCoroutine(WayToSupermarket(onComplete));
    }

    IEnumerator WayToSupermarket(Action onComplete)
    {
        wayToSupermarket.SetActive(true);
        yield return StartCoroutine(WaitForStateToFinish(carAnimationInSuper, "Car Animation"));
        // yield return new WaitForSeconds(10f);
        WebGLBridge.SendEvent(Events.GoToSupermarket.ToString());
        wayToSupermarket.SetActive(false);
        supermarketMainObject.SetActive(true);
        onComplete?.Invoke();
    }


    public void OnSimulationEnd()
    {
        Debug.Log("end SuperMarket mod");
    }

    public void Cleanup()
    {
        Debug.Log("Cleanup SuperMarket mod");
    }
    public void OnGoToSupermarket(Action onComplete) // This method can be called to trigger the transition to the supermarket mode
    {
        // StartCoroutine(Tosupermarket(onComplete));
    }

    // IEnumerator Tosupermarket(Action onComplete)
    // {
    //     //SimulationManager.Instance.SwitchMode(ModeName.SuperMarket);
    //     yield return new WaitForSeconds(7f);
    //     WebGLBridge.SendEvent(Events.GoToSupermarket.ToString());
    //     yield return new WaitForSeconds(5f);
    //     onComplete?.Invoke();
    // }

    public void OnGetCannedFood(Action onComplete)
    {
        StartCoroutine(GetCannedFood(momAnimator, new Vector3(1f, 0f, 0f), 2f, true, onComplete));

    }

    public void OnGetCrackers(Action onComplete)
    {
        StartCoroutine(GetCrackers(momAnimator, new Vector3(1f, 0f, 0f), 2f, true, onComplete));
    }

    public void OnGetChicken(Action onComplete)
    {
        StartCoroutine(GetChicken(momAnimator, new Vector3(4f, 0f, 0f), 2f, true, onComplete));
    }
    public void OnGetCheese(Action onComplete)
    {
        StartCoroutine(GetCheese(momAnimator, new Vector3(5f, 0f, 0f), 2f, true, onComplete));
    }

    public void OnGetFish(Action onComplete)
    {
        StartCoroutine(GetFish(momAnimator, new Vector3(4f, 0f, 0f), 2f, true, onComplete));
    }

    public void FoodAnimation(AnimationsInSuper animationToPlay)
    {
        Debug.Log("FoodAnimation called with animation: " + animationToPlay);
        switch (animationToPlay)
        {
            case AnimationsInSuper.GetCannedFood:
                OnGetCannedFood(() => { });
                break;
            case AnimationsInSuper.GetCrackers:
                OnGetCrackers(() => { });
                break;
            case AnimationsInSuper.GetWater:
                OnGetWater(() => { });
                break;
            case AnimationsInSuper.GetCheese:
                OnGetCheese(() => { });
                break;
            case AnimationsInSuper.GetEggs:
                OnGetEggs(() => { });
                break;
            case AnimationsInSuper.GetChicken:
                OnGetChicken(() => { });
                break;
            case AnimationsInSuper.GetFish:
                OnGetFish(() => { });
                break;
            default:
                break;
        }
    }

    public void OnGetWater(Action onComplete)
    {
        StartCoroutine(PlayGetWaterSequence(onComplete));
    }

    private IEnumerator PlayGetWaterSequence(Action onComplete)
    {
        Animator anim = momAnimator.GetComponent<Animator>();

        anim.SetTrigger("GetWater");
        yield return StartCoroutine(WaitForStateToFinish(anim, "GetWater"));


        anim.SetTrigger("CartR");
        yield return StartCoroutine(WaitForStateToFinish(anim, "CartR"));
        WebGLBridge.SendEvent(Events.GetWater.ToString());

        onComplete?.Invoke();
    }

    public void OnGetEggs(Action onComplete)
    {
        StartCoroutine(PlayGetEggsSequence(onComplete));
    }

    private IEnumerator PlayGetEggsSequence(Action onComplete)
    {
        Animator anim = momAnimator.GetComponent<Animator>();

        anim.SetTrigger("GetEggs");
        yield return StartCoroutine(WaitForStateToFinish(anim, "GetEggs"));

        anim.SetTrigger("CartL");
        yield return StartCoroutine(WaitForStateToFinish(anim, "CartL"));

        onComplete?.Invoke();
    }
    private IEnumerator WaitForStateToFinish(Animator animator, string stateName, int layer = 0)
    {
        yield return null;

        while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
            yield return null;

        while (animator.IsInTransition(layer))
            yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);

        while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 1f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        }
    }


    private IEnumerator GetCannedFood(GameObject objToMove, Vector3 endPoint, float speed, bool isLeftSide = false, Action onComplete = null)
    {
        Vector3 startPoint = objToMove.transform.position;
        objToMove.transform.localScale = new Vector3(scale * (isLeftSide ? -1f : 1f), scale, scale); // Flip the sprite if it's on the left side
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        Vector3 targetPoint = endPoint;
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
        objToMove.transform.localScale = new Vector3(scale, scale, scale); // Reset the sprite scale to normal
        objToMove.GetComponent<Animator>().SetTrigger("GetCannedFood");

        yield return new WaitForSeconds(2f); // Wait for the "GetCannedFood" animation to finish
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        while (Vector3.Distance(objToMove.transform.position, startPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, startPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.GetComponent<Animator>().SetTrigger("CartL");
        yield return new WaitForSeconds(2f); // Optional: wait a bit before calling onComplete
        WebGLBridge.SendEvent(Events.GetCannedFood.ToString());
        onComplete?.Invoke();
    }
    private IEnumerator GetCrackers(GameObject objToMove, Vector3 endPoint, float speed, bool isLeftSide = false, Action onComplete = null)
    {
        Vector3 startPoint = objToMove.transform.position;
        objToMove.transform.localScale = new Vector3(scale * (isLeftSide ? -1f : 1f), scale, scale); // Flip the sprite if it's on the left side
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        Vector3 targetPoint = endPoint;
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
        objToMove.transform.localScale = new Vector3(scale, scale, scale); // Reset the sprite scale to normal
        objToMove.GetComponent<Animator>().SetTrigger("GetCrackers");



        yield return new WaitForSeconds(1f); // Wait for the "GetCrackers" animation to finish
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        while (Vector3.Distance(objToMove.transform.position, startPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, startPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.GetComponent<Animator>().SetTrigger("CartR");
        yield return new WaitForSeconds(2f); // Optional: wait a bit before calling onComplete
        WebGLBridge.SendEvent(Events.GetCrackers.ToString());
        onComplete?.Invoke();

    }

    private IEnumerator GetCheese(GameObject objToMove, Vector3 endPoint, float speed, bool isLeftSide = false, Action onComplete = null)
    {
        Vector3 startPoint = objToMove.transform.position;
        objToMove.transform.localScale = new Vector3(scale * (isLeftSide ? -1f : 1f), scale, scale); // Flip the sprite if it's on the left side
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        Vector3 targetPoint = endPoint;
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
        objToMove.transform.localScale = new Vector3(scale, scale, scale); // Reset the sprite scale to normal
        objToMove.GetComponent<Animator>().SetTrigger("GetCheese");
        yield return new WaitForSeconds(2f); // Wait for the "GetCheese" animation to finish
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        while (Vector3.Distance(objToMove.transform.position, startPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, startPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.GetComponent<Animator>().SetTrigger("CartL");
        yield return new WaitForSeconds(2f); // Optional: wait a bit before calling onComplete
        onComplete?.Invoke();
    }

    private IEnumerator GetChicken(GameObject objToMove, Vector3 endPoint, float speed, bool isLeftSide = false, Action onComplete = null)
    {
        Vector3 startPoint = objToMove.transform.position;
        objToMove.transform.localScale = new Vector3(scale * (isLeftSide ? -1f : 1f), scale, scale); // Flip the sprite if it's on the left side
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        Vector3 targetPoint = endPoint;
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
        objToMove.transform.localScale = new Vector3(scale, scale, scale); // Reset the sprite scale to normal
        objToMove.GetComponent<Animator>().SetTrigger("GetChicken");
        yield return new WaitForSeconds(2f); // Wait for the "GetChicken" animation to finish
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        while (Vector3.Distance(objToMove.transform.position, startPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, startPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.GetComponent<Animator>().SetTrigger("CartL");
        yield return new WaitForSeconds(2f); // Optional: wait a bit before calling onComplete
        onComplete?.Invoke();
    }
    private IEnumerator GetFish(GameObject objToMove, Vector3 endPoint, float speed, bool isLeftSide = false, Action onComplete = null)
    {
        Vector3 startPoint = objToMove.transform.position;
        objToMove.transform.localScale = new Vector3(scale * (isLeftSide ? -1f : 1f), scale, scale); // Flip the sprite if it's on the left side
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        Vector3 targetPoint = endPoint;
        while (Vector3.Distance(objToMove.transform.position, targetPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, targetPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.transform.position = targetPoint;
        objToMove.transform.localScale = new Vector3(scale, scale, scale); // Reset the sprite scale to normal
        objToMove.GetComponent<Animator>().SetTrigger("GetFish");
        yield return new WaitForSeconds(2f); // Wait for the "GetFish" animation to finish
        objToMove.GetComponent<Animator>().SetTrigger("Walk");
        while (Vector3.Distance(objToMove.transform.position, startPoint) > 0.01f)
        {
            objToMove.transform.position = Vector3.MoveTowards(objToMove.transform.position, startPoint, speed * Time.deltaTime);
            yield return null;
        }
        objToMove.GetComponent<Animator>().SetTrigger("CartL");
        yield return new WaitForSeconds(2f); // Optional: wait a bit before calling onComplete
        onComplete?.Invoke();
    }


}
