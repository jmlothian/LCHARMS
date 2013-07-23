namespace LTestApp
{
    partial class Form1
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnTestDocService = new System.Windows.Forms.Button();
            this.btnCreateDoc = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(314, 60);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnTestDocService
            // 
            this.btnTestDocService.Location = new System.Drawing.Point(49, 206);
            this.btnTestDocService.Name = "btnTestDocService";
            this.btnTestDocService.Size = new System.Drawing.Size(239, 23);
            this.btnTestDocService.TabIndex = 1;
            this.btnTestDocService.Text = "Test Channel Binding to Doc Service Host";
            this.btnTestDocService.UseVisualStyleBackColor = true;
            this.btnTestDocService.Click += new System.EventHandler(this.btnTestDocService_Click);
            // 
            // btnCreateDoc
            // 
            this.btnCreateDoc.Location = new System.Drawing.Point(49, 141);
            this.btnCreateDoc.Name = "btnCreateDoc";
            this.btnCreateDoc.Size = new System.Drawing.Size(112, 23);
            this.btnCreateDoc.TabIndex = 2;
            this.btnCreateDoc.Text = "Create New Doc";
            this.btnCreateDoc.UseVisualStyleBackColor = true;
            this.btnCreateDoc.Click += new System.EventHandler(this.btnCreateDoc_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 260);
            this.Controls.Add(this.btnCreateDoc);
            this.Controls.Add(this.btnTestDocService);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnTestDocService;
        private System.Windows.Forms.Button btnCreateDoc;
    }
}

