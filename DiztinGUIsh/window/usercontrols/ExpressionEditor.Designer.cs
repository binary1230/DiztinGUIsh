namespace DiztinGUIsh.window.dialog
{
    partial class ExpressionEditor
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.exprOriginal = new DiztinGUIsh.window.usercontrols.ExpressionEditBox();
            this.exprEditable = new DiztinGUIsh.window.usercontrols.ExpressionEditBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Expression Editor";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 229);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(196, 39);
            this.label2.TabIndex = 1;
            this.label2.Text = "NOTE: Expression must evaluate to the \r\nsame bytes as the original ROM.  \r\nIf the" +
    "y don\'t match, you can\'t save.";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(15, 271);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(96, 271);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // exprOriginal
            // 
            this.exprOriginal.EditingEnabled = true;
            this.exprOriginal.Expression = null;
            this.exprOriginal.Label = "Expression";
            this.exprOriginal.Location = new System.Drawing.Point(6, 139);
            this.exprOriginal.Name = "exprOriginal";
            this.exprOriginal.Size = new System.Drawing.Size(469, 78);
            this.exprOriginal.TabIndex = 3;
            // 
            // exprEditable
            // 
            this.exprEditable.EditingEnabled = true;
            this.exprEditable.Expression = null;
            this.exprEditable.Label = "Expression";
            this.exprEditable.Location = new System.Drawing.Point(6, 38);
            this.exprEditable.Name = "exprEditable";
            this.exprEditable.Size = new System.Drawing.Size(469, 78);
            this.exprEditable.TabIndex = 2;
            this.exprEditable.ExprTextChanged += new System.EventHandler(this.exprEditable_ExprTextChanged);
            this.exprEditable.Load += new System.EventHandler(this.exprEditable_Load);
            // 
            // ExpressionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.exprOriginal);
            this.Controls.Add(this.exprEditable);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "ExpressionEditor";
            this.Size = new System.Drawing.Size(475, 299);
            this.Load += new System.EventHandler(this.ExpressionEditor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private usercontrols.ExpressionEditBox exprEditable;
        private usercontrols.ExpressionEditBox exprOriginal;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
