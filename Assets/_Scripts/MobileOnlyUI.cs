using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileOnlyUI : MonoBehaviour
{
    void Update() { }
    // Start is called before the first frame update
    void Awake()
    {
        bool isMobile = Application.isMobilePlatform;

    #if !UNITY_EDITOR
        if (!isMobile)
            {
                gameObject.SetActive(false);
            }
    #endif
    }

}
