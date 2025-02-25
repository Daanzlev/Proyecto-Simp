using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy}
public class BattleSystem : MonoBehaviour
{
   [SerializeField] BattleUnit playerUnit;
   [SerializeField] BattleUnit enemyUnit;
   [SerializeField] BattleHud playerHud;
   [SerializeField] BattleHud enemyHud;
   [SerializeField] BattleDialogBox dialogBox;

   public event Action <bool> OnEncountered;

    BattleState state;
    int currentAction;
    int currentMove;
   private void Start()
   {
        StartCoroutine(SetupBattle());
   }

   public IEnumerator SetupBattle()
   {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHud.SetData(playerUnit.Enemy);
        enemyHud.SetData(enemyUnit.Enemy);

        dialogBox.SetMoveNames(playerUnit.Enemy.Moves);

       yield return dialogBox.TypeDialog( $"A wild {enemyUnit.Enemy.Base.Name} appeared. ");
       yield return new WaitForSeconds(1f);

       PlayerAction();
   }
   void PlayerAction()
   {
     state = BattleState.PlayerAction;
     StartCoroutine(dialogBox.TypeDialog("Choose an action"));
     dialogBox.EnableActionSelector(true);
   }
   void PlayerMove()
   {
      state= BattleState.PlayerAction;
      dialogBox.EnableActionSelector(false);
      //StartCoroutine(dialogBox.TypeDialog("Choose an action"));
      dialogBox.EnableActionSelector(false);
      dialogBox.EnableDialogText(false);
      dialogBox.EnableMoveSelector(true);
   }
   public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
          if (currentAction <1)
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
          if (currentAction >0)
            --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
          if (currentAction == 0)
          {
            //Fight
            PlayerMove();
          }
          else if (currentAction == 1)
          {
            //Run
          }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < playerUnit.Enemy.Moves.Count - 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Enemy.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove-= 2;
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Enemy.Moves[currentAction]);
    }
}
