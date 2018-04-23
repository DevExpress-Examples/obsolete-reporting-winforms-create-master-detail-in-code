Imports Microsoft.VisualBasic
Imports System
Namespace MasterDetailAtRuntime
	Partial Public Class Form1
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.IContainer = Nothing

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing AndAlso (components IsNot Nothing) Then
				components.Dispose()
			End If
			MyBase.Dispose(disposing)
		End Sub

		#Region "Windows Form Designer generated code"

		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Me.radioGroup1 = New DevExpress.XtraEditors.RadioGroup()
			Me.simpleButton1 = New DevExpress.XtraEditors.SimpleButton()
			CType(Me.radioGroup1.Properties, System.ComponentModel.ISupportInitialize).BeginInit()
			Me.SuspendLayout()
			' 
			' radioGroup1
			' 
			Me.radioGroup1.EditValue = True
			Me.radioGroup1.Location = New System.Drawing.Point(13, 13)
			Me.radioGroup1.Name = "radioGroup1"
			Me.radioGroup1.Properties.Items.AddRange(New DevExpress.XtraEditors.Controls.RadioGroupItem() { New DevExpress.XtraEditors.Controls.RadioGroupItem(True, "Subreports"), New DevExpress.XtraEditors.Controls.RadioGroupItem(False, "DetailReportBands")})
			Me.radioGroup1.Size = New System.Drawing.Size(210, 79)
			Me.radioGroup1.TabIndex = 0
			' 
			' simpleButton1
			' 
			Me.simpleButton1.Location = New System.Drawing.Point(13, 109)
			Me.simpleButton1.Name = "simpleButton1"
			Me.simpleButton1.Size = New System.Drawing.Size(210, 23)
			Me.simpleButton1.TabIndex = 1
			Me.simpleButton1.Text = "Create and Show the Report"
'			Me.simpleButton1.Click += New System.EventHandler(Me.simpleButton1_Click);
			' 
			' Form1
			' 
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6F, 13F)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(237, 144)
			Me.Controls.Add(Me.simpleButton1)
			Me.Controls.Add(Me.radioGroup1)
			Me.Name = "Form1"
			Me.Text = "Form1"
			CType(Me.radioGroup1.Properties, System.ComponentModel.ISupportInitialize).EndInit()
			Me.ResumeLayout(False)

		End Sub

		#End Region

		Private radioGroup1 As DevExpress.XtraEditors.RadioGroup
		Private WithEvents simpleButton1 As DevExpress.XtraEditors.SimpleButton
	End Class
End Namespace

