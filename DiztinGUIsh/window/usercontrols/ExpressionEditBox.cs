using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiztinGUIsh.window.usercontrols
{
    public partial class ExpressionEditBox : UserControl
    {
        private string expression;
        private bool updatingText;

        public string Expression
        {
            get => expression;
            set
            {
                UpdateFromExpression(value);
                expression = value;
            }
        }

        public string Label
        {
            get => lblLabel.Text;
            set => lblLabel.Text = value;
        }

        public bool EditingEnabled
        {
            get => txtExpression.Enabled;
            set => txtExpression.Enabled = value;
        }

        public event EventHandler ExprTextChanged;

        // take an expression, return error msg or ""
        public Func<string, string> IsValidExpression;
        public string ErrorMsg = "";

        private bool UpdateFromExpression(string expr)
        {
            ErrorMsg = "";
            if (IsValidExpression != null)
                ErrorMsg = IsValidExpression(expr);

            lblError.Text = ErrorMsg;
            Expression = expr;

            return ErrorMsg == "";
        }

        public ExpressionEditBox()
        {
            InitializeComponent();
        }

        private void txtExpression_TextChanged(object sender, EventArgs e)
        {
            if (updatingText) 
                return;

            updatingText = true;

            Expression = txtExpression.Text;
            ExprTextChanged?.Invoke(sender, e);

            updatingText = false;
        }

        private void ExpressionEditBox_Load(object sender, EventArgs e)
        {

        }
    }
}
