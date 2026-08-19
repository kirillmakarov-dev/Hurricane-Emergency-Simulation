using UnityEngine;

public class ScineSwicher : MonoBehaviour
{
    public GameObject nextObjectToSwitch;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SwitchToNextObject()
    {
        nextObjectToSwitch.SetActive(true);
        gameObject.SetActive(false);
    }

   
}
