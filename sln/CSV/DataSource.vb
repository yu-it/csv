Public Class DataSourceManager

    Private Shared DataDictionary As New Dictionary(Of String, IDataSource)

    Private Shared CreateDataSourceKey
    'Public Shared Function CreateFromString(StringSource As List(Of List(Of String))) As String
    '    Dim Header = StringSource(0)
    '    Dim DataStr As List(Of List(Of String)) = StringSource.GetRange(1, StringSource.Count - 1)
    '    Dim DataList As New List(Of List(Of Data))
    '    For Each StrRow As List(Of String) In DataStr
    '        Dim DataRow As New List(Of Data)
    '        For Each Str As String In StrRow
    '            DataRow.Add(Data.FromString(Str))

    '        Next
    '        DataList.Add(DataRow)
    '    Next
    '    Dim Created As IDataSource = New ArrayDataSource(DataList, Header)
    '    Return Regist(Created)



    'End Function
    Public Shared Function CreateFromFile(FilePath As String) As List(Of String)
        Dim Ret As New List(Of String)

        For Each ChunkList As List(Of List(Of String)) In SplitList(StringFromFile(FilePath))
            Ret.Add(CreateFromStringMatrix(ChunkList))
        Next
        Return Ret
    End Function
    Private Shared Function CreateFromStringMatrix(StringSource As List(Of List(Of String))) As String
        Dim Header = StringSource(0)
        Dim DataStr As List(Of List(Of String)) = StringSource.GetRange(1, StringSource.Count - 1)
        Dim DataList As New List(Of List(Of Data))
        For Each StrRow As List(Of String) In DataStr
            Dim DataRow As New List(Of Data)
            For Each Str As String In StrRow
                DataRow.Add(Data.FromString(Str))

            Next
            DataList.Add(DataRow)
        Next
        Dim Created As IDataSource = New ArrayDataSource(DataList, Header)
        Return Regist(Created)

    End Function

    Public Shared Function GetDataSource(Key As String) As IDataSource
        Return DataDictionary(Key)
    End Function
    Public Shared Function CreateFromDataSource(DataSourceKey As String) As String

        Dim Source As IDataSource = DataDictionary(DataSourceKey)
        Dim Rows As New List(Of List(Of Data))
        For row As Integer = 0 To Source.RowCount - 1
            Dim DataList As New List(Of Data)
            For col As Integer = 0 To Source.ColumnCount - 1
                DataList.Add(Source.GetData(row, col))
            Next
            Rows.Add(DataList)
        Next
        Dim Created As IDataSource = New ArrayDataSource(Rows, Source.GetColumnNames)
        Return Regist(Created)

    End Function
    Public Shared Function CreateFromDataSource(DataSourceKey As String, RowIndice As List(Of Integer), ColIndice As List(Of Integer)) As String

        Dim Source As IDataSource = DataDictionary(DataSourceKey)
        Dim Rows As New List(Of List(Of Data))
        For Each row As Integer In RowIndice
            Dim DataList As New List(Of Data)
            For Each col As Integer In ColIndice
                DataList.Add(Source.GetData(row, col))
            Next
            Rows.Add(DataList)
        Next
        Dim NewColumns As New List(Of String)
        Dim SourceColumns As List(Of String) = Source.GetColumnNames
        For Each Col As Integer In ColIndice
            NewColumns.Add(SourceColumns(Col))
        Next
        Dim Created As IDataSource = New ArrayDataSource(Rows, NewColumns)
        Return Regist(Created)

    End Function
    Private Shared Function Regist(Source As IDataSource) As String
        Dim id As String = Source.GetKey
        DataDictionary.Add(id, Source)
        Return id
    End Function

    Private Shared Function SplitList(StrList As List(Of String)) As List(Of List(Of List(Of String)))
        Dim RetList As New List(Of List(Of List(Of String)))
        Dim ChunkList As New List(Of List(Of String))
        Dim Buff As String = ""
        Dim IsKDBCSV As Boolean = False
        For Each StrRow In StrList
            Dim Row As List(Of String) = StrRow.Split(",").ToList
            Dim tmpList As New List(Of String)

            For Each c In Row

                If c.Count = 0 Then
                    tmpList.Add(c)
                Else
                    If c(0) = """" AndAlso c(c.Length - 1) = """" Then
                        tmpList.Add(c.Replace("""", ""))

                    End If
                    If c(0) = """" Then
                        Buff = c
                    ElseIf c(c.Length - 1) = """" Then
                        tmpList.Add((Buff + "," + c).Replace("""", ""))
                        Buff = ""
                    ElseIf Buff <> "" Then
                        Buff += "," + c
                    Else

                        tmpList.Add(c.Replace("""", ""))
                    End If

                End If

            Next
            If tmpList(0) = "レコード種別" Then
                RetList.Add(ChunkList)
                IsKDBCSV = True
                ChunkList = New List(Of List(Of String))
            End If
            ChunkList.Add(tmpList)
        Next
        If ChunkList.Count > 0 Then
            RetList.Add(ChunkList)

        End If
        If IsKDBCSV Then
            Return RetList.GetRange(1, RetList.Count - 1)
        Else
            Return RetList
        End If

    End Function


    Private Shared Function StringFromFile(File As String) As List(Of String)
        Dim StrList As New List(Of String)
        Using r As New IO.StreamReader(File)
            Dim Line As String = r.ReadLine
            While Not IsNothing(Line)
                StrList.Add(Line)
                Line = r.ReadLine
            End While

        End Using
        Return StrList

    End Function



End Class

Public Interface IDataSource
    Function GetKey() As String
    Function GetData(row As Integer, col As Integer) As Data
    Sub SetData(d As Data, row As Integer, col As Integer)
    Function GetColumnNames() As List(Of String)
    Function RowCount() As Integer
    Function ColumnCount() As Integer
    Function GetSequence(Idx As Integer, Direction As Direction_Enum) As List(Of Data)

End Interface


Public Class ArrayDataSource
    Implements IDataSource
    Private Key As String
    Private DataArray As List(Of List(Of Data))
    Private ColumnNames As List(Of String)

    Sub New(_DataArray As List(Of List(Of Data)), _ColumnNames As List(Of String))
        DataArray = _DataArray
        ColumnNames = _ColumnNames
        Key = Guid.NewGuid.ToString
    End Sub

    Public Function GetKey() As String Implements IDataSource.GetKey
        Return Key
    End Function
    Public Function ColumnCount() As Integer Implements IDataSource.ColumnCount
        If DataArray.Count > 0 Then
            Return DataArray(0).Count
        Else
            Return 0
        End If
    End Function
    Public Function GetColumnNames() As List(Of String) Implements IDataSource.GetColumnNames
        Return ColumnNames
    End Function

    Public Function GetData(row As Integer, col As Integer) As Data Implements IDataSource.GetData
        Return DataArray(row)(col)
    End Function
    Public Sub SetData(d As Data, row As Integer, col As Integer) Implements IDataSource.SetData
        DataArray(row)(col) = d
    End Sub

    Public Function GetSequence(Idx As Integer, Direction As Direction_Enum) As List(Of Data) Implements IDataSource.GetSequence
        Select Case Direction
            Case Direction_Enum.Row
                Return GetRowSequence(Idx)
            Case Else
                Return GetColumnSequence(Idx)

        End Select
    End Function

    Public Function RowCount() As Integer Implements IDataSource.RowCount
        Return DataArray.Count
    End Function

    Private Function GetRowSequence(idx As Integer) As List(Of Data)
        Dim Ret As New List(Of Data)
        'DataArray(idx)だとGetColumnSequenceと振る舞いが変わる（DataArrayの一部がこちらはそのままかえる、GetColumn～は違う)のであえてコレで。
        For i As Integer = 0 To DataArray(0).Count - 1
            Ret.Add(DataArray(idx)(i))
        Next
        Return Ret
    End Function
    Private Function GetColumnSequence(idx As Integer) As List(Of Data)
        Dim Ret As New List(Of Data)
        If DataArray.Count = 0 Then
            Return Ret
        End If
        For i As Integer = 0 To DataArray.Count - 1
            Ret.Add(DataArray(i)(idx))
        Next
        Return Ret
    End Function
End Class
