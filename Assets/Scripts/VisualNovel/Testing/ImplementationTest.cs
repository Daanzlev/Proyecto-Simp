using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImplementationTest : MonoBehaviour
{
    [SerializeField] private TextAsset file;
    
    private GameObject VNsys;

    public event Action OnShowVisualNovel;
    public event Action OnCloseVisualNovel;

    Action onDialogFinished = null;

    public static ImplementationTest Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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

    public IEnumerator StartConversation(TextAsset dialog, Action onFinished=null)
    {
        if (VNsys != null){
            OnShowVisualNovel?.Invoke();
            VNsys.SetActive(true);
            List<string> lines = FileManager.ReadTextAsset(dialog, false);
            DialogueSystem.instance.Say(lines);
            onDialogFinished = onFinished;
        }
        else{
            Debug.LogWarning("GameObject not found!");
        }
        yield break;
    }

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
}
