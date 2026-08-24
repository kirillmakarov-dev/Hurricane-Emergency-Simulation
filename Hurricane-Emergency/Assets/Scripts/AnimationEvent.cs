using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public List<GameObject> planksList;
    private int currentPlanksIndex = 0;

    [SerializeField] private GameObject textCanvas; // Reference to the second animation GameObject

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ChangeAnimation(string animationName)
    {
        Debug.Log("Animation event triggered: ChangeAnimation");
        Animator animator = GetComponent<Animator>();
        animator.enabled = true;
        animator.SetTrigger(animationName);
    }

    public void ActivatePlanks() // This method will be called by the animation event to activate the planks one by one
    {
        if (planksList != null && planksList.Count > 0)
        {
            planksList[currentPlanksIndex].SetActive(true);
            currentPlanksIndex++;
            WebGLBridge.SendEvent(Events.CollectPlywood.ToString());
            if (currentPlanksIndex >= planksList.Count)
            {
               
                StartCoroutine(DisableAnimator());
            }
        }
    }

    IEnumerator DisableAnimator()
    {
        yield return new WaitForSeconds(0.4f); // Wait for 0.4 seconds before disabling the animator
        Debug.Log("Disabling animator after activating the last plank");
        GetComponent<Animator>().enabled = false; // Disable the animator after activating the last plank
    }

    public void ActivateJune1() // This method will be called by the animation event to activate the June 1 animation
    {
        Debug.Log("Activating June 1");
        WebGLBridge.OnJuneArrives(ObjectsHolder.instance.GetJune1ID());
        WebGLBridge.SendEvent(Events.JuneFirst.ToString());

        if (SimulationManager.Instance != null && SimulationManager.Instance.CurrentMode == ModeName.House)
        {
            HouseMod house = SimulationManager.Instance.GetMode<HouseMod>();
            house?.HandleJune1AnimationCompleted();
        }
    }


    public void ActivateMay1() // This method will be called by the animation event to activate the May 1 animation
    {
        WebGLBridge.SendEvent(Events.MayArrives.ToString());
        WebGLBridge.OnMayArrives(ObjectsHolder.instance.GetMayID());

        if (SimulationManager.Instance != null && SimulationManager.Instance.CurrentMode == ModeName.House)
        {
            HouseMod house = SimulationManager.Instance.GetMode<HouseMod>();
            house?.HandleMayArrivalForConfiguredLesson();
        }
    }

   

    public void ActivateTextCanvas() // This method will be called by the animation event to activate the text canvas
    {
        Debug.Log("Activating text canvas");
        if (textCanvas != null)
        {
            textCanvas.SetActive(true);
        }
    }
    public void TurnOffTextCanvas() // This method will be called by the animation event to activate the text canvas
    {
        if (textCanvas != null)
        {
            textCanvas.SetActive(false);
            gameObject.SetActive(false); // Deactivate the entire GameObject after turning off the text canvas          
        }
        HouseMod houseMod = FindFirstObjectByType<HouseMod>();
        if (houseMod != null)
        {
            houseMod.canPlayEmergencyPlan = true; // Reset the flag to allow the emergency plan to be shown again
        }
    }

    public void WondowPlanks() // This method will be called by the animation event to activate the planks one by one
    {
        if (planksList != null && planksList.Count > 0)
        {
            planksList[currentPlanksIndex].SetActive(true);
            currentPlanksIndex++;
            WebGLBridge.SendEvent(Events.CoverWindow.ToString());
        }
    }


}
