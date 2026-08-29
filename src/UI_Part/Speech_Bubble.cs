using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class Speech_Bubble : MonoBehaviour
{
    string[] Speechs;
    [SerializeField] private TextMeshProUGUI SpeechText;
    [SerializeField] private Transform m_Transform;
    [SerializeField] private Vector3 m_Pos;
    Camera cam;

    public void Init(Transform transform, Vector3 pos, string[] methodes, Camera camera)
    {

        cam = camera;
        Speechs = methodes;
        m_Transform = transform;
        m_Pos = pos;
        StartCoroutine(SpeechTextCoroutine());
    }

    private void LateUpdate()
    {
        if (m_Transform == null || m_Transform.gameObject == null) //캐릭터가 삭제되었는지 확인
        {
            Destroy(gameObject); //말풍선도 삭제
            return;
        }

        if (cam != null)
        transform.position = cam.WorldToScreenPoint(m_Transform.position + m_Pos);
    }

    IEnumerator SpeechTextCoroutine()
    {
        string temp = Speechs[Random.Range(0, Speechs.Length)];

        // 로컬라이즈 딕셔너리가 아직 준비되지 않았을 가능성
        float waitTime = 0f;
        while ((Local_Manager.local_Data == null || !Local_Manager.local_Data.ContainsKey(temp)) && waitTime < 2f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (Local_Manager.local_Data != null && Local_Manager.local_Data.ContainsKey(temp))
        {
            SpeechText.text = Local_Manager.local_Data[temp].Get_Data();
            LayoutRebuilder.ForceRebuildLayoutImmediate(SpeechText.rectTransform);
        }
        else
        {
            Debug.LogWarning($"[Speech_Bubble] 로컬라이즈 키 '{temp}' 를 찾지 못했습니다. 키 출력.");
            SpeechText.text = temp;
        }

        // 말풍선 등장 애니메이션
        float current = 0;
        float percent = 0;
        float start = 0.0f;
        float end = 1.0f;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / 0.3f;
            float LerpScale = Mathf.Lerp(start, end, percent);
            transform.localScale = new Vector3(LerpScale, LerpScale, LerpScale);
            yield return null;
        }

        yield return new WaitForSeconds(5.0f);

        current = 0;
        percent = 0;
        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / 0.3f;
            float LerpScale = Mathf.Lerp(end, start, percent);
            transform.localScale = new Vector3(LerpScale, LerpScale, LerpScale);
            yield return null;
        }

        Destroy(this.gameObject);
    }

}
