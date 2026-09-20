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
    private AstrologyChart? _chart;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AstrologyChart? Chart
    {
        get => _chart;
        set
        {
            _chart = value;
            Invalidate();
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

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        if (_chart is null || _chart.Houses.Count != 12)
        {
            using var emptyBrush = new SolidBrush(Color.FromArgb(91, 104, 125));
            e.Graphics.DrawString(
                "Calculate a horoscope to display this chart.",
                Font,
                emptyBrush,
                new PointF(16, 16));
            return;
        }

        var margin = Math.Max(14F, Math.Min(Width, Height) * 0.055F);
        var diameter = Math.Max(0F, Math.Min(Width, Height) - margin * 2F);
        var center = new PointF(Width / 2F, Height / 2F);
        var outerRadius = diameter / 2F;
        var centerRadius = outerRadius * 0.235F;
        var signRadius = outerRadius * 0.835F;
        var houseRadius = outerRadius * 0.675F;
        var placementRadius = outerRadius * 0.455F;
        const float startAngle = -90F;

        using var outerPen = new Pen(Color.FromArgb(63, 81, 181), Math.Max(1.7F, outerRadius / 105F));
        using var dividerPen = new Pen(Color.FromArgb(102, 119, 224), Math.Max(1F, outerRadius / 150F));
        using var ringPen = new Pen(Color.FromArgb(218, 167, 39), Math.Max(1F, outerRadius / 155F));
        using var centerPen = new Pen(Color.FromArgb(218, 167, 39), Math.Max(1.5F, outerRadius / 105F));
        using var centerBrush = new SolidBrush(Color.White);
        using var signBrush = new SolidBrush(Color.FromArgb(25, 33, 52));
        using var placementBrush = new SolidBrush(Color.FromArgb(20, 28, 45));
        using var ascendantBrush = new SolidBrush(Color.FromArgb(174, 113, 12));
        using var houseBrush = new SolidBrush(Color.FromArgb(91, 104, 125));
        using var chartBrush = new SolidBrush(Color.FromArgb(79, 70, 229));
        using var signFont = new Font(Font.FontFamily, Math.Max(8F, Font.Size * 0.86F), FontStyle.Bold);
        using var houseFont = new Font(Font.FontFamily, Math.Max(7F, Font.Size * 0.70F));
        using var placementFont = new Font(Font.FontFamily, Math.Max(8F, Font.Size * 0.82F), FontStyle.Bold);
        using var chartFont = new Font(Font.FontFamily, Math.Max(18F, outerRadius * 0.18F), FontStyle.Bold);

        var chartBounds = new RectangleF(
            center.X - outerRadius,
            center.Y - outerRadius,
            outerRadius * 2F,
            outerRadius * 2F);

        // Alternating fills make the house boundaries easier to follow without
        // competing with the planet labels.
        for (var index = 0; index < 12; index++)
        {
            using var sectorBrush = new SolidBrush(index % 2 == 0
                ? Color.FromArgb(250, 252, 255)
                : Color.FromArgb(242, 246, 253));
            var sectorStart = startAngle - index * 30F - 15F;
            e.Graphics.FillPie(sectorBrush, chartBounds, sectorStart, -30F);
        }

        e.Graphics.DrawEllipse(outerPen, chartBounds);

        for (var index = 0; index < 12; index++)
        {
            var boundaryAngle = DegreesToRadians(startAngle - index * 30F - 15F);
            e.Graphics.DrawLine(
                dividerPen,
                PointOnCircle(center, centerRadius, boundaryAngle),
                PointOnCircle(center, outerRadius, boundaryAngle));

            var house = _chart.Houses[index];
            var sectorAngle = DegreesToRadians(startAngle - index * 30F);
            var signPoint = PointOnCircle(center, signRadius, sectorAngle);
            var housePoint = PointOnCircle(center, houseRadius, sectorAngle);
            var placementPoint = PointOnCircle(center, placementRadius, sectorAngle);

            DrawCenteredText(e.Graphics, house.SignNameEn, signBrush, signFont, signPoint);
            DrawCenteredText(e.Graphics, $"H{house.HouseNumber}", houseBrush, houseFont, housePoint);

            DrawPlacementLabels(
                e.Graphics,
                house.Placements,
                placementPoint,
                placementFont,
                placementBrush,
                ascendantBrush,
                outerRadius * 0.22F);
        }

        // A subtle inner ring separates the placement area from the chart title.
        var ringRadius = centerRadius * 1.55F;
        e.Graphics.DrawEllipse(
            ringPen,
            center.X - ringRadius,
            center.Y - ringRadius,
            ringRadius * 2F,
            ringRadius * 2F);

        e.Graphics.FillEllipse(
            centerBrush,
            center.X - centerRadius,
            center.Y - centerRadius,
            centerRadius * 2F,
            centerRadius * 2F);
        e.Graphics.DrawEllipse(
            centerPen,
            center.X - centerRadius,
            center.Y - centerRadius,
            centerRadius * 2F,
            centerRadius * 2F);
        DrawCenteredText(e.Graphics, _chart.ChartType.ToString(), chartBrush, chartFont, center);
    }

    private static void DrawPlacementLabels(
        Graphics graphics,
        IReadOnlyList<AstrologyChartPlacement> placements,
        PointF center,
        Font font,
        Brush placementBrush,
        Brush ascendantBrush,
        float maxWidth)
    {
        if (placements.Count == 0)
        {
            return;
        }

        var labels = placements
            .Select(placement => (placement, label: FormatPlacement(placement)))
            .ToArray();
        var lineHeight = Math.Max(font.GetHeight(graphics) * 0.92F, 11F);
        var firstLineY = center.Y - (labels.Length - 1) * lineHeight / 2F;

        for (var index = 0; index < labels.Length; index++)
        {
            var brush = labels[index].placement.Body == CelestialBody.Ascendant
                ? ascendantBrush
                : placementBrush;
            DrawCenteredText(
                graphics,
                labels[index].label,
                brush,
                font,
                new PointF(center.X, firstLineY + index * lineHeight),
                maxWidth);
        }
    }

    private static double DegreesToRadians(float degrees) => degrees * Math.PI / 180D;

    private static PointF PointOnCircle(PointF center, float radius, double angle)
    {
        return new PointF(
            center.X + radius * (float)Math.Cos(angle),
            center.Y + radius * (float)Math.Sin(angle));
    }

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

    private static string FormatPlacement(AstrologyChartPlacement placement) => placement.Body switch
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
        _ => placement.Body.ToString()[..Math.Min(2, placement.Body.ToString().Length)],
    };
}
