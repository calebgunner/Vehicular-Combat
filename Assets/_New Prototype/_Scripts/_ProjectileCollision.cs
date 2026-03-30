using UnityEngine;

public class _ProjectileCollision : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

}
