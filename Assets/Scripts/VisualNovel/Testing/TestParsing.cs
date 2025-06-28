using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

namespace TESTING{
public class TestParsing : MonoBehaviour
{
    [SerializeField] private TextAsset file;
    void Start()
    {
        SendFileToParse();
    }

    // Update is called once per frame
    void SendFileToParse()
    {
        List<string> lines = FileManager.ReadTextAsset(file, false);

        foreach(string line in lines){
            Dialogue_Line dl = DialogueParser.Parse(line);
        }
    }
}
}
