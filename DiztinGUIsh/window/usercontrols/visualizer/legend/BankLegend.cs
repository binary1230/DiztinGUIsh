﻿using System.Drawing;
using System.Windows.Forms;
using Diz.Core.model;
using Diz.Core.util;

// shows all the legend items in a collection

namespace DiztinGUIsh.window.usercontrols.visualizer.legend
{
    public partial class BankLegend : UserControl
    {
        public BankLegend()
        {
            InitializeComponent();
        }

        private void AddControl(string name, Color color)
        {
            flowLayoutPanel1.Controls.Add(
                new BankLegendItem(name, color)
            );
        }

        private void BankLegend_Load(object sender, System.EventArgs e)
        {
            var enums = Util.GetEnumColorDescriptions<FlagType>();
            foreach (var en in enums)
            {
                AddControl(en.Key.ToString(), Util.GetColorFromFlag(en.Key));
            }
        }
    }
}
