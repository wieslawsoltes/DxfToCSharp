using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using DxfToCSharp.Core;
using DxfToCSharp.ViewModels;
using ReactiveUI;

namespace DxfToCSharp;

public partial class DxfGeneratorOptionsControl : UserControl, IViewFor<DxfGeneratorOptionsViewModel>
{
    public static readonly StyledProperty<DxfGeneratorOptionsViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<DxfGeneratorOptionsControl, DxfGeneratorOptionsViewModel?>(nameof(ViewModel));

    public DxfGeneratorOptionsViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (DxfGeneratorOptionsViewModel?)value;
    }
    public event EventHandler<OptionsChangedEventArgs>? OptionsChanged;

    public DxfGeneratorOptionsControl()
    {
        InitializeComponent();
        ViewModel = new DxfGeneratorOptionsViewModel();
        DataContext = ViewModel;

        // Subscribe to view model changes to raise OptionsChanged event
        this.WhenAnyValue(x => x.ViewModel)
            .Where(vm => vm != null)
            .Subscribe(vm =>
            {
                vm!.OptionsChanged
                    .Subscribe(options => OptionsChanged?.Invoke(this, new OptionsChangedEventArgs(options)));
            });
    }

    public DxfCodeGenerationOptions GetOptionsFromUI()
    {
        return ViewModel?.ToOptions() ?? new DxfCodeGenerationOptions();
    }

    public void SetAllOptions(DxfCodeGenerationOptions options)
    {
        ViewModel?.FromOptions(options);
    }
}
