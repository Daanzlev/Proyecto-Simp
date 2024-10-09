using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Dialogue_Expresion
{
    Idle,
    Charging
}

public enum Dialogue_Animation
{
    None,
    Idle,
    Jumping,
    SlideIn,
    SlideOut,
    Squash,
    Shiver,
}

public enum Sprite_Side
{
    None,
    Left,
    Right
}

public class DialogueLoader : MonoBehaviour
{
    [Header("Settings")]
    public DialogueCharacterIndex CharacterIndex;

    [Header("Display Settings")]
    public float ImageDistance;
    public float AnimationDuration;

    [Header("Static References")]
    public KeyCode ProgressKey;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI Name;
    public Image RightImage;
    public Image LeftImage;

    private IEnumerator Left_Animation;
    private IEnumerator Right_Animation;

    public string Character_Left;
    public string Character_Right;

    private DialogueObject LoadedDialogue;

    void Start()
    {
        gameObject.SetActive(false);
    }

    // Receive dialogue node from event
    public void StartDialogue(DialogueObject newDialogue)
    {
        gameObject.SetActive(true);

        RightImage.rectTransform.anchoredPosition = new Vector3(ImageDistance, 0, 0);
        LeftImage.rectTransform.anchoredPosition = new Vector3(ImageDistance * -1, 0, 0);

        Character_Left = "";
        Character_Right = "";

        LoadedDialogue = newDialogue;
        ExecuteDialogueNode();
    }

    // Load dialogue node to screen
    void ExecuteDialogueNode()
    {
        Text.text = LoadedDialogue.Text;
        Name.text = LoadedDialogue.Name;

        bool IntroducedCharacter = false;
        if(LoadedDialogue.Side == Sprite_Side.Left) {
            if (LoadedDialogue.Name == Character_Left || LoadedDialogue.Name == "")
            {
                Debug.Log("Left character expresion change");
                LeftImage.sprite = CharacterIndex.Get(LoadedDialogue.Name).Get(LoadedDialogue.Expresion);
            }
            else
            {
                Debug.Log("Left character intro");
                LeftImage.sprite = CharacterIndex.Get(LoadedDialogue.Name).Get(LoadedDialogue.Expresion);
                AnimateSprite(LoadedDialogue, Dialogue_Animation.SlideIn);
                IntroducedCharacter = true;
                Character_Left = LoadedDialogue.Name;
            }
        }
        else
        {
            if (LoadedDialogue.Name == Character_Right || LoadedDialogue.Name == "")
            {
                Debug.Log("Right character expresion change");
                RightImage.sprite = CharacterIndex.Get(LoadedDialogue.Name).Get(LoadedDialogue.Expresion);
            }
            else
            {
                Debug.Log("Right character intro");
                RightImage.sprite = CharacterIndex.Get(LoadedDialogue.Name).Get(LoadedDialogue.Expresion);
                AnimateSprite(LoadedDialogue, Dialogue_Animation.SlideIn);
                IntroducedCharacter = true;
                Character_Right = LoadedDialogue.Name;
            }
        }

        if(!IntroducedCharacter)
            AnimateSprite(LoadedDialogue);
        
        if(LoadedDialogue.Next.Length <= 1)
            StartCoroutine(WaitForNextKey());
    }

    // Call respective animation coroutines
    void AnimateSprite(DialogueObject dialogue, Dialogue_Animation OverrideAnimation = Dialogue_Animation.None)
    {
        if (dialogue.Side == Sprite_Side.Left)
        {
            if (Left_Animation != null)
            {
                StopCoroutine(Left_Animation);
            }
        }
        else
        {
            if (Right_Animation != null)
            {
                StopCoroutine(Right_Animation);
            }
        }
        ResetSprite(dialogue.Side);

        IEnumerator coroutine;

        // if (OverrideAnimation == Dialogue_Animation.None)
            OverrideAnimation = LoadedDialogue.Action;

        switch (OverrideAnimation)
        {
            case Dialogue_Animation.SlideIn: coroutine = (SlideSprite(OverrideAnimation, LoadedDialogue.Side)); break;
            case Dialogue_Animation.SlideOut: coroutine = (SlideSprite(OverrideAnimation, LoadedDialogue.Side)); break;
            case Dialogue_Animation.Jumping: coroutine = (JumpSprite(LoadedDialogue.Side)); break;
            case Dialogue_Animation.Squash: coroutine = (SquashSprite(LoadedDialogue.Side)); break;
            case Dialogue_Animation.Shiver: coroutine = (ShiverSprite(LoadedDialogue.Side)); break;
            default: coroutine = null; break;
        }

        if (dialogue.Side == Sprite_Side.Left)
            Left_Animation = coroutine;
        else
            Right_Animation = coroutine;

        if (coroutine != null)
            StartCoroutine(coroutine);
    }

    // Wait until key is pressed to go to next dialogue
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

    void ResetSprite(Sprite_Side side)
    {
        Image current;
        if (side == Sprite_Side.Left)
        {
            current = LeftImage;
        }
        else
        {
            current = RightImage;
        }

        current.rectTransform.anchoredPosition = Vector3.zero;
        current.rectTransform.anchoredPosition = Vector3.one;
    }

    // Slide in-and-out animations
    IEnumerator SlideSprite(Dialogue_Animation action, Sprite_Side side)
    {
        float Timer = 0;
        Image current;
        if (side == Sprite_Side.Left)
        {
            current = LeftImage;
        }
        else
        {
            current = RightImage;
        }

        Vector3 origin = Vector3.zero;
        Vector3 destination = Vector3.zero;

        if (action == Dialogue_Animation.SlideIn)
        {
            origin = new Vector3(ImageDistance * ((side == Sprite_Side.Right) ? 1 : -1), 0, 0);
            destination = Vector3.zero;
        }
        else if(action == Dialogue_Animation.SlideOut)
        {
            origin = Vector3.zero;
            destination = new Vector3(ImageDistance * ((side == Sprite_Side.Right) ? 1 : -1), 0, 0);
        }
        

        while (Timer < 1)
        {
            current.rectTransform.anchoredPosition = Vector3.Slerp(origin, destination, Timer);
            Timer += Time.deltaTime / AnimationDuration;
            yield return null;
        }
    }

    IEnumerator JumpSprite(Sprite_Side side)
    {
        float Timer = 0;
        Image current;
        if (side == Sprite_Side.Left)
        {
            current = LeftImage;
        }
        else
        {
            current = RightImage;
        }

        while (true)
        {
            current.rectTransform.anchoredPosition = new Vector3(0, Mathf.Abs(Mathf.Cos(Timer)) * 50f, 0);
            Timer += Time.deltaTime / AnimationDuration;
            yield return null;
        }
    }
    IEnumerator SquashSprite(Sprite_Side side)
    {
        float Timer = 0;
        Image current;
        if (side == Sprite_Side.Left)
        {
            current = LeftImage;
        }
        else
        {
            current = RightImage;
        }

        while (true)
        {
            current.rectTransform.localScale = new Vector3(((Mathf.Sin(Timer)) * 0.1f + 1f) * ((side == Sprite_Side.Right) ? -1 : 1), (Mathf.Cos(Timer)) * 0.1f + 1f, 0);
            Timer += Time.deltaTime / AnimationDuration;
            yield return null;
        }
    }

    IEnumerator ShiverSprite(Sprite_Side side)
    {
        float Timer = 0;
        Image current;
        if (side == Sprite_Side.Left)
        {
            current = LeftImage;
        }
        else
        {
            current = RightImage;
        }

        while (true)
        {
            current.rectTransform.anchoredPosition = new Vector3((Mathf.Sin(Timer * Mathf.Rad2Deg)) * 10f, 0, 0);
            Timer += Time.deltaTime / AnimationDuration;
            yield return null;
        }
    }
}
