using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    public static LoadingScene instance = null;

    private void Awake()
    {
        if (instance == null) instance = this;

        #if UNITY_IOS

        #endif
    }

    GameObject sliderParent;
    public Slider slider;
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI PercentageText;
    public GameObject TapToStartOBJ;
    private AsyncOperation asyncOperation;

    private void Start()
    {
        Base_Manager.Sound.Play(Sound.BGM, "Lobby");
        versionText.text = "App Version. " + Application.version;
        sliderParent = slider.transform.parent.gameObject;

    }

    private void Update()
    {
        if (asyncOperation != null)
        if (asyncOperation.progress >= 0.9f && Input.GetMouseButtonDown(0))
        {
            asyncOperation.allowSceneActivation = true;
            Base_Manager.GetGameStart = true;
        }


    }

    IEnumerator LoadDataCoroutine()
    {
        // SceneManaget.LoadScene = 즉각적으로 씬이동 , Async는 미리 로드 후에 로드가 어느정도 완료 후 씬을 이동
        asyncOperation = SceneManager.LoadSceneAsync("Main");
        asyncOperation.allowSceneActivation = false; //  씬 자동전환 방지

        while(asyncOperation.progress < 0.9f)
        {
            LoadingUpdate(asyncOperation.progress);
            yield return null;
        }
        LoadingUpdate(1.0f);
        TapToStartOBJ.SetActive(true);

    }

    public void LoadingMain()
    {
        StartCoroutine(LoadDataCoroutine());
    }

    private void LoadingUpdate(float progress)
    {
        slider.value = progress;
        PercentageText.text = string.Format(Local_Manager.local_Data["Loading"].Get_Data() + "<color=#FFFF00>{0}% ", progress * 100);
    }
}
