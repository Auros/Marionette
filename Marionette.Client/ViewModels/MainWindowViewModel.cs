using Marionette.Views;

namespace Marionette.ViewModels;

[View(typeof(MainWindow))]
public partial class MainWindowViewModel
{
    public string Hello => "Hello!";

    public MainWindowViewModel() { }
}