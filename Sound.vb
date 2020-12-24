Imports System
Imports System.Runtime.InteropServices
Imports System.Resources
Imports System.IO

Public Class Sound

    Private Shared SND_ASYNC As Integer = 1
    Private Shared SND_MEMORY As Integer = 4

    Declare Function PlaySound Lib "winmm.dll" Alias "PlaySoundA" (ByVal lpszName As Byte(), ByVal hModule As Integer, ByVal dwFlags As Integer) As Integer

    Public Shared Sub PlayWavResource(ByVal wav As String)

        ' get the namespace	
        Dim strNameSpace As String = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString()

        ' get the resource into a stream
        Dim resourceStream As Stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(strNameSpace + "." + wav)
        If resourceStream Is Nothing Then Exit Sub

        ' bring stream into a byte array
        Dim wavData As Byte()
        ReDim wavData(CInt(resourceStream.Length))
        resourceStream.Read(wavData, 0, CInt(resourceStream.Length))

        ' play the resource
        PlaySound(wavData, 0, SND_ASYNC Or SND_MEMORY)
    End Sub
End Class