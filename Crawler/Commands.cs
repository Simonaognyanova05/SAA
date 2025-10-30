using System;
using System.Collections.Generic;
using System.Linq;

namespace HtmlCrawler
{
    public static class Commands
    {
        public static void SetNodes(List<HtmlNode> nodes, string text)
        {
            foreach (var n in nodes)
            {
                if (n.Type == NodeType.Text) n.Text = text;
                else
                {
                    n.Children.Clear();
                    var t = new HtmlNode(NodeType.Text) { Text = text };
                    n.AddChild(t);
                }
            }
        }

        public static void CopyNodes(List<HtmlNode> src, List<HtmlNode> dst)
        {
            if (src.Count == 0 || dst.Count == 0) return;
            var nodeToCopy = src[0];
            foreach (var target in dst)
            {
                var clone = nodeToCopy.ShallowClone();
                target.AddChild(clone);
            }
        }
    }
}
