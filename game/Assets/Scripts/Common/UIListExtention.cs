using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIListExtention : MonoBehaviour
{
    public UIList uiList;
    public bool isOn;

    // Use this for initialization
    void Start()
    {
        var selectable = gameObject.GetComponent<Selectable>();
        if (selectable != null)
        {
            if(isOn)
            {
                selectable.Select();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
