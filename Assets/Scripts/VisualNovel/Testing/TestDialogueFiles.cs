using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset file;
    void Start()
    {
        StartConversation();
    }

    // Update is called once per frame
    void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset(file, false);

        DialogueSystem.instance.Say(lines);
    }
}
