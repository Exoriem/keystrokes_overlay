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
        private CheckedListBox keyList;
        private Button btnStart;

        // przyciski do toggle grup
        private Button btnToggleLetters;
        private Button btnToggleNumbers;
        private Button btnToggleSpecial;

        private HashSet<string> allowedKeys = new()
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M",
            "N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
            "0","1","2","3","4","5","6","7","8","9",
            "Lctrl","Rctrl","Lshift","Rshift","Lalt","Ralt",
            "Backspace","Enter","Esc","Tab","Space",
            "Insert","Delete","Home","End","PageUp","PageDown",
            "Up","Down","Left","Right",
            "F1","F2","F3","F4","F5","F6",
            "F7","F8","F9","F10","F11","F12",
            "LMB","RMB","MMB","XMB1","XMB2"
        };

        private HashSet<string> Letters = new()
        {
            "A","B","C","D","E","F","G","H","I","J","K","L","M",
            "N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
        };

        private HashSet<string> Numbers = new()
        {
            "0","1","2","3","4","5","6","7","8","9"
        };

        private HashSet<string> SpecialKeys = new()
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
        private void LoadSettings()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.AllowedKeys))
            {
                allowedKeys = Properties.Settings.Default.AllowedKeys
                    .Split(',')
                    .ToHashSet();

                for (int i = 0; i < keyList.Items.Count; i++)
                {
                    string key = keyList.Items[i].ToString();
                    if (key.StartsWith("---")) continue;
                    keyList.SetItemChecked(i, allowedKeys.Contains(key));
                }
            }

            chkTopMost.Checked = Properties.Settings.Default.OverlayTopMost;
        }

        private void InitUI()
        {
            // Checkbox nad/pod aplikacjami
            chkTopMost = new CheckBox
            {
                Text = "Overlay nad aplikacjami",
                Checked = true,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(128, 0, 0, 0),
                Location = new Point(10, 10),
                AutoSize = true
            };
            Controls.Add(chkTopMost);

            // Lista przycisków
            keyList = new CheckedListBox
            {
                Location = new Point(10, 40),
                Size = new Size(200, 300),
                CheckOnClick = true
            };

            // nagłówki i grupy
            keyList.Items.Add("--- Letters ---", false);
            foreach (var k in Letters) keyList.Items.Add(k, true);

            keyList.Items.Add("--- Numbers ---", false);
            foreach (var k in Numbers) keyList.Items.Add(k, true);

            keyList.Items.Add("--- Special Keys ---", false);
            foreach (var k in SpecialKeys) keyList.Items.Add(k, true);

            // event do aktualizacji allowedKeys
            keyList.ItemCheck += (s, e) =>
            {
                string key = keyList.Items[e.Index].ToString();
                if (key.StartsWith("---")) { e.NewValue = e.CurrentValue; return; } // nagłówek nie zmienia stanu
                if (e.NewValue == CheckState.Checked) allowedKeys.Add(key);
                else allowedKeys.Remove(key);
            };
            Controls.Add(keyList);

            // Przycisk Start
            btnStart = new Button
            {
                Text = "Start Overlay",
                Location = new Point(10, 350),
                Size = new Size(150, 30)
            };
            btnStart.Click += BtnStart_Click;
            Controls.Add(btnStart);

            // Przyciski do toggle grup
            btnToggleLetters = new Button { Text = "Toggle Letters", Location = new Point(220, 40), Size = new Size(120, 25) };
            btnToggleLetters.Click += (s, e) => ToggleGroup(Letters);
            Controls.Add(btnToggleLetters);

            btnToggleNumbers = new Button { Text = "Toggle Numbers", Location = new Point(220, 70), Size = new Size(120, 25) };
            btnToggleNumbers.Click += (s, e) => ToggleGroup(Numbers);
            Controls.Add(btnToggleNumbers);

            btnToggleSpecial = new Button { Text = "Toggle Special", Location = new Point(220, 100), Size = new Size(120, 25) };
            btnToggleSpecial.Click += (s, e) => ToggleGroup(SpecialKeys);
            Controls.Add(btnToggleSpecial);
        }

        private void ToggleGroup(HashSet<string> group)
        {
            bool enable = group.Any(k => !allowedKeys.Contains(k)); // jeśli choć jeden wyłączony, włącz całą grupę

            for (int i = 0; i < keyList.Items.Count; i++)
            {
                string key = keyList.Items[i].ToString();
                if (group.Contains(key))
                {
                    keyList.SetItemChecked(i, enable);
                    if (enable) allowedKeys.Add(key);
                    else allowedKeys.Remove(key);
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            // Zaktualizuj allowedKeys zgodnie z zaznaczeniem
            var selectedKeys = new HashSet<string>();
            foreach (var item in keyList.CheckedItems)
                selectedKeys.Add(item.ToString());

            bool topMost = chkTopMost.Checked;

            // Start overlay
            OverlayForm overlay = new OverlayForm(selectedKeys, topMost);
            overlay.Show();
            Properties.Settings.Default.AllowedKeys = string.Join(",", selectedKeys);
            Properties.Settings.Default.OverlayTopMost = chkTopMost.Checked;
            Properties.Settings.Default.Save();

            // Schowaj UI
            this.Hide();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
