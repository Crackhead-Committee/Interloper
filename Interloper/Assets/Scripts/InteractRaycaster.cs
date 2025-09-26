using UnityEngine;
using UnityEngine.InputSystem;

public class InteractRaycaster : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactMask = ~0; // everything by default

    private InputAction interactAction;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main;

        interactAction = new InputAction("Interact");
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonSouth"); // A/Cross
    }

    void OnEnable()  => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        if (DialogueController.Instance && DialogueController.Instance.IsActive)
            return; // ignore interactions while talking

        if (!interactAction.WasPerformedThisFrame()) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent<DialogueTrigger>(out var trigger))
            {
                DialogueController.Instance.StartDialogue(trigger.lines);
            }
        }
    }
}
