using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemIconDatabase", menuName = "GachaSurvivor/Shop Item Icon Database")]
public class ShopItemIconDatabase : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string itemId;
        public Sprite icon;
    }

    [SerializeField] Entry[] entries;

    Dictionary<string, Sprite> lookup;

    public Sprite GetIcon(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        EnsureLookup();
        lookup.TryGetValue(itemId, out Sprite icon);
        return icon;
    }

    void EnsureLookup()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, Sprite>();
        if (entries == null)
            return;

        for (int i = 0; i < entries.Length; i++)
        {
            Entry entry = entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.itemId) || entry.icon == null)
                continue;

            lookup[entry.itemId] = entry.icon;
        }
    }

    void OnEnable()
    {
        lookup = null;
    }
}
