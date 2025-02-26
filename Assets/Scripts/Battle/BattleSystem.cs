using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BattleSystem : MonoBehaviour
{
   [SerializeField] BattleUnit playerUnit;
   [SerializeField] BattleUnit enemyUnit;
   [SerializeField] BattleHud playerHud;
   [SerializeField] BattleHud enemyHud;

   private void Start()
   {
        SetupBattle();
   }

   public void SetupBattle()
   {
        playerUnit.Setup();
        enemyUnit.Setup();
        playerHud.SetData(playerUnit.Enemy);
        enemyHud.SetData(enemyUnit.Enemy);
   }
}
