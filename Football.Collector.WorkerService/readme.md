# Create and manage the Windows Service

```
$acl = Get-Acl "{EXE PATH}"
$aclRuleArgs = "{DOMAIN OR COMPUTER NAME\USER}", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "{EXE PATH}"
```

New-Service -Name {SERVICE NAME} -BinaryPathName "{EXE FILE PATH}" -Credential "{DOMAIN OR COMPUTER NAME\USER}" -Description "{DESCRIPTION}" -DisplayName "{DISPLAY NAME}" -StartupType Automatic

- `{EXE PATH}`: Path to the app's folder on the host (for example, d:\myservice). Don't include the app's executable in the path. A trailing slash isn't required.
- `{DOMAIN OR COMPUTER NAME\USER}`: Service user account (for example, Contoso\ServiceUser).
- `{SERVICE NAME}`: Service name (for example, MyService).
- `{EXE FILE PATH}`: The app's executable path (for example, d:\myservice\myservice.exe). Include the executable's file name with extension.
- `{DESCRIPTION}`: Service description (for example, My sample service).
- `{DISPLAY NAME}`: Service display name (for example, My Service).

# Start a service
```
Start-Service -Name {SERVICE NAME}
```

# Determine a service's status
```
Get-Service -Name {SERVICE NAME}
```

The status is reported as one of the following values:

- `Starting`
- `Running`
- `Stopping`
- `Stopped`

# Stop a service
```
Stop-Service -Name {SERVICE NAME}
```

# Remove a service
```
Remove-Service -Name {SERVICE NAME}
```
