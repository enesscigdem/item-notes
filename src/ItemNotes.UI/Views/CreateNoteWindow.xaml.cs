using System;
using Avalonia.Controls;
using ItemNotes.UI.ViewModels;

namespace ItemNotes.UI.Views
{
    /// <summary>Yeni not oluşturma penceresi. DataContext dışarıdan verilmeli.</summary>
    public partial class CreateNoteWindow : Window
    {
        public CreateNoteWindow()
        {
            InitializeComponent();

            // Eğer DataContext dışarıdan verilmediyse, kapanış kablosunu güvene al
            if (DataContext is CreateNoteWindowViewModel vm)
                WireClose(vm);
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is CreateNoteWindowViewModel vm)
                WireClose(vm);
        }

        private void WireClose(CreateNoteWindowViewModel vm)
        {
           
            vm.CloseRequested -= VmOnCloseRequested;
            vm.CloseRequested += VmOnCloseRequested;
        }

        private void VmOnCloseRequested(Guid? id)
        {
            Close(id);
        }
    }
}