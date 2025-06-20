namespace FileTransferClient.Winform
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // º”‘ÿ≈‰÷√
            var config = ConfigurationHelper.GetConfig();

            Application.Run(new Form1(config));
        }
    }
}