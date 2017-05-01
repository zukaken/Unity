using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class FlowNode : MonoBehaviour
{
    public List<UnityEvent> Input;
    public List<UnityEvent> Output;

    public List<NodeLink> NodeLinks = new List<NodeLink>();

    public Vector2 mPosition;
    public Vector2 mSize;
    public Rect NodeRect
    {
        get
        {
            return new Rect(mPosition, mSize);
        }
    }

#if UNITY_EDITOR

    System.DateTime _LastActivateTime;
    List<EditorPin> _EditorInputPins;
    List<EditorPin> _EditorOutputPins;

    public System.DateTime LastActivateTime
    {
        get
        {
            return _LastActivateTime;
        }
        set
        {
            _LastActivateTime = value;
        }
    }

    public Color LastTimeColor
    {
        get
        {
            return GUIExtention.LerpColor(System.DateTime.Now, LastActivateTime, Color.red, Color.white);
        }
    }

    public List<EditorPin> EditorInputPins
    {
        get
        {
            if (_EditorInputPins == null)
            {
                _EditorInputPins = new List<EditorPin>();
                Pin[] pins = (Pin[])this.GetType().GetCustomAttributes(typeof(Pin), false);
                var inputs = pins.Where<Pin>(pin => pin.PinType == Pin.Type.Input);
                Vector2 pos = mPosition + new Vector2(0, 40);
                foreach (var pin in inputs)
                {
                    var newEditorPin = new EditorPin();
                    newEditorPin.Pin = pin;
                    newEditorPin.Position = pos;
                    _EditorInputPins.Add(newEditorPin);
                    pos += new Vector2(0, 30);
                }
                return _EditorInputPins;
            }
            else
            {
                Vector2 pos = mPosition + new Vector2(0, 40);
                foreach (var pin in _EditorInputPins)
                {
                    pin.Position = pos;
                    pos += new Vector2(0, 30);
                }
            }
            return _EditorInputPins;
        }
    }

    public List<EditorPin> EditorOutputPins
    {
        get
        {
            if (_EditorOutputPins == null)
            {
                _EditorOutputPins = new List<EditorPin>();
                Pin[] pins = (Pin[])this.GetType().GetCustomAttributes(typeof(Pin), false);
                var inputs = pins.Where<Pin>(pin => pin.PinType == Pin.Type.Output);
                Vector2 pos = mPosition + new Vector2(0, 40 + EditorInputPins.Count * 30);
                foreach (var pin in inputs)
                {
                    var newEditorPin = new EditorPin();
                    newEditorPin.Pin = pin;
                    newEditorPin.Position = pos;
                    _EditorOutputPins.Add(newEditorPin);
                    pos += new Vector2(0, 30);
                }
                return _EditorOutputPins;
            }
            else
            {
                Vector2 pos = mPosition + new Vector2(mSize.x - 20, 40 + EditorInputPins.Count * 30);
                foreach (var pin in _EditorOutputPins)
                {
                    pin.Position = pos;
                    pos += new Vector2(0, 30);
                }
            }
            return _EditorOutputPins;
        }
    }
#endif


    public List<Pin> InputPins
    {
        get
        {
            Pin[] pins = (Pin[])this.GetType().GetCustomAttributes(typeof(Pin), false);
            if (pins == null)
            {
                return new List<Pin>();
            }
            return pins.Where<Pin>(pin => pin.PinType == Pin.Type.Input).ToList<Pin>();
        }
    }
    public List<Pin> OutputPins
    {
        get
        {
            Pin[] pins = (Pin[])this.GetType().GetCustomAttributes(typeof(Pin), false);
            if (pins == null)
            {
                return new List<Pin>();
            }
            return pins.Where<Pin>(pin => pin.PinType == Pin.Type.Output).ToList<Pin>();
        }
    }
    public void Join(int sourcePin, FlowNode dstNode, int dstPin)
    {
        bool result = NodeLinks.FindIndex(nodelink => nodelink.Equals(sourcePin, dstNode, dstPin)) != -1;
        if(!result)
        {
            var newLink = new NodeLink();
            newLink.Join(sourcePin, dstNode, dstPin);
            NodeLinks.Add(newLink);
        }
    }

    public void ActivateOutputLinks(int pinID)
    {
        var reuslt = NodeLinks.Where(link => link.srcPin == pinID);
        foreach (var link in reuslt)
        {
            link.LastActivateTime = System.DateTime.Now;
            link.dstNode.ActivatePin(link.dstPin);
            link.dstNode.LastActivateTime = System.DateTime.Now;
        }
    }

    protected virtual void ActivatePin(int pinID)
    {

    }
}


[System.Serializable]
public class NodeLink
{
    public FlowNode dstNode;
    public int dstPin;
    public int srcPin;
#if UNITY_EDITOR

    public Color LastTimeColor
    {
        get
        {
            return GUIExtention.LerpColor(System.DateTime.Now, LastActivateTime, Color.red, Color.white);
        }
    }

    System.DateTime _LastActivateTime;
    public System.DateTime LastActivateTime
    {
        get
        {
            return _LastActivateTime;
        }
        set
        {
            _LastActivateTime = value;
        }
    }
#endif

    public void Join(int sourcePin, FlowNode dstNode, int dstPin)
    {
        this.srcPin = sourcePin;
        this.dstNode = dstNode;
        this.dstPin = dstPin;
    }
    public bool Equals(int sourcePin, FlowNode dstNode, int dstPin)
    {
        return srcPin == sourcePin &&
               this.dstPin == dstPin &&
               this.dstNode == dstNode;
    }
}


public class EditorPin
{
    public Vector2 Position;
    public Rect Rect
    {
        get
        {
            return new Rect(Position, new Vector2(20, 20));
        }
    }
    public Rect LabelRect
    {
        get
        {
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(Pin.Name));
            if (Pin.PinType == Pin.Type.Output)
            {
                return new Rect(Position - new Vector2(size.x + 10, 0), size);
            }
            else
            {
                return new Rect(Position + new Vector2(20 + 10, 0), size);
            }
        }
    }
    public Pin Pin;
}

#region Attributes

[System.AttributeUsage( System.AttributeTargets.Class,AllowMultiple = true)]
public class Pin : System.Attribute
{
    public enum Type
    {
        Input,
        Output
    }

    public string Name;
    public int ID;
    public Type PinType;
    public Pin(int id, string name, Type type)
    {
        ID = id;
        Name = name;
        PinType = type;
    }
}

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false)]
public class FlowMenuItem : System.Attribute
{
    public string ItemName;
    public FlowMenuItem(string itemName)
    {
        ItemName = itemName;
    }
}

#endregion