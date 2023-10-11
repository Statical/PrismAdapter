$CurrentDir = Split-Path -Path $PSScriptRoot -Leaf
Write-Host Current directory: $CurrentDir
if ($CurrentDir.Contains("src")) {
    Write-Host Deleting bin and obj folders...
    Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }
    Write-Host ..done!
} else {
    Write-Host NOT deleting, as current directory name does not start with src
}
Read-Host -Prompt "Press Enter to continue..."