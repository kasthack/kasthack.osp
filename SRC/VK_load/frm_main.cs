﻿/*
*************************************************************************************
Tis file is distributed under MIT license:
*************************************************************************************
The MIT License (MIT)

Copyright © 2013-2014 kasthack/EpicMorg

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ''Software''), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ''AS IS'', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.", @"The MIT License (MIT)
*************************************************************************************
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using kasthack.vksharp;
using kasthack.vksharp.DataTypes.Enums;

namespace VK_load {
    public partial class FrmMain : Form {
        private Core _api;
        private bool _running;
        private bool _cancel;
        private LoadOptions options;
        public FrmMain() {
            InitializeComponent();
            lstFields.DataSource = ((UserFields[])Enum.GetValues(typeof(UserFields))).Where(a => a != UserFields.Anything && a != UserFields.Everything).OrderBy( a=>a.ToString() ).ToArray();
            CheckForIllegalCrossThreadCalls = false;
            //start, end, (int) nud_threads.Value, txt_outpath.Text, GetFields(), volume, chkCompressOutput.Checked, UpdateProfilesCount, UpdateTraffic, () => _cancel );
            options = new LoadOptions() { ShowCount = UpdateProfilesCount, ShowTraffic = UpdateTraffic, CancellationToken = () => _cancel };
            this.nud_threads.DataBindings.Add( nameof( nud_threads.Value ), options, nameof( options.Threads ), false, DataSourceUpdateMode.OnPropertyChanged );
            this.nud_start.DataBindings.Add(nameof(nud_start.Value), options, nameof(options.Start), false, DataSourceUpdateMode.OnPropertyChanged);
            this.nud_end.DataBindings.Add(nameof(nud_end.Value), options, nameof(options.End), false, DataSourceUpdateMode.OnPropertyChanged);
            this.nud_volume.DataBindings.Add(nameof(nud_volume.Value), options, nameof(options.VolumeSize), false, DataSourceUpdateMode.OnPropertyChanged);
            this.txt_outpath.DataBindings.Add( nameof( txt_outpath.Text ), options, nameof( options.Path ), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkGZip.DataBindings.Add( nameof( chkGZip.Checked ), options, nameof( options.GZip ), false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkExecute.DataBindings.Add(nameof(chkExecute.Checked), options, nameof(options.Execute), false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void OpenOAuth(object sender, EventArgs e) => Process.Start(Token.GetOAuthURL( Core.AppID, Permission.Offline ));

        private void CompleteAuth(object sender, EventArgs e) {
            _api = new Core(txt_token.Text);
            if ( !_api.IsLogged )
                MessageBox.Show( @"Bad url! Please try again.", @"Warning!", MessageBoxButtons.OK, MessageBoxIcon.Stop );
            grp_conf.Enabled = grp_control.Enabled = grp_fileds.Enabled = _api.IsLogged;
        }
        
        private void Browse(object sender, EventArgs e) {
            using (var dialog = new FolderBrowserDialog())
                if (dialog.ShowDialog() == DialogResult.OK)
                    txt_outpath.Text = dialog.SelectedPath;
        }

        private async void Run(object sender, EventArgs e) {
            if ( _running ) {
                _cancel = true;
                btn_run.Enabled = false;
                return;
            }

            _running = true;
            _cancel = false;
            UpdateInterfaceIsRunnig();
            this.options.Fields = GetFields();
            if ( options.End - options.Start + 1 < options.VolumeSize ) MessageBox.Show( "Volume must be less than end-start", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation );
            else await _api.LoadUsers( options ).ConfigureAwait( true );
            _running = false;
            btn_run.Enabled = true;
            UpdateInterfaceIsRunnig();
            MessageBox.Show( @"Download complete!", @"Win!", MessageBoxButtons.OK, MessageBoxIcon.Information );
            progressbar.Value = 0;
        }

        private UserFields GetFields() => lstFields.CheckedItems.Cast<UserFields>().Aggregate( UserFields.None, ( current, fields ) => current | fields );

        private void UpdateProfilesCount(long a) {
            var total = nud_end.Value - nud_start.Value;
            lbl_profiles_loaded.Text = a.ToString(CultureInfo.InvariantCulture);
            progressbar.Value = Math.Min((int)(a * 100 / total), 100);
        }

        private void UpdateTraffic(long a) {
            var postfix = new[] { 'B', 'K', 'M', 'G', 'T' };
            var index = 0;
            while (a > 1024) {
                index++;
                a >>= 10;
            }
            lbl_traf.Text = $"{a.ToString( CultureInfo.InvariantCulture )} {postfix[ index ]}";

        }

        private void UpdateInterfaceIsRunnig() {
            if (_running) {
                btn_run.Text = "Step 5. STOP IT!";
                lbl_state.Text = "Running";
                lbl_state.ForeColor = Color.DarkGreen;
                grp_conf.Enabled = grp_fileds.Enabled = grp_auth.Enabled = false;
            }
            else {
                btn_run.Text = "Step 5. RUT IT!";
                lbl_state.Text = "Stand by";
                lbl_state.ForeColor = Color.DarkGoldenrod;
                grp_conf.Enabled = grp_fileds.Enabled = grp_auth.Enabled = true;
            }

        }

        private void CheckAllFields(object sender, EventArgs e) => CheckAll(true);
        private void UncheckAllFields(object sender, EventArgs e) => CheckAll(false);
        private void CheckAll(bool value) { for ( var i = 0; i < lstFields.Items.Count; i++ ) lstFields.SetItemChecked( i, value ); }

        private void AboutBox(object sender, EventArgs e) {
            MessageBox.Show(@"Created by KASTHACK" + Environment.NewLine +
                             "GUI Fixes by STAM" + Environment.NewLine +
                             "License: The MIT License (MIT)" + Environment.NewLine +
                             "_______________________________________________________" + Environment.NewLine + 
                             "https://github.com/kasthack/OslikSuslikPaukan", @"Info:", MessageBoxButtons.OK, MessageBoxIcon.Information); 
            MessageBox.Show(@"The MIT License (MIT)" + Environment.NewLine + Environment.NewLine +
                            @"Copyright © 2013-2014 EpicMorg" + Environment.NewLine + Environment.NewLine +
                            @"Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ''Software'')," +
                            "to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or" +
                            "sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:)" + Environment.NewLine + Environment.NewLine +
                            @"The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.)" + Environment.NewLine + Environment.NewLine +
                            @"THE SOFTWARE IS PROVIDED ''AS IS'', WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY," +
                            "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER" +
                            "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS" +
                            "IN THE SOFTWARE.", @"The MIT License (MIT)", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        } 
        private void ResetToken(object sender, LinkLabelLinkClickedEventArgs e) => txt_token.Text = "";

        private void checkBox1_CheckedChanged(object sender, EventArgs e) => nud_volume.Maximum = chkExecute.Checked ? 25000 : 1000;

        //private void OnLoad(object sender, EventArgs e) => 
    }
}
