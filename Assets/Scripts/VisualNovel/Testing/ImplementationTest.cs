using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the current working implementation of the VN system with the rest of the game
public class ImplementationTest : MonoBehaviour
{
    // This used to be loaded in by defautl for testing, can be removed
    //[SerializeField] private TextAsset file;

    // This is the vn system object, maybe would be better to asign different on start()
    private GameObject VNsys;

    // These 2 are for interaction with the rest of the game
    public event Action OnShowVisualNovel;
    public event Action OnCloseVisualNovel;

    // To do something after dialogue finished
    Action onDialogFinished = null;

    // For use in other scripts in the scene, THIS IMPLIES THERE SHOULD ONLY BE ONE INSTANCE OF THIS CLASS PER SCENE
    public static ImplementationTest Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // WE get the VN object assigned here, maybe change implementation
        GameObject obj = GameObject.Find("VN Controller");
        if (obj != null)
        {
            VNsys = obj;
            obj.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameObject not found!");
        }

        //Attach events to the finishing of a dialogue
        DialogueSystem.instance.dialogueFinished += () =>
        {
            OnCloseVisualNovel?.Invoke();
            onDialogFinished?.Invoke();
            onDialogFinished = null;
        };
    }

    void Update()
    {
    }


    // The standard use of the dialogue system, uses txt file and can have an action after
    public IEnumerator StartConversation(TextAsset dialog, Action onFinished = null)
    {
        if (VNsys != null)
        {
            OnShowVisualNovel?.Invoke();
            VNsys.SetActive(true);
            List<string> lines = FileManager.ReadTextAsset(dialog, false); // Read txt file into string list
            DialogueSystem.instance.Say(lines);
            onDialogFinished = onFinished; // Here we just assign the usage is attached on start() and ran on dialogueFinished on the DialogueSystem
        }
        else
        {
            Debug.LogWarning("GameObject not found!");
        }
        yield break;
    }

    //Same as above Function but takes a Dialog instead
    public IEnumerator StartConversation(Dialog dialog, Action onFinished = null)
    {
        if (VNsys != null)
        {
            OnShowVisualNovel?.Invoke();
            VNsys.SetActive(true);
            List<string> lines = dialog.Lines;
            DialogueSystem.instance.Say(lines);
            onDialogFinished = onFinished;
        }
        else
        {
            Debug.LogWarning("GameObject not found!");
        }
        yield break;
    }
    //Conversation but as a subscribable event

    // All these functions are what they say, setCharacterSprites is for convos with several sprites, sprites are changed in dialogue line with the changeChar(i) command
    public void ChangeBackground(Texture newBackground)
    {
        DialogueSystem.instance.ChangeBackground(newBackground);
    }

    public void ChangeCharacter(Sprite newCharacter)
    {
        DialogueSystem.instance.ChangeCharacter(newCharacter);
    }

    public void setCharacterSprites(Sprite[] spriteSet)
    {
        DialogueSystem.instance.setCharacterSprites(spriteSet);
    }

    // If the conversation is a multiple path one, use this function ----------------------------------------------
    // By default it will grab the first one in the list
    // It is required that if a path does not end in exit(), it ends on the option to change path, otherwise the system is stuck forever, check into this
    public IEnumerator StartPathConversation(MultiplePathsVNContainer[] paths, Action onFinished = null)
    {
        if (paths.Length == 0)
        {
            Debug.LogWarning("There are no paths");
            yield break;
        }
        if (VNsys == null)
        {
            Debug.LogWarning("GameObject not found!");
            yield break;
        }
        OnShowVisualNovel?.Invoke();
        VNsys.SetActive(true);
        //change
        DialogueSystem.instance.SayPathConversation(paths);
        //till here
        onDialogFinished = onFinished; // Here we just assign the usage is attached on start() and ran on dialogueFinished on the DialogueSystem
        yield break;
    }
}
