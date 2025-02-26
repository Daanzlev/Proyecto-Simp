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

   public event Action<bool> OnBattleOver;

  
     BattleState state;
     int currentAction;

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

       yield return dialogBox.TypeDialog( $"A wild {playerUnit.Enemy.Base.Name} appeared. ");
       yield return new WaitForSeconds(1f);

       PlayerAction();
   }
   void PlayerAction()
   {
     state = BattleState.PlayerAction;
     StartCoroutine(dialogBox.TypeDialog("Choose an action"));
     dialogBox.EnableActionSelector(true);
   }
   public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
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
    }

    //MISSING IEnuamerator PerformPlayerMove()
    //MISSING IEnuamerator PerformEnemyMove()
}