using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Parameters;

namespace MasterDetailAtRuntime
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            XtraReport report = new XtraReport();
            NwindDataSet dataSet = FillDataSet();
            report.DataSource = dataSet;
            report.DataMember = dataSet.Customers.TableName;

            CreateReportHeader(report, "Runtime Generated Customer-Orders report", Color.PowderBlue, 32);
            CreateDetailBand(report, dataSet.Customers, "Customers", Color.Silver, false);
            CreateGrouping(report);            

            switch (radioGroup1.SelectedIndex)
            {
                case 0:
                    CreateDetailReportBasedOnSubreport(report);
                    break;
                case 1:
                    CreateDetailReportBasedOnDetailReportBand(report);
                    break;
            }

            report.ShowPreviewDialog();
        }

        private NwindDataSet FillDataSet()
        {
            //Filling DataTables
            NwindDataSet dataSet = new NwindDataSet();
            NwindDataSetTableAdapters.CustomersTableAdapter custAdapter = new NwindDataSetTableAdapters.CustomersTableAdapter();
            NwindDataSetTableAdapters.OrdersTableAdapter orderAdapter = new NwindDataSetTableAdapters.OrdersTableAdapter();

            custAdapter.Fill(dataSet.Customers);
            orderAdapter.Fill(dataSet.Orders);

            return dataSet;
        }

        private void CreateReportHeader(XtraReportBase report, string caption, Color labelColor, int fontSize)
        {
            //Creating a Report header
            ReportHeaderBand header = new ReportHeaderBand();
            report.Bands.Add(header);
            header.HeightF = 0;

            XRLabel label = new XRLabel();
            header.Controls.Add(label);

            label.BackColor = labelColor;
            label.Font = new Font("Tahoma", fontSize, FontStyle.Bold);
            label.Text = caption;
            label.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            label.LocationF = new PointF(0, 0);

            XtraReport rep = report.RootReport;
            label.WidthF = rep.PageWidth - rep.Margins.Right - rep.Margins.Left;
        }

        private void CreateDetailBand(XtraReportBase report, DataTable templateTable, String dataMember, Color backColor, bool useStyles)
        {
            DetailBand detail = new DetailBand();
            report.Bands.Add(detail);
            detail.HeightF = 0;

            // Creating Header and Detail Tables
            XRTable headerTable = new XRTable();
            report.Bands[BandKind.ReportHeader].Controls.Add(headerTable);

            XRTable detailTable = new XRTable();
            detail.Controls.Add(detailTable);

            detailTable.BeginInit();
            headerTable.BeginInit();

            XRTableRow headerRow = new XRTableRow();
            headerTable.Rows.Add(headerRow);

            XRTableRow detailRow = new XRTableRow();
            detailTable.Rows.Add(detailRow);
            
            int colCount = templateTable.Columns.Count;
            XtraReport rootReport = report.RootReport;
            int pageWidth = (rootReport.PageWidth - (rootReport.Margins.Left + rootReport.Margins.Right));
            float colWidth = pageWidth / colCount;
            //Creating an XRTableCell for each column in the corresponding DataTable
            for (int i = 0; i < colCount; i++)
            {
                XRTableCell headerCell = new XRTableCell();
                headerRow.Cells.Add(headerCell);
                headerCell.WidthF = colWidth;
                headerCell.Text = templateTable.Columns[i].Caption;
                headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All;

                XRTableCell detailCell = new XRTableCell();
                detailRow.Cells.Add(detailCell);
                detailCell.WidthF = colWidth;
                string actualDM = string.Empty;
                if (dataMember == string.Empty)
                    actualDM = templateTable.Columns[i].Caption;
                else
                    actualDM = string.Format("{0}.{1}", dataMember, templateTable.Columns[i].Caption);
                detailCell.DataBindings.Add("Text", null, actualDM);
                detailCell.Borders = DevExpress.XtraPrinting.BorderSide.All;                                
            }                        

            headerTable.EndInit();
            detailTable.EndInit();

            headerTable.LocationF = new PointF(0, report.Bands[BandKind.ReportHeader].HeightF);
            headerTable.WidthF = pageWidth;
            headerTable.Font = new Font(headerTable.Font, FontStyle.Bold);

            detailTable.LocationF = new PointF(0, 0);
            detailTable.WidthF = pageWidth;
            detailTable.BackColor = backColor;
            //Applying styles if necessary
            if (useStyles)
            {
                detailTable.EvenStyleName = "EvenStyle";
                detailTable.OddStyleName = "OddStyle";
            }
        }

        private void CreateGrouping(XtraReport report)
        {
            //Add a group to the RootReport
            GroupHeaderBand groupband = new GroupHeaderBand();
            report.Bands.Add(groupband);
            groupband.HeightF = 0;
            groupband.GroupUnion = GroupUnion.WithFirstDetail;

            groupband.GroupFields.Add(new GroupField("Country"));

            XRLabel label = new XRLabel();
            groupband.Controls.Add(label);

            label.DataBindings.Add("Text", null, "Country", "Country: {0}");
            label.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            label.LocationF = new PointF(0, 0);
            label.WidthF = report.PageWidth - report.Margins.Right - report.Margins.Left;
            label.BackColor = Color.Wheat;
        }

        private void CreateDetailReportBasedOnSubreport(XtraReport report)
        {
            //Create a Subreport
            XRSubreport subreport = new XRSubreport();
            DetailBand detailBand = report.Bands[BandKind.Detail] as DetailBand;
            detailBand.Controls.Add(subreport);

            subreport.LocationF = new PointF(0, detailBand.HeightF);
            subreport.WidthF = report.PageWidth - report.Margins.Right - report.Margins.Left;
            //Create a detail Report
            XtraReport detailReport = new XtraReport() { DataSource = report.DataSource, DataMember = "Orders" };
            InitStyles(detailReport);

            subreport.ReportSource = detailReport;
            //Create bands
            CreateReportHeader(detailReport, "Orders", Color.Gold, 16);

            NwindDataSet ds = report.DataSource as NwindDataSet;
            CreateDetailBand(detailReport, ds.Orders, "Orders", Color.Transparent, true);

            //Add a parameter for filtering
            Parameter param = new Parameter() { Name = "custID", Type = typeof(string), Visible = false, Value = string.Empty };

            detailReport.Parameters.Add(param);
            detailReport.FilterString = "[CustomerID]==?custID";
            
            //Handle the Subreport.BeforePrint event for filtering details dynamically
            subreport.BeforePrint += subreport_BeforePrint;
        }

        void subreport_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            XRSubreport subreport = sender as XRSubreport;
            XtraReport mainReport = subreport.Report as XtraReport;

            //Obtain the current CustomerID value for filtering the detail report
            String currentCustID = mainReport.GetCurrentColumnValue("CustomerID").ToString();
            subreport.ReportSource.Parameters["custID"].Value = currentCustID;
        }

        private void CreateDetailReportBasedOnDetailReportBand(XtraReport report)
        {
            //Create a Detail Report
            DetailReportBand detailReport = new DetailReportBand();
            report.Bands.Add(detailReport);
            //Init Styles at the level of the RootReport
            InitStyles(report);

            //Generate the DataMember based on data relations
            NwindDataSet ds = report.DataSource as NwindDataSet;
            string detailDataMember = string.Format("{0}.{1}", ds.Tables[report.DataMember].TableName,
                ds.Relations[0].RelationName);

            detailReport.DataSource = ds;
            detailReport.DataMember = detailDataMember;

            //Create bands
            CreateReportHeader(detailReport, "Orders", Color.Gold, 16);            
            CreateDetailBand(detailReport, ds.Orders, detailDataMember, Color.Transparent, true);            
        }

        public static void InitStyles(XtraReportBase rep)
        {
            // Create different odd and even styles
            XRControlStyle oddStyle = new XRControlStyle();
            XRControlStyle evenStyle = new XRControlStyle();

            // Specify the odd style appearance
            oddStyle.BackColor = Color.LightBlue;
            oddStyle.StyleUsing.UseBackColor = true;
            oddStyle.StyleUsing.UseBorders = false;
            oddStyle.Name = "OddStyle";

            // Specify the even style appearance
            evenStyle.BackColor = Color.LightPink;
            evenStyle.StyleUsing.UseBackColor = true;
            evenStyle.StyleUsing.UseBorders = false;
            evenStyle.Name = "EvenStyle";

            // Add styles to report's style sheet
            rep.RootReport.StyleSheet.AddRange(new DevExpress.XtraReports.UI.XRControlStyle[] { oddStyle, evenStyle });
        }
    }
}
