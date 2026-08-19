using UnityEngine;

public class SuperTakesObjects : MonoBehaviour
{
    [SerializeField] private GameObject sardinesCart;
    [SerializeField] private GameObject waterCart;
    [SerializeField] private GameObject sardinesHeand;
    [SerializeField] private GameObject waterHeand;
    [SerializeField] private GameObject chipsHeand;
    [SerializeField] private GameObject chipsCart;

    [SerializeField] private GameObject cheeseHeand;
    [SerializeField] private GameObject cheeseCart;
    [SerializeField] private GameObject eggsHeand;
    [SerializeField] private GameObject eggsCart;
    [SerializeField] private GameObject chickenHeand;
    [SerializeField] private GameObject chickenCart;
    [SerializeField] private GameObject fishHeand;
    [SerializeField] private GameObject fishCart;

    private string currentObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HandleObjectTaken(string objectName)
    {
        if (objectName == "Sardines")
        {
            sardinesHeand.SetActive(true);
        }
        if (objectName == "Water")
        {
            waterHeand.SetActive(true);
        }
        if (objectName == "Chips")
        {
            chipsHeand.SetActive(true);
        }
        if (objectName == "Cheese")
        {
            cheeseHeand.SetActive(true);
        }
        if (objectName == "Eggs")
        {
            eggsHeand.SetActive(true);
        }
        if (objectName == "Chicken")
        {
            chickenHeand.SetActive(true);
        }
        if (objectName == "Fish")
        {
            fishHeand.SetActive(true);
        }
        currentObject = objectName;
    }
    public void HandleObjectPlaced()
    {
        if (currentObject == "Sardines")
        {
            sardinesCart.SetActive(true);
            sardinesHeand.SetActive(false);
        }
        if (currentObject == "Water")
        {
            waterCart.SetActive(true);
            waterHeand.SetActive(false);
        }
        if (currentObject == "Chips")
        {
            chipsCart.SetActive(true);
            chipsHeand.SetActive(false);
        }
        if (currentObject == "Cheese")
        {
            cheeseCart.SetActive(true);
            cheeseHeand.SetActive(false);
        }
        if (currentObject == "Eggs")
        {
            eggsCart.SetActive(true);
            eggsHeand.SetActive(false);
        }
        if (currentObject == "Chicken")
        {
            chickenCart.SetActive(true);
            chickenHeand.SetActive(false);
        }
        if (currentObject == "Fish")
        {
            fishCart.SetActive(true);
            fishHeand.SetActive(false);
        }

    }
}
