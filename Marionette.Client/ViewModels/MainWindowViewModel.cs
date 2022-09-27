using CommunityToolkit.Mvvm.ComponentModel;
using Marionette.Models;
using Marionette.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;

namespace Marionette.ViewModels;

[View(typeof(MainWindow))]
public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveView))]
    [NotifyPropertyChangedFor(nameof(ActiveViewModel))]
    private NavigationPageItem? _activePage;

    public object? ActiveView => _activePage?.View;

    public object? ActiveViewModel => _activePage?.ViewModel;

    public ObservableCollection<NavigationPageItem> NavigationLocations { get; } = new();

    public MainWindowViewModel() { } /* Constructor for Avalonia designer */
    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        NavigationLocations.Add(new NavigationPageItem("Info", serviceProvider.GetRequiredService<InfoViewModel>()));
        NavigationLocations.Add(new NavigationPageItem("Settings", serviceProvider.GetRequiredService<SettingsViewModel>()));
        NavigationLocations.Add(new NavigationPageItem("Status", serviceProvider.GetRequiredService<StatusViewModel>()));
    }
}