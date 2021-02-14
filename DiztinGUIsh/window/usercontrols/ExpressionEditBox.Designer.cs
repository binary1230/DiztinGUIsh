namespace DiztinGUIsh.window.usercontrols
{
    partial class ExpressionEditBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblLabel = new System.Windows.Forms.Label();
            this.txtExpression = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblEvaluatesToBytes = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblLabel
            // 
            this.lblLabel.AutoSize = true;
            this.lblLabel.Location = new System.Drawing.Point(4, 4);
            this.lblLabel.Name = "lblLabel";
            this.lblLabel.Size = new System.Drawing.Size(58, 13);
            this.lblLabel.TabIndex = 0;
            this.lblLabel.Text = "Expression";
            // 
            // txtExpression
            // 
            this.txtExpression.Location = new System.Drawing.Point(7, 23);
            this.txtExpression.Name = "txtExpression";
            this.txtExpression.Size = new System.Drawing.Size(318, 20);
            this.txtExpression.TabIndex = 1;
            this.txtExpression.TextChanged += new System.EventHandler(this.txtExpression_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(369, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Evaluates To:";
            // 
            // lblEvaluatesToBytes
            // 
            this.lblEvaluatesToBytes.AutoSize = true;
            this.lblEvaluatesToBytes.Location = new System.Drawing.Point(369, 27);
            this.lblEvaluatesToBytes.Name = "lblEvaluatesToBytes";
            this.lblEvaluatesToBytes.Size = new System.Drawing.Size(64, 13);
            this.lblEvaluatesToBytes.TabIndex = 3;
            this.lblEvaluatesToBytes.Text = "00 00 00 00";
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(7, 50);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(28, 13);
            this.lblError.TabIndex = 4;
            this.lblError.Text = "error";
            // 
            // ExpressionEditBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblError);
            this.Controls.Add(this.lblEvaluatesToBytes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtExpression);
            this.Controls.Add(this.lblLabel);
            this.Name = "ExpressionEditBox";
            this.Size = new System.Drawing.Size(469, 78);
            this.Load += new System.EventHandler(this.ExpressionEditBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblLabel;
        private System.Windows.Forms.TextBox txtExpression;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblEvaluatesToBytes;
        private System.Windows.Forms.Label lblError;
    }
}
