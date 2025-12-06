using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Crawler
{
    public partial class FormHtmlRender : Form
    {
        private HtmlNode root;

        public FormHtmlRender(HtmlNode node)
        {
            root = node;
            this.DoubleBuffered = true;
            this.BackColor = Color.White;
            this.Font = new Font("Arial", 11);
            this.Width = 1000;
            this.Height = 800;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int y = 20;
            DrawNode(e.Graphics, root, 20, ref y);
        }

        private void DrawNode(Graphics g, HtmlNode node, int x, ref int y)
        {
            if (node == null) return;

            if (EqualsIgnoreCase(node.TagName, "table"))
            {
                DrawTable(g, node, x, ref y);
                return;
            }

            if (EqualsIgnoreCase(node.TagName, "img"))
            {
                DrawImage(g, node, x, ref y);
                return;
            }

            if (EqualsIgnoreCase(node.TagName, "a"))
            {
                DrawLink(g, node, x, ref y);
                return;    
            }

            if (!string.IsNullOrWhiteSpace(node.InnerText))
            {
                string text = node.InnerText.Trim();
                SizeF sz = g.MeasureString(text, this.Font);
                g.DrawString(text, this.Font, Brushes.Black, x, y);
                y += (int)sz.Height + 5;
            }

            HtmlNode child = node.FirstChild;
            while (child != null)
            {
                DrawNode(g, child, x + 20, ref y);
                child = child.NextSibling;
            }
        }

        private void DrawTable(Graphics g, HtmlNode table, int x, ref int y)
        {
            int curY = y;

            HtmlNode row = table.FirstChild;

            while (row != null)
            {
                int rowHeight = GetRowHeight(row);
                int curX = x;

                HtmlNode cell = row.FirstChild;
                while (cell != null)
                {
                    Rectangle rect = new Rectangle(curX, curY, 180, rowHeight);
                    g.DrawRectangle(Pens.Black, rect);

                    string txt = cell.InnerText == null ? "" : cell.InnerText.Trim();
                    if (txt != "")
                    {
                        g.DrawString(txt, this.Font, Brushes.Black, rect);
                    }

                    curX += 180;
                    cell = cell.NextSibling;
                }

                curY += rowHeight;
                row = row.NextSibling;
            }

            y = curY + 10;
        }

        private int GetRowHeight(HtmlNode row)
        {
            int max = 25;

            HtmlNode cell = row.FirstChild;
            while (cell != null)
            {
                if (!string.IsNullOrWhiteSpace(cell.InnerText))
                {
                    Size size = TextRenderer.MeasureText(cell.InnerText.Trim(), this.Font);
                    if (size.Height + 8 > max)
                        max = size.Height + 8;
                }
                cell = cell.NextSibling;
            }
            return max;
        }

        private void DrawImage(Graphics g, HtmlNode node, int x, ref int y)
        {
            string src = node.GetAttribute("src");
            if (src == null) return;

            string full = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, src);

            if (!File.Exists(full)) return;

            using (Bitmap bmp = new Bitmap(full))
            {
                g.DrawImage(bmp, x, y);
                y += bmp.Height + 10;
            }
        }

        private void DrawLink(Graphics g, HtmlNode node, int x, ref int y)
        {
            string txt = node.InnerText == null ? "" : node.InnerText.Trim();

            if (txt == "") return;

            SizeF size = g.MeasureString(txt, this.Font);

            g.DrawString(txt, this.Font, Brushes.Blue, x, y);
            g.DrawLine(Pens.Blue, x, y + (int)size.Height, x + (int)size.Width, y + (int)size.Height);

            y += (int)size.Height + 5;
        }

        private bool EqualsIgnoreCase(string a, string b)
        {
            if (a == null || b == null) return false;
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
        private void FormHtmlRender_Load(object sender, EventArgs e)
        {

        }
    }
}
