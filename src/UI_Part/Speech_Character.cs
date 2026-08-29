using UnityEngine;
using System.Collections;

public class Speech_Character : MonoBehaviour
{
    [SerializeField] private Speech_Bubble bubble;
    public Vector3 pos = new Vector3(0, 0.3f, 0);
    [SerializeField] private string[] speech_Types;
    public Camera cam;
    public float MinSecond, MaxSecond;
    public bool OutUI = false;

    public void Init(bool isReadyCharacter)
    {
        StopAllCoroutines();
        StartCoroutine(Speech_Coroutine(isReadyCharacter));
    }

    public void DisableCoroutine()
    {
        StopAllCoroutines();
    }

    IEnumerator Speech_Coroutine(bool isReadyCharacter)
    {
        yield return new WaitForSeconds(Random.Range(0.0f, 1.0f));
        Transform parentTransform;

        if (OutUI)
        {
            parentTransform = Base_Canvas.instance.HOLDER_LAYER(3).transform;
        }
        else if (Utils.UI_Holder != null && Utils.UI_Holder.Count > 0)
        {
            parentTransform = Utils.UI_Holder.Peek().transform;
        }
        else
        {
            Debug.LogWarning("UI_Holder가 비어 있습니다. 기본 캔버스로 대체합니다.");
            parentTransform = Base_Canvas.instance.transform; // 안전한 기본값
        }

        if (!OutUI && (Utils.UI_Holder == null || Utils.UI_Holder.Count == 0))
            yield break;

        var go = Instantiate(bubble, OutUI ? Base_Canvas.instance.HOLDER_LAYER(3).transform : Utils.UI_Holder.Peek().transform);
        go.Init(transform, pos, speech_Types, isReadyCharacter ? Camera.main : cam);
        yield return new WaitForSeconds(Random.Range(MinSecond, MaxSecond));

        if(!isReadyCharacter)
        StartCoroutine(Speech_Coroutine(isReadyCharacter));
    }

    public void SetSpeechTypes(string[] types)
    {
        speech_Types = null;
        speech_Types = types;
    }
}
