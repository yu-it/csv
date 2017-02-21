[Reflection.Assembly]::LoadFrom((([io.DirectoryInfo]$MyInvocation.MyCommand.Path).parent.FullName) + "\\CSV.dll")


function Indexing {
    [CmdletBinding()]
    param (
        [parameter(Mandatory=$true,ValueFromPipeline=$true,Position=0)]
        [string[]]$path,
        
        [parameter(Mandatory=$false,Position=1)]
        #[alias("PF")]
        [AllowEmptyString()]
        [string]$name,
        
        [parameter(Mandatory=$false)]
        #[alias("PF")]
        [AllowEmptyString()]
        [switch]$recurse,
        
        [parameter(Mandatory=$false	)]
        [AllowEmptyString()]
        [string]$filter
        
    )
    PROCESS {
        if ($recurse -eq $null) {
            $recurse = $false
        }
        if ($name -eq "") {
            $name = "Default"
        }
        if ($filter -eq "") {
            $filter = "*.csv"
        }
        $path | %{
            [CSV]::Indexing($_, $name, $Filter, $recurse)
        }
    }
}

function desc {
    [CmdletBinding()]
    param (
        [parameter(Mandatory=$false)]
        [string[]]$name
        
        
    )
    PROCESS {
        
        if ($name -eq $null) {
            $name = @("Default")
        }
        $name | %{
            $idx_name = $_
            if (-not [CSV]::Indice.ContainsKey($idx_name)) {
                return
            }
            $index = [CSV]::Indice[$idx_name]
            if ($index -eq $null) {
                return
            }
            $index.keys | %{
                #$mem = @{"idx"=$idx_name; "key"=$_; "csvdesc"=$_;}
                #$ret = New-Object pscustomobject -property $mem
                #$ret
                "idx=" + $idx_name + ",key=" + $_ + ", file=" + $index[$_].toString();
            }
            
            <#
            $index.keys | %{
                $mem = @{"idx"=$idx_name; "key"=$_; "csvdesc"=$index[$_].toString(); "csv"=$index[$_];}
                $ret = New-Object pscustomobject -property $mem
                $ret
            }
            #>
        
        }
        
        
    }
}
function clr {
    [CmdletBinding()]
    param (
        [parameter(Mandatory=$false)]
        [string[]]$name
        
        
    )
    PROCESS {
        
        if ($name -eq $null) {
            $name = @("Default")
        }
        $name | %{
            $idx_name = $_
            if (-not [CSV]::Indice.ContainsKey($idx_name)) {
                return
            }
            $index = [CSV]::Indice[$idx_name]
            if ($index -eq $null) {
                return
            }
            $index.clear()	
        
        }
        
        
    }
}


function sel {
    [CmdletBinding()]
    param (
        [parameter(Mandatory=$true)]
        [string]$name,
        [parameter(Mandatory=$false)]
        [string]$idxname
        
        
    )
    PROCESS {
        
        if ($idxname -eq "") {
            $idxname = "Default"
        }
        if ([CSV]::indice.ContainsKey($idxname)) {
            [CSV]::indice[$idxname][$name]
        }
        
        
    }
}
function show {
    [CmdletBinding()]
    param (
        [parameter(Mandatory=$true)]
        [CSV]$obj
        
        
    )
    PROCESS {
        
        $file = [IO.path]::GetTempFileName()
        $obj.ToStringCSV() > $file
        start CSVViewer.exe $file
        
    }
}

function test {
    [CmdletBinding()]
    param (
        [parameter(Mandatory=$false)]
        [string[]]$name
        
    )
    PROCESS {
        write-host($name -is [Array])
        
    }
}
