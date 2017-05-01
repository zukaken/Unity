using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[FlowMenuItem("Scene/Scene Change")]
[Pin(PIN_ID_CHANGE_SCENE, "Change Scene", Pin.Type.Input)]
[Pin(PIN_ID_CHANGED, "Changed", Pin.Type.Output)]
public class FlowNode_SceneChange : FlowNode
{
    const int PIN_ID_CHANGE_SCENE = 0;
    const int PIN_ID_CHANGED = 10;

    public string SceneName;

    protected override void ActivatePin(int pinID)
    {
        switch (pinID)
        {
            case  PIN_ID_CHANGE_SCENE:
                ActivateOutputLinks(PIN_ID_CHANGED);
                break;
        }
    }
}