using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class TypeView : InlineNodeView
{
    public TypeItem TypeModel => (TypeItem)Model;

    public string? Tag
    {
        get => TypeModel.Type;
        set
        {
            if (TypeModel.Type == value) return;
            TypeModel.Type = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Background));
        }
    }

    public ObservableCollection<InlineNodeView> Children { get; }

    public IBrush Background => new SolidColorBrush(DetermineColor());

    public TypeView(TypeItem model, SelectionManager selectionManager)
        : base(model, selectionManager)
    {
        Children = new ObservableCollection<InlineNodeView>(
            model.Children.Select(child => ViewFactory.Create(child, selectionManager))
        );

        model.PropertyChanged += ModelOnPropertyChanged;
        model.Children.CollectionChanged += ModelChildrenOnCollectionChanged;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TypeItem.Type))
        {
            OnPropertyChanged(nameof(Tag));
            OnPropertyChanged(nameof(Background));
        }
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
                    Children.Insert(addIndex, ViewFactory.Create(node, SelectionManager));
                    addIndex++;
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is null) return;

                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    Children.RemoveAt(e.OldStartingIndex);
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems is null) return;

                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    Children[e.NewStartingIndex + i] =
                        ViewFactory.Create((InlineNode)e.NewItems[i]!, SelectionManager);
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                Children.Clear();
                foreach (var child in TypeModel.Children)
                {
                    Children.Add(ViewFactory.Create(child, SelectionManager));
                }
                break;

            case NotifyCollectionChangedAction.Move:
                break;
        }
    }

    private Color DetermineColor()
    {
        if (Tag is null) return Colors.Black;

        var c = char.ToLower(Tag[0]);
        return c switch
        {
            'a' => Colors.DarkGoldenrod,
            'g' => Colors.DarkGreen,
            'i' => Colors.Brown,
            'm' => Colors.DarkBlue,
            'n' => Colors.DarkRed,
            'o' => Colors.DarkOrchid,
            'p' => Colors.DarkCyan,
            't' => Colors.DarkSlateGray,
            _ => Colors.Transparent
        };
    }
}