Imports System.Net
Imports Microsoft.Win32
Imports System.IO




Public Class mainForm

    Dim whereToSave As String 'Where the program save the file
    Dim ZUrl As String = "http://gs-redirect1.teamplay.de/ut-redirect/ut.php/85.14.228.234/13570/ut2004/"
    Dim UTFilename As String
    Dim UTFilenameLB As String
    Dim ZDate As String = Now
    Dim r As IO.StreamReader
    Public UTDirectory As String = "c:\UTDownloader"



    Delegate Sub ChangeTextsSafe(ByVal length As Long, ByVal position As Integer, ByVal percent As Integer, ByVal speed As Double)
    Delegate Sub DownloadCompleteSafe(ByVal cancelled As Boolean)


    



    Public Sub DownloadComplete(ByVal cancelled As Boolean)
        Me.txtFileName.Enabled = True
        Me.btnDownload.Enabled = True
        Me.btnDownload2.Enabled = True
        Me.btnCancel.Enabled = False
        Me.Cursor = Cursors.Default ' default


        If cancelled Then

            Me.Label4.Text = "Cancelled"

            MessageBox.Show("Download aborted", "Aborted", MessageBoxButtons.OK, MessageBoxIcon.Information)


        Else
            Me.Label4.Text = "Successfully downloaded"
            MessageBox.Show("Successfully downloaded!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.Label3.Font = New Font("Comic Sans MS", 8, FontStyle.Regular)
            Me.btnUpdate.Enabled = False


        End If

        Me.ProgressBar1.Value = 0
        Me.Label5.Text = "Downloading: "
        Me.Label6.Text = "Save to: "
        Me.Label3.Text = "File size: "
        Me.Label2.Text = "Download speed: "
        Me.Label4.Text = ""
        Me.txtFileName.Text = ""

    End Sub

    Public Sub ChangeTexts(ByVal length As Long, ByVal position As Integer, ByVal percent As Integer, ByVal speed As Double)

        Me.Label3.Font = New Font("Comic Sans MS", 11, FontStyle.Bold)
        Me.Label3.Text = "File Size: " & Math.Round((length / 1024), 0) & " KB" & "  Progress " & Me.ProgressBar1.Value & "%"
        Me.Label5.Text = "Downloading: " & Me.UTFilename
        Me.Label4.Text = "Downloading " & Math.Round((position / 1024), 0) & " KB of " & Math.Round((length / 1024), 0) & " KB"

        If speed = -1 Then
            Me.Label2.Text = "Speed: calculating..."
        Else
            Me.Label2.Text = "Speed: " & Math.Round((speed / 102.4), 0) & " Kbit/s"
        End If

        Me.ProgressBar1.Value = percent
        Me.btnDownload.Enabled = False

    End Sub

    Private Sub btnDownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDownload.Click

        Me.UTFilename = txtFileName.Text
        'MsgBox(Me.UTFilename)


        Me.txtFileName.Text = Me.ZUrl & Me.txtFileName.Text

        If Me.txtFileName.Text <> "" AndAlso Me.txtFileName.Text.StartsWith("http://") Then


            Me.SaveFileDialog1.FileName = Me.txtFileName.Text.Split("/"c)(Me.txtFileName.Text.Split("/"c).Length - 1)

            If Me.SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then

                Me.whereToSave = Me.SaveFileDialog1.FileName

                Me.SaveFileDialog1.FileName = ""

                Me.Label6.Text = "Save to: " & Me.whereToSave

                Me.txtFileName.Enabled = False
                Me.btnDownload.Enabled = False
                Me.btnDownload2.Enabled = False
                Me.btnCancel.Enabled = True
                Me.Cursor = Cursors.WaitCursor ' hourglass

                Me.BackgroundWorker1.RunWorkerAsync() 'Start download

            End If

        Else

            MessageBox.Show("Please insert valid URL for download", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        End If

    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork


        'Creating the request and getting the response
        Dim theResponse As HttpWebResponse
        Dim theRequest As HttpWebRequest
        Try 'Checks if the file exist

            theRequest = WebRequest.Create(Me.txtFileName.Text)
            theResponse = theRequest.GetResponse

        Catch ex As Exception

            MessageBox.Show("An error occurred while downloading file. Possibe causes:" & ControlChars.CrLf & _
                            "1) File doesn't exist" & ControlChars.CrLf & _
                            "2) Remote server error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

            Dim cancelDelegate As New DownloadCompleteSafe(AddressOf DownloadComplete)

            Me.Invoke(cancelDelegate, True)

            Exit Sub

        End Try
        Dim length As Long = theResponse.ContentLength 'Size of the response (in bytes)

        Dim safedelegate As New ChangeTextsSafe(AddressOf ChangeTexts)
        Me.Invoke(safedelegate, length, 0, 0, 0) 'Invoke the TreadsafeDelegate


        Me.whereToSave = Me.whereToSave.Substring(0, Me.whereToSave.Length - 2)
        Dim writeStream As New IO.FileStream(Me.whereToSave, IO.FileMode.Create)

        'Replacement for Stream.Position (webResponse stream doesn't support seek)
        Dim nRead As Integer

        'To calculate the download speed
        Dim speedtimer As New Stopwatch
        Dim currentspeed As Double = -1
        Dim readings As Integer = 0

        Do

            If BackgroundWorker1.CancellationPending Then 'If user abort download
                Exit Do
            End If

            speedtimer.Start()

            Dim readBytes(4095) As Byte
            Dim bytesread As Integer = theResponse.GetResponseStream.Read(readBytes, 0, 4096)

            nRead += bytesread

            Dim percent As Short = (nRead * 99.9) / length

            Me.Invoke(safedelegate, length, nRead, percent, currentspeed)

            If bytesread = 0 Then Exit Do

            writeStream.Write(readBytes, 0, bytesread)

            speedtimer.Stop()

            readings += 1
            If readings >= 100 Then 'For increase precision, the speed it's calculated only every five cicles
                currentspeed = 20480 / (speedtimer.ElapsedMilliseconds / 1000)
                speedtimer.Reset()
                readings = 0
            End If
        Loop

        'Close the streams
        theResponse.GetResponseStream.Close()
        writeStream.Close()

        If Me.BackgroundWorker1.CancellationPending Then

            IO.File.Delete(Me.whereToSave)

            Dim cancelDelegate As New DownloadCompleteSafe(AddressOf DownloadComplete)

            Me.Invoke(cancelDelegate, True)

            Exit Sub

        End If

        Dim completeDelegate As New DownloadCompleteSafe(AddressOf DownloadComplete)

        Me.Invoke(completeDelegate, False)

    End Sub

    Private Sub mainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        My.Computer.Audio.Play("http://www.lanstar.de/ut/was2.wav")
        Me.TextBox1.Text = ""
        Me.Label11.Text = CurDir()
        Me.LblTest.Text = Me.UTDirectory
        Me.TextBox1.ForeColor = System.Drawing.Color.BlueViolet

        'creating the UT Dir
        If Not Directory.Exists(Me.UTDirectory) Then

            Directory.CreateDirectory(Me.UTDirectory)

        End If


        'My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Lanstar\UT\Date", "Lastvisit", "never")

        Dim exists As Boolean = False
        Try
            If My.Computer.Registry.CurrentUser.OpenSubKey("HKEY_CURRENT_USER\Software\Lanstar\UT\Date") IsNot Nothing Then
                exists = True
            End If
        Finally
            Dim keyValue As String
            keyValue = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Lanstar\UT\Date", "Lastvisit", "Default Value")
            Me.Label10.Text = keyValue
            'My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Lanstar\UT\Date", "Lastvisit", Now)
            My.Computer.Registry.CurrentUser.Close()
        End Try

        'message of the day
        Me.TextBox1.Text = ""

        If My.Computer.FileSystem.FileExists(CurDir() & "\" & "motd.ini") Then
            My.Computer.FileSystem.DeleteFile(CurDir() & "\" & "motd.ini")
        Else
        End If

        'My.Computer.Network.DownloadFile("http://www.lanstar.de/ut/motd.ini", CurDir() & "\" & "motd.ini")

        'r = New IO.StreamReader("motd.ini")
        'TextBox1.Text = (r.ReadToEnd)
        'r.Close()


        
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Me.BackgroundWorker1.CancelAsync() 'Send cancel request
    End Sub

    Private Sub LinkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        MessageBox.Show("Created by DeathBronner (www.lanstar.de)" & ControlChars.CrLf & "hey you Anal Knights ! To report bugs/suggestions/comments contact me by email: rainer@lanstar.de", "About", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged

    End Sub


    Private Sub BackgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted

    End Sub

    Private Sub btnDownload2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDownload2.Click

        'Me.UTFilename = ListBox1.SelectedItem
        'MsgBox(Me.UTFilename)


        'Me.UTFilenameLB = Me.ZUrl & Me.ListBox1.SelectedItem
        'MsgBox(UTFilenameLB)

        'If Me.UTFilename <> "" AndAlso Me.UTFilenameLB.StartsWith("http://") Then


        'Me.SaveFileDialog1.FileName = Me.ListBox1.SelectedItem.Split("/"c)(Me.ListBox1.SelectedItem.Split("/"c).Length - 1)

        'If Me.SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then

        Me.whereToSave = Me.UTDirectory & "\" & Me.UTFilename

        'check if the files exists
        If File.Exists(Me.whereToSave) Then

            TextBox1.Text = Me.TextBox1.Text + "selected File: " & Me.ListBox1.SelectedItem
            TextBox1.Text = Me.TextBox1.Text + vbCrLf
            TextBox1.Text = Me.TextBox1.Text + "selected Item #: " & Me.ListBox1.SelectedIndex
            TextBox1.Text = Me.TextBox1.Text + vbCrLf
            TextBox1.Text = Me.TextBox1.Text + Me.whereToSave & (" : file exists")
            TextBox1.Text = Me.TextBox1.Text + vbCrLf


            TextBox1.SelectAll()
            TextBox1.ScrollToCaret()
            TextBox1.DeselectAll()

            Exit Sub
        End If



        'Me.SaveFileDialog1.FileName = ""

        Me.Label6.Text = "Save to: " & Me.whereToSave

        Me.txtFileName.Enabled = False
        Me.btnDownload.Enabled = False
        Me.btnDownload2.Enabled = False
        Me.btnCancel.Enabled = True
        Me.Cursor = Cursors.WaitCursor
        Me.txtFileName.Text = UTFilenameLB
        'MsgBox(txtFileName.Text)


        Me.BackgroundWorker1.RunWorkerAsync() 'Start download

        'End If

        'Else

        'MessageBox.Show("Please insert valid URL for download", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        'End If

    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        Me.btnDownload.Enabled = False
        Me.btnDownload3.Enabled = False
        Me.btnDownload2.Enabled = True
    End Sub

    Private Sub txtFileName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFileName.TextChanged
        Me.btnDownload.Enabled = True
        Me.btnDownload3.Enabled = False
        Me.btnDownload2.Enabled = False
    End Sub

    Private Sub btnUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUpdate.Click

        If My.Computer.FileSystem.FileExists(CurDir() & "\" & "config.ini") Then
            'MessageBox.Show("config.ini found")
            My.Computer.FileSystem.DeleteFile(CurDir() & "\" & "config.ini")
            'MessageBox.Show("config.ini deleted")
        Else
            'MessageBox.Show("config.ini not found")
        End If

        If My.Computer.FileSystem.FileExists(CurDir() & "\" & "config2.ini") Then
            My.Computer.FileSystem.DeleteFile(CurDir() & "\" & "config2.ini")
        Else
            'MessageBox.Show("config.ini not found")
        End If

        My.Computer.Network.DownloadFile("http://www.lanstar.de/ut/config.ini", CurDir() & "\" & "config.ini")
        My.Computer.Network.DownloadFile("http://www.lanstar.de/ut/config2.ini", CurDir() & "\" & "config2.ini")
        MessageBox.Show("Config Files downloaded")

        r = New IO.StreamReader("config.ini")
        While (r.Peek() > -1)
            ListBox1.Items.Add(r.ReadLine)
        End While
        r.Close()

        r = New IO.StreamReader("config2.ini")
        While (r.Peek() > -1)
            ListBox2.Items.Add(r.ReadLine)
        End While
        r.Close()

        My.Computer.Registry.SetValue("HKEY_CURRENT_USER\Software\Lanstar\UT\Date", "Lastvisit", Now)

        Dim keyValue As String
        keyValue = My.Computer.Registry.GetValue("HKEY_CURRENT_USER\Software\Lanstar\UT\Date", "Lastvisit", "Default Value")
        Me.Label10.Text = keyValue
        Me.btnUpdate.Enabled = False

    End Sub

  
    Private Sub MenuStrip1_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs)

    End Sub

    Private Sub LinkLabel2_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)

        Process.Start("http://www.lanstar.de/ut/UTDownloader.exe")

    End Sub

    Private Sub ListBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox2.SelectedIndexChanged
        Me.btnDownload.Enabled = False
        Me.btnDownload3.Enabled = True
        Me.btnDownload2.Enabled = False
    End Sub

    Private Sub btnDownload3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDownload3.Click
        Me.UTFilename = ListBox2.SelectedItem
        'MsgBox(Me.UTFilename)


        Me.UTFilenameLB = Me.ZUrl & Me.ListBox2.SelectedItem
        'MsgBox(UTFilenameLB)

        If Me.UTFilename <> "" AndAlso Me.UTFilenameLB.StartsWith("http://") Then


            Me.SaveFileDialog1.FileName = Me.ListBox2.SelectedItem.Split("/"c)(Me.ListBox2.SelectedItem.Split("/"c).Length - 1)

            If Me.SaveFileDialog1.ShowDialog = Windows.Forms.DialogResult.OK Then

                Me.whereToSave = Me.SaveFileDialog1.FileName

                Me.SaveFileDialog1.FileName = ""

                Me.Label6.Text = "Save to: " & Me.whereToSave

                Me.txtFileName.Enabled = False
                Me.btnDownload.Enabled = False
                Me.btnDownload2.Enabled = False
                Me.btnCancel.Enabled = True
                Me.Cursor = Cursors.WaitCursor
                Me.txtFileName.Text = UTFilenameLB
                'MsgBox(txtFileName.Text)


                Me.BackgroundWorker1.RunWorkerAsync() 'Start download

            End If

        Else

            MessageBox.Show("Please insert valid URL for download", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        End If
    End Sub

    Private Sub LinkLabel3_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Process.Start("http://www.lanstar.de/serverstatus.php")

    End Sub

    Private Sub LinkLabel4_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Process.Start("http://ut2004stats.epicgames.com/serverstats.php?server=24523828")

    End Sub

    Private Sub LinkLabel5_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs)
        Process.Start("http://www.lanstar.de")
    End Sub

    Private Sub Label11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label11.Click

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)









    End Sub

    Private Sub Label5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label5.Click

    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        End
    End Sub

    Private Sub HelpToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub PathsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PathsToolStripMenuItem.Click
        Dialog1.ShowDialog()
    End Sub

    Private Sub LblTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LblTest.Click

    End Sub

    Private Sub LblTest_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles LblTest.TextChanged
        LblTest.Refresh()
    End Sub

    Private Sub mainForm_LocationChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.LocationChanged

    End Sub

    Private Sub mainForm_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
        LblTest.Refresh()
    End Sub

    Private Sub btnTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnTest.Click
        Me.LblTest.Text = UTDirectory
        Me.LblTest.Refresh()
    End Sub

    Private Sub btndltest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btndltest.Click
        


        Dim s As New System.Text.StringBuilder()

        For Each item As Object In Me.ListBox1.SelectedItems
            s.Append(item)
            s.Append(vbCrLf)
            Me.UTFilename = s.ToString

            Me.UTFilenameLB = Me.ZUrl & "\" & Me.UTFilename


            btnDownload2.PerformClick()
            Threading.Thread.Sleep(2000)
        Next

        Me.TextBox1.Text = s.ToString()


          




    End Sub

    Private Sub Label2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label2.Click

    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub UpdateVersionToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UpdateVersionToolStripMenuItem.Click
        Process.Start("http://www.lanstar.de/ut/UTDownloader.exe")
    End Sub

    Private Sub AboutThisShitProgToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutThisShitProgToolStripMenuItem.Click
        MessageBox.Show("Created by DeathBronner (www.lanstar.de)" & ControlChars.CrLf & "hey you Anal Knights ! To report bugs/suggestions/comments contact me by email: rainer@lanstar.de", "About", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ServerStatsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ServerStatsToolStripMenuItem.Click
        Process.Start("http://www.lanstar.de/serverstatus.php")
    End Sub

    Private Sub PlayerStatsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlayerStatsToolStripMenuItem.Click
        Process.Start("http://ut2004stats.epicgames.com/serverstats.php?server=24523828")
    End Sub

    Private Sub GoToWwwlanstardeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GoToWwwlanstardeToolStripMenuItem.Click
        Process.Start("http://www.lanstar.de")
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Process.Start("https://wi.teamplay.de")
    End Sub

    Private Sub HelpToolStripMenuItem_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpToolStripMenuItem.Click

    End Sub

    Private Sub PathsUT2004ToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PathsUT2004ToolStripMenuItem.Click

    End Sub
End Class


