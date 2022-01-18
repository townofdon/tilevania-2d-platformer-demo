using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] GameObject livesContainer;
    [SerializeField] Slider healthbarSlider;
    [SerializeField] Image healthbarFill;
    [SerializeField] Image healthbarFillBG;
    [SerializeField] Gradient healthbarGrad;

    [Header("Weapons")]
    [SerializeField] Image bowAndArrow;

    [Header("Coins")]
    [SerializeField] TextMeshProUGUI coins;

    // cached
    Image[] lives;

    // singleton
    private static PlayerUI _instance;
    public static PlayerUI instance {
        get {
            AppIntegrity.AssertPresent<PlayerUI>(_instance);
            return _instance;
        }
    }

    void Awake() {
        // singleton pattern
        if (_instance != null) {
            if (_instance != this) { Destroy(gameObject); }
            return;
        }

        DontDestroyOnLoad(gameObject);
        _instance = this;
    }

    void Start() {
        AppIntegrity.AssertPresent<GameObject>(livesContainer);
        AppIntegrity.AssertPresent<Slider>(healthbarSlider);
        AppIntegrity.AssertPresent<Image>(healthbarFill);
        AppIntegrity.AssertPresent<Image>(healthbarFillBG);
        AppIntegrity.AssertPresent<Image>(bowAndArrow);
        AppIntegrity.AssertPresent<TextMeshProUGUI>(coins);
        InitLives();
    }

    void InitLives() {
        if (lives == null) {
            lives = livesContainer.transform.GetComponentsInChildren<Image>();
        }
    }

    public void SetHealth(float health) {
        healthbarSlider.value = health;
        Color c = healthbarGrad.Evaluate(healthbarSlider.normalizedValue);
        healthbarFill.color = c;
        c.a = 0.1f;
        healthbarFillBG.color = c;
    }

    public void SetLives(int numLives) {
        InitLives();
        for (int i = 0; i < lives.Length; i++)
        {
            if (i < numLives)
                lives[i].enabled = true;
            else
                lives[i].enabled = false;
        }
    }

    public void SetHasWeapon(bool hasWeapon) {
        if (hasWeapon)
            bowAndArrow.enabled = true;
        else
            bowAndArrow.enabled = false;
    }

    public void SetNumCoins(int numCoins) {
        coins.text = (numCoins % 100).ToString();
    }
}
