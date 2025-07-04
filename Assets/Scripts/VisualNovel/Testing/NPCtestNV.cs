using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVNtest : MonoBehaviour, Interactable
{
    //public string[] dialogue;
    //public string name;

    //NPC Walkable
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBTWPattern;
    [SerializeField] Texture backgroundImage;
    [SerializeField] Sprite[] characterSrpite;
    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Interact(Transform initiator)

    {
        Debug.Log("Interacting with NPC");
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            ImplementationTest.Instance.ChangeBackground(backgroundImage);
            //ImplementationTest.Instance.ChangeCharacter(characterSrpite);
            ImplementationTest.Instance.setCharacterSprites(characterSrpite);
            StartCoroutine(ImplementationTest.Instance.StartConversation(dialog, () => {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));
        }
    }


    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBTWPattern)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                {
                    //StartCoroutine(character.Move(new Vector2(2, 0)));
                    //state = NPCState.Walking;
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if (transform.position != oldPos)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }

        state = NPCState.Idle;
    }

    public enum NPCState { Idle, Walking, Dialog }
}
