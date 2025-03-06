using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, Interactable
{
    //public string[] dialogue;
    //public string name;


    //NPC Walkable

    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBTWPattern;
    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }
    public void Interact()
    {
        Debug.Log("Interacting with NPC");
        //DialogueSystem.Instance.AddNewDialogue(dialogue, name);
        // Test MOVE  - - StartCoroutine(character.Move(new Vector2(-2,0)));
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

        yield  return character.Move(movementPattern[currentPattern]);

        if(transform.position != oldPos)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }
        
        state = NPCState.Idle;
        
    }
    public enum NPCState{ Idle , Walking}
}
