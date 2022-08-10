using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Marionette;

public class ViewLocator : IDataTemplate
{
    private readonly IServiceProvider _container;

    public ViewLocator()
    {
        // Not too big of a fan of this pattern, but this app doesn't need to be that big... I just want to get it done.
        _container = Program.Container;
    }

    public IControl Build(object param)
    {
        var type = param.GetType();
        var viewAttribute = type.GetCustomAttribute<ViewAttribute>();
        if (viewAttribute is null)
            return new TextBlock { Text = "Could not find view for " + type.Name };

        var control = (Control)Activator.CreateInstance(viewAttribute.ViewType)!;
        control.DataContext = _container.GetRequiredService(type);

        return control;
    }

    public bool Match(object data)
    {
        return data is INotifyPropertyChanged;
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal class ViewAttribute : Attribute
{
    public Type ViewType { get; } 

    public ViewAttribute(Type viewType)
    {
        ViewType = viewType;
    }
}