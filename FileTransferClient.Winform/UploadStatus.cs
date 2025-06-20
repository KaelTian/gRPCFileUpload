using System.ComponentModel;

namespace FileTransferClient.Winform
{
    public enum UploadStatus
    {
        [Description("未知")]
        Unknown = -1,

        [Description("未开始")]
        NotStarted,

        [Description("上传中")]
        Uploading,

        [Description("已暂停")]
        Paused,

        [Description("上传完成")]
        Completed,

        [Description("上传异常")]
        Failed
    }
}
