using System;
using System.Collections.Generic;
using System.Text;

namespace HtmlCrawler
{
    public enum NodeType { Element, Text }

    public class HtmlAttribute
    {
        public string Name;
        public string Value;
        public HtmlAttribute(string n, string v) { Name = n; Value = v; }
    }

    public class HtmlNode
    {
        public NodeType Type;
        public string? TagName;
        public string? Text;
        public List<HtmlAttribute> Attributes = new List<HtmlAttribute>();
        public HtmlNode? Parent;
        public List<HtmlNode> Children = new List<HtmlNode>();

        public HtmlNode(NodeType t) { Type = t; }

        public string GetInnerText()
        {
            if (Type == NodeType.Text) return Text ?? "";
            var sb = new StringBuilder();
            AppendInnerText(this, sb);
            return sb.ToString();
        }

        private void AppendInnerText(HtmlNode n, StringBuilder sb)
        {
            if (n.Type == NodeType.Text) { sb.Append(n.Text); return; }
            foreach (var c in n.Children) AppendInnerText(c, sb);
        }

        public HtmlNode ShallowClone()
        {
            var clone = new HtmlNode(this.Type)
            {
                TagName = this.TagName,
                Text = this.Text
            };
            foreach (var a in this.Attributes) clone.Attributes.Add(new HtmlAttribute(a.Name, a.Value));
            foreach (var c in this.Children) clone.Children.Add(c);
            return clone;
        }

        public HtmlNode DeepCopy(HtmlNode? parent = null)
        {
            var c = new HtmlNode(this.Type) { Parent = parent, TagName = this.TagName, Text = this.Text };
            foreach (var a in this.Attributes) c.Attributes.Add(new HtmlAttribute(a.Name, a.Value));
            foreach (var child in this.Children) c.Children.Add(child.DeepCopy(c));
            return c;
        }

        public void AddChild(HtmlNode child) { child.Parent = this; Children.Add(child); }
    }

    public class HtmlDocument
    {
        public HtmlNode Root;
        public HtmlDocument(HtmlNode root) { Root = root; }
    }
}
