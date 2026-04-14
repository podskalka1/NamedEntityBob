using System;
using System.Collections.Generic;
using Avalonia.Controls;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public static class NeContextMenu
{
    public static Action<InlineNodeView, string?>? ApplyTypeAction { get; set; }

    private static ContextMenu BuildContextMenu(InlineNodeView node)
    {
        MenuItem BuildMenuItem(NeType type)
        {
            var item = new MenuItem
            {
                Header = type.Description
            };

            item.Click += (_, _) =>
            {
                ApplyTypeAction?.Invoke(node, type.Type);
                node.IsSelected = false;
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

    public static void OpenMenu(Border border, InlineNodeView node)
    {
        node.IsSelected = true;

        var menu = BuildContextMenu(node);
        border.ContextMenu = menu;
        menu.Open(border);
    }
}