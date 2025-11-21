using UnityEngine;
using UnityEngine.UI;

public class DestroyAllButton : MonoBehaviour
{
    public Board board;
    public float cooldownTime = 30f; // Thời gian chờ giữa các lần sử dụng (30 giây)
    private float cooldownTimer = 0f;
    private Button button;
    private Image fillImage; // Image để hiển thị cooldown

    void Start()
    {
        // Tìm board nếu chưa được gán
        if (board == null)
        {
            board = FindObjectOfType<Board>();
        }

        // Lấy component Button
        button = GetComponent<Button>();

        // Tìm Image con có tag "CooldownFill"
        fillImage = transform.Find("CooldownFill")?.GetComponent<Image>();

        // Thêm listener cho button
        button.onClick.AddListener(OnButtonClick);

        // Set cooldown timer
        cooldownTimer = 0f;
        UpdateButtonState();
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;

            // Cập nhật fill amount của image
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
            // Kích hoạt hiệu ứng phá hủy
            board.DestroyAllDots();

            // Set cooldown
            cooldownTimer = cooldownTime;
            UpdateButtonState();
        }
    }

    void UpdateButtonState()
    {
        // Enable/disable button dựa trên cooldown
        if (button != null)
        {
            button.interactable = (cooldownTimer <= 0);
        }

        // Cập nhật fill image
        if (fillImage != null)
        {
            fillImage.fillAmount = cooldownTimer / cooldownTime;
        }
    }
}