using UnityEngine;
using TMPro;

// This is just a container for the different objects in the system that need changing like the name and the sprites and the text

namespace DIALOGUE
{
[System.Serializable]
public class DialogueContainer
{
    public GameObject root;
    public NameContainer nameContainer;
    public BackAndCharContainer backAndCharContainer;
    public TextMeshProUGUI dialogueText;
}
}
