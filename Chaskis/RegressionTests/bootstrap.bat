nuget install netrunner -ExcludeVersion

if not exist "runner" mkdir "runner"
COPY .\NetRunner\tools\net45\NetRunner.Executable.exe .\runner\
COPY ".\NetRunner\lib\portable-net4+sl4+wp7+win8\*" .\runner\
