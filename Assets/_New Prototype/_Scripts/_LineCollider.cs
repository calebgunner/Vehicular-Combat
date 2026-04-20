using UnityEngine;

public class _LineCollider : MonoBehaviour
{
    public bool playerInCrosshair; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCrosshair = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCrosshair = false;
        }
    }
}
