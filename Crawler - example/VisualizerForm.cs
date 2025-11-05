using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HtmlCrawler
{
    public class VisualizerForm : Form
    {
        private HtmlDocument doc;
        private TreeView tree;
        private Panel renderPanel;

        public VisualizerForm(HtmlDocument document)
        {
            doc = document;
            Text = "HtmlCrawler Visualizer";
            Width = 1000;
            Height = 700;
            InitComponents();
            BuildTree();
        }

        private void InitComponents()
        {
            tree = new TreeView { Dock = DockStyle.Left, Width = 300 };
            renderPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            tree.AfterSelect += (s, e) => RenderNode(FindNodeByPath(e.Node.Tag as string));
            Controls.Add(renderPanel);
            Controls.Add(tree);
        }

        private void BuildTree()
        {
            tree.Nodes.Clear();
            var root = new TreeNode("root") { Tag = "/" };
            tree.Nodes.Add(root);
            BuildTreeNodes(doc.Root, root, "/");
            tree.ExpandAll();
        }

        private void BuildTreeNodes(HtmlNode n, TreeNode tn, string path)
        {
            int idx = 0;
            foreach (var c in n.Children)
            {
                idx++;
                var label = c.Type == NodeType.Element ? $"<{c.TagName}>" : ($"{(c.Text?.Trim() ?? "")}");
                var childPath = path + (c.Type == NodeType.Element ? (c.TagName ?? "elem") : "text") + $"[{idx}]";
                var childTn = new TreeNode(label) { Tag = childPath };
                tn.Nodes.Add(childTn);
                BuildTreeNodes(c, childTn, childPath + "/");
            }
        }

        private HtmlNode? FindNodeByPath(string? tag)
        {
            if (tag == null) return null;
            // tag format produced above, we'll do a best-effort find by traversing nodes in order
            var parts = tag.Trim('/').Split('/');
            var current = doc.Root;
            foreach (var p in parts)
            {
                if (string.IsNullOrEmpty(p)) continue;
                // p like tag[n] or text[n]
                var m = System.Text.RegularExpressions.Regex.Match(p, @"^(.*)\[(\d+)\]$");
                if (!m.Success) continue;
                var name = m.Groups[1].Value;
                var idx = int.Parse(m.Groups[2].Value);
                if (name == "text")
                {
                    var texts = current.Children.Where(x => x.Type == NodeType.Text).ToList();
                    if (idx - 1 < texts.Count) current = texts[idx - 1];
                    else return null;
                }
                else
                {
                    var elems = current.Children.Where(x => x.Type == NodeType.Element && string.Equals(x.TagName, name, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (idx - 1 < elems.Count) current = elems[idx - 1];
                    else return null;
                }
            }
            return current;
        }

        private void RenderNode(HtmlNode? node)
        {
            renderPanel.Controls.Clear();
            if (node == null) return;
            int y = 10;
            if (node.Type == NodeType.Text)
            {
                var lbl = new Label { Text = node.Text, AutoSize = true, Location = new Point(10, y) };
                renderPanel.Controls.Add(lbl); return;
            }
            // for element, render children in a vertical layout; special handling for table and img and a
            foreach (var c in node.Children)
            {
                if (c.Type == NodeType.Text)
                {
                    var lbl = new Label { Text = c.Text, AutoSize = true, Location = new Point(10, y) };
                    renderPanel.Controls.Add(lbl); y += lbl.Height + 6;
                }
                else if (string.Equals(c.TagName, "img", StringComparison.OrdinalIgnoreCase))
                {
                    var srcAttr = c.Attributes.Find(a => string.Equals(a.Name, "src", StringComparison.OrdinalIgnoreCase));
                    if (srcAttr != null)
                    {
                        var imgPath = srcAttr.Value;
                        if (File.Exists(imgPath))
                        {
                            try
                            {
                                var bmp = Image.FromFile(imgPath);
                                var pic = new PictureBox { Image = bmp, Location = new Point(10, y), SizeMode = PictureBoxSizeMode.AutoSize };
                                renderPanel.Controls.Add(pic); y += pic.Height + 6;
                            }
                            catch { /* ignore */ }
                        }
                    }
                }
                else if (string.Equals(c.TagName, "a", StringComparison.OrdinalIgnoreCase))
                {
                    var text = c.GetInnerText();
                    var l = new LinkLabel { Text = text, AutoSize = true, Location = new Point(10, y) };
                    renderPanel.Controls.Add(l); y += l.Height + 6;
                }
                else if (string.Equals(c.TagName, "table", StringComparison.OrdinalIgnoreCase))
                {
                    // simple table render: find all tr -> td (assume rectangular)
                    var table = new TableLayoutPanel { Location = new Point(10, y), AutoSize = true, CellBorderStyle = TableLayoutPanelCellBorderStyle.Single };
                    var rows = c.Children.Where(r => string.Equals(r.TagName, "tr", StringComparison.OrdinalIgnoreCase)).ToList();
                    int rcount = rows.Count;
                    int ccount = 0;
                    if (rcount > 0) ccount = rows[0].Children.Count;
                    table.RowCount = Math.Max(rcount,1);
                    table.ColumnCount = Math.Max(ccount,1);
                    for (int r=0;r<rcount;r++)
                    {
                        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        for (int cc=0;cc<ccount;cc++)
                        {
                            if (r==0) table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                            var cell = rows[r].Children[cc];
                            var lbl = new Label { Text = cell.GetInnerText(), AutoSize = true, Padding = new Padding(4) };
                            table.Controls.Add(lbl, cc, r);
                        }
                    }
                    renderPanel.Controls.Add(table);
                    y += table.Height + 6;
                }
                else
                {
                    var lbl = new Label { Text = $"<{c.TagName}> " + c.GetInnerText(), AutoSize = true, Location = new Point(10, y) };
                    renderPanel.Controls.Add(lbl); y += lbl.Height + 6;
                }
            }
        }
    }
}
