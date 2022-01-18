using UnityEngine;

public class Coin : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            GameSession.instance.ProcessAcquireCoin();
            Destroy(gameObject);
        }
    }
}
