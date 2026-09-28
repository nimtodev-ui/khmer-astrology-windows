using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.WinForms;

/// <summary>
/// Standard Khmer wall calendar (ប្រតិទិនចន្ទគតិ) for one week, month or year:
/// Gregorian days with the sheet 30 lunar day, moon phase on holy days
/// (ថ្ងៃសីល) and the Buddhist observances fixed by the lunar date.
/// </summary>
public sealed class KhmerLunarCalendarControl : Control
{
    private static readonly Color Surface = Color.White;
    private static readonly Color Border = Color.FromArgb(218, 224, 234);
    private static readonly Color TextPrimary = Color.FromArgb(25, 42, 70);
    private static readonly Color TextSecondary = Color.FromArgb(91, 104, 125);
    private static readonly Color TextFaded = Color.FromArgb(178, 186, 200);
    private static readonly Color BrandBlue = Color.FromArgb(35, 92, 152);
    private static readonly Color BrandBlueLight = Color.FromArgb(232, 241, 250);
    private static readonly Color SundayRed = Color.FromArgb(192, 48, 48);
    private static readonly Color HolyGreen = Color.FromArgb(30, 110, 60);
    public static readonly Color ObservanceColor = Color.FromArgb(150, 70, 20);
    private static readonly Color OutsideBackground = Color.FromArgb(249, 250, 252);
    private static readonly Color MoonLight = Color.FromArgb(245, 196, 60);
    private static readonly Color MoonDark = Color.FromArgb(70, 82, 104);

    public static readonly Color TodayColor = Color.FromArgb(255, 238, 186);
    private static readonly Color FullMoonColor = Color.FromArgb(255, 250, 228);
    private static readonly Color NewMoonColor = Color.FromArgb(232, 236, 244);
    private static readonly Color HolyDayColor = Color.FromArgb(234, 246, 236);

    private static readonly string[] KhmerWeekdays = ["អាទិត្យ", "ចន្ទ", "អង្គារ", "ពុធ", "ព្រហស្បតិ៍", "សុក្រ", "សៅរ៍"];
    private static readonly string[] KhmerWeekdaysShort = ["អា", "ច", "អ", "ព", "ព្រ", "សុ", "ស"];

    private const TextFormatFlags SingleLine =
        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

    private readonly List<(Rectangle Bounds, DateTime Date)> _hitTargets = [];
    private readonly Font _titleFont;
    private readonly Font _subtitleFont;
    private readonly Font _headerFont;
    private readonly Font _bodyFont;
    private readonly Font _bodyBoldFont;
    private readonly Font _smallFont;
    private readonly Font _monthDayFont;
    private readonly Font _weekDayFont;
    private readonly Font _miniFont;
    private readonly Font _miniBoldFont;
    private readonly VScrollBar? _scrollBar;
    private DateTime _selectedDate = DateTime.Today;
    private bool _isKhmer;

    public KhmerLunarCalendarControl(KhmerLunarCalendarView view, UiFontProvider fonts)
    {
        ArgumentNullException.ThrowIfNull(fonts);
        View = view;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint | ControlStyles.Selectable,
            true);
        BackColor = Surface;
        Cursor = Cursors.Hand;
        Dock = DockStyle.Fill;
        TabStop = true;

        _titleFont = fonts.CreateDisplay(15F, FontStyle.Bold);
        _subtitleFont = fonts.CreateBody(9.5F);
        _headerFont = fonts.CreateBody(9.5F, FontStyle.Bold);
        _bodyFont = fonts.CreateBody(9.5F);
        _bodyBoldFont = fonts.CreateBody(9.5F, FontStyle.Bold);
        _smallFont = fonts.CreateBody(8F);
        _monthDayFont = fonts.CreateDisplay(16F, FontStyle.Bold);
        _weekDayFont = fonts.CreateDisplay(30F, FontStyle.Bold);
        _miniFont = fonts.CreateBody(8F);
        _miniBoldFont = fonts.CreateBody(8.5F, FontStyle.Bold);

        // Twelve mini months need a minimum size, so on small windows the year view scrolls.
        if (view == KhmerLunarCalendarView.Year)
        {
            _scrollBar = new VScrollBar { Dock = DockStyle.Right, Visible = false };
            _scrollBar.ValueChanged += (_, _) => Invalidate();
            Controls.Add(_scrollBar);
        }
    }

    public KhmerLunarCalendarView View { get; }

    /// <summary>Sheet 30 row for a Gregorian date, or null when it cannot be calculated.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Func<DateTime, AutomaticCalendarDayRow?>? DayProvider { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsKhmer
    {
        get => _isKhmer;
        set
        {
            _isKhmer = value;
            UpdateScrollBar(); // Khmer digits change the year view's minimum size
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            var date = value.Date;
            if (date == _selectedDate)
            {
                return;
            }

            _selectedDate = date;
            Invalidate();
        }
    }

    /// <summary>The user picked a day (click or arrow keys).</summary>
    public event EventHandler? SelectedDateChanged;

    /// <summary>The user double-clicked a day or pressed Enter on it.</summary>
    public event EventHandler? DateActivated;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleFont.Dispose();
            _subtitleFont.Dispose();
            _headerFont.Dispose();
            _bodyFont.Dispose();
            _bodyBoldFont.Dispose();
            _smallFont.Dispose();
            _monthDayFont.Dispose();
            _weekDayFont.Dispose();
            _miniFont.Dispose();
            _miniBoldFont.Dispose();
        }

        base.Dispose(disposing);
    }

    // ---------------------------------------------------------------- input

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (e.Button == MouseButtons.Left && HitTest(e.Location) is DateTime date)
        {
            PickDate(date);
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (e.Button == MouseButtons.Left && HitTest(e.Location) is not null)
        {
            DateActivated?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override bool IsInputKey(Keys keyData) =>
        (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        int? days = e.KeyCode switch
        {
            Keys.Left => -1,
            Keys.Right => 1,
            Keys.Up => -7,
            Keys.Down => 7,
            _ => null,
        };
        if (days is int delta)
        {
            e.Handled = true;
            if (TryAddDays(_selectedDate, delta, out var date))
            {
                PickDate(date);
            }
        }
        else if (e.KeyCode == Keys.Enter)
        {
            e.Handled = true;
            DateActivated?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_scrollBar is { Visible: true } bar)
        {
            var maximumValue = bar.Maximum - bar.LargeChange + 1;
            bar.Value = Math.Clamp(bar.Value - e.Delta / 120 * bar.SmallChange * 3, 0, Math.Max(0, maximumValue));
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateScrollBar();
    }

    private void UpdateScrollBar()
    {
        if (_scrollBar is null)
        {
            return;
        }

        var needed = PreferredYearHeight(ClientSize.Width - _scrollBar.Width);
        var visible = needed > ClientSize.Height;
        if (visible)
        {
            _scrollBar.Maximum = needed;
            _scrollBar.LargeChange = Math.Max(1, ClientSize.Height);
            _scrollBar.SmallChange = S(40);
            _scrollBar.Value = Math.Min(_scrollBar.Value, Math.Max(0, needed - ClientSize.Height));
        }
        else
        {
            _scrollBar.Value = 0;
        }

        _scrollBar.Visible = visible;
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    private void PickDate(DateTime date)
    {
        if (date == _selectedDate)
        {
            return;
        }

        SelectedDate = date;
        SelectedDateChanged?.Invoke(this, EventArgs.Empty);
    }

    private DateTime? HitTest(Point point)
    {
        foreach (var (bounds, date) in _hitTargets)
        {
            if (bounds.Contains(point))
            {
                return date;
            }
        }

        return null;
    }

    // ---------------------------------------------------------------- painting

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        _hitTargets.Clear();

        var client = ClientRectangle;
        if (_scrollBar is { Visible: true } bar)
        {
            // Paint the whole scrolled page shifted up; hit targets stay in client coordinates.
            client = new Rectangle(0, -bar.Value, client.Width - bar.Width, bar.Maximum);
        }

        var content = Rectangle.Inflate(client, -S(10), -S(8));
        if (content.Width < S(120) || content.Height < S(120))
        {
            return;
        }

        switch (View)
        {
            case KhmerLunarCalendarView.Week:
                PaintWeek(graphics, content);
                break;
            case KhmerLunarCalendarView.Month:
                PaintMonth(graphics, content);
                break;
            default:
                PaintYear(graphics, content);
                break;
        }
    }

    private int PaintHeader(Graphics graphics, Rectangle bounds, string title, string subtitle)
    {
        var titleHeight = TextRenderer.MeasureText(graphics, "Ag ខ្មែរ", _titleFont).Height;
        var subtitleHeight = TextRenderer.MeasureText(graphics, "Ag ខ្មែរ", _subtitleFont).Height;
        TextRenderer.DrawText(graphics, title, _titleFont,
            new Rectangle(bounds.X, bounds.Y, bounds.Width, titleHeight), TextPrimary, SingleLine);
        TextRenderer.DrawText(graphics, subtitle, _subtitleFont,
            new Rectangle(bounds.X, bounds.Y + titleHeight, bounds.Width, subtitleHeight), TextSecondary, SingleLine);
        return titleHeight + subtitleHeight + S(8);
    }

    private void PaintMonth(Graphics graphics, Rectangle bounds)
    {
        var year = _selectedDate.Year;
        var month = _selectedDate.Month;
        var monthDays = DaysOf(new DateTime(year, month, 1), DateTime.DaysInMonth(year, month));
        var y = bounds.Y + PaintHeader(graphics, bounds,
            $"{GregorianMonthName(month)} {Num(year)}",
            LunarSummary(monthDays));

        var headerHeight = TextRenderer.MeasureText(graphics, "ខ្មែរ", _headerFont).Height + S(8);
        var columns = ColumnEdges(bounds.X, bounds.Width, 7);
        for (var i = 0; i < 7; i++)
        {
            var cell = new Rectangle(columns[i], y, columns[i + 1] - columns[i], headerHeight);
            using var brush = new SolidBrush(BrandBlue);
            graphics.FillRectangle(brush, cell);
            TextRenderer.DrawText(graphics, WeekdayName(i, abbreviated: false), _headerFont, cell,
                i == 0 ? Color.FromArgb(255, 205, 205) : Color.White,
                SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        y += headerHeight;
        var weeks = KhmerLunarCalendarLayout.MonthWeekRows(year, month);
        var rowEdges = ColumnEdges(y, bounds.Bottom - y, weeks);
        var start = KhmerLunarCalendarLayout.MonthGridStart(year, month);
        for (var week = 0; week < weeks; week++)
        {
            for (var weekday = 0; weekday < 7; weekday++)
            {
                if (!TryAddDays(start, week * 7 + weekday, out var date))
                {
                    continue;
                }

                var cell = Rectangle.FromLTRB(columns[weekday], rowEdges[week], columns[weekday + 1], rowEdges[week + 1]);
                PaintMonthCell(graphics, cell, date, inMonth: date.Month == month);
            }
        }
    }

    private void PaintMonthCell(Graphics graphics, Rectangle cell, DateTime date, bool inMonth)
    {
        var row = Day(date);
        using (var background = new SolidBrush(inMonth ? DayBackground(date, row) : OutsideBackground))
        {
            graphics.FillRectangle(background, cell);
        }

        using (var pen = new Pen(Border))
        {
            graphics.DrawRectangle(pen, cell.X, cell.Y, cell.Width - 1, cell.Height - 1);
        }

        _hitTargets.Add((cell, date));
        var inner = Rectangle.Inflate(cell, -S(6), -S(3));

        // Khmer fonts carry a tall line box, so lines are stepped tighter than it. Short
        // cells (small windows) switch to a smaller day number to keep the lunar day visible.
        var lineHeight = TextHeight(_bodyFont);
        var smallHeight = TextHeight(_smallFont);
        var largeDayHeight = TextHeight(_monthDayFont);
        var compact = inner.Height < largeDayHeight * 9 / 10 + lineHeight * 4 / 5;
        var dayFont = compact ? _headerFont : _monthDayFont;
        var dayHeight = compact ? TextHeight(_headerFont) : largeDayHeight;
        var dayColor = !inMonth ? TextFaded : date.DayOfWeek == DayOfWeek.Sunday ? SundayRed : TextPrimary;
        TextRenderer.DrawText(graphics, Num(date.Day), dayFont,
            new Rectangle(inner.X, inner.Y, inner.Width, dayHeight), dayColor, SingleLine);

        if (row is null)
        {
            PaintSelection(graphics, cell, date);
            return;
        }

        var lineY = inner.Y + dayHeight * (compact ? 8 : 9) / 10;
        var showLunar = lineY + lineHeight * 3 / 4 <= inner.Bottom;
        var noteY = lineY + (showLunar ? lineHeight * 4 / 5 : 0);
        var note = row.Observance?.Name(IsKhmer) ?? (row.IsHolyDay ? HolyDayName(row) : null);
        var showNote = note is not null && noteY + smallHeight * 3 / 4 <= inner.Bottom;

        var moonSize = Math.Min(S(compact ? 14 : 18), dayHeight - S(4));
        var markerRight = inner.Right;
        if (row.MoonPhase != KhmerMoonPhase.None && moonSize > S(6))
        {
            DrawMoon(graphics, new Rectangle(inner.Right - moonSize, inner.Y + S(3), moonSize, moonSize), row.MoonPhase, faded: !inMonth);
            markerRight -= moonSize + S(3);
        }

        if (row.Observance is not null && !showNote)
        {
            var dot = S(7);
            using var brush = new SolidBrush(inMonth ? ObservanceColor : TextFaded);
            graphics.FillEllipse(brush, markerRight - dot, inner.Y + S(3) + (moonSize - dot) / 2, dot, dot);
        }

        if (showLunar)
        {
            var lunarText = KhmerLunarCalendarRules.FormatLunarDay(row.LunarDayNumber, IsKhmer);
            if (row.LunarDayNumber == 1 || date.Day == 1)
            {
                lunarText += "  " + KhmerLunarCalendarRules.GetLunarMonthName(row.LunarMonth, IsKhmer);
            }

            TextRenderer.DrawText(graphics, lunarText, row.IsHolyDay ? _bodyBoldFont : _bodyFont,
                new Rectangle(inner.X, lineY, inner.Width, lineHeight),
                !inMonth ? TextFaded : row.IsHolyDay ? HolyGreen : TextSecondary, SingleLine);
        }

        if (showNote)
        {
            TextRenderer.DrawText(graphics, note, _smallFont,
                new Rectangle(inner.X, noteY, inner.Width, smallHeight),
                !inMonth ? TextFaded : row.Observance is not null ? ObservanceColor : HolyGreen, SingleLine);
        }

        PaintSelection(graphics, cell, date);
    }

    private void PaintWeek(Graphics graphics, Rectangle bounds)
    {
        var start = KhmerLunarCalendarLayout.WeekStart(_selectedDate);
        var days = DaysOf(start, 7);
        var end = days.Count > 0 ? days[^1].Date : start;
        var title = IsKhmer
            ? $"សប្តាហ៍ {Num(start.Day)} {GregorianMonthName(start.Month)} – {Num(end.Day)} {GregorianMonthName(end.Month)} {Num(end.Year)}"
            : $"Week of {start.Day} {GregorianMonthName(start.Month)} – {end.Day} {GregorianMonthName(end.Month)} {end.Year}";
        var y = bounds.Y + PaintHeader(graphics, bounds, title, LunarSummary(days));

        var columns = ColumnEdges(bounds.X, bounds.Width, 7);
        var gap = S(6);
        for (var i = 0; i < 7; i++)
        {
            if (!TryAddDays(start, i, out var date))
            {
                continue;
            }

            var card = Rectangle.FromLTRB(columns[i] + (i == 0 ? 0 : gap / 2), y, columns[i + 1] - (i == 6 ? 0 : gap / 2), bounds.Bottom);
            PaintWeekCard(graphics, card, date);
        }
    }

    private void PaintWeekCard(Graphics graphics, Rectangle card, DateTime date)
    {
        var row = Day(date);
        using (var background = new SolidBrush(DayBackground(date, row)))
        {
            graphics.FillRectangle(background, card);
        }

        using (var pen = new Pen(Border))
        {
            graphics.DrawRectangle(pen, card.X, card.Y, card.Width - 1, card.Height - 1);
        }

        _hitTargets.Add((card, date));
        var sunday = date.DayOfWeek == DayOfWeek.Sunday;
        var bandHeight = TextRenderer.MeasureText(graphics, "ខ្មែរ", _headerFont).Height + S(10);
        var band = new Rectangle(card.X, card.Y, card.Width, bandHeight);
        using (var bandBrush = new SolidBrush(sunday ? SundayRed : BrandBlue))
        {
            graphics.FillRectangle(bandBrush, band);
        }

        TextRenderer.DrawText(graphics, WeekdayName((int)date.DayOfWeek, abbreviated: false), _headerFont, band, Color.White,
            SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var inner = Rectangle.FromLTRB(card.X + S(6), band.Bottom + S(6), card.Right - S(6), card.Bottom - S(6));
        var y = inner.Y;

        void Line(string? text, Font font, Color color, int extraSpace = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var height = TextRenderer.MeasureText(graphics, "Ag ខ្មែរ", font).Height;
            if (y + height > inner.Bottom + S(2))
            {
                return;
            }

            TextRenderer.DrawText(graphics, text, font, new Rectangle(inner.X, y, inner.Width, height), color,
                SingleLine | TextFormatFlags.HorizontalCenter);
            y += height + extraSpace;
        }

        Line(Num(date.Day), _weekDayFont, sunday ? SundayRed : TextPrimary);
        Line($"{GregorianMonthName(date.Month, abbreviated: true)} {Num(date.Year)}", _bodyFont, TextSecondary, S(8));
        if (row is null)
        {
            PaintSelection(graphics, card, date);
            return;
        }

        using (var pen = new Pen(Border))
        {
            graphics.DrawLine(pen, inner.X + S(8), y, inner.Right - S(8), y);
        }

        y += S(8);
        Line(KhmerLunarCalendarRules.FormatLunarDay(row.LunarDayNumber, IsKhmer), _headerFont, row.IsHolyDay ? HolyGreen : TextPrimary);
        Line(KhmerLunarCalendarRules.GetLunarMonthName(row.LunarMonth, IsKhmer), _bodyFont, TextSecondary);
        Line(row.TithiName, _smallFont, TextSecondary, S(6));

        if (row.MoonPhase != KhmerMoonPhase.None)
        {
            var moonSize = S(30);
            if (y + moonSize <= inner.Bottom)
            {
                DrawMoon(graphics, new Rectangle(inner.X + (inner.Width - moonSize) / 2, y, moonSize, moonSize), row.MoonPhase, faded: false);
                y += moonSize + S(4);
            }

            Line(MoonPhaseName(row.MoonPhase), _bodyBoldFont, HolyGreen);
        }

        if (row.Observance is { } observance)
        {
            Line(observance.Name(IsKhmer), _bodyBoldFont, ObservanceColor);
        }

        // Year information sits at the bottom of the card.
        var smallHeight = TextRenderer.MeasureText(graphics, "Ag ខ្មែរ", _smallFont).Height;
        var bottom = inner.Bottom - smallHeight * 2;
        if (bottom > y)
        {
            y = bottom;
            Line($"{Localize("Year", "ឆ្នាំ")} {KhmerLunarCalendarRules.GetAnimalYearName(row.AnimalYear, IsKhmer)} {KhmerLunarCalendarRules.GetSakName(row.ChulaSakaraj, IsKhmer)}",
                _smallFont, TextSecondary);
            Line($"{Localize("BE", "ព.ស.")} {Num(row.BuddhistYear)}", _smallFont, TextSecondary);
        }

        PaintSelection(graphics, card, date);
    }

    private void PaintYear(Graphics graphics, Rectangle bounds)
    {
        var year = _selectedDate.Year;
        var first = new DateTime(year, 1, 1);
        var yearDays = DaysOf(first, DateTime.IsLeapYear(year) ? 366 : 365);
        var subtitle = LunarSummary(yearDays);
        var eraChange = yearDays.FirstOrDefault(day => day.Row.BuddhistYear != yearDays[0].Row.BuddhistYear);
        if (eraChange is not null)
        {
            subtitle += IsKhmer
                ? $"  ·  ឆ្លងឆ្នាំ (ព.ស.) ថ្ងៃទី {Num(eraChange.Date.Day)} {GregorianMonthName(eraChange.Date.Month)}"
                : $"  ·  Era changes {eraChange.Date.Day} {GregorianMonthName(eraChange.Date.Month)}";
        }

        var y = bounds.Y + PaintHeader(graphics, bounds, $"{Localize("Year", "ឆ្នាំ")} {Num(year)}", subtitle);
        var area = Rectangle.FromLTRB(bounds.X, y, bounds.Right, bounds.Bottom);
        var (columns, rows, minimumHeight) = YearLayout(area.Width);
        area.Height = Math.Max(area.Height, minimumHeight);
        var columnEdges = ColumnEdges(area.X, area.Width, columns);
        var rowEdges = ColumnEdges(area.Y, area.Height, rows);
        var gap = S(10);
        for (var month = 1; month <= 12; month++)
        {
            var column = (month - 1) % columns;
            var row = (month - 1) / columns;
            var box = Rectangle.FromLTRB(
                columnEdges[column] + (column == 0 ? 0 : gap / 2),
                rowEdges[row] + (row == 0 ? 0 : gap / 2),
                columnEdges[column + 1] - (column == columns - 1 ? 0 : gap / 2),
                rowEdges[row + 1] - (row == rows - 1 ? 0 : gap / 2));
            PaintMiniMonth(graphics, box, year, month, yearDays);
        }
    }

    /// <summary>
    /// Height the year view needs at <paramref name="width"/> so every mini month keeps
    /// readable day cells; the host scrolls when the tab is shorter than this.
    /// </summary>
    private int PreferredYearHeight(int width)
    {
        var header = TextHeight(_titleFont) + TextHeight(_subtitleFont) + S(8);
        return S(16) + header + YearLayout(width - S(20)).MinimumHeight;
    }

    /// <summary>Most mini-month columns (6, 4, 3, 2 or 1) that keep the minimum day-cell width.</summary>
    private (int Columns, int Rows, int MinimumHeight) YearLayout(int width)
    {
        var gap = S(10);
        var cellWidth = TextRenderer.MeasureText(Num(28), _miniBoldFont, Size.Empty, TextFormatFlags.NoPadding).Width + S(8);
        var cellHeight = TextHeight(_miniFont) * 4 / 5;
        var boxWidth = cellWidth * 7 + S(8);
        var boxHeight = TextHeight(_miniBoldFont) + S(6) + cellHeight * (KhmerLunarCalendarLayout.MonthGridWeeks + 1) + S(4);
        var columns = new[] { 6, 4, 3, 2 }.FirstOrDefault(count => count * boxWidth + (count - 1) * gap <= width, 1);
        var rows = (12 + columns - 1) / columns;
        return (columns, rows, rows * boxHeight + (rows - 1) * gap);
    }

    private void PaintMiniMonth(Graphics graphics, Rectangle box, int year, int month, IReadOnlyList<CalendarDay> yearDays)
    {
        using (var pen = new Pen(Border))
        {
            graphics.DrawRectangle(pen, box.X, box.Y, box.Width - 1, box.Height - 1);
        }

        var headerHeight = TextRenderer.MeasureText(graphics, "ខ្មែរ", _miniBoldFont).Height + S(4);
        var header = new Rectangle(box.X, box.Y, box.Width, headerHeight);
        var selectedMonth = month == _selectedDate.Month;
        using (var brush = new SolidBrush(selectedMonth ? BrandBlue : BrandBlueLight))
        {
            graphics.FillRectangle(brush, header);
        }

        var monthDays = yearDays.Where(day => day.Date.Month == month).ToArray();
        var lunarMonths = string.Join(" – ", monthDays
            .Select(day => KhmerLunarCalendarRules.GetLunarMonthName(day.Row.LunarMonth, IsKhmer))
            .Distinct());
        var headerInner = Rectangle.Inflate(header, -S(6), 0);
        var monthName = GregorianMonthName(month);
        var nameWidth = TextRenderer.MeasureText(graphics, monthName, _miniBoldFont).Width;
        TextRenderer.DrawText(graphics, monthName, _miniBoldFont, headerInner, selectedMonth ? Color.White : TextPrimary,
            SingleLine | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(graphics, lunarMonths, _miniFont,
            Rectangle.FromLTRB(headerInner.X + nameWidth + S(8), headerInner.Y, headerInner.Right, headerInner.Bottom),
            selectedMonth ? Color.FromArgb(215, 230, 245) : TextSecondary,
            SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

        var grid = Rectangle.FromLTRB(box.X + S(4), header.Bottom + S(2), box.Right - S(4), box.Bottom - S(4));
        var gridRows = ColumnEdges(grid.Y, grid.Height, KhmerLunarCalendarLayout.MonthGridWeeks + 1);
        var weekdayHeight = gridRows[1] - gridRows[0];
        var columns = ColumnEdges(grid.X, grid.Width, 7);
        for (var i = 0; i < 7; i++)
        {
            TextRenderer.DrawText(graphics, WeekdayName(i, abbreviated: true), _miniFont,
                new Rectangle(columns[i], grid.Y, columns[i + 1] - columns[i], weekdayHeight),
                i == 0 ? SundayRed : TextSecondary,
                SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        var rowEdges = gridRows[1..];
        var firstWeekday = (int)new DateTime(year, month, 1).DayOfWeek;
        var byDay = monthDays.ToDictionary(day => day.Date.Day, day => day.Row);
        for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++)
        {
            var slot = firstWeekday + day - 1;
            var cell = Rectangle.FromLTRB(columns[slot % 7], rowEdges[slot / 7], columns[slot % 7 + 1], rowEdges[slot / 7 + 1]);
            var date = new DateTime(year, month, day);
            byDay.TryGetValue(day, out var row);
            _hitTargets.Add((cell, date));

            var marker = Rectangle.Inflate(cell, -S(1), -S(1));
            var isSelected = date == _selectedDate;
            var fill = isSelected ? BrandBlue
                : date == DateTime.Today ? TodayColor
                : row?.MoonPhase switch
                {
                    KhmerMoonPhase.Full => MoonLight,
                    KhmerMoonPhase.New => Color.FromArgb(200, 208, 222),
                    KhmerMoonPhase.FirstQuarter or KhmerMoonPhase.LastQuarter => Color.FromArgb(200, 232, 206),
                    _ => Color.Empty,
                };
            if (!fill.IsEmpty)
            {
                using var brush = new SolidBrush(fill);
                using var path = RoundedRectangle(marker, S(4));
                graphics.FillPath(brush, path);
            }

            if (row?.Observance is not null && !isSelected)
            {
                var dot = S(4);
                using var brush = new SolidBrush(ObservanceColor);
                graphics.FillEllipse(brush, marker.Right - dot - S(1), marker.Y + S(1), dot, dot);
            }

            var color = isSelected ? Color.White : date.DayOfWeek == DayOfWeek.Sunday ? SundayRed : TextPrimary;
            TextRenderer.DrawText(graphics, Num(day), row?.IsHolyDay == true ? _miniBoldFont : _miniFont, cell, color,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoClipping |
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private void PaintSelection(Graphics graphics, Rectangle cell, DateTime date)
    {
        if (date == DateTime.Today)
        {
            using var todayPen = new Pen(Color.FromArgb(214, 160, 30), S(2));
            graphics.DrawRectangle(todayPen, Rectangle.Inflate(cell, -S(1), -S(1)));
        }

        if (date != _selectedDate)
        {
            return;
        }

        using var pen = new Pen(Focused ? BrandBlue : Color.FromArgb(120, 160, 205), S(3));
        graphics.DrawRectangle(pen, Rectangle.Inflate(cell, -S(2), -S(2)));
    }

    /// <summary>Moon glyph used on holy days; also drawn by the tab's legend.</summary>
    public static void DrawMoon(Graphics graphics, Rectangle bounds, KhmerMoonPhase phase, bool faded)
    {
        var light = faded ? Color.FromArgb(240, 225, 180) : MoonLight;
        var dark = faded ? Color.FromArgb(200, 206, 216) : MoonDark;
        using var lightBrush = new SolidBrush(light);
        using var darkBrush = new SolidBrush(dark);
        using var outline = new Pen(faded ? TextFaded : Color.FromArgb(120, 110, 70), Math.Max(1F, bounds.Width / 16F));
        switch (phase)
        {
            case KhmerMoonPhase.Full:
                graphics.FillEllipse(lightBrush, bounds);
                break;
            case KhmerMoonPhase.New:
                graphics.FillEllipse(darkBrush, bounds);
                break;
            case KhmerMoonPhase.FirstQuarter:
                // Waxing: the right half is lit.
                graphics.FillEllipse(darkBrush, bounds);
                graphics.FillPie(lightBrush, bounds, -90, 180);
                break;
            case KhmerMoonPhase.LastQuarter:
                graphics.FillEllipse(darkBrush, bounds);
                graphics.FillPie(lightBrush, bounds, 90, 180);
                break;
            default:
                return;
        }

        graphics.DrawEllipse(outline, bounds);
    }

    // ---------------------------------------------------------------- data

    private sealed record CalendarDay(DateTime Date, AutomaticCalendarDayRow Row);

    private AutomaticCalendarDayRow? Day(DateTime date) => DayProvider?.Invoke(date);

    private List<CalendarDay> DaysOf(DateTime start, int count)
    {
        var days = new List<CalendarDay>(count);
        for (var i = 0; i < count; i++)
        {
            if (!TryAddDays(start, i, out var date))
            {
                break;
            }

            if (Day(date) is { } row)
            {
                days.Add(new CalendarDay(date, row));
            }
        }

        return days;
    }

    /// <summary>Lunar months, animal year, Sak and BE covered by the days, e.g. "ខែភទ្របទ – ខែអស្សុជ · ឆ្នាំមមី អដ្ឋស័ក · ព.ស. ២៥៧០".</summary>
    private string LunarSummary(IReadOnlyList<CalendarDay> days)
    {
        if (days.Count == 0)
        {
            return Localize("The lunar calendar could not be calculated for this period.", "មិនអាចគណនាប្រតិទិនចន្ទគតិសម្រាប់រយៈពេលនេះបានទេ។");
        }

        string Range(Func<AutomaticCalendarDayRow, string> selector, string separator) =>
            string.Join(separator, days.Select(day => selector(day.Row)).Distinct());

        var lunarMonths = days.Count > 60
            ? null
            : Range(row => KhmerLunarCalendarRules.GetLunarMonthName(row.LunarMonth, IsKhmer), " – ");
        var years = Range(row =>
            $"{Localize("Year of the", "ឆ្នាំ")} {KhmerLunarCalendarRules.GetAnimalYearName(row.AnimalYear, IsKhmer)} {KhmerLunarCalendarRules.GetSakName(row.ChulaSakaraj, IsKhmer)}",
            " → ");
        var eras = Range(row => Num(row.BuddhistYear), " → ");
        var parts = new List<string>();
        if (lunarMonths is not null)
        {
            parts.Add(lunarMonths);
        }

        parts.Add(years);
        parts.Add($"{Localize("BE", "ព.ស.")} {eras}");
        return string.Join("  ·  ", parts);
    }

    private Color DayBackground(DateTime date, AutomaticCalendarDayRow? row)
    {
        if (date == DateTime.Today)
        {
            return TodayColor;
        }

        return row?.MoonPhase switch
        {
            KhmerMoonPhase.Full => FullMoonColor,
            KhmerMoonPhase.New => NewMoonColor,
            KhmerMoonPhase.FirstQuarter or KhmerMoonPhase.LastQuarter => HolyDayColor,
            _ => Surface,
        };
    }

    private string HolyDayName(AutomaticCalendarDayRow row) => row.MoonPhase switch
    {
        KhmerMoonPhase.Full => Localize("Full moon · Holy day", "ពេញបូណ៌មី · ថ្ងៃសីល"),
        KhmerMoonPhase.New => Localize("New moon · Holy day", "អមាវសី · ថ្ងៃសីល"),
        _ => Localize("Holy day", "ថ្ងៃសីល"),
    };

    // Short label under the week card's moon glyph; the glyph already marks the holy day.
    private string MoonPhaseName(KhmerMoonPhase phase) => phase switch
    {
        KhmerMoonPhase.Full => Localize("Full moon", "ពេញបូណ៌មី"),
        KhmerMoonPhase.New => Localize("New moon", "អមាវសី"),
        _ => Localize("Holy day", "ថ្ងៃសីល"),
    };

    // ---------------------------------------------------------------- helpers

    private string Localize(string english, string khmer) => IsKhmer ? khmer : english;

    private string Num(int value) => IsKhmer
        ? AutomaticCalendarCalculator.ToKhmerNumerals(value)
        : value.ToString(CultureInfo.InvariantCulture);

    private string GregorianMonthName(int month, bool abbreviated = false) => IsKhmer
        ? AutomaticCalendarCalculator.GregorianMonthKhmerNames[month - 1]
        : (abbreviated ? CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames : CultureInfo.InvariantCulture.DateTimeFormat.MonthNames)[month - 1];

    private string WeekdayName(int weekday, bool abbreviated) => IsKhmer
        ? (abbreviated ? KhmerWeekdaysShort : KhmerWeekdays)[weekday]
        : abbreviated
            ? CultureInfo.InvariantCulture.DateTimeFormat.ShortestDayNames[weekday]
            : CultureInfo.InvariantCulture.DateTimeFormat.DayNames[weekday];

    private static int TextHeight(Font font) => TextRenderer.MeasureText("Ag ខ្មែរ", font).Height;

    private int S(int pixels) => (int)Math.Round(pixels * DeviceDpi / 96F);

    /// <summary>Splits a span into <paramref name="parts"/> integer edges without accumulating rounding gaps.</summary>
    private static int[] ColumnEdges(int start, int length, int parts)
    {
        var edges = new int[parts + 1];
        for (var i = 0; i <= parts; i++)
        {
            edges[i] = start + (int)Math.Round(length * i / (double)parts);
        }

        return edges;
    }

    private static bool TryAddDays(DateTime date, int days, out DateTime result)
    {
        var ticks = date.Ticks + days * TimeSpan.TicksPerDay;
        if (ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Date.Ticks)
        {
            result = date;
            return false;
        }

        result = new DateTime(ticks);
        return true;
    }

    private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height)));
        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
}
