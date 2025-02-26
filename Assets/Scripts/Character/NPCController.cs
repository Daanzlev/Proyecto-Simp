using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, Interactable
{
    //public string[] dialogue;
    //public string name;
    /* TEST Sprites Animation
    [SerializeField] List<Sprite> sprites;

    SpriteAnimator spriteAnimator;

    private void Start()
    {
        spriteAnimator = new SpriteAnimator(sprites, GetComponent<SpriteRenderer>());
        spriteAnimator.Start();
    }
    private void Update()
    {
        spriteAnimator.HandleUpdate();
    }*/


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
            idleTimer -= Time.deltaTime;
            if (idleTimer < timeBTWPattern)
            {
                idleTimer = Random.Range(1f, 4f);
                if (movementPattern.Count > 0)
                {
                    state = NPCState.Walking;
                    StartCoroutine(Walk());
                }
                StartCoroutine(character.Move(new Vector2(2, 0)));
            }
        }
        character.HandleUpdate();
    }
    IEnumerator Walk()
    {
        state = NPCState.Walking;
        yield  return character.Move(movementPattern[currentPattern]);
        currentPattern = (currentPattern + 1) % movementPattern.Count;
        state = NPCState.Idle;
        
    }
    public enum NPCState{ Idle , Walking}
}
