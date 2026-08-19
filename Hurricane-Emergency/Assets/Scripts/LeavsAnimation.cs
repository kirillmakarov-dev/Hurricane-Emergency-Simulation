using System.Collections.Generic;
using UnityEngine;

public class LeavsAnimation : MonoBehaviour
{
    [SerializeField] private List<GameObject> singleLeafsList; // List of leaf GameObjects to be activated one by one
    private int currentLeafsIndex = 0;
    [SerializeField] private List<GameObject> pileOfLeaves; // List of leaf GameObjects to be activated one by one
    private int currentPileIndex = 0;
    [SerializeField] private ParticleSystem leavsParticleSystem; // Reference to the particle system for the hurricane effect

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void RemoveSingleLeafs() 
    {
        
        if (singleLeafsList != null && singleLeafsList.Count > 0)
        {
            singleLeafsList[currentLeafsIndex].SetActive(false);
            currentLeafsIndex = (currentLeafsIndex + 1) % singleLeafsList.Count;
            leavsParticleSystem.Play();
            AddPileOfLeaves();
        }
    }

    public void AddPileOfLeaves() 
    {
        if (pileOfLeaves != null && pileOfLeaves.Count > 0)
        {
            pileOfLeaves[currentPileIndex].SetActive(true);
            currentPileIndex = (currentPileIndex + 1) % pileOfLeaves.Count;
            if(currentPileIndex == 3)
            {
                pileOfLeaves[3].SetActive(true);
            }
        }
    }
}
