using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.TextCore.Text;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] TextAsset dialog;
    [SerializeField] TextAsset dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

     // State
    bool battleLost = false;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }
     private void Update()
    {
        character.HandleUpdate();
    }

    public void Interact(Transform initiator)
    {
        
        Debug.Log("Interacting with Trainer");
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            StartCoroutine(ImplementationTest.Instance.StartConversation(dialog, () =>
            {
                GameController.Instance.StartTrainerBattle(this);
            }));
        }
        else
        {
            StartCoroutine(ImplementationTest.Instance.StartConversation(dialogAfterBattle));
        }
       
    }
   
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //Show Exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //Walk up to player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Show dialogue
        Debug.Log("Starting Trainer Battle");
        StartCoroutine(ImplementationTest.Instance.StartConversation(dialog, () =>
        {
            GameController.Instance.StartTrainerBattle(this);
        }));

        
    }
   
    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public string Name { 
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}
