using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class RadioDialogue : MonoBehaviour
{
    [Header("Refs")]
    public AudioSource staticSource;
    public DialogueTrigger dialogue;

    [Header("Settings")]
    public bool dialogueAvailableOnStart = true;
    public string playerTag = "Player";

    InputAction _interactAction;

    bool _playerInRange;
    bool _dialogueAvailable;

    void Awake()
    {
        if (!staticSource) staticSource = GetComponent<AudioSource>();
        if (staticSource) { staticSource.loop = true; staticSource.playOnAwake = false; }

        _interactAction = new InputAction("Interact");
        _interactAction.AddBinding("<Keyboard>/e");
        _interactAction.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable()  => _interactAction.Enable();
    void OnDisable() => _interactAction.Disable();

    void Start()
    {
        _dialogueAvailable = dialogueAvailableOnStart;
        ApplyStaticFromFlag();
    }

    void Update()
    {
        if (DialogueController.Instance && DialogueController.Instance.IsActive)
            return;

        if (!_playerInRange) return;
        if (!_dialogueAvailable) return;
        if (dialogue == null || !dialogue.HasAnyRemaining())
        {
            SetDialogueAvailable(false);
            return;
        }

        if (_interactAction.WasPerformedThisFrame())
            TriggerDialogue();
    }

    void TriggerDialogue()
    {
        if (staticSource && staticSource.isPlaying)
            staticSource.Stop();

        var lines = dialogue ? dialogue.GetCurrentLines() : null;

        if (lines == null || lines.Count == 0)
        {
            OnMomentFinished();
            return;
        }

        if (DialogueController.Instance)
            DialogueController.Instance.StartDialogue(lines, OnMomentFinished);
        else
            OnMomentFinished();
    }

    void OnMomentFinished()
    {
        bool hasNext = dialogue && dialogue.Advance();

        SetDialogueAvailable(hasNext);
    }

    public void SetDialogueAvailable(bool available)
    {
        _dialogueAvailable = available;
        ApplyStaticFromFlag();
    }

    void ApplyStaticFromFlag()
    {
        if (!staticSource) return;

        if (_dialogueAvailable)
        {
            if (!staticSource.isPlaying) staticSource.Play();
        }
        else
        {
            if (staticSource.isPlaying) staticSource.Stop();
        }
    }

    public void Interact()
    {
        if (DialogueController.Instance && DialogueController.Instance.IsActive) return;

        if (!_dialogueAvailable)
            return;

        if (dialogue == null || !dialogue.HasAnyRemaining())
        {
            SetDialogueAvailable(false);
            return;
        }
        TriggerDialogue();
    }

    void StopStatic()
    {
        if (staticSource && staticSource.isPlaying) staticSource.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            _playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            _playerInRange = false;
    }
}
