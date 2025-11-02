using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class InteractRaycaster : MonoBehaviour
{
    [Header("Raycast")]
    public Camera playerCamera;
    public float defaultDistance = 3f;
    public LayerMask interactMask = ~0;

    [Header("Prompt UI")]
    public CanvasGroup promptGroup;   // panel CanvasGroup (alpha fades)
    public Image promptIcon;          // optional
    public TMP_Text promptText;       // e.g. "[E] Open"
    public float fadeSpeed = 12f;
    public Vector2 screenOffset = new Vector2(0, -80); // if you position via world-space later

    InputAction interactAction;
    Interactable current;
    float targetAlpha;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main;

        interactAction = new InputAction("Interact");
        interactAction.AddBinding("<Keyboard>/e");
    }

    void OnEnable()  => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        if (DialogueController.Instance && DialogueController.Instance.IsActive)
        {
            SetCurrent(null);
            Fade(0f);
            return;
        }

        Interactable found = null;
        float foundDist = float.MaxValue;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out var hit, 100f, interactMask, QueryTriggerInteraction.Ignore))
        {
            // Prefer a proper Interactable component
            if (hit.collider.TryGetComponent(out Interactable it))
            {
                float maxDist = it.maxDistance > 0 ? it.maxDistance : defaultDistance;
                float d = Vector3.Distance(playerCamera.transform.position, hit.point);
                if (d <= maxDist) { found = it; foundDist = d; }
            }
            // Backward-compat: your old RadioDialogue targets still work without adding Interactable
            else if (hit.collider.TryGetComponent(out RadioDialogue radio))
            {
                // Create a transient prompt using defaults
                // (Better: add Interactable to those objects and wire onInteract to radio.Interact)
                found = null; // keep current null; show a generic prompt instead
                if (promptText) promptText.text = "[E] Interact";
                if (promptIcon) promptIcon.sprite = null;
                Fade(1f);

                if (interactAction.triggered)
                    radio.Interact();

                return;
            }
        }

        if (found != current)
            SetCurrent(found);

        Fade(current ? 1f : 0f);

        if (current != null && interactAction.triggered)
            current.Interact();
    }

    void SetCurrent(Interactable it)
    {
        current = it;
        if (!promptGroup) return;

        if (current)
        {
            if (promptText) promptText.text = string.IsNullOrWhiteSpace(current.promptText) ? "[E] Interact" : current.promptText;
            if (promptIcon) promptIcon.sprite = current.promptIcon;
        }
        else
        {
            if (promptText) promptText.text = "";
            if (promptIcon) promptIcon.sprite = null;
        }
    }

    void Fade(float to)
    {
        if (!promptGroup) return;
        promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, to, fadeSpeed * Time.deltaTime);
    }
}
