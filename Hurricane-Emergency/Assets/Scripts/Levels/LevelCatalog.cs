using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Hurricane/Level Catalog")]
public sealed class LevelCatalog : ScriptableObject
{
    [SerializeField] private List<LevelDefinition> levels = new();

    public IReadOnlyList<LevelDefinition> Levels => levels;

#if UNITY_EDITOR
    public void ConfigureEditor(IEnumerable<LevelDefinition> orderedLevels)
    {
        levels = new List<LevelDefinition>(orderedLevels);
    }
#endif
}
