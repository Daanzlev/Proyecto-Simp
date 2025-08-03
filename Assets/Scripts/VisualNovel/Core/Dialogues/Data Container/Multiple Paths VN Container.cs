using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]

public class MultiplePathsVNContainer
{
    [SerializeField] public string name;
    [SerializeField] public TextAsset textFile;
    [SerializeField] public Dialog dialog;
    [SerializeField] public Choice[] pathOptions;

    // Struct to contain the options a path can go to next, trigger choic by command
    [System.Serializable]
    public struct Choice
    {
        public string pathName;
        public string buttonDialogue;
        public UnityEvent onChoose;
    }

    private void UseInput()
    {
        if (textFile != null)
        {
            // Prefer text file for now 
            Debug.Log("Using text file");
            dialog = null;
        }
        else if (dialog != null)
        {
            Debug.Log("Using Dialog");
        }
    }
}
