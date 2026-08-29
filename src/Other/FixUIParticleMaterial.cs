using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class FixUIParticleMaterial : MonoBehaviour
{
    void Start()
    {
        // 현재 GameObject 및 모든 자식 오브젝트의 Renderer 확인
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (!material.HasProperty("_Stencil"))
                {
                    // Stencil 속성이 없는 Material 교체
                    renderer.material = Resources.GetBuiltinResource<Material>("UI/Default.mat");
                }
            }
        }
    }
}
