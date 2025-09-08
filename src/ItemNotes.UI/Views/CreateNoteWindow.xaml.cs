using System;
using Avalonia.Controls;
using ItemNotes.UI.ViewModels;

namespace ItemNotes.UI.Views
{
    /// <summary>Yeni not oluşturma penceresi. DataContext dışarıdan verilmeli.</summary>
    public partial class CreateNoteWindow : Window
    {
        private CreateNoteWindowViewModel? _attachedVm;

        public CreateNoteWindow()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);

            // Eski VM varsa bağlantıyı kes
            if (_attachedVm is not null)
                _attachedVm.CloseRequested -= VmOnCloseRequested;

            // Yeni VM varsa bağlan
            _attachedVm = DataContext as CreateNoteWindowViewModel;
            if (_attachedVm is not null)
                _attachedVm.CloseRequested += VmOnCloseRequested;
        }

        private void VmOnCloseRequested(Guid? id)
        {
            Close(id);
        }
    }
}