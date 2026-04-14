using Avalonia.Controls;
using Avalonia.Input;

namespace bakalarka5.Core.Views;

public class SelectionController
{
    private readonly SelectionManager _selectionManager;

    public SelectionController(SelectionManager selectionManager)
    {
        _selectionManager = selectionManager;
    }

    public void HandlePointerPressed(Border border, InlineNodeView node, PointerPressedEventArgs e)
    {
        bool isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (isShiftPressed)
        {
            _selectionManager.ExtendSelectionTo(node.Model);
        }
        else
        {
            _selectionManager.SelectSingle(node.Model);
        }

        if (e.GetCurrentPoint(border).Properties.IsRightButtonPressed)
        {
            NeContextMenu.OpenMenu(border, node);
        }

        e.Handled = true;
    }
}