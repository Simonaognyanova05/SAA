using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HtmlCrawler
{
    public static class ArchiveManager
    {
        public static void SaveArchive(string filename, HtmlDocument doc)
        {
            var json = JsonSerializer.Serialize(SerializeNode(doc.Root));
            File.WriteAllText(filename + ".json", json);
        }

        public static HtmlDocument LoadArchive(string filename)
        {
            var json = File.ReadAllText(filename + ".json");
            var rootObj = JsonSerializer.Deserialize<SerializedNode>(json)!;
            var root = DeserializeNode(rootObj);
            return new HtmlDocument(root);
        }

        private static SerializedNode SerializeNode(HtmlNode n)
        {
            var s = new SerializedNode { Type = n.Type.ToString(), TagName = n.TagName, Text = n.Text };
            foreach (var a in n.Attributes) s.Attributes.Add(new KeyValuePair<string,string>(a.Name,a.Value));
            foreach (var c in n.Children) s.Children.Add(SerializeNode(c));
            return s;
        }

        private static HtmlNode DeserializeNode(SerializedNode s, HtmlNode? parent = null)
        {
            var n = new HtmlNode(s.Type == "Element" ? NodeType.Element : NodeType.Text) { TagName = s.TagName, Text = s.Text, Parent = parent };
            foreach (var kv in s.Attributes) n.Attributes.Add(new HtmlAttribute(kv.Key, kv.Value));
            foreach (var c in s.Children) n.Children.Add(DeserializeNode(c, n));
            return n;
        }

        private class SerializedNode
        {
            public string Type { get; set; } = "Element";
            public string? TagName { get; set; }
            public string? Text { get; set; }
            public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,string>> Attributes { get; set; } = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<string,string>>();
            public System.Collections.Generic.List<SerializedNode> Children { get; set; } = new System.Collections.Generic.List<SerializedNode>();
        }
    }
}
