using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueLoader : MonoBehaviour
{
    private DialogueObject LoadedDialogue;

    [Header("Settings")]
    public KeyCode ProgressKey;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI Name;
    public Image RightImage;
    public Image LeftImage;

    [Header("Display Settings")]
    public float ImageDistance;
    public float AnimationDuration;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void StartDialogue(DialogueObject newDialogue)
    {
        gameObject.SetActive(true);

        RightImage.rectTransform.anchoredPosition = new Vector3(ImageDistance, 0, 0);
        LeftImage.rectTransform.anchoredPosition = new Vector3(ImageDistance * -1, 0, 0);


        LoadedDialogue = newDialogue;
        ExecuteDialogueNode();
    }

    void ExecuteDialogueNode()
    {
        Text.text = LoadedDialogue.Text;
        Name.text = LoadedDialogue.Name;
        switch (LoadedDialogue.Action)
        {
            case SpriteAction.SlideIn: StartCoroutine(SlideSprite(LoadedDialogue.Action, LoadedDialogue.Side)); break;
            case SpriteAction.SlideOut: StartCoroutine(SlideSprite(LoadedDialogue.Action, LoadedDialogue.Side)); break;
        }
        if(LoadedDialogue.Next.Length <= 1)
            StartCoroutine(WaitForNextKey());
    }

    IEnumerator WaitForNextKey()
    {
        while (Input.GetKeyDown(ProgressKey)) { yield return null; }
        while (!Input.GetKeyDown(ProgressKey)) { yield return null; }
        Debug.Log(LoadedDialogue.Next.Length);
        if (LoadedDialogue.Next.Length == 1)
        {
            LoadedDialogue = LoadedDialogue.Next[0];
            ExecuteDialogueNode();
        }
        else if(LoadedDialogue.Next.Length == 0)
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator SlideSprite(SpriteAction action, SpriteSide side)
    {
        float Timer = 0;
        Image current;
        if (side == SpriteSide.Left)
        {
            current = LeftImage;
        }
        else
        {
            current = RightImage;
        }

        Vector3 origin = Vector3.zero;
        Vector3 destination = Vector3.zero;

        if (action == SpriteAction.SlideIn)
        {
            origin = new Vector3(ImageDistance * ((side == SpriteSide.Right) ? 1 : -1), 0, 0);
            destination = Vector3.zero;
        }
        else if(action == SpriteAction.SlideOut)
        {
            origin = Vector3.zero;
            destination = new Vector3(ImageDistance * ((side == SpriteSide.Right) ? 1 : -1), 0, 0);
        }
        

        while (Timer < 1)
        {
            current.rectTransform.anchoredPosition = Vector3.Slerp(origin, destination, Timer);
            Timer += Time.deltaTime / AnimationDuration;
            yield return null;
        }
    }
}
