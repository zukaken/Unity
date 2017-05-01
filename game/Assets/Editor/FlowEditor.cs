using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class FlowEditor : EditorWindow
{
    FlowNode currentFlowNode;
    List<FlowNode> currentFlowNodes;
    List<FlowNode> selection = new List<FlowNode>();
    GameObject currentGameObject;

    Vector2 dragNodePos;
    bool needRepaint;

    bool dragPin;
    int dragPinID;
    DragType dragType;
    Vector2 LastMousePos;

    enum DragType
    {
        Free,
        Hold,
        Release,
    }

    [MenuItem("Tools/FlowEditor")]
    public static void Open()
    {
        GetWindow<FlowEditor>().Show();
    }

    public void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject sceneGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (sceneGameObject == null) return;
        FlowNode[] flowNodes = sceneGameObject.GetComponents<FlowNode>();

        // フローノードが定義されている
        if (flowNodes.Length > 0)
        {
            // インスタンスIDがアクティブになったら
            if (Selection.activeInstanceID == instanceID)
            {
                currentGameObject = sceneGameObject;
                currentFlowNodes = flowNodes.ToList();
            }
        }
    }
    void UpdateFlowNodes()
    {
        if (currentGameObject != null)
        {
            FlowNode[] flowNodes = currentGameObject.GetComponents<FlowNode>();
            currentFlowNodes = flowNodes.ToList();
        }
        else
        {
            currentFlowNodes = null;
        }
    }
    private void Reset()
    {
        currentFlowNodes = null;
        currentFlowNode = null;
        currentGameObject = null;
        dragPin = false;
        dragType = DragType.Free;
    }

    private void OnPlaymodeStateChanged()
    {
        Reset();
    }

    private void OnEnable()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemOnGUI;
        EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            needRepaint = true;
        }

        if (needRepaint)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (dirty)
        {
            dirty = false;
            return;
        }
        needRepaint = true;
        if (currentFlowNodes == null) return;

        switch (Event.current.type)
        {
            case EventType.Repaint:
                break;
            case EventType.MouseUp:
                if (Event.current.isMouse && Event.current.button == 1)
                {
                    var menu = new Kiz_GenericMenu();
                    var list = GetAllFlow();

                    AddFlowNodePosition = Event.current.mousePosition;

                    foreach (System.Type listItem in list)
                    {
                        var attributes = listItem.GetCustomAttributes(false);
                        foreach (var attribute in attributes)
                        {
                            if (attribute is FlowMenuItem)
                            {
                                var flowMenuItem = attribute as FlowMenuItem;
                                menu.Add(flowMenuItem.ItemName.Split('/'), listItem);
                            }
                        }
                    }

                    menu.Show(new Rect(Event.current.mousePosition, new Vector2(1, 1)), OnItemSelect);

                    Debug.Log("KeyCode.Mouse0");
                    Event.current.Use();
                }
                break;
        }

        DrawNodes();
    }
    Vector2 AddFlowNodePosition;

    public void OnItemSelect(object userData)
    {
        System.Type type = (System.Type)userData;
        Debug.Log(userData.ToString());
        FlowNode node = currentGameObject.AddComponent(type) as FlowNode;
        node.mPosition = AddFlowNodePosition;
        FlowNode[] flowNodes = currentGameObject.GetComponents<FlowNode>();
        currentFlowNodes = flowNodes.ToList();
        dirty = true;
    }

    bool dirty;
    List<System.Type> GetAllFlow()
    {
        var baseType = typeof(FlowNode);
        var assembly = baseType.Assembly;
        var types = assembly.GetTypes();
        var flowClasses = types.Where(t => t.IsSubclassOf(baseType));
        return flowClasses.ToList<System.Type>();
    }

    bool HitTest_Pin()
    {
        return false;
    }
    bool  HitTest_Node()
    {
        return false;
    }
    bool hand;

    void DeleteFlowNode(FlowNode target)
    {
        if (currentFlowNodes == null) return;

        foreach(var node in currentFlowNodes)
        {
            int index = 0;
            while(index < node.NodeLinks.Count)
            {
                var link = node.NodeLinks[index];
                if (object.Equals(link.dstNode, target))
                {
                    node.NodeLinks.RemoveAt(index);
                }
                else
                {
                    ++index;
                }
            }
        }
        GameObject.DestroyImmediate(target);
    }

    void HandleGUIEvent()
    {

    }

    interface IState
    {
        void Update();
    }
    class Nutral : IState
    {
        public void Update()
        {

        }
    }
    class Drag : IState
    {
        public void Update()
        {

        }
    }
    class RectTool : IState
    {
        public void Update()
        {

        }
    }

    Vector2 downPos;
    Vector2 dragPos;
    bool rectTool;

    void DrawNodes()
    {
        if (currentFlowNodes == null) return;

        bool mouseDown = false;
        bool hit = false;
        switch (Event.current.type)
        {
            case EventType.MouseDown:
                if (Event.current.button == 2)
                {
                    hand = true;
                }
                if (Event.current.isMouse && Event.current.button == 0)
                {
                    downPos = Event.current.mousePosition;
                    dragPos = downPos;
                    rectTool = true;
                    mouseDown = true;
                    Event.current.Use();
                }
                break;
            case EventType.MouseMove:
                break;
            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.Delete)
                {
                    for (int i = 0; i < selection.Count; ++i)
                    {
                        DeleteFlowNode(selection[i]);
                        selection[i] = null;
                    }
                    selection.Clear();
                    currentFlowNode = null;
                    UpdateFlowNodes();
                }
                break;
            case EventType.Repaint:
                if (hand)
                {
                    int id = GUIUtility.GetControlID(FocusType.Passive);
                    EditorGUIUtility.AddCursorRect(new Rect(new Vector2(0, 0), new Vector2(Screen.width, Screen.height)), MouseCursor.Pan, id);
                    //hand = false;
                }
                if(rectTool)
                {
                    Rect rect = new Rect(0, 0, 100, 200);
                    rect.position = downPos;
                    rect.yMax = dragPos.y;
                    rect.xMax = dragPos.x;
                    GUI.color = Kiz_GUIUtility.RectToolColor;
                    GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                    GUI.color = Color.white;
                }
                break;
            case EventType.MouseDrag:
                if (Event.current.button == 2)
                {
                    hand = true;
                }
                else
                {
                    hand = false;
                }
                if (Event.current.isMouse && Event.current.button == 0)
                {
                    if (currentFlowNode != null)
                    {
                        if (dragPin)
                        {

                        }
                        else
                        {
                            currentFlowNode.mPosition = Event.current.mousePosition - dragNodePos;
                        }
                        needRepaint = true;
                    }
                    LastMousePos = Event.current.mousePosition;
                    Event.current.Use();
                }
                if (rectTool)
                {
                    dragPos = Event.current.mousePosition;
                }
                break;
            case EventType.MouseUp:
                if (Event.current.button == 2)
                {
                    hand = false;
                }
                if (Event.current.isMouse && Event.current.button == 0)
                {
                    dragPin = false;
                    if (dragType == DragType.Hold)
                    {
                        dragType = DragType.Release;
                    }
                    else
                    {
                        currentFlowNode = null;
                    }
                    Event.current.Use();
                }
                rectTool = false;
                break;
        }
        foreach (var node in currentFlowNodes)
        {
            Color nodeColor = selection.Contains(node) ? Color.yellow : node.LastTimeColor;
            // ノードの描画
            using (new ColorScope(nodeColor))
            {
                GUI.Box(node.NodeRect, node.GetType().ToString().Replace("FlowNode_", ""), Styles.Node);
            }

            float lastY = node.NodeRect.yMin;

            // ノブとか描画
            foreach (var inputPin in node.EditorInputPins)
            {
                GUI.Box(inputPin.Rect, "◎", Styles.Pin);
                GUI.Label(inputPin.LabelRect, inputPin.Pin.Name, Styles.PinLabel);

                if (dragType == DragType.Release && !object.Equals(node, currentFlowNode))
                {
                    if (inputPin.Rect.Contains(LastMousePos))
                    {
                        currentFlowNode.Join(dragPinID, node, inputPin.Pin.ID);
                    }
                }
                lastY = inputPin.Rect.yMax + 10;

                node.mSize = new Vector2(Mathf.Max(node.mSize.x, inputPin.LabelRect.width + inputPin.Rect.width + 20), node.mSize.y);
            }

            // ノブとか描画
            foreach (var outputPin in node.EditorOutputPins)
            {
                GUI.Box(outputPin.Rect, "◎", Styles.Pin);
                GUI.Label(outputPin.LabelRect, outputPin.Pin.Name, Styles.PinLabel);

                // あたり判定の処理
                if (mouseDown && !hit)
                {
                    if (outputPin.Rect.Contains(Event.current.mousePosition))
                    {
                        dragNodePos = outputPin.Rect.center;
                        currentFlowNode = node;
                        hit = true;
                        dragPin = true;
                        dragType = DragType.Hold;
                        dragPinID = outputPin.Pin.ID;
                        rectTool = false;
                    }
                }
                lastY = outputPin.Rect.yMax + 10;

                node.mSize = new Vector2(Mathf.Max(node.mSize.x, outputPin.LabelRect.width + outputPin.Rect.width + 20), node.mSize.y);
            }

            node.mSize.y = lastY - node.mPosition.y;

            // あたり判定の処理
            if (mouseDown && !hit)
            {
                // 当たっていた
                if (node.NodeRect.Contains(Event.current.mousePosition))
                {
                    if (!selection.Contains(node))
                    {
                        selection.Clear();
                        selection.Add(node);
                    }
                    dragNodePos = Event.current.mousePosition - node.mPosition;
                    currentFlowNode = node;
                    hit = true;
                    rectTool = false;
                }
            }
        }
        if (mouseDown && !hit)
        {
            selection.Clear();
        }
        if (dragPin)
        {
            Handles.BeginGUI();
            Handles.DrawLine(dragNodePos, Event.current.mousePosition);
            Handles.EndGUI();
        }
        if (dragType == DragType.Release)
        {
            dragType = DragType.Free;
            currentFlowNode = null;
        }

        foreach (var node in currentFlowNodes)
        {
            foreach (var link in node.NodeLinks)
            {
                var srcPin = node.EditorOutputPins.Find(pin => pin.Pin.ID == link.srcPin);
                var dstPin = link.dstNode.EditorInputPins.Find(pin => pin.Pin.ID == link.dstPin);

                Handles.BeginGUI();
                bool mouseOverConnection = HitCheckConnection(Event.current.mousePosition, srcPin.Rect.center, dstPin.Rect.center);
                Color connectionColor = mouseOverConnection ? Kiz_GUIUtility.SelectedColor : link.LastTimeColor;
                DrawConnection(srcPin.Rect.center, dstPin.Rect.center, connectionColor);
                Handles.EndGUI();
            }
        }
    }
    bool HitCheckConnection(Vector2 pos, Vector2 start, Vector2 end)
    {
        var startV3 = new Vector3(start.x, start.y, 0f);
        var endV3 = new Vector3(end.x, end.y + 1f, 0f);

        var centerPoint = startV3 + ((endV3 - startV3) / 2);

        var pointDistance = (endV3.x - startV3.x) / 3f;

        var startTan = new Vector3(startV3.x + pointDistance, startV3.y, 0f);
        var endTan = new Vector3(endV3.x - pointDistance, endV3.y, 0f);

        return HandleUtility.DistancePointBezier(pos, start, end, startTan, endTan) < 5.0f;
    }
    public void DrawConnection(Vector2 start, Vector2 end, Color color)
    {
        var startV3 = new Vector3(start.x, start.y, 0f);
        var endV3 = new Vector3(end.x, end.y + 1f, 0f);

        var centerPoint = startV3 + ((endV3 - startV3) / 2);

        var pointDistance = (endV3.x - startV3.x) / 3f;

        var startTan = new Vector3(startV3.x + pointDistance, startV3.y, 0f);
        var endTan = new Vector3(endV3.x - pointDistance, endV3.y, 0f);

        Handles.DrawBezier(startV3, endV3, startTan, endTan, color, null, 2f);
    }

    class Styles
    {
        public static GUIStyle Pin;
        public static GUIStyle PinLabel;
        public static GUIStyle Node;

        static Styles()
        {
            PinLabel = new GUIStyle(GUI.skin.label);
            Node = new GUIStyle(GUI.skin.box);
            Node.normal.textColor = PinLabel.normal.textColor;
            Pin = new GUIStyle(GUI.skin.box);
            Pin.normal.textColor = PinLabel.normal.textColor;
            Pin.alignment = TextAnchor.MiddleCenter;
        }
    }
}