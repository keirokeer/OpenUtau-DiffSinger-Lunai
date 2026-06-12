using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Avalonia.Media;
using OpenUtau.Colors;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtau.App.ViewModels;

public class ThemeColorFieldViewModel : ReactiveObject {
    public string Key { get; }

    [Reactive] public Color Color { get; set; }
    public string HexCode => ThemeColorStorage.ToStorageString(Color);

    public ThemeColorFieldViewModel(string key, Color color) {
        Key = key;
        Color = color;
        this.WhenAnyValue(viewModel => viewModel.Color)
            .Subscribe(new Action<Color>(_ => this.RaisePropertyChanged(nameof(HexCode))));
    }
}

public class ThemeColorSectionViewModel {
    public string Title { get; }
    public IList<ThemeColorFieldViewModel> Fields { get; }

    public ThemeColorSectionViewModel(string title, IList<ThemeColorFieldViewModel> fields) {
        Title = title;
        Fields = fields;
    }
}
