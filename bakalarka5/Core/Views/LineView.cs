using System.Collections.ObjectModel;
using System.Linq;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class LineView
{
    public LineItem Model { get; }

    public ObservableCollection<InlineNodeView> Nodes { get; }

    public LineView(LineItem model)
    {
        Model = model;
        Nodes = new ObservableCollection<InlineNodeView>(
            model.Children.Select(ViewFactory.Create)
        );
    }
}