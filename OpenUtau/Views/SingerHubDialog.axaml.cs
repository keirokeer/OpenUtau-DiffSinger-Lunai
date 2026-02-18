using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenUtau.App.ViewModels;
using OpenUtau;
using OpenUtau.Core;

namespace OpenUtau.App.Views {
    public partial class SingerHubDialog : Window {
        public SingerHubDialog() {
            InitializeComponent();
            // Share the pane's ViewModel so DataContext is available when dialog is used as a window.
            if (Content is SingerHubPane pane) {
                DataContext = pane.DataContext;
            }
        }

        async void OnPrimaryActionClick(object sender, RoutedEventArgs e) {
            try {
                if (DataContext is SingerHubViewModel vm && sender is Button b && b.DataContext is SingerHubRowViewModel row && row.CanInstall) {
                    var msg = string.Format(ThemeManager.GetString("lunai.confirm.install.message"), row.Name);
                    if (!string.IsNullOrEmpty(row.TosUrl)) {
                        msg += "\n" + ThemeManager.GetString("lunai.confirm.install.tos") + " " + row.TosUrl;
                    }
                    var caption = ThemeManager.GetString("lunai.confirm.install.caption");
                    var result = await MessageBox.Show(this, msg, caption, MessageBox.MessageBoxButtons.YesNo);
                    if (result != MessageBox.MessageBoxResult.Yes) return;
                    await vm.InstallOrUpdateAsync(row);
                }
            } catch (Exception ex) {
                DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(ex));
            }
        }

        void OnOpenPageClick(object? sender, RoutedEventArgs e) {
            var dc = (sender as Avalonia.Controls.Control)?.DataContext as SingerHubRowViewModel;
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

