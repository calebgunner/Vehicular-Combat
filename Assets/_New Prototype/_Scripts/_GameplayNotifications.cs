using UnityEngine;

public class _GameplayNotifications : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("DeactivateObject", 5);
    }

    void DeactivateObject()
    {
        gameObject.SetActive(false);
    }
}
