using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestButton : MonoBehaviour
{
    public DialogueObject myDialogue;

    public void Execute()
    {
        DialogueLoader DL = GameObject.FindObjectOfType<DialogueLoader>(true);
        DL.StartDialogue(myDialogue);
    }
}
