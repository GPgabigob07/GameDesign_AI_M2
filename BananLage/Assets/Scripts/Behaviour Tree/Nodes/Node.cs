using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Behaviour_Tree
{
    public abstract class Node<M>
        where M : BehaviourTreeManager<M>
    {
        public virtual string Name { get; protected set; } = null;

        protected int CurrentChild;
        protected M Manager;

        public abstract NodeResult Process();

        protected abstract void CreateChildren();
        protected List<Node<M>> Children { get; private set; }

        public void Setup(M manager)
        {
            Name ??= GetType().Name;
            Manager = manager;
            Children = new();
            if (manager.debug) Debug.Log($"Setting up {Name}");
            CreateChildren();
        }

        private string DescribeChildren(int depth = 0)
        {
            return Children.Aggregate("",
                (acc, next) => $"\n|{new string('\t', depth)}{next}[{next.DescribeChildren(depth + 1)}]| ");
        }

        protected void AddChild(Node<M> node, [CanBeNull] string name = null)
        {
            node.Name ??= name ?? node.Name;
            Children.Add(node);
            node.Setup(Manager);
        }

        public void Reset()
        {
            CurrentChild = 0;
            Children.ForEach(c => c.Reset());
        }
    }
}