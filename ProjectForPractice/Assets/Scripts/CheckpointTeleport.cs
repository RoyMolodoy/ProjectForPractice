using UnityEngine;

public class CheckpointTeleport : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;
        Debug.Log("Player entered checkpoint trigger");
        Rigidbody2D rb = other.attachedRigidbody;

        if (rb != null)
        {
            rb.angularVelocity = 0f;

            rb.position = respawnPoint.position;
            //rb.Sleep();
        }
        else
        {
            other.transform.position = respawnPoint.position;
        }
    }
}