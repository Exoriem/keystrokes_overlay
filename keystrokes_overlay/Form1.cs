using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using System.Runtime.InteropServices;

namespace keystrokes_overlay
{
    

    public partial class Form1 : Form
    {
        public class DraggableLabel : Label
        {
            // Importy z WinAPI do przesuwania okna
            [DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
            [DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HTCAPTION = 0x2;

            public DraggableLabel()
            {
                this.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        var form = this.FindForm();
                        if (form != null)
                        {
                            ReleaseCapture();
                            SendMessage(form.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                        }
                    }
                };
            }
        }
        class WheelNumericUpDown : NumericUpDown
        {
            protected override void OnMouseWheel(MouseEventArgs e)
            {
                decimal step = this.Increment;

                if (e.Delta > 0)
                    Value = Math.Min(Maximum, Value + step);
                else
                    Value = Math.Max(Minimum, Value - step);

                // NIE wywołujemy base.OnMouseWheel(e);
            }
        }
        // Importy do przesuwania okna
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        private CheckBox chkTopMost = null!;
        private Button btnStart = null!;

        private Button btnTextColor = null!;
        private Button btnArrowColor = null!;
        private Button btnOutlineColor = null!;
        private WheelNumericUpDown nudOutlineThickness = null!;
        private WheelNumericUpDown nudDurationTime = null!;
        private WheelNumericUpDown nudFadeoutTime = null!;

        private GroupBox grpLetters = null!;
        private GroupBox grpNumbers = null!;
        private GroupBox grpSpecial = null!;

        private CheckedListBox clbLetters = null!;
        private CheckedListBox clbNumbers = null!;
        private CheckedListBox clbSpecial = null!;

        private Button btnToggleLetters = null!;
        private Button btnToggleNumbers = null!;
        private Button btnToggleSpecial = null!;

        private HashSet<string> allowedKeys = new();

        private readonly string[] Letters =
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M",
            "N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
        };

        private readonly string[] Numbers =
        {
            "0","1","2","3","4","5","6","7","8","9",
            "NumPad0","NumPad1","NumPad2","NumPad3","NumPad4",
            "NumPad5","NumPad6","NumPad7","NumPad8","NumPad9"
        };

        private readonly string[] SpecialKeys =
        {
            "LMB","RMB","MMB","XMB1","XMB2", //mouse
            "Up","Down","Left","Right",  //arrows
            "Lctrl","Rctrl","Lshift","Rshift","Lalt","Ralt","LWin", //alt ,ctrl, shift
            "Backspace","Enter","Esc","Tab","Space", "Capital", "NumLock",
            "Scroll","Pause","PrintScreen", "Insert","Delete","Home","End","PageUp","Next", //keys abouve arrows
            "+","-","Divide", "Oemplus","OemPeriod", "Oemcomma","Oem6", "Oem4", "Subtract", "Decimal", "Clear", // +, -, /, =, ., ,, ],[, -, ,
            "OemSemicolon", "OemQuotes", "OemPeriod","Oemtilde", "OemPipe", "Apps",// ;, ', ., /, `. \
            "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12"
        };

        public Form1()
        {
            InitializeComponent();
            // przesuwanie klikając formę w dowolnym miejscu
            this.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            };
            InitUI();
            LoadSettings();
        }

        private void InitUI()
        {
            Button btnClose = new Button
            {
                Text = "X",
                Size = new Size(20, 19),
                Location = new Point(this.Width - 20, -4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7, FontStyle.Bold), // zmiana fontu i rozmiaru
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            // Hover
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(255, 155, 0, 0); // czerwone po najechaniu
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.Transparent; // normalnie przezroczysty
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);

            Button btnMinimize = new Button
            {
                Text = "–",
                Size = new Size(20, 19),
                Location = new Point(this.Width - 40, -4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7, FontStyle.Bold), // zmiana fontu i rozmiaru
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            // Hover
            btnMinimize.MouseEnter += (s, e) => btnMinimize.BackColor = Color.FromArgb(255, 55, 55, 55); // czerwone po najechaniu
            btnMinimize.MouseLeave += (s, e) => btnMinimize.BackColor = Color.Transparent; // normalnie przezroczysty
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            this.Controls.Add(btnMinimize);

            Button infoTooltip = new Button
            {
                Text = "?",
                Size = new Size(20, 19),
                Location = new Point(this.Width - 60, -4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Regular), // zmiana fontu i rozmiaru
                ForeColor = Color.White,
                Cursor = Cursors.Help
            };
            // Hover
            infoTooltip.MouseEnter += (s, e) => infoTooltip.BackColor = Color.FromArgb(255, 55, 55, 55); // czerwone po najechaniu
            infoTooltip.MouseLeave += (s, e) => infoTooltip.BackColor = Color.Transparent; // normalnie przezroczysty
            infoTooltip.FlatAppearance.BorderSize = 0;
            ToolTip tt = new ToolTip
            {
                InitialDelay = 0,      // czas przed pokazaniem po najechaniu
                ShowAlways = true
            };
            tt.SetToolTip(infoTooltip, "Ustawienia są zapisywane lokalnie.\nPo przeniesieniu programu zostanie utworzona nowa konfiguracja.\nNie przenoś aplikacji po ustawieniu opcji.");

            this.Controls.Add(infoTooltip);

            DraggableLabel textTitle = new DraggableLabel
            {
                Text = "Keystrokes Overlay",
                Location = new Point(5, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 16, FontStyle.Bold), // zmiana fontu i rozmiaru
                ForeColor = Color.FromArgb(255, 200, 200, 200)
            };
            Controls.Add(textTitle);
            DraggableLabel textVersion = new DraggableLabel
            {
                Text = "v0.1",
                Location = new Point(215, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold), // zmiana fontu i rozmiaru
                ForeColor = Color.FromArgb(255, 155, 155, 155)
            };
            Controls.Add(textVersion);
            LinkLabel textDevelopedby = new LinkLabel
            {
                Text = "Developed by Exoriem",
                Location = new Point(245, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold), // zmiana fontu i rozmiaru
                ForeColor = Color.FromArgb(255, 155, 155, 155)
            };
            textDevelopedby.LinkBehavior = LinkBehavior.NeverUnderline;
            textDevelopedby.LinkColor = Color.FromArgb(255, 142, 150, 199);
            textDevelopedby.MouseEnter += (s, e) => textDevelopedby.LinkColor = Color.FromArgb(255, 30, 152, 255); // kolor na hover
            textDevelopedby.MouseLeave += (s, e) => textDevelopedby.LinkColor = Color.FromArgb(255, 142, 150, 199);   // przywrócenie koloru normalnego
            textDevelopedby.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://github.com/Exoriem") { UseShellExecute = true });
            Controls.Add(textDevelopedby);

            LinkLabel textTitle3 = new LinkLabel
            {
                Text = "Buy Me a Coffee!",
                Location = new Point(400, 18),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold), // zmiana fontu i rozmiaru
                ForeColor = Color.FromArgb(255, 155, 155, 155)
            };

            PictureBox pb = new PictureBox
            {
                Image = Properties.Resources.coffee2, // <-- obrazek z resources
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(376, 14),
                Size = new Size(22, 22),
                Cursor = Cursors.Hand
            };
            pb.MouseEnter += (s, e) => { pb.Image = Properties.Resources.coffee; textTitle3.LinkColor = Color.FromArgb(255, 30, 152, 255); }; // kolor na hover
            pb.MouseLeave += (s, e) => { pb.Image = Properties.Resources.coffee2; textTitle3.LinkColor = Color.FromArgb(255, 142, 150, 199); };  // przywrócenie koloru normalnego
            textTitle3.MouseEnter += (s, e) => pb.Image = Properties.Resources.coffee; // kolor na hover
            textTitle3.MouseLeave += (s, e) => pb.Image = Properties.Resources.coffee2;   // przywrócenie koloru normalnego

            pb.Click += (s, e) => Process.Start(new ProcessStartInfo("https://buymeacoffee.com/exoriem") { UseShellExecute = true });

            this.Controls.Add(pb);

            textTitle3.LinkBehavior = LinkBehavior.NeverUnderline;
            textTitle3.LinkColor = Color.FromArgb(255, 142, 150, 199);
            textTitle3.MouseEnter += (s, e) => textTitle3.LinkColor = Color.FromArgb(255, 30, 152, 255); // kolor na hover
            textTitle3.MouseLeave += (s, e) => textTitle3.LinkColor = Color.FromArgb(255, 142, 150, 199);   // przywrócenie koloru normalnego
            textTitle3.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://buymeacoffee.com/exoriem") { UseShellExecute = true });
            Controls.Add(textTitle3);

            chkTopMost = new CheckBox
            {
                Text = "Overlay over applications",
                Location = new Point(10, 445),
                Font = new Font("Segoe UI", 8, FontStyle.Bold), // zmiana fontu i rozmiaru
                AutoSize = true,
                Cursor = Cursors.Hand
             };
            Controls.Add(chkTopMost);

            // ===== LETTERS =====
            grpLetters = new GroupBox
            {
                Text = "Letters",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 40),
                Size = new Size(150, 300)
            };

            clbLetters = CreateCheckedListBox(Letters);
            grpLetters.Controls.Add(clbLetters);
            Controls.Add(grpLetters);

            btnToggleLetters = new Button
            {
                Text = "Toggle Letters",
                Location = new Point(10, 345),
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 25, 25), // ciemnoszary
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            // Efekt hover
            btnToggleLetters.MouseEnter += (s, e) => btnToggleLetters.BackColor = Color.FromArgb(70, 70, 70);
            btnToggleLetters.MouseLeave += (s, e) => btnToggleLetters.BackColor = Color.FromArgb(25, 25, 25);

            btnToggleLetters.Click += (s, e) => ToggleGroup(clbLetters);
            Controls.Add(btnToggleLetters);
            clbLetters.MouseMove += (s, e) =>
            {
                int index = clbLetters.IndexFromPoint(e.Location);
                clbLetters.Cursor = index != ListBox.NoMatches ? Cursors.Hand : Cursors.Default;
            };
            // ===== NUMBERS =====
            grpNumbers = new GroupBox
            {
                Text = "Numbers",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(180, 40),
                Size = new Size(150, 300)
            };

            clbNumbers = CreateCheckedListBox(Numbers);
            grpNumbers.Controls.Add(clbNumbers);
            Controls.Add(grpNumbers);
            clbNumbers.MouseMove += (s, e) =>
            {
                int index = clbNumbers.IndexFromPoint(e.Location);
                clbNumbers.Cursor = index != ListBox.NoMatches ? Cursors.Hand : Cursors.Default;
            };
            clbNumbers.MouseDown += (s, e) =>
            {
                int index = clbNumbers.IndexFromPoint(e.Location);
                if (index == ListBox.NoMatches)
                {
                    clbNumbers.SelectedIndex = -1;
                }
            };

            btnToggleNumbers = new Button
            {
                Text = "Toggle Numbers",
                Location = new Point(180, 345),
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 25, 25), // ciemnoszary
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            // Efekt hover
            btnToggleNumbers.MouseEnter += (s, e) => btnToggleNumbers.BackColor = Color.FromArgb(70, 70, 70);
            btnToggleNumbers.MouseLeave += (s, e) => btnToggleNumbers.BackColor = Color.FromArgb(25, 25, 25);
            btnToggleNumbers.Click += (s, e) => ToggleGroup(clbNumbers);
            Controls.Add(btnToggleNumbers);

            // ===== SPECIAL =====
            grpSpecial = new GroupBox
            {
                Text = "Special Keys",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(350, 40),
                Size = new Size(150, 300)
            };

            clbSpecial = CreateCheckedListBox(SpecialKeys);
            grpSpecial.Controls.Add(clbSpecial);
            Controls.Add(grpSpecial);
            clbSpecial.MouseMove += (s, e) =>
            {
                int index = clbSpecial.IndexFromPoint(e.Location);
                clbSpecial.Cursor = index != ListBox.NoMatches ? Cursors.Hand : Cursors.Default;
            };

            btnToggleSpecial = new Button
            {
                Text = "Toggle Specialkeys",
                Location = new Point(350, 345),
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 25, 25), // ciemnoszary
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            // Efekt hover
            btnToggleSpecial.MouseEnter += (s, e) => btnToggleSpecial.BackColor = Color.FromArgb(70, 70, 70);
            btnToggleSpecial.MouseLeave += (s, e) => btnToggleSpecial.BackColor = Color.FromArgb(25, 25, 25);

            btnToggleSpecial.Click += (s, e) => ToggleGroup(clbSpecial);
            Controls.Add(btnToggleSpecial);

            // ===== START =====
            btnStart = new Button
            {
                Text = "Save and Start Overlay",
                Location = new Point(10, 470),
                Size = new Size(490, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 25, 25), // ciemnoszary
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            // Efekt hover
            btnStart.MouseEnter += (s, e) => btnStart.BackColor = Color.FromArgb(70, 70, 70);
            btnStart.MouseLeave += (s, e) => btnStart.BackColor = Color.FromArgb(25, 25, 25);
            btnStart.Click += BtnStart_Click;
            Controls.Add(btnStart);
            // ===== NOWE KONFIGURACJE OVERLAY =====
            DraggableLabel lblTextColor = new DraggableLabel { Text = "Text Color:", Location = new Point(10, 385), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
            Controls.Add(lblTextColor);

            btnTextColor = new Button { Location = new Point(75, 380), Size = new Size(25, 25), Cursor = Cursors.Hand };
            btnTextColor.Click += (s, e) => PickColor(c => btnTextColor.BackColor = c);
            btnTextColor.FlatStyle = FlatStyle.Flat;
            btnTextColor.FlatAppearance.BorderColor = Color.White; // kolor obwódki
            Controls.Add(btnTextColor);

            DraggableLabel lblArrowColor = new DraggableLabel { Text = "Arrow Color:", Location = new Point(110, 385), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
            Controls.Add(lblArrowColor);

            btnArrowColor = new Button { Location = new Point(185, 380), Size = new Size(25, 25), Cursor = Cursors.Hand };
            btnArrowColor.Click += (s, e) => PickColor(c => btnArrowColor.BackColor = c);
            btnArrowColor.FlatStyle = FlatStyle.Flat;
            btnArrowColor.FlatAppearance.BorderColor = Color.White; // kolor obwódki
            Controls.Add(btnArrowColor);

            DraggableLabel lblOutlineColor = new DraggableLabel { Text = "Outline Color:", Location = new Point(220, 385), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
            Controls.Add(lblOutlineColor);

            btnOutlineColor = new Button { Location = new Point(300, 380), Size = new Size(25, 25), Cursor = Cursors.Hand };
            btnOutlineColor.Click += (s, e) => PickColor(c => btnOutlineColor.BackColor = c);
            btnOutlineColor.FlatStyle = FlatStyle.Flat;
            btnOutlineColor.FlatAppearance.BorderColor = Color.White; // kolor obwódki
            Controls.Add(btnOutlineColor);

            DraggableLabel lblOutlineThickness = new DraggableLabel { Text = "Outline Thickness:", Location = new Point(335, 385), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
            Controls.Add(lblOutlineThickness);

            nudOutlineThickness = new WheelNumericUpDown { Location = new Point(440, 383), Size = new Size(58, 25), Minimum = 0, Maximum = 2, Value = 0, Increment = 1, Cursor = Cursors.Hand, BorderStyle = BorderStyle.None };
            Controls.Add(nudOutlineThickness);

            DraggableLabel lblDurationTime = new DraggableLabel { Text = "Duration Time:", Location = new Point(10, 420), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
            Controls.Add(lblDurationTime);

            nudDurationTime = new WheelNumericUpDown { Location = new Point(100, 418), Size = new Size(60, 25), Minimum = 0, Maximum = 5000, Value = 500, Increment = 10, Cursor = Cursors.Hand, BorderStyle = BorderStyle.None };
            Controls.Add(nudDurationTime);

            DraggableLabel lblFadeoutTime = new DraggableLabel { Text = "Fade Out Time:", Location = new Point(170, 420), Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true };
            Controls.Add(lblFadeoutTime);

            nudFadeoutTime = new WheelNumericUpDown { Location = new Point(260, 418), Size = new Size(60, 25), Minimum = 0, Maximum = 5000, Value = 500, Increment = 10, Cursor = Cursors.Hand, BorderStyle = BorderStyle.None };
            Controls.Add(nudFadeoutTime);

        }

        private CheckedListBox CreateCheckedListBox(IEnumerable<string> items)
        {
            var clb = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true
            };

            foreach (var item in items)
                clb.Items.Add(item, false);

            clb.ItemCheck += (s, e) =>
            {
                string? key = clb.Items[e.Index] as string;
                if (key == null) return; // nic nie robimy jeśli null
                if (e.NewValue == CheckState.Checked)
                    allowedKeys.Add(key);
                else
                    allowedKeys.Remove(key);
            };

            return clb;
        }
        private void PickColor(Action<Color> callback)
        {
            using ColorDialog dlg = new ColorDialog
            {
                FullOpen = true // umożliwia gradient i pełną paletę
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                callback(dlg.Color);
        }

        private void ToggleGroup(CheckedListBox clb)
        {
            bool enable = clb.Items.Cast<string>().Any(k => !allowedKeys.Contains(k));

            for (int i = 0; i < clb.Items.Count; i++)
            {
                clb.SetItemChecked(i, enable);
                string? key = clb.Items[i] as string;
                if (key == null) return; // nic nie robimy jeśli null
                if (enable) allowedKeys.Add(key);
                else allowedKeys.Remove(key);
            }
        }

        private void LoadSettings()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.AllowedKeys))
            {
                allowedKeys = Properties.Settings.Default.AllowedKeys.Split(',').ToHashSet();

                RestoreCheckedState(clbLetters);
                RestoreCheckedState(clbNumbers);
                RestoreCheckedState(clbSpecial);
            }

            chkTopMost.Checked = Properties.Settings.Default.OverlayTopMost;
            btnArrowColor.BackColor = Properties.Settings.Default.btnArrowColor;
            btnTextColor.BackColor = Properties.Settings.Default.btnTextColor;
            btnOutlineColor.BackColor = Properties.Settings.Default.btnOutlineColor;
            nudOutlineThickness.Value = Properties.Settings.Default.nudOutlineThickness;
            nudDurationTime.Value = Properties.Settings.Default.durationTime;
            nudFadeoutTime.Value = Properties.Settings.Default.fadeDuration;
        }

        private void RestoreCheckedState(CheckedListBox clb)
        {
            for (int i = 0; i < clb.Items.Count; i++)
            {
                string? key = clb.Items[i] as string;
                if (key == null) return; // nic nie robimy jeśli null
                clb.SetItemChecked(i, allowedKeys.Contains(key));
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            var selectedKeys = new HashSet<string>(allowedKeys);

            OverlayForm overlay = new OverlayForm(selectedKeys, chkTopMost.Checked)
            {
                TextColor = btnTextColor.BackColor,
                ArrowColor = btnArrowColor.BackColor,
                OutlineColor = btnOutlineColor.BackColor,
                OutlineThickness = (int)nudOutlineThickness.Value,
                DurationTime = (int)nudDurationTime.Value,
                fadeDuration = (int)nudFadeoutTime.Value
            };
            overlay.Show();

            Properties.Settings.Default.AllowedKeys = string.Join(",", selectedKeys);
            Properties.Settings.Default.btnArrowColor = btnArrowColor.BackColor;
            Properties.Settings.Default.btnTextColor = btnTextColor.BackColor;
            Properties.Settings.Default.btnOutlineColor = btnOutlineColor.BackColor;
            Properties.Settings.Default.nudOutlineThickness = nudOutlineThickness.Value;
            Properties.Settings.Default.durationTime = nudDurationTime.Value;
            Properties.Settings.Default.fadeDuration = nudFadeoutTime.Value;
            Properties.Settings.Default.OverlayTopMost = chkTopMost.Checked;
            Properties.Settings.Default.Save();

            this.Hide();
        }
    }
}
