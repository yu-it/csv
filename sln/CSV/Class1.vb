'絶対値の加算



Imports System.Text.RegularExpressions
Imports System.Drawing
Imports System.Text

Public Enum Direction_Enum
    Row
    Column
End Enum
Public Class CSV
    Private _CDatas As New List(Of Cdata)

#Region "Property"
    Public ReadOnly Property Overview As List(Of String)
        Get
            Dim Ret As New List(Of String)
            Dim Builder As New StringBuilder

            For i As Integer = 0 To Count() - 1
                Builder.Append(i)
                Builder.Append("-")
                Builder.Append(_CDatas(i).Cols)
                Builder.Append("Rows:")
                Builder.Append(_CDatas(i).RowCount)
                Builder.Append(vbCrLf)
                Builder.Append("Columns:")
                Builder.Append(_CDatas(i).ColumnCount)
                Builder.Append(vbCrLf)
                Ret.Add(Builder.ToString)
                Builder = New StringBuilder

            Next

            Return Ret
        End Get
    End Property
    Public Property OriginalFile As String = ""
    Public ReadOnly Property Count As Integer
        Get
            Return _CDatas.Count
        End Get
    End Property
    Default ReadOnly Property Item(ByVal index As Integer) As Cdata

        Get

            Return _CDatas(index)

        End Get

    End Property


#End Region
#Region "Operator"
    Public Function Mask(MaskData As CSV) As CSV

        If MaskData.Count <> Me.Count Then
            Throw New Exception("構造が違うのでムリ")
        End If
        Dim Ret As New CSV()
        Ret.OriginalFile = Me.OriginalFile

        For i As Integer = 0 To Me.Count - 1
            Ret._CDatas.Add(Me._CDatas(i).Mask(MaskData._CDatas(i)))

        Next
        Return Ret
    End Function
    '3^-3^r      :rowに3,colに-3の場所から値を取り、逆数を取る
    '3^-3^1      :rowに3,colに-3の場所から値を取り、1を掛け算する
    '3^-3^100    :rowに3,colに-3の場所から値を取り、100を掛け算する
    '0^0^.3      :小数第三位で四捨五入する。
    Public Function Transform(TransData As CSV) As CSV

        If TransData.Count <> Me.Count Then
            Throw New Exception("構造が違うのでムリ")
        End If
        Dim Ret As New CSV()
        Ret.OriginalFile = Me.OriginalFile

        For i As Integer = 0 To Me.Count - 1
            Ret._CDatas.Add(Me._CDatas(i).Transform(TransData._CDatas(i)))

        Next

        Return Ret

    End Function
    Public Function Overlay(OverlayData As CSV) As CSV
        If OverlayData.Count <> Me.Count Then
            Throw New Exception("構造が違うのでムリ")
        End If
        Dim Ret As New CSV()
        Ret.OriginalFile = Me.OriginalFile

        For i As Integer = 0 To Me.Count - 1
            Ret._CDatas.Add(Me._CDatas(i).Transform(OverlayData._CDatas(i)))

        Next

        Return Ret

    End Function

    Public Function Sum() As Decimal
        Dim ret As Decimal = CDec(0)
        For Each cd As Cdata In Me._CDatas
            ret += cd.Sum()
        Next
        Return ret
    End Function
    Friend Function Operation(ByVal c2 As Object, Op As OperationEnum) As CSV
        Dim c1 = Me
        Dim ret = New CSV
        ret.OriginalFile = Me.OriginalFile
        For Each cd As Cdata In _CDatas
            ret._CDatas.Add(cd.Operation(c2, Op))
        Next
        Return ret
    End Function

    Friend Function MassOperation(ByVal c2 As CSV, Op As OperationEnum) As CSV
        Dim c1 As CSV = Me
        If c1._CDatas.Count <> c2._CDatas.Count Then
            Throw New Exception("CSV数が違うから足せない")
        End If
        Dim ret = New CSV
        ret.OriginalFile = Me.OriginalFile
        For i As Integer = 0 To c1._CDatas.Count - 1
            ret._CDatas.Add(c1._CDatas(i).MassOperation(c2._CDatas(i), Op))
        Next
        Return ret

    End Function
    Public Function Plus(ByVal c2 As Object) As CSV
        If TypeOf c2 Is CSV Then
            Return MassOperation(CType(c2, CSV), OperationEnum.Plus)
        End If
        Return Operation(c2, OperationEnum.Plus)
    End Function
    Public Function Minus(ByVal c2 As Object) As CSV
        If TypeOf c2 Is CSV Then
            Return MassOperation(CType(c2, CSV), OperationEnum.Minus)
        End If
        Return Operation(c2, OperationEnum.Minus)
    End Function
    Public Function Product(ByVal c2 As Object) As CSV
        If TypeOf c2 Is CSV Then
            Return MassOperation(CType(c2, CSV), OperationEnum.Product)
        End If
        Return Operation(c2, OperationEnum.Product)
    End Function
    Public Function Divide(ByVal c2 As Object) As CSV
        If TypeOf c2 Is CSV Then
            Return MassOperation(CType(c2, CSV), OperationEnum.Divide)
        End If
        Return Operation(c2, OperationEnum.Divide)
    End Function
    Public Shared Operator +(ByVal c1 As CSV, ByVal c2 As Object) As CSV
        Return c1.Plus(c2)
    End Operator

    Public Shared Operator -(ByVal c1 As CSV, ByVal c2 As Object) As CSV
        Return c1.Minus(c2)

    End Operator
    Public Shared Operator *(ByVal c1 As CSV, ByVal c2 As Object) As CSV
        Return c1.Product(c2)
    End Operator
    Public Shared Operator /(ByVal c1 As CSV, ByVal c2 As Object) As CSV
        Return c1.Divide(c2)
    End Operator

#End Region

#Region "Public"



#End Region
#Region "Cool Treatment"
    Public Sub Show()
        Dim viewer As New CSVViewer
        viewer.data2 = Me
        viewer.ShowDialog()
    End Sub

#End Region

#Region "Factory系"
    Public Shared Function FromFile(Path As String) As CSV
        Dim NewObj As New CSV
        For Each DataKey As String In DataSourceManager.CreateFromFile(Path)
            NewObj._CDatas.Add(New Cdata(DataSourceManager.GetDataSource(DataKey), Path))
        Next

        NewObj.OriginalFile = Path
        Return NewObj
    End Function


#End Region
#Region "Index系"
    Public Shared Indice As New Dictionary(Of String, Dictionary(Of String, CSV))
    Public Shared ReadOnly Property Index As Dictionary(Of String, CSV)
        Get
            Return Indice("Default")
        End Get
    End Property

    Public Shared Sub Indexing(Path As String, Optional Name As String = "Default", Optional Filter As String = "*.csv", Optional Recurse As Boolean = False)
        If Not Indice.ContainsKey(Name) Then
            Indice.Add(Name, New Dictionary(Of String, CSV))
        End If
        Dim Index As Dictionary(Of String, CSV) = Indice.Item(Name)
        For Each f As String In System.IO.Directory.GetFiles( _
            Path, Filter, If(Recurse, System.IO.SearchOption.AllDirectories, System.IO.SearchOption.TopDirectoryOnly))
            Index.Add(CStr(Index.Count), FromFile(f))
        Next
    End Sub
    Public Shared Function DescIndex() As String
        Return DescIndex("Default")
    End Function
    Public Shared Function DescIndex(IndexName As String) As String
        If Not Indice.ContainsKey(IndexName) Then
            Return ""
        End If
        Dim Index As Dictionary(Of String, CSV) = Indice(IndexName)
        Dim Builder As New System.Text.StringBuilder
        For Each k As Integer In Index.Keys
            Builder.Append(k)
            Builder.Append(":")
            Builder.Append(Index(k).ToString)
            Builder.Append(vbCrLf)
        Next
        Return Builder.ToString
    End Function

    Shared Sub New()
        Indice.Add("Default", New Dictionary(Of String, CSV))
    End Sub
#End Region

    Public Overrides Function ToString() As String
        Dim Builder As New System.Text.StringBuilder
        Builder.Append(OriginalFile)
        For i As Integer = 0 To _CDatas.Count - 1
            Dim cd As Cdata = _CDatas(i)
            Builder.Append("[(")
            Builder.Append(i)
            Builder.Append(")")
            Builder.Append(cd.RowCount)
            Builder.Append(",")
            Builder.Append(cd.ColumnCount)
            Builder.Append("]")
        Next

        Return Builder.ToString
    End Function
    Public Function ToStringCSV() As String
        Dim Builder As New System.Text.StringBuilder
        For Each cd As Cdata In _CDatas
            Builder.Append(cd.ToString)
        Next

        Return Builder.ToString

    End Function


#Region "Enumerator IF"
    Public Class CSVEnumerator
        Implements IEnumerable(Of Cdata)
        Private _MyCDatas As List(Of Cdata)
        Public Sub New(ByVal __MyCDatas)
            _MyCDatas = __MyCDatas
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of Cdata) Implements IEnumerable(Of Cdata).GetEnumerator
            Return _MyCDatas.GetEnumerator
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return _MyCDatas.GetEnumerator
        End Function

    End Class
    Public Function stream() As CSVEnumerator
        Return New CSVEnumerator(Me._CDatas)
    End Function


#End Region
End Class

Enum OperationEnum
    Plus
    Minus
    Product
    Divide
    Reciprocal
    Round

End Enum

Public Class Cdata

    Public VisibleColumnNames As List(Of String)
    Public VisibleColumns As List(Of Integer)
    Public VisibleRows As List(Of Integer)

    Public ShowColumnName As Boolean = True
    Public ShowDescription As Boolean = False
    Public ShowColumnNum As Boolean = False
    Public ShowRowNum As Boolean = False
    Public ShowCoordinatesInCell As Boolean = False
#Region "Cool Treatment"
    Public Sub Show()
        Dim viewer As New CSVViewer
        viewer.data = Me
        viewer.ShowDialog()
    End Sub
#End Region

    Friend Pointer As Integer = -1

    Private Source As IDataSource
#Region "indexer"
    Default ReadOnly Property Item(ByVal index As Integer) As Cdata

        Get
            Select Case Direction
                Case Direction_Enum.Row
                    Return ProjectionAlongDirection(New Integer() {VisibleRows(index)}.ToList)
                Case Else
                    Return ProjectionAlongDirection(New Integer() {VisibleColumns(index)}.ToList)
            End Select

        End Get

    End Property
    ReadOnly Property Items(ByVal ParamArray index() As Integer) As Cdata

        Get
            Dim IndexList As New List(Of Integer)
            Dim VisibleIndexList As List(Of Integer)

            Select Case Direction
                Case Direction_Enum.Row
                    VisibleIndexList = VisibleRows

                Case Else
                    VisibleIndexList = VisibleColumns
            End Select
            For Each i In index
                IndexList.Add(VisibleIndexList(i))
            Next
            Return ProjectionAlongDirection(IndexList)

        End Get

    End Property
    Default ReadOnly Property Item(ByVal index As String) As Cdata

        Get
            Return ProjectionAlongColumns(New Integer() {VisibleColumnNames.IndexOf(index)}.ToList)

        End Get

    End Property
    ReadOnly Property Items(ByVal ParamArray index() As String) As Cdata

        Get
            Dim IList As New List(Of Integer)
            For Each C In index
                IList.Add(VisibleColumnNames.IndexOf(C))
            Next
            Return ProjectionAlongDirection(IList)

        End Get

    End Property

#End Region
#Region "Property"

    Public Property OriginalFile As String

    Public ReadOnly Property Cols As String
        Get
            Dim Builder As New StringBuilder
            Dim ColumnNames As List(Of String) = VisibleColumnNames
            For i As Integer = 0 To ColumnNames.Count - 1
                Builder.Append(i)
                Builder.Append("-")
                Builder.Append(ColumnNames(i))
                Builder.Append(vbCrLf)

            Next

            Return Builder.ToString
        End Get
    End Property
    Public ReadOnly Property RowCount As Integer
        Get
            Return VisibleRows.Count
        End Get
    End Property

    Public ReadOnly Property Count As Integer
        Get
            Select Case Direction
                Case Direction_Enum.Row
                    Return VisibleRows.Count
                Case Else
                    Return VisibleColumns.Count
            End Select
        End Get
    End Property

    Public ReadOnly Property ColumnCount As Integer
        Get
            Return VisibleColumns.Count
        End Get
    End Property

    Private _Direction As Direction_Enum
    Public Property Direction As Direction_Enum
        Get
            Return _Direction
        End Get
        Set(value As Direction_Enum)
            _Direction = value
            Me.Reset()
        End Set
    End Property

    Public ReadOnly Property Value As Data
        Get
            If Source.RowCount = 0 Then
                Return Data.ZeroData
            Else
                Return Source.GetData(VisibleRows(0), VisibleColumns(0))

            End If

        End Get
    End Property

#End Region
#Region "Constructor"
    Public Sub New(_Source As IDataSource, ByVal _OriginalFile As String)
        Source = _Source
        VisibleColumnNames = Source.GetColumnNames
        VisibleColumns = New List(Of Integer)
        For i As Integer = 0 To Source.ColumnCount - 1
            VisibleColumns.Add(i)
        Next
        VisibleRows = New List(Of Integer)
        For i As Integer = 0 To Source.RowCount - 1
            VisibleRows.Add(i)
        Next

    End Sub

    Public Sub New(_Source As IDataSource, _VisibleRows As List(Of Integer), _VisibleColumns As List(Of Integer))
        Source = _Source
        VisibleColumns = _VisibleColumns
        VisibleRows = _VisibleRows
        VisibleColumnNames = New List(Of String)
        Dim TmpCols As List(Of String) = Source.GetColumnNames
        For Each i As Integer In VisibleColumns
            VisibleColumnNames.Add(TmpCols(i))
        Next

    End Sub

#End Region

#Region "Public"
    Public Function IsProjection() As Boolean
        Return VisibleRows.Count <> Source.RowCount OrElse VisibleColumns.Count <> Source.ColumnCount
    End Function
    Public Function ToStringCSV() As String
        Return ToString()

    End Function

    Public Overrides Function ToString() As String
        Dim Builder As New System.Text.StringBuilder()
        If ShowDescription Then
            Builder.Append("Direction:" & Direction)
            Builder.Append(vbCrLf)
            Builder.Append("File:" & OriginalFile)
            Builder.Append(vbCrLf)

        End If
        If ShowColumnName Then
            If ShowRowNum Then
                Builder.Append("""RowNum"",")
            End If
            Dim ColumnNames As List(Of String) = VisibleColumnNames
            For Each ColIdx As Integer In VisibleColumns
                Dim C As String = Source.GetColumnNames(ColIdx)
                Builder.Append("""")
                If ShowColumnNum Then
                    Builder.Append("[" + CStr(ColIdx) + "]")
                End If
                Builder.Append(C)
                Builder.Append(""",")
            Next
            If Builder.Length > 0 Then
                Builder.Remove(Builder.Length - 1, 1)
            End If
            Builder.Append(vbCrLf)
        End If

        For Each RowIdx As Integer In VisibleRows
            Dim Row As List(Of Data) = Source.GetSequence(RowIdx, Direction_Enum.Row)
            If ShowRowNum Then
                Builder.Append("""" & CStr(RowIdx) & """,")
            End If

            For Each ColIdx As Integer In VisibleColumns
                Dim c As Data = Row(ColIdx)
                Builder.Append("""")
                If ShowCoordinatesInCell Then
                    Builder.Append("[" & CStr(RowIdx) & "," & CStr(ColIdx) & "]")

                End If
                Builder.Append(c.ToString)
                Builder.Append(""",")
            Next
            If Builder.Length > 0 Then
                Builder.Remove(Builder.Length - 1, 1)
            End If
            Builder.Append(vbCrLf)
        Next
        Return Builder.ToString


    End Function

    Public Function Clone() As Cdata

        Dim Rec As New Cdata(DataSourceManager.GetDataSource(DataSourceManager.CreateFromDataSource(Me.Source.GetKey)), Me.VisibleRows, Me.VisibleColumns)
        Rec.Direction = Me.Direction
        Return Rec 'これ、何とかしておきたい
    End Function


#End Region

#Region "Private"
    Private Function ProjectionAlongDirection(Indice As List(Of Integer), Optional Transpose As Boolean = True) As Cdata
        Select Case Direction
            Case Direction_Enum.Row
                Return ProjectionAlongRows(Indice)

            Case Else
                Return ProjectionAlongColumns(Indice)

        End Select
    End Function
    Private Function ProjectionAlongRows(Indice As List(Of Integer), Optional Transpose As Boolean = True) As Cdata
        Dim Ret As Cdata = New Cdata(Source, Indice, VisibleColumns)
        If Transpose Then
            Ret.Direction = Direction_Enum.Column
        End If
        Return Ret
    End Function
    Private Function ProjectionAlongColumns(Indice As List(Of Integer), Optional Transpose As Boolean = True) As Cdata
        Dim Ret As Cdata = New Cdata(Source, VisibleRows, Indice)
        If Transpose Then
            Ret.Direction = Direction_Enum.Row
        End If
        Return Ret
    End Function
    Private Sub SetDataUsingVisibleIndex(ByVal d As Data, ByVal VisibleRowIdx As Integer, ByVal VisibleColIdx As Integer)
        Source.SetData(d, VisibleRows(VisibleRowIdx), VisibleColumns(VisibleColIdx))

    End Sub
    Private Function GetDataUsingVisibleIndex(ByVal VisibleRowIdx As Integer, ByVal VisibleColIdx As Integer) As Data
        Return Source.GetData(VisibleRows(VisibleRowIdx), VisibleColumns(VisibleColIdx))

    End Function



#End Region

#Region "Operator"

#Region "Operator_Aggregate"
    '演算子のうちスカラ値を返すもの
    Public Function Sum() As Decimal
        Dim ret As Decimal = CDec(0)
        For Each row_idx As Integer In VisibleRows
            For Each col_idx As Integer In VisibleColumns
                Dim col As Data = Source.GetData(row_idx, col_idx)
                If col.Dtype = Data.DTypeEnum.Numeric Then
                    ret += col.NData
                End If
            Next
        Next
        Return ret
    End Function

#End Region

#Region "Operator_SingleTerm"
    '単項演算

    'Public Function R() As Decimal  'reciprocal
    '    Dim Ret As Cdata = If(IsInmutable, Me.Shrink, Me)
    '    Return Ret
    'End Function

#End Region
    Private MaskedCell As New HashSet(Of Point) '実座標系で
#Region "Operator_Dicer"
    'イテレーション方向をかえる。
    Public Function T() As Cdata
        Dim ret As Cdata = Me.Clone
        Select Case Direction
            Case Direction_Enum.Row
                ret.Direction = Direction_Enum.Column
            Case Else
                ret.Direction = Direction_Enum.Row
        End Select
        Return ret

    End Function
    Public Function RD() As Cdata
        Dim ret As Cdata = Me.Clone
        Select Case Direction
            Case Direction_Enum.Row
                ret.Direction = Direction_Enum.Column
            Case Else
                ret.Direction = Direction_Enum.Row
        End Select
        Return ret

    End Function
    Public Function CD() As Cdata
        Dim ret As Cdata = Me.Clone
        Select Case Direction
            Case Direction_Enum.Row
                ret.Direction = Direction_Enum.Column
            Case Else
                ret.Direction = Direction_Enum.Row
        End Select
        Return ret

    End Function

#End Region

#Region "operator_Util"
    Public Function Shrink() As Cdata
        Dim RetSource As IDataSource = DataSourceManager.GetDataSource(DataSourceManager.CreateFromDataSource(Me.Source.GetKey, Me.VisibleRows, Me.VisibleColumns))
        Return New Cdata(RetSource, OriginalFile)

    End Function

#End Region
    Public Property IsInmutable As Boolean = False  'ゆくゆくデータソース的に変更が効かないヤツの場合にsetメソッドで例外投げるようにしたりしたい
    Public Function Mask(MaskData As Cdata) As Cdata
        Dim Ret As Cdata = If(IsInmutable, Me.Shrink, Me)

        If Ret.RowCount <> MaskData.RowCount OrElse Ret.ColumnCount <> MaskData.ColumnCount Then
            Throw New Exception("構造が違うのでムリ")

        End If
        For row_idx As Integer = 0 To MaskData.RowCount - 1
            For col_idx As Integer = 0 To MaskData.ColumnCount - 1
                If MaskData(row_idx)(col_idx).Value.SData = "1" Then
                    Ret.MaskedCell.Add(New Point(row_idx, col_idx))
                ElseIf Not MaskData(row_idx)(col_idx).Value.SData = "0" Then
                    Throw New Exception("マスクは0か1で書いて(" & MaskData(row_idx)(col_idx).Value.SData & ")")
                End If
            Next

        Next
        Return Ret
    End Function
    '3^-3^r      :rowに3,colに-3の場所から値を取り、逆数を取る
    '3^-3^1      :rowに3,colに-3の場所から値を取り、1を掛け算する
    '3^-3^100    :rowに3,colに-3の場所から値を取り、100を掛け算する
    '0^0^.3      :小数第三位で四捨五入する。
    Public Function Transform(TransData As Cdata) As Cdata
        Dim WorkData As Cdata = Me.Shrink
        Dim Ret As Cdata = If(IsInmutable, Me.Shrink, Me)

        If Ret.RowCount <> TransData.RowCount OrElse Ret.ColumnCount <> TransData.ColumnCount Then
            Throw New Exception("構造が違うのでムリ")

        End If
        For row_idx As Integer = 0 To TransData.RowCount - 1
            For col_idx As Integer = 0 To TransData.ColumnCount - 1
                Dim Description_src As String = TransData(row_idx)(col_idx).Value.SData
                Dim Descriptions As String() = Description_src.Split("^")
                If Descriptions.Count <> 3 Then
                    Throw New Exception(Description_src & "ではない、row^col^[属性(例えばr,数値,.数値]")
                End If
                Dim offset_row As Integer
                Dim offset_col As Integer
                Dim attr As String = Descriptions(2).ToLower
                Try
                    offset_row = CInt(Descriptions(0))
                Catch ex As Exception
                    Throw New Exception(Description_src & "のrowオフセットが数値ではない、書式はrow^col^[属性(例えばr,数値,.数値]だす。")

                End Try
                Try
                    offset_col = CInt(Descriptions(1))
                Catch ex As Exception
                    Dim target_idx As Integer = Ret.VisibleColumnNames.IndexOf(Descriptions(1))
                    If target_idx = -1 Then
                        Throw New Exception(Description_src & "の" & Descriptions(1) & "という列はない")

                    End If
                    offset_col = target_idx - col_idx

                End Try

                If Not (attr = "r" Or Regex.IsMatch(attr, "\d+") Or Regex.IsMatch(attr, "\.\d+")) Then
                    Throw New Exception(Description_src & "の属性がヤバイ、r（逆数）,数値（定数倍）,.数値（四捨五入）のどれかで")

                End If



                Dim TargetData As Data = WorkData.Source.GetData(row_idx + offset_row, col_idx + offset_col)
                If TargetData.Dtype = Data.DTypeEnum.Numeric Then

                    If attr = "r" Then
                        TargetData = Data.FromObject(CDec(1) / TargetData.NData)
                    ElseIf Regex.IsMatch(attr, "^\d+$") Then
                        TargetData = Data.FromObject(CDec(attr) * TargetData.NData)

                    ElseIf Regex.IsMatch(attr, "^\.\d+$") Then
                        Dim scale As Integer = CInt(attr.Substring(1))
                        TargetData = Data.FromObject(Decimal.Round(TargetData.NData, scale))


                    End If
                    Ret.SetDataUsingVisibleIndex(TargetData, row_idx, col_idx)

                End If
            Next

        Next
        Return Ret

    End Function
    Public Function Overlay(OverlayData As Cdata) As Cdata
        Dim Ret As Cdata = If(IsInmutable, Me.Clone, Me)
        If Ret.RowCount <> OverlayData.RowCount OrElse Ret.ColumnCount <> OverlayData.ColumnCount Then
            Throw New Exception("構造が違うのでムリ")

        End If
        If Me.IsProjection Then
            Throw New Exception("オーバレイは射影したものには適用できない")

        End If
        For row_idx As Integer = 0 To OverlayData.RowCount - 1
            For col_idx As Integer = 0 To OverlayData.ColumnCount - 1
                If Not Me.MaskedCell.Contains(New Point(row_idx, col_idx)) Then
                    Ret.Source.SetData(OverlayData.ProjectionAlongRows(New Integer() {row_idx}.ToList).ProjectionAlongColumns(New Integer() {col_idx}.ToList).Value, row_idx, col_idx)

                End If
            Next

        Next
        Return Ret


    End Function
    Public Shared Operator +(ByVal c1 As Cdata, ByVal c2 As Object) As Cdata
        Return c1.Plus(c2)
    End Operator

    Public Shared Operator -(ByVal c1 As Cdata, ByVal c2 As Object) As Cdata
        Return c1.Minus(c2)

    End Operator
    Public Shared Operator *(ByVal c1 As Cdata, ByVal c2 As Object) As Cdata
        Return c1.Product(c2)
    End Operator
    Public Shared Operator /(ByVal c1 As Cdata, ByVal c2 As Object) As Cdata
        Return c1.Divide(c2)
    End Operator
    Public Function Plus(ByVal c2 As Object) As Cdata
        If TypeOf c2 Is Cdata Then
            Return MassOperation(CType(c2, Cdata), OperationEnum.Plus)
        End If
        Return Operation(c2, OperationEnum.Plus)
    End Function
    Public Function Minus(ByVal c2 As Object) As Cdata
        If TypeOf c2 Is Cdata Then
            Return MassOperation(CType(c2, Cdata), OperationEnum.Minus)
        End If
        Return Operation(c2, OperationEnum.Minus)
    End Function
    Public Function Product(ByVal c2 As Object) As Cdata
        If TypeOf c2 Is Cdata Then
            Return MassOperation(CType(c2, Cdata), OperationEnum.Product)
        End If
        Return Operation(c2, OperationEnum.Product)
    End Function
    Public Function Divide(ByVal c2 As Object) As Cdata
        If TypeOf c2 Is Cdata Then
            Return MassOperation(CType(c2, Cdata), OperationEnum.Divide)
        End If
        Return Operation(c2, OperationEnum.Divide)
    End Function

    Friend Function Operation(ByVal c2 As Object, Op As OperationEnum) As Cdata
        Dim Ret As Cdata = If(IsInmutable, Me.Shrink, Me)

        For Row As Integer = 0 To Ret.RowCount - 1
            For Col = 0 To Ret.ColumnCount - 1
                Dim TargetData As Data = Ret.GetDataUsingVisibleIndex(Row, Col)
                If Not MaskedCell.Contains(New Point(Row, Col)) Then
                    Ret.SetDataUsingVisibleIndex(TargetData.Operation(c2, Op), Row, Col)

                End If
            Next
        Next
        Return Ret
    End Function

    Friend Function MassOperation(ByVal c2 As Cdata, Op As OperationEnum) As Cdata
        Dim Ret As Cdata = If(IsInmutable, Me.Shrink, Me)
        Dim Arg As Cdata = c2.Shrink
        If Ret.ColumnCount <> Arg.ColumnCount Then
            Throw New Exception("列数が違うから足せない")
        End If
        If Arg.RowCount > Ret.RowCount Then
            Throw New Exception("左側を行数の大きなCSVにするか同じ行数のCSVにしてください。")
        End If

        For Row As Integer = 0 To Arg.RowCount - 1
            For Col = 0 To Arg.ColumnCount - 1
                If Not MaskedCell.Contains(New Point(Row, Col)) Then
                    Dim TargetData As Data = Ret.GetDataUsingVisibleIndex(Row, Col)
                    Dim TargetData2 As Data = Arg.GetDataUsingVisibleIndex(Row, Col)
                    Ret.SetDataUsingVisibleIndex(TargetData.Operation(TargetData2, Op), Row, Col)

                End If
            Next

        Next
        Return Ret

    End Function

#End Region

#Region "Enumerator IF"
    Public Function stream() As CdataEnumerator
        Return New CdataEnumerator(Me)
    End Function
    Public Sub Reset()
        Me.Pointer = -1

    End Sub
    Public Class CdataEnumerator
        Implements IEnumerable(Of Cdata), IEnumerator(Of Cdata)
        Private _MyParent As Cdata
        Public Sub New(ByVal __MyParent As Cdata)
            _MyParent = __MyParent.Clone
        End Sub

        Public Sub Dispose() Implements IEnumerator(Of Cdata).Dispose

        End Sub
        Public Function GetEnumerator() As IEnumerator(Of Cdata) Implements IEnumerable(Of Cdata).GetEnumerator
            Return New CdataEnumerator(_MyParent)
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New CdataEnumerator(_MyParent)
        End Function

        Public ReadOnly Property Current As Cdata Implements IEnumerator(Of Cdata).Current
            Get
                Select Case _MyParent.Direction
                    Case Direction_Enum.Row
                        Return _MyParent.ProjectionAlongDirection(New Integer() {_MyParent.VisibleRows(_MyParent.Pointer)}.ToList, False)
                    Case Else
                        Return _MyParent.ProjectionAlongDirection(New Integer() {_MyParent.VisibleColumns(_MyParent.Pointer)}.ToList, False)

                End Select
            End Get
        End Property

        Public ReadOnly Property Current1 As Object Implements IEnumerator.Current
            Get
                Return Current
            End Get
        End Property


        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext

            _MyParent.Pointer += 1
            If _MyParent.Pointer < _MyParent.Count Then
                Return True
            Else
                Return False

            End If

        End Function
        Public Sub Reset() Implements IEnumerator.Reset
            _MyParent.Reset()

        End Sub
    End Class



#End Region

End Class


Public Class Data
    Public Shared ZeroData As Data = FromObject(0)
    Public Function Clone() As Data
        Return Data.FromObject(Me.Obj)

    End Function

    Public Overrides Function ToString() As String
        If Dtype = DTypeEnum.Numeric Then
            Return Me.NData.ToString("#,##0.############")
        End If
        Return Me.RawString
    End Function
    Public Shared Function FromString(Str As String) As Data
        Dim d As New Data
        d.Dtype = GetDType(Str)

        Select Case d.Dtype
            Case DTypeEnum.Chara
                d.Obj = Str.ToString
                d.RawString = Str.ToString
            Case DTypeEnum.Numeric
                d.Obj = CDec(Str)
                d.RawString = Str

        End Select
        Return d
    End Function
    Public Shared Function FromObject(Obj As Object) As Data
        Dim d As New Data
        d.Dtype = GetDType(Obj)

        Select Case d.Dtype
            Case DTypeEnum.Chara
                d.Obj = Obj.ToString
                d.RawString = Obj.ToString
            Case DTypeEnum.Numeric
                d.Obj = CDec(Obj)
                d.RawString = Obj.ToString

        End Select
        Return d


    End Function

    Private Shared Function GetDType(Arg As Object) As DTypeEnum

        Dim T As Type = Arg.GetType
        If {1.GetType, New Decimal(1).GetType, 1.1.GetType}.Contains(T) Then
            Return DTypeEnum.Numeric
        End If

        If Regex.IsMatch(Arg, "^[\d,\.]+$") Then
            Return DTypeEnum.Numeric
        End If
        Return DTypeEnum.Chara


    End Function


    Public Enum DTypeEnum
        Chara
        Numeric
    End Enum

    Public Dtype As DTypeEnum

    Public Obj As Object
    Public RawString As String

    Public Function Plus(obj As Object) As Data
        Return Operation(obj, OperationEnum.Plus)
    End Function
    Public Function Minus(obj As Object) As Data
        Return Operation(obj, OperationEnum.Minus)
    End Function
    Public Function Product(obj As Object) As Data
        Return Operation(obj, OperationEnum.Product)
    End Function
    Public Function Divide(obj As Object) As Data
        Return Operation(obj, OperationEnum.Divide)
    End Function

    Friend Function Operation(ByVal c2 As Object, Op As OperationEnum) As Data
        If TypeOf c2 Is Data Then
            c2 = CType(c2, Data).Obj
        End If
        If GetDType(c2) = DTypeEnum.Numeric Then
            Dim cd2 As Decimal = CDec(c2)
            Select Case Op
                Case OperationEnum.Plus
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Dim ret = FromObject(Me.NData + cd2)
                        Return ret
                    End If
                Case OperationEnum.Minus
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Dim ret = FromObject(Me.NData - cd2)
                        Return ret
                    End If
                Case OperationEnum.Product
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Dim ret = FromObject(Me.NData * cd2)
                        Return ret
                    End If
                Case Else
                    If Me.Dtype = DTypeEnum.Numeric Then
                        If cd2 = 0 Then
                            Return Data.ZeroData
                        End If
                        Dim ret = FromObject(Me.NData / cd2)
                        Return ret
                    End If

            End Select
        Else
            Dim cs2 As String = CStr(c2)
            Select Case Op
                Case OperationEnum.Plus
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Return Me.Clone
                    End If
                Case OperationEnum.Minus
                    Return FromObject(Me.SData.CompareTo(cs2))

                Case OperationEnum.Product
                    Return Me.Clone
                Case Else
                    Return Me.Clone

            End Select
        End If


        Return Me.Clone

    End Function

    Friend Function SingleTermOperation(ByVal c2 As Object, Op As OperationEnum) As Data
        If TypeOf c2 Is Data Then
            c2 = CType(c2, Data).Obj
        End If
        If GetDType(c2) = DTypeEnum.Numeric Then
            Dim cd2 As Decimal = CDec(c2)
            Select Case Op
                Case OperationEnum.Plus
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Dim ret = FromObject(Me.NData + cd2)
                        Return ret
                    End If
                Case OperationEnum.Minus
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Dim ret = FromObject(Me.NData - cd2)
                        Return ret
                    End If
                Case OperationEnum.Product
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Dim ret = FromObject(Me.NData * cd2)
                        Return ret
                    End If
                Case Else
                    If Me.Dtype = DTypeEnum.Numeric Then
                        If cd2 = 0 Then
                            Return Data.ZeroData
                        End If
                        Dim ret = FromObject(Me.NData / cd2)
                        Return ret
                    End If

            End Select
        Else
            Dim cs2 As String = CStr(c2)
            Select Case Op
                Case OperationEnum.Plus
                    If Me.Dtype = DTypeEnum.Numeric Then
                        Return Me.Clone
                    End If
                Case OperationEnum.Minus
                    Return FromObject(Me.SData.CompareTo(cs2))

                Case OperationEnum.Product
                    Return Me.Clone
                Case Else
                    Return Me.Clone

            End Select
        End If


        Return Me.Clone

    End Function


    Public ReadOnly Property SData As String
        Get
            If Dtype = DTypeEnum.Chara Then
                Return Obj.ToString
            Else
                Return Obj.ToString '書式とかどうしようか・・・。
            End If
        End Get
    End Property

    Public ReadOnly Property NData As Decimal
        Get
            If Dtype = DTypeEnum.Chara Then
                Throw New Exception("文字だし")
            Else
                Return CDec(Obj)
            End If

        End Get
    End Property

End Class

