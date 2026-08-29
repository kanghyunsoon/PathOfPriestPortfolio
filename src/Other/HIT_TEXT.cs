using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HIT_TEXT : MonoBehaviour
{
    Vector3 target;
    Camera cam;

    public TextMeshProUGUI m_Text;

    [SerializeField] private GameObject m_Critical;

    float UpRange = 0.0f;


    private void Start()
    {
        cam = Camera.main;
    }

    public void Init(Vector3 pos, double dmg, Color color, bool Monster = false, bool Critical = false)
    {
        UI_SavingMode.m_OnSaving += OnSave;

        pos.x += Random.Range(-0.1f, 0.1f);
        pos.z += Random.Range(-0.1f, 0.1f);

        transform.localScale = Vector3.one;

        target = pos;
        m_Text.text = StringMethod.ToCurrencyString(dmg);

        m_Text.color = color;

        transform.SetParent(Base_Canvas.instance.HOLDER_LAYER(1));


        m_Critical.SetActive(Critical);

        if (Critical) m_Text.colorGradient = new VertexGradient(color, Color.red, Color.white, Color.white);
        else m_Text.colorGradient = new VertexGradient(Color.white, Color.white, Color.white, Color.white);

        Base_Manager.instance.Return_Pool(2.0f, this.gameObject, "HIT_TEXT");
    }

    private void OnDisable()
    {
        UI_SavingMode.m_OnSaving -= OnSave;
    }

    void OnSave()
    {
        ReturnText();
    }

    private void Update()
    {
        if (Base_Canvas.isSave == false)
        {
            Vector3 targetPos = new Vector3(target.x, target.y + UpRange, target.z);
            if(cam !=null)
            transform.position = cam.WorldToScreenPoint(targetPos);

            if (UpRange <= 0.3f)
            {
                UpRange += Time.deltaTime;
            }
        }
    }

    private void ReturnText()
    {
        Base_Manager.Pool.m_pool_Dictionary["HIT_TEXT"].Return(this.gameObject);

    }
}
