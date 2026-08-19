using System.Collections.Generic;
using UnityEngine;

public class LeavsAnimationEvent : MonoBehaviour
{
    [SerializeField] private List<LeavsAnimation> pileOfLeaves; // Reference to the LeavsAnimation script
    public int currentLeavsAnimationIndex = 0;
    public int currentPileIndex = 0;
    public int animationCounter = 0;


    void Start()
    {

    }

    public void ActivateLeavsAnimation() // This method will be called by the animation event to activate the leaf animations one by one
    {
        if (pileOfLeaves != null && pileOfLeaves.Count > 0)
        {
            if (currentPileIndex < pileOfLeaves.Count)
            {
                pileOfLeaves[currentPileIndex].RemoveSingleLeafs();
            }

            currentLeavsAnimationIndex++;
            WebGLBridge.SendEvent(Events.CleanYard.ToString());
            if (currentLeavsAnimationIndex == 3)
            {
                currentLeavsAnimationIndex = 0;
                animationCounter++;
                if (animationCounter == 2)
                {
                    GetComponent<Animator>().SetTrigger("Standing");
                }
                currentPileIndex = (currentPileIndex + 1) % pileOfLeaves.Count;
            }
        }
    }


}
