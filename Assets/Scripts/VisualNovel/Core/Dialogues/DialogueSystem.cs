using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This script is where all the interfacing with the rest of the game is put into, it mostly interact with the conversation manager and dialogue container

namespace DIALOGUE
{
public class DialogueSystem : MonoBehaviour
{
    public DialogueContainer dialogueContainer = new DialogueContainer();
    private ConversationManager conversationManager;
    private TextArchitect architect;

    public static DialogueSystem instance;

    //handle the input as events 
    public delegate void DialogueSystemEvent();
    public event DialogueSystemEvent onUserPrompt_Next;

    public event DialogueSystemEvent dialogueFinished;

    public bool isRunningConversation => conversationManager.isRunning;

    private void Awake(){
        // Make sure theres only one dialogue system in the scene
        if (instance == null){
            instance = this;
            Initialize();
        }
        else{
            DestroyImmediate(gameObject);
        }
    }

    bool _initialized = false;
    private void Initialize(){
        if(_initialized){
            return;
        }
        architect = new TextArchitect(dialogueContainer.dialogueText);
        conversationManager = new ConversationManager(architect);
    }

    public void OnUserPrompt_Next(){
        onUserPrompt_Next?.Invoke();
    }

    public void ShowSpeakerName(string speakerName = ""){
        if(speakerName.ToLower() != "narrator"){
            dialogueContainer.nameContainer.Show(speakerName);
        }
        else{
            HideSpeakerName();
        }
    }
    public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

    public void Say(string speaker, string dialogue){
        List<string> conversation = new List<string>() {$"{speaker} \"{dialogue}\""};
        Say(conversation);
    }

    public void Say(List<string> conversation){
        conversationManager.StartConversation(conversation);
    }
    
    public void SayPathConversation(MultiplePathsVNContainer[] paths){
        conversationManager.StartPathConversation(paths);
    }

    public void ShowBackGround(bool shown)
        {
            dialogueContainer.backAndCharContainer.ShowBackGround(shown);
        }

    public void ShowCharacter(bool shown){
        dialogueContainer.backAndCharContainer.ShowCharacter(shown);
    }
    
    public void triggerEndevent(){
        dialogueFinished?.Invoke();
    }

    public void ChangeBackground(Texture newBackground)
    {
        dialogueContainer.backAndCharContainer.ChangeBackground(newBackground);
    }

    public void ChangeCharacter(Sprite newCharacter)
    {
        dialogueContainer.backAndCharContainer.ChangeCharacter(newCharacter);
    }

    // This is used when having multiple sprites in a convo and is meant to be used with the changeChar(i) command in dialogue
    public void setCharacterSprites(Sprite[] spriteSet)
    {
        dialogueContainer.backAndCharContainer.setCharacterSprites(spriteSet);
    }

    // This assumes a list of character sprites has been set to work
    public void changeCharIndex(int index)
    {
        dialogueContainer.backAndCharContainer.changeCharIndex(index);
    }
}
}
