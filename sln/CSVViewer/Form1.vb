Public Class Form1

    Private Data2 As CSV
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            If System.Environment.GetCommandLineArgs().Count <= 1 Then

                CSV.Indexing("C:\Program Files\#d\20170217_temp\t", "data")
                CSV.Indexing("C:\Program Files\#d\20170217_temp\t\filter", "filter")
                Dim data As CSV = CSV.Indice("data")(2)
                Dim bunbo As CSV = CSV.Indice("filter")(0)
                Dim bunshi As CSV = CSV.Indice("filter")(1)
                Dim databunshi As CSV = data.Transform(bunshi)


                Return

            End If
            Dim File As String = System.Environment.GetCommandLineArgs(1)
            Data2 = CSV.FromFile(File)
            DataGridView1.Rows.Clear()
            DataGridView1.Columns.Clear()

            Dim MaxOfColumnCount As Integer = -1
            For Each cd As Cdata In Data2.stream
                If MaxOfColumnCount < cd.ColumnCount Then
                    MaxOfColumnCount = cd.ColumnCount
                End If

            Next
            For i As Integer = 1 To MaxOfColumnCount
                DataGridView1.Columns.Add(CStr(i), CStr(i))
            Next
            For Each cd As Cdata In Data2.stream
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

        Catch ex As Exception

        End Try


    End Sub
End Class
