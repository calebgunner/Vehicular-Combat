using UnityEngine;

public class _MiniMapCamera : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        //FOLLOW THE PLAYER'S X AND Z POSITION, BUT KEEP THE MINIMAP CAMERA'S OWN HEIGHT
        transform.position = new Vector3(player.position.x, transform.position.y, player.position.z);

        //FOLLOW ONLY THE PLAYER'S Y ROTATION
        transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
