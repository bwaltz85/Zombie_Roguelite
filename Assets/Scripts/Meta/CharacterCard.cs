using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{
    [Header("UI")]
    public Image portraitImage;
    public Text nameText;
    public Button selectButton;
    public Image selectionHighlight;

    private int _index = -1;
    private Action<int> _onClick;

    void Awake()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(() => { if (_onClick != null) _onClick(_index); });
    }

    public void Setup(int index, Sprite portrait, string displayName, Action<int> onClick, bool selected)
    {
        _index = index;
        _onClick = onClick;

        if (portraitImage)
        {
            portraitImage.enabled = portrait != null;
            portraitImage.sprite = portrait;
        }
        if (nameText)
            nameText.text = string.IsNullOrWhiteSpace(displayName) ? "Unnamed" : displayName;

        SetSelected(selected);
    }

    public void SetSelected(bool selected)
    {
        if (selectionHighlight) selectionHighlight.enabled = selected;
    }
}
