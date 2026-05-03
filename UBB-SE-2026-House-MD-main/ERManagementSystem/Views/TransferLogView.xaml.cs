using ERManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class TransferLogView : Page
    {
        public TransferLogViewModel? ViewModel { get; private set; }

        public TransferLogView()
        {
            InitializeComponent();
        }

        public TransferLogView(TransferLogViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is TransferLogViewModel transferLogViewModel)
            {
                ViewModel = transferLogViewModel;
            }

            if (ViewModel == null)
            {
                ViewModel = App.Services.GetRequiredService<TransferLogViewModel>();
            }

            this.Loaded += (s, args) =>
            {
                ViewModel.XamlRoot = this.XamlRoot;
            };
            ViewModel.LoadData();
        }
    }
}