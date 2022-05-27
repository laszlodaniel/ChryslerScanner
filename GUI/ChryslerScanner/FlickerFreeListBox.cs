using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChryslerScanner
{
    public class FlickerFreeListBox : ListBox
    {
        public event ListBoxScrolledEventHandler Scrolled;
        public delegate void ListBoxScrolledEventHandler(object sender, EventArgs e);

        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        // For future reference: ListView controls have their own LVM_SCROLL message:
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/6989386b-ba06-4fe8-a7d9-9d3d91475465/sendmessage-message-gets-sent-but-has-no-effect?forum=csharpgeneral
        // https://docs.microsoft.com/hu-hu/windows/win32/controls/lvm-scroll?redirectedfrom=MSDN

        private const int SB_HORIZONTAL = 0;
        private const int SB_VERTICAL = 1;
        private const int SB_PAGELEFT = 2;
        private const int SB_PAGERIGHT = 3;
        private const int SB_THUMBPOSITION = 4;
        private const int SB_THUMBTRACK = 5;
        private const int SB_TOP = 6;
        private const int SB_LEFT = 6;
        private const int SB_BOTTOM = 7;
        private const int SB_RIGHT = 7;
        private const int SB_ENDSCROLL = 8;

        private const int SIF_TRACKPOS = 0x10;
        private const int SIF_RANGE = 0x1;
        private const int SIF_POS = 0x4;
        private const int SIF_PAGE = 0x2;
        private const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

        [DllImport("user32.dll")]
        private static extern int GetScrollInfo(IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo);

        [DllImport("user32.dll")]
        static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref ScrollInfoStruct lpsi, bool fRedraw);

        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private struct ScrollInfoStruct
        {
            public int cbSize;
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }

        public FlickerFreeListBox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            DrawMode = DrawMode.OwnerDrawFixed;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (Items.Count > 0)
            {
                e.DrawBackground();
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Bounds);
                if (Enabled) TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, e.Bounds, ForeColor, TextFormatFlags.Default);
                else TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, e.Bounds, Color.Gray, TextFormatFlags.Default);
            }

            base.OnDrawItem(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Region region = new Region(e.ClipRectangle);
            e.Graphics.FillRegion(new SolidBrush(BackColor), region);
            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; ++i)
                {
                    Rectangle rectangle = GetItemRectangle(i);
                    if (!e.ClipRectangle.IntersectsWith(rectangle)) continue;
                    if ((SelectionMode == SelectionMode.One && SelectedIndex == i) ||
                        (SelectionMode == SelectionMode.MultiSimple && SelectedIndices.Contains(i)) ||
                        (SelectionMode == SelectionMode.MultiExtended && SelectedIndices.Contains(i)))
                    {
                        OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, rectangle, i, DrawItemState.Selected, ForeColor, BackColor));
                    }
                    else
                    {
                        OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, rectangle, i, DrawItemState.Default, ForeColor, BackColor));
                    }

                    region.Complement(rectangle);
                }
            }

            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HSCROLL | m.Msg == WM_VSCROLL)
            {
                if (Scrolled != null)
                {
                    ScrollInfoStruct si = new ScrollInfoStruct();
                    si.fMask = SIF_ALL;
                    si.cbSize = Marshal.SizeOf(si);
                    GetScrollInfo(m.HWnd, 0, ref si);

                    switch (m.WParam.ToInt32())
                    {
                        default:
                            Scrolled?.Invoke(this, EventArgs.Empty); // raise scrolled event
                            break;
                    }
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Gets the current vertical scrollbar position of the listbox.
        /// </summary>
        public int GetVerticalScrollPosition()
        {
            return GetScrollPos((IntPtr)this.Handle, SB_VERTICAL);
        }

        /// <summary>
        /// Sets the vertical scrollbar position of the listbox.
        /// </summary>
        /// <param name="vertScrollPos">Position.</param>
        /// <returns>None.</returns>
        public void SetVerticalScrollPosition(int vertScrollPos)
        {
            ScrollInfoStruct newPos = new ScrollInfoStruct();
            int mask = SIF_TRACKPOS;
            newPos.cbSize = Marshal.SizeOf(newPos);
            newPos.nTrackPos = vertScrollPos;
            newPos.fMask = mask;

            IntPtr wParam = new IntPtr((vertScrollPos << 16) | SB_THUMBPOSITION); // scroll
            SendMessage(new HandleRef(null, this.Handle), WM_VSCROLL, wParam, IntPtr.Zero);

            wParam = new IntPtr(SB_ENDSCROLL); // end scrolling
            SendMessage(new HandleRef(null, this.Handle), WM_VSCROLL, wParam, IntPtr.Zero);
        }
    }
}
