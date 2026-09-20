using System.Globalization;
using System.Diagnostics;
using KhmerAstrology.Application.Interfaces;
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
    private readonly Label _statusLabel = new();
    private readonly TextBox _technicalTextBox = new();
    private readonly DataGridView _resultGrid = new();
    private readonly DataGridView _planetGrid = new();
    private readonly DataGridView _nakshatraGrid = new();
    private readonly DataGridView _calendarGrid = new();
    private readonly DataGridView _atthabhujjGrid = new();
    private readonly Dictionary<string, Control> _atthabhujjValues = new(StringComparer.Ordinal);
    private readonly TextBox _atthabhujjYearTextBox = new();
    private readonly Button _calculateAtthabhujjButton = new();
    private readonly HoroscopeChartControl _d1Chart = new();
    private readonly HoroscopeChartControl _d3Chart = new();
    private readonly HoroscopeChartControl _d9Chart = new();
    private readonly TextBox _interpretationTextBox = new();
    private readonly Button _calculateButton = new();
    private readonly Label _resultTitleLabel = new();
    private readonly Label _resultSubtitleLabel = new();
    private readonly Label _headerStatusLabel = new();
    private readonly Label _languageLabel = new();
    private readonly ComboBox _languageComboBox = new();
    private readonly List<(Label Label, string English, string Khmer)> _localizedFieldLabels = [];
    private readonly List<(Control Control, string English, string Khmer)> _localizedControls = [];
    private readonly List<(TabPage Page, string English, string Khmer)> _localizedTabs = [];
    private readonly List<(DataGridViewColumn Column, string English, string Khmer)> _localizedColumns = [];
    private KhmerAstrology.Application.DTOs.AstrologyResult? _lastResult;
    private Label? _brandTitleLabel;
    private Label? _brandSubtitleLabel;
    private GroupBox? _birthProfileGroup;
    private GroupBox? _calculationWorkspaceGroup;
    private TabControl? _tabs;
    private TabPage? _resultTab;

    private bool IsKhmer => _languageComboBox.SelectedIndex == 1;

    private string Localize(string english, string khmer) => IsKhmer ? khmer : english;

    public MainForm(
        IAstrologyCalculationService astrologyCalculationService,
        IKhmerCalendarCalculator khmerCalendarCalculator,
        ILocationReferenceDataSource locationReferenceDataSource,
        UiFontProvider fontProvider)
    {
        _astrologyCalculationService = astrologyCalculationService;
        _khmerCalendarCalculator = khmerCalendarCalculator;
        _locationReferenceDataSource = locationReferenceDataSource;
        _fontProvider = fontProvider;

        _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        InitializeForm();
        BuildLayout();
        AcceptButton = _calculateButton;
        BindLocations();
        ApplyLanguage();
    }

    private void InitializeForm()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = PageBackground;
        ClientSize = new Size(1_280, 820);
        Font = _fontProvider.CreateBody(10F);
        MinimumSize = new Size(1_080, 700);
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
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

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
            Padding = new Padding(16, 8, 16, 8),
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
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
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
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0),
            WrapContents = false,
        };
        _languageLabel.AutoSize = true;
        _languageLabel.Font = _fontProvider.CreateBody(8.5F, FontStyle.Bold);
        _languageLabel.ForeColor = TextSecondary;
        _languageLabel.Margin = new Padding(8, 3, 6, 0);
        ConfigureComboBox(_languageComboBox, ["English", "ខ្មែរ"]);
        _languageComboBox.Width = 122;
        _languageComboBox.Margin = new Padding(0);
        _languageComboBox.SelectedIndex = 1;
        _languageComboBox.SelectedIndexChanged += LanguageComboBoxOnSelectedIndexChanged;
        languagePanel.Controls.Add(_languageComboBox);
        languagePanel.Controls.Add(_languageLabel);
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
            ItemSize = new Size(136, 36),
            Padding = new Point(14, 6),
            SizeMode = TabSizeMode.Fixed,
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
        _tabs.TabPages.Add(atthabhujj);
        _tabs.TabPages.Add(interpretation);
        _tabs.TabPages.Add(technical);
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
            Padding = new Padding(8, 18, 8, 8),
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
            SplitterDistance = 640,
            SplitterWidth = 8,
        };
        split.Panel1.Padding = new Padding(0, 0, 10, 0);
        split.Panel2.Padding = new Padding(10, 0, 0, 0);

        _birthProfileGroup = new GroupBox
        {
            BackColor = Surface,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(11F, FontStyle.Bold),
            Padding = new Padding(10, 22, 10, 10),
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
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 156));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < table.RowCount; i++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        }

        ConfigureTextBox(_nameTextBox);
        ConfigureComboBox(_genderComboBox, new[] { "Male", "Female", "Other / Not specified" });
        ConfigureDatePicker(_birthDatePicker, DateTime.Today);
        ConfigureDatePicker(_birthTimePicker, DateTime.Today.AddHours(8).AddMinutes(30), DateTimePickerFormat.Time);
        ConfigureComboBox(_countryComboBox, new[] { "Cambodia", "International" });
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
        CountryComboBoxOnSelectedIndexChanged(_countryComboBox, EventArgs.Empty);

        AddField(table, 0, "Name", "ឈ្មោះ", _nameTextBox);
        AddField(table, 1, "Gender", "ភេទ", _genderComboBox);
        AddField(table, 2, "Birth date", "ថ្ងៃខែឆ្នាំកំណើត", _birthDatePicker);
        AddField(table, 3, "Birth time", "ម៉ោងកំណើត", _birthTimePicker);
        AddField(table, 4, "Location mode / country", "របៀបទីតាំង / ប្រទេស", _countryComboBox);
        AddField(table, 5, "Cambodian province / city", "ក្រុង/ខេត្តកម្ពុជា", _locationComboBox);
        AddField(table, 6, "International country", "ប្រទេសអន្តរជាតិ", _internationalCountryTextBox);
        AddField(table, 7, "International region / state", "រដ្ឋ/តំបន់អន្តរជាតិ", _internationalRegionTextBox);
        AddField(table, 8, "Latitude", "រយៈទទឹង", _latitudeTextBox);
        AddField(table, 9, "Longitude", "រយៈបណ្ដោយ", _longitudeTextBox);
        AddField(table, 10, "IANA time zone", "តំបន់ម៉ោង IANA", _timeZoneTextBox);
        AddField(table, 11, "UTC override (optional)", "UTC ប្តូរជំនួស (ជាជម្រើស)", _utcOverrideTextBox);

        _calculateButton.AutoSize = true;
        _calculateButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        _calculateButton.BackColor = BrandBlue;
        _calculateButton.FlatStyle = FlatStyle.Flat;
        _calculateButton.Font = _fontProvider.CreateBody(10F, FontStyle.Bold);
        _calculateButton.ForeColor = Color.White;
        _calculateButton.Margin = new Padding(3, 12, 3, 3);
        _calculateButton.MinimumSize = new Size(250, 46);
        _calculateButton.MaximumSize = new Size(330, 54);
        _calculateButton.Padding = new Padding(18, 8, 18, 8);
        _calculateButton.Text = "CALCULATE HOROSCOPE";
        _calculateButton.UseVisualStyleBackColor = false;
        _calculateButton.FlatAppearance.BorderSize = 0;
        _calculateButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 76, 128);
        _calculateButton.Click += CalculateButtonOnClick;
        table.Controls.Add(_calculateButton, 1, 12);

        inputGroup.Controls.Add(table);
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
            Padding = new Padding(10, 22, 10, 10),
            Text = "Calculation workspace",
        };
        var group = _calculationWorkspaceGroup;
        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 12, 20, 16),
            RowCount = 4,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var explanation = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ForeColor = TextLabel,
            MaximumSize = new Size(410, 0),
            Text = "Complete the birth profile, then calculate. Results are produced by the workbook-derived calendar, planetary, and chart engines.",
        };
        RegisterLocalizedControl(
            explanation,
            "Enter a complete birth profile, then calculate once to populate the result workspace. Values are calculated by the workbook-derived calendar, ascendant, planetary, and divisional-chart engines.",
            "សូមបញ្ចូលព័ត៌មានកំណើតឱ្យពេញលេញ រួចចុចគណនា ដើម្បីបង្ហាញលទ្ធផល។ តម្លៃទាំងអស់គណនាតាមប្រតិទិន លគ្គនៈ ភព និងតារាងចែក ដែលបានយកពីសៀវភៅការងារ។");
        var phaseLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Font = _fontProvider.CreateBody(12F, FontStyle.Bold),
            ForeColor = BrandBlue,
            Text = "READY\r\nWorkbook-backed calculations",
        };
        RegisterLocalizedControl(phaseLabel, "READY\r\nWorkbook result engine", "រួចរាល់\r\nម៉ាស៊ីនលទ្ធផលផ្អែកលើសៀវភៅការងារ");
        var sourceLabel = new Label
        {
            AutoSize = true,
            ForeColor = TextSecondary,
            Text = "Reference: EXCEL_FORMULA_MAPPING.md",
        };
        RegisterLocalizedControl(sourceLabel, "Reference: EXCEL_FORMULA_MAPPING.md", "ឯកសារយោង៖ EXCEL_FORMULA_MAPPING.md");
        var scopeLabel = new Label
        {
            AutoSize = true,
            ForeColor = TextSecondary,
            MaximumSize = new Size(410, 0),
            Padding = new Padding(0, 12, 0, 0),
            Text = "Includes Khmer calendar, planetary positions, workbook points, D1/D3/D9 charts, Nakshatra, and interpretations.",
        };
        RegisterLocalizedControl(
            scopeLabel,
            "Results include Khmer calendar, Ascendant, 13 planetary positions, additional workbook points, D1/D3/D9 charts, Nakshatra, and interpretations.",
            "លទ្ធផលរួមមានប្រតិទិនខ្មែរ លគ្គនៈ ទីតាំងភព ១៣ ចំណុចបន្ថែមពីសៀវភៅការងារ តារាង D1/D3/D9 នក្ខត្តឫក្ស និងការបកស្រាយ។");
        RegisterLocalizedControl(
            phaseLabel,
            "READY\r\nWorkbook-backed calculations",
            "រួចរាល់\r\nការគណនាផ្អែកលើសៀវភៅការងារ");
        RegisterLocalizedControl(
            explanation,
            "Complete the birth profile, then calculate. Results are produced by the workbook-derived calendar, planetary, and chart engines.",
            "សូមបញ្ចូលព័ត៌មានកំណើត រួចចុចគណនា។ លទ្ធផលគណនាតាមប្រតិទិន ភព និងតារាងចែកពីសៀវភៅការងារ។");
        RegisterLocalizedControl(
            scopeLabel,
            "Includes Khmer calendar, planetary positions, workbook points, D1/D3/D9 charts, Nakshatra, and interpretations.",
            "រួមមានប្រតិទិនខ្មែរ ទីតាំងភព ចំណុចពីសៀវភៅការងារ តារាង D1/D3/D9 នក្ខត្តឫក្ស និងការបកស្រាយ។");
        layout.Controls.Add(explanation, 0, 0);
        layout.Controls.Add(phaseLabel, 0, 1);
        layout.Controls.Add(sourceLabel, 0, 2);
        layout.Controls.Add(scopeLabel, 0, 3);
        group.Controls.Add(layout);
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
        _resultGrid.Columns[0].FillWeight = 28;
        _resultGrid.Columns[1].FillWeight = 72;
        _resultGrid.Rows.Add("Status", "No result yet \u2014 enter a birth profile and click Calculate Horoscope.");

        ConfigureGrid(_planetGrid);
        _planetGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        AddLocalizedColumn(_planetGrid, "body", "Planet / point", "តារាគ្រោះ / ចំណុច");
        AddLocalizedColumn(_planetGrid, "sign", "Sign", "រាសី");
        AddLocalizedColumn(_planetGrid, "position", "Position (DMS)", "ទីតាំង (អង្សា-លិប្ដា-ពិលិប្ដា)");
        AddLocalizedColumn(_planetGrid, "nakshatra", "Nakshatra", "នក្ខត្តឫក្ស");
        AddLocalizedColumn(_planetGrid, "pada", "Pada", "បាទា");
        AddLocalizedColumn(_planetGrid, "house", "House", "ឋាន");
        AddLocalizedColumn(_planetGrid, "longitude", "Longitude (arcminutes)", "សំស្ផុដ (លិប្ដា)");
        _planetGrid.Columns[0].FillWeight = 15;
        _planetGrid.Columns[1].FillWeight = 18;
        _planetGrid.Columns[2].FillWeight = 18;
        _planetGrid.Columns[3].FillWeight = 20;
        _planetGrid.Columns[4].FillWeight = 8;
        _planetGrid.Columns[5].FillWeight = 8;
        _planetGrid.Columns[6].FillWeight = 13;
        _planetGrid.Rows.Add("\u2014", "\u2014", "\u2014", "No calculation yet", "\u2014", "\u2014", "\u2014");

        var resultTabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(10, 4),
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
        AddLocalizedColumn(_nakshatraGrid, "body", "Body", "ភព");
        AddLocalizedColumn(_nakshatraGrid, "longitude", "Longitude (arcminutes)", "រយៈបណ្តោយ (នាទីធ្នូ)");
        AddLocalizedColumn(_nakshatraGrid, "nakshatra", "Nakshatra", "នក្ខត្តឫក្ស");
        AddLocalizedColumn(_nakshatraGrid, "pada", "Pada", "បាទា");
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
        AddLocalizedColumn(_calendarGrid, "field", "Calendar field", "វាលប្រតិទិន");
        AddLocalizedColumn(_calendarGrid, "value", "Value", "តម្លៃ");
        _calendarGrid.Columns[0].FillWeight = 34;
        _calendarGrid.Columns[1].FillWeight = 66;
        _calendarGrid.Rows.Add("Status", "Calculate a horoscope to load Khmer calendar data.");
        return BuildDataPanel(
            "Khmer calendar",
            "ប្រតិទិនខ្មែរ",
            "Calendar, Ahargana, Sankranta, lunar-year, and Suriyayātra bridge values from the workbook chain.",
            "តម្លៃប្រតិទិន អហរគណ សង្ក្រាន្ត ឆ្នាំចន្ទគតិ និងសូរ្យយាត្រា ដែលបានមកពីខ្សែគណនារបស់សៀវភៅការងារ។",
            _calendarGrid);
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
            Text = "Formulas developed by Master Vann Chansaren • Workbook bridge: អដ្ឋភុជ្ជ",
        };
        RegisterLocalizedControl(
            subtitle,
            "Formulas developed by Master Vann Chansaren • Workbook bridge: Atthabhujj",
            "រូបមន្តរៀបចំដោយលោកគ្រូ វ៉ាន់ ចាន់សារ៉ែន • ខ្សែភ្ជាប់សៀវភៅការងារ៖ អដ្ឋភុជ្ជ");
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
        _calculateAtthabhujjButton.FlatStyle = FlatStyle.Flat;
        _calculateAtthabhujjButton.ForeColor = Color.White;
        _calculateAtthabhujjButton.Font = _fontProvider.CreateBody(9F, FontStyle.Bold);
        _calculateAtthabhujjButton.Margin = new Padding(0, 2, 0, 0);
        _calculateAtthabhujjButton.Padding = new Padding(12, 5, 12, 5);
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
            SplitterDistance = 620,
            SplitterWidth = 8,
        };
        split.Panel1.Padding = new Padding(0, 0, 8, 0);
        split.Panel2.Padding = new Padding(8, 0, 0, 0);

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
                ("ceYear", "CE Year (+) / BCE Year (-)", "ឆ្នាំ គ.ស. (+) / មុន គ.ស. (-)", "B3"),
                ("chulaSakaraj", "Chulasakaraj Year", "ចុល្លសករាជ", "B37"),
                ("buddhistEra", "Buddhist Era (BE)", "ពុទ្ធសករាជ (ព.ស.)", "B6"),
                ("aharganaRemainder", "Ahargana Remainder", "សំណល់អហរគណ", "B10"),
                ("ahargana", "Ahargana to Target Date", "អហរគណទៅថ្ងៃបំណង", "B11 / B33"),
                ("kammaja", "Kammaja Result for Target Date", "កម្មជពលទៅថ្ងៃបំណង", "B12 / B34"),
                ("uccabala", "Uccabala", "ឧច្ចពល", "B13"),
                ("avamana", "Avamana", "អវមាន", "B14"),
                ("masakendra", "Masakendra", "មាសកេន្ទ្រ", "B15"),
                ("boriTithi", "Bori Tithi", "បូរតិថី", "B16"),
                ("newEraDayNumber", "New Era Day (Number)", "ថ្ងៃឡើងស័ក (លេខ)", "B17"),
            ]), 0, 0);
        left.Controls.Add(BuildAtthabhujjGroup(
            "Year and lunar rules",
            "ច្បាប់ឆ្នាំ និងចន្ទគតិ",
            [
                ("yearType", "Year Type", "ប្រភេទឆ្នាំ", "B20"),
                ("daysInYear", "Days in Year", "ចំនួនថ្ងៃក្នុងឆ្នាំ", "B21"),
                ("januaryLength", "January", "ខែមករា", "B22"),
                ("jyeshthaLength", "Jyeshtha", "ជេស្ឋ", "B24"),
                ("nextWeekdayRule", "Next Year's New Era Weekday", "ពារឡើងស័កឆ្នាំបន្ទាប់", "B26"),
                ("lunarYearType", "Lunar Year Type", "ប្រភេទឆ្នាំចន្ទគតិ", "B28"),
            ]), 0, 1);
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
            "New Era and Maha Sankranta information",
            "ព័ត៌មានឡើងស័ក និងមហាសង្ក្រាន្ត",
            [
                ("riseOfSak", "New Era Time / Rise of Sak", "វេលាឡើងស័ក", "B30"),
                ("mahaSankranta", "Maha Sankranta Time", "វេលាមហាសង្ក្រាន្ត", "B31"),
                ("mahaSankrantaWeekday", "Maha Sankranta", "ថ្ងៃមហាសង្ក្រាន្ត", "B32"),
                ("riseOfSakWeekday", "New Era Weekday", "ថ្ងៃឡើងស័ក", "B45 / B46"),
            ]), 0, 0);
        right.Controls.Add(BuildAtthabhujjGroup(
            "Workbook reference",
            "ឯកសារយោងសៀវភៅការងារ",
            [
                ("sourceStatus", "Calculation source", "ប្រភពការគណនា", "អដ្ឋភុជ្ជ!A1:J47"),
                ("formulaStatus", "Formula status", "ស្ថានភាពរូបមន្ត", "Workbook-derived"),
                ("resultStatus", "Result status", "ស្ថានភាពលទ្ធផល", "READY"),
            ]), 0, 1);
        split.Panel2.Controls.Add(right);
        root.Controls.Add(split, 0, 1);
        return root;
    }

    private Control BuildAtthabhujjGroup(
        string title,
        string khmerTitle,
        IReadOnlyList<(string Key, string English, string Khmer, string Note)> fields)
    {
        var group = new GroupBox
        {
            AutoSize = true,
            BackColor = Surface,
            Dock = DockStyle.Top,
            Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(8, 22, 8, 8),
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
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

        for (var index = 0; index < fields.Count; index++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            var field = fields[index];
            var fieldLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                ForeColor = TextPrimary,
                Margin = new Padding(4, 4, 4, 4),
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
                _atthabhujjYearTextBox.Margin = new Padding(0, 1, 0, 1);
                _atthabhujjYearTextBox.Text = _birthDatePicker.Value.Year.ToString(CultureInfo.InvariantCulture);
                _atthabhujjYearTextBox.AccessibleName = "CE Year or BCE Year";
                valueControl = _atthabhujjYearTextBox;
            }
            else
            {
                valueControl = new Label
                {
                    AutoSize = true,
                    BackColor = Color.White,
                    Dock = DockStyle.Fill,
                    Font = _fontProvider.CreateBody(10F, FontStyle.Bold),
                    ForeColor = TextPrimary,
                    Margin = new Padding(0, 1, 0, 1),
                    Padding = new Padding(8, 5, 8, 5),
                    Text = "—",
                    TextAlign = ContentAlignment.MiddleLeft,
                };
            }
            _atthabhujjValues[field.Key] = valueControl;
            var noteLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                ForeColor = TextSecondary,
                Margin = new Padding(8, 4, 4, 4),
                Text = field.Note,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            table.Controls.Add(fieldLabel, 0, index);
            table.Controls.Add(valueControl, 1, index);
            table.Controls.Add(noteLabel, 2, index);
        }

        group.Controls.Add(table);
        return group;
    }

    private Control BuildAtthabhujjPanelLegacy()
    {
        ConfigureGrid(_atthabhujjGrid);
        AddLocalizedColumn(_atthabhujjGrid, "field", "Atthabhujj field", "វាលអដ្ឋភុជ្ជ");
        AddLocalizedColumn(_atthabhujjGrid, "value", "Value", "តម្លៃ");
        AddLocalizedColumn(_atthabhujjGrid, "source", "Workbook cell", "ក្រឡាសៀវភៅការងារ");
        _atthabhujjGrid.Columns[0].FillWeight = 34;
        _atthabhujjGrid.Columns[1].FillWeight = 46;
        _atthabhujjGrid.Columns[2].FillWeight = 20;
        _atthabhujjGrid.Rows.Add(
            Localize("Status", "ស្ថានភាព"),
            Localize("Calculate a horoscope to load Atthabhujj values.", "សូមគណនាហោរាសាស្ត្រ ដើម្បីផ្ទុកតម្លៃអដ្ឋភុជ្ជ។"),
            "អដ្ឋភុជ្ជ");

        return BuildDataPanel(
            "Atthabhujj / calendar bridge",
            "អដ្ឋភុជ្ជ / ខ្សែភ្ជាប់ប្រតិទិន",
            "Workbook-backed calendar bridge and Maha Sankranta values from the Atthabhujj sheet.",
            "ខ្សែភ្ជាប់ប្រតិទិន និងតម្លៃមហាសង្ក្រាន្តដែលផ្អែកលើសន្លឹកអដ្ឋភុជ្ជក្នុងសៀវភៅការងារ។",
            _atthabhujjGrid);
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
    }

    private void CountryComboBoxOnSelectedIndexChanged(object? sender, EventArgs e)
    {
        var cambodiaSelected = string.Equals(
            _countryComboBox.SelectedItem?.ToString(),
            "Cambodia",
            StringComparison.Ordinal);

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
    }

    private void LanguageComboBoxOnSelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        var khmer = _languageComboBox.SelectedIndex == 1;

        Text = khmer
            ? "ហោរាសាស្ត្រខ្មែរ — ប្រព័ន្ធគណនាសូរ្យយាត្រ"
            : "Khmer Astrology — Suriyayātra Calculation System";
        _brandTitleLabel?.Text = khmer ? "ហោរាសាស្ត្រខ្មែរ" : "KHMER ASTROLOGY";
        _brandSubtitleLabel?.Text = khmer
            ? "ប្រព័ន្ធគណនាសូរ្យយាត្រ • យោងតាមសៀវភៅ Excel"
            : "Suriyayātra Calculation System  •  Workbook-backed foundation";
        _languageLabel.Text = khmer ? "ភាសា" : "Language";
        _birthProfileGroup?.Text = khmer ? "ព័ត៌មានកំណើត" : "Birth profile";
        _calculationWorkspaceGroup?.Text = khmer ? "ផ្ទាំងការគណនា" : "Calculation workspace";
        _calculateButton.Text = khmer ? "គណនាហោរាសាស្ត្រ" : "CALCULATE HOROSCOPE";
        _resultTitleLabel.Text = khmer ? "លទ្ធផលការគណនា" : "Calculation results";
        _resultSubtitleLabel.Text = khmer
            ? "បញ្ចូលព័ត៌មានកំណើត រួចចុចគណនា ដើម្បីបង្ហាញលទ្ធផល។"
            : "Calculate a birth profile to populate this workspace.";
        _headerStatusLabel.Text = khmer ? "រួចរាល់សម្រាប់ព័ត៌មានកំណើត" : "Ready for a birth profile";
        _statusLabel.Text = khmer
            ? "រួចរាល់ — មិនទាន់បានស្នើសុំការគណនា។"
            : "Ready — no calculation has been requested.";

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

        if (_lastResult is not null)
        {
            DisplayResult(_lastResult);
        }
        else
        {
            ApplyEmptyGridLanguage();
        }
    }

    private void CalculateAtthabhujjYearOnClick(object? sender, EventArgs e)
    {
        _errorProvider.SetError(_atthabhujjYearTextBox, string.Empty);
        var text = _atthabhujjYearTextBox.Text.Trim();
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ceOrBceYear)
            || ceOrBceYear == 0)
        {
            _errorProvider.SetError(
                _atthabhujjYearTextBox,
                Localize(
                    "Enter a non-zero year: positive for CE or negative for BCE.",
                    "សូមបញ្ចូលឆ្នាំមិនមែនសូន្យ៖ លេខវិជ្ជមានសម្រាប់ គ.ស. ឬលេខអវិជ្ជមានសម្រាប់ មុន គ.ស."));
            _atthabhujjYearTextBox.Focus();
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
            var calendar = result.KhmerCalendar;
            _statusLabel.Text =
                $"Calculated {input.Name}: Khmer year {calendar.KhmerYear}, " +
                $"{calendar.YearType}; {result.CalculationStage}";
            _headerStatusLabel.Text = $"Calculated {input.Name}  •  {input.BirthDate:dd/MM/yyyy}";
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
            _statusLabel.Text = "Calculation failed. Correct the input or review the technical log.";
            _headerStatusLabel.Text = "Calculation failed — review the input";
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

        var internationalSelected = string.Equals(
            _countryComboBox.SelectedItem?.ToString(),
            "International",
            StringComparison.Ordinal);
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
        var internationalSelected = string.Equals(
            _countryComboBox.SelectedItem?.ToString(),
            "International",
            StringComparison.Ordinal);
        return new BirthInput
        {
            Name = _nameTextBox.Text.Trim(),
            Gender = _genderComboBox.Text,
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

    private void DisplayResult(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        _lastResult = result;
        var input = result.BirthInput;
        var calendar = result.KhmerCalendar;
        var ascendantSign = Localize(result.Ascendant.SignNameEn, result.Ascendant.SignNameKm);
        _statusLabel.Text = Localize(
            $"Calculated {input.Name}: Khmer year {calendar.KhmerYear}, {calendar.YearType}; {result.CalculationStage}",
            $"បានគណនា {input.Name}៖ ឆ្នាំខ្មែរ {calendar.KhmerYear}, {calendar.YearType}; {result.CalculationStage}");
        _headerStatusLabel.Text = Localize(
            $"Calculated {input.Name}  •  {input.BirthDate:dd/MM/yyyy}",
            $"បានគណនា {input.Name}  •  {input.BirthDate:dd/MM/yyyy}");

        _resultTitleLabel.Text = $"{Localize("Calculation results", "លទ្ធផលការគណនា")}  •  {input.Name}";
        _resultSubtitleLabel.Text =
            $"{input.BirthDate:dd/MM/yyyy} {Localize("at", "វេលា")} {input.BirthTime:HH:mm:ss}  •  " +
            $"{input.Province ?? input.Country}  •  {ascendantSign} {Localize("Ascendant", "លគ្គនៈ")}";

        _resultGrid.Rows.Clear();
        _resultGrid.Rows.Add(Localize("Status", "ស្ថានភាព"), Localize("Input validated. Khmer calendar calculation completed.", "បានផ្ទៀងផ្ទាត់ព័ត៌មានបញ្ចូល។ ការគណនាប្រតិទិនខ្មែរបានបញ្ចប់។"));
        _resultGrid.Rows.Add(Localize("Name", "ឈ្មោះ"), input.Name);
        _resultGrid.Rows.Add(Localize("Gender", "ភេទ"), input.Gender);
        _resultGrid.Rows.Add(Localize("Birth date", "ថ្ងៃខែឆ្នាំកំណើត"), input.BirthDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        _resultGrid.Rows.Add(Localize("Birth time", "ម៉ោងកំណើត"), input.BirthTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        _resultGrid.Rows.Add(Localize("Location", "ទីតាំង"), $"{input.Country} / {input.Province ?? Localize("International location", "ទីតាំងអន្តរជាតិ")}");
        _resultGrid.Rows.Add(Localize("Coordinates", "កូអរដោនេ"), $"{input.Latitude.ToString(CultureInfo.InvariantCulture)}, {input.Longitude.ToString(CultureInfo.InvariantCulture)}");
        _resultGrid.Rows.Add(Localize("IANA time zone", "តំបន់ម៉ោង IANA"), input.TimeZoneId);
        var utcOffsetHours = ResolveEffectiveUtcOffsetHours(input);
        _resultGrid.Rows.Add(Localize("UTC effective", "UTC មានប្រសិទ្ធភាព"), $"UTC {utcOffsetHours:+0.##;-0.##;0} ({Localize("hours", "ម៉ោង")})");
        _resultGrid.Rows.Add(Localize("UTC instant", "ពេលវេលា UTC"), FormatUtcInstant(input, utcOffsetHours));
        _resultGrid.Rows.Add(Localize("Location link", "ស្ថានភាពភ្ជាប់ទីតាំង"), $"READY — {input.Country} / {input.Province ?? Localize("manual coordinates", "កូអរដោនេដោយដៃ")}");
        _resultGrid.Rows.Add(Localize("Astronomical / Khmer / Buddhist year", "ឆ្នាំតារាសាស្ត្រ / ឆ្នាំខ្មែរ / ពុទ្ធសករាជ"), $"{calendar.AstronomicalYear} / {calendar.KhmerYear} / {calendar.BuddhistYear}");
        _resultGrid.Rows.Add(Localize("Ahargana / Kammaja", "អហរគណ / កម្មជៈ"), $"{calendar.Ahargana:0.##########} / {calendar.Kammaja:0.##########}");
        _resultGrid.Rows.Add(Localize("Calendar year type", "ប្រភេទឆ្នាំប្រតិទិន"), $"{calendar.YearType} ({calendar.DaysInYear} {Localize("days", "ថ្ងៃ")})");
        _resultGrid.Rows.Add(Localize("Month length / weekday rule", "ច្បាប់ប្រវែងខែ / ថ្ងៃសប្តាហ៍"), $"{calendar.MonthLengthRule} / {calendar.WeekdayAdjustment}");
        _resultGrid.Rows.Add(Localize("Lunar year", "ឆ្នាំចន្ទគតិ"), calendar.LunarYearType);
        _resultGrid.Rows.Add(Localize("Avamana / Masakendra / Bori Tithi", "អវមាណ / មាសកេន្ទ្រ / បុរីទិថី"), $"{calendar.Avamana} / {calendar.Masakendra} / {calendar.BoriTithi}");
        _resultGrid.Rows.Add(Localize("New Era weekday", "ថ្ងៃសប្តាហ៍សករាជថ្មី"), calendar.NewEraWeekday);
        _resultGrid.Rows.Add(Localize("Maha Sankranta", "មហាសង្ក្រាន្ត"), $"{Localize("Day", "ថ្ងៃទី")} {calendar.MahaSankrantaDay}, {calendar.MahaSankrantaTime:hh\\:mm\\:ss}");
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
        foreach (var point in result.AdditionalPoints)
        {
            AddPlanetRow(point);
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

        if (_tabs is not null && _resultTab is not null)
        {
            _tabs.SelectedTab = _resultTab;
        }
    }

    private void DisplayResultLegacy(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        var input = result.BirthInput;
        var calendar = result.KhmerCalendar;
        _resultTitleLabel.Text = $"Calculation results  •  {input.Name}";
        _resultSubtitleLabel.Text =
            $"{input.BirthDate:dd/MM/yyyy} at {input.BirthTime:HH:mm:ss}  •  " +
            $"{input.Province ?? input.Country}  •  {result.Ascendant.SignNameEn} Ascendant";
        _resultGrid.Rows.Clear();
        _resultGrid.Rows.Add("Status", "Input validated. Khmer calendar calculation completed.");
        _resultGrid.Rows.Add("Name", input.Name);
        _resultGrid.Rows.Add("Gender", input.Gender);
        _resultGrid.Rows.Add("Birth date", input.BirthDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
        _resultGrid.Rows.Add("Birth time", input.BirthTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
        _resultGrid.Rows.Add("Location", $"{input.Country} / {input.Province ?? "International location"}");
        _resultGrid.Rows.Add("Coordinates", $"{input.Latitude.ToString(CultureInfo.InvariantCulture)}, {input.Longitude.ToString(CultureInfo.InvariantCulture)}");
        _resultGrid.Rows.Add("Timezone", input.TimeZoneId);
        _resultGrid.Rows.Add("Khmer year / Buddhist year", $"{calendar.KhmerYear} / {calendar.BuddhistYear}");
        _resultGrid.Rows.Add("Ahargana / Kammaja", $"{calendar.Ahargana:0.##########} / {calendar.Kammaja:0.##########}");
        _resultGrid.Rows.Add("Calendar year type", $"{calendar.YearType} ({calendar.DaysInYear} days)");
        _resultGrid.Rows.Add("Lunar year", calendar.LunarYearType);
        _resultGrid.Rows.Add("Avamana / Masakendra / Bori Tithi", $"{calendar.Avamana} / {calendar.Masakendra} / {calendar.BoriTithi}");
        _resultGrid.Rows.Add("New Era weekday", calendar.NewEraWeekday);
        _resultGrid.Rows.Add("Maha Sankranta", $"Day {calendar.MahaSankrantaDay}, {calendar.MahaSankrantaTime:hh\\:mm\\:ss}");
        _resultGrid.Rows.Add("Ascendant", $"{result.Ascendant.SignNameEn} / {result.Ascendant.SignNameKm}, {result.Ascendant.Degree:00}° {result.Ascendant.Minute:00}' {result.Ascendant.Second:00.##}\"; {result.Ascendant.NakshatraName}, Pada {result.Ascendant.Pada}");
        _resultGrid.Rows.Add("Traditional Sun chain", $"{result.TraditionalSun.LongitudeArcMinutes:0.#####} arcminutes (ព្រះអាទិត្យ!B34)");
        _resultGrid.Rows.Add("Traditional Moon chain", $"{result.TraditionalMoon.LongitudeArcMinutes:0.#####} arcminutes (ព្រះចន្ទ!B43)");
        _resultGrid.Rows.Add("Planetary positions", result.Planets.Count == 0
            ? "No planetary positions returned."
            : $"{result.Planets.Count} workbook-derived positions calculated.");
        _resultGrid.Rows.Add("Interpretations", $"{result.Interpretations.Count} workbook house interpretations generated.");
        _resultGrid.Rows.Add("Additional workbook points", $"{result.AdditionalPoints.Count} traditional outer-point positions calculated.");
        _planetGrid.Rows.Clear();
        AddPlanetRow(result.Ascendant);
        foreach (var planet in result.Planets)
        {
            AddPlanetRow(planet);
        }
        foreach (var point in result.AdditionalPoints)
        {
            AddPlanetRow(point);
        }
        _d1Chart.Chart = result.D1;
        _d3Chart.Chart = result.D3;
        _d9Chart.Chart = result.D9;
        PopulateNakshatraGrid(result);
        PopulateCalendarGrid(calendar);
        _interpretationTextBox.Text = BuildInterpretationText(result);
        _resultGrid.ClearSelection();
        _resultGrid.FirstDisplayedScrollingRowIndex = 0;
        _planetGrid.ClearSelection();
        _planetGrid.FirstDisplayedScrollingRowIndex = 0;

        if (_tabs is not null && _resultTab is not null)
        {
            _tabs.SelectedTab = _resultTab;
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

    private void AddPlanetRow(PlanetPosition position)
    {
        _planetGrid.Rows.Add(
            GetBodyDisplayName(position.Body),
            Localize(position.SignNameEn, position.SignNameKm),
            $"{position.Degree:00}° {position.Minute:00}' {position.Second:00.##}\"",
            position.NakshatraName,
            position.Pada,
            position.House,
            position.LongitudeArcMinutes.ToString("0.######", CultureInfo.InvariantCulture));
    }

    private void AddPlanetRowLegacy(PlanetPosition position)
    {
        _planetGrid.Rows.Add(
            GetBodyDisplayName(position.Body),
            $"{position.SignNameEn} / {position.SignNameKm}",
            $"{position.Degree:00}° {position.Minute:00}' {position.Second:00.##}\"",
            position.NakshatraName,
            position.Pada,
            position.House,
            position.LongitudeArcMinutes.ToString("0.######", CultureInfo.InvariantCulture));
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

    private void PopulateNakshatraGridLegacy(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        _nakshatraGrid.Rows.Clear();
        AddNakshatraRow(result.Ascendant);
        foreach (var planet in result.Planets)
        {
            AddNakshatraRow(planet);
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

    private void AddNakshatraRowLegacy(PlanetPosition position)
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
        AddCalendarRow("New Era weekday", "ថ្ងៃសប្តាហ៍សករាជថ្មី", calendar.NewEraWeekday);
        AddCalendarRow("Maha Sankranta", "មហាសង្ក្រាន្ត", $"{Localize("Day", "ថ្ងៃទី")} {calendar.MahaSankrantaDay}, {calendar.MahaSankrantaTime:hh\\:mm\\:ss}");
        AddCalendarRow("Next Maha Sankranta", "មហាសង្ក្រាន្តបន្ទាប់", $"{Localize("Day", "ថ្ងៃទី")} {calendar.NextMahaSankrantaDay}, {calendar.NextMahaSankrantaTime:hh\\:mm\\:ss} ({calendar.NextMahaSankrantaWeekday})");
        _calendarGrid.ClearSelection();
    }

    private void AddCalendarRow(string english, string khmer, object value)
    {
        _calendarGrid.Rows.Add(Localize(english, khmer), value);
    }

    private void PopulateAtthabhujjGrid(KhmerCalendarResult calendar)
    {
        var januaryMonthLength = calendar.Kammaja <= 207D ? 30 : 29;
        var jyeshthaMonthLength = calendar.Kammaja <= 207D
            ? calendar.Avamana <= 125 ? 30 : 29
            : calendar.Avamana <= 136 ? 30 : 29;
        SetAtthabhujjValue("chulaSakaraj", calendar.KhmerYear);
        SetAtthabhujjValue("buddhistEra", calendar.BuddhistYear);
        SetAtthabhujjValue("aharganaRemainder", calendar.SolarYearFraction.ToString("0.##########", CultureInfo.InvariantCulture));
        SetAtthabhujjValue("ahargana", calendar.Ahargana.ToString("0.##########", CultureInfo.InvariantCulture));
        SetAtthabhujjValue("kammaja", calendar.Kammaja.ToString("0.##########", CultureInfo.InvariantCulture));
        SetAtthabhujjValue("uccabala", calendar.Uccabal);
        SetAtthabhujjValue("avamana", calendar.Avamana);
        SetAtthabhujjValue("masakendra", calendar.Masakendra);
        SetAtthabhujjValue("boriTithi", calendar.BoriTithi);
        SetAtthabhujjValue("newEraDayNumber", calendar.NewEraDay);
        SetAtthabhujjValue("yearType", LocalizeCalendarValue(calendar.YearType));
        SetAtthabhujjValue("daysInYear", calendar.DaysInYear);
        SetAtthabhujjValue("januaryLength", januaryMonthLength);
        SetAtthabhujjValue("jyeshthaLength", jyeshthaMonthLength);
        SetAtthabhujjValue("nextWeekdayRule", calendar.WeekdayAdjustment);
        SetAtthabhujjValue("lunarYearType", LocalizeCalendarValue(calendar.LunarYearType));
        SetAtthabhujjValue("riseOfSak", FormatDayAndTime(calendar.NextMahaSankrantaDay, calendar.NextMahaSankrantaTime));
        SetAtthabhujjValue("mahaSankranta", FormatDayAndTime(calendar.MahaSankrantaDay, calendar.MahaSankrantaTime));
        SetAtthabhujjValue("mahaSankrantaWeekday", LocalizeWeekday(calendar.NewEraWeekday));
        SetAtthabhujjValue("riseOfSakWeekday", LocalizeWeekday(calendar.NextMahaSankrantaWeekday));
        SetAtthabhujjValue("sourceStatus", Localize("Atthabhujj!A1:J47", "អដ្ឋភុជ្ជ!A1:J47"));
        SetAtthabhujjValue("formulaStatus", Localize("Workbook-derived", "ផ្អែកលើរូបមន្តសៀវភៅការងារ"));
        SetAtthabhujjValue("resultStatus", Localize("READY", "រួចរាល់"));
    }

    private void SetAtthabhujjValue(string key, object value)
    {
        if (_atthabhujjValues.TryGetValue(key, out var label))
        {
            label.Text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "—";
        }
    }

    private string LocalizeCalendarValue(string value) => value switch
    {
        "Leap Year" => Localize("Leap Year", "ឆ្នាំអធិកសុទិន"),
        "Common Year" => Localize("Common Year", "ឆ្នាំសុទិនធម្មតា"),
        "13-Month Lunar Year" => Localize("13-Month Lunar Year", "ឆ្នាំចន្ទគតិ ១៣ ខែ"),
        "12-Month Lunar Year" => Localize("12-Month Lunar Year", "ឆ្នាំចន្ទគតិ ១២ ខែ"),
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

    private string FormatDayAndTime(int day, TimeSpan time) =>
        $"{Localize("Day", "ថ្ងៃទី")} {day}, {time:hh\\:mm\\:ss}";

    private void PopulateCalendarGridLegacy(KhmerCalendarResult calendar)
    {
        _calendarGrid.Rows.Clear();
        _calendarGrid.Rows.Add("Astronomical year", calendar.AstronomicalYear);
        _calendarGrid.Rows.Add("Khmer year", calendar.KhmerYear);
        _calendarGrid.Rows.Add("Buddhist year", calendar.BuddhistYear);
        _calendarGrid.Rows.Add("Weekday", calendar.Weekday);
        if (!string.IsNullOrWhiteSpace(calendar.TraditionalDate))
        {
            _calendarGrid.Rows.Add("Traditional date", calendar.TraditionalDate);
            _calendarGrid.Rows.Add("Lunar day / phase", $"{calendar.LunarDayDisplay} — {calendar.TithiName}");
            _calendarGrid.Rows.Add("Lunar month", $"{calendar.LunarMonthNameEn} ({calendar.LunarMonthNumber})");
            _calendarGrid.Rows.Add("Animal year", calendar.AnimalYear);
            _calendarGrid.Rows.Add("Sesa-Kala-Yoga / Yuga", $"{calendar.SesaKalaYoga} / {calendar.Yuga}");
        }
        _calendarGrid.Rows.Add("Ahargana", calendar.Ahargana.ToString("0.##########", CultureInfo.InvariantCulture));
        _calendarGrid.Rows.Add("Ahargana day", calendar.AharganaDay);
        _calendarGrid.Rows.Add("Kammaja", calendar.Kammaja.ToString("0.##########", CultureInfo.InvariantCulture));
        _calendarGrid.Rows.Add("Year type", $"{calendar.YearType} ({calendar.DaysInYear} days)");
        _calendarGrid.Rows.Add("Lunar year type", calendar.LunarYearType);
        _calendarGrid.Rows.Add("Avamana / Masakendra / Bori Tithi", $"{calendar.Avamana} / {calendar.Masakendra} / {calendar.BoriTithi}");
        _calendarGrid.Rows.Add("New Era weekday", calendar.NewEraWeekday);
        _calendarGrid.Rows.Add("Maha Sankranta", $"Day {calendar.MahaSankrantaDay}, {calendar.MahaSankrantaTime:hh\\:mm\\:ss}");
        _calendarGrid.Rows.Add("Next Maha Sankranta", $"Day {calendar.NextMahaSankrantaDay}, {calendar.NextMahaSankrantaTime:hh\\:mm\\:ss} ({calendar.NextMahaSankrantaWeekday})");
        _calendarGrid.ClearSelection();
    }

    private string BuildInterpretationText(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        var ascendantSign = Localize(result.Ascendant.SignNameEn, result.Ascendant.SignNameKm);
        var header = $"{Localize("Workbook-backed interpretation for", "ការបកស្រាយផ្អែកលើសៀវភៅការងារសម្រាប់")} {result.BirthInput.Name}\r\n" +
                     $"{Localize("Ascendant", "លគ្គនៈ")}: {ascendantSign}\r\n\r\n";
        var entries = result.Interpretations.Select(interpretation =>
            $"{GetBodyDisplayName(interpretation.Body)} — {Localize("House", "ឋាន")} {interpretation.House}\r\n" +
            $"{interpretation.Description}\r\n" +
            $"{Localize("Source", "ប្រភព")}: {interpretation.Source}");
        return header + string.Join("\r\n\r\n", entries);
    }

    private string BuildInterpretationTextLegacy(KhmerAstrology.Application.DTOs.AstrologyResult result)
    {
        var header = $"Workbook-backed interpretation for {result.BirthInput.Name}\r\n" +
                     $"Ascendant: {result.Ascendant.SignNameEn} / {result.Ascendant.SignNameKm}\r\n\r\n";
        var entries = result.Interpretations.Select(interpretation =>
            $"{GetBodyDisplayName(interpretation.Body)} — House {interpretation.House}\r\n" +
            $"{interpretation.Description}\r\n" +
            $"Source: {interpretation.Source}");
        return header + string.Join("\r\n\r\n", entries);
    }

    private static string GetBodyDisplayNameLegacy(CelestialBody body) => body switch
    {
        CelestialBody.KetuVeda => "Ketu Veda / កេតុវេទ",
        CelestialBody.KetuDivya => "Ketu Divya / កេតុទិព្វ",
        CelestialBody.Mrityu => "Mrityu / ម្រឹត្យូវ",
        CelestialBody.Varuna => "Varuna / ព្រះវរុណ",
        CelestialBody.Yama => "Yama / ព្រះយម",
        _ => body.ToString(),
    };

    private string GetBodyDisplayName(CelestialBody body) => IsKhmer
        ? body switch
        {
            CelestialBody.Ascendant => "លគ្គនៈ",
            CelestialBody.Sun => "ព្រះអាទិត្យ",
            CelestialBody.Moon => "ព្រះចន្ទ",
            CelestialBody.Mars => "ព្រះអង្គារ",
            CelestialBody.Mercury => "ព្រះពុធ",
            CelestialBody.Jupiter => "ព្រះព្រហស្បតិ៍",
            CelestialBody.Venus => "ព្រះសុក្រ",
            CelestialBody.Saturn => "ព្រះសៅរ៍",
            CelestialBody.Rahu => "រាហូ",
            CelestialBody.Ketu => "ព្រះកេតុ",
            CelestialBody.KetuVeda => "កេតុវេទ",
            CelestialBody.KetuDivya => "កេតុទិព្វ",
            CelestialBody.Uranus => "អ៊ុយរ៉ានុស",
            CelestialBody.Neptune => "ណិបទូន",
            CelestialBody.Pluto => "ភ្លុយតូ",
            CelestialBody.Mrityu => "ម្រឹត្យូវ",
            CelestialBody.Varuna => "ព្រះវរុណ",
            CelestialBody.Yama => "ព្រះយម",
            _ => body.ToString(),
        }
        : body switch
        {
            CelestialBody.KetuVeda => "Ketu Veda",
            CelestialBody.KetuDivya => "Ketu Divya",
            CelestialBody.Mrityu => "Mrityu",
            CelestialBody.Varuna => "Varuna",
            CelestialBody.Yama => "Yama",
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

        if (_planetGrid.Rows.Count > 0)
        {
            _planetGrid.Rows[0].SetValues(
                "—", "—", "—",
                Localize("No calculation yet", "មិនទាន់មានការគណនាទេ"),
                "—", "—", "—");
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
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.ColumnHeadersHeight = 38;
        grid.EnableHeadersVisualStyles = false;
        grid.GridColor = Border;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.DefaultCellStyle.BackColor = Surface;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.SelectionBackColor = BrandBlueLight;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
        grid.Dock = DockStyle.Fill;
        grid.RowTemplate.Height = 36;
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
