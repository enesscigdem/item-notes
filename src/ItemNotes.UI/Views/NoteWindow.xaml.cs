using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ItemNotes.Domain.Enums;
using ItemNotes.UI.Converters;
using ItemNotes.UI.Controls;
using ItemNotes.UI.ViewModels;

namespace ItemNotes.UI.Views
{
    public partial class NoteWindow : Window
    {
        private readonly NoteWindowViewModel _viewModel;

        public NoteWindow(NoteWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            DataContext = _viewModel;

            this.Closing += async (_, __) =>
            {
                try
                {
                    await SyncEditorsToViewModel();
                    await _viewModel.SaveAllPagesAsync();
                }
                catch { /* ignore */ }
            };
        }

        public async Task ShowNoteAsync(Guid noteId, Window owner)
        {
            await _viewModel.LoadNoteAsync(noteId);
            await ShowDialog(owner);
        }

        public void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

        public async void ChangeColorYellow(object? s, RoutedEventArgs e) => await ChangeColor(NoteColor.Yellow);
        public async void ChangeColorGreen(object? s, RoutedEventArgs e)  => await ChangeColor(NoteColor.Green);
        public async void ChangeColorRed(object? s, RoutedEventArgs e)    => await ChangeColor(NoteColor.Red);

        private async Task ChangeColor(NoteColor newColor)
        {
            if (_viewModel.Note != null && _viewModel.Note.Color != newColor)
            {
                _viewModel.Note.Color = newColor;
                await _viewModel.NoteService.UpdateNoteAsync(_viewModel.Note);

                var converter = new NoteColorToBrushConverter();
                if (this.FindControl<Border>("RootBorder") is { } border)
                {
                    border.Background = converter.Convert(newColor, typeof(IBrush), null, CultureInfo.InvariantCulture) as IBrush;
                }
            }
        }

        private async void OnSaveClick(object? sender, RoutedEventArgs e)
        {
            await SyncEditorsToViewModel();
            await _viewModel.SaveAllPagesAsync();
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            try { BeginMoveDrag(e); } catch { /* ignore */ }
        }

        /// <summary>
        /// Mevcut CKEditor içeriklerini ViewModel'e yazar.
        /// </summary>
        private async Task SyncEditorsToViewModel()
        {
            var tabs = this.FindControl<TabControl>("PagesTabControl");
            if (tabs?.Items is null) return;

            var items = tabs.Items.Cast<object>().ToList();
            for (int i = 0; i < items.Count; i++)
            {
                var container = tabs.ItemContainerGenerator.ContainerFromIndex(i) as TabItem;
                var editor = (container?.Content as Control)?.FindControl<CkEditorView>("Editor");
                if (editor != null && items[i] is PageViewModel vm)
                {
                    vm.Html = await editor.GetHtmlAsync();
                }
            }
        }
    }
}
