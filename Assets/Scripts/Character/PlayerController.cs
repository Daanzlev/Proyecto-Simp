using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
   public float moveSpeed;

    /// 
    /// GameController
    public event Action OnEncountered;
    /// 

   private bool isMoving;
   private Vector2 input;

    private Animator animator;

 private void Awake()
{
    animator = GetComponent<Animator>();   
}

   public void HandleUpdate()
    {
         if (!isMoving)
         {
              input.x = Input.GetAxisRaw("Horizontal");
              input.y = Input.GetAxisRaw("Vertical");

              //Quita el movimiento en diagonal
              if(input.x !=0) input.y = 0; 
    
              if (input != Vector2.zero)
              {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
              }
         }

         animator.SetBool("isMoving",isMoving);

         if(Input.GetKeyDown(KeyCode.Z))
                Interact();
    
    }
    void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
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
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
       if(Physics2D.OverlapCircle(targetPos, 0.2f,GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) !=null)
       {
        return false;
       }
        return true;
    }

    private void OnMover()
    {
        //Physics2D.OverlapCircle(transform.position - new Vector3(0, offsetY), 0.2f, GameLayers.i.GrassLayer)
    }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10)
            {
                Debug.Log("Encountered a simp fight");
                animator.SetBool("isMoving",false);
                OnEncountered();
            }
        }
    }
}
