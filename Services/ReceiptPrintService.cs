using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using RajasvSchoolManagement.Models;

namespace RajasvSchoolManagement.Services
{
    public static class ReceiptPrintService
    {
        public static void PreviewAndPrint(FeePaymentReceipt receipt, SchoolSettings settings)
        {
            if (receipt == null) throw new ArgumentNullException("receipt");
            settings = settings ?? new SchoolSettings();

            PrintDocument document = new PrintDocument();
            document.DocumentName = "Fee Receipt " + receipt.ReceiptNumber;
            document.PrintPage += delegate(object sender, PrintPageEventArgs e)
            {
                DrawReceipt(e.Graphics, e.MarginBounds, receipt, settings);
                e.HasMorePages = false;
            };

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = document;
                preview.Width = 1000;
                preview.Height = 700;
                preview.ShowDialog();
            }
        }

        private static void DrawReceipt(Graphics g, Rectangle bounds, FeePaymentReceipt receipt, SchoolSettings settings)
        {
            Font title = new Font("Segoe UI", 16, FontStyle.Bold);
            Font heading = new Font("Segoe UI", 10, FontStyle.Bold);
            Font normal = new Font("Segoe UI", 10);
            Pen pen = Pens.Black;
            float y = bounds.Top;

            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(settings.SchoolName, title, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 30), center);
            y += 32;
            if (!string.IsNullOrWhiteSpace(settings.Address))
            {
                g.DrawString(settings.Address, normal, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 25), center);
                y += 24;
            }
            g.DrawString("FEE RECEIPT", heading, Brushes.Black, new RectangleF(bounds.Left, y, bounds.Width, 22), center);
            y += 30;
            g.DrawLine(pen, bounds.Left, y, bounds.Right, y);
            y += 10;

            DrawPair(g, normal, heading, bounds.Left, ref y, "Receipt No:", receipt.ReceiptNumber, "Date:", receipt.ReceiptDate.ToString("dd-MMM-yyyy HH:mm"), bounds.Width);
            DrawPair(g, normal, heading, bounds.Left, ref y, "Student:", receipt.StudentName, "Roll No:", receipt.RollNumber, bounds.Width);
            DrawPair(g, normal, heading, bounds.Left, ref y, "Session:", receipt.SessionName, "Class:", receipt.ClassSectionName, bounds.Width);
            DrawPair(g, normal, heading, bounds.Left, ref y, "Mode:", receipt.PaymentMethod, "Transaction:", receipt.TransactionId, bounds.Width);
            y += 6;

            g.DrawLine(pen, bounds.Left, y, bounds.Right, y);
            y += 8;
            g.DrawString("Fee Head / Month", heading, Brushes.Black, bounds.Left, y);
            g.DrawString("Paid", heading, Brushes.Black, bounds.Right - 120, y);
            y += 22;

            foreach (FeePaymentReceiptItem item in receipt.Items)
            {
                string label = item.FeeHeadName + (string.IsNullOrWhiteSpace(item.FeeMonth) ? "" : " - " + item.FeeMonth);
                g.DrawString(label, normal, Brushes.Black, bounds.Left, y);
                g.DrawString(item.PaidAmount.ToString("0.00"), normal, Brushes.Black, bounds.Right - 120, y);
                y += 20;
            }

            y += 5;
            g.DrawLine(pen, bounds.Left, y, bounds.Right, y);
            y += 8;
            g.DrawString("Total Paid:", heading, Brushes.Black, bounds.Right - 260, y);
            g.DrawString(receipt.ReceiptAmount.ToString("0.00"), heading, Brushes.Black, bounds.Right - 120, y);
            y += 30;
            g.DrawString("Received By: " + receipt.ReceivedBy, normal, Brushes.Black, bounds.Left, y);
            y += 22;
            if (!string.IsNullOrWhiteSpace(receipt.Remarks))
            {
                g.DrawString("Remarks: " + receipt.Remarks, normal, Brushes.Black, bounds.Left, y);
                y += 22;
            }
            y += 20;
            g.DrawString("Computer generated receipt", normal, Brushes.Gray, new RectangleF(bounds.Left, y, bounds.Width, 20), center);

            title.Dispose();
            heading.Dispose();
            normal.Dispose();
            center.Dispose();
        }

        private static void DrawPair(Graphics g, Font normal, Font heading, float left, ref float y, string label1, string value1, string label2, string value2, float width)
        {
            float half = width / 2f;
            g.DrawString(label1, heading, Brushes.Black, left, y);
            g.DrawString(value1 ?? string.Empty, normal, Brushes.Black, left + 90, y);
            g.DrawString(label2, heading, Brushes.Black, left + half, y);
            g.DrawString(value2 ?? string.Empty, normal, Brushes.Black, left + half + 90, y);
            y += 22;
        }
    }
}
