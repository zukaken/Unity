using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[FlowMenuItem("Event/OnClick")]
[Pin(0, "Output", Pin.Type.Output)]
public class FlowNode_OnClick : FlowNode
{
    public Button Target;

    private void Start()
    {
        if (Target != null)
        {
            Target.onClick.AddListener(new UnityEngine.Events.UnityAction(OnClick));
        }
    }

    void OnClick()
    {
        ActivateOutputLinks(0);
    }
}
