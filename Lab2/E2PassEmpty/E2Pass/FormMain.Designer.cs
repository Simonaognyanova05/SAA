namespace E2Pass
{
    partial class FormMain
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.radioButtonButton = new System.Windows.Forms.RadioButton();
            this.radioButtonTextBox = new System.Windows.Forms.RadioButton();
            this.buttonRun = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new System.Drawing.Point(103, 19);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(43, 42);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            // 
            // radioButtonButton
            // 
            this.radioButtonButton.AutoSize = true;
            this.radioButtonButton.ForeColor = System.Drawing.Color.Red;
            this.radioButtonButton.Location = new System.Drawing.Point(12, 37);
            this.radioButtonButton.Name = "radioButtonButton";
            this.radioButtonButton.Size = new System.Drawing.Size(56, 17);
            this.radioButtonButton.TabIndex = 1;
            this.radioButtonButton.Text = "Button";
            this.radioButtonButton.UseVisualStyleBackColor = true;
            // 
            // radioButtonTextBox
            // 
            this.radioButtonTextBox.AutoSize = true;
            this.radioButtonTextBox.Checked = true;
            this.radioButtonTextBox.ForeColor = System.Drawing.Color.Red;
            this.radioButtonTextBox.Location = new System.Drawing.Point(12, 12);
            this.radioButtonTextBox.Name = "radioButtonTextBox";
            this.radioButtonTextBox.Size = new System.Drawing.Size(64, 17);
            this.radioButtonTextBox.TabIndex = 2;
            this.radioButtonTextBox.TabStop = true;
            this.radioButtonTextBox.Text = "TextBox";
            this.radioButtonTextBox.UseVisualStyleBackColor = true;
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(185, 12);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 42);
            this.buttonRun.TabIndex = 3;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Visible = false;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(272, 73);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.radioButtonTextBox);
            this.Controls.Add(this.radioButtonButton);
            this.Controls.Add(this.pictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FormCaptue";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.RadioButton radioButtonButton;
        private System.Windows.Forms.RadioButton radioButtonTextBox;
        private System.Windows.Forms.Button buttonRun;
    }
}

