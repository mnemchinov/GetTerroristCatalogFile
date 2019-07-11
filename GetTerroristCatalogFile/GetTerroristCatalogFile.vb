Module GetTerroristCatalogFile
    Public Function Main() As Integer
        Dim strNameParametr As String = ""
        Dim pFilePath As String = ""
        Dim pUserName As String = ""
        Dim pPassword As String = ""
        Dim pType As String = "XML"
        Dim DoGetFile As Boolean = True
        Dim Errors As Boolean = False
        If My.Application.CommandLineArgs.Count = 0 Then
            PrintInfo()
            Return 1
            Exit Function
        End If
        For Each args As String In My.Application.CommandLineArgs
            strNameParametr = LCase(Left(args, 2))
            'console.writeline(args)
            'Console.WriteLine(strNameParametr + " - " + Right(args, Len(args) - 2))
            Select Case strNameParametr
                Case "/?"
                    PrintInfo()
                    Exit Function
                Case "/f"
                    pFilePath = Right(args, Len(args) - 2)
                Case "/u"
                    pUserName = Right(args, Len(args) - 2)
                Case "/p"
                    pPassword = Right(args, Len(args) - 2)
                Case "/t"
                    pType = UCase(Right(args, Len(args) - 2))
            End Select
            Select Case UCase(args)
                Case "getfile"
                    DoGetFile = True
            End Select
        Next
        If pFilePath.Trim() = "" Then
            Console.WriteLine("Не задано имя выходного файла!")
            Errors = True
        End If
        If pUserName.Trim() = "" Then
            Console.WriteLine("Не задано имя пользователя!")
            Errors = True
        End If
        If pPassword.Trim() = "" Then
            Console.WriteLine("Не задан пароль пользователя!")
            Errors = True
        End If
        If Errors Then
            Return 1
            Exit Function
        End If
        If DoGetFile Then
            If GetTerroristCatalogFile(pFilePath, pUserName, pPassword, pType) Then
                Return 0
                Exit Function
            Else
                Return 1
                Exit Function
            End If
        End If
        Return 0
    End Function

    Private Function CreateClient() As TerroristInfoService.TerroristInfoServiceClient
        Dim binding As New System.ServiceModel.WSHttpBinding()
        With binding
            .Name = "WSHttpBinding_ITerroristInfoService"
            .MaxBufferPoolSize = "400000000"
            .MaxReceivedMessageSize = "400000000"
            .Security.Mode = System.ServiceModel.SecurityMode.TransportWithMessageCredential
            .Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.None
            .Security.Message.ClientCredentialType = System.ServiceModel.MessageCredentialType.UserName
        End With
        Dim EndPointAdress As New System.ServiceModel.EndpointAddress("https://portal.fedsfm.ru/Services/TerroristInfoService/TerroristInfoService.svc")
        Dim client As TerroristInfoService.TerroristInfoServiceClient = New TerroristInfoService.TerroristInfoServiceClient(binding, EndPointAdress)
        Return client
    End Function

    Private Function GetTerroristCatalogFile(ByVal FilePath As String, ByVal UserName As String, ByVal Password As String, ByVal Type As String) As Boolean
        Try
            Dim client As TerroristInfoService.TerroristInfoServiceClient = CreateClient()
            client.ClientCredentials.UserName.UserName = UserName
            client.ClientCredentials.UserName.Password = Password
            Dim TerroristCatalog As TerroristInfoService.TerroristCatalog = client.GetCurrentTerroristCatalog()
            Dim TerroristCatalogId As Guid
            If Type = "DBF" Then
                TerroristCatalogId = TerroristCatalog.IdDbf.Value
            Else
                TerroristCatalogId = TerroristCatalog.IdXml.Value
            End If
            Dim TerroristCatalogFile As TerroristInfoService.PortalFile = client.GetFile(TerroristCatalogId)
            My.Computer.FileSystem.WriteAllBytes(FilePath, TerroristCatalogFile.FileData, False)
            client.Close()
            Return True
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            Return False
        End Try
    End Function

    Private Sub PrintInfo()
        Console.WriteLine(My.Application.Info.ProductName + " version " + My.Application.Info.Version.ToString + " " + My.Application.Info.Copyright)
        Console.WriteLine(My.Application.Info.Description)
        Console.WriteLine("gtcf.exe GETFILE /f<Путь> /u<Имя> /p<Пароль> /tXML")
        Console.WriteLine(" Параметры:")
        Console.WriteLine("     GETFILE - Скачать актуальный файл со списком террористов ")
        Console.WriteLine("     /f<Путь>- Имя выходного файла")
        Console.WriteLine("     /u<Имя> - Имя пользователя личного кабинета fedsfm.ru")
        Console.WriteLine("     /p<Пароль> - Пароль пользователя личного кабинета fedsfm.ru")
        Console.WriteLine("     /t<Тип> - Тип файла (XML/DBF)")
        'Console.ReadLine()
    End Sub
End Module
