using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel.Design;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }



public class BattleSystem : MonoBehaviour
{
    /* -------------------------------------------------------
>>>>>>>>> VARIABLES
-------------------------------------------------------  */
    // UI
    [SerializeField] BattleUnit playerUnit;
   [SerializeField] BattleUnit enemyUnit;
   [SerializeField] BattleHud playerHud;
   [SerializeField] BattleHud enemyHud;
   [SerializeField] BattleDialogBox dialogBox;
   [SerializeField] PartyScreen partyScreen;
   [SerializeField] Image playerImage;
   [SerializeField] Image trainerImage;


    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;

    SimpParty playerParty;
    SimpParty trainerParty;
    Simp wildSimp;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;



    /* -------------------------------------------------------
    >>>>>>>>> METHODS
    -------------------------------------------------------  */

    public void StartBattle(SimpParty playerParty, Simp wildSimp) {

        this.playerParty = playerParty;
        this.wildSimp = wildSimp;
        StartCoroutine(SetupBattle());

    }

    public void StartTrainerBattle(SimpParty playerParty, SimpParty trainerParty)
   {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        StartCoroutine(SetupBattle());
   }
   public IEnumerator SetupBattle()
   {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            //Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHealthySimp());
            enemyUnit.Setup(wildSimp);

            dialogBox.SetMoveNames(playerUnit.Simp.Moves);

            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Simp.Base.Name} appeared. ");
        }
        else
        {
            //Trainer Battle

            //Show Trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            //Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemySimp = trainerParty.GetHealthySimp();
            //enemySimp.Setup(enemySimp);
            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");


            //Send out first pokemon of the player

        }

        //playerHud.SetData(playerUnit.Simp);
        //enemyHud.SetData(enemyUnit.Simp);

        partyScreen.Init();

        ActionSelection();

    }

    void ActionSelection() {

        state = BattleState.ActionSelection;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);

    }

    void OpenPartyScreen() {

        partyScreen.SetPartyData(playerParty.Simps);
        partyScreen.gameObject.SetActive(true);

    }

    void MoveSelection() {

        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);

    }

    void BattleOver(bool win) {

        state = BattleState.BattleOver;
        OnBattleOver(win);

    }

    void CheckForBattleOver(BattleUnit faintedUnit) {

        //If player SIMP fainted
        if (faintedUnit.IsPlayerUnit) {

            var nextSimp = playerParty.GetHealthySimp();

            //If there is another SIMP
            if (nextSimp != null) {
                OpenPartyScreen();
            }
            else {
                BattleOver(false);
            }

        }
        //If enemy SIMP fainted
        else {
            BattleOver(true);
        }

    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Simp.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAtackAnimation();
        yield return new WaitForSeconds(1f);

        //Player Hit
        targetUnit.PlayHitAnimation();
        yield return new WaitForSeconds(1f);

        var damageDetails = targetUnit.Simp.TakeDamage(move, sourceUnit.Simp);
        yield return targetUnit.Hud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) {

            yield return dialogBox.TypeDialog($"{targetUnit.Simp.Base.Name} fainted");
            targetUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);

        }

    }

    IEnumerator PlayerMove() {

        //Player Attack
        state = BattleState.PerformMove;
        var move = playerUnit.Simp.Moves[currentMove];

        yield return RunMove(playerUnit, enemyUnit, move);

        //If enemy SIMP has not fainted
        if (state == BattleState.PerformMove) {
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove() {

        //Enemy Attack
        state = BattleState.PerformMove;
        var move = enemyUnit.Simp.GetRandomMove();

        yield return RunMove(enemyUnit, playerUnit, move);

        //If player SIMP has not fainted
        if (state == BattleState.PerformMove) {
            ActionSelection();
        }

    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) {

        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.Type > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.Type < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");

    }

    public void HandleUpdate() {

        if (state == BattleState.ActionSelection) {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection) {
            HandleMoveSelection();
        }

    }

    void HandleActionSelection() {

        if (Input.GetKeyDown(KeyCode.RightArrow))  {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z)) {

            //Fight
            if (currentAction == 0) {
                MoveSelection();
            }

            //Bag            
            else if (currentAction == 1) {
                //In progress
            }

            //SIMP party
            else if (currentAction == 2) {
                OpenPartyScreen();
            }

            //Run
            else if (currentAction == 3)  {
                //In progress
            }

        }

    }

    void HandleMoveSelection() {

        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Simp.Moves.Count - 1);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Simp.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) {

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());

        }
        else if (Input.GetKeyDown(KeyCode.X)) {

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableActionSelector(true);
            ActionSelection();

        }

    }

    //MISSING IEnuamerator PerformEnemyMove()

}