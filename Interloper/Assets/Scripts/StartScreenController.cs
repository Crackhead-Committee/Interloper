using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StartScreenController : MonoBehaviour
{
    [Header("Panels (CanvasGroups)")]
    public CanvasGroup splashPanel;   // logo + "press any button"
    public CanvasGroup titlePanel;    // title image behind menu
    public CanvasGroup menuPanel;     // Start / About / Controls
    public CanvasGroup aboutPanel;    // about text
    public CanvasGroup controlsPanel; // control images

    [Header("Menu Buttons")]
    public Button btnStart;
    public Button btnAbout;
    public Button btnControls;

    [Header("Back Buttons")]
    public Button btnBackMenuPanel;      // from Menu -> Splash
    public Button btnBackAboutPanel;     // from About -> Menu
    public Button btnBackControlsPanel;  // from Controls -> Menu

    [Header("Audio")]
    public AudioSource uiAudio;     // shared 2D AudioSource
    public AudioClip pressAnyClip;  // used once on Splash
    public AudioClip clickClip;     // used on every button click

    [Header("Config")]
    public string gameSceneName = "Game";
    public float fadeDuration = 0.35f;

    // Input
    private InputAction anyPress;

    enum State { Splash, Menu, About, Controls, Transition }
    State state = State.Splash;

    void Awake()
    {
        // Any button/key
        anyPress = new InputAction("AnyPress");
        anyPress.AddBinding("<Keyboard>/anyKey");
        anyPress.AddBinding("<Mouse>/leftButton");
        anyPress.AddBinding("<Mouse>/rightButton");
        anyPress.AddBinding("<Mouse>/middleButton");

        // Initial visibility
        SetActive(titlePanel,   false, 0f);
        SetActive(menuPanel,    false, 0f);
        SetActive(aboutPanel,   false, 0f);
        SetActive(controlsPanel,false, 0f);
        SetActive(splashPanel,  true,  1f);

        // Menu clicks
        if (btnStart)    btnStart.onClick.AddListener(() => { PlayClick(); StartGame(); });
        if (btnAbout)    btnAbout.onClick.AddListener(() => { PlayClick(); ShowAbout(); });
        if (btnControls) btnControls.onClick.AddListener(() => { PlayClick(); ShowControls(); });

        // Back clicks
        if (btnBackMenuPanel)     btnBackMenuPanel.onClick.AddListener(() => { PlayClick(); BackMenuToSplash(); });
        if (btnBackAboutPanel)    btnBackAboutPanel.onClick.AddListener(() => { PlayClick(); BackAboutToMenu(); });
        if (btnBackControlsPanel) btnBackControlsPanel.onClick.AddListener(() => { PlayClick(); BackControlsToMenu(); });

        // Make sure back buttons are only visible where needed
        if (btnBackMenuPanel)     btnBackMenuPanel.gameObject.SetActive(false); // hidden until we're on Menu (if you want it visible there)
        if (btnBackAboutPanel)    btnBackAboutPanel.gameObject.SetActive(false);
        if (btnBackControlsPanel) btnBackControlsPanel.gameObject.SetActive(false);
    }

    void OnEnable()  => anyPress.Enable();
    void OnDisable() => anyPress.Disable();

    void Start()
    {
        state = State.Splash;
    }

    void Update()
    {
        if (state == State.Splash && anyPress.WasPerformedThisFrame())
        {
            if (uiAudio && pressAnyClip) uiAudio.PlayOneShot(pressAnyClip);

            StartCoroutine(SplashToMenu());
        }
    }
    // ───────── Transitions ─────────

    IEnumerator SplashToMenu()
    {
        state = State.Transition;

        // fade out splash
        yield return FadeCanvas(splashPanel, 1f, 0f, fadeDuration);
        SetActive(splashPanel, false, 0f);

        // fade in title behind menu
        SetActive(titlePanel, true, 0f);
        yield return FadeCanvas(titlePanel, 0f, 1f, fadeDuration);

        // fade in menu
        SetActive(menuPanel, true, 0f);
        yield return FadeCanvas(menuPanel, 0f, 1f, fadeDuration);

        // back button for menu (to go back to splash)
        if (btnBackMenuPanel) btnBackMenuPanel.gameObject.SetActive(true);

        state = State.Menu;
        if (btnStart) btnStart.Select();
    }

    void ShowAbout()
    {
        if (state != State.Menu) return;
        StartCoroutine(MenuToAbout());
    }

    IEnumerator MenuToAbout()
    {
        state = State.Transition;

        // hide menu
        yield return FadeCanvas(menuPanel, menuPanel.alpha, 0f, fadeDuration);
        SetActive(menuPanel, false, 0f);
        if (btnBackMenuPanel) btnBackMenuPanel.gameObject.SetActive(false);

        // show about
        SetActive(aboutPanel, true, 0f);
        yield return FadeCanvas(aboutPanel, 0f, 1f, fadeDuration);

        if (btnBackAboutPanel) btnBackAboutPanel.gameObject.SetActive(true);

        state = State.About;
        if (btnBackAboutPanel) btnBackAboutPanel.Select();
    }

    void ShowControls()
    {
        if (state != State.Menu) return;
        StartCoroutine(MenuToControls());
    }

    IEnumerator MenuToControls()
    {
        state = State.Transition;

        // hide menu
        yield return FadeCanvas(menuPanel, menuPanel.alpha, 0f, fadeDuration);
        SetActive(menuPanel, false, 0f);
        if (btnBackMenuPanel) btnBackMenuPanel.gameObject.SetActive(false);

        // show controls
        SetActive(controlsPanel, true, 0f);
        yield return FadeCanvas(controlsPanel, 0f, 1f, fadeDuration);

        if (btnBackControlsPanel) btnBackControlsPanel.gameObject.SetActive(true);

        state = State.Controls;
        if (btnBackControlsPanel) btnBackControlsPanel.Select();
    }

    // Back: Menu -> Splash
    void BackMenuToSplash()
    {
        if (state != State.Menu) return;
        StartCoroutine(MenuToSplash());
    }

    IEnumerator MenuToSplash()
    {
        state = State.Transition;

        // hide menu
        if (btnBackMenuPanel) btnBackMenuPanel.gameObject.SetActive(false);
        yield return FadeCanvas(menuPanel, menuPanel.alpha, 0f, fadeDuration);
        SetActive(menuPanel, false, 0f);

        // hide title
        yield return FadeCanvas(titlePanel, titlePanel.alpha, 0f, fadeDuration);
        SetActive(titlePanel, false, 0f);

        // show splash again
        SetActive(splashPanel, true, 0f);
        yield return FadeCanvas(splashPanel, 0f, 1f, fadeDuration);

        state = State.Splash;

        // play the splash prompt sound again if you want
        if (uiAudio && pressAnyClip) uiAudio.PlayOneShot(pressAnyClip);
    }

    // Back: About -> Menu
    void BackAboutToMenu()
    {
        if (state != State.About) return;
        StartCoroutine(AboutToMenu());
    }

    IEnumerator AboutToMenu()
    {
        state = State.Transition;

        if (btnBackAboutPanel) btnBackAboutPanel.gameObject.SetActive(false);
        yield return FadeCanvas(aboutPanel, aboutPanel.alpha, 0f, fadeDuration);
        SetActive(aboutPanel, false, 0f);

        SetActive(menuPanel, true, 0f);
        yield return FadeCanvas(menuPanel, 0f, 1f, fadeDuration);

        if (btnBackMenuPanel) btnBackMenuPanel.gameObject.SetActive(true);

        state = State.Menu;
        if (btnStart) btnStart.Select();
    }

    // Back: Controls -> Menu
    void BackControlsToMenu()
    {
        if (state != State.Controls) return;
        StartCoroutine(ControlsToMenu());
    }

    IEnumerator ControlsToMenu()
    {
        state = State.Transition;

        if (btnBackControlsPanel) btnBackControlsPanel.gameObject.SetActive(false);
        yield return FadeCanvas(controlsPanel, controlsPanel.alpha, 0f, fadeDuration);
        SetActive(controlsPanel, false, 0f);

        SetActive(menuPanel, true, 0f);
        yield return FadeCanvas(menuPanel, 0f, 1f, fadeDuration);

        if (btnBackMenuPanel) btnBackMenuPanel.gameObject.SetActive(true);

        state = State.Menu;
        if (btnStart) btnStart.Select();
    }

    // Start game
    void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // ───────── Utils ─────────
    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        if (!cg) yield break;
        float t = 0f;
        cg.alpha = from;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
        cg.blocksRaycasts = to > 0.99f;
        cg.interactable = to > 0.99f;
    }

    void SetActive(CanvasGroup cg, bool active, float alphaIfActive)
    {
        if (!cg) return;
        cg.gameObject.SetActive(active);
        cg.alpha = active ? alphaIfActive : 0f;
        cg.blocksRaycasts = active && alphaIfActive > 0.99f;
        cg.interactable = cg.blocksRaycasts;
    }

    void PlayClick()
    {
        if (uiAudio && clickClip) uiAudio.PlayOneShot(clickClip);
    }
}
