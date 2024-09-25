using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueLoader : MonoBehaviour
{
    private DialogueObject LoadedDialogue;

    [Header("Settings")]
    public KeyCode ProgressKey;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI Name;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void StartDialogue(DialogueObject newDialogue)
    {
        gameObject.SetActive(true);
        LoadedDialogue = newDialogue;
        ExecuteDialogueNode();
    }

    void ExecuteDialogueNode()
    {
        Text.text = LoadedDialogue.Text;
        Name.text = LoadedDialogue.Name;
        if(LoadedDialogue.Next.Length <= 1)
            StartCoroutine(WaitForNextKey());
    }

    IEnumerator WaitForNextKey()
    {
        while (Input.GetKeyDown(ProgressKey)) { yield return null; }
        while (!Input.GetKeyDown(ProgressKey)) { yield return null; }
        Debug.Log(LoadedDialogue.Next.Length);
        if (LoadedDialogue.Next.Length == 1)
        {
            LoadedDialogue = LoadedDialogue.Next[0];
            ExecuteDialogueNode();
        }
        else if(LoadedDialogue.Next.Length == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
