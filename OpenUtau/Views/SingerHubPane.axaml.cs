using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;

namespace OpenUtau.App.Views {
    public partial class SingerHubPane : UserControl {
        public SingerHubPane() {
            InitializeComponent();
        }

        static Window? GetOwnerWindow(Visual? visual) {
            if (visual?.GetVisualRoot() is Window w) {
                return w;
            }
            var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            return lifetime?.MainWindow;
        }

        async void OnPrimaryActionClick(object sender, RoutedEventArgs e) {
            try {
                if (DataContext is SingerHubViewModel vm &&
                    sender is Button b &&
                    b.DataContext is SingerHubRowViewModel row &&
                    row.CanInstall) {
                    var owner = GetOwnerWindow(this);
                    if (owner == null) {
                        DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(new InvalidOperationException("No owner window for Singer Hub dialog.")));
                        return;
                    }
                    var msg = string.Format(ThemeManager.GetString("lunai.confirm.install.message"), row.Name);
                    if (!string.IsNullOrEmpty(row.TosUrl)) {
                        msg += "\n" + ThemeManager.GetString("lunai.confirm.install.tos") + " " + row.TosUrl;
                    }
                    var caption = ThemeManager.GetString("lunai.confirm.install.caption");
                    var result = await MessageBox.Show(owner, msg, caption, MessageBox.MessageBoxButtons.YesNo);
                    if (result != MessageBox.MessageBoxResult.Yes) return;
                    await vm.InstallOrUpdateAsync(row);
                }
            } catch (Exception ex) {
                DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(ex));
            }
        }

        void OnOpenPageClick(object? sender, RoutedEventArgs e) {
            var dc = (sender as Control)?.DataContext as SingerHubRowViewModel;
            if (dc != null && !string.IsNullOrEmpty(dc.PageUrl)) {
                try {
                    if (!Uri.TryCreate(dc.PageUrl, UriKind.Absolute, out var uri)) {
                        return;
                    }
                    // Only allow http/https links to be opened.
                    if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
                        return;
                    }
                    OS.OpenWeb(uri.ToString());
                } catch (Exception ex) {
                    DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(ex));
                }
            }
        }
    }
}

