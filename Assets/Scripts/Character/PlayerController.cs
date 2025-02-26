using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    // Pattern  GameController to PlayerController and vice versa
    public event Action OnEncountered;

   private Vector2 input;

    //References to our custom animator
    //private CharacterAnimator animator;

    //References to Character 
    private Character character;    

 private void Awake()
{
    //animator = GetComponent<CharacterAnimator>();   
    character = GetComponent<Character>();
}

   public void HandleUpdate()
    {
         if (!character.IsMoving)
         {
              input.x = Input.GetAxisRaw("Horizontal");
              input.y = Input.GetAxisRaw("Vertical");

              //Quita el movimiento en diagonal
              if(input.x !=0) input.y = 0; 
    
              if (input != Vector2.zero)
              {
                StartCoroutine (character.Move(input, CheckForEncounters));
              }
         }
        
        character.HandleUpdate();
        //Interact with the environment
         if(Input.GetKeyDown(KeyCode.Z))
                Interact();
    
    }
    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;
        
       
         //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);
        //Debug.DrawRay(interactPos, transform.position, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
            //Aquí voy a llamar a la interfaz Interactable, pero a la funcion que esta en NPCController void interact
        }
    }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10)
            {
                Debug.Log("Encountered a simp fight");
                character.Animator.IsMoving =  false;
                OnEncountered();
            }
        }
    }
}