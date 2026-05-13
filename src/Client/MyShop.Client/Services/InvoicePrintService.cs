using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using MyShop.Client.Models;
using MyShop.Client.Services.Interfaces;
using LuciferCore.Attributes;

namespace MyShop.Client.Services
{
    [Plugin("Service", "InvoicePrint")]
    public class InvoicePrintService : IInvoicePrintService
    {
        public void ExportToXps(Order order, string filePath)
        {
            var document = BuildInvoiceDocument(order);

            using var xpsDocument = new XpsDocument(
                filePath,
                FileAccess.ReadWrite);

            var writer =
                XpsDocument.CreateXpsDocumentWriter(xpsDocument);

            writer.Write(
                ((IDocumentPaginatorSource)document)
                .DocumentPaginator);
        }

        private FlowDocument BuildInvoiceDocument(Order order)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                ColumnWidth = double.PositiveInfinity
            };

            // HEADER

            doc.Blocks.Add(new Paragraph(new Bold(new Run("MYSHOP")))
            {
                FontSize = 28,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            });

            doc.Blocks.Add(new Paragraph(new Run("HÓA ĐƠN THANH TOÁN"))
            {
                FontSize = 18,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // ORDER INFO

            var infoTable = new Table
            {
                CellSpacing = 0
            };

            infoTable.Columns.Add(new TableColumn
            {
                Width = new GridLength(180)
            });

            infoTable.Columns.Add(new TableColumn());

            var infoGroup = new TableRowGroup();

            void AddInfoRow(string label, string value)
            {
                var row = new TableRow();

                row.Cells.Add(new TableCell(
                    new Paragraph(new Bold(new Run(label))))
                {
                    Padding = new Thickness(4)
                });

                row.Cells.Add(new TableCell(
                    new Paragraph(new Run(value)))
                {
                    Padding = new Thickness(4)
                });

                infoGroup.Rows.Add(row);
            }

            AddInfoRow("Mã đơn hàng", $"#{order.OrderId}");

            AddInfoRow(
                "Ngày tạo",
                order.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-");

            AddInfoRow(
                "Trạng thái",
                order.StatusText);

            AddInfoRow(
                "Phương thức thanh toán",
                order.PaymentMethod switch
                {
                    1 => "Tiền mặt",
                    2 => "Chuyển khoản",
                    3 => "Ví điện tử",
                    _ => "Chưa thanh toán"
                });

            AddInfoRow(
                "Voucher",
                string.IsNullOrWhiteSpace(order.VoucherCode)
                    ? "Không có"
                    : order.VoucherCode);

            AddInfoRow(
                "Ghi chú",
                string.IsNullOrWhiteSpace(order.Note)
                    ? "-"
                    : order.Note);

            infoTable.RowGroups.Add(infoGroup);

            doc.Blocks.Add(infoTable);

            // SPACING

            doc.Blocks.Add(new Paragraph(new Run(" ")));

            // PRODUCT TABLE

            var table = new Table
            {
                CellSpacing = 0
            };

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(260)
            });

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(80)
            });

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(120)
            });

            table.Columns.Add(new TableColumn
            {
                Width = new GridLength(140)
            });

            var group = new TableRowGroup();

            var header = new TableRow
            {
                Background = Brushes.LightGray
            };

            header.Cells.Add(CreateHeaderCell("Sản phẩm"));
            header.Cells.Add(CreateHeaderCell("SL"));
            header.Cells.Add(CreateHeaderCell("Đơn giá"));
            header.Cells.Add(CreateHeaderCell("Thành tiền"));

            group.Rows.Add(header);

            foreach (var item in order.OrderItems)
            {
                var total =
                    (item.Quantity ?? 0)
                    * (item.UnitPrice ?? 0);

                var row = new TableRow();

                row.Cells.Add(CreateCell(item.ProductName));

                row.Cells.Add(CreateCell(
                    item.Quantity?.ToString() ?? "0"));

                row.Cells.Add(CreateCell(
                    $"{item.UnitPrice:N0} VNĐ"));

                row.Cells.Add(CreateCell(
                    $"{total:N0} VNĐ"));

                group.Rows.Add(row);
            }

            table.RowGroups.Add(group);

            doc.Blocks.Add(table);

            // TOTALS

            doc.Blocks.Add(new Paragraph(new Run(" "))
            {
                Margin = new Thickness(0, 10, 0, 0)
            });

            var totalTable = new Table();

            totalTable.Columns.Add(new TableColumn
            {
                Width = new GridLength(300)
            });

            totalTable.Columns.Add(new TableColumn
            {
                Width = new GridLength(180)
            });

            var totalGroup = new TableRowGroup();

            void AddTotalRow(
                string label,
                string value,
                bool bold = false)
            {
                var row = new TableRow();

                row.Cells.Add(new TableCell(
                    new Paragraph(new Run(label)))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(4)
                });

                row.Cells.Add(new TableCell(
                    new Paragraph(
                        bold
                            ? new Bold(new Run(value))
                            : new Run(value)))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(4)
                });

                totalGroup.Rows.Add(row);
            }

            AddTotalRow(
                "Tạm tính:",
                $"{order.SubTotal:N0} VNĐ");

            AddTotalRow(
                "Giảm giá:",
                $"{order.DiscountAmount:N0} VNĐ");

            AddTotalRow(
                "Tổng thanh toán:",
                $"{order.FinalTotal:N0} VNĐ",
                true);

            totalTable.RowGroups.Add(totalGroup);

            doc.Blocks.Add(totalTable);

            // FOOTER

            doc.Blocks.Add(new Paragraph(new Run(" "))
            {
                Margin = new Thickness(0, 20, 0, 0)
            });

            doc.Blocks.Add(new Paragraph(
                new Italic(
                    new Run(
                        "Cảm ơn quý khách đã mua hàng tại MyShop!")))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 14
            });

            return doc;
        }

        private TableCell CreateHeaderCell(string text)
        {
            return new TableCell(
                new Paragraph(new Bold(new Run(text))))
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6),
                TextAlignment = TextAlignment.Center
            };
        }

        private TableCell CreateCell(string text)
        {
            return new TableCell(
                new Paragraph(new Run(text)))
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(6)
            };
        }
    }
}