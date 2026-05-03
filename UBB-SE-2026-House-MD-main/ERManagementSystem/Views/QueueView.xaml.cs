using ERManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class QueueView : Page
    {
        public QueueViewModel? ViewModel { get; private set; }

        public QueueView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is QueueViewModel vm)
            {
                ViewModel = vm;
            }
            else if (ViewModel == null)
            {
                ViewModel = App.Services.GetRequiredService<QueueViewModel>();
            }

            if (ViewModel == null)
            {
                return;
            }

            ViewModel.LoadQueueCommand.Execute(null);
            Bindings.Update();
        }
    }
}
