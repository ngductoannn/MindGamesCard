using UnityEngine;

public enum CardType
{
    King,
    Soldier,
    Beggar
}

[System.Serializable]
public class CardData
{
    public CardType type;
    public Sprite faceSprite;

    public CardData(CardType type, Sprite faceSprite)
    {
        this.type = type;
        this.faceSprite = faceSprite;
    }
}
