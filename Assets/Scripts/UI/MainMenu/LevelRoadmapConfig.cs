using UnityEngine;

[CreateAssetMenu(menuName = "Main Menu/Level Roadmap Config")]
public class LevelRoadmapConfig : ScriptableObject
{
    [SerializeField] private LevelInfo[] levels;

    public LevelInfo[] Levels => levels;
}
