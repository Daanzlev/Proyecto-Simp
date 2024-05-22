using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, Interactable
{
    //public string[] dialogue;
    //public string name;

    public void Interact()
    {
        Debug.Log("Interacting with NPC");
        //DialogueSystem.Instance.AddNewDialogue(dialogue, name);
    }
}
