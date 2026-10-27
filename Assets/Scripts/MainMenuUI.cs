using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene")]
    public string gameSceneName = "MainScene";

    private Font font;
    private Canvas canvas;
    private GameObject root;
    private GameObject panel;
    private Text message;
    private InputField cheatInput;
    private Text moneyLabel;
    private int classPage = 0;

    private readonly Color bg = new Color(0.035f, 0.07f, 0.12f);
    private readonly Color blue = new Color(0.05f, 0.48f, 0.8f);
    private readonly Color white = new Color(0.92f, 0.96f, 1f);

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("MainMenuCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObject.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        root = new GameObject("MenuBackground");
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        root.AddComponent<Image>().color = bg;

        ShowMain();
    }

    private GameObject NewPanel(string title)
    {
        if (panel != null)
            Destroy(panel);

        panel = new GameObject("MenuPanel");
        panel.transform.SetParent(root.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(800, 850);
        rt.anchoredPosition = Vector2.zero;

        panel.AddComponent<Image>().color = new Color(0.02f, 0.04f, 0.08f, 0.96f);
        AddText(panel.transform, title, 44, new Vector2(0, 350), new Vector2(720, 80), true);
        return panel;
    }

    private Text AddText(Transform parent, string value, int size, Vector2 pos, Vector2 dimensions, bool bold = false)
    {
        GameObject obj = new GameObject("Text");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = dimensions;

        Text t = obj.AddComponent<Text>();
        t.font = font;
        t.text = value;
        t.fontSize = size;
        t.color = white;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    private void AddButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject("Button_" + label);
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(480, 65);

        Image image = obj.AddComponent<Image>();
        image.color = blue;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        AddText(obj.transform, label, 19, Vector2.zero, new Vector2(460, 60), true);
    }

    private void ShowMain()
    {
        NewPanel("SKY ISLAND SURVIVAL");
        AddText(panel.transform, "SURVIVE THE FLOATING ISLAND", 22, new Vector2(0, 270), new Vector2(650, 50));
        AddButton(panel.transform, "START GAME", new Vector2(0, 165), () => ShowClassPage(0));
        AddButton(panel.transform, "GAME INFO", new Vector2(0, 80), ShowInfo);
        AddButton(panel.transform, "CREDITS", new Vector2(0, -5), ShowCredits);
        AddButton(panel.transform, "CHEAT CODES", new Vector2(0, -90), ShowCheats);
        AddButton(panel.transform, "EXIT GAME", new Vector2(0, -175), ExitGame);
    }

    private void ShowClassPage(int page)
    {
        classPage = Mathf.Clamp(page, 0, 3);

        switch (classPage)
        {
            case 0: ShowClasses(); break;
            case 1: ShowMoreClasses(); break;
            case 2: ShowAdvancedClasses(); break;
            case 3: ShowNewClasses(); break;
        }
    }

    private void ShowClasses()
    {
        classPage = 0;
        NewPanel("SELECT YOUR CLASS");
        AddMoneyLabel();
        AddText(panel.transform, "PAGE 1 / 4 - STARTER CLASSES", 22, new Vector2(0, 255), new Vector2(650, 40));
        AddButton(panel.transform, "SPEED RUNNER | 75 HP | Speed 8", new Vector2(0, 150), () => SelectClass("Runner"));
        AddButton(panel.transform, "NORMAL | 100 HP | Speed 6", new Vector2(0, 50), () => SelectClass("Normal"));
        AddButton(panel.transform, "TANK | 160 HP | Speed 4", new Vector2(0, -50), () => SelectClass("Tank"));
        AddButton(panel.transform, "NEXT PAGE >>", new Vector2(0, -180), () => ShowClassPage(1));
        AddButton(panel.transform, "MAIN MENU", new Vector2(0, -270), ShowMain);
    }

    private void ShowMoreClasses()
    {
        classPage = 1;
        NewPanel("SPECIAL CLASSES");
        AddMoneyLabel();
        AddText(panel.transform, "PAGE 2 / 4 - SPECIAL CLASSES", 22, new Vector2(0, 255), new Vector2(650, 40));
        AddButton(panel.transform, ClassButtonText("Soft", 150, "50 HP | Speed 9 | High Jump"), new Vector2(0, 150), () => BuyOrSelectClass("Soft", 150));
        AddButton(panel.transform, ClassButtonText("HealthEmpire", 250, "400 HP | Speed 5 | High Health Regen"), new Vector2(0, 50), () => BuyOrSelectClass("HealthEmpire", 250));
        AddButton(panel.transform, ClassButtonText("Elite", 500, "125 HP | Speed 7 | Sword 35 | -10% Money"), new Vector2(0, -50), () => BuyOrSelectClass("Elite", 500));
        AddButton(panel.transform, "<< BACK", new Vector2(-145, -180), () => ShowClassPage(0));
        AddButton(panel.transform, "NEXT >>", new Vector2(145, -180), () => ShowClassPage(2));
        AddButton(panel.transform, "MAIN MENU", new Vector2(0, -270), ShowMain);
    }

    private void ShowAdvancedClasses()
    {
        classPage = 2;
        NewPanel("ADVANCED CLASSES");
        AddMoneyLabel();
        AddText(panel.transform, "PAGE 3 / 4 - ADVANCED CLASSES", 22, new Vector2(0, 255), new Vector2(650, 40));
        AddButton(panel.transform, ClassButtonText("Vampire", 750, "120 HP | Speed 6 | Lifesteal 5%"), new Vector2(0, 150), () => BuyOrSelectClass("Vampire", 750));
        AddButton(panel.transform, ClassButtonText("GodSword", 1000, "140 HP | Speed 6.5 | Sword 50"), new Vector2(0, 50), () => BuyOrSelectClass("GodSword", 1000));
        AddButton(panel.transform, ClassButtonText("Richman", 1000, "180 HP | Speed 8 | Money +20%"), new Vector2(0, -50), () => BuyOrSelectClass("Richman", 1000));
        AddButton(panel.transform, "<< BACK", new Vector2(-145, -180), () => ShowClassPage(1));
        AddButton(panel.transform, "NEXT >>", new Vector2(145, -180), () => ShowClassPage(3));
        AddButton(panel.transform, "MAIN MENU", new Vector2(0, -270), ShowMain);
    }

    private void ShowNewClasses()
    {
        classPage = 3;
        NewPanel("NEW CLASSES");
        AddMoneyLabel();
        AddText(panel.transform, "PAGE 4 / 4 - ELITE CLASSES", 22, new Vector2(0, 255), new Vector2(650, 40));
        AddButton(panel.transform, ClassButtonText("MazeRunner", 1500, "80 HP | Speed 10 | Trap Damage -80% | Orb +50%"), new Vector2(0, 150), () => BuyOrSelectClass("MazeRunner", 1500));
        AddButton(panel.transform, ClassButtonText("Reaper", 2000, "100 HP | Speed 6 | Gain strong went kill zombie max 30"), new Vector2(0, 50), () => BuyOrSelectClass("Reaper", 2000));
        AddButton(panel.transform, ClassButtonText("Magician", 2500, "150 HP | Speed 7.5 | Anti Magic | Magic Z / X / C"), new Vector2(0, -50), () => BuyOrSelectClass("Magician", 2500));
        AddButton(panel.transform, "<< BACK", new Vector2(-145, -180), () => ShowClassPage(2));
        AddButton(panel.transform, "MAIN MENU", new Vector2(145, -180), ShowMain);
    }

    private string ClassButtonText(string className, int price, string stats)
    {
        bool unlocked = PlayerPrefs.GetInt("ClassUnlocked_" + className, 0) == 1;
        if (unlocked)
            return className + " | UNLOCKED - SELECT\n" + stats;
        return className + " | BUY $" + price + "\n" + stats;
    }

    private void AddMoneyLabel()
    {
        int balance = PlayerPrefs.GetInt("PlayerMoney", 0);
        moneyLabel = AddText(panel.transform, "YOUR MONEY: $" + balance, 26, new Vector2(0, 305), new Vector2(650, 45), true);
        moneyLabel.color = Color.yellow;
    }

    private void BuyOrSelectClass(string className, int price)
    {
        string key = "ClassUnlocked_" + className;
        bool unlocked = PlayerPrefs.GetInt(key, 0) == 1;

        if (unlocked)
        {
            SelectClass(className);
            return;
        }

        int balance = PlayerPrefs.GetInt("PlayerMoney", 0);
        if (balance < price)
        {
            ShowPurchaseMessage("Not enough money! Need $" + price);
            return;
        }

        balance -= price;
        PlayerPrefs.SetInt("PlayerMoney", balance);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        ShowClassPage(classPage);
        ShowPurchaseMessage(className + " UNLOCKED! Click again to play.");
        Debug.Log("Unlocked: " + key + " | Money left: $" + balance);
    }

    private void ShowPurchaseMessage(string text)
    {
        AddText(panel.transform, text, 18, new Vector2(0, -350), new Vector2(720, 40), true);
    }

    private void SelectClass(string className)
    {
        if (className == "Runner" || className == "Normal" || className == "Tank")
        {
            PlayerPrefs.SetString("SelectedClass", className);
            PlayerPrefs.Save();
            SceneManager.LoadScene(gameSceneName);
            return;
        }

        if (PlayerPrefs.GetInt("ClassUnlocked_" + className, 0) != 1)
        {
            Debug.LogWarning("Class has not been unlocked: " + className);
            return;
        }

        PlayerPrefs.SetString("SelectedClass", className);
        PlayerPrefs.Save();
        Debug.Log("Selected class: " + className);
        SceneManager.LoadScene(gameSceneName);
    }

    private void ShowInfo()
    {
        NewPanel("GAME INFO");
        AddText(panel.transform, "SKY ISLAND SURVIVAL\n\nExplore the maze on a floating island.\n\nW A S D : Move\nMouse : Look around\nSPACE : Jump\n\nAvoid traps and Kill zombies.\nKill the boss and survive!", 24, new Vector2(0, 70), new Vector2(650, 400));
        AddButton(panel.transform, "BACK", new Vector2(0, -250), ShowMain);
    }

    private void ShowCredits()
    {
        NewPanel("CREDITS");
        AddText(panel.transform, "GAME DEVELOPER\n\nπ“¬¿Ÿ√‘  ’À√“™ 6806397 \n\nπ“¬Õ‘∑∏‘°√   ÿ«¥‘…∞Ï 6804014\n\nCreated with Unity 6", 28, new Vector2(0, 70), new Vector2(650, 250));
        AddButton(panel.transform, "BACK", new Vector2(0, -200), ShowMain);
    }

    private void ShowCheats()
    {
        NewPanel("CHEAT CODES");
        AddText(panel.transform, "Enter a code to unlock bonus features", 21, new Vector2(0, 230), new Vector2(650, 60));

        GameObject inputObject = new GameObject("CheatInput");
        inputObject.transform.SetParent(panel.transform, false);
        RectTransform rt = inputObject.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(480, 65);
        rt.anchoredPosition = new Vector2(0, 140);
        inputObject.AddComponent<Image>().color = white;

        cheatInput = inputObject.AddComponent<InputField>();
        Text inputText = AddText(inputObject.transform, "", 24, Vector2.zero, new Vector2(450, 55));
        inputText.color = Color.black;
        cheatInput.textComponent = inputText;

        Text placeholder = AddText(inputObject.transform, "Enter code...", 22, Vector2.zero, new Vector2(450, 55));
        placeholder.color = Color.gray;
        cheatInput.placeholder = placeholder;

        message = AddText(panel.transform, "", 22, new Vector2(0, 65), new Vector2(650, 60));
        AddButton(panel.transform, "UNLOCK", new Vector2(0, -20), CheckCheat);
        AddButton(panel.transform, "RESET CHEATS", new Vector2(0, -100), ResetCheats);
        AddButton(panel.transform, "BACK", new Vector2(0, -180), ShowMain);
    }


    private void ResetCheats()
    {
        // √’‡´Áµ Cheat Codes ∑—ÈßÀ¡¥
        PlayerPrefs.DeleteKey("SpeedJumpUnlocked");
        PlayerPrefs.DeleteKey("GodModeUnlocked");
        PlayerPrefs.DeleteKey("InfManaUnlocked"); // √’‡´Áµ INFMANA

        // √’‡´Áµ‡ß‘π
        PlayerPrefs.SetInt("PlayerMoney", 0);

        // ≈ÁÕ°§≈“ ∑’Ë´◊ÈÕ‰«È„À¡Ë
        string[] classes =
        {
        "Soft", "HealthEmpire", "Elite", "Vampire", "GodSword",
        "Richman", "MazeRunner", "Reaper", "Magician"
    };

        foreach (string className in classes)
            PlayerPrefs.DeleteKey("ClassUnlocked_" + className);

        PlayerPrefs.Save();

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.money = 0;

        if (cheatInput != null)
            cheatInput.text = "";

        if (message != null)
        {
            message.text =
                "RESET COMPLETE! INFMANA and cheats reset.";
            message.color = Color.yellow;
        }

        Debug.Log("RESET COMPLETE! INFMANA and other cheats reset.");
    }


    private void CheckCheat()
    {
        if (cheatInput == null || message == null)
            return;

        string code = cheatInput.text.Trim().ToUpper();

        if (code == "SKY2026")
        {
            PlayerPrefs.SetInt("SpeedJumpUnlocked", 1);
            PlayerPrefs.Save();
            message.text = "UNLOCKED! Super Speed + High Jump";
            message.color = Color.green;
        }
        else if (code == "GODMODE")
        {
            PlayerPrefs.SetInt("GodModeUnlocked", 1);
            PlayerPrefs.Save();
            message.text = "UNLOCKED! Infinite Health";
            message.color = Color.green;
        }
        else if (code == "RICHMASTER")
        {
            int richMoney = 999999999;
            PlayerPrefs.SetInt("PlayerMoney", richMoney);
            PlayerPrefs.Save();

            if (MoneyManager.Instance != null)
                MoneyManager.Instance.money = richMoney;

            message.text = "RICHMASTER ACTIVATED! Unlimited Money!";
            message.color = Color.green;
            Debug.Log("RICHMASTER ACTIVATED! Money = " + richMoney);
        }

        else if (code == "INFMANA")
        {
            PlayerPrefs.SetInt("InfManaUnlocked", 1);
            PlayerPrefs.Save();

            message.text = "INFMANA UNLOCKED! Magician: No Cooldown";
            message.color = Color.green;

            Debug.Log("INFMANA unlocked!");
        }

        else
        {
            message.text = "Invalid code. Try again.";
            message.color = Color.red;
        }
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
