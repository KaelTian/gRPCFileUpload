using FileTransferClient.Winform.Configs;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcService1.Protos;

namespace FileTransferClient.Winform
{
    public partial class Form1 : Form
    {
        private GrpcChannel? _channel;
        private FileTransfer.FileTransferClient? _fileClient;
        private HealthCheck.HealthCheckClient? _healthCheckClient;

        private CancellationTokenSource? _cancellationTokenSource;

        private AppConfig? _config;

        public Form1()
        {
            InitializeComponent();
            //_ = InitializeGrpcClients();
        }

        public Form1(AppConfig? config)
        {

            InitializeComponent();
            _config = config;
            _ = InitializeSettings();

            ConfigurationHelper.OnConfigurationChanged += () =>
            {
                // 注意：需要在 UI 线程上更新
                this.Invoke((MethodInvoker)async delegate
                {
                    _config = ConfigurationHelper.GetConfig();
                    await InitializeSettings();
                });
            };
        }

        private void UpdateUploadStatusLabel(UploadStatus status = UploadStatus.NotStarted)
        {
            lblUploadStatus.Text = $"上传状态: {status.GetDescription()}";
        }

        private async Task InitializeSettings()
        {
            UpdateUploadStatusLabel();
            // 初始化组件
            ApplyConfiguration();
            // 初始化gRPC客户端
            await InitializeGrpcClients();
        }

        private void ApplyConfiguration()
        {
            if (_config == null)
            {
                MessageBox.Show("配置未加载或无效.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 使用配置值
            this.Text = _config?.Application?.Title;
            this.BackColor = _config?.Application?.Theme == "Dark" ? Color.DarkGray : Color.White;
        }

        private async Task InitializeGrpcClients()
        {
            try
            {
                _fileClient = GrpcClientFactory.CreateClient<FileTransfer.FileTransferClient>("FileTransferClient");
                _healthCheckClient = GrpcClientFactory.CreateClient<HealthCheck.HealthCheckClient>("HealthCheckClient");

                // 测试连接
                await TestGrpcConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化gRPC客户端失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private async Task TestGrpcConnection()
        {
            try
            {
                var timeout = TimeSpan.FromSeconds(
                    ConfigurationHelper.GetGrpcServiceSettings("HealthCheckClient")!.TimeoutSeconds);

                using var cts = new CancellationTokenSource(timeout);
                await _healthCheckClient!.PingAsync(new PingRequest(), cancellationToken: cts.Token);

                statusLabel.Text = "gRPC连接正常";
                statusLabel.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                statusLabel.Text = "gRPC连接异常";
                statusLabel.ForeColor = Color.Red;
                MessageBox.Show("gRPC连接测试失败.", ex.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                //LogError($"gRPC连接测试失败: {ex.Message}");
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = false;
                openFileDialog.CheckFileExists = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    var fileInfo = new FileInfo(openFileDialog.FileName);
                    lblFileSize.Text = $"大小: {FormatFileSize(fileInfo.Length)}";

                    // 重置进度显示
                    progressBar.Value = 0;
                    lblProgress.Text = "";
                    lblPercentage.Text = "0%";
                    UpdateUploadStatusLabel();
                }
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("请选择一个有效文件.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var filePath = txtFilePath.Text;
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Length > 100L * 1024 * 1024 * 1024) // 100GB
            {
                MessageBox.Show("文件大小限制为100GB.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 禁用UI控件
            SetUploadControls(false);

            _cancellationTokenSource = new CancellationTokenSource();

            var uploadResult = UploadStatus.Uploading;
            UpdateUploadStatusLabel(uploadResult);
            try
            {
                uploadResult = await UploadFileWithResume(filePath, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("上传取消.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                uploadResult = UploadStatus.Failed;
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Unavailable)
            {
                MessageBox.Show("服务无效,请检查连接.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                uploadResult = UploadStatus.Failed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"上传失败: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                uploadResult = UploadStatus.Failed;
            }
            finally
            {
                if (uploadResult == UploadStatus.Completed)
                {
                    MessageBox.Show("文件上传成功!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (uploadResult == UploadStatus.Paused)
                {
                    MessageBox.Show("文件上传暂停!", "Pausing",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                }
                UpdateUploadStatusLabel(uploadResult);
                // 重新启用UI控件
                SetUploadControls(true);
            }
        }

        private void SetUploadControls(bool enable)
        {
            btnBrowse.Enabled = enable;
            btnUpload.Enabled = enable;
            btnPause.Enabled = !enable;
            btnCancel.Enabled = !enable;
        }

        private async Task<UploadStatus> UploadFileWithResume(string filePath, CancellationToken cancellationToken)
        {
            UploadStatus uploadResult = UploadStatus.Unknown;
            var fileInfo = new FileInfo(filePath);
            var fileHash = await FileHelper.GetQuickFileIdentity(filePath);
            const int chunkSize = 10 * 1024 * 1024; // 1MB chunk size
            // 检查服务器上是否已有上传记录
            var statusResponse = await _fileClient!.CheckUploadStatusAsync(new CheckUploadStatusRequest
            {
                FileName = fileInfo.Name,
                FileHash = fileHash
            });
            string sessionId;
            long uploadedBytes = 0;
            if (statusResponse.Exists && statusResponse.UploadedBytes > 0)
            {
                // 恢复已有上传
                sessionId = statusResponse.SessionId;
                uploadedBytes = statusResponse.UploadedBytes;
            }
            else
            {
                // 开始新上传
                var initResponse = await _fileClient.InitUploadAsync(new InitUploadRequest
                {
                    FileName = fileInfo.Name,
                    FileSize = fileInfo.Length,
                    FileHash = fileHash
                });
                sessionId = initResponse.SessionId;
                uploadedBytes = initResponse.UploadedBytes;
            }
            // 更新UI显示初始进度
            UpdateProgress(uploadedBytes, fileInfo.Length);
            // 打开文件准备读取
            using (var fileStream = File.OpenRead(filePath))
            {
                fileStream.Seek(uploadedBytes, SeekOrigin.Begin);

                var call = _fileClient.UploadChunk();
                var remainingBytes = fileInfo.Length - uploadedBytes;

                while (remainingBytes > 0 && !cancellationToken.IsCancellationRequested)
                {
                    var currentChunkSize = (int)Math.Min(chunkSize, remainingBytes);
                    var buffer = new byte[currentChunkSize];
                    var bytesRead = await fileStream.ReadAsync(buffer, 0, currentChunkSize, cancellationToken);

                    if (bytesRead == 0) break;

                    await call.RequestStream.WriteAsync(new UploadChunkRequest
                    {
                        SessionId = sessionId,
                        ChunkData = Google.Protobuf.ByteString.CopyFrom(buffer),
                        Offset = uploadedBytes
                    });

                    uploadedBytes += bytesRead;
                    remainingBytes -= bytesRead;

                    // 更新UI进度
                    UpdateProgress(uploadedBytes, fileInfo.Length);
                }

                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                // 完成上传
                var completeResponse = await _fileClient.CompleteUploadAsync(new CompleteUploadRequest
                {
                    SessionId = sessionId,
                    FileHash = fileHash
                });
                if (!completeResponse.Success)
                {
                    throw new Exception(completeResponse.Message);
                }
                uploadResult = UploadStatus.Completed;
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                uploadResult = UploadStatus.Paused;
            }
            return uploadResult;
        }

        private void UpdateProgress(long uploadedBytes, long totalBytes)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(uploadedBytes, totalBytes)));
                return;
            }

            progressBar.Value = (int)((double)uploadedBytes / totalBytes * 100);
            lblProgress.Text = $"{FormatFileSize(uploadedBytes)} / {FormatFileSize(totalBytes)}";
            lblPercentage.Text = $"{progressBar.Value}%";
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            btnPause.Enabled = false;
            btnUpload.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            btnCancel.Enabled = false;
            btnPause.Enabled = false;
            btnUpload.Enabled = true;

            // 重置进度显示
            progressBar.Value = 0;
            lblProgress.Text = "";
            lblPercentage.Text = "0%";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _channel?.ShutdownAsync().Wait();
        }

        private async void reconnectBtn_Click(object sender, EventArgs e)
        {
            await InitializeGrpcClients();
        }
    }
}
