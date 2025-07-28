using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple Input system, uses a function from DialogueSystem, can be used or modified as needed

namespace DIALOGUE{
public class PlayerInputManager : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)){
            PromptAdvance();
        }
    }

    public void PromptAdvance(){
        DialogueSystem.instance.OnUserPrompt_Next();
    }
}
}