using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Setting : UI_Base
{
    public Slider BGM, VFX;
    public TextMeshProUGUI UniqueID;
    public Image CheckBox;

    public GameObject ChangeLanguagePanel;
    public TextMeshProUGUI NoneChangeText, GetChangeText;
    public Button RestoreButton;

    private string saveLang;
    private Button deleteAccountButton;
    private TextMeshProUGUI deleteAccountText;
    private bool deleteConfirmationPending;

    public void GetChangeLanguage(string lang)
    {
        saveLang = lang;
        ChangeLanguagePanel.SetActive(true);
        NoneChangeText.text = Local_Manager.local_Data["UI/LanguageChange"].Get_Data();
        GetChangeText.text = Local_Manager.local_Data["UI/LanguageChange"].Get_Data(lang);
    }

    public void Yes()
    {
        PlayerPrefs.SetString("LOCAL", saveLang);
        Application.Quit();
    }

    public override bool Init()
    {
        string userId = Base_Manager.Firebase.currentUser != null
            ? Base_Manager.Firebase.currentUser.UserId
            : "-";
        UniqueID.text = "UNIQUE ID : " + userId;

        BGM.value = Base_Manager.Sound.BGMValue;
        VFX.value = Base_Manager.Sound.VFXValue;

        CameraShakeCheck();
        CreateDeleteAccountButton();

#if UNITY_IOS
        RestoreButton.gameObject.SetActive(true);
        RestoreButton.onClick.AddListener(() => Base_Manager.IAP.RestorePurchase());
#endif
        return base.Init();
    }

    private void CreateDeleteAccountButton()
    {
        if (deleteAccountButton != null || RestoreButton == null)
        {
            return;
        }

        deleteAccountButton = Instantiate(RestoreButton, RestoreButton.transform.parent);
        deleteAccountButton.name = "DeleteAccount";
        deleteAccountButton.gameObject.SetActive(true);
        deleteAccountButton.onClick.RemoveAllListeners();
        deleteAccountButton.onClick.AddListener(OnDeleteAccountClicked);

        RectTransform rect = deleteAccountButton.GetComponent<RectTransform>();
        RectTransform sourceRect = RestoreButton.GetComponent<RectTransform>();
        rect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(0f, -sourceRect.rect.height - 20f);

        Image background = deleteAccountButton.GetComponent<Image>();
        if (background != null)
        {
            background.color = new Color(0.65f, 0.12f, 0.12f, 1f);
        }

        deleteAccountText = deleteAccountButton.GetComponentInChildren<TextMeshProUGUI>(true);
        SetDeleteAccountText("계정 탈퇴");
    }

    private void OnDeleteAccountClicked()
    {
        if (!deleteConfirmationPending)
        {
            deleteConfirmationPending = true;
            SetDeleteAccountText("한 번 더 눌러 계정 탈퇴");
            StartCoroutine(ResetDeleteConfirmation());
            return;
        }

        deleteConfirmationPending = false;
        deleteAccountButton.interactable = false;
        SetDeleteAccountText("계정 삭제 중...");

        Base_Manager.Firebase.DeleteCurrentAccount((success, message) =>
        {
            if (success)
            {
                ShowAccountMessage("계정과 저장 데이터가 삭제되었습니다.");
                StartCoroutine(QuitAfterAccountDeletion());
                return;
            }

            Debug.LogError(message);
            ShowAccountMessage("계정 삭제에 실패했습니다. 다시 로그인한 뒤 시도해 주세요.");
            deleteAccountButton.interactable = true;
            SetDeleteAccountText("계정 탈퇴");
        });
    }

    private IEnumerator ResetDeleteConfirmation()
    {
        yield return new WaitForSecondsRealtime(8f);

        if (deleteConfirmationPending)
        {
            deleteConfirmationPending = false;
            SetDeleteAccountText("계정 탈퇴");
        }
    }

    private IEnumerator QuitAfterAccountDeletion()
    {
        yield return new WaitForSecondsRealtime(2f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetDeleteAccountText(string value)
    {
        if (deleteAccountText != null)
        {
            deleteAccountText.text = value;
        }
    }

    private void ShowAccountMessage(string message)
    {
        if (Base_Canvas.instance != null)
        {
            Base_Canvas.instance.Get_Toast().Initialize(message, Color.white);
        }
    }

    private bool CameraShakeCheck()
    {
        bool checkBox = PlayerPrefs.GetInt("CAM") == 0;
        CheckBox.gameObject.SetActive(checkBox);
        return checkBox;
    }

    public void CameraShakeButton()
    {
        PlayerPrefs.SetInt("CAM", CameraShakeCheck() ? 1 : 0);
        CameraShakeCheck();
    }

    public void ApplicationURL(string url)
    {
        Application.OpenURL(url);
    }

    public void GetUniqueClipboard()
    {
        if (Base_Manager.Firebase.currentUser == null)
        {
            return;
        }

        GUIUtility.systemCopyBuffer = Base_Manager.Firebase.currentUser.UserId;
        Base_Canvas.instance.Get_Toast().Initialize(
            Local_Manager.local_Data["IdCopySuccess"].Get_Data(),
            Color.white);
    }

    private void Update()
    {
        Base_Manager.Sound.BGMValue = BGM.value;
        Base_Manager.Sound._audioSources[0].volume = BGM.value;
        Base_Manager.Sound.VFXValue = VFX.value;
        Base_Manager.Sound._audioSources[1].volume = VFX.value;
    }

    public override void DisableOBJ()
    {
        PlayerPrefs.SetFloat("BGM", BGM.value);
        PlayerPrefs.SetFloat("VFX", VFX.value);
        base.DisableOBJ();
    }
}
