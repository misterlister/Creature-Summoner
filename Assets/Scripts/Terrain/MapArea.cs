using UnityEngine;
using System.Collections.Generic;
using static GameConstants;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<CreatureConfig> wildCreaturesFront;
    [SerializeField] List<CreatureConfig> wildCreaturesMid;
    [SerializeField] List<CreatureConfig> wildCreaturesBack;

    [SerializeField] int frontColMinSize = 1;
    [SerializeField] int frontColMaxSize = 3;
    [SerializeField] int backColMinSize = 1;
    [SerializeField] int backColMaxSize = 3;

    [SerializeField] CreatureTeam wildTeam;

    public CreatureConfig GetRandomCreatureConfig(List<CreatureConfig> pool)
    {
        return pool[Random.Range(0, pool.Count)];
    }

    public CreatureTeam GenerateWildCreatureTeam()
    {
        // Clamp column sizes
        frontColMinSize = Mathf.Clamp(frontColMinSize, 1, BATTLE_COLS);
        frontColMaxSize = Mathf.Clamp(frontColMaxSize, frontColMinSize, BATTLE_COLS);

        backColMinSize = Mathf.Clamp(backColMinSize, 0, BATTLE_COLS);
        backColMaxSize = Mathf.Clamp(backColMaxSize, backColMinSize, BATTLE_COLS);

        wildTeam.ClearCreatures();

        // Roll how many creatures appear in each column
        int frontCount = Random.Range(frontColMinSize, frontColMaxSize + 1);
        int backCount = Random.Range(backColMinSize, backColMaxSize + 1);

        // Add front creatures
        for (int i = 0; i < frontCount; i++)
        {
            var config = GetRandomCreatureConfig(wildCreaturesFront);
            wildTeam.AddCreatureFromConfig(config);
        }

        // Add back creatures
        for (int i = 0; i < backCount; i++)
        {
            var config = GetRandomCreatureConfig(wildCreaturesBack);
            wildTeam.AddCreatureFromConfig(config);
        }

        return wildTeam;
    }

    private List<CreatureConfig> GenerateColumnConfigs(
        List<CreatureConfig> sourcePool,
        int count
    )
    {
        var result = new List<CreatureConfig>();
        for (int i = 0; i < count; i++)
        {
            result.Add(GetRandomCreatureConfig(sourcePool));
        }
        return result;
    }

}
