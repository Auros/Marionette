using CommunityToolkit.Mvvm.ComponentModel;
using Marionette.Models;
using Marionette.Views;
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

    public MainWindowViewModel() { } /* Constructor for design */
    public MainWindowViewModel(InfoViewModel infoViewModel, SettingsViewModel settingsViewModel)
    {
        NavigationLocations.Add(new NavigationPageItem("Info", infoViewModel));
        NavigationLocations.Add(new NavigationPageItem("Settings", settingsViewModel));
    }
}