Imports System.Windows.Forms
Imports System.IO

Public Class Dialog1

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        If RadioButton1.Checked = True Then
            mainForm.UTDirectory = TextBox1.Text
            Dim tmputdir As String = TextBox1.Text
        End If

        If RadioButton2.Checked = True Then
            mainForm.UTDirectory = TextBox2.Text
            Dim tmputdir As String = TextBox2.Text
        End If

        If RadioButton3.Checked = True Then
            mainForm.UTDirectory = TextBox3.Text
            Dim tmputdir As String = TextBox2.Text
        End If

        If Not Directory.Exists(TextBox3.Text) Then

            Directory.CreateDirectory(TextBox3.Text)

        End If





        
        mainForm.LblTest.Refresh()
        'MessageBox.Show(mainForm.LblTest.Text)

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub Dialog1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        mainForm.LblTest.Text = mainForm.UTDirectory
        mainForm.LblTest.Refresh()
    End Sub

    Private Sub Dialog1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        mainForm.LblTest.Text = mainForm.UTDirectory
        mainForm.LblTest.Refresh()
    End Sub

    Private Sub Dialog1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        mainForm.LblTest.Text = mainForm.UTDirectory
        mainForm.LblTest.Refresh()
    End Sub
End Class
