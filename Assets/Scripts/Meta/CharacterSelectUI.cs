using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Gameplay scene to load when Play is pressed (must be in Build Settings).")]
    public string mainSceneName = "Main";

    [Header("Data Sources")]
    [Tooltip("Optional: Supply a CharacterLibrary to auto-build the grid at runtime.")]
    public CharacterLibrary library;

    [System.Serializable]
    public class Choice
    {
        public string displayName;
        public GameObject playerPrefab;
        public CharacterData data;
        public Sprite portrait;
    }

    [Tooltip("Optional: Manual choices (used if 'library' is null OR in addition to it).")]
    public Choice[] manualChoices;

    [Header("UI – Grid/List")]
    [Tooltip("Parent transform (e.g., a GridLayoutGroup) where CharacterCard instances will be spawned.")]
    public Transform gridContent;
    [Tooltip("Prefab with CharacterCard component.")]
    public CharacterCard cardPrefab;

    [Header("UI – Details & Play")]
    public Image portraitImage;
    public Text displayNameText;
    [Tooltip("If left empty, will try to find a Button on this GameObject or its children.")]
    public Button playButton;

    // internal
    readonly List<Choice> _choices = new List<Choice>();
    readonly List<CharacterCard> _spawnedCards = new List<CharacterCard>();
    int _currentIndex = -1;

    void Awake()
    {
        if (!playButton)
        {
            playButton = GetComponent<Button>();
            if (!playButton) playButton = GetComponentInChildren<Button>(true);
        }
    }

    void OnEnable()
    {
        if (playButton) playButton.onClick.AddListener(OnPlayPressed);
    }

    void OnDisable()
    {
        if (playButton) playButton.onClick.RemoveListener(OnPlayPressed);
    }

    void Start()
    {
        BuildChoices();
        BuildGrid();

        // Auto-select first valid entry
        SetSelection(FindFirstValidIndex());

        if (playButton) playButton.interactable = SelectedChoice.HasSelection;
    }

    // ----- Build data -----
    void BuildChoices()
    {
        _choices.Clear();

        if (library != null && library.entries != null)
        {
            foreach (var e in library.entries)
            {
                if (e == null) continue;
                _choices.Add(new Choice
                {
                    displayName = e.displayName,
                    playerPrefab = e.playerPrefab,
                    data = e.data,
                    portrait = e.portrait
                });
            }
        }

        if (manualChoices != null && manualChoices.Length > 0)
        {
            foreach (var c in manualChoices)
            {
                if (c == null) continue;
                _choices.Add(c);
            }
        }

        if (_choices.Count == 0)
            Debug.LogWarning("[CharacterSelectUI] No characters found: fill 'library' or 'manualChoices'.");
    }

    void BuildGrid()
    {
        // clear old cards
        if (gridContent != null && _spawnedCards.Count > 0)
        {
            foreach (var c in _spawnedCards)
                if (c) Destroy(c.gameObject);
            _spawnedCards.Clear();
        }

        if (!gridContent || !cardPrefab)
        {
            if (!_choices.Exists(IsValidChoice))
                return;

            Debug.LogWarning("[CharacterSelectUI] GridContent/CardPrefab not set; using details-only view.");
            return;
        }

        for (int i = 0; i < _choices.Count; i++)
        {
            var c = _choices[i];
            var card = Instantiate(cardPrefab, gridContent);
            card.Setup(
                i,
                c?.portrait,
                !string.IsNullOrEmpty(c?.displayName) ? c.displayName : c?.data ? c.data.characterName : "Unnamed",
                OnCardClicked,
                selected: false
            );
            _spawnedCards.Add(card);
        }
    }

    int FindFirstValidIndex()
    {
        for (int i = 0; i < _choices.Count; i++)
            if (IsValidChoice(_choices[i])) return i;
        return -1;
    }

    static bool IsValidChoice(Choice c)
    {
        return c != null && c.playerPrefab != null && c.data != null;
    }

    // ----- Selection -----
    public void OnCardClicked(int index) => SetSelection(index);
    public void OnSelectIndex(int index) => SetSelection(index); // compatible with old UI buttons

    void SetSelection(int index)
    {
        if (index < 0 || index >= _choices.Count || !IsValidChoice(_choices[index]))
        {
            _currentIndex = -1;
            SelectedChoice.Clear();
            RefreshDetails(null);
            UpdateCardHighlights(-1);
            if (playButton) playButton.interactable = false;
            Debug.LogWarning("[CharacterSelectUI] Invalid selection; cleared.");
            return;
        }

        _currentIndex = index;
        var c = _choices[index];

        SelectedChoice.PlayerPrefab = c.playerPrefab;
        SelectedChoice.CharacterData = c.data;

        RefreshDetails(c);
        UpdateCardHighlights(index);
        if (playButton) playButton.interactable = true;

        string label = !string.IsNullOrEmpty(c.displayName) ? c.displayName :
                       (c.data ? c.data.characterName : "Unnamed");
        Debug.Log($"[CharacterSelectUI] Selected '{label}'. Prefab={c.playerPrefab.name}, Data={c.data.name}");
    }

    void RefreshDetails(Choice c)
    {
        if (displayNameText)
            displayNameText.text = c != null
                ? (!string.IsNullOrEmpty(c.displayName) ? c.displayName : c.data.characterName)
                : "---";

        if (portraitImage)
        {
            if (c != null && c.portrait)
            {
                portraitImage.enabled = true;
                portraitImage.sprite = c.portrait;
            }
            else
            {
                portraitImage.enabled = false;
                portraitImage.sprite = null;
            }
        }
    }

    void UpdateCardHighlights(int selectedIndex)
    {
        for (int i = 0; i < _spawnedCards.Count; i++)
        {
            if (_spawnedCards[i])
                _spawnedCards[i].SetSelected(i == selectedIndex);
        }
    }

    // ----- Play -----
    public void OnPlayPressed()
    {
        if (!SelectedChoice.HasSelection)
        {
            Debug.LogWarning("[CharacterSelectUI] No character selected; cannot start.");
            return;
        }
        if (string.IsNullOrEmpty(mainSceneName))
        {
            Debug.LogError("[CharacterSelectUI] mainSceneName not set.");
            return;
        }

        Debug.Log($"[CharacterSelectUI] Loading '{mainSceneName}' with Prefab='{SelectedChoice.PlayerPrefab?.name}', Data='{SelectedChoice.CharacterData?.name}'");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }

    // optional aliases in case you want to wire simpler names
    public void Play() => OnPlayPressed();
    public void Next()
    {
        if (_choices.Count == 0) return;
        int start = Mathf.Max(0, _currentIndex);
        for (int step = 1; step <= _choices.Count; step++)
        {
            int idx = (start + step) % _choices.Count;
            if (IsValidChoice(_choices[idx])) { SetSelection(idx); return; }
        }
    }
    public void Prev()
    {
        if (_choices.Count == 0) return;
        int start = _currentIndex < 0 ? 0 : _currentIndex;
        for (int step = 1; step <= _choices.Count; step++)
        {
            int idx = (start - step + _choices.Count) % _choices.Count;
            if (IsValidChoice(_choices[idx])) { SetSelection(idx); return; }
        }
    }
}
