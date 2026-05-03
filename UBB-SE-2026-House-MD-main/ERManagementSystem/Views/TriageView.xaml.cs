using System.ComponentModel;
using ERManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ERManagementSystem.Views
{
    public sealed partial class TriageView : Page
    {
        public TriageViewModel? ViewModel { get; private set; }

        public TriageView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is TriageViewModel vm)
            {
                ViewModel = vm;
            }
            else if (ViewModel == null)
            {
                ViewModel = App.Services.GetRequiredService<TriageViewModel>();
            }

            if (ViewModel == null)
            {
                return;
            }

            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.LoadVisitsForTriageCommand.Execute(null);
            Bindings.Update();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // When IsTriaged resets to false (via CancelTriage), clear all ComboBoxes
            if (e.PropertyName == nameof(ViewModel.Consciousness) && ViewModel?.Consciousness == 0)
            {
                ConsciousnessCombo.SelectedItem = null;
            }

            if (e.PropertyName == nameof(ViewModel.Breathing) && ViewModel?.Breathing == 0)
            {
                BreathingCombo.SelectedItem = null;
            }

            if (e.PropertyName == nameof(ViewModel.Bleeding) && ViewModel?.Bleeding == 0)
            {
                BleedingCombo.SelectedItem = null;
            }

            if (e.PropertyName == nameof(ViewModel.InjuryType) && ViewModel?.InjuryType == 0)
            {
                InjuryTypeCombo.SelectedItem = null;
            }

            if (e.PropertyName == nameof(ViewModel.PainLevel) && ViewModel?.PainLevel == 0)
            {
                PainLevelCombo.SelectedItem = null;
            }
        }

        // ── ComboBox SelectionChanged handlers ──────────────────────────
        // These read the Tag from the selected ComboBoxItem and set the ViewModel property.
        private void ConsciousnessCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
            {
                ViewModel.Consciousness = int.Parse(item.Tag?.ToString() ?? "0");
            }
        }

        private void BreathingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
            {
                ViewModel.Breathing = int.Parse(item.Tag?.ToString() ?? "0");
            }
        }

        private void BleedingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
            {
                ViewModel.Bleeding = int.Parse(item.Tag?.ToString() ?? "0");
            }
        }

        private void InjuryTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
            {
                ViewModel.InjuryType = int.Parse(item.Tag?.ToString() ?? "0");
            }
        }

        private void PainLevelCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            if (sender is ComboBox box && box.SelectedItem is ComboBoxItem item)
            {
                ViewModel.PainLevel = int.Parse(item.Tag?.ToString() ?? "0");
            }
        }
    }
}
