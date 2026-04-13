using System;
using System.Collections.Generic;
using Avalonia.Controls;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public static class NeContextMenu
{
    private static TokenView? _contextToken;
    private static ContextMenu? _contextMenu;

    static NeContextMenu()
    {
        _contextMenu = BuildTokenContextMenu();
    }

    private static ContextMenu BuildTokenContextMenu()
    {
        MenuItem BuildMenuItem(NeType type)
        {
            var item = new MenuItem
            {
                Header = type.Description
            };

            item.Click += (_, _) =>
            {
                _contextToken.Tag = type.Type;
                _contextToken.IsSelected = false;
                _contextToken = null;
            };
            return item;
        }

        MenuItem BuildSubMenu(NeType type, List<NeType> types)
        { 
            var subMenu = new MenuItem
            {
                Header = type.Description
            };

            var items = new List<MenuItem>();
            
            items.Add(BuildMenuItem(type));
            items.Add(new MenuItem { Header = "-" });
            
            foreach (var nestedType in types)
            {
                var nestedItem = BuildMenuItem(nestedType);

                items.Add(nestedItem);
            }
            subMenu.ItemsSource = items;
            
            return subMenu;
        }
        
        var items = new List<MenuItem>();
        
        //TODO at the top of this Menu will come a MenuItem which will work as a gateway into that Entity Linking Thingamajig 
        
        items.Add(BuildSubMenu(TypeSet.NumbersType,TypeSet.Numbers));
        items.Add(BuildSubMenu(TypeSet.PlacesType,TypeSet.Places));
        items.Add(BuildSubMenu(TypeSet.InstitutionType,TypeSet.Institutions));
        items.Add(BuildSubMenu(TypeSet.MediaType,TypeSet.Media));
        items.Add(BuildSubMenu(TypeSet.NumberExpressionsType,TypeSet.NumberExpressions));
        items.Add(BuildSubMenu(TypeSet.ArtifactType,TypeSet.Artifacts));
        items.Add(BuildSubMenu(TypeSet.PersonsType,TypeSet.Persons));
        items.Add(BuildSubMenu(TypeSet.TimeType,TypeSet.Time));

        items.Add(new MenuItem { Header = "-" });

        items.Add(BuildMenuItem(TypeSet.NoneType));
        
        return new ContextMenu
        {
            ItemsSource = items
        };
    }

    public static void OpenMenu(Border border, TokenView token)
    {
        var menu = BuildTokenContextMenu();
        _contextToken = token;
        border.ContextMenu =  menu;
        menu.Open(border);
    }
}