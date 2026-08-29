using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Manager : MonoBehaviour
{
    public static Camera_Manager instance = null;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    float m_Distance = 4.0f;
    [Range(0.0f, 10.0f)]
    [SerializeField] private float Distance_Value;

    [Space(20f)]
    [Header("## Camera Shake")]
    [Range(0.0f, 10.0f)]
    [SerializeField] private float Duration;
    [Range(0.0f, 10.0f)] [SerializeField] private float Power;

    Vector3 OriginalPos;
    bool isCameraShake = false;
    Camera cam;

    // 카메라의 초기 오프셋 값
    private Vector3 initialOffset = new Vector3(0f, 5f, -3.5f);

    private void Start()
    {
        cam = GetComponent<Camera>();
        transform.position = initialOffset;
        OriginalPos = transform.localPosition;
    }

    private void Update()
    {
        // 메인 캐릭터 추적
        Player mainCharacter = FindMainCharacter();
        if (mainCharacter == null)
        {
            Debug.LogWarning("No active player found. Stopping Update().");
            return; //  메인 캐릭터가 없으면 실행 중단
        }

        if (mainCharacter != null)
        {
            // 카메라를 메인 캐릭터의 위치로 이동
            Vector3 targetPosition = mainCharacter.transform.position + initialOffset;

            // 카메라와 메인 캐릭터 사이의 거리 유지 (y값은 mainCharacter.y + initialOffset.y로 조정)
            targetPosition.x += initialOffset.x; // x값 고정 (오프셋에 맞춰 이동)
            targetPosition.y += initialOffset.y; // y값은 초기 오프셋을 더해줌
            targetPosition.z += initialOffset.z; // z값 고정 (오프셋에 맞춰 이동)

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f); // 부드럽게 이동

            OriginalPos = transform.localPosition;
            // 카메라 크기 조정
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, Distance(), Time.deltaTime * 2.0f);
        }
    }

    float Distance()
    {
        Player mainCharacter = FindMainCharacter();
        if (mainCharacter == null)
        {

            return m_Distance;
        }

        var players = Spawner.m_Players.ToArray();
        float maxDistance = m_Distance;

        //  `m_Players` 리스트에서 `null`인 객체를 제거한 후 거리 계산
        for (int i = Spawner.m_Players.Count - 1; i >= 0; i--)
        {
            Player player = Spawner.m_Players[i];

            if (player == null || player.isDead)  // 삭제된 플레이어 확인
            {
                Debug.LogWarning("Skipping null or dead player in Distance() calculation.");
                Spawner.m_Players.RemoveAt(i); // 리스트에서 제거
                continue;
            }

            float targetDistance = Vector3.Distance(mainCharacter.transform.position, player.transform.position) + Distance_Value;

            if (targetDistance > maxDistance)
            {
                maxDistance = targetDistance;
            }
        }
        return maxDistance;
    }

    public void CameraShake()
    {
        if (PlayerPrefs.GetInt("CAM") == 1) return;
        if (isCameraShake) return;

        isCameraShake = true;
        StartCoroutine(CameraShake_Coroutine());
    }

    IEnumerator CameraShake_Coroutine()
    {
        float timer = 0.0f;

        while(timer <= Duration)
        {
            transform.localPosition = Random.insideUnitSphere * Power + OriginalPos;

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = OriginalPos;
        isCameraShake = false;
    }

    private Player FindMainCharacter()
    {
        if (Spawner.m_Players == null || Spawner.m_Players.Count == 0) return null; // 플레이어 리스트가 비어있으면 null 반환

        Player mainCharacter = Spawner.m_Players.Find(p => p != null && p.MainCharacter);
        if (mainCharacter != null) return mainCharacter;

        // 메인 캐릭터가 없으면 리스트에서 첫 번째 살아있는 플레이어 반환
        foreach (var player in Spawner.m_Players)
        {
            if (player != null) return player;
        }

        return null;
    }
}
