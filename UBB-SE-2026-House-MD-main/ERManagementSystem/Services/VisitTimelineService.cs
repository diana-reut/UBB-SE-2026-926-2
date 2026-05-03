using System.Collections.Generic;
using ERManagementSystem.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;

namespace ERManagementSystem.Services
{
    /// <summary>
    /// Service for building visit timeline UI items.
    /// Separates business logic (timeline progression) from UI logic.
    /// </summary>
    public class VisitTimelineService
    {
        /// <summary>
        /// Builds a timeline of visit status progression for UI display.
        /// </summary>
        /// <param name="currentStatus">The current status of the visit</param>
        /// <returns>A list of timeline items ready for UI binding</returns>
        public static List<TimelineItem> BuildTimeline(string currentStatus)
        {
            var states = new[]
            {
                ER_Visit.VisitStatus.REGISTERED,
                ER_Visit.VisitStatus.TRIAGED,
                ER_Visit.VisitStatus.WAITING_FOR_ROOM,
                ER_Visit.VisitStatus.IN_ROOM,
                ER_Visit.VisitStatus.WAITING_FOR_DOCTOR,
                ER_Visit.VisitStatus.IN_EXAMINATION,
                ER_Visit.VisitStatus.TRANSFERRED,
                ER_Visit.VisitStatus.CLOSED
            };

            bool passedCurrent = false;
            var items = new List<TimelineItem>();

            foreach (var state in states)
            {
                bool isCurrent = state == currentStatus;
                bool isPast = !passedCurrent && !isCurrent;

                items.Add(new TimelineItem
                {
                    Label = state.Replace("_", " "),
                    DotColor = isCurrent ? new SolidColorBrush(Colors.DodgerBlue)
                                    : isPast ? new SolidColorBrush(Colors.Gray)
                                                : new SolidColorBrush(Colors.LightGray),
                    TextColor = isCurrent ? new SolidColorBrush(Colors.DodgerBlue)
                                    : isPast ? new SolidColorBrush(Colors.Gray)
                                                : new SolidColorBrush(Colors.LightGray),
                    Weight = isCurrent ? "Bold" : "Normal",
                    BadgeVisibility = isCurrent ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed
                });

                if (isCurrent)
                {
                    passedCurrent = true;
                }
            }

            return items;
        }
    }

    /// <summary>
    /// Timeline item for UI binding - represents a single step in the visit workflow.
    /// </summary>
    public class TimelineItem
    {
        public string Label { get; set; } = string.Empty;
        public SolidColorBrush DotColor { get; set; } = new SolidColorBrush(Colors.LightGray);
        public SolidColorBrush TextColor { get; set; } = new SolidColorBrush(Colors.LightGray);
        public string Weight { get; set; } = "Normal";
        public Microsoft.UI.Xaml.Visibility BadgeVisibility { get; set; } = Microsoft.UI.Xaml.Visibility.Collapsed;
    }
}
