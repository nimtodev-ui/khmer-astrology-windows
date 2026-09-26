using System.Globalization;
using System.Diagnostics;
using KhmerAstrology.Application.Interfaces;
using KhmerAstrology.Calculation.Calendar;
using KhmerAstrology.Calculation.Interfaces;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Interfaces;
using KhmerAstrology.Domain.Models;
using KhmerAstrology.Infrastructure.ReferenceData;

namespace KhmerAstrology.WinForms;

public sealed class MainForm : Form
{
    private static readonly Color PageBackground = Color.FromArgb(245, 247, 250);
    private static readonly Color Surface = Color.White;
    private static readonly Color Border = Color.FromArgb(218, 224, 234);
    private static readonly Color TextPrimary = Color.FromArgb(25, 42, 70);
    private static readonly Color TextSecondary = Color.FromArgb(91, 104, 125);
    private static readonly Color TextLabel = Color.FromArgb(75, 86, 106);
    private static readonly Color BrandBlue = Color.FromArgb(35, 92, 152);
    private static readonly Color BrandBlueLight = Color.FromArgb(232, 241, 250);
    private static readonly Color SuccessGreen = Color.FromArgb(32, 122, 83);

    private readonly IAstrologyCalculationService _astrologyCalculationService;
    private readonly IKhmerCalendarCalculator _khmerCalendarCalculator;
    private readonly IAutomaticCalendarCalculator _automaticCalendarCalculator;
    private readonly ILocationReferenceDataSource _locationReferenceDataSource;
    private readonly UiFontProvider _fontProvider;
    private readonly ErrorProvider _errorProvider = new();
    private readonly TextBox _nameTextBox = new();
    private readonly ComboBox _genderComboBox = new();
    private readonly DateTimePicker _birthDatePicker = new();
    private readonly DateTimePicker _birthTimePicker = new();
    private readonly ComboBox _countryComboBox = new();
    private readonly ComboBox _locationComboBox = new();
    private readonly TextBox _internationalCountryTextBox = new();
    private readonly TextBox _internationalRegionTextBox = new();
    private readonly TextBox _latitudeTextBox = new();
    private readonly TextBox _longitudeTextBox = new();
    private readonly TextBox _timeZoneTextBox = new();
    private readonly TextBox _utcOverrideTextBox = new();
    private readonly TextBox _astronomicalMasterYearTextBox = new();
    private readonly Dictionary<string, Label> _masterBridgeLabels = new(StringComparer.Ordinal);
    private readonly Label _statusLabel = new();
    private readonly TextBox _technicalTextBox = new();
    private readonly DataGridView _resultGrid = new();
    private readonly HorizontalScrollDataGridView _planetGrid = new();
    private readonly DataGridView _nakshatraGrid = new();
    private readonly DataGridView _calendarGrid = new();
    private readonly DataGridView _atthabhujjGrid = new();
    private readonly Dictionary<string, Control> _atthabhujjValues = new(StringComparer.Ordinal);
    private readonly TextBox _atthabhujjYearTextBox = new();
    private readonly Button _calculateAtthabhujjButton = new();
    private readonly TextBox _autoCalendarYearTextBox = new();
    private readonly TextBox _autoCalendarKsTextBox = new();
    private readonly ComboBox _autoCalendarMonthComboBox = new();
    private readonly Button _calculateAutoCalendarButton = new();
    private readonly Button _autoCalendarTodayButton = new();
    private readonly DataGridView _autoCalendarGrid = new();
    private readonly ComboBox _searchDateDayComboBox = new();
    private readonly ComboBox _searchDateMonthComboBox = new();
    private readonly TextBox _searchDateYearTextBox = new();
    private readonly TextBox _searchDateKsTextBox = new();
    private readonly Button _searchDateButton = new();
    private readonly DataGridView _searchDateGrid = new();
    private readonly Dictionary<string, Label> _searchDateSummaryLabels = new(StringComparer.Ordinal);
    private readonly HoroscopeChartControl _d1Chart = new();
    private readonly HoroscopeChartControl _d3Chart = new();
    private readonly HoroscopeChartControl _d9Chart = new();
    private readonly TextBox _interpretationTextBox = new();
    private readonly Button _calculateButton = new();
    private readonly Label _resultTitleLabel = new();
    private readonly Label _resultSubtitleLabel = new();
    private readonly Label _headerStatusLabel = new();
    private readonly Label _languageLabel = new();
    private readonly Button _englishLanguageButton = new();
    private readonly Button _khmerLanguageButton = new();
    private readonly List<(Label Label, string English, string Khmer)> _localizedFieldLabels = [];
    private readonly List<(Control Control, string English, string Khmer)> _localizedControls = [];
    private readonly List<(TabPage Page, string English, string Khmer)> _localizedTabs = [];
    private readonly List<(DataGridViewColumn Column, string English, string Khmer)> _localizedColumns = [];
    private KhmerAstrology.Application.DTOs.AstrologyResult? _lastResult;
    private KhmerCalendarResult? _lastCalendarResult;
    private Label? _brandTitleLabel;
    private Label? _brandSubtitleLabel;
    private GroupBox? _birthProfileGroup;
    private GroupBox? _calculationWorkspaceGroup;
    private TabControl? _tabs;
    private TabPage? _resultTab;
    private bool _isRelocalizingChoices;

    // Canonical (English) values stored in BirthInput; the combo boxes show the localized text.
    private static readonly (string English, string Khmer)[] GenderChoices =
    [
        ("Male", "ប្រុស"),
        ("Female", "ស្រី"),
        ("Other / Not specified", "ផ្សេងៗ / មិនបញ្ជាក់"),
    ];

    private static readonly (string English, string Khmer)[] CountryChoices =
    [
        ("Cambodia", "កម្ពុជា"),
        ("International", "អន្តរជាតិ"),
    ];

    private const int InternationalCountryIndex = 1;

    private bool IsInternationalSelected => _countryComboBox.SelectedIndex == InternationalCountryIndex;
    private bool _isKhmer = UiPreferences.LoadIsKhmer();

    private bool IsKhmer => _isKhmer;

    private string Localize(string english, string khmer) => IsKhmer ? khmer : english;

    public MainForm(
        IAstrologyCalculationService astrologyCalculationService,
        IKhmerCalendarCalculator khmerCalendarCalculator,
        IAutomaticCalendarCalculator automaticCalendarCalculator,
        ILocationReferenceDataSource locationReferenceDataSource,
        UiFontProvider fontProvider)
    {
        _astrologyCalculationService = astrologyCalculationService;
        _khmerCalendarCalculator = khmerCalendarCalculator;
        _automaticCalendarCalculator = automaticCalendarCalculator;
        _locationReferenceDataSource = locationReferenceDataSource;
        _fontProvider = fontProvider;

        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        InitializeForm();
        BuildLayout();
        AcceptButton = _calculateButton;
        BindLocations();
        ApplyLanguage();
        UpdateMasterBridgeLabels();
        InitializeDefaultAtthabhujj();
        PopulateAutomaticCalendar();
        PopulateSearchDate();
        CalculateHoroscopeSilently();
    }

    private void InitializeForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = PageBackground;
        ClientSize = new Size(1_340, 840);
        Font = _fontProvider.CreateBody(10F);
        MinimumSize = new Size(1_120, 700);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Khmer Astrology \u2014 Suriyay\u0101tra Calculation System";
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 20, 24, 14),
            RowCount = 3,
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildTabs(), 0, 1);
        root.Controls.Add(BuildStatusBar(), 0, 2);

        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 1,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

        var branding = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 2,
        };
        branding.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        branding.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _brandTitleLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateDisplay(20F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "KHMER ASTROLOGY",
        };
        branding.Controls.Add(_brandTitleLabel, 0, 0);
        _brandSubtitleLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(10.5F),
            ForeColor = TextSecondary,
            Padding = new Padding(2, 2, 0, 0),
            Text = "Suriyay\u0101tra Calculation System  \u2022  Workbook-backed foundation",
        };
        branding.Controls.Add(_brandSubtitleLabel, 0, 1);

        var statusPanel = new Panel
        {
            BackColor = Surface,
            Dock = DockStyle.Fill,
            Margin = new Padding(16, 2, 0, 2),
            Padding = new Padding(16, 6, 16, 6),
        };
        statusPanel.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Border);
            eventArgs.Graphics.DrawRectangle(pen, 0, 0, statusPanel.Width - 1, statusPanel.Height - 1);
        };
        var statusLayout = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var workbookModeLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(8.5F, FontStyle.Bold),
            ForeColor = SuccessGreen,
            Text = "WORKBOOK MODE",
        };
        RegisterLocalizedControl(workbookModeLabel, "WORKBOOK MODE", "របៀបសៀវភៅការងារ");
        statusLayout.Controls.Add(workbookModeLabel, 0, 0);
        var languagePanel = new FlowLayoutPanel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(8, 0, 0, 0),
            WrapContents = false,
        };
        _languageLabel.AutoSize = true;
        _languageLabel.Font = _fontProvider.CreateBody(8.5F, FontStyle.Bold);
        _languageLabel.ForeColor = TextSecondary;
        _languageLabel.Margin = new Padding(0, 6, 8, 0);
        ConfigureLanguageButton(_englishLanguageButton, "English", isKhmer: false);
        ConfigureLanguageButton(_khmerLanguageButton, "ខ្មែរ", isKhmer: true);
        languagePanel.Controls.Add(_languageLabel);
        languagePanel.Controls.Add(_englishLanguageButton);
        languagePanel.Controls.Add(_khmerLanguageButton);
        statusLayout.Controls.Add(languagePanel, 1, 0);
        _headerStatusLabel.AutoSize = true;
        _headerStatusLabel.Dock = DockStyle.Fill;
        _headerStatusLabel.Font = _fontProvider.CreateBody(9F);
        _headerStatusLabel.ForeColor = TextSecondary;
        _headerStatusLabel.Text = "Ready for a birth profile";
        _headerStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusLayout.Controls.Add(_headerStatusLabel, 0, 1);
        statusLayout.SetColumnSpan(_headerStatusLabel, 2);
        statusPanel.Controls.Add(statusLayout);

        layout.Controls.Add(branding, 0, 0);
        layout.Controls.Add(statusPanel, 1, 0);
        return layout;
    }

    private TabControl BuildTabs()
    {
        _tabs = new TabControl
        {
            Appearance = TabAppearance.Normal,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(9.5F),
            ItemSize = new Size(0, 42),
            Padding = new Point(14, 7),
            SizeMode = TabSizeMode.Normal,
        };
        var overview = new TabPage("Birth Profile") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((overview, "Birth Profile", "ព័ត៌មានកំណើត"));
        overview.Controls.Add(BuildInputPanel());

        _resultTab = new TabPage("Results") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((_resultTab, "Results", "លទ្ធផល"));
        _resultTab.Controls.Add(BuildResultPanel());

        var technical = new TabPage("Technical Details") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((technical, "Technical Details", "ព័ត៌មានបច្ចេកទេស"));
        _technicalTextBox.Dock = DockStyle.Fill;
        _technicalTextBox.Multiline = true;
        _technicalTextBox.ReadOnly = true;
        _technicalTextBox.ScrollBars = ScrollBars.Vertical;
        _technicalTextBox.BackColor = Color.FromArgb(250, 251, 253);
        _technicalTextBox.BorderStyle = BorderStyle.FixedSingle;
        _technicalTextBox.Font = _fontProvider.CreateBody(9.5F);
        _technicalTextBox.Text =
            "Migration status\r\n\r\n" +
            "✓ Workbook inventory completed\r\n" +
            "✓ Formula mapping created\r\n" +
            "✓ Longitude normalization, D1, D3, D9\r\n" +
            "✓ Zodiac and Nakshatra/Pada calculators\r\n" +
            "✓ Whole-sign house fallback (pending workbook house validation)\r\n\r\n" +
            "✓ Khmer calendar/Ahargana chain migrated from អដ្ឋភុជ្ជ!B14:B47\r\n" +
            "✓ Traditional Sun/Moon sheet chains migrated and tested\r\n" +
            "✓ Modern Ascendant chain migrated from ក្បួនគណនា!Q41:Q52\r\n\r\n" +
            "✓ Modern planetary result-table rows migrated from the extracted\r\n" +
            "workbook coefficient and interpolation tables.\r\n" +
            "✓ Radial D1 / D9 / D3 comparison charts\r\n" +
            "✓ Workbook house interpretation rules loaded from JSON\r\n";
        technical.Controls.Add(_technicalTextBox);

        var charts = new TabPage("D1 / D9 / D3") { BackColor = Surface, Padding = new Padding(10) };
        _localizedTabs.Add((charts, "D1 / D9 / D3", "D1 / D9 / D3"));
        charts.Controls.Add(BuildChartsPanel());
        var nakshatra = new TabPage("Nakshatra") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((nakshatra, "Nakshatra", "នក្ខត្តឫក្ស"));
        nakshatra.Controls.Add(BuildNakshatraPanel());
        var calendar = new TabPage("Khmer Calendar") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((calendar, "Khmer Calendar", "ប្រតិទិនខ្មែរ"));
        calendar.Controls.Add(BuildCalendarPanel());
        var autoCalendar = new TabPage("Automatic Calendar") { BackColor = Surface, Padding = new Padding(10) };
        _localizedTabs.Add((autoCalendar, "Automatic Calendar", "ប្រតិទិនស្វ័យប្រវត្តិ"));
        autoCalendar.Controls.Add(BuildAutomaticCalendarPanel());
        var searchDate = new TabPage("Search Date") { BackColor = Surface, Padding = new Padding(10) };
        _localizedTabs.Add((searchDate, "Search Date", "ស្វែងរក ថ្ងៃខែឆ្នាំ"));
        searchDate.Controls.Add(BuildSearchDatePanel());
        var atthabhujj = new TabPage("Atthabhujj") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((atthabhujj, "Atthabhujj", "អដ្ឋភុជ្ជ"));
        atthabhujj.Controls.Add(BuildAtthabhujjPanel());
        var interpretation = new TabPage("Interpretation") { BackColor = Surface, Padding = new Padding(16) };
        _localizedTabs.Add((interpretation, "Interpretation", "ការបកស្រាយ"));
        interpretation.Controls.Add(BuildInterpretationPanel());

        _tabs.TabPages.Add(overview);
        _tabs.TabPages.Add(_resultTab);
        _tabs.TabPages.Add(charts);
        _tabs.TabPages.Add(nakshatra);
        _tabs.TabPages.Add(calendar);
        _tabs.TabPages.Add(autoCalendar);
        _tabs.TabPages.Add(searchDate);
        _tabs.TabPages.Add(atthabhujj);
        _tabs.TabPages.Add(interpretation);
        _tabs.TabPages.Add(technical);
        _tabs.SelectedIndexChanged += (s, e) =>
        {
            if (_tabs.SelectedTab == atthabhujj)
            {
                AcceptButton = _calculateAtthabhujjButton;
            }
            else if (_tabs.SelectedTab == autoCalendar)
            {
                AcceptButton = _calculateAutoCalendarButton;
            }
            else if (_tabs.SelectedTab == searchDate)
            {
                AcceptButton = _searchDateButton;
            }
            else
            {
                AcceptButton = _calculateButton;
            }
        };
        return _tabs;
    }

    private Control BuildChartsPanel()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 8, 0, 0),
            RowCount = 1,
        };
        for (var index = 0; index < 3; index++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333F));
        }

        layout.Controls.Add(BuildChartCard("D1  ·  Rāśi chart", "D1  ·  រាសិចក្ក", _d1Chart), 0, 0);
        layout.Controls.Add(BuildChartCard("D9  ·  Navāṃśa chart", "D9  ·  នវម្ស", _d9Chart), 1, 0);
        layout.Controls.Add(BuildChartCard("D3  ·  Drekkāṇa chart", "D3  ·  ទ្រេកាណ", _d3Chart), 2, 0);
        return layout;
    }

    private Control BuildChartCard(string title, string khmerTitle, HoroscopeChartControl chart)
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
            Margin = new Padding(5, 0, 5, 0),
            Padding = new Padding(8, 26, 8, 8),
            Text = title,
        };
        _localizedControls.Add((group, title, khmerTitle));
        group.Controls.Add(chart);
        return group;
    }

    private Control BuildInputPanel()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 8,
        };
        split.Panel1.Padding = new Padding(0, 0, 10, 0);
        split.Panel2.Padding = new Padding(10, 0, 0, 0);

        // Keep the birth profile / workspace ratio when the window resizes, and
        // remember where the user drags the splitter to.
        var profileRatio = 0.56;
        var adjustingSplitter = false;
        var userDragging = false;
        void AdjustSplitter()
        {
            if (split.Width <= 700)
            {
                return;
            }

            adjustingSplitter = true;
            try
            {
                var minLeft = 420;
                var maxLeft = Math.Max(minLeft, split.Width - 360);
                split.SplitterDistance = Math.Clamp((int)(split.Width * profileRatio), minLeft, maxLeft);
            }
            catch (InvalidOperationException)
            {
                // Layout transition while the handle is being created.
            }
            finally
            {
                adjustingSplitter = false;
            }
        }

        split.HandleCreated += (_, _) => AdjustSplitter();
        split.SizeChanged += (_, _) => AdjustSplitter();
        split.SplitterMoving += (_, _) => userDragging = true;
        split.SplitterMoved += (_, _) =>
        {
            if (userDragging && !adjustingSplitter && split.Width > 700)
            {
                profileRatio = (double)split.SplitterDistance / split.Width;
            }
            userDragging = false;
        };

        _birthProfileGroup = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(11F, FontStyle.Bold),
            Padding = new Padding(10, 28, 10, 10),
            Text = "Birth profile",
        };
        var inputGroup = _birthProfileGroup;

        var table = new TableLayoutPanel
        {
            AutoScroll = true,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Padding = new Padding(20, 12, 20, 16),
            RowCount = 13,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        for (var i = 0; i < table.RowCount; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        }

        _nameTextBox.Text = "គំរូសូរ្យយាត្រ ២០២៤";
        ConfigureTextBox(_nameTextBox);
        ConfigureComboBox(_genderComboBox, GenderChoices.Select(choice => Localize(choice.English, choice.Khmer)));
        ConfigureDatePicker(_birthDatePicker, new DateTime(2024, 10, 28));
        ConfigureDatePicker(_birthTimePicker, new DateTime(2024, 10, 28, 20, 0, 12), DateTimePickerFormat.Time);
        ConfigureTextBox(_astronomicalMasterYearTextBox);
        _astronomicalMasterYearTextBox.ReadOnly = true;
        _astronomicalMasterYearTextBox.BackColor = Color.FromArgb(245, 248, 252);
        _astronomicalMasterYearTextBox.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _astronomicalMasterYearTextBox.ForeColor = BrandBlue;
        ConfigureComboBox(_countryComboBox, CountryChoices.Select(choice => Localize(choice.English, choice.Khmer)));
        ConfigureComboBox(_locationComboBox, Array.Empty<string>());
        _locationComboBox.DropDownWidth = 320;
        ConfigureTextBox(_internationalCountryTextBox);
        ConfigureTextBox(_internationalRegionTextBox);
        ConfigureTextBox(_latitudeTextBox);
        ConfigureTextBox(_longitudeTextBox);
        ConfigureTextBox(_timeZoneTextBox);
        ConfigureTextBox(_utcOverrideTextBox);

        _genderComboBox.SelectedIndex = 0;
        _countryComboBox.SelectedIndex = 0;
        _locationComboBox.SelectedIndexChanged += LocationComboBoxOnSelectedIndexChanged;
        _countryComboBox.SelectedIndexChanged += CountryComboBoxOnSelectedIndexChanged;
        _birthDatePicker.ValueChanged += (_, _) => UpdateMasterBridgeLabels();
        _birthTimePicker.ValueChanged += (_, _) => UpdateMasterBridgeLabels();
        _internationalCountryTextBox.TextChanged += (_, _) => UpdateMasterBridgeLabels();
        _internationalRegionTextBox.TextChanged += (_, _) => UpdateMasterBridgeLabels();
        _latitudeTextBox.TextChanged += (_, _) => UpdateMasterBridgeLabels();
        _longitudeTextBox.TextChanged += (_, _) => UpdateMasterBridgeLabels();
        _timeZoneTextBox.TextChanged += (_, _) => UpdateMasterBridgeLabels();
        _utcOverrideTextBox.TextChanged += (_, _) => UpdateMasterBridgeLabels();
        CountryComboBoxOnSelectedIndexChanged(_countryComboBox, EventArgs.Empty);

        AddField(table, 0, "Name", "ឈ្មោះ", _nameTextBox);
        AddField(table, 1, "Gender", "ភេទ", _genderComboBox);
        AddField(table, 2, "Birth date", "ថ្ងៃខែឆ្នាំកំណើត", _birthDatePicker);
        AddField(table, 3, "Astronomical / Master year (Auto)", "ឆ្នាំតារាសាស្ត្រ / ក.ស. (Auto)", _astronomicalMasterYearTextBox);
        AddField(table, 4, "Birth time", "ម៉ោងកំណើត", _birthTimePicker);
        AddField(table, 5, "Location mode / country", "របៀបទីតាំង / ប្រទេស", _countryComboBox);
        AddField(table, 6, "Cambodian province / city", "ក្រុង/ខេត្តកម្ពុជា", _locationComboBox);
        AddField(table, 7, "International country", "ប្រទេសអន្តរជាតិ", _internationalCountryTextBox);
        AddField(table, 8, "International region / state", "រដ្ឋ/តំបន់អន្តរជាតិ", _internationalRegionTextBox);
        AddField(table, 9, "Latitude", "រយៈទទឹង", _latitudeTextBox);
        AddField(table, 10, "Longitude", "រយៈបណ្តោយ", _longitudeTextBox);
        AddField(table, 11, "IANA time zone", "តំបន់ម៉ោង IANA", _timeZoneTextBox);
        AddField(table, 12, "UTC override (optional)", "UTC ប្តូរជំនួស (ជាជម្រើស)", _utcOverrideTextBox);

        _calculateButton.AutoSize = true;
        _calculateButton.Anchor = AnchorStyles.Right;
        _calculateButton.BackColor = BrandBlue;
        _calculateButton.FlatStyle = FlatStyle.Flat;
        _calculateButton.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _calculateButton.ForeColor = Color.White;
        _calculateButton.Margin = new Padding(3, 10, 20, 6);
        _calculateButton.MinimumSize = new Size(250, 46);
        _calculateButton.MaximumSize = new Size(330, 54);
        _calculateButton.Padding = new Padding(18, 8, 18, 8);
        _calculateButton.Text = "CALCULATE HOROSCOPE";
        _calculateButton.UseVisualStyleBackColor = false;
        _calculateButton.FlatAppearance.BorderSize = 0;
        _calculateButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 76, 128);
        _calculateButton.Click += CalculateButtonOnClick;

        // Fields scroll; the Calculate action stays pinned below them so it is
        // always reachable, even on short windows.
        var fieldsScroller = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
        };
        fieldsScroller.Controls.Add(table);
        var actionBar = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.FromArgb(248, 250, 253),
            ColumnCount = 1,
            Dock = DockStyle.Bottom,
            RowCount = 1,
        };
        actionBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        actionBar.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Border);
            eventArgs.Graphics.DrawLine(pen, 0, 0, actionBar.Width, 0);
        };
        actionBar.Controls.Add(_calculateButton, 0, 0);

        inputGroup.Controls.Add(fieldsScroller);
        inputGroup.Controls.Add(actionBar);
        split.Panel1.Controls.Add(inputGroup);
        split.Panel2.Controls.Add(BuildFoundationPreview());
        return split;
    }

    private Control BuildFoundationPreview()
    {
        _calculationWorkspaceGroup = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(11F, FontStyle.Bold),
            Padding = new Padding(12, 28, 12, 12),
            Text = "Calculation workspace",
        };
        var group = _calculationWorkspaceGroup;

        var scrollContainer = new Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
        };

        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            Padding = new Padding(8, 4, 8, 12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var statusCard = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = BrandBlueLight,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(12, 8, 12, 8),
        };
        statusCard.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Color.FromArgb(197, 216, 235));
            eventArgs.Graphics.DrawRectangle(pen, 0, 0, statusCard.Width - 1, statusCard.Height - 1);
        };
        var statusLayout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            RowCount = 2,
        };
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var phaseLabel = new Label
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            AutoSize = true,
            Font = _fontProvider.CreateBody(10.5F, FontStyle.Bold),
            ForeColor = BrandBlue,
            Text = "READY  •  Workbook-backed calculations",
        };
        RegisterLocalizedControl(phaseLabel, "READY  •  Workbook-backed calculations", "រួចរាល់  •  ការគណនាផ្អែកលើសៀវភៅការងារ");
        var activeLinkStatusLabel = new Label
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9F),
            ForeColor = TextSecondary,
            Text = "READY — Cambodia / Phnom Penh",
        };
        _masterBridgeLabels["activeLinkStatus"] = activeLinkStatusLabel;
        statusLayout.Controls.Add(phaseLabel, 0, 0);
        statusLayout.Controls.Add(activeLinkStatusLabel, 0, 1);
        statusCard.Controls.Add(statusLayout);
        layout.Controls.Add(statusCard);

        var yearBridgeGroup = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Top,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = BrandBlue,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(10, 22, 10, 8),
            Text = "Master Year Bridge  ·  Sheet 2 (A6:D7)",
        };
        RegisterLocalizedControl(
            yearBridgeGroup,
            "Master Year Bridge  ·  Sheet 2 (A6:D7)",
            "ស្ពានឆ្នាំមេ  ·  សន្លឹកទី ២ (A6:D7)");
        var yearTable = CreateBridgeKeyValueTable(4);
        AddBridgeRow(yearTable, 0, "ceYear", "Year CE / BCE", "ឆ្នាំ គ.ស. (+) / មុន គ.ស. (−)", "2024");
        AddBridgeRow(yearTable, 1, "astroYear", "Astronomical Year (Auto)", "ឆ្នាំតារាសាស្ត្រ (Auto)", "2024");
        AddBridgeRow(yearTable, 2, "ksYear", "Krom Sakaraj (Master Year)", "ក.ស. (Internal Master Year)", "5124");
        AddBridgeRow(yearTable, 3, "equivYear", "Year Equivalent", "សមមូលឆ្នាំ", "2024 គ.ស.");
        yearBridgeGroup.Controls.Add(yearTable);
        FitGroupToContent(yearBridgeGroup, yearTable);
        layout.Controls.Add(yearBridgeGroup);

        var locationBridgeGroup = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Top,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = BrandBlue,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(10, 22, 10, 8),
            Text = "Location Bridge  ·  Sheet 2 (D10:F15)",
        };
        RegisterLocalizedControl(
            locationBridgeGroup,
            "Location Bridge  ·  Sheet 2 (D10:F15)",
            "ស្ពានទីតាំង  ·  សន្លឹកទី ២ (D10:F15)");
        var locTable = CreateBridgeKeyValueTable(5);
        AddBridgeRow(locTable, 0, "activeLoc", "Active Location", "ទីតាំងសកម្ម", "កម្ពុជា / ភ្នំពេញ");
        AddBridgeRow(locTable, 1, "coordinates", "Coordinates (Lat / Long)", "កូអរដោនេ (Lat / Long)", "11.55, 104.92");
        AddBridgeRow(locTable, 2, "timeZoneUtc", "IANA / UTC", "តំបន់ម៉ោង / UTC", "Asia/Phnom_Penh / UTC 7.00");
        AddBridgeRow(locTable, 3, "calcChain", "Calculation Chain", "ខ្សែគណនា", "លគ្នា + តារាគ្រោះ + ឆាយាគ្រោះ (AUTO)");
        AddBridgeRow(locTable, 4, "linkStatus", "Link Status", "ស្ថានភាព Link", "READY — កម្ពុជា / ភ្នំពេញ");
        locationBridgeGroup.Controls.Add(locTable);
        FitGroupToContent(locationBridgeGroup, locTable);
        layout.Controls.Add(locationBridgeGroup);

        var refGroup = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Top,
            Font = _fontProvider.CreateBody(9F, FontStyle.Bold),
            ForeColor = TextLabel,
            Margin = new Padding(0, 0, 0, 4),
            Padding = new Padding(10, 20, 10, 8),
            Text = "System Reference  ·  ឯកសារយោង",
        };
        RegisterLocalizedControl(refGroup, "System Reference", "ឯកសារយោងប្រព័ន្ធ");
        var refTable = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Dock = DockStyle.Top,
            RowCount = 1,
        };
        refTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        refTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var refText = new Label
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            AutoSize = true,
            Font = _fontProvider.CreateBody(8.5F),
            ForeColor = TextSecondary,
            Text = "• គម្ពីរសូរ្យយាត្រ៥១០៣ ឆ្នាំ — សន្លឹកទី ២ «សូរ្យយាត្រ» (ចងរូបមន្តដោយលោកគ្រូ វ៉ាន់ ចាន់សារ៉ែន)\r\n" +
                   "• Swiss Ephemeris Lahiri geocentric positions with UTC instant\r\n" +
                   "• 27 Nakshatras with Pali names, 9 Nakshatra types, D1 / D3 / D9 charts",
        };
        refTable.Controls.Add(refText, 0, 0);
        refGroup.Controls.Add(refTable);
        FitGroupToContent(refGroup, refTable);
        layout.Controls.Add(refGroup);

        scrollContainer.Controls.Add(layout);
        group.Controls.Add(scrollContainer);
        return group;
    }

    private Control BuildResultPanel()
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(4),
            RowCount = 4,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var heading = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            RowCount = 2,
        };
        heading.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heading.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _resultTitleLabel.AutoSize = true;
        _resultTitleLabel.Font = _fontProvider.CreateDisplay(17F, FontStyle.Bold);
        _resultTitleLabel.ForeColor = TextPrimary;
        _resultTitleLabel.Text = "Calculation results";
        RegisterLocalizedControl(_resultTitleLabel, "Calculation results", "លទ្ធផលការគណនា");
        _resultSubtitleLabel.AutoSize = true;
        _resultSubtitleLabel.ForeColor = TextSecondary;
        _resultSubtitleLabel.Padding = new Padding(0, 2, 0, 0);
        _resultSubtitleLabel.Text = "Calculate a birth profile to populate this workspace.";
        RegisterLocalizedControl(_resultSubtitleLabel, "Calculate a birth profile to populate this workspace.", "សូមគណនាព័ត៌មានកំណើត ដើម្បីបង្ហាញលទ្ធផលនៅទីនេះ។");
        heading.Controls.Add(_resultTitleLabel, 0, 0);
        heading.Controls.Add(_resultSubtitleLabel, 0, 1);

        ConfigureGrid(_resultGrid);
        AddLocalizedColumn(_resultGrid, "field", "Field", "វាល");
        AddLocalizedColumn(_resultGrid, "value", "Value", "តម្លៃ");
        _resultGrid.Columns[0].FillWeight = 36;
        _resultGrid.Columns[1].FillWeight = 64;
        _resultGrid.Columns[0].MinimumWidth = 320;
        _resultGrid.Columns[1].MinimumWidth = 450;
        _resultGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _resultGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _resultGrid.Rows.Add("Status", "No result yet \u2014 enter a birth profile and click Calculate Horoscope.");

        ConfigureGrid(_planetGrid);
        _planetGrid.AllowUserToResizeColumns = true;
        _planetGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _planetGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _planetGrid.ScrollBars = ScrollBars.Both;
        _planetGrid.ColumnHeadersHeight = 44;
        _planetGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
        _planetGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
        _planetGrid.ShowCellToolTips = true;

        AddLocalizedColumn(_planetGrid, "body", "Planet / point", "តារាគ្រោះ");
        AddLocalizedColumn(_planetGrid, "longitude", "Total Longitude (arcmin)", "សំស្ផុដ (លិប្ដា)");
        AddLocalizedColumn(_planetGrid, "sign", "Sign", "រាសី");
        AddLocalizedColumn(_planetGrid, "deg", "Deg (°)", "អង្សា");
        AddLocalizedColumn(_planetGrid, "min", "Min (′)", "លិប្ដា");
        AddLocalizedColumn(_planetGrid, "sec", "Sec (″)", "ពិលិប្ដា");
        AddLocalizedColumn(_planetGrid, "nakshatraPada", "Nakshatra & Pada", "នក្ខត្តប្ញក្ស និង បាទ");
        AddLocalizedColumn(_planetGrid, "trueNakshatra", "True Nakshatra", "នក្ខត្តឫក្សពិត");
        AddLocalizedColumn(_planetGrid, "nakshatraType", "9 Nakshatra Types", "ឫក្ស ៩ ប្រការ");
        AddLocalizedColumn(_planetGrid, "d9", "Navamsha (D9)", "នវាង្ស");
        AddLocalizedColumn(_planetGrid, "d3", "Drekkana (D3)", "ត្រិយាង្ស");
        AddLocalizedColumn(_planetGrid, "linkStatus", "Link Status", "ស្ថានភាព Link");

        int[] planetWidths = [170, 220, 110, 95, 90, 95, 330, 165, 215, 215, 210, 450];
        for (var i = 0; i < planetWidths.Length; i++)
        {
            _planetGrid.Columns[i].MinimumWidth = planetWidths[i];
            _planetGrid.Columns[i].Width = planetWidths[i];
        }

        _planetGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _planetGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _planetGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _planetGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _planetGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _planetGrid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _planetGrid.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _planetGrid.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _planetGrid.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _planetGrid.Columns[9].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _planetGrid.Columns[10].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        _planetGrid.Columns[11].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        _planetGrid.Rows.Add("—", "—", "—", "—", "—", "—", "No calculation yet", "—", "—", "—", "—", "—");

        var resultTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(9.5F),
            ItemSize = new Size(0, 38),
            Padding = new Point(14, 6),
        };
        var summaryPage = new TabPage("Summary") { BackColor = Surface, Padding = new Padding(6) };
        var planetsPage = new TabPage("Planets & Points") { BackColor = Surface, Padding = new Padding(6) };
        _localizedTabs.Add((summaryPage, "Summary", "សង្ខេប"));
        _localizedTabs.Add((planetsPage, "Planets & Points", "ភព និងចំណុច"));
        summaryPage.Controls.Add(_resultGrid);
        planetsPage.Controls.Add(_planetGrid);
        resultTabs.TabPages.Add(summaryPage);
        resultTabs.TabPages.Add(planetsPage);

        var note = new Label
        {
            AutoSize = true,
            ForeColor = TextSecondary,
            Padding = new Padding(0, 12, 0, 0),
            Text = "All values are calculated from the entered birth date, time, location, and extracted workbook reference data.",
        };
        RegisterLocalizedControl(
            note,
            "All values are calculated from the entered birth date, time, location, and extracted workbook reference data.",
            "តម្លៃទាំងអស់គណនាតាមថ្ងៃខែឆ្នាំកំណើត ម៉ោង ទីតាំង និងទិន្នន័យយោងដែលបានស្រង់ចេញពីសៀវភៅការងារ។");

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(BuildResultsSummaryBar(), 0, 1);
        layout.Controls.Add(resultTabs, 0, 2);
        layout.Controls.Add(note, 0, 3);
        return layout;
    }

    private Control BuildResultsSummaryBar()
    {
        var panel = new Panel
        {
            BackColor = BrandBlueLight,
            Dock = DockStyle.Fill,
            Height = 42,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(12, 6, 12, 6),
        };
        panel.Paint += (_, eventArgs) =>
        {
            using var pen = new Pen(Color.FromArgb(197, 216, 235));
            eventArgs.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        var label = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = BrandBlue,
            Text = "RESULT WORKSPACE  •  Summary, planets, charts, calendar, and interpretation are available in the tabs above.",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        RegisterLocalizedControl(
            label,
            "RESULT WORKSPACE  •  Summary, planets, charts, calendar, and interpretation are available in the tabs above.",
            "ផ្ទាំងលទ្ធផល  •  សង្ខេប ភព តារាង ប្រតិទិន និងការបកស្រាយមាននៅក្នុងផ្ទាំងខាងលើ។");
        panel.Controls.Add(label);
        return panel;
    }

    private Control BuildNakshatraPanel()
    {
        ConfigureGrid(_nakshatraGrid);
        _nakshatraGrid.AllowUserToResizeColumns = true;
        AddLocalizedColumn(_nakshatraGrid, "body", "Body", "ភព");
        AddLocalizedColumn(_nakshatraGrid, "longitude", "Longitude (arcminutes)", "រយៈបណ្តោយ (នាទីធ្នូ)");
        AddLocalizedColumn(_nakshatraGrid, "nakshatra", "Nakshatra", "នក្ខត្តឫក្ស");
        AddLocalizedColumn(_nakshatraGrid, "pada", "Pada", "បាទា");
        _nakshatraGrid.Columns[0].MinimumWidth = 160;
        _nakshatraGrid.Columns[1].MinimumWidth = 200;
        _nakshatraGrid.Columns[2].MinimumWidth = 220;
        _nakshatraGrid.Columns[3].MinimumWidth = 90;
        _nakshatraGrid.Rows.Add("\u2014", "\u2014", "Calculate a horoscope to load positions", "\u2014");
        return BuildDataPanel(
            "Nakshatra and Pada",
            "នក្ខត្តឫក្ស និង បាទា",
            "Workbook-derived lunar mansion and quarter for the Ascendant, planets, and additional points.",
            "នក្ខត្តឫក្ស និងបាទារបស់លគ្គនៈ ភព និងចំណុចបន្ថែម ដែលបានគណនាតាមសៀវភៅការងារ។",
            _nakshatraGrid);
    }

    private Control BuildCalendarPanel()
    {
        ConfigureGrid(_calendarGrid);
        _calendarGrid.AllowUserToResizeColumns = true;
        AddLocalizedColumn(_calendarGrid, "field", "Calendar field", "វាលប្រតិទិន");
        AddLocalizedColumn(_calendarGrid, "value", "Value", "តម្លៃ");
        _calendarGrid.Columns[0].FillWeight = 38;
        _calendarGrid.Columns[1].FillWeight = 62;
        _calendarGrid.Columns[0].MinimumWidth = 280;
        _calendarGrid.Columns[1].MinimumWidth = 380;
        _calendarGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _calendarGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        _calendarGrid.Rows.Add("Status", "Calculate a horoscope to load Khmer calendar data.");
        return BuildDataPanel(
            "Khmer calendar",
            "ប្រតិទិនខ្មែរ",
            "Calendar, Ahargana, Sankranta, lunar-year, and Suriyayātra bridge values from the workbook chain.",
            "តម្លៃប្រតិទិន អហរគណ សង្ក្រាន្ត ឆ្នាំចន្ទគតិ និងសូរ្យយាត្រា ដែលបានមកពីខ្សែគណនារបស់សៀវភៅការងារ។",
            _calendarGrid);
    }

    private Control BuildAutomaticCalendarPanel()
    {
        var root = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 2,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
            RowCount = 2,
        };
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateDisplay(14F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 0, 0, 6),
            Text = "Suriyayātra Automatic Calendar",
        };
        RegisterLocalizedControl(title, "Suriyayātra Automatic Calendar", "ប្រតិទិនសូរ្យយាត្រស្វ័យប្រវត្តិ");
        header.Controls.Add(title, 0, 0);

        var bar = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = Color.FromArgb(238, 246, 237),
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 4, 0, 0),
            Padding = new Padding(10, 8, 10, 8),
            WrapContents = true,
        };

        var yearLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 5, 4, 0),
            Text = "Year CE (+) / BCE (−):",
        };
        RegisterLocalizedControl(yearLabel, "Year CE (+) / BCE (−):", "ឆ្នាំ គ.ស. (+) / មុន គ.ស. (−)");

        var today = DateTime.Today;
        _autoCalendarYearTextBox.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _autoCalendarYearTextBox.Margin = new Padding(0, 2, 14, 0);
        _autoCalendarYearTextBox.Size = new Size(85, 28);
        _autoCalendarYearTextBox.Text = today.Year.ToString(CultureInfo.InvariantCulture);
        _autoCalendarYearTextBox.TextAlign = HorizontalAlignment.Center;
        _autoCalendarYearTextBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PopulateAutomaticCalendar();
            }
        };

        var ksLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 5, 4, 0),
            Text = "K.S. (Auto):",
        };
        RegisterLocalizedControl(ksLabel, "K.S. (Auto):", "ក.ស. (Auto)");

        _autoCalendarKsTextBox.BackColor = Color.White;
        _autoCalendarKsTextBox.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _autoCalendarKsTextBox.ForeColor = Color.FromArgb(30, 90, 45);
        _autoCalendarKsTextBox.Margin = new Padding(0, 2, 14, 0);
        _autoCalendarKsTextBox.ReadOnly = true;
        _autoCalendarKsTextBox.Size = new Size(75, 28);
        _autoCalendarKsTextBox.Text = (today.Year + 3100).ToString(CultureInfo.InvariantCulture);
        _autoCalendarKsTextBox.TextAlign = HorizontalAlignment.Center;

        var monthLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 5, 4, 0),
            Text = "Select Month:",
        };
        RegisterLocalizedControl(monthLabel, "Select Month:", "ជ្រើសខែ");

        _autoCalendarMonthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _autoCalendarMonthComboBox.Font = _fontProvider.CreateBody(10F);
        _autoCalendarMonthComboBox.Margin = new Padding(0, 2, 10, 0);
        _autoCalendarMonthComboBox.Size = new Size(110, 28);
        _autoCalendarMonthComboBox.Items.AddRange(AutomaticCalendarCalculator.GregorianMonthKhmerNames);
        _autoCalendarMonthComboBox.SelectedIndex = today.Month - 1; // Current month
        _autoCalendarMonthComboBox.SelectedIndexChanged += (_, _) => PopulateAutomaticCalendar();

        var hintLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(9F),
            ForeColor = Color.FromArgb(90, 110, 90),
            Margin = new Padding(0, 6, 12, 0),
            Text = "▼ Click to switch month",
        };
        RegisterLocalizedControl(hintLabel, "▼ Click to switch month", "▼ ចុចដើម្បីប្តូរខែទាំង ១២");

        _calculateAutoCalendarButton.AutoSize = true;
        _calculateAutoCalendarButton.BackColor = BrandBlue;
        _calculateAutoCalendarButton.FlatAppearance.BorderSize = 0;
        _calculateAutoCalendarButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 76, 128);
        _calculateAutoCalendarButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 56, 96);
        _calculateAutoCalendarButton.FlatStyle = FlatStyle.Flat;
        _calculateAutoCalendarButton.Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold);
        _calculateAutoCalendarButton.ForeColor = Color.White;
        _calculateAutoCalendarButton.Cursor = Cursors.Hand;
        _calculateAutoCalendarButton.Margin = new Padding(0, 1, 0, 0);
        _calculateAutoCalendarButton.Padding = new Padding(14, 5, 14, 5);
        _calculateAutoCalendarButton.Text = "Calculate";
        _calculateAutoCalendarButton.Click += (_, _) => PopulateAutomaticCalendar();
        RegisterLocalizedControl(_calculateAutoCalendarButton, "Calculate", "គណនា");

        _autoCalendarTodayButton.AutoSize = true;
        _autoCalendarTodayButton.BackColor = Color.FromArgb(235, 244, 234);
        _autoCalendarTodayButton.FlatAppearance.BorderSize = 0;
        _autoCalendarTodayButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(215, 235, 214);
        _autoCalendarTodayButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(195, 225, 194);
        _autoCalendarTodayButton.FlatStyle = FlatStyle.Flat;
        _autoCalendarTodayButton.Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold);
        _autoCalendarTodayButton.ForeColor = Color.FromArgb(30, 90, 45);
        _autoCalendarTodayButton.Cursor = Cursors.Hand;
        _autoCalendarTodayButton.Margin = new Padding(6, 1, 0, 0);
        _autoCalendarTodayButton.Padding = new Padding(14, 5, 14, 5);
        _autoCalendarTodayButton.Text = "Today";
        _autoCalendarTodayButton.Click += (_, _) =>
        {
            var now = DateTime.Today;
            _autoCalendarYearTextBox.Text = now.Year.ToString(CultureInfo.InvariantCulture);
            _autoCalendarMonthComboBox.SelectedIndex = now.Month - 1;
            PopulateAutomaticCalendar();
        };
        RegisterLocalizedControl(_autoCalendarTodayButton, "Today", "ថ្ងៃនេះ");

        bar.Controls.Add(yearLabel);
        bar.Controls.Add(_autoCalendarYearTextBox);
        bar.Controls.Add(ksLabel);
        bar.Controls.Add(_autoCalendarKsTextBox);
        bar.Controls.Add(monthLabel);
        bar.Controls.Add(_autoCalendarMonthComboBox);
        bar.Controls.Add(hintLabel);
        bar.Controls.Add(_calculateAutoCalendarButton);
        bar.Controls.Add(_autoCalendarTodayButton);
        header.Controls.Add(bar, 0, 1);
        root.Controls.Add(header, 0, 0);

        ConfigureGrid(_autoCalendarGrid);
        _autoCalendarGrid.AllowUserToResizeColumns = true;
        _autoCalendarGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        _autoCalendarGrid.ScrollBars = ScrollBars.Both;
        _autoCalendarGrid.RowTemplate.Height = 36;
        _autoCalendarGrid.ColumnHeadersHeight = 42;
        _autoCalendarGrid.DefaultCellStyle.Font = _fontProvider.CreateBody(9.5F);
        _autoCalendarGrid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        _autoCalendarGrid.ColumnHeadersDefaultCellStyle.Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold);
        _autoCalendarGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        _autoCalendarGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 244, 234);
        _autoCalendarGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
        _autoCalendarGrid.EnableHeadersVisualStyles = false;
        _autoCalendarGrid.ShowCellToolTips = true;

        AddLocalizedColumn(_autoCalendarGrid, "no", "No.", "ល.រ.");
        AddLocalizedColumn(_autoCalendarGrid, "weekday", "Weekday", "ថ្ងៃ");
        AddLocalizedColumn(_autoCalendarGrid, "day", "Day", "ទី");
        AddLocalizedColumn(_autoCalendarGrid, "month", "Month", "ខែ");
        AddLocalizedColumn(_autoCalendarGrid, "ce", "CE", "គ.ស.");
        AddLocalizedColumn(_autoCalendarGrid, "be", "BE", "ព.ស.");
        AddLocalizedColumn(_autoCalendarGrid, "ms", "MS", "ម.ស.");
        AddLocalizedColumn(_autoCalendarGrid, "cs", "CS", "ច.ស.");
        AddLocalizedColumn(_autoCalendarGrid, "ks", "KS", "ក.ស.");
        AddLocalizedColumn(_autoCalendarGrid, "lunarDay", "Lunar Day", "តិថីចន្ទគតិ");
        AddLocalizedColumn(_autoCalendarGrid, "tithiName", "Tithi Name", "ឈ្មោះតិថី");
        AddLocalizedColumn(_autoCalendarGrid, "lunarMonth", "Lunar Month", "ខែចន្ទគតិ");
        AddLocalizedColumn(_autoCalendarGrid, "animalYear", "Animal Year", "ឆ្នាំសត្វ");
        AddLocalizedColumn(_autoCalendarGrid, "sesa", "Sesa-Kala-Yoga", "សេសកាលយោគ");
        AddLocalizedColumn(_autoCalendarGrid, "yuga", "Yuga", "យុគ");
        AddLocalizedColumn(_autoCalendarGrid, "samvatsara", "Samvatsara Name", "ឈ្មោះសំវត្សរ៍");
        AddLocalizedColumn(_autoCalendarGrid, "meaning", "Meaning", "អត្ថន័យ");

        int[] widths = [65, 115, 55, 85, 70, 70, 70, 70, 70, 115, 125, 135, 90, 130, 65, 145];
        for (var i = 0; i < widths.Length; i++)
        {
            _autoCalendarGrid.Columns[i].MinimumWidth = widths[i];
            _autoCalendarGrid.Columns[i].Width = widths[i];
            _autoCalendarGrid.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        _autoCalendarGrid.Columns[16].MinimumWidth = 260;
        _autoCalendarGrid.Columns[16].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _autoCalendarGrid.Columns[16].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        root.Controls.Add(_autoCalendarGrid, 0, 1);
        return root;
    }

    private void PopulateAutomaticCalendar()
    {
        var yearText = _autoCalendarYearTextBox.Text.Trim();
        if (!KhmerCalendarCalculator.TryParseYear(yearText, out var parsedYear) || parsedYear == 0)
        {
            _autoCalendarKsTextBox.Text = "-";
            _errorProvider.SetError(_autoCalendarYearTextBox, Localize("Invalid year. Enter CE or BCE year.", "ឆ្នាំមិនត្រឹមត្រូវ។ សូមបញ្ចូលឆ្នាំ គ.ស. ឬ មុន គ.ស."));
            return;
        }

        _errorProvider.SetError(_autoCalendarYearTextBox, string.Empty);
        var today = DateTime.Today;
        var selectedMonth = _autoCalendarMonthComboBox.SelectedIndex + 1;
        if (selectedMonth is < 1 or > 12)
        {
            selectedMonth = today.Month;
        }

        try
        {
            var result = _automaticCalendarCalculator.CalculateMonth(parsedYear, selectedMonth);
            _autoCalendarKsTextBox.Text = result.KromSakarajAuto.ToString(CultureInfo.InvariantCulture);

            _autoCalendarGrid.Rows.Clear();
            var isCurrentMonth = parsedYear == today.Year && selectedMonth == today.Month;
            var todayRowIdx = -1;

            foreach (var row in result.Days)
            {
                var rowIdx = _autoCalendarGrid.Rows.Add(
                    row.DayNumber,
                    row.Weekday,
                    row.Day,
                    row.MonthName,
                    row.CeYear,
                    row.BuddhistYear,
                    row.MahaSakaraj,
                    row.ChulaSakaraj,
                    row.KromSakaraj,
                    row.LunarDay,
                    row.TithiName,
                    row.LunarMonth,
                    row.AnimalYear,
                    row.SesaKalaYoga,
                    row.Yuga,
                    row.SamvatsaraName,
                    row.Meaning);

                if (isCurrentMonth && row.Day == today.Day)
                {
                    todayRowIdx = rowIdx;
                    var gridRow = _autoCalendarGrid.Rows[rowIdx];
                    gridRow.DefaultCellStyle.BackColor = Color.FromArgb(255, 246, 214);
                    gridRow.DefaultCellStyle.Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold);
                }
            }

            if (todayRowIdx >= 0)
            {
                _autoCalendarGrid.ClearSelection();
                _autoCalendarGrid.Rows[todayRowIdx].Selected = true;
                _autoCalendarGrid.FirstDisplayedScrollingRowIndex = Math.Max(0, todayRowIdx - 2);
            }
            else
            {
                _autoCalendarGrid.ClearSelection();
            }
        }
        catch (Exception ex)
        {
            _autoCalendarKsTextBox.Text = "-";
            Trace.WriteLine($"Error calculating automatic calendar: {ex.Message}");
        }
    }

    private Control BuildSearchDatePanel()
    {
        var root = new TableLayoutPanel
        {
            AutoScroll = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 2,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
            RowCount = 3,
        };
        var title = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateDisplay(17F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "Search Date",
        };
        RegisterLocalizedControl(title, "Search Date", "ស្វែងរក ថ្ងៃខែឆ្នាំ");

        var subtitle = new Label
        {
            AutoSize = true,
            ForeColor = TextSecondary,
            Text = "Daily Khmer calendar lookup according to Sheet 31 (ស្វែងរក ថ្ងៃខែឆ្នាំ) of the master workbook.",
        };
        RegisterLocalizedControl(
            subtitle,
            "Daily Khmer calendar lookup according to Sheet 31 (ស្វែងរក ថ្ងៃខែឆ្នាំ) of the master workbook.",
            "ការស្វែងរក និងផ្ទៀងផ្ទាត់ថ្ងៃខែឆ្នាំតាមសន្លឹក ៣១ (ស្វែងរក ថ្ងៃខែឆ្នាំ) នៃគម្ពីរសៀវភៅការងារ។");

        header.Controls.Add(title, 0, 0);
        header.Controls.Add(subtitle, 0, 1);

        var inputCard = new GroupBox
        {
            AutoSize = true,
            BackColor = Surface,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 4),
            Padding = new Padding(12, 16, 12, 10),
            Text = "Search Criteria",
        };
        RegisterLocalizedControl(inputCard, "Search Criteria", "លក្ខខណ្ឌស្វែងរក");

        var inputTable = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 5,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            RowCount = 3,
        };
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 225));
        inputTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        inputTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

        // Row 0: Day & Year Rule
        var dayLabel = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "Day:",
        };
        RegisterLocalizedControl(dayLabel, "Day:", "ថ្ងៃទី");

        _searchDateDayComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _searchDateDayComboBox.Font = _fontProvider.CreateBody(10F);
        _searchDateDayComboBox.Width = 110;
        _searchDateDayComboBox.Anchor = AnchorStyles.Left;
        for (var d = 1; d <= 31; d++)
        {
            _searchDateDayComboBox.Items.Add(d);
        }
        _searchDateDayComboBox.SelectedIndex = 0; // Day 1 default (01-01-2000)
        _searchDateDayComboBox.SelectedIndexChanged += (_, _) => PopulateSearchDate();

        var instructionLabel = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(10, 0, 0, 0),
            Text = "Year Rule:",
        };
        RegisterLocalizedControl(instructionLabel, "Year Rule:", "របៀបវាយឆ្នាំ");

        var instructionBadge = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            BackColor = Color.FromArgb(226, 240, 217),
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(20, 60, 20),
            Padding = new Padding(8, 4, 8, 4),
            Text = "គ.ស. = + / មុន គ.ស. = −",
        };

        inputTable.Controls.Add(dayLabel, 0, 0);
        inputTable.Controls.Add(_searchDateDayComboBox, 1, 0);
        inputTable.Controls.Add(instructionLabel, 2, 0);
        inputTable.Controls.Add(instructionBadge, 3, 0);

        // Row 1: Month & Example
        var monthLabel = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "Month:",
        };
        RegisterLocalizedControl(monthLabel, "Month:", "ខែ");

        _searchDateMonthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _searchDateMonthComboBox.Font = _fontProvider.CreateBody(10F);
        _searchDateMonthComboBox.Width = 110;
        _searchDateMonthComboBox.Anchor = AnchorStyles.Left;
        _searchDateMonthComboBox.Items.AddRange(AutomaticCalendarCalculator.GregorianMonthKhmerNames);
        _searchDateMonthComboBox.SelectedIndex = 0; // January default (01-01-2000)
        _searchDateMonthComboBox.SelectedIndexChanged += (_, _) =>
        {
            AdjustSearchDateDays();
            PopulateSearchDate();
        };

        var exampleLabel = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(10, 0, 0, 0),
            Text = "Example:",
        };
        RegisterLocalizedControl(exampleLabel, "Example:", "ឧទាហរណ៍");

        var exampleBadge = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            BackColor = Color.FromArgb(226, 240, 217),
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(20, 60, 20),
            Padding = new Padding(8, 4, 8, 4),
            Text = "2026 ឬ -644",
        };

        inputTable.Controls.Add(monthLabel, 0, 1);
        inputTable.Controls.Add(_searchDateMonthComboBox, 1, 1);
        inputTable.Controls.Add(exampleLabel, 2, 1);
        inputTable.Controls.Add(exampleBadge, 3, 1);

        // Row 2: Year CE/BCE & Auto KS + Search Button
        var yearLabel = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "Year CE:",
        };
        RegisterLocalizedControl(yearLabel, "Year CE:", "ឆ្នាំ គ.ស.");

        _searchDateYearTextBox.Anchor = AnchorStyles.Left;
        _searchDateYearTextBox.BackColor = Color.FromArgb(255, 248, 218);
        _searchDateYearTextBox.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _searchDateYearTextBox.Margin = new Padding(0, 2, 0, 2);
        _searchDateYearTextBox.Size = new Size(110, 28);
        _searchDateYearTextBox.Text = "2000"; // 01-01-2000
        _searchDateYearTextBox.TextAlign = HorizontalAlignment.Center;
        _searchDateYearTextBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PopulateSearchDate();
            }
        };

        var ksLabel = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(10, 0, 0, 0),
            Text = "K.S. (Auto):",
        };
        RegisterLocalizedControl(ksLabel, "K.S. (Auto):", "ក.ស. (Auto)");

        _searchDateKsTextBox.Anchor = AnchorStyles.Left;
        _searchDateKsTextBox.BackColor = Color.White;
        _searchDateKsTextBox.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _searchDateKsTextBox.ForeColor = Color.FromArgb(30, 90, 45);
        _searchDateKsTextBox.Margin = new Padding(0, 2, 0, 2);
        _searchDateKsTextBox.ReadOnly = true;
        _searchDateKsTextBox.Size = new Size(110, 28);
        _searchDateKsTextBox.Text = "5100"; // 01-01-2000
        _searchDateKsTextBox.TextAlign = HorizontalAlignment.Center;

        _searchDateButton.Anchor = AnchorStyles.Left;
        _searchDateButton.AutoSize = false;
        _searchDateButton.Size = new Size(130, 38);
        _searchDateButton.BackColor = BrandBlue;
        _searchDateButton.FlatAppearance.BorderSize = 0;
        _searchDateButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 76, 128);
        _searchDateButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 56, 96);
        _searchDateButton.FlatStyle = FlatStyle.Flat;
        _searchDateButton.Font = _fontProvider.CreateBody(10.5F, FontStyle.Bold);
        _searchDateButton.ForeColor = Color.White;
        _searchDateButton.Cursor = Cursors.Hand;
        _searchDateButton.Margin = new Padding(16, 2, 0, 2);
        _searchDateButton.Padding = new Padding(0);
        _searchDateButton.TextAlign = ContentAlignment.MiddleCenter;
        _searchDateButton.Text = "SEARCH";
        _searchDateButton.Click += (_, _) => PopulateSearchDate();
        RegisterLocalizedControl(_searchDateButton, "SEARCH", "ស្វែងរក");

        inputTable.Controls.Add(yearLabel, 0, 2);
        inputTable.Controls.Add(_searchDateYearTextBox, 1, 2);
        inputTable.Controls.Add(ksLabel, 2, 2);
        inputTable.Controls.Add(_searchDateKsTextBox, 3, 2);
        inputTable.Controls.Add(_searchDateButton, 4, 2);

        inputCard.Controls.Add(inputTable);
        header.Controls.Add(inputCard, 0, 2);
        root.Controls.Add(header, 0, 0);

        var resultTabs = new TabControl
        {
            Appearance = TabAppearance.Normal,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(9.5F),
            ItemSize = new Size(160, 32),
            Margin = new Padding(0, 4, 0, 0),
            Padding = new Point(14, 4),
        };

        var cardsTab = new TabPage("Summary Cards")
        {
            BackColor = Surface,
            Padding = new Padding(4),
        };
        _localizedTabs.Add((cardsTab, "Summary Cards", "កាតព័ត៌មាន"));
        cardsTab.Controls.Add(BuildSearchDateCards());

        var gridTab = new TabPage("Sheet 31 Table")
        {
            BackColor = Surface,
            Padding = new Padding(4),
        };
        _localizedTabs.Add((gridTab, "Sheet 31 Table", "តារាងសន្លឹក ៣១"));

        ConfigureGrid(_searchDateGrid);
        _searchDateGrid.AllowUserToResizeColumns = true;
        _searchDateGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _searchDateGrid.RowTemplate.Height = 36;
        _searchDateGrid.ColumnHeadersHeight = 40;
        _searchDateGrid.DefaultCellStyle.Font = _fontProvider.CreateBody(10F);
        _searchDateGrid.DefaultCellStyle.Padding = new Padding(8, 3, 8, 3);
        _searchDateGrid.ColumnHeadersDefaultCellStyle.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _searchDateGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 244, 234);
        _searchDateGrid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;

        AddLocalizedColumn(_searchDateGrid, "field", "Information", "ព័ត៌មាន");
        AddLocalizedColumn(_searchDateGrid, "value", "Value", "តម្លៃ");
        _searchDateGrid.Columns[0].FillWeight = 40;
        _searchDateGrid.Columns[1].FillWeight = 60;
        _searchDateGrid.Columns[0].MinimumWidth = 240;
        _searchDateGrid.Columns[1].MinimumWidth = 360;
        _searchDateGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _searchDateGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

        gridTab.Controls.Add(_searchDateGrid);

        resultTabs.TabPages.Add(cardsTab);
        resultTabs.TabPages.Add(gridTab);

        root.Controls.Add(resultTabs, 0, 1);
        return root;
    }

    private Control BuildSearchDateCards()
    {
        var container = new TableLayoutPanel
        {
            AutoScroll = true,
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 1,
        };
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var card1 = BuildSearchDateGroup(
            "Universal Solar Calendar",
            "សុរិយគតិសកល",
            [
                ("solarFullDate", "Full Date", "កាលបរិច្ឆេទពេញ"),
                ("solarWeekday", "Weekday", "ថ្ងៃសប្តាហ៍"),
                ("solarDay", "Day", "ថ្ងៃទី"),
                ("solarMonth", "Month", "ខែ"),
                ("solarYear", "Year", "ឆ្នាំ"),
            ]);

        var card2 = BuildSearchDateGroup(
            "Lunar Calendar",
            "ចន្ទគតិ",
            [
                ("lunarWeekday", "Weekday", "ថ្ងៃសប្តាហ៍"),
                ("lunarDay", "Lunar Day / Phase", "តិថីចន្ទគតិ"),
                ("tithiName", "Tithi Name", "ឈ្មោះតិថី"),
                ("lunarMonth", "Lunar Month", "ខែចន្ទគតិ"),
                ("animalYear", "Animal Year", "ឆ្នាំនក្សត្រ"),
            ]);

        var card3 = BuildSearchDateGroup(
            "Other Eras / Sakaraj",
            "ឆ្នាំផ្សេងៗ",
            [
                ("be", "Buddhist Era (BE)", "ព.ស."),
                ("ms", "Maha Sakaraj (MS)", "ម.ស."),
                ("cs", "Chula Sakaraj (CS)", "ច.ស."),
                ("ks", "Krom Sakaraj (KS)", "ក.ស."),
                ("sesa", "Sesa-Kala-Yoga", "សេសកាលយោគ"),
            ]);

        container.Controls.Add(card1, 0, 0);
        container.Controls.Add(card2, 1, 0);
        container.Controls.Add(card3, 2, 0);

        return container;
    }

    private Control BuildSearchDateGroup(
        string title,
        string khmerTitle,
        IReadOnlyList<(string Key, string English, string Khmer)> fields)
    {
        var group = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(10.5F, FontStyle.Bold),
            ForeColor = BrandBlue,
            Margin = new Padding(4),
            Padding = new Padding(12, 24, 12, 10),
            Text = title,
        };
        RegisterLocalizedControl(group, title, khmerTitle);

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0),
            RowCount = fields.Count,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));

        for (var index = 0; index < fields.Count; index++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            var field = fields[index];
            var fieldLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = _fontProvider.CreateBody(9.5F),
                ForeColor = TextPrimary,
                Margin = new Padding(4, 2, 4, 2),
                Text = field.English,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            RegisterLocalizedControl(fieldLabel, field.English, field.Khmer);

            var valueLabel = new Label
            {
                AutoSize = false,
                BackColor = field.Key switch
                {
                    "solarFullDate" => Color.FromArgb(232, 241, 250),
                    "lunarDay" or "tithiName" => Color.FromArgb(255, 248, 218),
                    "be" or "ks" => Color.FromArgb(235, 247, 235),
                    _ => Color.White,
                },
                Dock = DockStyle.Fill,
                Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Margin = new Padding(0, 2, 0, 2),
                Padding = new Padding(8, 2, 8, 2),
                Text = "—",
                TextAlign = ContentAlignment.MiddleLeft,
            };

            _searchDateSummaryLabels[field.Key] = valueLabel;
            table.Controls.Add(fieldLabel, 0, index);
            table.Controls.Add(valueLabel, 1, index);
        }

        group.Controls.Add(table);
        return group;
    }

    private void AdjustSearchDateDays()
    {
        var yearText = _searchDateYearTextBox.Text.Trim();
        var year = KhmerCalendarCalculator.TryParseYear(yearText, out var parsedYear) && parsedYear != 0
            ? parsedYear
            : 2000;
        var month = _searchDateMonthComboBox.SelectedIndex + 1;
        if (month is < 1 or > 12)
        {
            month = 1;
        }

        var astroYear = year < 0 ? year + 1 : year;
        var maxDays = (month == 2)
            ? (astroYear % 400 == 0 || (astroYear % 4 == 0 && astroYear % 100 != 0) ? 29 : 28)
            : (month is 4 or 6 or 9 or 11 ? 30 : 31);

        var currentDay = _searchDateDayComboBox.SelectedIndex + 1;
        if (currentDay < 1)
        {
            currentDay = 1;
        }
        if (currentDay > maxDays)
        {
            currentDay = maxDays;
        }

        _searchDateDayComboBox.BeginUpdate();
        _searchDateDayComboBox.Items.Clear();
        for (var d = 1; d <= maxDays; d++)
        {
            _searchDateDayComboBox.Items.Add(d);
        }
        _searchDateDayComboBox.SelectedIndex = currentDay - 1;
        _searchDateDayComboBox.EndUpdate();
    }

    private void PopulateSearchDate()
    {
        _errorProvider.SetError(_searchDateYearTextBox, string.Empty);
        var yearText = _searchDateYearTextBox.Text.Trim();
        if (!KhmerCalendarCalculator.TryParseYear(yearText, out var parsedYear) || parsedYear == 0)
        {
            _searchDateKsTextBox.Text = "-";
            _errorProvider.SetError(_searchDateYearTextBox, Localize("Invalid year. Enter CE or BCE year.", "ឆ្នាំមិនត្រឹមត្រូវ។ សូមបញ្ចូលឆ្នាំ គ.ស. ឬ មុន គ.ស."));
            return;
        }

        var selectedMonth = _searchDateMonthComboBox.SelectedIndex + 1;
        if (selectedMonth is < 1 or > 12)
        {
            selectedMonth = 1;
        }

        var selectedDay = _searchDateDayComboBox.SelectedIndex + 1;
        if (selectedDay < 1)
        {
            selectedDay = 1;
        }

        try
        {
            var result = _automaticCalendarCalculator.SearchDate(parsedYear, selectedMonth, selectedDay);
            _searchDateKsTextBox.Text = result.KromSakarajAuto.ToString(CultureInfo.InvariantCulture);

            _searchDateGrid.Rows.Clear();

            void AddSectionHeader(string englishTitle, string khmerTitle, string value = "")
            {
                var rowIdx = _searchDateGrid.Rows.Add(Localize(englishTitle, khmerTitle), value);
                var row = _searchDateGrid.Rows[rowIdx];
                row.DefaultCellStyle.BackColor = Color.FromArgb(235, 244, 234);
                row.DefaultCellStyle.Font = _fontProvider.CreateBody(10.5F, FontStyle.Bold);
                row.DefaultCellStyle.ForeColor = BrandBlue;
            }

            void AddDataRow(string englishLabel, string khmerLabel, object value)
            {
                _searchDateGrid.Rows.Add(Localize(englishLabel, khmerLabel), value.ToString() ?? string.Empty);
            }

            void AddSpacer()
            {
                var rowIdx = _searchDateGrid.Rows.Add(string.Empty, string.Empty);
                _searchDateGrid.Rows[rowIdx].Height = 12;
            }

            // Section 1: Solar
            AddSectionHeader("Universal Solar Calendar", "សុរិយគតិសកល", result.SolarFullDate);
            AddDataRow("Weekday", "ថ្ងៃសប្តាហ៍", result.SolarWeekday);
            AddDataRow("Day", "ថ្ងៃទី", result.SolarDay);
            AddDataRow("Month", "ខែ", result.SolarMonth);
            AddDataRow("Year", "ឆ្នាំ", result.SolarYear);

            AddSpacer();

            // Section 2: Lunar
            AddSectionHeader("Lunar Calendar", "ចន្ទគតិ", string.Empty);
            AddDataRow("Weekday", "ថ្ងៃសប្តាហ៍", result.LunarWeekday);
            AddDataRow("Lunar Day", "តិថីចន្ទគតិ", result.LunarDay);
            AddDataRow("Tithi Name", "ឈ្មោះតិថី", result.TithiName);
            AddDataRow("Lunar Month", "ខែចន្ទគតិ", result.LunarMonth);
            AddDataRow("Animal Year", "ឆ្នាំនក្សត្រ", result.AnimalYear);

            AddSpacer();

            // Section 3: Other Eras
            AddSectionHeader("Other Eras", "ឆ្នាំផ្សេងៗ", string.Empty);
            AddDataRow("Buddhist Era (BE)", "ព.ស.", result.BuddhistYear);
            AddDataRow("Maha Sakaraj (MS)", "ម.ស.", result.MahaSakaraj);
            AddDataRow("Chula Sakaraj (CS)", "ច.ស.", result.ChulaSakaraj);
            AddDataRow("Krom Sakaraj (KS)", "ក.ស.", result.KromSakaraj);
            AddDataRow("Sesa Kala Yoga", "សេសកាលយោគ", result.SesaKalaYoga);

            _searchDateGrid.ClearSelection();

            // Update Summary Cards
            SetSearchSummaryValue("solarFullDate", result.SolarFullDate);
            SetSearchSummaryValue("solarWeekday", result.SolarWeekday);
            SetSearchSummaryValue("solarDay", result.SolarDay.ToString(CultureInfo.InvariantCulture));
            SetSearchSummaryValue("solarMonth", result.SolarMonth);
            SetSearchSummaryValue("solarYear", result.SolarYear);

            SetSearchSummaryValue("lunarWeekday", result.LunarWeekday);
            SetSearchSummaryValue("lunarDay", result.LunarDay);
            SetSearchSummaryValue("tithiName", result.TithiName);
            SetSearchSummaryValue("lunarMonth", result.LunarMonth);
            SetSearchSummaryValue("animalYear", result.AnimalYear);

            SetSearchSummaryValue("be", result.BuddhistYear.ToString(CultureInfo.InvariantCulture));
            SetSearchSummaryValue("ms", result.MahaSakaraj.ToString(CultureInfo.InvariantCulture));
            SetSearchSummaryValue("cs", result.ChulaSakaraj.ToString(CultureInfo.InvariantCulture));
            SetSearchSummaryValue("ks", result.KromSakaraj.ToString(CultureInfo.InvariantCulture));
            SetSearchSummaryValue("sesa", result.SesaKalaYoga.ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception ex)
        {
            _searchDateKsTextBox.Text = "-";
            Trace.WriteLine($"Error searching date: {ex.Message}");
        }
    }

    private void SetSearchSummaryValue(string key, string value)
    {
        if (_searchDateSummaryLabels.TryGetValue(key, out var label))
        {
            label.Text = value;
        }
    }

    private Control BuildAtthabhujjPanel()
    {
        var root = new TableLayoutPanel
        {
            AutoScroll = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            RowCount = 2,
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 10),
            RowCount = 3,
        };
        var title = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateDisplay(17F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Text = "Suriyayātra Vyākhyāna",
        };
        RegisterLocalizedControl(title, "Suriyayātra Vyākhyāna", "សូរ្យយាត្រាវ្យាខ្យាន");
        var subtitle = new Label
        {
            AutoSize = true,
            ForeColor = TextSecondary,
            Text = "Formulas developed by Master Vann Chansaren • Configuration for calculation and results display",
        };
        RegisterLocalizedControl(
            subtitle,
            "Formulas developed by Master Vann Chansaren • Configuration for calculation and results display",
            "កំណែសម្រាប់គណនា និងបង្ហាញលទ្ធផល — រូបមន្តរៀបចំដោយលោកគ្រូ វ៉ាន់ ចាន់សារ៉ែន");
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(subtitle, 0, 1);

        var yearInput = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 8, 0, 0),
            WrapContents = false,
        };
        var yearInputLabel = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
            ForeColor = TextLabel,
            Margin = new Padding(0, 7, 8, 0),
            Text = "Edit the CE/BCE year below, then:",
        };
        RegisterLocalizedControl(
            yearInputLabel,
            "Edit the CE/BCE year below, then:",
            "សូមកែឆ្នាំ គ.ស. / មុន គ.ស. ខាងក្រោម រួចចុច៖");
        _calculateAtthabhujjButton.AutoSize = true;
        _calculateAtthabhujjButton.BackColor = BrandBlue;
        _calculateAtthabhujjButton.FlatAppearance.BorderSize = 0;
        _calculateAtthabhujjButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 76, 128);
        _calculateAtthabhujjButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 56, 96);
        _calculateAtthabhujjButton.FlatStyle = FlatStyle.Flat;
        _calculateAtthabhujjButton.ForeColor = Color.White;
        _calculateAtthabhujjButton.Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold);
        _calculateAtthabhujjButton.Cursor = Cursors.Hand;
        _calculateAtthabhujjButton.Margin = new Padding(0, 2, 0, 0);
        _calculateAtthabhujjButton.Padding = new Padding(14, 5, 14, 5);
        _calculateAtthabhujjButton.Text = "CALCULATE YEAR";
        _calculateAtthabhujjButton.Click += CalculateAtthabhujjYearOnClick;
        RegisterLocalizedControl(
            _calculateAtthabhujjButton,
            "CALCULATE YEAR",
            "គណនាឆ្នាំ");
        yearInput.Controls.Add(yearInputLabel);
        yearInput.Controls.Add(_calculateAtthabhujjButton);
        header.Controls.Add(yearInput, 0, 2);
        root.Controls.Add(header, 0, 0);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 8,
        };
        split.Panel1.Padding = new Padding(0, 0, 8, 0);
        split.Panel2.Padding = new Padding(8, 0, 0, 0);

        void AdjustSplitter()
        {
            if (split.Width > 500)
            {
                try
                {
                    // User requirement: Left side (ខ្សែគណនាប្រតិទិន) width 60%, Right side 40%
                    var desired = (int)(split.Width * 0.60);
                    var minLeft = 440;
                    var maxLeft = Math.Max(minLeft, split.Width - 320);
                    split.SplitterDistance = Math.Clamp(desired, minLeft, maxLeft);
                }
                catch
                {
                    // Ignore layout transition issues
                }
            }
        }

        split.HandleCreated += (_, _) => AdjustSplitter();
        split.SizeChanged += (_, _) => AdjustSplitter();

        var left = new TableLayoutPanel
        {
            AutoScroll = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        left.Controls.Add(BuildAtthabhujjGroup(
            "Calendar calculation bridge",
            "ខ្សែគណនាប្រតិទិន",
            [
                ("ceYear", "CE Year (+) / BCE Year (-)", "ឆ្នាំ គ.ស. (+) / មុន គ.ស. (-)", "", ""),
                ("chulaSakaraj", "Chulasakaraj Year", "ចុល្លសករាជ", "Chulasakaraj (CS = CE - 638)", "ចុល្លសករាជ (គ.ស. - ៦៣៨)"),
                ("buddhistEra", "Buddhist Era (BE)", "ពុទ្ធសករាជ (ព.ស.)", "Buddhist Era (BE = CE + 544)", "ពុទ្ធសករាជ (គ.ស. + ៥៤៤)"),
                ("aharganaRemainder", "Ahargana Remainder", "សំណល់អហ៌គណ", "Remainder of (CS × 292207 + 373) ÷ 800", "សំណល់នៃ (ច.ស. × 292207 + 373) ÷ 800"),
                ("ahargana", "Ahargana to Target Date", "អហ៌គណទៅថ្ងៃបំណង", "Ahargana to target date", "អហ៌គណតាមថ្ងៃបំណង"),
                ("kammaja", "Kammaja Result for Target Date", "កម្មជផលទៅថ្ងៃបំណង", "By hour / minute / second", "តាមម៉ោង/នាទី/វិនាទី"),
                ("uccabala", "Uccabala", "ឧច្ចពល", "Remainder (Ahargana + 2611) ÷ 3232 — verify with table", "សំណល់ (អហ៌គណ + 2611) ÷ 3232 — ដាក់ជាចំណុចត្រូវផ្ទៀងផ្ទាត់"),
                ("avamana", "Avamana", "អវមាន", "Remainder (Ahargana × 11 + 650) ÷ 692", "សំណល់ (អហ៌គណ × 11 + 650) ÷ 692"),
                ("masakendra", "Masakendra", "មាសកេន្ទ្រ", "(Ahargana × 703 + 650) ÷ 20760; integer part", "(អហ៌គណ × 703 + 650) ÷ 20760; យកផលចំនួនគត់"),
                ("boriTithi", "Bori Tithi", "បូរតិថី", "(Masakendra − integer part) × 30", "(មាសកេន្ទ្រ − ផលចំនួនគត់) × 30"),
                ("newEraDayNumber", "New Era Day (Number)", "ថ្ងៃឡើងស័ក (លេខ)", "Weekday index from Ahargana", "វារៈពីអហ៌គណ"),
            ],
            col0Weight: 26,
            col1Weight: 18,
            col2Weight: 56), 0, 0);
        left.Controls.Add(BuildAtthabhujjGroup(
            "Year and lunar rules",
            "ច្បាប់ឆ្នាំ និងចន្ទគតិ",
            [
                ("yearType", "Year Type", "ប្រភេទឆ្នាំ", "Kammaja 1–207 = 366 days; 208–800 = 365 days", "កម្មជផល 1–207 = 366 ថ្ងៃ; 208–800 = 365 ថ្ងៃ"),
                ("daysInYear", "Days in Year", "ចំនួនថ្ងៃក្នុងឆ្នាំ", "Leap Year = 366; Common Year = 365", "ឆ្នាំអធិកសុទិន = 366; ឆ្នាំសុភាព = 365"),
                ("januaryLength", "January", "ខែមករា", "Leap Year = 30 days; Common Year = 29 days", "ឆ្នាំអធិកសុទិន = 30 ថ្ងៃ; ឆ្នាំសុភាព = 29 ថ្ងៃ"),
                ("jyeshthaLength", "Jyeshtha", "ជេស្ឋ", "Leap Year: Avamana 0–125 = 30; 126–691 = 29", "ឆ្នាំអធិកសុទិន: អវមាន 0–125 = 30; 126–691 = 29. ឆ្នាំ"),
                ("nextWeekdayRule", "Next Year's New Era Weekday", "ពារឡើងស័កឆ្នាំបន្ទាប់", "Kammaja 208–800 → Next Weekday; 1–207 → Skip 1 Weekday", "កម្មជផល 208–800 → វារៈបន្ទាប់; 1–207 → លែង 1 ពារ"),
                ("lunarYearType", "Lunar Year Type", "ប្រភេទឆ្នាំចន្ទគតិ", "Bori Tithi 24..29, 0..5 → 13 mo; 6..25 → 12 mo", "បូរតិថី 24,25,26,27,29,0..5 → 13 ខែ; 6..25 → 12 ខែ"),
            ],
            col0Weight: 26,
            col1Weight: 18,
            col2Weight: 56), 0, 1);
        split.Panel1.Controls.Add(left);

        var right = new TableLayoutPanel
        {
            AutoScroll = true,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 2,
        };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.Controls.Add(BuildAtthabhujjGroup(
            "New Era and Maha Sankranta information — Public display",
            "ព័ត៌មានមហាសង្ក្រាន្ត និងឡើងស័ក — បង្ហាញជាសាធារណៈ",
            [
                ("riseOfSak", "New Era Time / Rise of Sak", "វេលាឡើងស័ក", "", ""),
                ("mahaSankranta", "Maha Sankranta Time", "វេលាមហាសង្ក្រាន្ត", "", ""),
                ("mahaSankrantaWeekday", "Maha Sankranta", "ថ្ងៃមហាសង្ក្រាន្ត", "", ""),
                ("riseOfSakWeekday", "New Era Weekday", "ថ្ងៃឡើងស័ក", "", ""),
            ],
            col0Weight: 30,
            col1Weight: 70,
            col2Weight: 0), 0, 0);
        right.Controls.Add(BuildAtthabhujjGroup(
            "Workbook reference",
            "ឯកសារយោងសៀវភៅការងារ",
            [
                ("sourceStatus", "Calculation source", "ប្រភពការគណនា", "Workbook", "អដ្ឋភុជ្ជ!A1:J47"),
                ("formulaStatus", "Formula status", "ស្ថានភាពរូបមន្ត", "Master Vann Chansaren", "រូបមន្តលោកគ្រូ វ៉ាន់ ចាន់សារ៉ែន"),
                ("resultStatus", "Result status", "ស្ថានភាពលទ្ធផល", "Verified", "ផ្ទៀងផ្ទាត់រួច"),
            ],
            col0Weight: 30,
            col1Weight: 38,
            col2Weight: 32), 0, 1);
        split.Panel2.Controls.Add(right);
        root.Controls.Add(split, 0, 1);
        return root;
    }

    private Control BuildAtthabhujjGroup(
        string title,
        string khmerTitle,
        IReadOnlyList<(string Key, string English, string Khmer, string EnglishNote, string KhmerNote)> fields,
        int col0Weight = 28,
        int col1Weight = 20,
        int col2Weight = 52)
    {
        var group = new GroupBox
        {
            AutoSize = true,
            BackColor = Surface,
            Dock = DockStyle.Top,
            Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(10, 28, 10, 10),
            Text = title,
        };
        RegisterLocalizedControl(group, title, khmerTitle);

        var table = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 3,
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            RowCount = fields.Count,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, col0Weight));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, col1Weight));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, col2Weight));

        for (var index = 0; index < fields.Count; index++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var field = fields[index];
            var fieldLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = _fontProvider.CreateBody(9.5F),
                ForeColor = TextPrimary,
                Margin = new Padding(4, 2, 4, 2),
                Text = field.English,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            RegisterLocalizedControl(fieldLabel, field.English, field.Khmer);
            Control valueControl;
            if (field.Key == "ceYear")
            {
                ConfigureTextBox(_atthabhujjYearTextBox);
                _atthabhujjYearTextBox.BackColor = Color.FromArgb(255, 248, 218);
                _atthabhujjYearTextBox.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
                _atthabhujjYearTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                _atthabhujjYearTextBox.Margin = new Padding(0, 4, 0, 4);
                _atthabhujjYearTextBox.Text = "2027";
                _atthabhujjYearTextBox.AccessibleName = "CE Year or BCE Year";
                _atthabhujjYearTextBox.PreviewKeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.IsInputKey = true;
                    }
                };
                _atthabhujjYearTextBox.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        CalculateAtthabhujjYearOnClick(s, e);
                    }
                };
                _atthabhujjYearTextBox.GotFocus += (s, e) => _atthabhujjYearTextBox.SelectAll();
                valueControl = _atthabhujjYearTextBox;
            }
            else
            {
                var label = new Label
                {
                    AutoSize = false,
                    BackColor = field.Key switch
                    {
                        "chulaSakaraj" => Color.FromArgb(255, 248, 218),
                        "aharganaRemainder" => Color.FromArgb(228, 237, 248),
                        "riseOfSak" => Color.FromArgb(235, 247, 235),
                        _ => Color.White,
                    },
                    Dock = DockStyle.Fill,
                    Font = _fontProvider.CreateBody(9.5F, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Margin = new Padding(0, 2, 0, 2),
                    Padding = new Padding(8, 2, 8, 2),
                    Text = "—",
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                valueControl = label;
            }
            _atthabhujjValues[field.Key] = valueControl;
            var noteLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = _fontProvider.CreateBody(8.5F),
                ForeColor = TextSecondary,
                Margin = new Padding(6, 2, 4, 2),
                Text = field.EnglishNote,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            if (!string.IsNullOrEmpty(field.EnglishNote) || !string.IsNullOrEmpty(field.KhmerNote))
            {
                RegisterLocalizedControl(noteLabel, field.EnglishNote, field.KhmerNote);
            }
            table.Controls.Add(fieldLabel, 0, index);
            table.Controls.Add(valueControl, 1, index);
            table.Controls.Add(noteLabel, 2, index);
        }

        group.Controls.Add(table);
        return group;
    }

    private Control BuildInterpretationPanel()
    {
        _interpretationTextBox.Dock = DockStyle.Fill;
        _interpretationTextBox.Multiline = true;
        _interpretationTextBox.ReadOnly = true;
        _interpretationTextBox.ScrollBars = ScrollBars.Vertical;
        _interpretationTextBox.BackColor = Color.FromArgb(250, 251, 253);
        _interpretationTextBox.Font = _fontProvider.CreateBody(11F);
        _interpretationTextBox.Text =
            "Workbook-backed house interpretation rules will appear after calculation.\r\n\r\n" +
            "The displayed text is loaded from the supplied workbook interpretation table; generic astrology text is not substituted.";
        return BuildTextPanel(
            "Workbook interpretation",
            "ការបកស្រាយផ្អែកលើសៀវភៅការងារ",
            "Interpretation text is loaded from the workbook-backed JSON reference table and is shown with its source.",
            "អត្ថបទបកស្រាយត្រូវបានផ្ទុកពីតារាងយោង JSON របស់សៀវភៅការងារ និងបង្ហាញជាមួយប្រភពរបស់វា។",
            _interpretationTextBox);
    }

    private Control BuildDataPanel(
        string title,
        string khmerTitle,
        string description,
        string khmerDescription,
        Control content)
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(BuildPanelTitle(title, khmerTitle), 0, 0);
        layout.Controls.Add(BuildPanelDescription(description, khmerDescription), 0, 1);
        layout.Controls.Add(content, 0, 2);
        return layout;
    }

    private Control BuildTextPanel(
        string title,
        string khmerTitle,
        string description,
        string khmerDescription,
        Control content)
    {
        var layout = BuildDataPanel(title, khmerTitle, description, khmerDescription, content);
        return layout;
    }

    private Control BuildPanelTitle(string title, string khmerTitle)
    {
        var label = new Label
        {
            AutoSize = true,
            Font = _fontProvider.CreateDisplay(16F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 0, 0, 2),
            Text = title,
        };
        RegisterLocalizedControl(label, title, khmerTitle);
        return label;
    }

    private Control BuildPanelDescription(string description, string khmerDescription)
    {
        var label = new Label
        {
            AutoSize = true,
            ForeColor = TextSecondary,
            Margin = new Padding(0, 0, 0, 10),
            Text = description,
        };
        RegisterLocalizedControl(label, description, khmerDescription);
        return label;
    }

    private Control BuildStatusBar()
    {
        _statusLabel.AutoSize = false;
        _statusLabel.BackColor = Color.FromArgb(238, 242, 247);
        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.ForeColor = TextSecondary;
        _statusLabel.Padding = new Padding(10, 0, 10, 0);
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.Text = "Ready \u2014 no calculation has been requested.";
        return _statusLabel;
    }

    private void BindLocations()
    {
        _locationComboBox.DataSource = _locationReferenceDataSource.GetAll().ToList();
        _locationComboBox.DisplayMember = nameof(AstrologyLocation.NameEn);
        _locationComboBox.ValueMember = nameof(AstrologyLocation.Id);
        _locationComboBox.FormattingEnabled = true;
        _locationComboBox.Format += (_, eventArgs) =>
        {
            if (eventArgs.ListItem is AstrologyLocation location)
            {
                eventArgs.Value = string.IsNullOrWhiteSpace(location.NameKm)
                    ? location.NameEn
                    : $"{location.NameEn} / {location.NameKm}";
            }
        };
        _locationComboBox.SelectedIndex = 0;
    }

    private void LocationComboBoxOnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_locationComboBox.SelectedItem is not AstrologyLocation location)
        {
            return;
        }

        _latitudeTextBox.Text = location.Latitude.ToString(CultureInfo.InvariantCulture);
        _longitudeTextBox.Text = location.Longitude.ToString(CultureInfo.InvariantCulture);
        _timeZoneTextBox.Text = location.TimeZoneId;
        UpdateMasterBridgeLabels();
    }

    private void CountryComboBoxOnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_isRelocalizingChoices)
        {
            return;
        }

        var cambodiaSelected = !IsInternationalSelected;

        _locationComboBox.Enabled = cambodiaSelected;
        _internationalCountryTextBox.Enabled = !cambodiaSelected;
        _internationalRegionTextBox.Enabled = !cambodiaSelected;
        _latitudeTextBox.ReadOnly = cambodiaSelected;
        _longitudeTextBox.ReadOnly = cambodiaSelected;
        _timeZoneTextBox.ReadOnly = cambodiaSelected;
        if (!cambodiaSelected)
        {
            _locationComboBox.SelectedIndex = -1;
            _internationalCountryTextBox.Clear();
            _internationalRegionTextBox.Clear();
            _latitudeTextBox.Clear();
            _longitudeTextBox.Clear();
            _timeZoneTextBox.Clear();
        }
        else if (_locationComboBox.Items.Count > 0 && _locationComboBox.SelectedIndex < 0)
        {
            _locationComboBox.SelectedIndex = 0;
        }
        UpdateMasterBridgeLabels();
    }

    // GroupBox.AutoSize measures its content before labels wrap, which clipped the
    // last rows; follow the docked content's real height instead.
    private static void FitGroupToContent(GroupBox group, Control content)
    {
        void Fit() => group.Height = content.Bottom + group.Padding.Bottom;
        content.SizeChanged += (_, _) => Fit();
        content.LocationChanged += (_, _) => Fit();
        Fit();
    }

    private TableLayoutPanel CreateBridgeKeyValueTable(int rowCount)
    {
        var table = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Padding = new Padding(4, 2, 4, 4),
            RowCount = rowCount,
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        for (var i = 0; i < rowCount; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        return table;
    }

    private void AddBridgeRow(
        TableLayoutPanel table,
        int row,
        string key,
        string englishLabel,
        string khmerLabel,
        string initialValue)
    {
        var label = new Label
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            AutoSize = true,
            Font = _fontProvider.CreateBody(8.5F),
            ForeColor = TextLabel,
            Margin = new Padding(0, 5, 6, 5),
            Text = Localize(englishLabel, khmerLabel),
        };
        _localizedFieldLabels.Add((label, englishLabel, khmerLabel));

        var valueLabel = new Label
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9F, FontStyle.Bold),
            ForeColor = TextPrimary,
            Margin = new Padding(0, 5, 0, 5),
            Text = initialValue,
        };
        _masterBridgeLabels[key] = valueLabel;

        table.Controls.Add(label, 0, row);
        table.Controls.Add(valueLabel, 1, row);
    }

    private void UpdateMasterBridgeLabels()
    {
        var date = _birthDatePicker.Value;
        var year = date.Year;
        var astroYear = year;
        var ksYear = year + 3100;

        if (_astronomicalMasterYearTextBox is not null)
        {
            _astronomicalMasterYearTextBox.Text = IsKhmer
                ? $"{ToKhmerDigits(astroYear)}  •  ក.ស. {ToKhmerDigits(ksYear)}"
                : $"{astroYear}  •  Krom Sakaraj {ksYear}";
        }

        if (_masterBridgeLabels.TryGetValue("ceYear", out var ceLbl))
        {
            ceLbl.Text = IsKhmer ? ToKhmerDigits(year) : year.ToString(CultureInfo.InvariantCulture);
        }

        if (_masterBridgeLabels.TryGetValue("astroYear", out var astroLbl))
        {
            astroLbl.Text = IsKhmer ? ToKhmerDigits(astroYear) : astroYear.ToString(CultureInfo.InvariantCulture);
        }

        if (_masterBridgeLabels.TryGetValue("ksYear", out var ksLbl))
        {
            ksLbl.Text = IsKhmer ? ToKhmerDigits(ksYear) : ksYear.ToString(CultureInfo.InvariantCulture);
        }

        if (_masterBridgeLabels.TryGetValue("equivYear", out var equivLbl))
        {
            equivLbl.Text = IsKhmer ? $"{ToKhmerDigits(year)} គ.ស." : $"{year} CE";
        }

        var internationalSelected = IsInternationalSelected;
        var activeLoc = internationalSelected
            ? $"{_internationalCountryTextBox.Text.Trim()} / {_internationalRegionTextBox.Text.Trim()}".Trim(' ', '/')
            : $"{Localize(CountryChoices[0].English, CountryChoices[0].Khmer)} / " +
              (_locationComboBox.SelectedItem is AstrologyLocation loc ? Localize(loc.NameEn, loc.NameKm) : "—");
        if (string.IsNullOrWhiteSpace(activeLoc))
        {
            activeLoc = Localize("International location not entered", "មិនទាន់បញ្ចូលទីតាំងអន្តរជាតិ");
        }

        if (_masterBridgeLabels.TryGetValue("activeLoc", out var locLbl))
        {
            locLbl.Text = activeLoc;
        }

        if (_masterBridgeLabels.TryGetValue("coordinates", out var coordLbl))
        {
            coordLbl.Text = $"{_latitudeTextBox.Text}, {_longitudeTextBox.Text}";
        }

        var effectiveUtc = ResolveWorkspaceUtcOffsetHours();
        if (_masterBridgeLabels.TryGetValue("timeZoneUtc", out var tzLbl))
        {
            var utcText = effectiveUtc is double offset
                ? $"UTC {offset.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture)}"
                : Localize("UTC unknown", "UTC មិនស្គាល់");
            var zoneText = string.IsNullOrWhiteSpace(_timeZoneTextBox.Text) ? "—" : _timeZoneTextBox.Text.Trim();
            tzLbl.Text = $"{zoneText} / {utcText}";
        }

        if (_masterBridgeLabels.TryGetValue("calcChain", out var chainLbl))
        {
            chainLbl.Text = Localize(
                "Ascendant + planets + shadow points (AUTO)",
                "លគ្នា + តារាគ្រោះ + ឆាយាគ្រោះ (AUTO)");
        }

        var coordinatesValid =
            double.TryParse(_latitudeTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            && double.TryParse(_longitudeTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
            && latitude is >= -90 and <= 90
            && longitude is >= -180 and <= 180;
        var isReady = coordinatesValid && effectiveUtc is not null;
        var readyStatus = isReady
            ? $"{Localize("READY", "រួចរាល់")} — {activeLoc}"
            : $"{Localize("CHECK INPUT", "សូមពិនិត្យព័ត៌មាន")} — " +
              (coordinatesValid
                  ? Localize("time zone not recognised", "មិនស្គាល់តំបន់ម៉ោង")
                  : Localize("latitude / longitude missing or invalid", "រយៈទទឹង / រយៈបណ្តោយ មិនត្រឹមត្រូវ"));
        if (_masterBridgeLabels.TryGetValue("linkStatus", out var linkLbl))
        {
            linkLbl.Text = readyStatus;
        }

        if (_masterBridgeLabels.TryGetValue("activeLinkStatus", out var activeLinkLbl))
        {
            activeLinkLbl.Text = readyStatus;
            activeLinkLbl.ForeColor = isReady ? TextSecondary : Color.FromArgb(176, 72, 32);
        }
    }

    private double? ResolveWorkspaceUtcOffsetHours()
    {
        if (double.TryParse(_utcOverrideTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var overrideHours))
        {
            return overrideHours;
        }

        var timeZoneId = _timeZoneTextBox.Text.Trim();
        if (timeZoneId.Length == 0)
        {
            return null;
        }

        try
        {
            var localDateTime = _birthDatePicker.Value.Date + _birthTimePicker.Value.TimeOfDay;
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId).GetUtcOffset(localDateTime).TotalHours;
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }

    private void CalculateHoroscopeSilently()
    {
        try
        {
            if (ValidateInput())
            {
                var input = CreateBirthInput();
                var result = _astrologyCalculationService.Calculate(input);
                DisplayResult(result, selectResultTab: false);
            }
        }
        catch
        {
            // Silently ignore if not ready on init
        }
    }

    // Plain buttons rather than RadioButtons: a RadioButton checks itself when it
    // receives keyboard focus, which silently switched the language at start-up.
    private void ConfigureLanguageButton(Button button, string text, bool isKhmer)
    {
        button.AutoSize = true;
        button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        button.Cursor = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Border;
        button.Font = _fontProvider.CreateBody(9F, FontStyle.Bold);
        button.Margin = new Padding(0);
        button.MinimumSize = new Size(64, 30);
        button.Padding = new Padding(8, 0, 8, 0);
        button.Text = text;
        button.TextAlign = ContentAlignment.MiddleCenter;
        button.UseVisualStyleBackColor = false;
        button.AccessibleName = isKhmer ? "Khmer language" : "English language";
        button.Click += (_, _) => SetLanguage(isKhmer);
    }

    private void UpdateLanguageButtons()
    {
        foreach (var (button, isKhmer) in new[] { (_englishLanguageButton, false), (_khmerLanguageButton, true) })
        {
            var selected = _isKhmer == isKhmer;
            button.BackColor = selected ? BrandBlue : Surface;
            button.ForeColor = selected ? Color.White : TextPrimary;
            button.FlatAppearance.MouseOverBackColor = selected ? BrandBlue : BrandBlueLight;
            button.AccessibleDescription = selected ? "Selected" : null;
        }
    }

    private void SetLanguage(bool isKhmer)
    {
        if (_isKhmer == isKhmer)
        {
            return;
        }

        _isKhmer = isKhmer;
        UiPreferences.SaveIsKhmer(isKhmer);
        SuspendLayout();
        try
        {
            ApplyLanguage();
        }
        finally
        {
            ResumeLayout(true);
        }
    }

    private void ApplyLanguage()
    {
        var khmer = _isKhmer;
        UpdateLanguageButtons();
        _resultTitleLabel.Text = Localize("Calculation results", "លទ្ធផលការគណនា");
        _resultSubtitleLabel.Text = Localize(
            "Calculate a birth profile to populate this workspace.",
            "បញ្ចូលព័ត៌មានកំណើត រួចចុចគណនា ដើម្បីបង្ហាញលទ្ធផល។");

        foreach (var field in _localizedFieldLabels)
        {
            field.Label.Text = khmer ? field.Khmer : field.English;
        }

        foreach (var tab in _localizedTabs)
        {
            tab.Page.Text = khmer ? tab.Khmer : tab.English;
        }

        foreach (var control in _localizedControls)
        {
            control.Control.Text = khmer ? control.Khmer : control.English;
        }

        foreach (var column in _localizedColumns)
        {
            column.Column.HeaderText = khmer ? column.Khmer : column.English;
        }

        Text = Localize(
            "Khmer Astrology — Suriyayātra Calculation System",
            "ហោរាសាស្ត្រខ្មែរ — ប្រព័ន្ធគណនាសូរ្យយាត្រា");
        _brandTitleLabel?.Text = Localize("KHMER ASTROLOGY", "ហោរាសាស្ត្រខ្មែរ");
        _brandSubtitleLabel?.Text = Localize(
            "Suriyayātra Calculation System  •  Workbook-backed foundation",
            "ប្រព័ន្ធគណនាសូរ្យយាត្រា  •  ផ្អែកលើសៀវភៅការងារ");
        _languageLabel.Text = Localize("Language", "ភាសា");
        _birthProfileGroup?.Text = Localize("Birth profile", "ព័ត៌មានកំណើត");
        _calculationWorkspaceGroup?.Text = Localize("Calculation workspace", "ផ្ទាំងការគណនា");
        _calculateButton.Text = Localize("CALCULATE HOROSCOPE", "គណនាហោរាសាស្ត្រ");
        _headerStatusLabel.Text = Localize("Ready for a birth profile", "រួចរាល់សម្រាប់ព័ត៌មានកំណើត");
        _statusLabel.Text = Localize("Ready — no calculation has been requested.", "រួចរាល់ — មិនទាន់មានការស្នើសុំគណនាទេ។");

        _d1Chart.IsKhmer = khmer;
        _d3Chart.IsKhmer = khmer;
        _d9Chart.IsKhmer = khmer;

        RelocalizeChoiceComboBox(_genderComboBox, GenderChoices);
        RelocalizeChoiceComboBox(_countryComboBox, CountryChoices);

        if (_lastResult is not null)
        {
            DisplayResult(_lastResult, selectResultTab: false);
        }
        else
        {
            ApplyEmptyGridLanguage();
            if (_lastCalendarResult is not null)
            {
                PopulateAtthabhujjGrid(_lastCalendarResult);
            }
        }
        PopulateSearchDate();
        UpdateMasterBridgeLabels();
    }

    private void InitializeDefaultAtthabhujj()
    {
        try
        {
            var year = KhmerCalendarCalculator.TryParseYear(_atthabhujjYearTextBox.Text, out var parsedYear) && parsedYear != 0
                ? parsedYear
                : 2027;
            var result = _khmerCalendarCalculator.CalculateForYear(
                year,
                _birthDatePicker.Value.Day,
                _birthDatePicker.Value.Month,
                TimeOnly.FromDateTime(_birthTimePicker.Value));
            PopulateAtthabhujjGrid(result);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Initial Atthabhujj calculation skipped: {ex.Message}");
        }
    }

    private void CalculateAtthabhujjYearOnClick(object? sender, EventArgs e)
    {
        _errorProvider.SetError(_atthabhujjYearTextBox, string.Empty);
        var text = _atthabhujjYearTextBox.Text;
        if (!KhmerCalendarCalculator.TryParseYear(text, out var ceOrBceYear)
            || ceOrBceYear == 0)
        {
            _errorProvider.SetError(
                _atthabhujjYearTextBox,
                Localize(
                    "Enter a non-zero year: positive for CE (e.g. 2026 or ២០២៦) or negative for BCE (e.g. -500).",
                    "សូមបញ្ចូលឆ្នាំមិនមែនសូន្យ៖ លេខវិជ្ជមានសម្រាប់ គ.ស. (ឧ. ២០២៦) ឬលេខអវិជ្ជមានសម្រាប់ មុន គ.ស. (ឧ. -៥០០)។"));
            _atthabhujjYearTextBox.Focus();
            _atthabhujjYearTextBox.SelectAll();
            return;
        }

        try
        {
            var result = _khmerCalendarCalculator.CalculateForYear(
                ceOrBceYear,
                _birthDatePicker.Value.Day,
                _birthDatePicker.Value.Month,
                TimeOnly.FromDateTime(_birthTimePicker.Value));

            PopulateAtthabhujjGrid(result);
            _statusLabel.Text = Localize(
                $"Atthabhujj year calculated for {ceOrBceYear}.",
                $"បានគណនាអដ្ឋភុជ្ជសម្រាប់ឆ្នាំ {ceOrBceYear}។");
            _headerStatusLabel.Text = Localize(
                "Atthabhujj year ready",
                "អដ្ឋភុជ្ជរួចរាល់");
        }
        catch (ArgumentOutOfRangeException exception)
        {
            _errorProvider.SetError(_atthabhujjYearTextBox, Localize(exception.Message, "តម្លៃឆ្នាំ ឬកាលបរិច្ឆេទមិនត្រឹមត្រូវទេ។"));
        }
        catch (Exception exception)
        {
            Trace.WriteLine($"Atthabhujj year calculation failed: {exception}");
            _statusLabel.Text = Localize(
                "Atthabhujj calculation failed. Review the year and target date.",
                "ការគណនាអដ្ឋភុជ្ជបានបរាជ័យ។ សូមពិនិត្យឆ្នាំ និងកាលបរិច្ឆេទគោលដៅ។");
        }
    }

    private void CalculateButtonOnClick(object? sender, EventArgs e)
    {
        if (!ValidateInput())
        {
            return;
        }

        var input = CreateBirthInput();
        _calculateButton.Enabled = false;
        _calculateButton.Text = Localize("CALCULATING...", "កំពុងគណនា...");
        UseWaitCursor = true;
        System.Windows.Forms.Application.DoEvents();
        try
        {
            var result = _astrologyCalculationService.Calculate(input);
            DisplayResult(result);
        }
        catch (Exception exception)
        {
            Trace.WriteLine(exception);
            _statusLabel.Text = Localize(
                "Calculation failed. Correct the input or review the technical log.",
                "ការគណនាបានបរាជ័យ។ សូមកែព័ត៌មានបញ្ចូល ឬពិនិត្យកំណត់ហេតុបច្ចេកទេស។");
            _headerStatusLabel.Text = Localize(
                "Calculation failed — review the input",
                "ការគណនាបានបរាជ័យ — សូមពិនិត្យព័ត៌មានបញ្ចូល");
            MessageBox.Show(
                this,
                Localize(
                    "The horoscope could not be calculated. Please verify the birth location, timezone, and date/time.",
                    "មិនអាចគណនាហោរាសាស្ត្របានទេ។ សូមពិនិត្យទីតាំងកំណើត តំបន់ម៉ោង និងថ្ងៃខែឆ្នាំ/ម៉ោង។"),
                Localize("Calculation error", "កំហុសក្នុងការគណនា"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            _calculateButton.Enabled = true;
            _calculateButton.Text = Localize("CALCULATE HOROSCOPE", "គណនាហោរាសាស្ត្រ");
            UseWaitCursor = false;
        }
    }

    private bool ValidateInput()
    {
        _errorProvider.Clear();
        var valid = true;

        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            SetError(_nameTextBox, Localize("Name is required.", "ត្រូវបញ្ចូលឈ្មោះ។"));
            valid = false;
        }

        var internationalSelected = IsInternationalSelected;
        if (internationalSelected && string.IsNullOrWhiteSpace(_internationalCountryTextBox.Text))
        {
            SetError(_internationalCountryTextBox, Localize("International country is required.", "ត្រូវបញ្ចូលប្រទេសអន្តរជាតិ។"));
            valid = false;
        }

        if (internationalSelected && string.IsNullOrWhiteSpace(_internationalRegionTextBox.Text))
        {
            SetError(_internationalRegionTextBox, Localize("International region/state is required.", "ត្រូវបញ្ចូលរដ្ឋ ឬតំបន់អន្តរជាតិ។"));
            valid = false;
        }

        if (!TryReadCoordinate(_latitudeTextBox, -90D, 90D, "Latitude"))
        {
            valid = false;
        }

        if (!TryReadCoordinate(_longitudeTextBox, -180D, 180D, "Longitude"))
        {
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(_timeZoneTextBox.Text))
        {
            SetError(_timeZoneTextBox, Localize("Timezone is required.", "ត្រូវបញ្ចូលតំបន់ម៉ោង។"));
            valid = false;
        }

        if (!string.IsNullOrWhiteSpace(_utcOverrideTextBox.Text)
            && (!double.TryParse(_utcOverrideTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var utcOffset)
                || utcOffset is < -14D or > 14D))
        {
            SetError(
                _utcOverrideTextBox,
                Localize(
                    "UTC override must be between -14 and 14 hours.",
                    "UTC ប្តូរជំនួសត្រូវស្ថិតនៅចន្លោះ -14 និង 14 ម៉ោង។"));
            valid = false;
        }

        return valid;
    }

    private bool TryReadCoordinate(Control control, double minimum, double maximum, string label)
    {
        if (!double.TryParse(control.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            || value < minimum
            || value > maximum)
        {
            var khmerLabel = label switch
            {
                "Latitude" => "រយៈទទឹង",
                "Longitude" => "រយៈបណ្តោយ",
                _ => label,
            };
            SetError(
                control,
                Localize(
                    $"{label} must be between {minimum} and {maximum}.",
                    $"{khmerLabel} ត្រូវស្ថិតនៅចន្លោះ {minimum} និង {maximum}។"));
            return false;
        }

        return true;
    }

    private BirthInput CreateBirthInput()
    {
        var internationalSelected = IsInternationalSelected;
        return new BirthInput
        {
            Name = _nameTextBox.Text.Trim(),
            Gender = _genderComboBox.SelectedIndex >= 0
                ? GenderChoices[_genderComboBox.SelectedIndex].English
                : GenderChoices[^1].English,
            BirthDate = DateOnly.FromDateTime(_birthDatePicker.Value),
            BirthTime = TimeOnly.FromDateTime(_birthTimePicker.Value),
            Country = internationalSelected ? _internationalCountryTextBox.Text.Trim() : "Cambodia",
            Province = internationalSelected
                ? _internationalRegionTextBox.Text.Trim()
                : _locationComboBox.SelectedItem is AstrologyLocation location ? location.NameEn : null,
            Latitude = double.Parse(_latitudeTextBox.Text, CultureInfo.InvariantCulture),
            Longitude = double.Parse(_longitudeTextBox.Text, CultureInfo.InvariantCulture),
            TimeZoneId = _timeZoneTextBox.Text.Trim(),
            UtcOffsetOverrideHours = double.TryParse(
                _utcOverrideTextBox.Text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var utcOffset)
                ? utcOffset
                : null,
        };
    }

    private void DisplayResult(KhmerAstrology.Application.DTOs.AstrologyResult result, bool selectResultTab = true)
    {
        _lastResult = result;
        var input = result.BirthInput;
        var calendar = result.KhmerCalendar;
        var ascendantSign = Localize(result.Ascendant.SignNameEn, result.Ascendant.SignNameKm);
        var location = FormatBirthLocation(input);
        _statusLabel.Text = Localize(
            $"Calculated {input.Name}: Khmer year {calendar.KhmerYear}, {calendar.YearType}; {result.CalculationStage}",
            $"បានគណនា {input.Name}៖ ឆ្នាំខ្មែរ {ToKhmerDigits(calendar.KhmerYear)}, {LocalizeCalendarValue(calendar.YearType)}");
        _headerStatusLabel.Text = Localize(
            $"Calculated {input.Name}  •  {input.BirthDate:dd/MM/yyyy}",
            $"បានគណនា {input.Name}  •  {input.BirthDate:dd/MM/yyyy}");

        _resultTitleLabel.Text = $"{Localize("Calculation results", "លទ្ធផលការគណនា")}  •  {input.Name}";
        _resultSubtitleLabel.Text =
            $"{input.BirthDate:dd/MM/yyyy} {Localize("at", "វេលា")} {input.BirthTime:HH:mm:ss}  •  " +
            $"{location.Place}  •  {ascendantSign} {Localize("Ascendant", "លគ្គនៈ")}";

        _resultGrid.Rows.Clear();
        _resultGrid.Rows.Add(Localize("Status", "ស្ថានភាព"), Localize("Input validated. Khmer calendar calculation completed.", "បានផ្ទៀងផ្ទាត់ព័ត៌មានបញ្ចូល។ ការគណនាប្រតិទិនខ្មែរបានបញ្ចប់។"));
        _resultGrid.Rows.Add(Localize("Name", "ឈ្មោះ"), input.Name);
        _resultGrid.Rows.Add(Localize("Gender", "ភេទ"), LocalizeChoice(GenderChoices, input.Gender));
        _resultGrid.Rows.Add(Localize("Birth date", "ថ្ងៃខែឆ្នាំកំណើត"), input.BirthDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        _resultGrid.Rows.Add(Localize("Birth time", "ម៉ោងកំណើត"), input.BirthTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        _resultGrid.Rows.Add(Localize("Location", "ទីតាំង"), $"{location.Country} / {location.Place}");
        _resultGrid.Rows.Add(Localize("Coordinates", "កូអរដោនេ"), $"{input.Latitude.ToString(CultureInfo.InvariantCulture)}, {input.Longitude.ToString(CultureInfo.InvariantCulture)}");
        _resultGrid.Rows.Add(Localize("IANA time zone", "តំបន់ម៉ោង IANA"), input.TimeZoneId);
        var utcOffsetHours = ResolveEffectiveUtcOffsetHours(input);
        _resultGrid.Rows.Add(Localize("UTC effective", "UTC មានប្រសិទ្ធភាព"), $"UTC {utcOffsetHours:+0.##;-0.##;0} ({Localize("hours", "ម៉ោង")})");
        _resultGrid.Rows.Add(Localize("UTC instant", "ពេលវេលា UTC"), FormatUtcInstant(input, utcOffsetHours));
        _resultGrid.Rows.Add(Localize("Location link", "ស្ថានភាពភ្ជាប់ទីតាំង"), $"{Localize("READY", "រួចរាល់")} — {location.Country} / {location.Place}");
        _resultGrid.Rows.Add(Localize("Astronomical / Khmer / Buddhist year", "ឆ្នាំតារាសាស្ត្រ / ឆ្នាំខ្មែរ / ពុទ្ធសករាជ"), $"{calendar.AstronomicalYear} / {calendar.KhmerYear} / {calendar.BuddhistYear}");
        _resultGrid.Rows.Add(Localize("Ahargana / Kammaja", "អហរគណ / កម្មជៈ"), $"{calendar.Ahargana:0.##########} / {calendar.Kammaja:0.##########}");
        _resultGrid.Rows.Add(Localize("Calendar year type", "ប្រភេទឆ្នាំប្រតិទិន"), $"{calendar.YearType} ({calendar.DaysInYear} {Localize("days", "ថ្ងៃ")})");
        _resultGrid.Rows.Add(Localize("Month length / weekday rule", "ច្បាប់ប្រវែងខែ / ថ្ងៃសប្តាហ៍"), $"{calendar.MonthLengthRule} / {calendar.WeekdayAdjustment}");
        _resultGrid.Rows.Add(Localize("Lunar year", "ឆ្នាំចន្ទគតិ"), calendar.LunarYearType);
        _resultGrid.Rows.Add(Localize("Avamana / Masakendra / Bori Tithi", "អវមាណ / មាសកេន្ទ្រ / បុរីទិថី"), $"{calendar.Avamana} / {calendar.Masakendra} / {calendar.BoriTithi}");
        _resultGrid.Rows.Add(Localize("New Era weekday", "ថ្ងៃសប្តាហ៍សករាជថ្មី"), LocalizeWeekday(calendar.RiseOfSakWeekday));
        _resultGrid.Rows.Add(Localize("Maha Sankranta", "មហាសង្ក្រាន្ត"), FormatSankrantaDateTime(calendar.MahaSankrantaDay, calendar.MahaSankrantaMonth, calendar.AstronomicalYear, calendar.MahaSankrantaTime));
        _resultGrid.Rows.Add(Localize("Rise of Sak", "វេលាឡើងស័ក"), FormatSankrantaDateTime(calendar.RiseOfSakDay, calendar.RiseOfSakMonth, calendar.AstronomicalYear, calendar.RiseOfSakTime));
        _resultGrid.Rows.Add(Localize("Ascendant", "លគ្គនៈ"), $"{ascendantSign}, {result.Ascendant.Degree:00}° {result.Ascendant.Minute:00}' {result.Ascendant.Second:00.##}\"; {result.Ascendant.NakshatraName}, {Localize("Pada", "បាទា")} {result.Ascendant.Pada}");
        _resultGrid.Rows.Add(Localize("Traditional Sun chain", "ខ្សែគណនាព្រះអាទិត្យបុរាណ"), $"{result.TraditionalSun.LongitudeArcMinutes:0.#####} {Localize("arcminutes", "នាទីធ្នូ")} (ព្រះអាទិត្យ!B34)");
        _resultGrid.Rows.Add(Localize("Traditional Moon chain", "ខ្សែគណនាព្រះចន្ទបុរាណ"), $"{result.TraditionalMoon.LongitudeArcMinutes:0.#####} {Localize("arcminutes", "នាទីធ្នូ")} (ព្រះចន្ទ!B43)");
        _resultGrid.Rows.Add(Localize("Planetary positions", "ទីតាំងភព"), result.Planets.Count == 0
            ? Localize("No planetary positions returned.", "មិនមានទីតាំងភពត្រូវបានបញ្ជូនមកទេ។")
            : $"{result.Planets.Count} {Localize("workbook-derived positions calculated.", "ទីតាំងដែលបានគណនាតាមសៀវភៅការងារ។")}");
        _resultGrid.Rows.Add(Localize("Interpretations", "ការបកស្រាយ"), $"{result.Interpretations.Count} {Localize("workbook house interpretations generated.", "ការបកស្រាយឋានតាមសៀវភៅការងារត្រូវបានបង្កើត។")}");
        _resultGrid.Rows.Add(Localize("Additional workbook points", "ចំណុចបន្ថែមពីសៀវភៅការងារ"), $"{result.AdditionalPoints.Count} {Localize("traditional outer-point positions calculated.", "ទីតាំងចំណុចខាងក្រៅបុរាណត្រូវបានគណនា។")}");

        _planetGrid.Rows.Clear();
        AddPlanetRow(result.Ascendant);
        foreach (var planet in result.Planets)
        {
            AddPlanetRow(planet);
        }

        _d1Chart.Chart = result.D1;
        _d3Chart.Chart = result.D3;
        _d9Chart.Chart = result.D9;
        PopulateNakshatraGrid(result);
        PopulateCalendarGrid(calendar);
        PopulateAtthabhujjGrid(calendar);
        _interpretationTextBox.Text = BuildInterpretationText(result);
        _resultGrid.ClearSelection();
        _planetGrid.ClearSelection();
        _nakshatraGrid.ClearSelection();
        _calendarGrid.ClearSelection();

        if (selectResultTab && _tabs is not null && _resultTab is not null)
        {
            _tabs.SelectedTab = _resultTab;
        }
    }

    private (string Country, string Place) FormatBirthLocation(BirthInput input)
    {
        var country = LocalizeChoice(CountryChoices, input.Country);
        if (string.IsNullOrWhiteSpace(input.Province))
        {
            return (country, Localize("International location", "ទីតាំងអន្តរជាតិ"));
        }

        var knownLocation = _locationComboBox.Items
            .OfType<AstrologyLocation>()
            .FirstOrDefault(location => string.Equals(location.NameEn, input.Province, StringComparison.Ordinal));
        return (country, knownLocation is null ? input.Province : Localize(knownLocation.NameEn, knownLocation.NameKm));
    }

    private string LocalizeChoice((string English, string Khmer)[] choices, string value)
    {
        foreach (var choice in choices)
        {
            if (string.Equals(choice.English, value, StringComparison.Ordinal))
            {
                return Localize(choice.English, choice.Khmer);
            }
        }
        return value;
    }

    private void RelocalizeChoiceComboBox(ComboBox comboBox, (string English, string Khmer)[] choices)
    {
        var selectedIndex = comboBox.SelectedIndex;
        _isRelocalizingChoices = true;
        comboBox.BeginUpdate();
        try
        {
            comboBox.Items.Clear();
            comboBox.Items.AddRange(choices.Select(choice => (object)Localize(choice.English, choice.Khmer)).ToArray());
            comboBox.SelectedIndex = selectedIndex;
        }
        finally
        {
            comboBox.EndUpdate();
            _isRelocalizingChoices = false;
        }
    }

    private static double ResolveEffectiveUtcOffsetHours(BirthInput input)
    {
        if (input.UtcOffsetOverrideHours is double overrideHours)
        {
            return overrideHours;
        }

        try
        {
            var localDateTime = input.BirthDate.ToDateTime(input.BirthTime);
            return TimeZoneInfo.FindSystemTimeZoneById(input.TimeZoneId)
                .GetUtcOffset(localDateTime)
                .TotalHours;
        }
        catch (TimeZoneNotFoundException)
        {
            return 0D;
        }
        catch (InvalidTimeZoneException)
        {
            return 0D;
        }
    }

    private static string FormatUtcInstant(BirthInput input, double utcOffsetHours)
    {
        var localDateTime = input.BirthDate.ToDateTime(input.BirthTime);
        var utcDateTime = DateTime.SpecifyKind(
            localDateTime.Subtract(TimeSpan.FromHours(utcOffsetHours)),
            DateTimeKind.Utc);
        return utcDateTime.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
    }

    private void SetError(Control control, string message) => _errorProvider.SetError(control, message);

    private static readonly string[] TrueNakshatraNamesKm =
    [
        "អស្សុជ", "ភរណី", "កត្តិក", "រោហិណី", "មិគសិរ", "អទ្ទា", "បុនព្វសុ", "បុស្ស", "អាសឡេស",
        "មាឃ", "បុព្វផល្គុនី", "ឧត្តរផល្គុនី", "ហត្ថ", "ចិត្ត", "សាតិ", "វិសាខ", "អនុរាធ",
        "ជេដ្ឋ", "មូល", "បុព្វាសាឡ្ហ", "ឧត្តរាសាឡ្ហ", "សវន", "ធនិដ្ឋ", "សតភិសជ", "បុព្វភទ្ទបទ",
        "ឧត្តរភទ្ទបទ", "រេវតី"
    ];

    private static readonly string[] TrueNakshatraNamesEn =
    [
        "Ashwini", "Bharani", "Krittika", "Rohini", "Mrigashira", "Ardra", "Punarvasu", "Pushya", "Ashlesha",
        "Magha", "Purva Phalguni", "Uttara Phalguni", "Hasta", "Chitra", "Swati", "Vishakha", "Anuradha",
        "Jyeshtha", "Mula", "Purva Ashadha", "Uttara Ashadha", "Shravana", "Dhanishta", "Shatabhisha", "Purva Bhadrapada",
        "Uttara Bhadrapada", "Revati"
    ];

    private static readonly string[] NakshatraTypes9Km =
    [
        "ទលិទ្ទោឫក្ស", "មហទ្ធនោឫក្ស", "ចោរោឫក្ស", "ភូមិបាលោឫក្ស", "វេសិយោឫក្ស",
        "ទេវីឫក្ស", "ពេជ្ឈឃាតោឫក្ស", "រាជាឫក្ស", "សមណោឫក្ស"
    ];

    private static readonly string[] NakshatraTypes9En =
    [
        "Dalidro (Destitute)", "Mahaddhano (Prosperous)", "Choro (Challenger)", "Bhumipalo (Guardian)", "Vesiyo (Enterprising)",
        "Devi (Grace)", "Pecheakhat (Decisive)", "Raja (Sovereign)", "Samano (Spiritual)"
    ];

    private static readonly string[] DSignNamesKm =
    [
        "មេស", "ឧសភ", "មិថុន", "កក្កដ", "សីហ", "កញ្ញា", "តុលា", "វិច្ឆិក", "ធនុ", "មករ", "កុម្ភ", "មីន"
    ];

    private static readonly string[] DSignNamesEn =
    [
        "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
    ];

    private static readonly string[] ZodiacSignNamesKm =
    [
        "មេសៈ", "ឧសភៈ", "មិថុនា", "កក្កដៈ", "សីហៈ", "កញ្ញា", "តុលា", "វិច្ឆិកៈ", "ធ្នូ", "មករៈ", "កុម្ភៈ", "មីនៈ"
    ];

    private static readonly string[] ZodiacSignNamesEn =
    [
        "Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"
    ];

    private void AddPlanetRow(PlanetPosition position)
    {
        var totalMinutes = position.LongitudeArcMinutes;
        var modMinutes = ((totalMinutes % 21600D) + 21600D) % 21600D;
        var signIndex = (int)(modMinutes / 1800D);
        if (signIndex < 0) signIndex = 0;
        if (signIndex > 11) signIndex = 11;

        var deg = (int)((modMinutes % 1800D) / 60D);
        var min = (int)(modMinutes % 60D);
        var secDecimals = position.Body == CelestialBody.Ascendant ? 0 : 2;
        var secVal = Math.Round((modMinutes % 1D) * 60D, secDecimals);

        var nakshatraIndex = (int)(modMinutes / 800D) + 1;
        if (nakshatraIndex < 1) nakshatraIndex = 1;
        if (nakshatraIndex > 27) nakshatraIndex = 27;

        var pada = (int)((modMinutes % 800D) / 200D) + 1;
        if (pada < 1) pada = 1;
        if (pada > 4) pada = 4;

        var trueNakshatraKm = TrueNakshatraNamesKm[nakshatraIndex - 1];
        var trueNakshatraEn = TrueNakshatraNamesEn[nakshatraIndex - 1];

        var typeIndex = (nakshatraIndex - 1) % 9;
        var typeKm = NakshatraTypes9Km[typeIndex];
        var typeEn = NakshatraTypes9En[typeIndex];

        var padaInSign = (int)((modMinutes % 1800D) / 200D) + 1;
        var totalNavamsha = (int)(modMinutes / 200D);
        var d9SignIndex = totalNavamsha % 12;
        if (d9SignIndex < 0) d9SignIndex += 12;
        var d9SignKm = DSignNamesKm[d9SignIndex];
        var d9SignEn = DSignNamesEn[d9SignIndex];

        var drekkanaInSign = (int)((modMinutes % 1800D) / 600D) + 1;
        var d3SignIndex = (signIndex + 4 * (drekkanaInSign - 1)) % 12;
        if (d3SignIndex < 0) d3SignIndex += 12;
        var d3SignKm = DSignNamesKm[d3SignIndex];
        var d3SignEn = DSignNamesEn[d3SignIndex];

        var bodyName = GetBodyDisplayName(position.Body);
        var signName = IsKhmer ? ZodiacSignNamesKm[signIndex] : ZodiacSignNamesEn[signIndex];
        var nakshatraPadaText = IsKhmer
            ? $"{trueNakshatraKm}នក្ខត្តប្ញក្ស ទី {nakshatraIndex} • បាទទី {pada}"
            : $"{trueNakshatraEn} Nakshatra {nakshatraIndex} • Pada {pada}";
        var trueNakshatraText = IsKhmer ? trueNakshatraKm : trueNakshatraEn;
        var nakshatraTypeText = IsKhmer ? typeKm : typeEn;
        var d9Text = IsKhmer
            ? $"នវាង្សទី {padaInSign} — {d9SignKm}រាសី"
            : $"Navamsha {padaInSign} — {d9SignEn}";
        var d3Text = IsKhmer
            ? $"ត្រិយាង្សទី {drekkanaInSign} — {d3SignKm}រាសី"
            : $"Drekkana {drekkanaInSign} — {d3SignEn}";

        var linkStatusText = position.Body switch
        {
            CelestialBody.Ascendant => "Astronomical Lahiri — Latitude/Longitude/UTC linked",
            CelestialBody.KetuDivya => Localize(
                "Traditional Ketu Divya — Displayed separately from Modern Lahiri",
                "Traditional Ketu Divya — បង្ហាញដាច់ដោយឡែកពី Modern Lahiri"),
            _ => "Modern Lahiri — UTC linked",
        };

        var secStr = secDecimals == 0
            ? secVal.ToString("0", CultureInfo.InvariantCulture)
            : (secVal % 1 == 0
                ? secVal.ToString("0", CultureInfo.InvariantCulture)
                : secVal.ToString("0.##", CultureInfo.InvariantCulture));

        _planetGrid.Rows.Add(
            bodyName,
            totalMinutes.ToString("0.00", CultureInfo.InvariantCulture),
            signName,
            deg.ToString(CultureInfo.InvariantCulture),
            min.ToString(CultureInfo.InvariantCulture),
            secStr,
            nakshatraPadaText,
            trueNakshatraText,
            nakshatraTypeText,
            d9Text,
            d3Text,
            linkStatusText);
    }

    private void PopulateNakshatraGrid(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        _nakshatraGrid.Rows.Clear();
        AddNakshatraRow(result.Ascendant);
        foreach (var planet in result.Planets)
        {
            AddNakshatraRow(planet);
        }
        foreach (var point in result.AdditionalPoints)
        {
            AddNakshatraRow(point);
        }
        _nakshatraGrid.ClearSelection();
    }

    private void AddNakshatraRow(PlanetPosition position)
    {
        _nakshatraGrid.Rows.Add(
            GetBodyDisplayName(position.Body),
            position.LongitudeArcMinutes.ToString("0.######", CultureInfo.InvariantCulture),
            $"{position.NakshatraName} / {position.NakshatraNumber}",
            position.Pada);
    }

    private void PopulateCalendarGrid(KhmerCalendarResult calendar)
    {
        _calendarGrid.Rows.Clear();
        AddCalendarRow("Astronomical year", "ឆ្នាំតារាសាស្ត្រ", calendar.AstronomicalYear);
        AddCalendarRow("Khmer year", "ឆ្នាំខ្មែរ", calendar.KhmerYear);
        AddCalendarRow("Buddhist year", "ពុទ្ធសករាជ", calendar.BuddhistYear);
        AddCalendarRow("Solar year fraction", "ប្រភាគឆ្នាំសុរិយគតិ", calendar.SolarYearFraction.ToString("0.##########", CultureInfo.InvariantCulture));
        AddCalendarRow("Weekday", "ថ្ងៃសប្តាហ៍", calendar.Weekday);
        if (!string.IsNullOrWhiteSpace(calendar.TraditionalDate))
        {
            AddCalendarRow("Traditional date", "កាលបរិច្ឆេទប្រពៃណី", calendar.TraditionalDate);
            AddCalendarRow("Lunar day / phase", "ថ្ងៃចន្ទគតិ / វគ្គ", $"{calendar.LunarDayDisplay} — {calendar.TithiName}");
            AddCalendarRow("Lunar month", "ខែចន្ទគតិ", $"{calendar.LunarMonthNameEn} ({calendar.LunarMonthNumber})");
            AddCalendarRow("Animal year", "ឆ្នាំសត្វ", calendar.AnimalYear);
            AddCalendarRow("Sesa-Kala-Yoga / Yuga", "សេសកាលយោគ / យុគ", $"{calendar.SesaKalaYoga} / {calendar.Yuga}");
        }
        AddCalendarRow("Ahargana", "អហរគណ", calendar.Ahargana.ToString("0.##########", CultureInfo.InvariantCulture));
        AddCalendarRow("Ahargana day", "ថ្ងៃអហរគណ", calendar.AharganaDay);
        AddCalendarRow("Kammaja", "កម្មជៈ", calendar.Kammaja.ToString("0.##########", CultureInfo.InvariantCulture));
        AddCalendarRow("Uccabal", "ឧច្ចបាល", calendar.Uccabal);
        AddCalendarRow("New Era day", "ថ្ងៃឡើងស័ក", calendar.NewEraDay);
        AddCalendarRow("Year type", "ប្រភេទឆ្នាំ", $"{calendar.YearType} ({calendar.DaysInYear} {Localize("days", "ថ្ងៃ")})");
        AddCalendarRow("Lunar year type", "ប្រភេទឆ្នាំចន្ទគតិ", calendar.LunarYearType);
        AddCalendarRow("Month length rule", "ច្បាប់ប្រវែងខែ", calendar.MonthLengthRule);
        AddCalendarRow("Weekday adjustment", "ការកែតម្រូវថ្ងៃសប្តាហ៍", calendar.WeekdayAdjustment);
        AddCalendarRow("Avamana / Masakendra / Bori Tithi", "អវមាណ / មាសកេន្ទ្រ / បុរីទិថី", $"{calendar.Avamana} / {calendar.Masakendra} / {calendar.BoriTithi}");
        AddCalendarRow("New Era weekday", "ថ្ងៃសប្តាហ៍សករាជថ្មី", LocalizeWeekday(calendar.RiseOfSakWeekday));
        AddCalendarRow("Maha Sankranta", "មហាសង្ក្រាន្ត", FormatSankrantaDateTime(calendar.MahaSankrantaDay, calendar.MahaSankrantaMonth, calendar.AstronomicalYear, calendar.MahaSankrantaTime));
        AddCalendarRow("Rise of Sak", "វេលាឡើងស័ក", FormatSankrantaDateTime(calendar.RiseOfSakDay, calendar.RiseOfSakMonth, calendar.AstronomicalYear, calendar.RiseOfSakTime));
        _calendarGrid.ClearSelection();
    }

    private void AddCalendarRow(string english, string khmer, object value)
    {
        _calendarGrid.Rows.Add(Localize(english, khmer), value);
    }

    private void PopulateAtthabhujjGrid(KhmerCalendarResult calendar)
    {
        _lastCalendarResult = calendar;
        var januaryMonthLength = calendar.Kammaja <= 207D ? 30 : 29;
        var jyeshthaMonthLength = calendar.Kammaja <= 207D
            ? calendar.Avamana <= 125 ? 30 : 29
            : calendar.Avamana <= 136 ? 30 : 29;
        var ceOrBceYear = calendar.AstronomicalYear <= 0 ? calendar.AstronomicalYear - 1 : calendar.AstronomicalYear;
        SetAtthabhujjValue("ceYear", ceOrBceYear);
        var displayYear = ceOrBceYear.ToString(CultureInfo.InvariantCulture);
        if (_atthabhujjYearTextBox.Text.Trim() != displayYear &&
            (!KhmerCalendarCalculator.TryParseYear(_atthabhujjYearTextBox.Text, out var currentYear) || currentYear != ceOrBceYear))
        {
            _atthabhujjYearTextBox.Text = displayYear;
        }
        var chulaSakaraj = ceOrBceYear - 638;
        var buddhistEra = ceOrBceYear + 544;
        SetAtthabhujjValue("chulaSakaraj", chulaSakaraj);
        SetAtthabhujjValue("buddhistEra", buddhistEra);
        SetAtthabhujjValue("aharganaRemainder", Convert.ToInt64(calendar.SolarYearFraction).ToString(CultureInfo.InvariantCulture));
        SetAtthabhujjValue("ahargana", calendar.AharganaDay);
        SetAtthabhujjValue("kammaja", calendar.Kammaja.ToString("0.#######", CultureInfo.InvariantCulture));
        SetAtthabhujjValue("uccabala", calendar.Uccabal);
        SetAtthabhujjValue("avamana", calendar.Avamana);
        SetAtthabhujjValue("masakendra", calendar.Masakendra);
        SetAtthabhujjValue("boriTithi", calendar.BoriTithi);
        SetAtthabhujjValue("newEraDayNumber", calendar.NewEraDay);
        SetAtthabhujjValue("yearType", LocalizeCalendarValue(calendar.YearType));
        SetAtthabhujjValue("daysInYear", calendar.DaysInYear);
        SetAtthabhujjValue("januaryLength", januaryMonthLength);
        SetAtthabhujjValue("jyeshthaLength", jyeshthaMonthLength);
        var nextWeekdayRule = IsKhmer
            ? (calendar.Kammaja <= 207D ? "លែង 1 ពារ" : "វារៈបន្ទាប់")
            : (calendar.Kammaja <= 207D ? "Skip 1 Weekday" : "Next Weekday");
        SetAtthabhujjValue("nextWeekdayRule", nextWeekdayRule);
        SetAtthabhujjValue("lunarYearType", LocalizeCalendarValue(calendar.LunarYearType));
        SetAtthabhujjValue("riseOfSak", FormatSankrantaDateTime(calendar.RiseOfSakDay, calendar.RiseOfSakMonth, calendar.AstronomicalYear, calendar.RiseOfSakTime));
        SetAtthabhujjValue("mahaSankranta", FormatSankrantaDateTime(calendar.MahaSankrantaDay, calendar.MahaSankrantaMonth, calendar.AstronomicalYear, calendar.MahaSankrantaTime));
        SetAtthabhujjValue("mahaSankrantaWeekday", LocalizeWeekday(calendar.MahaSankrantaWeekday));
        SetAtthabhujjValue("riseOfSakWeekday", LocalizeWeekday(calendar.RiseOfSakWeekday));
        SetAtthabhujjValue("sourceStatus", Localize("Atthabhujj!A1:J47", "អដ្ឋភុជ្ជ!A1:J47"));
        SetAtthabhujjValue("formulaStatus", Localize("Master Vann Chansaren", "រូបមន្តលោកគ្រូ វ៉ាន់ ចាន់សារ៉ែន"));
        SetAtthabhujjValue("resultStatus", Localize("Verified", "ផ្ទៀងផ្ទាត់រួច"));
    }

    private void SetAtthabhujjValue(string key, object value)
    {
        if (_atthabhujjValues.TryGetValue(key, out var control))
        {
            control.Text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "—";
        }
    }

    private string LocalizeCalendarValue(string value) => value switch
    {
        "Leap Year" => Localize("Leap Year", "ឆ្នាំអធិកសុទិន"),
        "Common Year" => Localize("Common Year", "ឆ្នាំសុភាព"),
        "13-Month Lunar Year" => Localize("13-Month Lunar Year", "ឆ្នាំចន្ទគតិ ១៣ ខែ"),
        "12-Month Lunar Year" => Localize("12-Month Lunar Year", "ឆ្នាំចន្ទគតិ ១២ ខែ"),
        "Check Required" => Localize("Check Required", "ត្រូវពិនិត្យ"),
        _ => value,
    };

    private string LocalizeWeekday(string weekday) => IsKhmer
        ? weekday switch
        {
            "Sunday" => "អាទិត្យ",
            "Monday" => "ចន្ទ",
            "Tuesday" => "អង្គារ",
            "Wednesday" => "ពុធ",
            "Thursday" => "ព្រហស្បតិ៍",
            "Friday" => "សុក្រ",
            "Saturday" => "សៅរ៍",
            _ => weekday,
        }
        : weekday;

    private static string ToKhmerDigits(int value) =>
        ToKhmerDigits(value.ToString(CultureInfo.InvariantCulture));

    private static string ToKhmerDigits(string text)
    {
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (var ch in text)
        {
            sb.Append(ch switch
            {
                >= '0' and <= '9' => (char)('០' + (ch - '0')),
                _ => ch,
            });
        }
        return sb.ToString();
    }

    private string FormatSankrantaDateTime(int day, int month, int year, TimeSpan time)
    {
        var monthName = month switch
        {
            3 => Localize("March", "មីនា"),
            5 => Localize("May", "ឧសភា"),
            _ => Localize("April", "មេសា"),
        };

        if (IsKhmer)
        {
            var dayStr = ToKhmerDigits(day);
            var yearStr = year < 0
                ? $"{ToKhmerDigits(-year)} មុន គ.ស."
                : ToKhmerDigits(year);
            var timeStr = ToKhmerDigits($"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}");
            return $"{dayStr} {monthName} {yearStr} — {timeStr}";
        }

        var enYearStr = year < 0 ? $"{-year} BCE" : year.ToString(CultureInfo.InvariantCulture);
        return $"{day} {monthName} {enYearStr} — {time:hh\\:mm\\:ss}";
    }

    private string BuildInterpretationText(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        var ascendantSign = Localize(result.Ascendant.SignNameEn, result.Ascendant.SignNameKm);
        var header = $"{Localize("Workbook-backed interpretation for", "ការបកស្រាយផ្អែកលើសៀវភៅការងារសម្រាប់")} {result.BirthInput.Name}\r\n" +
                     $"{Localize("Ascendant", "លគ្គនៈ")}: {ascendantSign}\r\n\r\n";
        var entries = result.Interpretations.Select(interpretation =>
        {
            var houseName = string.IsNullOrWhiteSpace(interpretation.HouseNameKm)
                ? string.Empty
                : $" ({interpretation.HouseNameKm})";
            var bodyMeaning = string.IsNullOrWhiteSpace(interpretation.BodyMeaningKm)
                ? string.Empty
                : $"{Localize("Planet signifies", "អត្ថន័យតារាគ្រោះ")}: {interpretation.BodyMeaningKm}\r\n";
            return $"{GetBodyDisplayName(interpretation.Body)} — {Localize("House", "ឋាន")} {interpretation.House}{houseName}\r\n" +
                   bodyMeaning +
                   $"{Localize("House signifies", "អត្ថន័យឋាន")}: {interpretation.Description}\r\n" +
                   $"{Localize("Source", "ប្រភព")}: {interpretation.Source}";
        });
        return header + string.Join("\r\n\r\n", entries);
    }

    private string GetBodyDisplayName(CelestialBody body) => IsKhmer
        ? body switch
        {
            CelestialBody.Ascendant => "លគ្នា",
            CelestialBody.Sun => "ព្រះអាទិត្យ",
            CelestialBody.Moon => "ព្រះចន្ទ",
            CelestialBody.Mars => "ព្រះអង្គារ",
            CelestialBody.Mercury => "ព្រះពុធ",
            CelestialBody.Jupiter => "ព្រះព្រហស្បតិ៍",
            CelestialBody.Venus => "ព្រះសុក្រ",
            CelestialBody.Saturn => "ព្រះសៅរ៍",
            CelestialBody.Rahu => "រាហូ",
            CelestialBody.Ketu => "ព្រះកេតុ",
            CelestialBody.KetuVeda => "ព្រះកេតុវេទ",
            CelestialBody.KetuDivya => "ព្រះកេតុទិព្វ",
            CelestialBody.Uranus or CelestialBody.Mrityu => "ម្រឹត្យូវ (Uranus)",
            CelestialBody.Neptune or CelestialBody.Varuna => "ព្រះវរុណ (Neptune)",
            CelestialBody.Pluto or CelestialBody.Yama => "ព្រះយម (Pluto)",
            _ => body.ToString(),
        }
        : body switch
        {
            CelestialBody.Ascendant => "Lagna (Ascendant)",
            CelestialBody.KetuVeda => "Ketu Veda",
            CelestialBody.KetuDivya => "Ketu Divya",
            CelestialBody.Uranus => "Uranus (Mrityu)",
            CelestialBody.Neptune => "Neptune (Varuna)",
            CelestialBody.Pluto => "Pluto (Yama)",
            CelestialBody.Mrityu => "Mrityu (Uranus)",
            CelestialBody.Varuna => "Varuna (Neptune)",
            CelestialBody.Yama => "Yama (Pluto)",
            _ => body.ToString(),
        };

    private void RegisterLocalizedControl(Control control, string english, string khmer)
    {
        _localizedControls.Add((control, english, khmer));
    }

    private void AddLocalizedColumn(
        DataGridView grid,
        string name,
        string english,
        string khmer)
    {
        var index = grid.Columns.Add(name, english);
        _localizedColumns.Add((grid.Columns[index], english, khmer));
    }

    private void ApplyEmptyGridLanguage()
    {
        if (_resultGrid.Rows.Count > 0)
        {
            _resultGrid.Rows[0].SetValues(
                Localize("Status", "ស្ថានភាព"),
                Localize(
                    "No result yet — enter a birth profile and click Calculate Horoscope.",
                    "មិនទាន់មានលទ្ធផលទេ — សូមបញ្ចូលព័ត៌មានកំណើត ហើយចុចគណនាហោរាសាស្ត្រ។"));
        }

        if (_planetGrid.Rows.Count > 0 && _lastResult is null)
        {
            _planetGrid.Rows[0].SetValues(
                "—", "—", "—", "—", "—", "—",
                Localize("No calculation yet", "មិនទាន់មានការគណនាទេ"),
                "—", "—", "—", "—", "—");
        }

        if (_nakshatraGrid.Rows.Count > 0)
        {
            _nakshatraGrid.Rows[0].SetValues(
                "—", "—",
                Localize("Calculate a horoscope to load positions", "សូមគណនាហោរាសាស្ត្រ ដើម្បីផ្ទុកទីតាំង"),
                "—");
        }

        if (_calendarGrid.Rows.Count > 0)
        {
            _calendarGrid.Rows[0].SetValues(
                Localize("Status", "ស្ថានភាព"),
                Localize("Calculate a horoscope to load Khmer calendar data.", "សូមគណនាហោរាសាស្ត្រ ដើម្បីផ្ទុកទិន្នន័យប្រតិទិនខ្មែរ។"));
        }

        if (_atthabhujjGrid.Rows.Count > 0)
        {
            _atthabhujjGrid.Rows[0].SetValues(
                Localize("Status", "ស្ថានភាព"),
                Localize("Calculate a horoscope to load Atthabhujj values.", "សូមគណនាហោរាសាស្ត្រ ដើម្បីផ្ទុកតម្លៃអដ្ឋភុជ្ជ។"),
                "អដ្ឋភុជ្ជ");
        }
    }

    private void ConfigureGrid(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        grid.ColumnHeadersDefaultCellStyle.BackColor = BrandBlue;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 2, 8, 2);
        grid.ColumnHeadersHeight = 42;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Border;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(8, 2, 8, 2);
        grid.DefaultCellStyle.SelectionBackColor = BrandBlueLight;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        grid.Dock = DockStyle.Fill;
        grid.RowTemplate.Height = 42;
    }

    private void AddField(TableLayoutPanel table, int row, string englishLabel, string khmerLabel, Control control)
    {
        var label = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = _fontProvider.CreateBody(9.5F),
            ForeColor = TextLabel,
            Margin = new Padding(0, 0, 8, 0),
            Text = englishLabel,
        };
        _localizedFieldLabels.Add((label, englishLabel, khmerLabel));
        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void ConfigureTextBox(TextBox textBox)
    {
        textBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        textBox.BackColor = Color.White;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = _fontProvider.CreateBody(10F);
        textBox.Margin = new Padding(3, 7, 3, 7);
        textBox.Padding = new Padding(6, 3, 6, 3);
    }

    private void ConfigureComboBox(ComboBox comboBox, IEnumerable<string> items)
    {
        comboBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        comboBox.BackColor = Color.White;
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.Font = _fontProvider.CreateBody(10F);
        comboBox.Items.AddRange(items.Cast<object>().ToArray());
        comboBox.Margin = new Padding(3, 7, 3, 7);
    }

    private void ConfigureDatePicker(DateTimePicker picker, DateTime value, DateTimePickerFormat format = DateTimePickerFormat.Short)
    {
        picker.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        picker.CalendarMonthBackground = Color.White;
        picker.Font = _fontProvider.CreateBody(10F);
        picker.Format = DateTimePickerFormat.Custom;
        picker.CustomFormat = format == DateTimePickerFormat.Time ? "HH:mm:ss" : "dd/MM/yyyy";
        picker.Margin = new Padding(3, 7, 3, 7);
        picker.ShowUpDown = format == DateTimePickerFormat.Time;
        picker.Value = value;
    }
}
