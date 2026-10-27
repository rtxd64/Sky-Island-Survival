
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject[] weapons;

    [Header("Weapon UI Icons")]
    [Tooltip("Sprite ของอาวุธแต่ละช่อง ต้องเรียงตาม weapons")]
    public Sprite[] weaponIcons;

    [Tooltip("ชื่ออาวุธแต่ละช่อง เช่น SWORD, GUN")]
    public string[] weaponNames;

    [Header("UI Appearance")]
    public Vector2 iconSize = new Vector2(100, 100);
    public Vector2 bottomOffset = new Vector2(0, 45);
    public Color glowColor = Color.cyan;
    public Color normalColor = Color.white;

    private int currentWeapon = -1;

    private Image weaponImage;
    private Text weaponLabel;
    private Outline iconOutline;
    private Shadow iconGlow;

    public int CurrentWeapon => currentWeapon;

    void Start()
    {
        CreateWeaponUI();

        if (weapons != null && weapons.Length > 0)
        {
            SwitchWeapon(0);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(1);
        }
    }

    public void SwitchWeapon(int weaponIndex)
    {
        if (weapons == null ||
            weaponIndex < 0 ||
            weaponIndex >= weapons.Length)
        {
            return;
        }

        if (weapons[weaponIndex] == null)
        {
            Debug.LogWarning(
                "Weapon slot " + weaponIndex + " is empty."
            );
            return;
        }

        // ปิดอาวุธทุกชิ้นก่อน
        foreach (GameObject weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.SetActive(false);
            }
        }

        // เปิดอาวุธที่เลือก
        weapons[weaponIndex].SetActive(true);
        currentWeapon = weaponIndex;

        UpdateWeaponUI();

        Debug.Log("Current weapon: " + GetWeaponName());
    }

    private string GetWeaponName()
    {
        if (weaponNames != null &&
            currentWeapon >= 0 &&
            currentWeapon < weaponNames.Length &&
            !string.IsNullOrEmpty(weaponNames[currentWeapon]))
        {
            return weaponNames[currentWeapon];
        }

        return "WEAPON " + (currentWeapon + 1);
    }

    private void CreateWeaponUI()
    {
        // สร้าง Canvas สำหรับไอคอนอาวุธ
        GameObject canvasObject =
            new GameObject("WeaponHUDCanvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1920, 1080);

        canvasObject.AddComponent<GraphicRaycaster>();

        // กล่อง UI ด้านล่างกลาง
        GameObject holder = new GameObject("WeaponHUD");
        holder.transform.SetParent(canvas.transform, false);

        RectTransform holderRect =
            holder.AddComponent<RectTransform>();

        holderRect.anchorMin = new Vector2(0.5f, 0f);
        holderRect.anchorMax = new Vector2(0.5f, 0f);
        holderRect.pivot = new Vector2(0.5f, 0f);
        holderRect.anchoredPosition = bottomOffset;
        holderRect.sizeDelta = new Vector2(240, 145);

        // พื้นหลังโปร่งใสสีดำ
        Image background = holder.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.45f);

        // รูปอาวุธ
        GameObject iconObject = new GameObject("WeaponIcon");
        iconObject.transform.SetParent(holder.transform, false);

        RectTransform iconRect =
            iconObject.AddComponent<RectTransform>();

        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 18);
        iconRect.sizeDelta = iconSize;

        weaponImage = iconObject.AddComponent<Image>();
        weaponImage.preserveAspect = true;
        weaponImage.color = normalColor;

        // เพิ่มเอฟเฟกต์เรืองแสงรอบไอคอน
        iconGlow = iconObject.AddComponent<Shadow>();
        iconGlow.effectColor = glowColor;
        iconGlow.effectDistance = new Vector2(0, 0);
        iconGlow.useGraphicAlpha = true;

        iconOutline = iconObject.AddComponent<Outline>();
        iconOutline.effectColor = glowColor;
        iconOutline.effectDistance = new Vector2(3, 3);
        iconOutline.useGraphicAlpha = true;

        // ชื่ออาวุธใต้ไอคอน
        GameObject labelObject = new GameObject("WeaponName");
        labelObject.transform.SetParent(holder.transform, false);

        RectTransform labelRect =
            labelObject.AddComponent<RectTransform>();

        labelRect.anchorMin = new Vector2(0.5f, 0f);
        labelRect.anchorMax = new Vector2(0.5f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = new Vector2(0, 5);
        labelRect.sizeDelta = new Vector2(220, 30);

        weaponLabel = labelObject.AddComponent<Text>();
        weaponLabel.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        weaponLabel.fontSize = 22;
        weaponLabel.fontStyle = FontStyle.Bold;
        weaponLabel.alignment = TextAnchor.MiddleCenter;
        weaponLabel.color = glowColor;

        UpdateWeaponUI();
    }

    private void UpdateWeaponUI()
    {
        if (weaponImage == null || weaponLabel == null)
            return;

        bool hasIcon =
            weaponIcons != null &&
            currentWeapon >= 0 &&
            currentWeapon < weaponIcons.Length &&
            weaponIcons[currentWeapon] != null;

        weaponImage.sprite = hasIcon
            ? weaponIcons[currentWeapon]
            : null;

        weaponImage.enabled = hasIcon;

        weaponLabel.text = currentWeapon >= 0
            ? GetWeaponName()
            : "NO WEAPON";

        if (iconGlow != null)
        {
            iconGlow.effectColor = glowColor;
        }

        if (iconOutline != null)
        {
            iconOutline.effectColor = glowColor;
        }

        weaponLabel.color = glowColor;
    }
}
