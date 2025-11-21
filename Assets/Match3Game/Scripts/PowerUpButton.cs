using UnityEngine;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour
{
    public Board board;
    public PowerUpState powerUpType;
    public float cooldownTime = 30f;
    public int randomDestroyCount = 5; // Số lượng dot bị phá hủy ngẫu nhiên

    private float cooldownTimer = 0f;
    private Button button;
    private Image fillImage;

    void Start()
    {
        if (board == null)
        {
            board = FindObjectOfType<Board>();
        }

        button = GetComponent<Button>();
        fillImage = transform.Find("CooldownFill")?.GetComponent<Image>();

        button.onClick.AddListener(OnButtonClick);

        cooldownTimer = 0f;
        UpdateButtonState();
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            if (fillImage != null)
            {
                fillImage.fillAmount = cooldownTimer / cooldownTime;
            }

            if (cooldownTimer <= 0)
            {
                cooldownTimer = 0;
                UpdateButtonState();
            }
        }
    }

    void OnButtonClick()
    {
        if (cooldownTimer <= 0 && board != null && board.currentState == GameState.move)
        {
            // Hủy power-up hiện tại nếu có
            board.CancelCurrentPowerUp();

            // Activate the new power-up
            switch (powerUpType)
            {
                case PowerUpState.ColorDestroy:
                    board.ActivateColorDestroy();
                    break;

                case PowerUpState.SwapRandom:
                    board.ActivateSwapRandom();
                    break;

                case PowerUpState.ColorChange:
                    board.ActivateColorChange();
                    break;

                case PowerUpState.DestroyDot:
                    board.ActivateDestroyDot();
                    break;

                case PowerUpState.DestroyColumn:
                    board.ActivateDestroyColumn();
                    break;

                case PowerUpState.DestroyRow:
                    board.ActivateDestroyRow();
                    break;

                case PowerUpState.DestroyCross:
                    board.ActivateDestroyCross();
                    break;

                case PowerUpState.Destroy3x3:
                    board.ActivateDestroy3x3();
                    break;

                case PowerUpState.SwapColumn:
                    board.ActivateSwapColumn();
                    break;

                case PowerUpState.SwapRow:
                    board.ActivateSwapRow();
                    break;

                case PowerUpState.ShuffleBoard:
                    board.ShuffleBoard();
                    break;

                case PowerUpState.None:
                    // For random destroy button
                    board.DestroyRandomDots(randomDestroyCount);
                    break;
            }

            // Start cooldown
            cooldownTimer = cooldownTime;
            UpdateButtonState();
        }
    }

    void UpdateButtonState()
    {
        if (button != null)
        {
            button.interactable = (cooldownTimer <= 0);
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = cooldownTimer / cooldownTime;
        }
    }
}