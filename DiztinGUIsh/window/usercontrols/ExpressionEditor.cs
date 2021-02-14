using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Diz.Core.model;

namespace DiztinGUIsh.window.dialog
{
    public partial class ExpressionEditor : UserControl
    {
        public Data Data { get; set; }
        public int RomOffset { get; set; }

        public string Expression => exprEditable.Expression;

        public event EventHandler Saved;
        public event EventHandler Cancelled;

        // take an expression, return error msg or ""
        public Func<string, string> IsValidExpression
        {
            get => exprEditable.IsValidExpression;
            set => exprEditable.IsValidExpression = value;
        }

        public ExpressionEditor()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Saved?.Invoke(null, null);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cancelled?.Invoke(null, null);
        }

        private void exprEditable_ExprTextChanged(object sender, EventArgs e)
        {
            if (sender != exprEditable)
                return;

            UpdateUI();
        }

        private void UpdateUI()
        {
            btnSave.Enabled = exprEditable.ErrorMsg != "";
        }

        private void ExpressionEditor_Load(object sender, EventArgs e)
        {
            exprEditable.Expression = Data.GetInstruction(RomOffset);
            exprEditable.EditingEnabled = true;

            exprOriginal.EditingEnabled = false;

            UpdateUI();
        }

        private void exprEditable_Load(object sender, EventArgs e)
        {

        }
    }
}
