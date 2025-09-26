using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }
    public bool IsActive { get; private set; }

    [Header("UI Refs")]
    public CanvasGroup dialogueGroup;        // Panel CanvasGroup
    public TextMeshProUGUI dialogueText;     // TMP text field

    [Header("Input")]
    [Tooltip("Space/Enter/MouseLeft/Gamepad South to advance")]
    public float inputDebounce = 0.1f;

    private List<string> currentLines;
    private int index;
    private Action onFinished;

    private InputAction advanceAction;
    private float lastAdvanceTime;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Hide UI on start
        SetVisible(false);

        // Input (no .inputactions asset needed)
        advanceAction = new InputAction("Advance");
        advanceAction.AddBinding("<Keyboard>/space");
        advanceAction.AddBinding("<Keyboard>/enter");
        advanceAction.AddBinding("<Mouse>/leftButton");
        advanceAction.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable()  => advanceAction.Enable();
    void OnDisable() => advanceAction.Disable();

    void Update()
    {
        if (!IsActive) return;

        if (advanceAction.WasPerformedThisFrame() && (Time.unscaledTime - lastAdvanceTime) > inputDebounce)
        {
            lastAdvanceTime = Time.unscaledTime;
            Next();
        }
    }

    public void StartDialogue(List<string> lines, Action onEnd = null)
    {
        if (lines == null || lines.Count == 0) return;

        currentLines = lines;
        index = 0;
        onFinished = onEnd;

        IsActive = true;
        Time.timeScale = 1f; // keep running; we’ll just disable movement
        Cursor.lockState = CursorLockMode.None; // optional: free cursor during dialogue

        SetVisible(true);
        dialogueText.text = currentLines[index];

        // Tell player movement to pause (if present)
        var mover = FindObjectOfType<PlayerController>();
        if (mover) mover.enabled = false;
    }

    private void Next()
    {
        index++;
        if (index >= currentLines.Count)
        {
            EndDialogue();
            return;
        }
        dialogueText.text = currentLines[index];
    }

    private void EndDialogue()
    {
        IsActive = false;
        SetVisible(false);

        // Re-enable movement
        var mover = FindObjectOfType<PlayerController>();
        if (mover) mover.enabled = true;

        // Lock cursor again if you want FPS look
        Cursor.lockState = CursorLockMode.Locked;

        onFinished?.Invoke();

        currentLines = null;
        index = 0;
        onFinished = null;
    }

    private void SetVisible(bool show)
    {
        if (!dialogueGroup) return;
        dialogueGroup.alpha = show ? 1f : 0f;
        dialogueGroup.blocksRaycasts = show;
        dialogueGroup.interactable = show;
    }
}
