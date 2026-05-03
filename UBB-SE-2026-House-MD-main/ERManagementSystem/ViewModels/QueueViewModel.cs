using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Services;

namespace ERManagementSystem.ViewModels
{
    public partial class QueueViewModel : BaseViewModel
    {
        private readonly IQueueService queueService;

        public QueueViewModel(IQueueService queueService)
        {
            this.queueService = queueService;
        }

        // ── Observable collection for DataGrid ──────────────────────────────
        public ObservableCollection<QueueItemDisplay> ActiveVisits { get; } = new ObservableCollection<QueueItemDisplay>();

        [RelayCommand]
        private void LoadQueue()
        {
            ActiveVisits.Clear();
            var queue = queueService.GetOrderedQueue();
            foreach (var (visit, triage) in queue)
            {
                ActiveVisits.Add(new QueueItemDisplay(visit, triage));
            }
        }

        [RelayCommand]
        private void RefreshQueue()
        {
            LoadQueue();
        }
    }
}
