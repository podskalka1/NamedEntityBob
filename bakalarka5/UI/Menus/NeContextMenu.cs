using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.VisualTree;
using bakalarka5.Core.Annotation;
using bakalarka5.UI.Windows;

namespace bakalarka5.UI.Menus;

public static class NeContextMenu
{
    private static DocumentEditor? _editor;
    private static Window? _ownerWindow;

    private static readonly ContextMenu _menu = BuildContextMenu();

    public static void SetEditor(DocumentEditor? editor)
    {
        _editor = editor;
    }

    private static ContextMenu BuildContextMenu()
    {
        MenuItem BuildMenuItem(NeType type)
        {
            var item = new MenuItem
            {
                Header = type.Description
            };

            item.Click += (_, _) =>
            {
                _editor?.ApplyTypeToSelection(type.Type);
            };

            return item;
        }

        MenuItem BuildSubMenu(NeType type, List<NeType> types)
        {
            var subMenu = new MenuItem
            {
                Header = type.Description
            };

            var items = new List<MenuItem>
            {
                BuildMenuItem(type),
                new MenuItem { Header = "-" }
            };

            foreach (var nestedType in types)
            {
                items.Add(BuildMenuItem(nestedType));
            }

            subMenu.ItemsSource = items;
            return subMenu;
        }

        var deleteSelectionItem = new MenuItem
        {
            Header = "Delete selection"
        };

        deleteSelectionItem.Click += async (_, _) =>
        {
            if (_editor?.CanDeleteSelection() != true)
                return;

            if (_ownerWindow is null)
                return;

            if (await ConfirmDeletion(_ownerWindow))
                _editor.DeleteSelection();
        };

        var deleteLineItem = new MenuItem
        {
            Header = "Delete line"
        };

        deleteLineItem.Click += async (_, _) =>
        {
            if (_editor?.CanDeleteCurrentLine() != true)
                return;

            if (_ownerWindow is null)
                return;

            if (await ConfirmDeletion(_ownerWindow))
                _editor.DeleteCurrentLine();
        };

        var items = new List<MenuItem>
        {
            BuildSubMenu(TypeSet.NumbersType, TypeSet.Numbers),
            BuildSubMenu(TypeSet.PlacesType, TypeSet.Places),
            BuildSubMenu(TypeSet.InstitutionType, TypeSet.Institutions),
            BuildSubMenu(TypeSet.MediaType, TypeSet.Media),
            BuildSubMenu(TypeSet.NumberExpressionsType, TypeSet.NumberExpressions),
            BuildSubMenu(TypeSet.ArtifactType, TypeSet.Artifacts),
            BuildSubMenu(TypeSet.PersonsType, TypeSet.Persons),
            BuildSubMenu(TypeSet.TimeType, TypeSet.Time),
            new MenuItem { Header = "-" },
            BuildMenuItem(TypeSet.NoneType),
            new MenuItem { Header = "-" },
            deleteSelectionItem,
            deleteLineItem
        };

        return new ContextMenu
        {
            ItemsSource = items
        };
    }

    public static void OpenMenu(Control target)
    {
        _ownerWindow = target.GetVisualRoot() as Window;

        target.ContextMenu = _menu;
        _menu.Open(target);
    }

    private static async Task<bool> ConfirmDeletion(Window owner)
    {
        var dialog = new DeleteConfirmWindow();
        return await dialog.ShowDialog<bool>(owner);
    }
}