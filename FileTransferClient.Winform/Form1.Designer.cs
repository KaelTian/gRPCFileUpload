namespace FileTransferClient.Winform
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            txtFilePath = new TextBox();
            btnBrowse = new Button();
            btnUpload = new Button();
            progressBar = new ProgressBar();
            lblProgress = new Label();
            lblFileSize = new Label();
            btnPause = new Button();
            btnCancel = new Button();
            lblPercentage = new Label();
            grpFileInfo = new GroupBox();
            lblUploadStatus = new Label();
            grpProgress = new GroupBox();
            groupBox1 = new GroupBox();
            reconnectBtn = new Button();
            statusLabel = new Label();
            grpFileInfo.SuspendLayout();
            grpProgress.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.Location = new Point(8, 29);
            txtFilePath.Margin = new Padding(4);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.ReadOnly = true;
            txtFilePath.Size = new Size(625, 27);
            txtFilePath.TabIndex = 0;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(641, 29);
            btnBrowse.Margin = new Padding(4);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(96, 31);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "浏览";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnUpload
            // 
            btnUpload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUpload.Location = new Point(641, 68);
            btnUpload.Margin = new Padding(4);
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(96, 31);
            btnUpload.TabIndex = 2;
            btnUpload.Text = "上传";
            btnUpload.UseVisualStyleBackColor = true;
            btnUpload.Click += btnUpload_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(8, 29);
            progressBar.Margin = new Padding(4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(626, 31);
            progressBar.TabIndex = 3;
            // 
            // lblProgress
            // 
            lblProgress.AutoSize = true;
            lblProgress.Location = new Point(8, 64);
            lblProgress.Margin = new Padding(4, 0, 4, 0);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(0, 20);
            lblProgress.TabIndex = 4;
            // 
            // lblFileSize
            // 
            lblFileSize.AutoSize = true;
            lblFileSize.Location = new Point(8, 73);
            lblFileSize.Margin = new Padding(4, 0, 4, 0);
            lblFileSize.Name = "lblFileSize";
            lblFileSize.Size = new Size(53, 20);
            lblFileSize.TabIndex = 5;
            lblFileSize.Text = "大小: -";
            // 
            // btnPause
            // 
            btnPause.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPause.Enabled = false;
            btnPause.Location = new Point(641, 107);
            btnPause.Margin = new Padding(4);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(96, 31);
            btnPause.TabIndex = 6;
            btnPause.Text = "暂停";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(641, 145);
            btnCancel.Margin = new Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(96, 31);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblPercentage
            // 
            lblPercentage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblPercentage.Location = new Point(641, 29);
            lblPercentage.Margin = new Padding(4, 0, 4, 0);
            lblPercentage.Name = "lblPercentage";
            lblPercentage.Size = new Size(96, 31);
            lblPercentage.TabIndex = 8;
            lblPercentage.Text = "0%";
            lblPercentage.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // grpFileInfo
            // 
            grpFileInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpFileInfo.Controls.Add(lblUploadStatus);
            grpFileInfo.Controls.Add(txtFilePath);
            grpFileInfo.Controls.Add(btnBrowse);
            grpFileInfo.Controls.Add(btnUpload);
            grpFileInfo.Controls.Add(btnCancel);
            grpFileInfo.Controls.Add(lblFileSize);
            grpFileInfo.Controls.Add(btnPause);
            grpFileInfo.Location = new Point(15, 16);
            grpFileInfo.Margin = new Padding(4);
            grpFileInfo.Name = "grpFileInfo";
            grpFileInfo.Padding = new Padding(4);
            grpFileInfo.Size = new Size(745, 187);
            grpFileInfo.TabIndex = 9;
            grpFileInfo.TabStop = false;
            grpFileInfo.Text = "文件信息";
            // 
            // lblUploadStatus
            // 
            lblUploadStatus.AutoSize = true;
            lblUploadStatus.Location = new Point(8, 150);
            lblUploadStatus.Margin = new Padding(4, 0, 4, 0);
            lblUploadStatus.Name = "lblUploadStatus";
            lblUploadStatus.Size = new Size(47, 20);
            lblUploadStatus.TabIndex = 8;
            lblUploadStatus.Text = "状态: ";
            // 
            // grpProgress
            // 
            grpProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpProgress.Controls.Add(progressBar);
            grpProgress.Controls.Add(lblPercentage);
            grpProgress.Controls.Add(lblProgress);
            grpProgress.Location = new Point(15, 211);
            grpProgress.Margin = new Padding(4);
            grpProgress.Name = "grpProgress";
            grpProgress.Padding = new Padding(4);
            grpProgress.Size = new Size(745, 107);
            grpProgress.TabIndex = 10;
            grpProgress.TabStop = false;
            grpProgress.Text = "上传进度";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(reconnectBtn);
            groupBox1.Controls.Add(statusLabel);
            groupBox1.Location = new Point(15, 335);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(745, 84);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "服务端连接状态";
            // 
            // reconnectBtn
            // 
            reconnectBtn.Location = new Point(644, 34);
            reconnectBtn.Name = "reconnectBtn";
            reconnectBtn.Size = new Size(94, 29);
            reconnectBtn.TabIndex = 1;
            reconnectBtn.Text = "重新连接";
            reconnectBtn.UseVisualStyleBackColor = true;
            reconnectBtn.Click += reconnectBtn_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 134);
            statusLabel.Location = new Point(8, 37);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(72, 27);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "未连接";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(776, 451);
            Controls.Add(groupBox1);
            Controls.Add(grpProgress);
            Controls.Add(grpFileInfo);
            Margin = new Padding(4);
            Name = "Form1";
            Text = "文件上传";
            FormClosing += MainForm_FormClosing;
            grpFileInfo.ResumeLayout(false);
            grpFileInfo.PerformLayout();
            grpProgress.ResumeLayout(false);
            grpProgress.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblPercentage;
        private System.Windows.Forms.GroupBox grpFileInfo;
        private System.Windows.Forms.GroupBox grpProgress;
        private GroupBox groupBox1;
        private Label statusLabel;
        private Button reconnectBtn;
        private Label lblUploadStatus;
    }
}
