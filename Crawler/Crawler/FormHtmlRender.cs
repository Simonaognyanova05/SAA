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
            this.Width = 900;
            this.Height = 700;
            this.BackColor = Color.White;
            this.Font = new Font("Arial", 9);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawNode(e.Graphics, root, 20, 20);
        }

        private int DrawNode(Graphics g, HtmlNode node, int x, int y)
        {
            if (node == null) return y;

            // TABLE
            if (EqualsIgnoreCase(node.TagName, "table"))
            {
                int startY = y;
                int curY = y;

                HtmlNode row = node.FirstChild;
                while (row != null)
                {
                    int rowHeight = GetRowHeight(row);
                    int curX = x;

                    HtmlNode cell = row.FirstChild;
                    while (cell != null)
                    {
                        Rectangle rect = new Rectangle(curX, curY, 150, rowHeight);

                        g.DrawRectangle(Pens.Black, rect);

                        if (!string.IsNullOrEmpty(cell.InnerText))
                        {
                            g.DrawString(cell.InnerText, this.Font, Brushes.Black, rect);
                        }

                        curX += 150;
                        cell = cell.NextSibling;
                    }

                    curY += rowHeight;
                    row = row.NextSibling;
                }

                return curY + 10;
            }

            // IMG (only BMP)
            if (EqualsIgnoreCase(node.TagName, "img"))
            {
                string src = node.GetAttribute("src");

                if (!string.IsNullOrEmpty(src))
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string fullPath = Path.Combine(baseDir, src);

                    if (File.Exists(fullPath) && src.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    {
                        using (Bitmap bmp = new Bitmap(fullPath))
                        {
                            g.DrawImage(bmp, x, y);
                            return y + bmp.Height + 10;
                        }
                    }
                }
            }

            // A (blue + underline)
            if (EqualsIgnoreCase(node.TagName, "a"))
            {
                string txt = node.InnerText;
                g.DrawString(txt, this.Font, Brushes.Blue, x, y);

                SizeF size = g.MeasureString(txt, this.Font);
                g.DrawLine(Pens.Blue, x, y + (int)size.Height, x + (int)size.Width, y + (int)size.Height);

                return y + (int)size.Height + 5;
            }

            // Plain text
            if (!string.IsNullOrWhiteSpace(node.InnerText))
            {
                g.DrawString(node.InnerText, this.Font, Brushes.Black, x, y);
                SizeF size = g.MeasureString(node.InnerText, this.Font);
                y += (int)size.Height + 5;
            }

            // Draw children
            HtmlNode child = node.FirstChild;
            while (child != null)
            {
                y = DrawNode(g, child, x + 20, y);
                child = child.NextSibling;
            }

            return y;
        }

        private int GetRowHeight(HtmlNode row)
        {
            int max = 30;

            HtmlNode cell = row.FirstChild;
            while (cell != null)
            {
                if (!string.IsNullOrEmpty(cell.InnerText))
                {
                    int h = (int)TextRenderer.MeasureText(cell.InnerText, this.Font).Height + 10;
                    if (h > max) max = h;
                }
                cell = cell.NextSibling;
            }

            return max;
        }

        private bool EqualsIgnoreCase(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private void FormHtmlRender_Load(object sender, EventArgs e)
        {

        }
    }
}
