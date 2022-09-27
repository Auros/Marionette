using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Marionette.Services;
using Marionette.Views;
using System;

namespace Marionette.ViewModels;

[View(typeof(StatusView))]
public partial class StatusViewModel : ObservableObject, IDisposable
{
    private readonly LocalStateService _localStateService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalServiceStatusText))]
    [NotifyPropertyChangedFor(nameof(LocalServiceStatusColor))]
    private bool _localServiceStatus;

    public string LocalServiceStatusColor => LocalServiceStatus ? "Green" : "Red";
    public string LocalServiceStatusText => LocalServiceStatus ? "Online" : "Offline";

    public StatusViewModel(LocalStateService localStateService)
    {
        _localStateService = localStateService;
        _localStateService.PingCycle += LocalStateService_PingCycle;
    }

    private void LocalStateService_PingCycle(bool value) => LocalServiceStatus = value;

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
        _localStateService.PingCycle -= LocalStateService_PingCycle;
    }
}