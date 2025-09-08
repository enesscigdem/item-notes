using Avalonia.Controls;
using Avalonia.Interactivity;
using ItemNotes.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ItemNotes.UI.Views
{
    /// <summary>
    /// Not listesini gösteren ana pencere. ViewModel'den gelen olayları yakalar ve ilgili pencereleri açar.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel viewModel)
        {
            _serviceProvider = serviceProvider;
            _viewModel = viewModel;
            InitializeComponent();
            DataContext = _viewModel;

            _viewModel.CreateNoteRequested += OnCreateNoteRequested;
            _viewModel.OpenNoteRequested += OnOpenNoteRequested;

            this.Opened += async (_, __) =>
            {
                await _viewModel.LoadNotesAsync();
            };
        }

        private async void OnCreateNoteRequested()
        {
            var createWindow = _serviceProvider.GetRequiredService<CreateNoteWindow>();

            // DataContext yoksa DI'dan al ve ata
            if (createWindow.DataContext is not CreateNoteWindowViewModel vm)
            {
                var vmFromDi = _serviceProvider.GetRequiredService<CreateNoteWindowViewModel>();
                createWindow.DataContext = vmFromDi;
            }

            // Pencere kapandığında sonucu almak için generic ShowDialog kullan
            var result = await createWindow.ShowDialog<Guid?>(this);
            if (result.HasValue)
            {
                await _viewModel.LoadNotesAsync();
                await OnOpenNoteAsync(result.Value);
            }
        }


        private async void OnOpenNoteRequested(Guid noteId)
        {
            await OnOpenNoteAsync(noteId);
        }

        private async Task OnOpenNoteAsync(Guid noteId)
        {
            var noteWindow = _serviceProvider.GetRequiredService<NoteWindow>();
            await noteWindow.ShowNoteAsync(noteId, this);
        }
        // menüdeki çıkış düğmesi için basit kapatma
        public void OnExit(object? sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}