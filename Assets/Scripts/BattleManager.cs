using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public enum Winner { None, A, B, Draw }

    [Header("Card sprites")]
    [SerializeField] private Sprite kingSprite;
    [SerializeField] private Sprite soldierSprite;
    [SerializeField] private Sprite beggarSprite;
    [SerializeField] private Sprite cardBackSprite;

    [Header("Hand containers - each has five card slots")]
    [SerializeField] private Transform handA;
    [SerializeField] private Transform handB;
    [SerializeField] private CardView cardPrefab;

    [Header("Selected cards display")]
    [SerializeField] private Image selectedCardAImage;
    [SerializeField] private TMP_Text selectedCardALabel;
    [SerializeField] private Image selectedCardBImage;
    [SerializeField] private TMP_Text selectedCardBLabel;

    [Header("Status and buttons")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button revealButton;
    [SerializeField] private Button restartButton;

    private readonly List<CardView> cardsA = new List<CardView>();
    private readonly List<CardView> cardsB = new List<CardView>();

    private CardView selectedA;
    private CardView selectedB;
    private bool choosingA = true;
    private bool readyToReveal;
    private bool gameOver;
    public Text resultText;
    public GameObject backToMenuButton;

    private void GoBackToMenu()
    {
        SceneManager.LoadScene("MENU");
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MENU");
    }

    private void Start()
    {
        if (backToMenuButton != null)
            backToMenuButton.SetActive(false);
        if (resultText != null)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }
        if (selectedCardAImage != null)
            selectedCardAImage.gameObject.SetActive(false);

        if (selectedCardBImage != null)
            selectedCardBImage.gameObject.SetActive(false);

        if (revealButton != null)
        {
            revealButton.onClick.RemoveListener(RevealAndResolve);
            revealButton.onClick.AddListener(RevealAndResolve);
            revealButton.gameObject.SetActive(false);
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        StartNewGame();
    }

    public void StartNewGame()
    {
        ClearCards();
        selectedA = null;
        selectedB = null;
        choosingA = true;
        readyToReveal = false;
        gameOver = false;

        SetSelectedDisplay(selectedCardAImage, selectedCardALabel, null, false);
        SetSelectedDisplay(selectedCardBImage, selectedCardBLabel, null, false);

        CreateHand(handA, cardsA, new[] {
            CardType.King, CardType.Soldier, CardType.Soldier,
            CardType.Soldier, CardType.Soldier
        });

        CreateHand(handB, cardsB, new[] {
            CardType.Beggar, CardType.Soldier, CardType.Soldier,
            CardType.Soldier, CardType.Soldier
        });

        if (statusText != null) statusText.text = "Lượt bên A: chọn 1 lá bài.";
        if (revealButton != null) revealButton.interactable = false;
        if (selectedCardAImage != null)
        selectedCardAImage.gameObject.SetActive(false);

        if (selectedCardBImage != null)
            selectedCardBImage.gameObject.SetActive(false);

        foreach (CardView c in cardsA)
            if (c != null) c.SetInteractable(true);

        foreach (CardView c in cardsB)
            if (c != null) c.SetInteractable(false);
    }

    
    private void CreateHand(Transform container, List<CardView> list, CardType[] deck)
    {
        
        if (container == null || cardPrefab == null)
        {
            Debug.LogError("BattleManager: gán Hand A, Hand B và Card Prefab trong Inspector.");
            return;
        }

        foreach (CardType type in deck)
        {
            CardView view = Instantiate(cardPrefab, container);

            // A ngửa bài ban đầu, B úp bài ban đầu
            bool showFace = (container == handA);

            view.Setup(
                new CardData(type, GetSprite(type)),
                OnCardClicked,
                showFace
            );

            // Ban đầu chỉ cho A bấm
            view.SetInteractable(showFace);

            list.Add(view);
        }
    }


        private Sprite GetSprite(CardType type)
        {
            switch (type)
            {
                case CardType.King: return kingSprite;
                case CardType.Soldier: return soldierSprite;
                case CardType.Beggar: return beggarSprite;
                default: return null;
            }
        }

    

    private void OnCardClicked(CardView card)
    {
        if (gameOver || readyToReveal || card == null)
            return;

        // A đang chọn
        if (choosingA)
        {
            if (!cardsA.Contains(card) || selectedA != null)
                return;

            selectedA = card;
            CardData chosenData = selectedA.Data;

            // Bỏ lá đã chọn khỏi hàng A
            cardsA.Remove(card);
            card.gameObject.SetActive(false);

            // Úp toàn bộ bài A còn lại
            foreach (CardView c in cardsA)
            {
                if (c != null)
                {
                    c.ShowBack();
                    c.SetInteractable(false);
                }
            }

            // Lá A đã chọn ở giữa vẫn úp
            if (selectedCardAImage != null)
            {
                selectedCardAImage.gameObject.SetActive(true);
                SetSelectedDisplay(
                    selectedCardAImage,
                    selectedCardALabel,
                    chosenData,
                    false
                );
            }

            // Chuyển lượt sang B
            choosingA = false;

            // Mở toàn bộ 5 lá B để B chọn
            foreach (CardView c in cardsB)
            {
                if (c != null)
                {
                    c.Reveal();
                    c.SetInteractable(true);
                }
            }

            if (statusText != null)
                statusText.text = "A đã chọn. Đến lượt B.";

            return;
        }

        // B đang chọn
        if (!cardsB.Contains(card) || selectedB != null)
            return;

        selectedB = card;
        CardData chosenDataB = selectedB.Data;

        // Bỏ lá đã chọn khỏi hàng B
        cardsB.Remove(card);
        card.gameObject.SetActive(false);

        // Úp toàn bộ bài B còn lại
        foreach (CardView c in cardsB)
        {
            if (c != null)
            {
                c.ShowBack();
                c.SetInteractable(false);
            }
        }

        // Lá B đã chọn ở giữa vẫn úp
        if (selectedCardBImage != null)
        {
            selectedCardBImage.gameObject.SetActive(true);
            SetSelectedDisplay(
                selectedCardBImage,
                selectedCardBLabel,
                chosenDataB,
                false
            );
        }

        readyToReveal = true;

        if (revealButton != null)
        {
            revealButton.gameObject.SetActive(true);
            revealButton.interactable = true;
        }

        if (statusText != null)
            statusText.text = "B đã chọn. Bấm LẬT BÀI!";
    }


    public void RevealAndResolve()
    {
        if (!readyToReveal || selectedA == null ||
            selectedB == null || gameOver)
            return;

        // Lật ngửa hai lá đã chọn ở giữa
        SetSelectedDisplay(
            selectedCardAImage,
            selectedCardALabel,
            selectedA.Data,
            true
        );

        SetSelectedDisplay(
            selectedCardBImage,
            selectedCardBLabel,
            selectedB.Data,
            true
        );

        // So sánh hai lá
        Winner result = CheckResult(
            selectedA.Data.type,
            selectedB.Data.type
        );

        // Có bên thắng
        if (result == Winner.A || result == Winner.B)
        {
            if (backToMenuButton != null)
                backToMenuButton.SetActive(true);
                
            gameOver = true;

            string message = result == Winner.A
                ? "A Win!"
                : "B Win!";

            Color winColor = result == Winner.A
                ? Color.blue
                : Color.red;

            if (statusText != null)
                statusText.text = message;

            if (resultText != null)
            {
                resultText.text = message;
                resultText.color = winColor;
                resultText.gameObject.SetActive(true);
            }

            SetAllCardsInteractable(false);

            // Ẩn nút LẬT BÀI khi trận kết thúc
            if (revealButton != null)
                revealButton.gameObject.SetActive(false);

            // hiện về menu

            return;
        }

        // Hòa: hiện thông báo DRAW!
        if (resultText != null)
        {
            resultText.text = "DRAW!";
            resultText.color = Color.green;
            resultText.gameObject.SetActive(true);

            StartCoroutine(HideResultAfterDelay(2f));
        }

        if (statusText != null)
            statusText.text = "HÒA LƯỢT! Hai lá đã bỏ.";

        // Xóa hai lá đã chọn
        if (selectedA != null)
        {
            Destroy(selectedA.gameObject);
            selectedA = null;
        }

        if (selectedB != null)
        {
            Destroy(selectedB.gameObject);
            selectedB = null;
        }

        readyToReveal = false;
        choosingA = true;

        // Ẩn hai lá ở giữa
        if (selectedCardAImage != null)
            selectedCardAImage.gameObject.SetActive(false);

        if (selectedCardBImage != null)
            selectedCardBImage.gameObject.SetActive(false);

        // Ẩn nút LẬT BÀI khi bắt đầu lượt mới
        if (revealButton != null)
            revealButton.gameObject.SetActive(false);

        // Lượt mới: A mở bài và được chọn
        foreach (CardView c in cardsA)
        {
            if (c != null)
            {
                c.gameObject.SetActive(true);
                c.Reveal();
                c.SetInteractable(true);
            }
        }

        // B úp bài và chưa được chọn
        foreach (CardView c in cardsB)
        {
            if (c != null)
            {
                c.gameObject.SetActive(true);
                c.ShowBack();
                c.SetInteractable(false);
            }
        }

        if (statusText != null)
            statusText.text += "\nLượt mới: bên A chọn bài.";
    }

    private System.Collections.IEnumerator HideResultAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (resultText != null && !gameOver)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }
    }


    public Winner CheckResult(CardType cardA, CardType cardB)
    {
        // Vua gặp Ăn mày: B thắng ngay.
        if (cardA == CardType.King && cardB == CardType.Beggar)
            return Winner.B;

        // Vua gặp Lính: A thắng ngay.
        if (cardA == CardType.King && cardB == CardType.Soldier)
            return Winner.A;

        // Lính gặp Vua: A thắng ngay.
        if (cardA == CardType.Soldier && cardB == CardType.King)
            return Winner.A;

        // Lính gặp Lính: hòa lượt.
        if (cardA == CardType.Soldier && cardB == CardType.Soldier)
            return Winner.Draw;

        // Ăn mày gặp Lính: Vua (bên A) thắng ngay.
        if ((cardA == CardType.Beggar && cardB == CardType.Soldier) ||
            (cardA == CardType.Soldier && cardB == CardType.Beggar))
            return Winner.A;

        // Ăn mày gặp Vua: B thắng ngay.
        if (cardA == CardType.Beggar && cardB == CardType.King)
            return Winner.B;

        return Winner.None;
    }

    private void RemoveUsedCard(List<CardView> hand, CardView card)
    {
        if (card == null) return;
        hand.Remove(card);
        Destroy(card.gameObject);
    }

    private void SetAllCardsInteractable(bool value)
    {
        foreach (CardView c in cardsA) if (c != null) c.SetInteractable(value);
        foreach (CardView c in cardsB) if (c != null) c.SetInteractable(value);
    }

    private void SetSelectedDisplay(Image image, TMP_Text label, CardData data, bool reveal)
    {
        if (image != null)
            image.sprite = data == null || !reveal ? cardBackSprite : data.faceSprite;

        if (label != null)
            label.text = data == null ? "CHƯA CHỌN"
                : (reveal ? CardView.GetVietnameseName(data.type) : "BÀI ÚP");
    }

    private void ClearCards()
    {
        foreach (CardView c in cardsA) if (c != null) Destroy(c.gameObject);
        foreach (CardView c in cardsB) if (c != null) Destroy(c.gameObject);
        cardsA.Clear();
        cardsB.Clear();
    }

    private void RestartGame()
    {
        StartNewGame();
    }
}
