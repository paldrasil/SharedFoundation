using System;
using System.Collections.Generic;

namespace Shared.Foundation
{
    public enum NodeState { Success, Failure, Running }

    public abstract class BTNode
    {
        public NodeState LastState { get; protected set; }
        public int LastTickFrame { get; protected set; }
        public string NodeName { get; protected set; }

        public NodeState Tick()
        {
            var s = OnTick();
            LastState = s;
            LastTickFrame = UnityEngine.Time.frameCount;
            return s;
        }

        protected abstract NodeState OnTick();

        public virtual void Reset() { }
    }

    public abstract class Composite : BTNode
    {
        protected readonly List<BTNode> children = new();
        public IReadOnlyList<BTNode> Children => children;

        public Composite Add(BTNode node)
        {
            children.Add(node);
            return this;
        }

        public override void Reset()
        {
            foreach (var c in children)
                c.Reset();
        }
    }

    /// Reactive priority selector + auto-reset old running branch when switching.
    public class Selector : Composite
    {
        int _activeChild = -1;
        
        public Selector() { NodeName = "Custom Selector"; }

        public Selector(string nodeName) { NodeName = nodeName; }

        protected override NodeState OnTick()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var s = children[i].Tick();
                if (s == NodeState.Failure) continue;

                if (_activeChild != -1 && _activeChild != i)
                    children[_activeChild].Reset();

                _activeChild = (s == NodeState.Running) ? i : -1;
                return s;
            }

            if (_activeChild != -1)
            {
                children[_activeChild].Reset();
                _activeChild = -1;
            }
            return NodeState.Failure;
        }

        public override void Reset()
        {
            if (_activeChild != -1)
            {
                children[_activeChild].Reset();
                _activeChild = -1;
            }
        }
    }

    /// Memory sequence (keeps index while Running).
    public class Sequence : Composite
    {
        int _idx = 0;

        public Sequence() { NodeName = "Custom Sequence"; }
        public Sequence(string nodeName) { NodeName = nodeName; }

        protected override NodeState OnTick()
        {
            while (_idx < children.Count)
            {
                var s = children[_idx].Tick();
                if (s == NodeState.Running) return NodeState.Running;
                if (s == NodeState.Failure)
                {
                    _idx = 0;
                    return NodeState.Failure;
                }
                _idx++;
            }
            _idx = 0;
            return NodeState.Success;
        }

        public override void Reset()
        {
            foreach (var c in children) c.Reset();
            _idx = 0;
        }
    }

    public class Condition : BTNode
    {
        private readonly Func<bool> _pred;
        public Condition(Func<bool> pred) { _pred = pred; NodeName = "Custom Condition"; }
        public Condition(string nodeName, Func<bool> pred) { _pred = pred; NodeName = nodeName; }
        protected override NodeState OnTick() => _pred() ? NodeState.Success : NodeState.Failure;
    }

    public class ActionNode : BTNode
    {
        private readonly Func<NodeState> _act;
        public ActionNode(Func<NodeState> act) { _act = act; NodeName = "Custom Action"; }
        public ActionNode(string nodeName, Func<NodeState> act) { _act = act; NodeName = nodeName; }
        protected override NodeState OnTick() => _act();
    }
}
