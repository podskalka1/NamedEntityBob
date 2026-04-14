using System.Collections.ObjectModel;
using System.Linq;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class LineView
{
    public LineItem Model { get; }
    public SelectionManager SelectionManager { get; }

    public ObservableCollection<InlineNodeView> Nodes { get; }

    public LineView(LineItem model, SelectionManager selectionManager)
    {
        Model = model;
        SelectionManager = selectionManager;
        Nodes = new ObservableCollection<InlineNodeView>(
            model.Children.Select(child => ViewFactory.Create(child, selectionManager))
        );
    }
}