using System;
using ERManagementSystem.Infrastructure;
using ERManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class ExaminationView : Page
    {
        public ExaminationViewModel? ViewModel { get; private set; }

        public ExaminationView()
        {
            InitializeComponent();
            Loaded += ExaminationView_Loaded;
        }

        private void ExaminationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.XamlRoot = XamlRoot;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ExaminationViewModel examinationViewModel)
            {
                ViewModel = examinationViewModel;
            }

            if (ViewModel == null)
            {
                ViewModel = ServiceRegistry.Services.GetRequiredService<ExaminationViewModel>();
            }

            Bindings.Update();
            _ = ViewModel.LoadData();
            UpdateGridHeights();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGridHeights();
        }

        private void UpdateGridHeights()
        {
            VisitsDataGrid.Height = Math.Clamp(ActualHeight * 0.26, 180, 420);
            HistoryDataGrid.Height = Math.Clamp(ActualHeight * 0.18, 120, 280);
        }
    }
}
