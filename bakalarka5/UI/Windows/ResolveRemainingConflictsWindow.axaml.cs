using Avalonia.Controls;
using Avalonia.Interactivity;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.Windows;

public partial class ResolveRemainingConflictsWindow : Window
{
    public ResolveRemainingConflictsWindow(int conflictCount)
    {
        InitializeComponent();
        MessageText.Text = $"There are still {conflictCount} unresolved conflicts. Choose which version should be used for all remaining conflicts before saving.";
    }

    private void UseA_Click(object? sender, RoutedEventArgs e)
    {
        Close(CurationResolutionKind.UseA);
    }

    private void UseB_Click(object? sender, RoutedEventArgs e)
    {
        Close(CurationResolutionKind.UseB);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}
