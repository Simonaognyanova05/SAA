using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using E2Pass.Resources;
using System.IO;
using System.Diagnostics;

namespace E2Pass
{
    public partial class FormMain : Form
    {
        #region CaptureFields

        private bool _capturing;
        private IntPtr _hCapturedWindow;
        private IntPtr _hTextBoxWindow;
        private IntPtr _hButtonWindow;

        private Image _finderHome;
        private Image _finderGone;
        private Cursor _cursorDefault;
        private Cursor _cursorFinder;

        #endregion

        public FormMain()
        {
            InitializeComponent();

            #region CaptureInit

            _cursorDefault = Cursor.Current;

            using (var cursorStream = new MemoryStream(EmbeddedResources.Finder))
                _cursorFinder = new Cursor(cursorStream);

            _finderHome = EmbeddedResources.FinderHome;          
            _finderGone = EmbeddedResources.FinderGone;

            pictureBox.Image = _finderHome;

            #endregion
        }

        #region Capture

        private void CaptureMouse(bool captured)
        {
            if (captured)
            {
                Win32.SetCapture(Handle);

                Cursor.Current = _cursorFinder;
                pictureBox.Image = _finderGone;
            }
            else
            {
                Win32.ReleaseCapture();

                Cursor.Current = _cursorDefault;
                pictureBox.Image = _finderHome;

                if (_hCapturedWindow != IntPtr.Zero)
                {
                    if (radioButtonTextBox.Checked)
                    {
                        _hTextBoxWindow = _hCapturedWindow;
                        radioButtonTextBox.ForeColor = Color.Green;
                        radioButtonTextBox.Visible = false;
                        radioButtonButton.Checked = true;
                    }
                    else if (radioButtonButton.Checked)
                    {
                        _hButtonWindow = _hCapturedWindow;
                        radioButtonButton.ForeColor = Color.Green;
                        radioButtonButton.Visible = false;
                        radioButtonTextBox.Checked = true;
                    }

                    if (_hTextBoxWindow != IntPtr.Zero &&
                        _hButtonWindow != IntPtr.Zero)
                    {
                        pictureBox.Visible = false;
                        buttonRun.Visible = true;
                    }

                    WindowHighlighter.Refresh(_hCapturedWindow);
                    _hCapturedWindow = IntPtr.Zero;
                }
            }

            _capturing = captured;
        }

        private void HandleMouseMovements()
        {
            if (!_capturing)
                return;

            IntPtr hWnd = Win32.WindowFromPoint(
                Cursor.Position);

            if (_hCapturedWindow != IntPtr.Zero && 
                _hCapturedWindow != hWnd)
                WindowHighlighter.Refresh(_hCapturedWindow);

            if (hWnd != IntPtr.Zero)
            {
                _hCapturedWindow = hWnd;                

                Win32.Rect rc = new Win32.Rect();
                Win32.GetWindowRect(hWnd, ref rc);

                WindowHighlighter.Highlight(hWnd);
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)Win32.WindowMessages.WM_LBUTTONUP:
                    CaptureMouse(false);
                    break;

                case (int)Win32.WindowMessages.WM_RBUTTONDOWN:
                    CaptureMouse(false);
                    break;

                case (int)Win32.WindowMessages.WM_MOUSEMOVE:
                    HandleMouseMovements();
                    break;
            };

            base.WndProc(ref m);
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {            
            if (e.Button == MouseButtons.Left)
                CaptureMouse(true);
        }

        private void SendTextAndClick(string text)
        {
            const int WM_SETTEXT = 0x000C;
            const int BM_CLICK = 0x00F5;

            Win32.SendMessage(
                _hTextBoxWindow,
                WM_SETTEXT,
                IntPtr.Zero,
                text);

            Win32.SendMessage(
                _hButtonWindow,
                BM_CLICK,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        #endregion        

        private void buttonRun_Click(object sender, EventArgs e)
        {
            SendTextAndClick("123");
        }
    }
}
