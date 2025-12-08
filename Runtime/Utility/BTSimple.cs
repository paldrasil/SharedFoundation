using UnityEngine;
using System.Collections.Generic;

namespace Shared.Foundation
{
    public enum NodeState { Success, Failure, Running }

    public abstract class BTNode
    {
        public abstract NodeState Tick();
    }

    public abstract class Composite : BTNode
    {
        protected readonly List<BTNode> children = new();
        public Composite Add(BTNode node) { children.Add(node); return this; }
    }

    public class Selector : Composite
    {
        public override NodeState Tick()
        {
            foreach (var c in children)
            {
                var s = c.Tick();
                if (s != NodeState.Failure) return s;
            }
            return NodeState.Failure;
        }
    }

    public class Sequence : Composite
    {
        int _idx = 0;
        public override NodeState Tick()
        {
            while (_idx < children.Count)
            {
                var s = children[_idx].Tick();
                if (s == NodeState.Running) return NodeState.Running;
                if (s == NodeState.Failure) { _idx = 0; return NodeState.Failure; }
                _idx++;
            }
            _idx = 0;
            return NodeState.Success;
        }
    }

    public class Condition : BTNode
    {
        private readonly System.Func<bool> _pred;
        public Condition(System.Func<bool> pred) { _pred = pred; }
        public override NodeState Tick() => _pred() ? NodeState.Success : NodeState.Failure;
    }

    public class ActionNode : BTNode
    {
        private readonly System.Func<NodeState> _act;
        public ActionNode(System.Func<NodeState> act) { _act = act; }
        public override NodeState Tick() => _act();
    }
}
