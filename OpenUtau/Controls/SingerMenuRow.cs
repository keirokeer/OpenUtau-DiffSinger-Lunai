using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using OpenUtau.App;
using OpenUtau.App.ViewModels;

namespace OpenUtau.App.Controls {
    static class SingerMenuRow {
        public static Grid Create(SingerMenuItemViewModel viewModel) {
            var grid = new Grid {
                ColumnDefinitions = new ColumnDefinitions("16,*,22"),
                MinHeight = 24,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var heart = new FavouriteToggleButton {
                Classes = { "singerMenuFavourite" },
                DataContext = viewModel,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            heart.Bind(FavouriteToggleButton.IsCheckedProperty, new Binding("IsFavourite"));
            ToolTip.SetTip(heart, ThemeManager.GetString("tracks.singer.favorite.tooltip"));
            Grid.SetColumn(heart, 0);

            var name = new TextBlock {
                Text = viewModel.Header as string ?? string.Empty,
                Margin = new Thickness(4, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
            };
            if (!string.IsNullOrEmpty(viewModel.Location)) {
                ToolTip.SetTip(name, viewModel.Location);
            }
            Grid.SetColumn(name, 1);

            var apply = new ApplyToAllTracksButton {
                Classes = { "singerMenuApplyAll" },
                Command = viewModel.SecondaryCommand,
                CommandParameter = viewModel.CommandParameter,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Focusable = false,
            };
            ToolTip.SetTip(apply, ThemeManager.GetString("tracks.singer.applytoalltracks.tooltip"));
            Grid.SetColumn(apply, 2);

            grid.Children.Add(heart);
            grid.Children.Add(name);
            grid.Children.Add(apply);
            return grid;
        }
    }
}
