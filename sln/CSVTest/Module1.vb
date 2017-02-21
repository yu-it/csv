
Imports CSV
Module Module1



    Sub Main()

        CSV.Indexing("C:\Program Files\#d\20170217_temp\t")
        Dim CSVD As CSV = CSV.Index.Item(0)
        Dim a As Cdata = CSVD(1)
        Trace.WriteLine(a(2)(7))
        Trace.WriteLine(a(2)(7) * 2)



        Trace.WriteLine(a(5))

        Dim d As CSV = CSVD + CSVD
        Trace.WriteLine(d.ToString)

        For Each cd As Cdata In CSVD.stream
            Trace.WriteLine(cd(0)(0)(0))
            For Each row As Cdata In cd.stream
                Trace.WriteLine("cd " + row(0).Value.RawString + ", " + row(1).Value.RawString)

            Next
            Trace.WriteLine("cd start")
        Next

        Dim CSVFile As CSV = CSV.FromFile("C:\\new_itowork\\#temp\\d健診・医療・介護データからみる地域の健康課題.csv")
        For Each CSV As Cdata In CSVFile.stream
            MsgBox("a")
        Next
    End Sub

End Module
