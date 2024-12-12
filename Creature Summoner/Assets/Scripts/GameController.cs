using UnityEngine;

public enum GameState { 
    FreeRoam, 
    Battle 
}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;

    private void Start()
    {
        ToFreeRoamState();
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    void ToFreeRoamState()
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerTeam = playerController.GetComponent<CreatureTeam>();
        var enemyTeam = FindFirstObjectByType<MapArea>().GetComponent<MapArea>().GenerateWildCreatureTeam();

        battleSystem.StartBattle(playerTeam, enemyTeam);
    }

    void EndBattle(bool won)
    {
        ToFreeRoamState();
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}
