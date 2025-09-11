using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterLibrary", menuName = "Game/Character Library", order = 0)]
public class CharacterLibrary : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string displayName;
        public GameObject playerPrefab;
        public CharacterData data;
        public Sprite portrait;
    }

    public List<Entry> entries = new List<Entry>();
}
