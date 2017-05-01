using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[FlowMenuItem("Debug/Print")]
[Pin(0, "Print", Pin.Type.Input)]
public class FlowNode_DebugPrint : FlowNode
{
    public string str;
    protected override void ActivatePin(int pinID)
    {
        if (pinID == 0)
        {
            Debug.Log(str);
        }
    }
}