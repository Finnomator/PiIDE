using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace PiIDE;

public partial class StatisticsWindow {

    private long TotalRenders;
    private long TotalCompletions;
    private const int LogLength = 7;
    private readonly Queue<RenderStat> RenderStats = new(LogLength);
    private readonly Queue<CompletionStat> CompletionStats = new(LogLength);

    public StatisticsWindow() {
        InitializeComponent();
        Topmost = true;

        Application.Current.MainWindow!.Closed += delegate {
            Close();
        };
    }

    public void AddRenderStat(long renderTimeMs) {
        string timeStamp = DateTime.Now.GetTimestamp();

        if (RenderStats.Count == LogLength)
            RenderStats.Dequeue();

        RenderStats.Enqueue(new() {
            RenderTimeMs = renderTimeMs,
            Number = ++TotalRenders,
            TimeStamp = timeStamp
        });

        LastRendersListView.ItemsSource = null;
        LastRendersListView.ItemsSource = RenderStats;
    }

    public void AddCompletionStat(long completionTimeMs) {
        string timeStamp = DateTime.Now.GetTimestamp();

        if (CompletionStats.Count == LogLength)
            CompletionStats.Dequeue();

        CompletionStats.Enqueue(new() {
            CompletionTimeMs = completionTimeMs,
            Number = ++TotalCompletions,
            TimeStamp = timeStamp
        });

        LastCompletionsListView.ItemsSource = null;
        LastCompletionsListView.ItemsSource = CompletionStats;
    }

    private void Window_Closed(object sender, EventArgs e) {
        Tools.StatsWindow = null;
        Tools.UpdateStats = false;
    }

#pragma warning disable IDE0079
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
#pragma warning restore IDE0079
    private readonly struct RenderStat {
        public required long Number { get; init; }
        public required long RenderTimeMs { get; init; }
        public required string TimeStamp { get; init; }
    }

#pragma warning disable IDE0079
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
#pragma warning restore IDE0079
    private readonly struct CompletionStat {
        public required long Number { get; init; }
        public required long CompletionTimeMs { get; init; }
        public required string TimeStamp { get; init; }
    }
}