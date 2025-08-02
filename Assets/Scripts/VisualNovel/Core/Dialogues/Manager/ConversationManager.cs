using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DIALOGUE{
public class ConversationManager
{
    private DialogueSystem dialogueSystem => DialogueSystem.instance;
    private Coroutine process = null;
    public bool isRunning => process != null ;

    private TextArchitect architect = null;
    private bool userPrompt = false;
    
    // Paths in case we have a multiple path convo:
    private MultiplePathsVNContainer[] convoPaths= {};
    private int currentPath = -1;
    
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
    
    public void StartPathConversation(MultiplePathsVNContainer[] paths){
        // paths[0].dialog.Lines
        // Make paths available to all class
        //convoPaths = paths;
        convoPaths = new MultiplePathsVNContainer[paths.Length];
        paths.CopyTo(convoPaths, 0);
        currentPath = 0;
        StartConversation(convoPaths[0].dialog.Lines);
    }

    // Meant for stopping the convo process
    public void StopConversation()
        {
            if (!isRunning)
            {
                return;
            }

            dialogueSystem.StopCoroutine(process);
            process = null;
        }
    
    // Meant as proper exit of the system
    private void exitConvo(GameObject vnSys){
        vnSys.SetActive(false);
        dialogueSystem.ShowBackGround(true);
        dialogueSystem.ShowCharacter(true);
        Array.Clear(convoPaths, 0, convoPaths.Length);
        currentPath = -1;
        StopConversation();
        dialogueSystem.triggerEndevent();
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
        //Debug.Log(line.commands);
        string commandName = line.commands.Substring(0, line.commands.IndexOf('(')).Trim();
        GameObject obj = GameObject.Find("VN Controller");
        if (commandName == "exit"){
            if (obj != null){
                exitConvo(obj);
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
        if (commandName == "changePath"){
            string pathName = CommandGetSingleArgument(line.commands);
            //Debug.Log(pathName);
            bool exists = false;
            int location = -1;
            for (int i = 0; i < convoPaths.Length; i++)
            {
                if (convoPaths[i].name == pathName)
                {
                    exists = true;
                    location = i;
                    break;
                }
            }
            
            if (exists){
                StartConversation(convoPaths[location].dialog.Lines);
            }
            else{
                Debug.LogWarning("Path not found, exiting vn sys");
                exitConvo(obj);
            }
        }
        if (commandName == "choice"){
            yield return Line_RunChoice();
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
    
    // The button handling
    IEnumerator Line_RunChoice()
    {
        if (convoPaths.Length == 0 || convoPaths[currentPath].pathOptions.Length == 0)
        {
            Debug.LogWarning("No path options found.");
            yield break;
        }

        MultiplePathsVNContainer.Choice[] options = convoPaths[currentPath].pathOptions;

        // Extract the button dialogue
        List<string> buttonTexts = new List<string>();
        foreach (var option in options)
        {
            buttonTexts.Add(option.buttonDialogue);
        }

        bool choiceMade = false;
        int chosenIndex = -1;

        // Show buttons and wait for callback
        ChoiceManager.instance.ShowChoices(buttonTexts, (index) =>
        {
            chosenIndex = index;
            choiceMade = true;
            ChoiceManager.instance.hide();
        });

        while (!choiceMade)
        {
            yield return null;
        }

        // Get the path name from the selected option
        string chosenPathName = options[chosenIndex].pathName;

        // Find and start that path
        int pathIndex = Array.FindIndex(convoPaths, x => x.name == chosenPathName);
        if (pathIndex != -1)
        {
            currentPath = pathIndex;
            StartConversation(convoPaths[pathIndex].dialog.Lines);
        }
        else
        {
            Debug.LogWarning($"Path '{chosenPathName}' not found.");
        }

        yield return null;
    }

}
}
