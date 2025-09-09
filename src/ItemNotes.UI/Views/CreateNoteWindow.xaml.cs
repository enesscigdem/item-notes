using System;
using Avalonia.Controls;
using ItemNotes.UI.ViewModels;

namespace ItemNotes.UI.Views;

public partial class CreateNoteWindow : Window
{
    private CreateNoteWindowViewModel? _vm;

    public CreateNoteWindow(CreateNoteWindowViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        _vm = vm;

        // VM pencereyi kapatmak istediğinde sonucu (Guid?) döndürür
        _vm.CloseRequested += id => Close(id);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_vm is not null) _vm.CloseRequested -= OnCloseRequested;
        _vm = DataContext as CreateNoteWindowViewModel;
        if (_vm is not null) _vm.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested(Guid? id) => Close(id);
}