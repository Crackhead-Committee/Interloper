using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea] public List<string> lines = new List<string>
    {
        "Test 1",
        "Test 2",
        "Test 3"
    };
}
