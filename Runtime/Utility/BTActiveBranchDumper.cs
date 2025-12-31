using System.Text;

namespace Shared.Foundation
{
    public static class BTActiveBranchDumper
    {
        public static string Dump(this BTNode root)
        {
            var sb = new StringBuilder();
            DumpNode(root, sb, "", true);
            return sb.ToString();
        }

        static void DumpNode(
            BTNode node,
            StringBuilder sb,
            string indent,
            bool last)
        {
            sb.Append(indent);
            sb.Append(last ? "└─ " : "├─ ");
            sb.Append(NodeLabel(node));
            sb.AppendLine();

            if (node is Composite comp)
            {
                var children = comp.Children;
                for (int i = 0; i < children.Count; i++)
                {
                    if(children[i].LastTickFrame < node.LastTickFrame) continue;
                    DumpNode(
                        children[i],
                        sb,
                        indent + (last ? "   " : "│  "),
                        i == children.Count - 1
                    );
                }
            }
        }

        static string NodeLabel(BTNode node)
        {
            string icon = node.LastState switch
            {
                NodeState.Success => "✔",
                NodeState.Failure => "✖",
                NodeState.Running => "▶",
                _ => "?"
            };

            return $"{icon} {node.NodeName}";
        }
    }
}
