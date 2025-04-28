using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel.Design;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver, PerformMove}
public enum BattleAction { Move, SwitchSimp, UseItem, Run }


public class BattleSystem : MonoBehaviour
{
    /* -------------------------------------------------------
    >>>>>>>>> VARIABLES
    -------------------------------------------------------  */
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    SimpParty playerParty;
    SimpParty trainerParty;
    Simp wildSimp;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;
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
            //WILD SIMP
            playerUnit.Setup(playerParty.GetHealthySimp());
            enemyUnit.Setup(wildSimp);

            dialogBox.SetMoveNames(playerUnit.Simp.Moves);

            yield return dialogBox.TypeDialog($"A wild {wildSimp.Base.Name} appeared!");
            
        }
        else 
        {
            //TRAINER BATTLE

            //Showing sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            // Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemySimp = trainerParty.GetHealthySimp();
            enemyUnit.Setup(enemySimp);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemySimp.Base.Name}");

            // Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            var playerSimp = playerParty.GetHealthySimp();
            playerUnit.Setup(playerSimp);
            yield return dialogBox.TypeDialog($"Go {playerSimp.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Simp.Moves);

           
        }
        
        partyScreen.Init();
        ActionSelection();

    }

    void BattleOver(bool win) {

        state = BattleState.BattleOver;
        playerParty.Simps.ForEach(s => s.OnBattleOver());
        OnBattleOver(win);

    }

     void ActionSelection() {

        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
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
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerUnit.Simp.CurrentMove = playerUnit.Simp.Moves[currentMove];
            enemyUnit.Simp.CurrentMove = enemyUnit.Simp.GetRandomMove();

            
            int playerMovePriority = playerUnit.Simp.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Simp.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Simp.Speed >= enemyUnit.Simp.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondSimp = secondUnit.Simp;

            // First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Simp.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondSimp.HP > 0)
            {
                // Second Turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Simp.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchSimp)
            {
                var selectedSimp = playerParty.Simps[currentMember];
                state = BattleState.Busy;
                yield return SwitchSimp(selectedSimp);
            }

            // Enemy Turn
            var enemyMove = enemyUnit.Simp.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }
    
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Simp.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Simp);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Simp);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Simp.Base.Name} used {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Simp, targetUnit.Simp))
        {

            sourceUnit.PlayAtackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Simp, targetUnit.Simp, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Simp.TakeDamage(move, sourceUnit.Simp);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Simp.HP > 0)
            {
                foreach (var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Simp, targetUnit.Simp, secondary.Target);
                }
            }

            if (targetUnit.Simp.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Simp.Base.Name} Fainted");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }

        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Simp.Base.Name}'s attack missed");
        }
    }
    
     IEnumerator RunMoveEffects(MoveEffects effects, Simp source, Simp target, MoveTarget moveTarget)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Statuses like burn or psn will hurt the pokemon after the turn
        sourceUnit.Simp.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Simp);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Simp.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Simp.Base.Name} Fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }
   
    bool CheckIfMoveHits(Move move, Simp source, Simp target)
    {
       if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Simp simp)
    {
        while(simp.StatusChanges.Count > 0) 
        { 
            var message = simp.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
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
           if(!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextSimp = trainerParty.GetHealthySimp();
                if(nextSimp != null)
                    //Send out next SIMP
                   StartCoroutine(SendNextTrainerSimp(nextSimp));
                else
                {
                    BattleOver(true);
                }
            }
            BattleOver(true);
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

  /*  IEnumerator PlayerMove() {

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

    */
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
                prevState = state;
                OpenPartyScreen();
            }

            //Run
            else if (currentAction == 3)  {
                //In progress
            }

        }

    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Simp.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Simp.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Simp.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    //MISSING IEnuamerator PerformEnemyMove()
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Simps.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Simps[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted SIMP");
                return;
            }
            if (selectedMember == playerUnit.Simp)
            {
                partyScreen.SetMessageText("You can't switch with the same SIMP");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchSimp));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchSimp(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    

    IEnumerator SwitchSimp(Simp newSimp)
    {
        if (playerUnit.Simp.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Simp.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newSimp);
        dialogBox.SetMoveNames(newSimp.Moves);
        yield return dialogBox.TypeDialog($"Go {newSimp.Base.Name}!");

        state = BattleState.RunningTurn;
    }

   IEnumerator SendNextTrainerSimp (Simp nextSimp) {
        state = BattleState.Busy;
        enemyUnit.Setup(nextSimp);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextSimp.Base.Name}");

        state = BattleState.RunningTurn;
    }

}