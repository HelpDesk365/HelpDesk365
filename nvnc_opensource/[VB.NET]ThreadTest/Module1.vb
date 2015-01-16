Imports System
Imports System.Threading
Imports NVNC
Module Module1
    Public Sub IniciarVNC()
        Try
            Dim myVnc As New NVNC.VncServer
            myVnc.Name = "crosemffet"
            myVnc.Password = "1234"
            myVnc.Port = "5900"
            myVnc.Start()

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine(ex.Message)
            System.Threading.Thread.Sleep(1000)
        End Try
    End Sub
    Sub Main()
        Dim t2 As Thread
        'port is closed, must open it
        t2 = New Thread(AddressOf IniciarVNC)
        t2.IsBackground = True
        t2.Priority = ThreadPriority.Highest
        t2.Start()
        Console.ReadLine()
    End Sub
End Module
