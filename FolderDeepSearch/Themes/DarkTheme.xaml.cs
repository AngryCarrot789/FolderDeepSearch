using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace TheRThemes
{
    public partial class DarkTheme
    {
        private void CloseWindow_Event(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
                try { CloseWind(Window.GetWindow((FrameworkElement)e.Source)); } catch { }
        }
        private void AutoMinimize_Event(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
                try { MaximizeRestore(Window.GetWindow((FrameworkElement)e.Source)); } catch { }
        }
        private void Minimize_Event(object sender, RoutedEventArgs e)
        {
            if (e.Source != null)
                try { MinimizeWind(Window.GetWindow((FrameworkElement)e.Source)); } catch { }
        }

        public void CloseWind(Window window) => window.Close();
        public void MaximizeRestore(Window window)
        {
            if (window.WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Normal;
            else if (window.WindowState == WindowState.Normal)
                window.WindowState = WindowState.Maximized;
        }
        public void MinimizeWind(Window window) => window.WindowState = WindowState.Minimized;

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Grid g && g.TemplatedParent is Window win)
            {

            }
                //WindowDropShadows.DropShadowToWindow(win);
        }
    }
}
