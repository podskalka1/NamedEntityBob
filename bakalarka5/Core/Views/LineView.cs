using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class LineView
{
    public LineItem Model { get; }
    public ObservableCollection<InlineNodeView> Nodes { get; }

    private readonly SelectionManager _selectionManager;

    public LineView(LineItem model, SelectionManager selectionManager)
    {
        Model = model;
        _selectionManager = selectionManager;

        Nodes = new ObservableCollection<InlineNodeView>(
            model.Children.Select(child => ViewFactory.Create(child, _selectionManager))
        );

        model.Children.CollectionChanged += ModelChildrenOnCollectionChanged;
    }

    private void ModelChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is null) return;

                int addIndex = e.NewStartingIndex;
                foreach (InlineNode node in e.NewItems)
                {
                    Nodes.Insert(addIndex, ViewFactory.Create(node, _selectionManager));
                    addIndex++;
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is null) return;

                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    Nodes.RemoveAt(e.OldStartingIndex);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems is null) return;

                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    Nodes[e.NewStartingIndex + i] =
                        ViewFactory.Create((InlineNode)e.NewItems[i]!, _selectionManager);
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                Nodes.Clear();
                foreach (var child in Model.Children)
                {
                    Nodes.Add(ViewFactory.Create(child, _selectionManager));
                }
                break;

            case NotifyCollectionChangedAction.Move:
                // probably unnecessary for your current editor operations
                break;
        }
    }
}