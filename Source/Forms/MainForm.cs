﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AutomaticReplayViewer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Check if config file exists and make a default one if not
            if (!File.Exists("AutomaticReplayViewer.exe.config"))
            {
                GenerateConfigFile();
                ConfigurationManager.RefreshSection("appSettings");
            }

            // Initialise keys
            SetKeys();

            // Initialise ReplayViewer objects
            viewSG = new SGReplayViewer();
            viewROA = new ROAReplayViewer();
            viewBBTag = new BBTagReplayViewer();

            // Initialise event handlers
            viewSG.PropertyChanged += ProgressText_PropertyChanged;
            viewSG.LoopEnded += ResetUI;
            viewROA.PropertyChanged += ProgressText_PropertyChanged;
            viewROA.LoopEnded += ResetUI;
            viewBBTag.PropertyChanged += ProgressText_PropertyChanged;
            viewBBTag.LoopEnded += ResetUI;

            // Set initial values of forms from config file
            numReplays.Text = ConfigurationManager.AppSettings["DefaultNumberOfReplays"];
            InputRecordHotkey.Text = ConfigurationManager.AppSettings["DefaultRecordHotkey"];
            InputStopHotkey.Text = ConfigurationManager.AppSettings["DefaultStopHotkey"];
            DisplayHitboxes.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["DefaultDisplayHitboxes"]);
            DisplayInputs.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["DefaultDisplayInputs"]);
            DisplayAttackData.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["DefaultDisplayAttackData"]);
            BBTagHideGauge.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["BBTag Hide Gauge"]);
            BBTagHideWindow.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["BBTag Hide Window"]);
            switch (ConfigurationManager.AppSettings["DefaultGame"])
            {
                default:
                    skullgirlsToolStripMenuItem_Click(skullgirlsToolStripMenuItem, new EventArgs());
                    break;
                case "Rivals of Aether":
                    rivalsOfAetherToolStripMenuItem_Click(rivalsOfAetherToolStripMenuItem, new EventArgs());
                    break;
                case "BBTag":
                    BBTagToolStripMenuItem_Click(bBTagToolStripMenuItem, new EventArgs());
                    break;
            }

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is TapToSetTextBox)
                {
                    boxes.Add(ctrl);
                }
                else if (ctrl is Panel)
                {
                    foreach (Control _ctrl in ctrl.Controls)
                    {
                        if (_ctrl is TapToSetTextBox)
                        {
                            boxes.Add(_ctrl);
                        }
                    }
                }
            }

            foreach (TapToSetTextBox box in boxes)
            {
                box.WaitingForKey += Box_WaitingForKey;
            }
        }

        private void Box_WaitingForKey(object sender, EventArgs e)
        {
            SuppressKeys = true;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // Parse and store the number of replays to be played
            string inputText = numReplays.Text;
            int ReplaysToPlay = 1;

            if (!string.IsNullOrEmpty(inputText))
            {
                int.TryParse(inputText, out ReplaysToPlay);
                if (ReplaysToPlay < 1)
                    ReplaysToPlay = 1;
            }

            // Parse and store the hotkeys for recording/stopping
            RecordHotkey = ParseKeys(InputRecordHotkey.Text);
            StopHotkey = ParseKeys(InputStopHotkey.Text);

            // Toggle the button states
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            numReplays.Text = ReplaysToPlay.ToString();
            numReplays.Enabled = false;
            InputRecordHotkey.Enabled = false;
            InputStopHotkey.Enabled = false;
            DisplayHitboxes.Enabled = false;
            DisplayInputs.Enabled = false;
            DisplayAttackData.Enabled = false;
            menuStrip.Enabled = false;

            // Start main loop
            switch (currentGame)
            {
                default:
                    viewSG.StartLoop(ReplaysToPlay, SGLP, SGLK, SGMP, SGRight, RecordHotkey, StopHotkey, DisplayHitboxes.Checked, DisplayInputs.Checked, DisplayAttackData.Checked);
                    break;
                case "Rivals of Aether":
                    viewROA.StartLoop(ReplaysToPlay, ROAStart, ROAL, RecordHotkey, StopHotkey);
                    break;
                case "BBTag":
                    viewBBTag.StartLoop(ReplaysToPlay, BBTagUp, BBTagConfirm, BBTagGauge, BBTagWindow, RecordHotkey, StopHotkey, BBTagHideGauge.Checked, BBTagHideWindow.Checked);
                    break;
            }

            StopButton.Focus();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            viewSG.ProcessRunning = false;
            viewROA.ProcessRunning = false;
            viewBBTag.ProcessRunning = false;
        }

        private void ResetUI(object sender, EventArgs e)
         {
            this.BeginInvoke((Action)delegate () { this.Enabled = true; this.Focus(); });
            StartButton.BeginInvoke((Action)delegate () { StartButton.Enabled = true; StartButton.Focus(); });
            StopButton.BeginInvoke((Action)delegate () { StopButton.Enabled = false; });
            numReplays.BeginInvoke((Action)delegate () { numReplays.Enabled = true; });
            InputRecordHotkey.BeginInvoke((Action)delegate () { InputRecordHotkey.Enabled = true; });
            InputStopHotkey.BeginInvoke((Action)delegate () { InputStopHotkey.Enabled = true; });
            DisplayHitboxes.BeginInvoke((Action)delegate () { DisplayHitboxes.Enabled = true; });
            DisplayInputs.BeginInvoke((Action)delegate () { DisplayInputs.Enabled = true; });
            DisplayAttackData.BeginInvoke((Action)delegate () { DisplayAttackData.Enabled = true; });
            menuStrip.BeginInvoke((Action)delegate () { menuStrip.Enabled = true; });
        }

        private void ProgressText_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (currentGame)
            {
                default:
                    labelText = viewSG.ProgressText;
                    break;
                case "Rivals of Aether":
                    labelText = viewROA.ProgressText;
                    break;
                case "BBTag":
                    labelText = viewBBTag.ProgressText;
                    break;
            }
        }

        public static Keys ParseKeys(string input)
        {
            Keys output = Keys.None;

            if (!string.IsNullOrEmpty(input))
                Enum.TryParse(input, out output);

            return output;
        }

        private static void GenerateConfigFile()
        {
            // Update for new config file w BBTag stuff
            System.Text.StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.AppendLine("<configuration>");
            sb.AppendLine("  <appSettings>");
            sb.AppendLine("    <!--Relevant Key Bindings in SG-->");
            sb.AppendLine("    <!--It is essential that the keys here match the bindings that you have in SG-->");
            sb.AppendLine("    <!--All key settings must be as they are for System.Windows.Forms.Keys-->");
            sb.AppendLine("    <!--This list can be checked here msdn.microsoft.com/en-us/library/system.windows.forms.keys.aspx-->");
            sb.AppendLine("    <add key=\"SG LP keyboard input\" value=\"A\" />");
            sb.AppendLine("    <add key=\"SG LK keyboard input\" value=\"Z\" />");
            sb.AppendLine("    <add key=\"SG MP keyboard input\" value=\"S\" />");
            sb.AppendLine("    <add key=\"SG Right keyboard input\" value=\"Right\" />");
            sb.AppendLine("    <add key=\"DefaultDisplayHitboxes\" value=\"False\" />");
            sb.AppendLine("    <add key=\"DefaultDisplayInputs\" value=\"False\" />");
            sb.AppendLine("    <add key=\"DefaultDisplayAttackData\" value=\"False\" />");
            sb.AppendLine("    <!--SG Pointers-->");
            sb.AppendLine("    <add key=\"SGMenuState\" value=\"3909632\" />");
            sb.AppendLine("    <!--Relevant Key Bindings in ROA-->");
            sb.AppendLine("    <add key=\"ROA Start keyboard input\" value=\"Return\" />");
            sb.AppendLine("    <add key=\"ROA L keyboard input\" value=\"A\" />");
            sb.AppendLine("    <!--ROA Pointers-->");
            sb.AppendLine("    <add key=\"ROAMenuState\" value=\"0x228A130\" />");
            sb.AppendLine("    <add key=\"ROACursorX\" value=\"0x01FCEDC8,0x154,0xC,0xA0\" />");
            sb.AppendLine("    <add key=\"ROACursorY\" value=\"0x01FCEDC8,0x154,0xC,0xA4\" />");
            sb.AppendLine("    <!--Relevant Key Bindings in BBTag-->");
            sb.AppendLine("    <add key=\"BBTag Up keyboard input\" value=\"W\" />");
            sb.AppendLine("    <add key=\"BBTag Confirm keyboard input\" value=\"J\" />");
            sb.AppendLine("    <add key=\"BBTag Gauge keyboard input\" value=\"U\" />");
            sb.AppendLine("    <add key=\"BBTag Window keyboard input\" value=\"I\" />");
            sb.AppendLine("    <add key=\"BBTag Hide Gauge\" value=\"False\" />");
            sb.AppendLine("    <add key=\"BBTag Hide Window\" value=\"True\" />");
            sb.AppendLine("    <!--BBTag Pointers-->");
            sb.AppendLine("    <add key=\"BBTagReplayTheaterActive\" value=\"0x1643DF8\" />");
            sb.AppendLine("    <add key=\"BBTagReplayPlaying\" value=\"0x5C9CD8\" />");
            sb.AppendLine("    <add key=\"BBTagCursor\" value=\"0xFF7D90\" />");
            sb.AppendLine("    <!--Default Settings on Load-->");
            sb.AppendLine("    <add key=\"DefaultNumberOfReplays\" value=\"1\" />");
            sb.AppendLine("    <add key=\"DefaultRecordHotkey\" value=\"\" />");
            sb.AppendLine("    <add key=\"DefaultStopHotkey\" value=\"\" />");
            sb.AppendLine("    <add key=\"DefaultGame\" value=\"Skullgirls\" />");
            sb.AppendLine("    <add key=\"LastTimePointersUpdated\" value=\"14/10/18\" />");
            sb.AppendLine("  </appSettings>");
            sb.AppendLine("</configuration>");

            string loc = Assembly.GetEntryAssembly().Location;
            System.IO.File.WriteAllText(String.Concat(loc, ".config"), sb.ToString());
        }

        private void SettingsClosed(object sender, EventArgs e)
        {
            SetKeys();
            ResetUI(sender, e);
        }

        private void SetKeys()
        {
            // Initialise keys
            SGLP = ParseKeys(ConfigurationManager.AppSettings["SG LP keyboard input"]);
            SGLK = ParseKeys(ConfigurationManager.AppSettings["SG LK keyboard input"]);
            SGMP = ParseKeys(ConfigurationManager.AppSettings["SG MP keyboard input"]);
            SGRight = ParseKeys(ConfigurationManager.AppSettings["SG Right keyboard input"]);
            ROAStart = ParseKeys(ConfigurationManager.AppSettings["ROA Start keyboard input"]);
            ROAL = ParseKeys(ConfigurationManager.AppSettings["ROA L keyboard input"]);
            BBTagUp = ParseKeys(ConfigurationManager.AppSettings["BBTag Up keyboard input"]);
            BBTagConfirm = ParseKeys(ConfigurationManager.AppSettings["BBTag Confirm keyboard input"]);
            BBTagGauge = ParseKeys(ConfigurationManager.AppSettings["BBTag Gauge keyboard input"]);
            BBTagWindow = ParseKeys(ConfigurationManager.AppSettings["BBTag Window keyboard input"]);
        }

        private SGReplayViewer viewSG;
        private ROAReplayViewer viewROA;
        private BBTagReplayViewer viewBBTag;
        private Keys SGLP = Keys.A;
        private Keys SGLK = Keys.Z;
        private Keys SGMP = Keys.S;
        private Keys SGRight = Keys.Right;
        private Keys ROAStart = Keys.Return;
        private Keys ROAL = Keys.A;
        private Keys BBTagUp = Keys.W;
        private Keys BBTagConfirm = Keys.J;
        private Keys BBTagGauge = Keys.U;
        private Keys BBTagWindow = Keys.I;
        private string currentGame = "Skullgirls";

        private void skullgirlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentGame = viewSG.game;
            skullgirlsToolStripMenuItem.Checked = true;
            rivalsOfAetherToolStripMenuItem.Checked = false;
            bBTagToolStripMenuItem.Checked = false;
            SGSettings.Visible = true;
            BBTagSettings.Visible = false;
        }

        private void rivalsOfAetherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentGame = viewROA.game;
            skullgirlsToolStripMenuItem.Checked = false;
            rivalsOfAetherToolStripMenuItem.Checked = true;
            bBTagToolStripMenuItem.Checked = false;
            SGSettings.Visible = false;
            BBTagSettings.Visible = false;
        }

        private void BBTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentGame = viewBBTag.game;
            skullgirlsToolStripMenuItem.Checked = false;
            rivalsOfAetherToolStripMenuItem.Checked = false;
            bBTagToolStripMenuItem.Checked = true;
            SGSettings.Visible = false;
            BBTagSettings.Visible = true;
        }

        private void moreOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            this.Enabled = false;
            settings.FormClosed += SettingsClosed;
            settings.Show();
        }

        private void readmeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", "README.txt");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            this.Enabled = false;
            about.FormClosed += ResetUI;
            about.Show();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (SuppressKeys)
            {
                foreach (TapToSetTextBox box in boxes)
                {
                    if (box.Focused == true)
                    {
                        box.SetKey(this, new KeyEventArgs(keyData));
                        break;
                    }
                }
                SuppressKeys = false;
                return true;
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private bool SuppressKeys = false;
        private List<Control> boxes = new List<Control>();
    }
}