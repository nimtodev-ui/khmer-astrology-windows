namespace KhmerAstrology.WinForms;

/// <summary>
/// DataGridView that scrolls sideways from touchpad two-finger swipes and tilt
/// wheels (WM_MOUSEHWHEEL), and from Shift + mouse wheel. The stock control
/// ignores horizontal wheel messages, leaving wide grids reachable only by
/// dragging the scroll bar.
/// </summary>
public sealed class HorizontalScrollDataGridView : DataGridView
{
    private const int WmMouseHWheel = 0x020E;

    // Pixels scrolled per standard wheel notch (120 delta units). Precision
    // touchpads send many small deltas, so fractions are carried over.
    private const int PixelsPerNotch = 60;

    private int _pendingDelta;

    public HorizontalScrollDataGridView()
    {
        DoubleBuffered = true;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmMouseHWheel)
        {
            var delta = unchecked((short)((m.WParam.ToInt64() >> 16) & 0xFFFF));
            ScrollHorizontally(delta);
            m.Result = (IntPtr)1;
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if ((ModifierKeys & Keys.Shift) == Keys.Shift && HorizontalScrollBar.Visible)
        {
            // Wheel down (negative delta) moves right, matching browsers and Excel.
            ScrollHorizontally(-e.Delta);
            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }
            return;
        }

        base.OnMouseWheel(e);
    }

    private void ScrollHorizontally(int wheelDelta)
    {
        if (!HorizontalScrollBar.Visible)
        {
            _pendingDelta = 0;
            return;
        }

        _pendingDelta += wheelDelta * PixelsPerNotch;
        var pixels = _pendingDelta / 120;
        _pendingDelta %= 120;
        if (pixels == 0)
        {
            return;
        }

        var bar = HorizontalScrollBar;
        var maximumOffset = Math.Max(0, bar.Maximum - bar.LargeChange + 1);
        var offset = Math.Clamp(HorizontalScrollingOffset + pixels, 0, maximumOffset);
        if (offset != HorizontalScrollingOffset)
        {
            HorizontalScrollingOffset = offset;
        }
    }
}
