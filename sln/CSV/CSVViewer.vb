Public Class CSVViewer

    Public data As Cdata
    Public data2 As CSV
    Private Sub CSVViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        DataGridView1.Rows.Clear()
        DataGridView1.Columns.Clear()

        If Not IsNothing(data) Then
            For i As Integer = 1 To data.ColumnCount
                DataGridView1.Columns.Add(CStr(i), CStr(i))
            Next
            DataGridView1.Rows.Add(data.VisibleColumnNames.ToArray)
            Dim DisplayData As Cdata = data.Clone()
            DisplayData.Direction = Direction_Enum.Row
            For Each row As Cdata In DisplayData.stream
                Dim List As New List(Of String)
                For Each col As Cdata In row.stream
                    List.Add(col.Value.ToString)

                Next
                DataGridView1.Rows.Add(List.ToArray())
            Next
        ElseIf Not IsNothing(data2) Then
            Dim MaxOfColumnCount As Integer = -1
            For Each cd As Cdata In data2.stream
                If MaxOfColumnCount < cd.ColumnCount Then
                    MaxOfColumnCount = cd.ColumnCount
                End If

            Next
            For i As Integer = 1 To MaxOfColumnCount
                DataGridView1.Columns.Add(CStr(i), CStr(i))
            Next
            For Each cd As Cdata In data2.stream
                Dim DisplayData As Cdata = cd.Clone()
                DisplayData.Direction = Direction_Enum.Row
                DataGridView1.Rows.Add(cd.VisibleColumnNames.ToArray)
                For Each row As Cdata In DisplayData.stream
                    Dim List As New List(Of String)
                    For Each col As Cdata In row.stream
                        List.Add(col.Value.ToString)

                    Next
                    DataGridView1.Rows.Add(List.ToArray())
                Next

            Next

        End If

    End Sub
End Class