using System.ComponentModel;

namespace FileTransferClient.Winform
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null)
            {
                return value.ToString();
            }

            var attribute = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                   .FirstOrDefault() as DescriptionAttribute;

            return attribute?.Description ?? value.ToString();
        }
    }
}
