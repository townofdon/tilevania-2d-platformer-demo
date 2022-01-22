using System.Collections;
using UnityEngine;

public class WeaponBow : MonoBehaviour
{
    PlayerMovement playerMovement;

    bool acquired = false;

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        AppIntegrity.AssertPresent<PlayerMovement>(playerMovement);
        AppIntegrity.AssertPresent<PauseMenu>(PauseMenu.instance);
        AppIntegrity.AssertPresent<AudioManager>(AudioManager.instance);
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (acquired) return;

        if (other.gameObject.tag == "Player")
        {
            acquired = true;

            if (playerMovement.AcquireWeaponBow()) {
                StartCoroutine(AcquireWeapon());
            } else {
                AudioManager.instance.Play("MenuSelect");
                Destroy(gameObject);
            }
        }
    }

    IEnumerator AcquireWeapon()
    {
        playerMovement.PlayParticleEffect();
        Time.timeScale = 0f;
        AudioManager.instance.PauseMusic();
        AudioManager.instance.Play("GetWeapon");

        yield return new WaitForSecondsRealtime(1f);

        if (!PauseMenu.instance.IsPaused) Time.timeScale = 1f;
        playerMovement.StopParticleEffect();
        playerMovement.RefreshUI();
        AudioManager.instance.UnPauseMusic();

        Destroy(gameObject);
    }
}
