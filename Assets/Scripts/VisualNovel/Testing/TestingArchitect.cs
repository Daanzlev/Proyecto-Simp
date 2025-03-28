using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

namespace TESTING{
    public class TestingArchitect : MonoBehaviour
    {
        DialogueSystem ds;
        TextArchitect architect;

        string[] lines = new string[5]{
            "BRO WE GOT DIALOGUE",
            "GAHHHHHHHHHHHH",
            "BRUH",
            "ojfkljsfkjnvjnvlijdnbljnbldjngbijdnbv",
            "===================================================================================================================="
        };

        // Start is called before the first frame update
        void Start()
        {
            ds = DialogueSystem.instance;
            architect = new TextArchitect(ds.dialogueContainer.dialogueText);
            architect.buildMethod = TextArchitect.BuildMethod.typewriter;
            architect.speed = 0.5f;
        }

        // Update is called once per frame
        void Update()
        {
            string longLine = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
            if (Input.GetKeyDown(KeyCode.Space)){
                if(architect.isBuilding){
                    if(!architect.hurryUp){
                        architect.hurryUp = true;
                    }
                    else{
                        architect.ForceComplete();
                    }
                }
                else{
                    //architect.Build(lines[Random.Range(0, lines.Length)]);
                    architect.Build(longLine);
                }
            }
            else if (Input.GetKeyDown(KeyCode.A)){
                //architect.Append(lines[Random.Range(0, lines.Length)]);
                architect.Append(longLine);
            }
        }
    }
}// a