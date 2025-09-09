using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Roster")]
    public CharacterData[] characters;

    [Header("UI")]
    public CanvasGroup group;        // overall panel
    public Transform listParent;     // VerticalLayoutGroup
    public Button buttonTemplate;    // a Button with a child Text

    void Start()
    {
        Time.timeScale = 0f; // pause while selecting (since you're in a UI scene it's harmless)
        Build();
        Show(true);
    }

    void Build()
    {
        if (!buttonTemplate || !listParent)
        {
            Debug.LogError("[CharacterSelectUI] Missing references.");
            return;
        }
        buttonTemplate.gameObject.SetActive(false);

        foreach (Transform c in listParent)
            if (c != buttonTemplate.transform) Destroy(c.gameObject);

        foreach (var cd in characters)
        {
            if (!cd) continue;
            var btn = Instantiate(buttonTemplate, listParent);
            btn.gameObject.SetActive(true);
            var txt = btn.GetComponentInChildren<Text>();
            if (txt) txt.text = cd.characterName;

            btn.onClick.AddListener(() =>
            {
                if (!SelectedCharacter.I)
                {
                    var root = new GameObject("SelectedCharacterRoot");
                    root.AddComponent<SelectedCharacter>();
                }
                SelectedCharacter.I.Choose(cd);
            });
        }
    }

    void Show(bool on)
    {
        if (!group) return;
        group.alpha = on ? 1f : 0f;
        group.interactable = on;
        group.blocksRaycasts = on;
    }
}
