using System;
using ERManagementSystem.Infrastructure;
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
            InitializeComponent();
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
                ViewModel = ServiceRegistry.Services.GetRequiredService<QueueViewModel>();
            }

            if (ViewModel == null)
            {
                return;
            }

            ViewModel.LoadQueueCommand.Execute(null);
            Bindings.Update();
            UpdateQueueGridHeight();
        }

        private void Page_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            UpdateQueueGridHeight();
        }

        private void UpdateQueueGridHeight()
        {
            double availableHeight = RootGrid.ActualHeight
                - QueueTitleText.ActualHeight
                - QueueActionsPanel.ActualHeight
                - 56;

            QueueDataGrid.Height = Math.Max(240, availableHeight);
        }
    }
}
