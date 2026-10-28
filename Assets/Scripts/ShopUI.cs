
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopUI : MonoBehaviour
{
    private GameObject shopPanel;
    private Text moneyText;
    private Text statusText;
    private Font font;
    private bool isOpen;

    private PlayerHealth playerHealth;

    [Header("Weapon HUD")]
    [Tooltip("ลาก GameObject ของ UI สลับปืน/ดาบมาใส่ช่องนี้")]
    [SerializeField] private GameObject weaponHUD;

    private bool weaponHUDWasActive;

    [Header("Shop Item Images")]
    public Sprite speedPotionIcon;
    public Sprite resistancePotionIcon;
    public Sprite damageSprayIcon;
    public Sprite healingPotionIcon;
    public Sprite medicalKitIcon;

    private readonly Color background =
        new Color(0.02f, 0.05f, 0.09f, 0.97f);

    private readonly Color buttonColor =
        new Color(0.05f, 0.42f, 0.72f);

    private void Start()
    {
        isOpen = false;

        font = Resources.GetBuiltinResource<Font>(
            "LegacyRuntime.ttf");

        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventObject = new GameObject("EventSystem");
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<StandaloneInputModule>();
        }

        CreateShop();
        CreateShopIcon();
        SetShopVisible(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            ToggleShop();

        if (isOpen)
        {
            UpdateMoneyText();

            if (Input.GetKeyDown(KeyCode.Escape))
                SetShopVisible(false);
        }
    }

    private void CreateShop()
    {
        GameObject canvasObject = new GameObject("ShopCanvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObject.AddComponent<GraphicRaycaster>();

        shopPanel = new GameObject("ShopPanel");
        shopPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect =
            shopPanel.AddComponent<RectTransform>();

        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760, 760);
        panelRect.anchoredPosition = Vector2.zero;

        shopPanel.AddComponent<Image>().color = background;

        AddText(
            shopPanel.transform,
            "SURVIVAL SHOP",
            38,
            new Vector2(0, 320),
            new Vector2(680, 60),
            true);

        moneyText = AddText(
            shopPanel.transform,
            "MONEY: $0",
            26,
            new Vector2(0, 270),
            new Vector2(650, 45),
            true);

        moneyText.color = Color.yellow;

        AddText(
            shopPanel.transform,
            "POTIONS & MEDICAL ITEMS",
            20,
            new Vector2(0, 225),
            new Vector2(650, 40));

        AddShopButton(
            "Speed Potion | $50 | Speed +50% / 60 sec",
            150,
            speedPotionIcon,
            () => BuySpeedPotion());

        AddShopButton(
            "Resistance Potion | $75 | Damage -25% / 60 sec",
            80,
            resistancePotionIcon,
            () => BuyResistancePotion());

        AddShopButton(
            "Damage Spray | $100 | Damage +50% / 60 sec",
            10,
            damageSprayIcon,
            () => BuyDamagePotion());

        AddShopButton(
            "Healing Potion | $50 | Heal 30% Max HP",
            -60,
            healingPotionIcon,
            () => BuyHealingPotion());

        AddShopButton(
            "Medical Kit | $150 | Restore Full HP",
            -130,
            medicalKitIcon,
            () => BuyMedicalKit());

        statusText = AddText(
            shopPanel.transform,
            "",
            19,
            new Vector2(0, -205),
            new Vector2(680, 45),
            true);

        AddButton(
            shopPanel.transform,
            "CLOSE SHOP [I / ESC]",
            new Vector2(0, -285),
            () => SetShopVisible(false));
    }

    private void CreateShopIcon()
    {
        GameObject iconObject = new GameObject("ShopIcon");

        iconObject.transform.SetParent(
            shopPanel.transform.parent, false);

        RectTransform rect =
            iconObject.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(20, 0);
        rect.sizeDelta = new Vector2(100, 75);

        Image image = iconObject.AddComponent<Image>();
        image.color = buttonColor;

        Button button = iconObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ToggleShop);

        Text label = AddText(
            iconObject.transform,
            "SHOP\n[I]",
            20,
            Vector2.zero,
            new Vector2(100, 70),
            true);

        label.raycastTarget = false;
    }

    private void AddShopButton(
        string label,
        float y,
        Sprite icon,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject("ShopItem_" + label);
        obj.transform.SetParent(shopPanel.transform, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(650, 58);

        Image backgroundImage = obj.AddComponent<Image>();
        backgroundImage.color = buttonColor;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = backgroundImage;
        button.onClick.AddListener(action);

        GameObject iconObject = new GameObject("ItemIcon");
        iconObject.transform.SetParent(obj.transform, false);

        RectTransform iconRect =
            iconObject.AddComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(10, 0);
        iconRect.sizeDelta = new Vector2(44, 44);

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        GameObject textObject = new GameObject("ItemText");
        textObject.transform.SetParent(obj.transform, false);

        RectTransform textRect =
            textObject.AddComponent<RectTransform>();

        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(65, 2);
        textRect.offsetMax = new Vector2(-5, -2);

        Text text = textObject.AddComponent<Text>();
        text.font = font;
        text.text = label;
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
    }

    private void AddButton(
        Transform parent,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject("Button_" + label);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(650, 58);

        Image image = obj.AddComponent<Image>();
        image.color = buttonColor;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        Text text = AddText(
            obj.transform,
            label,
            18,
            Vector2.zero,
            new Vector2(630, 55),
            true);

        text.raycastTarget = false;
    }

    private Text AddText(
        Transform parent,
        string value,
        int size,
        Vector2 position,
        Vector2 dimensions,
        bool bold = false)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        Text text = obj.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = bold
            ? FontStyle.Bold
            : FontStyle.Normal;

        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        return text;
    }

    private void ToggleShop()
    {
        SetShopVisible(!isOpen);
    }


    private void SetShopVisible(bool visible)
    {
        isOpen = visible;

        if (shopPanel != null)
            shopPanel.SetActive(visible);

        // ไม่ปิด WeaponController หรือระบบควบคุมอาวุธ
        if (weaponHUD != null)
            weaponHUD.SetActive(!visible);

        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UpdateMoneyText();
            SetStatus("");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    private void UpdateMoneyText()
    {
        int money = PlayerPrefs.GetInt("PlayerMoney", 0);

        if (MoneyManager.Instance != null)
            money = MoneyManager.Instance.money;

        if (moneyText != null)
            moneyText.text = "MONEY: $" + money;
    }

    private void SetStatus(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    private bool Pay(int price)
    {
        if (MoneyManager.Instance != null)
        {
            if (!MoneyManager.Instance.SpendMoney(price))
            {
                SetStatus("NOT ENOUGH MONEY!");
                return false;
            }
        }
        else
        {
            int money = PlayerPrefs.GetInt("PlayerMoney", 0);

            if (money < price)
            {
                SetStatus("NOT ENOUGH MONEY!");
                return false;
            }

            PlayerPrefs.SetInt("PlayerMoney", money - price);
            PlayerPrefs.Save();
        }

        UpdateMoneyText();
        return true;
    }

    private PlayerHealth GetHealth()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        return playerHealth;
    }

    private bool BuySpeedPotion()
    {
        if (!Pay(50))
            return false;

        if (PlayerPotionEffects.Instance == null)
        {
            SetStatus("Missing PlayerPotionEffects in scene!");
            Refund(50);
            return false;
        }

        PlayerPotionEffects.Instance.DrinkSpeedPotion();
        SetStatus("Speed Potion activated for 60 seconds!");
        return true;
    }

    private bool BuyResistancePotion()
    {
        if (!Pay(75))
            return false;

        if (PlayerPotionEffects.Instance == null)
        {
            SetStatus("Missing PlayerPotionEffects in scene!");
            Refund(75);
            return false;
        }

        PlayerPotionEffects.Instance.DrinkResistancePotion();
        SetStatus("Resistance Potion activated for 60 seconds!");
        return true;
    }

    private bool BuyDamagePotion()
    {
        if (!Pay(100))
            return false;

        if (PlayerPotionEffects.Instance == null)
        {
            SetStatus("Missing PlayerPotionEffects in scene!");
            Refund(100);
            return false;
        }

        PlayerPotionEffects.Instance.DrinkDamagePotion();
        SetStatus("Damage Spray activated for 60 seconds!");
        return true;
    }

    private bool BuyHealingPotion()
    {
        PlayerHealth health = GetHealth();

        if (health == null)
        {
            SetStatus("PlayerHealth not found!");
            return false;
        }

        if (health.currentHealth >= health.maxHealth)
        {
            SetStatus("Your health is already full!");
            return false;
        }

        if (!Pay(50))
            return false;

        int amount = Mathf.CeilToInt(health.maxHealth * 0.3f);
        health.Heal(amount);

        SetStatus("Healing Potion used: +" + amount + " HP");
        return true;
    }

    private bool BuyMedicalKit()
    {
        PlayerHealth health = GetHealth();

        if (health == null)
        {
            SetStatus("PlayerHealth not found!");
            return false;
        }

        if (health.currentHealth >= health.maxHealth)
        {
            SetStatus("Your health is already full!");
            return false;
        }

        if (!Pay(150))
            return false;

        health.Heal(health.maxHealth);

        SetStatus("Medical Kit used: FULL HP!");
        return true;
    }

    private void Refund(int amount)
    {
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.AddMoney(amount);
        }
        else
        {
            PlayerPrefs.SetInt(
                "PlayerMoney",
                PlayerPrefs.GetInt("PlayerMoney", 0) + amount);

            PlayerPrefs.Save();
        }

        UpdateMoneyText();
    }
}
