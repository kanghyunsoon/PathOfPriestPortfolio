using UnityEngine;

public class FixedResolution : MonoBehaviour
{
    public int targetWidth = 1440;
    public int targetHeight = 2560;

    private void Start()
    {
        Debug.Log("실행됨");
        ApplyFixedResolution();
    }

    private void ApplyFixedResolution()
    {
        float targetAspect = (float)targetWidth / targetHeight; // 목표화면비율
        float windowAspect = (float)Screen.width / Screen.height; // 현재화면 비율
        float scaleHeight = windowAspect / targetAspect;

        Camera mainCamera = Camera.main;

        if(scaleHeight < 1.0f) // 검은여백이 위아래로 생김
        {
            Rect rect = mainCamera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            mainCamera.rect = rect;
        }
        else // 좌우 검은여백
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = mainCamera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            mainCamera.rect = rect;
        }
        Screen.SetResolution(targetWidth, targetHeight, true);
    }

    //유니티에서 값변경시 즉시적용
    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            Debug.Log("실행됨");
            ApplyFixedResolution();
        }
    }
}
