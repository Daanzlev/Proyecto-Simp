using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel.Design;
using UnityEngine.UI;
using DG.Tweening;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen,AboutToUse, BattleOver, PerformMove}
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
    [SerializeField] GameObject pokeballSprite; // This name will be changed later

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = true;

    SimpParty playerParty;
    SimpParty trainerParty;
    Simp wildSimp;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    public void StartBattle(SimpParty playerParty, Simp wildSimp)
    {

        this.playerParty = playerParty;
        this.wildSimp = wildSimp;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;
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

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();

    }

    void BattleOver(bool win)
    {

        state = BattleState.BattleOver;
        playerParty.Simps.ForEach(s => s.OnBattleOver());
        OnBattleOver(win);

    }

    void ActionSelection()
    {

        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);

    }

    void OpenPartyScreen()
    {

        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Simps);
        partyScreen.gameObject.SetActive(true);

    }

    void MoveSelection()
    {

        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);

    }
    IEnumerator AboutToUse(Simp newSimp)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newSimp.Base.Name}. Do you want to change your Simp ?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
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
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowPokeball();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
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
                yield return HandlePokemonFainted(targetUnit);
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
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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
        while (simp.StatusChanges.Count > 0)
        {
            var message = simp.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Simp.Base.Name} Fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit) 
        {
            //Exp gain
            int expYield = faintedUnit.Simp.Base.ExpYield;
            int enemyLevel = faintedUnit.Simp.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus)/7);
            playerUnit.Simp.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Simp.Base.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();
            //Check Level Up
            while (playerUnit.Simp.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Simp.Base.Name} grew to level {playerUnit.Simp.Level}");
                var newMove = playerUnit.Simp.GetLearnableMoveAtCurrLevel();
                if (newMove != null)
                {
                    if(playerUnit.Simp.Moves.Count < SimpBase.MaxNumOfMoves)
                    {
                        playerUnit.Simp.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Simp.Base.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Simp.Moves);

                    }
                    else
                    {

                    }
                }
                yield return playerUnit.Hud.SetExpSmooth(true);

            }

            yield return new WaitForSeconds(1f);

        }

        CheckForBattleOver(faintedUnit);
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {

        //If player SIMP fainted
        if (faintedUnit.IsPlayerUnit)
        {

            var nextSimp = playerParty.GetHealthySimp();

            //If there is another SIMP
            if (nextSimp != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }

        }
        else
        {
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextSimp = trainerParty.GetHealthySimp();
                if (nextSimp != null)
                    //Send out next SIMP
                    StartCoroutine(AboutToUse(nextSimp));
                else
                {
                    BattleOver(true);
                }
            }
            // BattleOver(true);
        }

    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {

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
    public void HandleUpdate()
    {

        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        //PRUEBAS
        /*if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(ThrowPokeball());
        }*/

    }

    void HandleActionSelection()
    {

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 2;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {

            //Fight
            if (currentAction == 0)
            {
                MoveSelection();
            }

            //Bag            
            else if (currentAction == 1)
            {
                //In progress
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }

            //SIMP party
            else if (currentAction == 2)
            {
                prevState = state;
                OpenPartyScreen();
            }

            //Run
            else if (currentAction == 3)
            {
                //In progress
                StartCoroutine(RunTurns(BattleAction.Run));
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
            if (playerUnit.Simp.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a Simp to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerSimp());
            }
            else
                ActionSelection();
        }
    }
    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                // Yes Option
                prevState = BattleState.AboutToUse;
                OpenPartyScreen();
            }
            else
            {
                // No Option
                StartCoroutine(SendNextTrainerSimp());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerSimp());
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

        if (prevState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if (prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerSimp());
        }
    }

    IEnumerator SendNextTrainerSimp()
    {
        state = BattleState.Busy;
        var nextSimp = trainerParty.GetHealthySimp();
        enemyUnit.Setup(nextSimp);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextSimp.Base.Name}");

        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
         {
             yield return dialogBox.TypeDialog($"You can't steal the sidekicks SIMP!");
             state = BattleState.RunningTurn;
             yield break;
         }

        yield return dialogBox.TypeDialog($"{player.Name} used POKEBALL!");

        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();


        // Animations
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchSimp(enemyUnit.Simp);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            // Pokemon is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Simp.Base.Name} was caught");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddSimp(enemyUnit.Simp);
            yield return dialogBox.TypeDialog($"{enemyUnit.Simp.Base.Name} has been added to your party");

            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            // Pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Simp.Base.Name} broke free");
            else
                yield return dialogBox.TypeDialog($"Almost caught it");

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }

    }
    int TryToCatchSimp(Simp simp)
    {
        float a = (3 * simp.MaxHP - 2 * simp.HP) * simp.Base.CatchRate * ConditionsDB.GetStatusBonus(simp.Status) / (3 * simp.MaxHP);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            ++shakeCount;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle) 
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Simp.Speed;
        int enemySpeed = enemyUnit.Simp.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0,256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}