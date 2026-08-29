using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render_Manager : MonoBehaviour
{
    public static Render_Manager instance;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    public Camera cam;
    public Render_Hero HERO;
    public Render_Gacha GACHA;
    public Render_Dungeon DUNGEON;
    public Render_Debuff DEBUFF;

    public Vector2 ReturnScreenPoint(Transform pos)
    {
        Vector3 viewportPoint = cam.WorldToViewportPoint(pos.position);
        // Viewport의 y 값은 아래에서 위로 증가하므로, 이를 반전시켜서 화면 좌표계를 맞춰줌
        Vector2 screenpoint = new Vector2(viewportPoint.x * Screen.width, viewportPoint.y * Screen.height);
        return screenpoint;
    }
}
