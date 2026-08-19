using UnityEngine;

public class CanvasFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // The target to follow (e.g., the player)
    [SerializeField] private Vector3 offset; // Offset from the target's position
    void Start()
    {
        
    }

    // Update is called once per frame


    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}
