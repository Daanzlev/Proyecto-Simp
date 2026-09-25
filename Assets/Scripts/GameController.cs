using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    FreeRoam,
    Battle,
    Cutscene,
    Dialog,
    Menu,
    Paused
}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    GameState state;

    GameState stateBeforePause;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        SimpDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
       // Testiong refactoring playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        /* tESTING REFACTORING, DON'T ERAESE THIS YET
        playerController.OnEnterTrainersView += (Collider2D trainerCollider) =>
        {
            var  trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if (trainer != null)
            {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };
        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        }; */

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
        ImplementationTest.Instance.OnShowVisualNovel += () =>
        {
            state = GameState.Dialog;
        };
        ImplementationTest.Instance.OnCloseVisualNovel += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        //playerController.enabled = false;

        var playerParty = playerController.GetComponent<SimpParty>();
        var wildSimp = CurrentScene.GetComponent<MapArea>().GetRandomWildSimp();
        battleSystem.StartBattle(playerParty, wildSimp);
    }
    TrainerController trainer;
    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        //playerController.enabled = false;

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<SimpParty>();
        var trainerParty = trainer.GetComponent<SimpParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }
    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        //playerController.enabled = true;
    }
    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Return)) 
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        { 
            //Simp
        }
        else if (selectedItem == 1)
        {
            //Bag
        }
        else if (selectedItem == 2)
        {
            //Save
            SavingSystem.i.Save("saveSlot1");
        }
        else if (selectedItem == 3)
        {
            //Load
            SavingSystem.i.Load("saveSlot1");
        }

        state = GameState.FreeRoam;
    }
}