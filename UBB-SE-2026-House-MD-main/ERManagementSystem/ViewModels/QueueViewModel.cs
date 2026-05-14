using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERManagementSystem.Models;
using ERManagementSystem.Proxy.ERVisitProxy;
using ERManagementSystem.Proxy.TriageProxy;

namespace ERManagementSystem.ViewModels
{
    public partial class QueueViewModel : BaseViewModel
    {
        private readonly IERVisitProxy erVisitProxy;
        private readonly ITriageProxy triageProxy;

        public QueueViewModel(IERVisitProxy erVisitProxy, ITriageProxy triageProxy)
        {
            this.erVisitProxy = erVisitProxy;
            this.triageProxy = triageProxy;
        }

        // ── Observable collection for DataGrid ──────────────────────────────
        [ObservableProperty]
        private ObservableCollection<QueueItemDisplay> activeVisits = new ObservableCollection<QueueItemDisplay>();

        [RelayCommand]
        private async Task LoadQueue()
        {
            var waitingVisits = await erVisitProxy.GetByStatusAsync(ER_Visit.VisitStatus.WAITING_FOR_ROOM);
            var triages = await triageProxy.GetAllAsync();
            var queue = waitingVisits
                .Join(
                    triages,
                    visit => visit.Visit_ID,
                    triage => triage.Visit_ID,
                    (visit, triage) => (visit, triage))
                .OrderBy(queueEntry => queueEntry.triage.Triage_Level)
                .ThenBy(queueEntry => queueEntry.visit.Arrival_date_time);

            ObservableCollection<QueueItemDisplay> refreshedQueue = new ObservableCollection<QueueItemDisplay>();

            foreach (var (visit, triage) in queue)
            {
                refreshedQueue.Add(new QueueItemDisplay(visit, triage));
            }

            ActiveVisits = refreshedQueue;
        }

        [RelayCommand]
        private Task RefreshQueue()
        {
            return LoadQueue();
        }
    }
}
