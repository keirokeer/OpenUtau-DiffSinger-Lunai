using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenUtau.App.ViewModels;
using ReactiveUI;

namespace OpenUtau.App.Views {
    public partial class ThemeEditorWindow : Window {

        private static ThemeEditorWindow? _instance;

        public static bool IsOpen => _instance != null;

        private ThemeEditorWindow(string customThemePath) {
            InitializeComponent();
            EditorPane.LoadTheme(customThemePath);
            EditorPane.Finished += (_, _) => Close();
        }

        void WindowClosing(object? sender, WindowClosingEventArgs e) {
            _instance = null;
            MessageBus.Current.SendMessage(new ThemeEditorStateChangedEvent());
            App.SetTheme();
        }

        public static void CloseIfOpen() {
            _instance?.Close();
        }

        public static void Show(string customThemePath) {
            MessageBus.Current.SendMessage(new CloseDockedThemeEditorEvent());
            if (_instance == null) {
                _instance = new ThemeEditorWindow(customThemePath);
                _instance.Show();
                MessageBus.Current.SendMessage(new ThemeEditorStateChangedEvent());
            } else {
                _instance.EditorPane.LoadTheme(customThemePath);
                _instance.Activate();
            }
        }

    }
}
