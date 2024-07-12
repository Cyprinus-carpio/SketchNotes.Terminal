using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SketchNotes.Terminal.Pages;
using System;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SketchNotes.Terminal
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId id = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(id);

            // 调整窗口位置和大小，以屏幕像素为单位。
            appWindow.Resize(new SizeInt32(1000, 600));

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                TitleTextBlock.Text = "SketchNotes 终端 (管理员)";
            }
        }

        private void MainFrame_Loaded(object sender, RoutedEventArgs e)
        {
            string[] arguments = Environment.GetCommandLineArgs();

            if (arguments.Length > 1)
            {
                Match match = Regex.Match(arguments[1], @"\?cmd=(.+)");

                if (match.Success)
                {
                    string cmd = match.Groups[1].ToString().Replace("+", " ");
                    MainFrame.Navigate(typeof(RunPage), cmd);
                }
                else
                {
                    MainFrame.Navigate(typeof(StartPage));
                }
            }
            else
            {
                MainFrame.Navigate(typeof(StartPage));
            }
        }
    }
}
