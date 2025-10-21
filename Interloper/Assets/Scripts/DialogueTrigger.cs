using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Serializable]
    public class DialogueEvent
    {
        [TextArea] public List<string> lines = new List<string>();
    }

    [Header("Dialogue Moments (in order)")]
    public List<DialogueEvent> moments = new List<DialogueEvent>();

    [SerializeField, Tooltip("Current index into 'moments'.")]
    private int currentIndex = 0;

    public bool HasCurrent =>
        currentIndex >= 0 &&
        currentIndex < moments.Count &&
        moments[currentIndex] != null &&
        moments[currentIndex].lines != null &&
        moments[currentIndex].lines.Count > 0;

    public List<string> GetCurrentLines()
    {
        return HasCurrent ? moments[currentIndex].lines : null;
    }

    /// <summary>Advance to next moment. Returns true if a next moment exists after advancing.</summary>
    public bool Advance()
    {
        currentIndex++;
        return currentIndex >= 0 && currentIndex < moments.Count && HasCurrent;
    }

    public bool HasAnyRemaining()
    {
        if (currentIndex < 0 || currentIndex >= moments.Count) return false;

        for (int i = currentIndex; i < moments.Count; i++)
        {
            var m = moments[i];
            if (m != null && m.lines != null && m.lines.Count > 0) return true;
        }
        return false;
    }

    public void ResetIndex(int start = 0)
    {
        currentIndex = Mathf.Clamp(start, 0, Mathf.Max(0, moments.Count - 1));
    }
}
