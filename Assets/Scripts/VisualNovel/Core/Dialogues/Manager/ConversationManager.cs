using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DIALOGUE{
public class ConversationManager
{
    private DialogueSystem dialogueSystem => DialogueSystem.instance;
    private Coroutine process = null;
    public bool isRunning => process != null ;

    private TextArchitect architect = null;
    private bool userPrompt = false;
    public ConversationManager(TextArchitect architect){
        this.architect = architect;
        dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next; // The function in this class will happen when the even from Dialogue system happens
    }

    private void OnUserPrompt_Next(){
        userPrompt = true;
    }
    
    public void StartConversation(List<string> conversation){
        StopConversation();

        process = dialogueSystem.StartCoroutine(RunningConversation(conversation));
    }

    public void StopConversation(){
        if(!isRunning){
            return;
        }

        dialogueSystem.StopCoroutine(process);
        process = null;
    }

    IEnumerator RunningConversation(List<string> conversation){
        for(int i = 0; i < conversation.Count; i++){
            // If empty line skip
            if (string.IsNullOrWhiteSpace(conversation[i])){
                continue;
            }
            Dialogue_Line line = DialogueParser.Parse(conversation[i]);

            //show dialogue
            if(line.hasDialogue){
                yield return Line_RunDialogue(line);
            }

            if(line.hasCommands){
                yield return Line_RunCommands(line);
            }
        }
    }

    IEnumerator Line_RunDialogue(Dialogue_Line line){
        // if dialogue has speaker, show it
        if(line.hasSpeaker){
            dialogueSystem.ShowSpeakerName(line.speaker);
        }
        else{
            dialogueSystem.HideSpeakerName();
        }

        // Build dialogue
        yield return BuildDialogue(line.dialogue);

        //wait for user input
        yield return WaitForUserInput();
    }

    IEnumerator Line_RunCommands(Dialogue_Line line){
        Debug.Log(line.commands);
        string commandName = line.commands.Substring(0, line.commands.IndexOf('(')).Trim();
        if (commandName == "exit"){
            GameObject obj = GameObject.Find("VN Controller");
            if (obj != null){
                obj.SetActive(false);
                dialogueSystem.ShowBackGround(true);
                dialogueSystem.ShowCharacter(true);
                StopConversation();
                dialogueSystem.triggerEndevent();
            }
            else{
                Debug.LogWarning("GameObject not found!");
            }
        }
        if (commandName == "clear"){
            dialogueSystem.ShowBackGround(false);
            dialogueSystem.ShowCharacter(false);
        }
        if (commandName == "changeChar"){
            int indexChange = 0;
            Int32.TryParse(CommandGetSingleArgument(line.commands), out indexChange);
            dialogueSystem.changeCharIndex(indexChange);
        }
        yield return null;
    }

    IEnumerator BuildDialogue(string dialogue){
        architect.Build(dialogue);
        while(architect.isBuilding){
            if(userPrompt){
                if(!architect.hurryUp){
                    architect.hurryUp = true;
                }
                else{
                    architect.ForceComplete();
                }
                userPrompt = false;
            }
            yield return null;
        }
    }

    IEnumerator WaitForUserInput(){
        while(!userPrompt){
            yield return null;
        }
        userPrompt = false;
    }

    private string CommandGetSingleArgument(string command)
    {
        int parenthesisIndex = command.IndexOf('(');
        string argument = command.Substring(parenthesisIndex + 1, command.Length - parenthesisIndex - 2);
        return argument;
    }
}
}
