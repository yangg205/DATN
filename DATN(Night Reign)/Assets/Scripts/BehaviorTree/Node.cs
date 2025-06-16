using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum để định nghĩa trạng thái của Node trong Behavior Tree
public enum NodeState
{
    RUNNING,
    SUCCESS,
    FAILURE
}

// Lớp cơ sở cho tất cả các Node
public abstract class Node
{
    protected NodeState _nodeState;
    public NodeState nodeState => _nodeState;

    public abstract NodeState Evaluate();
}

// Node cho các hành động (lá của cây)
public class ActionNode : Node
{
    private Func<NodeState> _action;

    public ActionNode(Func<NodeState> action)
    {
        _action = action;
    }

    public override NodeState Evaluate()
    {
        _nodeState = _action.Invoke();
        return _nodeState;
    }
}

// Node cho các điều kiện
public class ConditionNode : Node
{
    private Func<bool> _condition;

    public ConditionNode(Func<bool> condition)
    {
        _condition = condition;
    }

    public override NodeState Evaluate()
    {
        _nodeState = _condition.Invoke() ? NodeState.SUCCESS : NodeState.FAILURE;
        return _nodeState;
    }
}

// Node Sequence: Chạy các node con theo thứ tự, dừng lại nếu một node con thất bại
public class SequenceNode : Node
{
    protected List<Node> _nodes = new List<Node>();

    public SequenceNode(List<Node> nodes)
    {
        _nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        bool anyChildRunning = false;
        foreach (var node in _nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    _nodeState = NodeState.FAILURE;
                    return _nodeState;
                case NodeState.SUCCESS:
                    continue;
                case NodeState.RUNNING:
                    anyChildRunning = true;
                    break;
                default:
                    _nodeState = NodeState.SUCCESS;
                    return _nodeState;
            }
        }

        _nodeState = anyChildRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        return _nodeState;
    }
}

// Node Selector: Chạy các node con theo thứ tự, dừng lại và trả về SUCCESS nếu một node con thành công
public class SelectorNode : Node
{
    protected List<Node> _nodes = new List<Node>();

    public SelectorNode(List<Node> nodes)
    {
        _nodes = nodes;
    }

    public override NodeState Evaluate()
    {
        foreach (var node in _nodes)
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

// Node Parallel: Chạy tất cả các node con đồng thời.
// Có thể tùy chỉnh chính sách thành công/thất bại (ví dụ: cần tất cả thành công, hoặc 1 thành công là đủ)
public class ParallelNode : Node
{
    protected List<Node> _nodes = new List<Node>();
    private int _successThreshold; // Số lượng node con cần SUCCESS để ParallelNode trả về SUCCESS

    public ParallelNode(List<Node> nodes, int successThreshold = 1)
    {
        _nodes = nodes;
        _successThreshold = Mathf.Clamp(successThreshold, 1, nodes.Count);
    }

    public override NodeState Evaluate()
    {
        int successCount = 0;
        int runningCount = 0;

        foreach (var node in _nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.SUCCESS:
                    successCount++;
                    break;
                case NodeState.RUNNING:
                    runningCount++;
                    break;
                case NodeState.FAILURE:
                    // Với Parallel, một failure có thể không làm cả node thất bại ngay lập tức, tùy thuộc vào logic game
                    break;
            }
        }

        if (successCount >= _successThreshold)
        {
            _nodeState = NodeState.SUCCESS;
        }
        else if (runningCount > 0)
        {
            _nodeState = NodeState.RUNNING;
        }
        else
        {
            _nodeState = NodeState.FAILURE;
        }

        return _nodeState;
    }
}
