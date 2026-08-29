using UnityEngine;

public class Render_Debuff : MonoBehaviour
{
   [SerializeField] private Transform Position;
   [SerializeField] private Camera cam;

    private GameObject currentCharacter;


    public void ShowDebuffCharacter(Character_Scriptable data)
    {
        // 기존 오브젝트 삭제
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
            currentCharacter = null;
        }

        // 해당 캐릭터 프리팹 로드 및 생성
        var prefab = Resources.Load<GameObject>("Character/" + data.m_CharacterName);
        if (prefab == null)
        {
            Debug.LogError($"[Render_Debuff] 해당 캐릭터 프리팹을 찾을 수 없습니다: {data.m_CharacterName}");
            return;
        }

        currentCharacter = Instantiate(prefab, Position.position, Quaternion.identity, Position);
        SetLayerRecursively(currentCharacter, LayerMask.NameToLayer("Render_Layer"));
        currentCharacter.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f); // 크기 조정

        // Player 컴포넌트 비활성화 (조작 방지)
        var player = currentCharacter.GetComponent<Player>();
        player.GetComponent<Player>().enabled = false;

        var animator = currentCharacter.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("IsSad");
        }


    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

}
