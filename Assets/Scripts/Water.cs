using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{

    [SerializeField] GameObject waterSplashPS;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            GameObject splash = Instantiate(
                waterSplashPS,
                other.gameObject.transform.position + Vector3.down * 0.2f,
                new Quaternion(0f, 0f, 0f, 0f)
            );
            AudioManager.instance.Play("Splash");
            StartCoroutine(RemoveSplashPS(splash));

            // TODO: apply low-pass filter
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag == "Player")
        {
            // TODO: remove low-pass filter
        }
    }

    IEnumerator RemoveSplashPS(GameObject splash)
    {
        yield return new WaitForSeconds(5f);
        Destroy(splash);
    }
}
