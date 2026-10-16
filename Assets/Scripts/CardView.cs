using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Sprite cardBackSprite;

    public CardData Data { get; private set; }
    private Action<CardView> onClicked;

    
public void Setup(CardData data, Action<CardView> clickCallback, bool showFace)
    {
        Data = data;
        onClicked = clickCallback;

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => onClicked?.Invoke(this));
        }

        if (showFace)
            Reveal();
        else
            ShowBack();
    }


    
    public void ShowBack()
    {
        if (cardImage != null)
        {
            cardImage.sprite = cardBackSprite;
            cardImage.color = Color.white;
        }

        if (labelText != null)
            labelText.text = "";
    }

    public void Reveal()
    {
        if (cardImage != null)
        {
            cardImage.sprite = Data != null && Data.faceSprite != null
                ? Data.faceSprite
                : cardBackSprite;

            cardImage.color = Color.white;
        }

        if (labelText != null)
            labelText.text = Data != null ? GetVietnameseName(Data.type) : "";
    }


    public void SetInteractable(bool value)
    {
        if (cardButton != null)
            cardButton.interactable = value;

        if (cardImage != null)
            cardImage.color = Color.white;
    }

    public static string GetVietnameseName(CardType type)
    {
        switch (type)
        {
            case CardType.King: return "VUA";
            case CardType.Soldier: return "LÍNH";
            case CardType.Beggar: return "ĂN MÀY";
            default: return "";
        }
    }
}
