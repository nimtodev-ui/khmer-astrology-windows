using System.Diagnostics;
using System.Globalization;
using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.WinForms;

/// <summary>
/// "Lunar Calendar" tab: the standard Khmer lunar calendar as week, month and
/// year views, built from the sheet 30 day walk.
/// </summary>
public sealed partial class MainForm
{
    // Gregorian dates the lunar calendar can show: DateTime starts at 1 CE and the
    // workbook's year reference table ends at 4459.
    private static readonly DateTime LunarCalendarMinDate = new(1, 1, 1);
    private static readonly DateTime LunarCalendarMaxDate = new(4459, 12, 31);
    private const int LunarCalendarCacheLimit = 48;

    private readonly Dictionary<(int Year, int Month), AutomaticCalendarMonthResult?> _lunarMonthCache = [];
    private readonly Dictionary<string, Label> _lunarDetailValues = new(StringComparer.Ordinal);
    private readonly Label _lunarDetailTitleLabel = new();
    private readonly DateTimePicker _lunarDatePicker = new();
    private TabControl? _lunarViewTabs;
    private KhmerLunarCalendarControl[] _lunarViews = [];
    private DateTime _lunarSelectedDate = DateTime.Today;
    private bool _isSyncingLunarDate;

    private Control BuildLunarCalendarPanel()
    {
        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 2,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(BuildLunarCalendarToolbar(), 0, 0);

        var body = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 1,
        };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _lunarViewTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            Margin = new Padding(0, 0, 8, 0),
            Padding = new Point(16, 5),
        };
        var views = new List<KhmerLunarCalendarControl>();
        foreach (var (view, english, khmer) in new[]
                 {
                     (KhmerLunarCalendarView.Week, "Weekly", "ប្រចាំសប្តាហ៍"),
                     (KhmerLunarCalendarView.Month, "Monthly", "ប្រចាំខែ"),
                     (KhmerLunarCalendarView.Year, "Yearly", "ប្រចាំឆ្នាំ"),
                 })
        {
            var control = new KhmerLunarCalendarControl(view, _fontProvider)
            {
                DayProvider = GetLunarCalendarDay,
                SelectedDate = _lunarSelectedDate,
            };
            control.SelectedDateChanged += (_, _) => SelectLunarDate(control.SelectedDate);
            control.DateActivated += (_, _) => DrillIntoLunarView(view);
            views.Add(control);

            var page = new TabPage(english) { BackColor = Surface, Padding = new Padding(4) };
            _localizedTabs.Add((page, english, khmer));
            page.Controls.Add(control);
            _lunarViewTabs.TabPages.Add(page);
        }

        _lunarViews = [.. views];
        _lunarViewTabs.SelectedIndex = (int)KhmerLunarCalendarView.Month;
        body.Controls.Add(_lunarViewTabs, 0, 0);
        body.Controls.Add(BuildLunarDetailCard(), 1, 0);
        root.Controls.Add(body, 0, 1);
        return root;
    }

    private Control BuildLunarCalendarToolbar()
    {
        var bar = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.FromArgb(238, 246, 237),
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(10, 8, 10, 8),
            WrapContents = true,
        };

        Button NavButton(string text, string english, string khmer, int delta)
        {
            var button = new Button
            {
                AccessibleName = english,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Margin = new Padding(0, 2, 4, 2),
                Size = new Size(36, 32),
                Text = text,
                UseVisualStyleBackColor = false,
                BackColor = Surface,
            };
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = BrandBlueLight;
            button.Click += (_, _) => StepLunarCalendar(delta);
            _toolTip.SetToolTip(button, $"{english} / {khmer}");
            return button;
        }

        var dateLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 8, 6, 0),
            Text = "Date:",
        };
        RegisterLocalizedControl(dateLabel, "Date:", "កាលបរិច្ឆេទ៖");
        bar.Controls.Add(dateLabel);

        _lunarDatePicker.Font = _fontProvider.CreateBody(10F);
        _lunarDatePicker.Format = DateTimePickerFormat.Custom;
        _lunarDatePicker.CustomFormat = "dd MMM yyyy";
        _lunarDatePicker.Margin = new Padding(0, 4, 10, 0);
        _lunarDatePicker.MaxDate = LunarCalendarMaxDate;
        _lunarDatePicker.Size = new Size(140, 28);
        _lunarDatePicker.Value = _lunarSelectedDate;
        _lunarDatePicker.ValueChanged += (_, _) =>
        {
            if (!_isSyncingLunarDate)
            {
                SelectLunarDate(_lunarDatePicker.Value);
            }
        };
        bar.Controls.Add(_lunarDatePicker);

        bar.Controls.Add(NavButton("◀", "Previous", "មុន", -1));
        var todayButton = new Button();
        ConfigureAutomaticCalendarActionButton(todayButton, "Today", "ថ្ងៃនេះ", primary: false);
        todayButton.Margin = new Padding(0, 2, 4, 2);
        todayButton.Click += (_, _) => SelectLunarDate(DateTime.Today);
        bar.Controls.Add(todayButton);
        bar.Controls.Add(NavButton("▶", "Next", "បន្ទាប់", 1));

        // Legend icons are drawn with the calendar's own moon glyphs; null marks "today".
        foreach (var (phase, english, khmer) in new (KhmerMoonPhase? Phase, string English, string Khmer)[]
                 {
                     (null, "Today", "ថ្ងៃនេះ"),
                     (KhmerMoonPhase.Full, "Full moon", "ពេញបូណ៌មី"),
                     (KhmerMoonPhase.New, "New moon", "អមាវសី"),
                     (KhmerMoonPhase.FirstQuarter, "Holy day (8 waxing / waning)", "ថ្ងៃសីល (៨កើត / ៨រោច)"),
                     (KhmerMoonPhase.None, "Observance", "បុណ្យ"),
                 })
        {
            var icon = new Panel
            {
                BackColor = Color.Transparent,
                Margin = new Padding(14, 9, 6, 0),
                Size = new Size(18, 18),
            };
            icon.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var bounds = Rectangle.Inflate(icon.ClientRectangle, -1, -1);
                if (phase is null)
                {
                    using var brush = new SolidBrush(KhmerLunarCalendarControl.TodayColor);
                    using var pen = new Pen(Color.FromArgb(214, 160, 30), 2);
                    e.Graphics.FillRectangle(brush, bounds);
                    e.Graphics.DrawRectangle(pen, bounds);
                }
                else if (phase == KhmerMoonPhase.None)
                {
                    using var brush = new SolidBrush(KhmerLunarCalendarControl.ObservanceColor);
                    e.Graphics.FillEllipse(brush, Rectangle.Inflate(bounds, -bounds.Width / 4, -bounds.Height / 4));
                }
                else
                {
                    KhmerLunarCalendarControl.DrawMoon(e.Graphics, bounds, phase.Value, faded: false);
                }
            };
            bar.Controls.Add(icon);
            var label = new Label
            {
                AutoSize = true,
                Font = _fontProvider.CreateBody(8.5F),
                ForeColor = TextSecondary,
                Margin = new Padding(0, 9, 0, 0),
                Text = english,
            };
            RegisterLocalizedControl(label, english, khmer);
            bar.Controls.Add(label);
        }

        var hint = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(8.5F),
            ForeColor = TextSecondary,
            Margin = new Padding(18, 9, 0, 0),
            Text = "Double-click a day to zoom in",
        };
        RegisterLocalizedControl(hint, "Double-click a day to zoom in", "ចុចពីរដងលើថ្ងៃ ដើម្បីពង្រីក");
        bar.Controls.Add(hint);
        return bar;
    }

    private Control BuildLunarDetailCard()
    {
        var card = new TableLayoutPanel
        {
            AutoScroll = true,
            BackColor = Color.FromArgb(248, 250, 253),
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 30, 0, 0),
            Padding = new Padding(12, 10, 12, 10),
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        card.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Border);
            eventArgs.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
        };

        _lunarDetailTitleLabel.AutoSize = true;
        _lunarDetailTitleLabel.Dock = DockStyle.Fill;
        _lunarDetailTitleLabel.Font = _fontProvider.CreateDisplay(12F, FontStyle.Bold);
        _lunarDetailTitleLabel.ForeColor = BrandBlue;
        _lunarDetailTitleLabel.Margin = new Padding(0, 0, 0, 10);
        card.Controls.Add(_lunarDetailTitleLabel, 0, 0);
        card.SetColumnSpan(_lunarDetailTitleLabel, 2);

        var fields = new (string Key, string English, string Khmer)[]
        {
            ("weekday", "Weekday", "ថ្ងៃ"),
            ("lunarDay", "Lunar day", "ថ្ងៃចន្ទគតិ"),
            ("tithi", "Tithi", "ឈ្មោះតិថី"),
            ("lunarMonth", "Lunar month", "ខែចន្ទគតិ"),
            ("holyDay", "Holy day", "ថ្ងៃសីល"),
            ("observance", "Observance", "បុណ្យ"),
            ("animalYear", "Animal year", "ឆ្នាំសត្វ"),
            ("sak", "Sak", "ស័ក"),
            ("be", "BE", "ព.ស."),
            ("ms", "MS", "ម.ស."),
            ("cs", "CS", "ច.ស."),
            ("ks", "KS", "ក.ស."),
            ("samvatsara", "Samvatsara", "សំវត្សរ៍"),
        };
        for (var index = 0; index < fields.Length; index++)
        {
            var (key, english, khmer) = fields[index];
            var keyLabel = new Label
            {
                AutoSize = true,
                Font = _fontProvider.CreateBody(9F),
                ForeColor = TextLabel,
                Margin = new Padding(0, 4, 10, 4),
                Text = english,
            };
            RegisterLocalizedControl(keyLabel, english, khmer);
            var valueLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Margin = new Padding(0, 4, 0, 4),
                MaximumSize = new Size(170, 0),
                Text = "—",
            };
            _lunarDetailValues[key] = valueLabel;
            card.Controls.Add(keyLabel, 0, index + 1);
            card.Controls.Add(valueLabel, 1, index + 1);
        }

        return card;
    }

    private AutomaticCalendarDayRow? GetLunarCalendarDay(DateTime date)
    {
        if (date < LunarCalendarMinDate || date > LunarCalendarMaxDate)
        {
            return null;
        }

        var key = (date.Year, date.Month);
        if (!_lunarMonthCache.TryGetValue(key, out var month))
        {
            if (_lunarMonthCache.Count >= LunarCalendarCacheLimit)
            {
                _lunarMonthCache.Clear();
            }

            try
            {
                month = _automaticCalendarCalculator.CalculateMonth(date.Year, date.Month);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error calculating lunar calendar month {date:yyyy-MM}: {ex.Message}");
                month = null;
            }

            _lunarMonthCache[key] = month;
        }

        return month is not null && date.Day <= month.Days.Count ? month.Days[date.Day - 1] : null;
    }

    private KhmerLunarCalendarView CurrentLunarView =>
        _lunarViewTabs is null ? KhmerLunarCalendarView.Month : (KhmerLunarCalendarView)Math.Max(0, _lunarViewTabs.SelectedIndex);

    private void StepLunarCalendar(int delta) =>
        SelectLunarDate(KhmerLunarCalendarLayout.Step(_lunarSelectedDate, CurrentLunarView, delta));

    private void DrillIntoLunarView(KhmerLunarCalendarView view)
    {
        if (_lunarViewTabs is not null && view != KhmerLunarCalendarView.Week)
        {
            _lunarViewTabs.SelectedIndex = (int)view - 1;
            _lunarViews[(int)view - 1].Focus();
        }
    }

    private void SelectLunarDate(DateTime date)
    {
        date = date.Date;
        if (date < LunarCalendarMinDate)
        {
            date = LunarCalendarMinDate;
        }
        else if (date > LunarCalendarMaxDate)
        {
            date = LunarCalendarMaxDate;
        }

        _lunarSelectedDate = date;
        foreach (var view in _lunarViews)
        {
            view.SelectedDate = date;
        }

        _isSyncingLunarDate = true;
        try
        {
            // The picker cannot go below its 1753 minimum; it simply keeps its value then.
            if (date >= _lunarDatePicker.MinDate && date <= _lunarDatePicker.MaxDate)
            {
                _lunarDatePicker.Value = date;
            }
        }
        finally
        {
            _isSyncingLunarDate = false;
        }

        UpdateLunarDetails();
    }

    private void ApplyLunarCalendarLanguage()
    {
        foreach (var view in _lunarViews)
        {
            view.IsKhmer = IsKhmer;
        }

        UpdateLunarDetails();
    }

    private void UpdateLunarDetails()
    {
        var date = _lunarSelectedDate;
        string Number(int value) => IsKhmer ? ToKhmerDigits(value) : value.ToString(CultureInfo.InvariantCulture);
        var gregorianMonth = GregorianMonthDisplayNames()[date.Month - 1];
        _lunarDetailTitleLabel.Text = $"{Number(date.Day)} {gregorianMonth} {Number(date.Year)}";

        void Set(string key, string? value) => _lunarDetailValues[key].Text = string.IsNullOrEmpty(value) ? "—" : value;
        var row = GetLunarCalendarDay(date);
        if (row is null)
        {
            foreach (var label in _lunarDetailValues.Values)
            {
                label.Text = "—";
            }

            Set("weekday", LocalizeKhmerWeekday(KhmerWeekdayNames[(int)date.DayOfWeek]));
            return;
        }

        Set("weekday", LocalizeKhmerWeekday(row.Weekday));
        Set("lunarDay", KhmerLunarCalendarRules.FormatLunarDay(row.LunarDayNumber, IsKhmer));
        Set("tithi", row.TithiName);
        Set("lunarMonth", IsKhmer
            ? $"{row.LunarMonth} ({Number(row.LunarMonthLength)} ថ្ងៃ)"
            : $"{KhmerLunarCalendarRules.GetLunarMonthName(row.LunarMonth, khmer: false)} ({row.LunarMonth}, {row.LunarMonthLength} days)");
        Set("holyDay", row.MoonPhase switch
        {
            KhmerMoonPhase.Full => Localize("Yes — full moon", "ជាថ្ងៃសីល — ពេញបូណ៌មី"),
            KhmerMoonPhase.New => Localize("Yes — new moon", "ជាថ្ងៃសីល — អមាវសី"),
            KhmerMoonPhase.FirstQuarter or KhmerMoonPhase.LastQuarter => Localize("Yes — 8th day", "ជាថ្ងៃសីល — ថ្ងៃ៨"),
            _ => Localize("No", "មិនមែន"),
        });
        Set("observance", row.Observance?.Name(IsKhmer));
        Set("animalYear", KhmerLunarCalendarRules.GetAnimalYearName(row.AnimalYear, IsKhmer));
        Set("sak", SakName(row.ChulaSakaraj));
        Set("be", Number(row.BuddhistYear));
        Set("ms", Number(row.MahaSakaraj));
        Set("cs", Number(row.ChulaSakaraj));
        Set("ks", Number(row.KromSakaraj));
        Set("samvatsara", $"{row.SamvatsaraName} — {row.Meaning}");
    }
}
