using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace keystrokes_overlay
{
    public class OverlayForm : Form
    {
        private IKeyboardMouseEvents globalHook;
        private System.Windows.Forms.Timer updateTimer;
        private List<OverlayItem> overlays = new();
        private List<string> pressedKeys = new();

        private HashSet<string> allowedKeys;
        private bool overlayOnTop;

        // ====================== CLICK-THROUGH ======================
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
           int X, int Y, int cx, int cy, uint uFlags);

        static readonly IntPtr HWND_BOTTOM = new(1);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_SHOWWINDOW = 0x0040;

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;

        private const int fadeDuration = 500;
        private const int holdTime = 250;
        private const int timerInterval = 30;

        public OverlayForm(HashSet<string> keys, bool topMost)
        {
            allowedKeys = keys;
            overlayOnTop = topMost;

            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            WindowState = FormWindowState.Maximized;

            updateTimer = new System.Windows.Forms.Timer { Interval = timerInterval };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            HookInput();

            ApplyWindowPosition();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            int exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
            SetWindowLong(Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);

            ApplyWindowPosition();
        }

        private void ApplyWindowPosition()
        {
            if (overlayOnTop)
                TopMost = true;
            else
                SetWindowPos(Handle, HWND_BOTTOM, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        private void HookInput()
        {
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;
            globalHook.KeyUp += GlobalHook_KeyUp;
            globalHook.MouseDown += GlobalHook_MouseDown;
        }

        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            string key = ConvertKey(e.KeyCode.ToString());
            if (!allowedKeys.Contains(key)) return;

            if (!pressedKeys.Contains(key))
            {
                pressedKeys.Add(key);
                AddOverlay(string.Join(" + ", pressedKeys));
            }
        }

        private void GlobalHook_KeyUp(object sender, KeyEventArgs e)
        {
            string key = ConvertKey(e.KeyCode.ToString());
            pressedKeys.Remove(key);
        }

        private void GlobalHook_MouseDown(object sender, MouseEventArgs e)
        {
            string button = e.Button switch
            {
                MouseButtons.Left => "LMB",
                MouseButtons.Right => "RMB",
                MouseButtons.Middle => "MMB",
                MouseButtons.XButton1 => "XMB1",
                MouseButtons.XButton2 => "XMB2",
                _ => null
            };

            if (button == null || !allowedKeys.Contains(button)) return;
            AddOverlay(button);
        }

        private string ConvertKey(string key)
        {
            if (key.StartsWith("D") && key.Length == 2 && char.IsDigit(key[1]))
                return key[1].ToString();

            return key switch
            {
                "LControlKey" => "Lctrl",
                "RControlKey" => "Rctrl",
                "LShiftKey" => "Lshift",
                "RShiftKey" => "Rshift",
                "LMenu" => "Lalt",
                "RMenu" => "Ralt",
                "Back" => "Backspace",
                "Return" => "Enter",
                "Escape" => "Esc",
                _ => key
            };
        }

        private void AddOverlay(string text)
        {
            overlays.Add(new OverlayItem
            {
                Position = Cursor.Position,
                Text = text,
                Alpha = 1f,
                StartTime = Environment.TickCount
            });
            Invalidate();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            int now = Environment.TickCount;
            bool refresh = false;

            for (int i = overlays.Count - 1; i >= 0; i--)
            {
                int elapsed = now - overlays[i].StartTime;
                if (elapsed >= fadeDuration)
                {
                    overlays.RemoveAt(i);
                    refresh = true;
                }
                else if (elapsed > holdTime)
                {
                    overlays[i].Alpha = 1f - (float)(elapsed - holdTime) / (fadeDuration - holdTime);
                    refresh = true;
                }
            }

            if (refresh) Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using Font font = new("Segoe UI", 12, FontStyle.Bold);
            using StringFormat sf = new() { Alignment = StringAlignment.Center };

            float flipY = Screen.PrimaryScreen.Bounds.Height * 0.05f;

            foreach (var item in overlays)
            {
                bool below = item.Position.Y < flipY;
                string arrow = below ? "▲" : "▼";

                PointF arrowPos = below
                    ? new(item.Position.X, item.Position.Y - 5)
                    : new(item.Position.X, item.Position.Y - 18);

                PointF textPos = below
                    ? new(item.Position.X, item.Position.Y + 14)
                    : new(item.Position.X, item.Position.Y - 36);

                int a = (int)(item.Alpha * 255);
                using Brush textBrush = new SolidBrush(Color.FromArgb(a, Color.Red));
                using Brush outline = new SolidBrush(Color.FromArgb(a, Color.Black));

                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        e.Graphics.DrawString(item.Text, font, outline, textPos.X + dx, textPos.Y + dy, sf);
                        e.Graphics.DrawString(arrow, font, outline, arrowPos.X + dx, arrowPos.Y + dy, sf);
                    }

                e.Graphics.DrawString(item.Text, font, textBrush, textPos, sf);
                e.Graphics.DrawString(arrow, font, textBrush, arrowPos, sf);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            globalHook.Dispose();
            base.OnFormClosing(e);
        }
    }

    public class OverlayItem
    {
        public Point Position;
        public string Text;
        public float Alpha;
        public int StartTime;
    }
}
