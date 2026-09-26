using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using KhmerAstrology.Domain.Enums;
using KhmerAstrology.Domain.Models;

namespace KhmerAstrology.WinForms;

/// <summary>
/// Responsive radial chart for the workbook-backed D1, D3 and D9 results.
/// The control only renders the supplied chart; all divisional calculations
/// remain in the calculation/application layers.
/// </summary>
public sealed class HoroscopeChartControl : UserControl
{
    private const float StartAngle = -90F;
    private const float MinimumPlacementFontSize = 6.5F;

    // Radii as fractions of the outer radius.
    private const float SignBandInner = 0.80F;
    private const float SignNameRadius = 0.90F;
    private const float HouseNumberRadius = 0.745F;
    private const float PlacementOuter = 0.69F;
    private const float PlacementInner = 0.35F;
    private const float CenterRadius = 0.22F;

    private static readonly Color LagnaFill = Color.FromArgb(255, 247, 228);
    private static readonly Color HoverFill = Color.FromArgb(226, 236, 252);

    private readonly ToolTip _toolTip = new() { InitialDelay = 0, ReshowDelay = 0, UseAnimation = false, UseFading = false };
    private AstrologyChart? _chart;
    private bool _isKhmer;
    private int _hoverIndex = -1;
    private PointF _center;
    private float _outerRadius;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AstrologyChart? Chart
    {
        get => _chart;
        set
        {
            _chart = value;
            ClearHover();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsKhmer
    {
        get => _isKhmer;
        set
        {
            if (_isKhmer != value)
            {
                _isKhmer = value;
                ClearHover();
                Invalidate();
            }
        }
    }

    public HoroscopeChartControl()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Dock = DockStyle.Fill;
        MinimumSize = new Size(250, 300);
        ResizeRedraw = true;
    }

    /// <summary>Short label drawn in the chart for a body (also used by the legend).</summary>
    public static string GetPlacementCode(CelestialBody body, bool isKhmer) =>
        isKhmer ? FormatPlacementKhmer(body) : FormatPlacement(body);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _toolTip.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        graphics.Clear(BackColor);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        if (_chart is null || _chart.Houses.Count != 12)
        {
            using var emptyBrush = new SolidBrush(Color.FromArgb(91, 104, 125));
            var emptyText = _isKhmer
                ? "សូមគណនាហោរាសាស្ត្រ ដើម្បីបង្ហាញតារាងនេះ។"
                : "Calculate a horoscope to display this chart.";
            graphics.DrawString(emptyText, Font, emptyBrush, new PointF(16, 16));
            return;
        }

        var margin = Math.Max(10F, Math.Min(Width, Height) * 0.03F);
        var outerRadius = Math.Max(0F, Math.Min(Width, Height) / 2F - margin);
        var center = new PointF(Width / 2F, Height / 2F);
        _center = center;
        _outerRadius = outerRadius;
        if (outerRadius < 40F)
        {
            return;
        }

        using var outerPen = new Pen(Color.FromArgb(63, 81, 181), Math.Max(1.7F, outerRadius / 105F));
        using var dividerPen = new Pen(Color.FromArgb(160, 172, 232), Math.Max(1F, outerRadius / 170F));
        using var bandPen = new Pen(Color.FromArgb(102, 119, 224), Math.Max(1F, outerRadius / 150F));
        using var ringPen = new Pen(Color.FromArgb(218, 167, 39), Math.Max(1F, outerRadius / 155F));
        using var centerPen = new Pen(Color.FromArgb(218, 167, 39), Math.Max(1.5F, outerRadius / 105F));
        using var signBrush = new SolidBrush(Color.FromArgb(25, 33, 52));
        using var placementBrush = new SolidBrush(Color.FromArgb(20, 28, 45));
        using var ascendantBrush = new SolidBrush(Color.FromArgb(174, 113, 12));
        using var houseBrush = new SolidBrush(Color.FromArgb(120, 132, 152));
        using var chartBrush = new SolidBrush(Color.FromArgb(79, 70, 229));
        var scale = Math.Clamp(outerRadius / 180F, 0.8F, 1.6F);
        using var signFont = new Font(Font.FontFamily, Math.Max(8F, Font.Size * 0.84F * scale), FontStyle.Bold);
        using var houseFont = new Font(Font.FontFamily, Math.Max(7F, Font.Size * 0.66F * scale));
        using var chartFont = new Font(Font.FontFamily, Math.Max(16F, outerRadius * CenterRadius * 0.62F), FontStyle.Bold);
        var basePlacementSize = Math.Max(8F, Font.Size * 0.86F * scale);

        var chartBounds = Circle(center, outerRadius);

        // Sector fills: Lagna (house 1) is tinted, the hovered house highlighted.
        for (var index = 0; index < 12; index++)
        {
            var fill = index == _hoverIndex
                ? HoverFill
                : index == 0
                    ? LagnaFill
                    : index % 2 == 0 ? Color.FromArgb(250, 252, 255) : Color.FromArgb(242, 246, 253);
            using var sectorBrush = new SolidBrush(fill);
            // Sector i spans StartAngle - 30i ± 15°; the sweep runs counter-clockwise.
            graphics.FillPie(sectorBrush, chartBounds, StartAngle - index * 30F + 15F, -30F);
        }

        graphics.DrawEllipse(bandPen, Circle(center, outerRadius * SignBandInner));
        graphics.DrawEllipse(outerPen, chartBounds);

        for (var index = 0; index < 12; index++)
        {
            var boundaryAngle = DegreesToRadians(StartAngle - index * 30F - 15F);
            graphics.DrawLine(
                dividerPen,
                PointOnCircle(center, outerRadius * CenterRadius, boundaryAngle),
                PointOnCircle(center, outerRadius, boundaryAngle));
        }

        for (var index = 0; index < 12; index++)
        {
            var house = _chart.Houses[index];
            var sectorAngle = DegreesToRadians(StartAngle - index * 30F);

            var signName = _isKhmer && !string.IsNullOrWhiteSpace(house.SignNameKm)
                ? house.SignNameKm
                : house.SignNameEn;
            // Horizontal room in the sign band depends on the sector's direction:
            // side sectors only have the band's radial thickness.
            var bandWidth = (outerRadius * (1F - SignBandInner) * Math.Abs((float)Math.Cos(sectorAngle))
                + outerRadius * 0.47F * Math.Abs((float)Math.Sin(sectorAngle))) * 0.95F;
            if (!_isKhmer && signName.Length > 3 && graphics.MeasureString(signName, signFont).Width > bandWidth)
            {
                signName = signName[..3];
            }
            DrawCenteredText(
                graphics,
                signName,
                signBrush,
                signFont,
                PointOnCircle(center, outerRadius * SignNameRadius, sectorAngle),
                maxWidth: Math.Max(bandWidth, outerRadius * 0.2F));
            DrawCenteredText(
                graphics,
                _isKhmer ? ToKhmerDigits(house.HouseNumber) : house.HouseNumber.ToString(),
                houseBrush,
                houseFont,
                PointOnCircle(center, outerRadius * HouseNumberRadius, sectorAngle));

            DrawPlacementGrid(
                graphics,
                house.Placements,
                sectorAngle,
                basePlacementSize,
                placementBrush,
                ascendantBrush);
        }

        graphics.DrawEllipse(ringPen, Circle(center, outerRadius * PlacementInner * 0.92F));
        using (var centerBrush = new SolidBrush(Color.White))
        {
            graphics.FillEllipse(centerBrush, Circle(center, outerRadius * CenterRadius));
        }
        graphics.DrawEllipse(centerPen, Circle(center, outerRadius * CenterRadius));
        DrawCenteredText(graphics, _chart.ChartType.ToString(), chartBrush, chartFont, center);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = HitTestHouse(e.Location);
        if (index == _hoverIndex)
        {
            return;
        }

        _hoverIndex = index;
        Invalidate();
        if (index < 0 || _chart is null)
        {
            _toolTip.Hide(this);
            return;
        }

        _toolTip.Show(BuildHouseDetails(_chart.Houses[index]), this, e.X + 18, e.Y + 18);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        ClearHover();
    }

    private void ClearHover()
    {
        if (_hoverIndex >= 0)
        {
            _hoverIndex = -1;
            Invalidate();
        }
        _toolTip.Hide(this);
    }

    private int HitTestHouse(Point location)
    {
        if (_chart is null || _outerRadius <= 0F)
        {
            return -1;
        }

        var dx = location.X - _center.X;
        var dy = location.Y - _center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < _outerRadius * CenterRadius || distance > _outerRadius)
        {
            return -1;
        }

        // Sector i is centred on StartAngle - 30 * i (screen coordinates).
        var angle = Math.Atan2(dy, dx) * 180D / Math.PI;
        var index = (int)Math.Round((StartAngle - angle) / 30D);
        return ((index % 12) + 12) % 12;
    }

    private string BuildHouseDetails(AstrologyChartHouse house)
    {
        var signName = _isKhmer ? house.SignNameKm : house.SignNameEn;
        var houseNumber = _isKhmer ? ToKhmerDigits(house.HouseNumber) : house.HouseNumber.ToString();
        var header = _isKhmer
            ? $"ឋាន {houseNumber}  •  {signName}  ({_chart!.ChartType})"
            : $"House {houseNumber}  •  {signName}  ({_chart!.ChartType})";
        if (house.Placements.Count == 0)
        {
            return header + Environment.NewLine + (_isKhmer ? "គ្មានតារាគ្រោះ" : "No planets or points");
        }

        var lines = house.Placements.Select(placement =>
            $"{GetPlacementCode(placement.Body, _isKhmer)}  {CelestialBodyNames.Get(placement.Body, _isKhmer)} — {FormatD1Position(placement.LongitudeArcMinutes)}");
        return header + Environment.NewLine + string.Join(Environment.NewLine, lines);
    }

    private string FormatD1Position(double longitudeArcMinutes)
    {
        var normalized = ((longitudeArcMinutes % 21_600D) + 21_600D) % 21_600D;
        var signNumber = (int)(normalized / 1_800D) + 1;
        var withinSign = normalized % 1_800D;
        var degree = (int)(withinSign / 60D);
        var minute = (int)(withinSign % 60D);
        var sign = _chart!.Houses.FirstOrDefault(house => house.SignNumber == signNumber);
        var signName = sign is null ? signNumber.ToString() : _isKhmer ? sign.SignNameKm : sign.SignNameEn;
        return _isKhmer
            ? $"{signName} {ToKhmerDigits(degree)}°{ToKhmerDigits(minute, 2)}′ (D1)"
            : $"{signName} {degree}°{minute:00}′ (D1)";
    }

    /// <summary>
    /// Lays the sector's labels out in the grid (1–3 columns) that needs the least
    /// shrinking to fit between the inner ring and the house-number ring.
    /// </summary>
    private void DrawPlacementGrid(
        Graphics graphics,
        IReadOnlyList<AstrologyChartPlacement> placements,
        double sectorAngle,
        float baseFontSize,
        Brush placementBrush,
        Brush ascendantBrush)
    {
        if (placements.Count == 0)
        {
            return;
        }

        var labels = placements.Select(placement => (placement, text: GetPlacementCode(placement.Body, _isKhmer))).ToArray();
        var radialLength = _outerRadius * (PlacementOuter - PlacementInner);
        var middleRadius = _outerRadius * (PlacementOuter + PlacementInner) / 2F;
        var tangentialLength = 2F * middleRadius * (float)Math.Sin(DegreesToRadians(15F)) * 0.9F;
        var cos = Math.Abs((float)Math.Cos(sectorAngle));
        var sin = Math.Abs((float)Math.Sin(sectorAngle));
        var availableWidth = radialLength * cos + tangentialLength * sin;
        var availableHeight = radialLength * sin + tangentialLength * cos;

        using var baseFont = new Font(Font.FontFamily, baseFontSize, FontStyle.Bold);
        var cellWidth = labels.Max(label => graphics.MeasureString(label.text, baseFont).Width) + 2F;
        var lineHeight = baseFont.GetHeight(graphics) * 0.95F;

        var bestColumns = 1;
        var bestScale = 0F;
        for (var columns = 1; columns <= Math.Min(3, labels.Length); columns++)
        {
            var rows = (labels.Length + columns - 1) / columns;
            var fit = Math.Min(1F, Math.Min(availableWidth / (columns * cellWidth), availableHeight / (rows * lineHeight)));
            if (fit > bestScale + 0.01F)
            {
                bestScale = fit;
                bestColumns = columns;
            }
        }

        var fontSize = Math.Max(MinimumPlacementFontSize, baseFontSize * bestScale);
        using var font = new Font(Font.FontFamily, fontSize, FontStyle.Bold);
        var ratio = fontSize / baseFontSize;
        var scaledCell = cellWidth * ratio;
        var scaledLine = lineHeight * ratio;
        var rowCount = (labels.Length + bestColumns - 1) / bestColumns;
        var gridCenter = PointOnCircle(_center, middleRadius, sectorAngle);

        for (var index = 0; index < labels.Length; index++)
        {
            var row = index / bestColumns;
            var column = index % bestColumns;
            var columnsInRow = Math.Min(bestColumns, labels.Length - row * bestColumns);
            var x = gridCenter.X + (column - (columnsInRow - 1) / 2F) * scaledCell;
            var y = gridCenter.Y + (row - (rowCount - 1) / 2F) * scaledLine;
            var brush = labels[index].placement.Body == CelestialBody.Ascendant ? ascendantBrush : placementBrush;
            DrawCenteredText(graphics, labels[index].text, brush, font, new PointF(x, y));
        }
    }

    private static RectangleF Circle(PointF center, float radius) =>
        new(center.X - radius, center.Y - radius, radius * 2F, radius * 2F);

    private static double DegreesToRadians(float degrees) => degrees * Math.PI / 180D;

    private static PointF PointOnCircle(PointF center, float radius, double angle) =>
        new(center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle));

    private static void DrawCenteredText(
        Graphics graphics,
        string text,
        Brush brush,
        Font font,
        PointF center,
        float? maxWidth = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var displayText = text;
        var measured = graphics.MeasureString(displayText, font);
        if (maxWidth is > 0F && measured.Width > maxWidth.Value && displayText.Length > 2)
        {
            var allowedCharacters = Math.Max(2, (int)(displayText.Length * maxWidth.Value / measured.Width));
            displayText = displayText[..Math.Min(displayText.Length, allowedCharacters)].TrimEnd() + "…";
            measured = graphics.MeasureString(displayText, font);
        }

        graphics.DrawString(
            displayText,
            font,
            brush,
            center.X - measured.Width / 2F,
            center.Y - measured.Height / 2F);
    }

    private static string ToKhmerDigits(int value, int minimumDigits = 1) =>
        string.Concat(value.ToString(new string('0', minimumDigits)).Select(digit => (char)('០' + (digit - '0'))));

    private static string FormatPlacement(CelestialBody body) => body switch
    {
        CelestialBody.Ascendant => "Asc",
        CelestialBody.Sun => "Su",
        CelestialBody.Moon => "Mo",
        CelestialBody.Mars => "Ma",
        CelestialBody.Mercury => "Me",
        CelestialBody.Jupiter => "Ju",
        CelestialBody.Venus => "Ve",
        CelestialBody.Saturn => "Sa",
        CelestialBody.Rahu => "Ra",
        CelestialBody.Ketu => "Ke",
        CelestialBody.KetuVeda => "KeV",
        CelestialBody.KetuDivya => "KeD",
        CelestialBody.Uranus => "Ur",
        CelestialBody.Neptune => "Ne",
        CelestialBody.Pluto => "Pl",
        CelestialBody.Mrityu => "Mr",
        CelestialBody.Varuna => "Va",
        CelestialBody.Yama => "Ya",
        _ => body.ToString()[..Math.Min(2, body.ToString().Length)],
    };

    // Planet codes from the workbook sheet `រាសិចក្ក D1-D9-D3` (A5:A18). That sheet
    // codes the សូរ្យយាត្រ!B28:B30 rows (Uranus/Neptune/Pluto here) as ០/វ/យ; the
    // separate traditional Mrityu/Varuna/Yama points keep Latin abbreviations.
    private static string FormatPlacementKhmer(CelestialBody body) => body switch
    {
        CelestialBody.Ascendant => "ល",
        CelestialBody.Sun => "១",
        CelestialBody.Moon => "២",
        CelestialBody.Mars => "៣",
        CelestialBody.Mercury => "៤",
        CelestialBody.Jupiter => "៥",
        CelestialBody.Venus => "៦",
        CelestialBody.Saturn => "៧",
        CelestialBody.Rahu => "៨",
        CelestialBody.Ketu or CelestialBody.KetuVeda or CelestialBody.KetuDivya => "៩",
        CelestialBody.Uranus => "០",
        CelestialBody.Neptune => "វ",
        CelestialBody.Pluto => "យ",
        _ => FormatPlacement(body),
    };
}
