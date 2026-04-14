using System.Collections.Generic;
using Avalonia.Controls;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public static class NeContextMenu
{
    private static DocumentEditor? _editor;
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
            BuildMenuItem(TypeSet.NoneType)
        };

        return new ContextMenu
        {
            ItemsSource = items
        };
    }

    public static void OpenMenu(Control target)
    {
        target.ContextMenu = _menu;
        _menu.Open(target);
    }
}