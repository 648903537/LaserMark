namespace LaserMark.com.amtec.forms
{
    partial class Reprint
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnComfirmPrint = new System.Windows.Forms.Button();
            this.dgvRePrintSN = new System.Windows.Forms.DataGridView();
            this.RPID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RPSerialNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRePrintSN)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.dgvRePrintSN, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.471366F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.52863F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(284, 499);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnComfirmPrint);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 457);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(278, 39);
            this.panel1.TabIndex = 58;
            // 
            // btnComfirmPrint
            // 
            this.btnComfirmPrint.Location = new System.Drawing.Point(93, 5);
            this.btnComfirmPrint.Name = "btnComfirmPrint";
            this.btnComfirmPrint.Size = new System.Drawing.Size(75, 29);
            this.btnComfirmPrint.TabIndex = 0;
            this.btnComfirmPrint.Text = "OK";
            this.btnComfirmPrint.UseVisualStyleBackColor = true;
            this.btnComfirmPrint.Click += new System.EventHandler(this.btnComfirmPrint_Click);
            // 
            // dgvRePrintSN
            // 
            this.dgvRePrintSN.AllowUserToAddRows = false;
            this.dgvRePrintSN.AllowUserToDeleteRows = false;
            this.dgvRePrintSN.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRePrintSN.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RPID,
            this.RPSerialNumber});
            this.dgvRePrintSN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRePrintSN.Location = new System.Drawing.Point(3, 46);
            this.dgvRePrintSN.Name = "dgvRePrintSN";
            this.dgvRePrintSN.ReadOnly = true;
            this.dgvRePrintSN.RowHeadersVisible = false;
            this.dgvRePrintSN.RowTemplate.Height = 23;
            this.dgvRePrintSN.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRePrintSN.Size = new System.Drawing.Size(278, 405);
            this.dgvRePrintSN.TabIndex = 60;
            // 
            // RPID
            // 
            this.RPID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RPID.DataPropertyName = "NextMaintenance";
            this.RPID.HeaderText = "ID";
            this.RPID.Name = "RPID";
            this.RPID.ReadOnly = true;
            // 
            // RPSerialNumber
            // 
            this.RPSerialNumber.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.RPSerialNumber.HeaderText = "SerialNumber";
            this.RPSerialNumber.Name = "RPSerialNumber";
            this.RPSerialNumber.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.textBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(278, 37);
            this.panel2.TabIndex = 61;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(42, 9);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(179, 21);
            this.textBox1.TabIndex = 0;
            this.textBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyUp);
            // 
            // Reprint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 499);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Reprint";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "重新打印";
            this.Load += new System.EventHandler(this.Reprint_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRePrintSN)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dgvRePrintSN;
        private System.Windows.Forms.DataGridViewTextBoxColumn RPID;
        private System.Windows.Forms.DataGridViewTextBoxColumn RPSerialNumber;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnComfirmPrint;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TextBox textBox1;

    }
}