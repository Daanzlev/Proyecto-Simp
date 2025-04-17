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
    int currentMember;

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

    public void StartTrainerBattle(SimpParty playerParty, SimpParty trainerParty) {

        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        StartCoroutine(SetupBattle());
   }

   public IEnumerator SetupBattle() {

        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle) {

            //Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHealthySimp());
            enemyUnit.Setup(wildSimp);

            dialogBox.SetMoveNames(playerUnit.Simp.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Simp.Base.Name} appeared. ");

        }
        else {
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
            enemyUnit.Setup(enemySimp);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemySimp.Base.Name}");


            //Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerSimp = playerParty.GetHealthySimp();
            playerUnit.Setup(playerSimp);
            yield return dialogBox.TypeDialog($"Go {playerSimp.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Simp.Moves);
        }

        //playerHud.SetData(playerUnit.Simp);
        //enemyHud.SetData(enemyUnit.Simp);

        partyScreen.Init();

        //ActionSelection();
        ChooseFirstTurn();

    }

    void ActionSelection() {

        state = BattleState.ActionSelection;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);

    }

    void OpenPartyScreen() {

        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Simps);
        partyScreen.gameObject.SetActive(true);

    }

    void MoveSelection() {

        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);

    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Simp.Speed >= enemyUnit.Simp.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool win) {

        state = BattleState.BattleOver;
        playerParty.Simps.ForEach(s => s.OnBattleOver());
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
        else {
            if(isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextSimp = trainerParty.GetHealthySimp();
                if(nextSimp != null)
                   StartCoroutine(SendNextTrainerSimp(nextSimp));
                else
                {
                    BattleOver(true);
                }
            }
        }

    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {

        bool canRunMove = sourceUnit.Simp.OnBeforeMove();
        if (!canRunMove) 
        {
            yield return ShowStatusChanges(sourceUnit.Simp);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Simp);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Simp.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAtackAnimation();
        yield return new WaitForSeconds(1f);

        //Player Hit
        targetUnit.PlayHitAnimation();
        yield return new WaitForSeconds(1f);

        if (move.Base.Category == MoveCategory.Status) {

           yield return RunMoveEffects(move, sourceUnit.Simp, targetUnit.Simp);

        }
        else {
            var damageDetails = targetUnit.Simp.TakeDamage(move, sourceUnit.Simp);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if ( targetUnit.Simp.HP <= 0 ) {

            yield return dialogBox.TypeDialog($"{targetUnit.Simp.Base.Name} fainted");
            targetUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);

        }

        //Statuses like burn or psn will hurt the pokemon after the turn
        sourceUnit.Simp.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Simp);
        yield return sourceUnit.Hud.UpdateHP();
        if (targetUnit.Simp.HP <= 0)
        {

            yield return dialogBox.TypeDialog($"{targetUnit.Simp.Base.Name} fainted");
            targetUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);

        }
    }

    IEnumerator RunMoveEffects(Move move, Simp source, Simp target) 
    {
        var effect = move.Base.Effects;
        //Stat Boosting
        if (effect.Boosts != null)
        {

            if (move.Base.Target == MoveTarget.Self)
            {
                source.ApplyBoosts(effect.Boosts);
            }
            else
            {
                target.ApplyBoosts(effect.Boosts);
            }
            //Status Condition
            if(effect.Status != ConditionID.none) 
            { 
                target.SetStatus(effect.Status);
            }
            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
        }
    }

    IEnumerator ShowStatusChanges(Simp simp)
    {
        while(simp.StatusChanges.Count > 0) 
        { 
            var message = simp.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
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
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
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
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMember;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Simps.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z)) {

            var selectedMember = playerParty.Simps[currentMember];
            if(selectedMember.HP <= 0) {
                partyScreen.SetMessageText("You can't send out a fainted SIMP");
                return;
            }
            if (selectedMember == playerUnit.Simp) {
                partyScreen.SetMessageText("You can't switch to the same SIMP");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchSimp(selectedMember));
        }
        else if (Input.GetKeyDown (KeyCode.X)) {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }

    }

    IEnumerator SwitchSimp (Simp newSimp) {

        bool currentSimpFainted = true;
        if(playerUnit.Simp.HP > 0)
        {
            currentSimpFainted = false;
            yield return dialogBox.TypeDialog($"Come back  {playerUnit.Simp.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newSimp);
        //playerHud.SetData(newSimp);
        dialogBox.SetMoveNames(newSimp.Moves);
        yield return dialogBox.TypeDialog($"Go {newSimp.Base.Name}!");

        if(currentSimpFainted) 
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());


    }

    IEnumerator SendNextTrainerSimp (Simp nextSimp) {
        state = BattleState.Busy;
        enemyUnit.Setup(nextSimp);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextSimp.Base.Name}");

        //state = BattleState.RunningTurn;
    }

}