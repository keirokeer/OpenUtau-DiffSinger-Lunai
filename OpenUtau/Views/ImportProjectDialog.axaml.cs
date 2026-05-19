using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OpenUtau.App.ViewModels;
using OpenUtau.Core.Format;

namespace OpenUtau.App.Views {
  public partial class ImportProjectDialog : Window {
    public ProjectImportOptions? Result { get; private set; }

    public ImportProjectDialog() {
      InitializeComponent();
    }

    public static async Task<ProjectImportOptions?> ShowAsync(Window owner, string fileName) {
      var vm = new ImportProjectDialogViewModel(fileName);
      var dialog = new ImportProjectDialog {
        DataContext = vm,
      };
      var tcs = new TaskCompletionSource<ProjectImportOptions?>();
      dialog.Closed += (_, _) => tcs.TrySetResult(dialog.Result);
      await dialog.ShowDialog(owner);
      return await tcs.Task;
    }

    void OnCancel(object? sender, RoutedEventArgs e) {
      Result = null;
      Close();
    }

    void OnApply(object? sender, RoutedEventArgs e) {
      if (DataContext is ImportProjectDialogViewModel vm) {
        Result = vm.ToOptions();
      }
      Close();
    }

    void OnKeyDown(object? sender, KeyEventArgs e) {
      if (e.Key == Key.Escape) {
        e.Handled = true;
        OnCancel(sender, e);
      } else if (e.Key == Key.Enter) {
        e.Handled = true;
        OnApply(sender, e);
      }
    }
  }
}
