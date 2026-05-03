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
            this.InitializeComponent();
            this.Loaded += ExaminationView_Loaded;
        }

        /// <summary>
        /// XamlRoot is only available after the page is in the visual tree.
        /// Setting it here guarantees ContentDialogs can be shown.
        /// </summary>
        private void ExaminationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.XamlRoot = this.XamlRoot;
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
                ViewModel = App.Services.GetRequiredService<ExaminationViewModel>();
            }

            // Re-evaluate all x:Bind bindings now that ViewModel is set.
            // x:Bind defaults to OneTime mode and evaluates during InitializeComponent()
            // when ViewModel is still null — this call fixes the command bindings.
            Bindings.Update();

            ViewModel.LoadData();
        }
    }
}