using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [TextArea] public string promptText = "Interact";
    public Sprite promptIcon;
    public float maxDistance = 3f;
    public UnityEvent onInteract;

    public void Interact() => onInteract?.Invoke();
}
