using Avalonia.Controls;
using Avalonia.Input;
using bakalarka5.UI.DocumentUI;
using bakalarka5.UI.Menus;

namespace bakalarka5.Core.Selection;

public class SelectionController
{
    private readonly SelectionManager _selectionManager;

    public SelectionController(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
    }

    public void HandlePointerPressed(Border border, InlineNodeView node, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(border);

        if (point.Properties.IsRightButtonPressed)
        {
            if (!_selectionManager.Current?.Contains(node.Model) ?? true)
                _selectionManager.SelectSingle(node.Model);

            NeContextMenu.OpenMenu(border);
            e.Handled = true;
            return;
        }
        
        bool isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (isShiftPressed)
            _selectionManager.ExtendSelectionTo(node.Model);
        else
            _selectionManager.SelectSingle(node.Model);

        e.Handled = true;
    }
}