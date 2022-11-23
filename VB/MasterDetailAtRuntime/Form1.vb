Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports DevExpress.XtraReports.UI
Imports DevExpress.XtraReports.Parameters

Namespace MasterDetailAtRuntime
	Partial Public Class Form1
		Inherits Form
		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub simpleButton1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles simpleButton1.Click
			Dim report As New XtraReport()
			Dim dataSet As NwindDataSet = FillDataSet()
			report.DataSource = dataSet
			report.DataMember = dataSet.Customers.TableName

			CreateReportHeader(report, "Runtime Generated Customer-Orders report", Color.PowderBlue, 32)
			CreateDetailBand(report, dataSet.Customers, "Customers", Color.Silver, False)
			CreateGrouping(report)

			Select Case radioGroup1.SelectedIndex
				Case 0
					CreateDetailReportBasedOnSubreport(report)
				Case 1
					CreateDetailReportBasedOnDetailReportBand(report)
			End Select

			report.ShowPreviewDialog()
		End Sub

		Private Function FillDataSet() As NwindDataSet
			'Filling DataTables
			Dim dataSet As New NwindDataSet()
			Dim custAdapter As New NwindDataSetTableAdapters.CustomersTableAdapter()
			Dim orderAdapter As New NwindDataSetTableAdapters.OrdersTableAdapter()

			custAdapter.Fill(dataSet.Customers)
			orderAdapter.Fill(dataSet.Orders)

			Return dataSet
		End Function

		Private Sub CreateReportHeader(ByVal report As XtraReportBase, ByVal caption As String, ByVal labelColor As Color, ByVal fontSize As Integer)
			'Creating a Report header
			Dim header As New ReportHeaderBand()
			report.Bands.Add(header)
			header.HeightF = 0

			Dim label As New XRLabel()
			header.Controls.Add(label)

			label.BackColor = labelColor
			label.Font = New Font("Tahoma", fontSize, FontStyle.Bold)
			label.Text = caption
			label.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
			label.LocationF = New PointF(0, 0)

			Dim rep As XtraReport = report.RootReport
			label.WidthF = rep.PageWidth - rep.Margins.Right - rep.Margins.Left
		End Sub

		Private Sub CreateDetailBand(ByVal report As XtraReportBase, ByVal templateTable As DataTable, ByVal dataMember As String, ByVal backColor As Color, ByVal useStyles As Boolean)
			Dim detail As New DetailBand()
			report.Bands.Add(detail)
			detail.HeightF = 0

			' Creating Header and Detail Tables
			Dim headerTable As New XRTable()
			report.Bands(BandKind.ReportHeader).Controls.Add(headerTable)

			Dim detailTable As New XRTable()
			detail.Controls.Add(detailTable)

			detailTable.BeginInit()
			headerTable.BeginInit()

			Dim headerRow As New XRTableRow()
			headerTable.Rows.Add(headerRow)

			Dim detailRow As New XRTableRow()
			detailTable.Rows.Add(detailRow)

			Dim colCount As Integer = templateTable.Columns.Count
			Dim rootReport As XtraReport = report.RootReport
			Dim pageWidth As Single = (rootReport.PageWidth - (rootReport.Margins.Left + rootReport.Margins.Right))
			Dim colWidth As Single = pageWidth \ colCount
			'Creating an XRTableCell for each column in the corresponding DataTable
			For i As Integer = 0 To colCount - 1
				Dim headerCell As New XRTableCell()
				headerRow.Cells.Add(headerCell)
				headerCell.WidthF = colWidth
				headerCell.Text = templateTable.Columns(i).Caption
				headerCell.Borders = DevExpress.XtraPrinting.BorderSide.All

				Dim detailCell As New XRTableCell()
				detailRow.Cells.Add(detailCell)
				detailCell.WidthF = colWidth
				Dim actualDM As String = String.Empty
				If dataMember = String.Empty Then
					actualDM = templateTable.Columns(i).Caption
				Else
					actualDM = String.Format("{0}.{1}", dataMember, templateTable.Columns(i).Caption)
				End If
				detailCell.DataBindings.Add("Text", Nothing, actualDM)
				detailCell.Borders = DevExpress.XtraPrinting.BorderSide.All
			Next i

			headerTable.EndInit()
			detailTable.EndInit()

			headerTable.LocationF = New PointF(0, report.Bands(BandKind.ReportHeader).HeightF)
			headerTable.WidthF = pageWidth
			headerTable.Font = New Font(headerTable.Font, FontStyle.Bold)

			detailTable.LocationF = New PointF(0, 0)
			detailTable.WidthF = pageWidth
			detailTable.BackColor = backColor
			'Applying styles if necessary
			If useStyles Then
				detailTable.EvenStyleName = "EvenStyle"
				detailTable.OddStyleName = "OddStyle"
			End If
		End Sub

		Private Sub CreateGrouping(ByVal report As XtraReport)
			'Add a group to the RootReport
			Dim groupband As New GroupHeaderBand()
			report.Bands.Add(groupband)
			groupband.HeightF = 0
			groupband.GroupUnion = GroupUnion.WithFirstDetail

			groupband.GroupFields.Add(New GroupField("Country"))

			Dim label As New XRLabel()
			groupband.Controls.Add(label)

			label.DataBindings.Add("Text", Nothing, "Country", "Country: {0}")
			label.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter
			label.LocationF = New PointF(0, 0)
			label.WidthF = report.PageWidth - report.Margins.Right - report.Margins.Left
			label.BackColor = Color.Wheat
		End Sub

		Private Sub CreateDetailReportBasedOnSubreport(ByVal report As XtraReport)
			'Create a Subreport
			Dim subreport As New XRSubreport()
			Dim detailBand As DetailBand = TryCast(report.Bands(BandKind.Detail), DetailBand)
			detailBand.Controls.Add(subreport)

			subreport.LocationF = New PointF(0, detailBand.HeightF)
			subreport.WidthF = report.PageWidth - report.Margins.Right - report.Margins.Left
			'Create a detail Report
			Dim detailReport As New XtraReport() With {.DataSource = report.DataSource, .DataMember = "Orders"}
			InitStyles(detailReport)

			subreport.ReportSource = detailReport
			'Create bands
			CreateReportHeader(detailReport, "Orders", Color.Gold, 16)

			Dim ds As NwindDataSet = TryCast(report.DataSource, NwindDataSet)
			CreateDetailBand(detailReport, ds.Orders, "Orders", Color.Transparent, True)

			'Add a parameter for filtering
			Dim param As New Parameter() With {.Name = "custID", .Type = GetType(String), .Visible = False, .Value = String.Empty}

			detailReport.Parameters.Add(param)
			detailReport.FilterString = "[CustomerID]==?custID"

			'Handle the Subreport.BeforePrint event for filtering details dynamically
			AddHandler subreport.BeforePrint, AddressOf subreport_BeforePrint
		End Sub

		Private Sub subreport_BeforePrint(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
			Dim subreport As XRSubreport = TryCast(sender, XRSubreport)
			Dim mainReport As XtraReport = TryCast(subreport.Report, XtraReport)

			'Obtain the current CustomerID value for filtering the detail report
			Dim currentCustID As String = mainReport.GetCurrentColumnValue("CustomerID").ToString()
			subreport.ReportSource.Parameters("custID").Value = currentCustID
		End Sub

		Private Sub CreateDetailReportBasedOnDetailReportBand(ByVal report As XtraReport)
			'Create a Detail Report
			Dim detailReport As New DetailReportBand()
			report.Bands.Add(detailReport)
			'Init Styles at the level of the RootReport
			InitStyles(report)

			'Generate the DataMember based on data relations
			Dim ds As NwindDataSet = TryCast(report.DataSource, NwindDataSet)
			Dim detailDataMember As String = String.Format("{0}.{1}", ds.Tables(report.DataMember).TableName, ds.Relations(0).RelationName)

			detailReport.DataSource = ds
			detailReport.DataMember = detailDataMember

			'Create bands
			CreateReportHeader(detailReport, "Orders", Color.Gold, 16)
			CreateDetailBand(detailReport, ds.Orders, detailDataMember, Color.Transparent, True)
		End Sub

		Public Shared Sub InitStyles(ByVal rep As XtraReportBase)
			' Create different odd and even styles
			Dim oddStyle As New XRControlStyle()
			Dim evenStyle As New XRControlStyle()

			' Specify the odd style appearance
			oddStyle.BackColor = Color.LightBlue
			oddStyle.StyleUsing.UseBackColor = True
			oddStyle.StyleUsing.UseBorders = False
			oddStyle.Name = "OddStyle"

			' Specify the even style appearance
			evenStyle.BackColor = Color.LightPink
			evenStyle.StyleUsing.UseBackColor = True
			evenStyle.StyleUsing.UseBorders = False
			evenStyle.Name = "EvenStyle"

			' Add styles to report's style sheet
			rep.RootReport.StyleSheet.AddRange(New DevExpress.XtraReports.UI.XRControlStyle() { oddStyle, evenStyle })
		End Sub
	End Class
End Namespace
