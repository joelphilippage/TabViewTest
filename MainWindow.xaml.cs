using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TabViewTest
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public Microsoft.UI.Windowing.AppWindowTitleBar titleBar;
        public MainWindow()
        {
            this.InitializeComponent();

            if (Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            {
                var m_AppWindow = GetAppWindowForCurrentWindow();
                titleBar = m_AppWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                AppTitleBar.Loaded += AppTitleBar_Loaded;
                //AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                CustomDragRegion.SizeChanged += CustomDragRegion_SizeChanged;
            }
        }

        private void CustomDragRegion_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDragRegion();
        }

        private void UpdateDragRegion()
        {
            var ttv = CustomDragRegion.TransformToVisual(AppTitleBar);
            var point = ttv.TransformPoint(new Point(0, 0));
            var x = (int)(point.X * GetScaleAdjustment());
            var width = (int)(CustomDragRegion.ActualWidth * GetScaleAdjustment());
            var height = (int)(CustomDragRegion.ActualHeight * GetScaleAdjustment());

            titleBar.SetDragRectangles(
            [
                new RectInt32(x, 0, width, height)
            ]);
            Debug.WriteLine("Updated drag region");
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            SetTitleBar(AppTitleBar);
        }

        private void TabView_AddTabButtonClick(TabView sender, object args)
        {
            MainTabView.TabItems.Add(new TabViewItem() { Header = "New Tab" });
        }

        private Microsoft.UI.Windowing.AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
        }

        private void MainTabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            MainTabView.TabItems.Remove(args.Tab);
        }

        [LibraryImport("Shcore.dll", SetLastError = true)]
        internal static partial int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        private double GetScaleAdjustment()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }
    }
}
