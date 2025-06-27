using System;
using System.Collections.Generic;
using UnityEngine; // Đảm bảo có namespace này nếu bạn dùng Mathf.Clamp trong ParallelNode

// Enum để định nghĩa trạng thái của Node trong Behavior Tree
public enum NodeState
{
    RUNNING,
    SUCCESS,
    FAILURE
}

public abstract class Node
{
    protected NodeState _nodeState;
    public NodeState nodeState
    {
        get { return _nodeState; }
    }

    public abstract NodeState Evaluate();
}

public class SequenceNode : Node
{
    private List<Node> _nodes = new List<Node>();

    public SequenceNode(List<Node> nodes)
    {
        _nodes = nodes;
    }

    public SequenceNode(params Node[] nodes)
    {
        _nodes = new List<Node>(nodes);
    }

    public override NodeState Evaluate()
    {
        bool anyChildIsRunning = false;

        foreach (Node node in _nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    _nodeState = NodeState.FAILURE;
                    return _nodeState;
                case NodeState.SUCCESS:
                    continue;
                case NodeState.RUNNING:
                    anyChildIsRunning = true;
                    break;
                default:
                    _nodeState = NodeState.SUCCESS;
                    return _nodeState;
            }
        }

        _nodeState = anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return _nodeState;
    }
}

public class SelectorNode : Node
{
    private List<Node> _nodes = new List<Node>();

    public SelectorNode(List<Node> nodes)
    {
        _nodes = nodes;
    }

    public SelectorNode(params Node[] nodes)
    {
        _nodes = new List<Node>(nodes);
    }

    public override NodeState Evaluate()
    {
        foreach (Node node in _nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    continue;
                case NodeState.SUCCESS:
                    _nodeState = NodeState.SUCCESS;
                    return _nodeState;
                case NodeState.RUNNING:
                    _nodeState = NodeState.RUNNING;
                    return _nodeState;
                default:
                    continue;
            }
        }

        _nodeState = NodeState.FAILURE;
        return _nodeState;
    }
}

public class ActionNode : Node
{
    public delegate NodeState ActionNodeDelegate();
    private ActionNodeDelegate _action;

    public ActionNode(ActionNodeDelegate action)
    {
        _action = action;
    }

    public override NodeState Evaluate()
    {
        return _action();
    }
}

public class ConditionNode : Node
{
    public delegate bool ConditionNodeDelegate();
    private ConditionNodeDelegate _condition;

    public ConditionNode(ConditionNodeDelegate condition)
    {
        _condition = condition;
    }

    public override NodeState Evaluate()
    {
        return _condition() ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}

public class Inverter : Node
{
    private Node _node;

    public Inverter(Node node)
    {
        _node = node;
    }

    public override NodeState Evaluate()
    {
        switch (_node.Evaluate())
        {
            case NodeState.FAILURE:
                _nodeState = NodeState.SUCCESS;
                return _nodeState;
            case NodeState.SUCCESS:
                _nodeState = NodeState.FAILURE;
                return _nodeState;
            case NodeState.RUNNING:
                _nodeState = NodeState.RUNNING;
                return _nodeState;
            default:
                _nodeState = NodeState.SUCCESS;
                return _nodeState;
        }
    }
}