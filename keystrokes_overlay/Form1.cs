using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace keystrokes_overlay
{
    public partial class Form1 : Form
    {
        private CheckBox chkTopMost;
        private Button btnStart;

        private Button btnTextColor;
        private Button btnArrowColor;
        private Button btnOutlineColor;
        private NumericUpDown nudOutlineThickness;

        private GroupBox grpLetters;
        private GroupBox grpNumbers;
        private GroupBox grpSpecial;

        private CheckedListBox clbLetters;
        private CheckedListBox clbNumbers;
        private CheckedListBox clbSpecial;

        private Button btnToggleLetters;
        private Button btnToggleNumbers;
        private Button btnToggleSpecial;

        private HashSet<string> allowedKeys = new();

        private readonly string[] Letters =
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M",
            "N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
        };

        private readonly string[] Numbers =
        {
            "0","1","2","3","4","5","6","7","8","9"
        };

        private readonly string[] SpecialKeys =
        {
            "Lctrl","Rctrl","Lshift","Rshift","Lalt","Ralt",
            "Backspace","Enter","Esc","Tab","Space",
            "Insert","Delete","Home","End","PageUp","PageDown",
            "Up","Down","Left","Right",
            "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12",
            "LMB","RMB","MMB","XMB1","XMB2"
        };

        public Form1()
        {
            InitializeComponent();
            InitUI();
            LoadSettings();
        }

        private void InitUI()
        {
            this.Size = new Size(520, 520);

            chkTopMost = new CheckBox
            {
                Text = "Overlay nad aplikacjami",
                Location = new Point(10, 375),
                AutoSize = true
            };
            Controls.Add(chkTopMost);

            // ===== LETTERS =====
            grpLetters = new GroupBox
            {
                Text = "Letters",
                ForeColor = Color.White,
                Location = new Point(10, 40),
                Size = new Size(150, 300)
            };

            clbLetters = CreateCheckedListBox(Letters);
            grpLetters.Controls.Add(clbLetters);
            Controls.Add(grpLetters);

            btnToggleLetters = new Button
            {
                Text = "Toggle",
                Location = new Point(10, 345),
                Size = new Size(150, 25)
            };
            btnToggleLetters.Click += (s, e) => ToggleGroup(clbLetters);
            Controls.Add(btnToggleLetters);

            // ===== NUMBERS =====
            grpNumbers = new GroupBox
            {
                Text = "Numbers",
                ForeColor = Color.White,
                Location = new Point(180, 40),
                Size = new Size(120, 300)
            };

            clbNumbers = CreateCheckedListBox(Numbers);
            grpNumbers.Controls.Add(clbNumbers);
            Controls.Add(grpNumbers);

            btnToggleNumbers = new Button
            {
                Text = "Toggle",
                Location = new Point(180, 345),
                Size = new Size(120, 25)
            };
            btnToggleNumbers.Click += (s, e) => ToggleGroup(clbNumbers);
            Controls.Add(btnToggleNumbers);

            // ===== SPECIAL =====
            grpSpecial = new GroupBox
            {
                Text = "Special Keys",
                ForeColor = Color.White,
                Location = new Point(320, 40),
                Size = new Size(170, 300)
            };

            clbSpecial = CreateCheckedListBox(SpecialKeys);
            grpSpecial.Controls.Add(clbSpecial);
            Controls.Add(grpSpecial);

            btnToggleSpecial = new Button
            {
                Text = "Toggle",
                Location = new Point(320, 345),
                Size = new Size(170, 25)
            };
            btnToggleSpecial.Click += (s, e) => ToggleGroup(clbSpecial);
            Controls.Add(btnToggleSpecial);

            // ===== START =====
            btnStart = new Button
            {
                Text = "Start Overlay",
                Location = new Point(10, 400),
                Size = new Size(200, 35)
            };
            btnStart.Click += BtnStart_Click;
            Controls.Add(btnStart);
            // ===== NOWE KONFIGURACJE OVERLAY =====
            Label lblTextColor = new Label { Text = "Text Color:", Location = new Point(10, 440), AutoSize = true };
            Controls.Add(lblTextColor);

            btnTextColor = new Button { Text = "Pick", Location = new Point(80, 435), Size = new Size(80, 25), BackColor = Color.Red };
            btnTextColor.Click += (s, e) => PickColor(c => btnTextColor.BackColor = c);
            Controls.Add(btnTextColor);

            Label lblArrowColor = new Label { Text = "Arrow Color:", Location = new Point(180, 440), AutoSize = true };
            Controls.Add(lblArrowColor);

            btnArrowColor = new Button { Text = "Pick", Location = new Point(260, 435), Size = new Size(80, 25), BackColor = Color.Yellow };
            btnArrowColor.Click += (s, e) => PickColor(c => btnArrowColor.BackColor = c);
            Controls.Add(btnArrowColor);

            Label lblOutlineColor = new Label { Text = "Outline Color:", Location = new Point(360, 440), AutoSize = true };
            Controls.Add(lblOutlineColor);

            btnOutlineColor = new Button { Text = "Pick", Location = new Point(450, 435), Size = new Size(60, 25), BackColor = Color.Black };
            btnOutlineColor.Click += (s, e) => PickColor(c => btnOutlineColor.BackColor = c);
            Controls.Add(btnOutlineColor);

            Label lblOutlineThickness = new Label { Text = "Outline Thickness:", Location = new Point(10, 470), AutoSize = true };
            Controls.Add(lblOutlineThickness);

            nudOutlineThickness = new NumericUpDown { Location = new Point(130, 465), Size = new Size(60, 25), Minimum = 0, Maximum = 5, Value = 1 };
            Controls.Add(nudOutlineThickness);

        }

        private CheckedListBox CreateCheckedListBox(IEnumerable<string> items)
        {
            var clb = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true
            };

            foreach (var item in items)
                clb.Items.Add(item, true);

            clb.ItemCheck += (s, e) =>
            {
                string key = clb.Items[e.Index].ToString();
                if (e.NewValue == CheckState.Checked)
                    allowedKeys.Add(key);
                else
                    allowedKeys.Remove(key);
            };

            return clb;
        }
        private void PickColor(Action<Color> callback)
        {
            using ColorDialog dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                callback(dlg.Color);
        }

        private void ToggleGroup(CheckedListBox clb)
        {
            bool enable = clb.Items.Cast<string>().Any(k => !allowedKeys.Contains(k));

            for (int i = 0; i < clb.Items.Count; i++)
            {
                clb.SetItemChecked(i, enable);
                string key = clb.Items[i].ToString();

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
        }

        private void RestoreCheckedState(CheckedListBox clb)
        {
            for (int i = 0; i < clb.Items.Count; i++)
            {
                string key = clb.Items[i].ToString();
                clb.SetItemChecked(i, allowedKeys.Contains(key));
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var selectedKeys = new HashSet<string>(allowedKeys);

            OverlayForm overlay = new OverlayForm(selectedKeys, chkTopMost.Checked)
            {
                TextColor = btnTextColor.BackColor,
                ArrowColor = btnArrowColor.BackColor,
                OutlineColor = btnOutlineColor.BackColor,
                OutlineThickness = (int)nudOutlineThickness.Value
            };
            overlay.Show();

            Properties.Settings.Default.AllowedKeys = string.Join(",", selectedKeys);
            Properties.Settings.Default.OverlayTopMost = chkTopMost.Checked;
            Properties.Settings.Default.Save();

            this.Hide();
        }
    }
}
