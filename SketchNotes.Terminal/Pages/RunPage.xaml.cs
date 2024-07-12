using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SketchNotes.Terminal.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RunPage : Page
    {
        public RunPage()
        {
            InitializeComponent();
        }

        private string Command;
        private Process MainProcess = new Process();
        private int ProcessId;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null)
            {
                base.OnNavigatedTo(e);
                Command = (string)e.Parameter;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                AdminBtn.Visibility = Visibility.Collapsed;
            }

            ProgressBar progressBar = new ProgressBar
            {
                IsIndeterminate = true
            };

            ContentDialog taskDialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "���ڴ�������",
                CloseButtonText = "��������",
                Content = progressBar
            };

            taskDialog.CloseButtonClick += Dialog_CloseButtonClick;
            _ = taskDialog.ShowAsync();

            string result = await RunCommandAsync(Command);
            OutputTextBox.Text = result;
            ExitCodeTextBox.Text = "���� " + ProcessId + " �� " + MainProcess.ExitTime + " ���˳�������ֵΪ " + MainProcess.ExitCode;

            taskDialog.Hide();

            if (MainProcess.ExitCode == 1)
            {
                ContentDialog infoDialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "����ʧ��",
                    CloseButtonText = "ȷ��",
                    Content = "�ɳ����Թ���Ա�������ִ�С�"
                };

                await infoDialog.ShowAsync();
            }
        }

        private void Dialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            MainProcess.Kill(true);
            ExitCodeTextBox.Text = "���� " + ProcessId + " �� " + MainProcess.ExitTime + " ���˳�������ֵΪ " + MainProcess.ExitCode;
        }

        private Task<string> RunCommandAsync(string command)
        {
            return Task.Run(async () =>
            {
                StringBuilder output = new StringBuilder();

                MainProcess.StartInfo.FileName = "cmd.exe";
                MainProcess.StartInfo.Arguments = "/c" + command;
                MainProcess.StartInfo.RedirectStandardOutput = true;
                MainProcess.StartInfo.CreateNoWindow = true;
                MainProcess.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
                MainProcess.Start();
                ProcessId = MainProcess.Id;
                MainProcess.BeginOutputReadLine();
                await MainProcess.WaitForExitAsync();
                MainProcess.CancelOutputRead();

                return output.ToString();
            });
        }

        private async void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c start mascot-snterminal:?cmd=" + Command.Replace(" ", "+");
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.Verb = "runas";
                bool success = process.Start();

                App.Current.Exit();
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "����ʧ��",
                    CloseButtonText = "ȷ��",
                    Content = "�޷���ȡ������Ȩ�ޡ�\n\n" + ex
                };

                await dialog.ShowAsync();
            }
        }
    }
}
