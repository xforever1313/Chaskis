if not exist "runner" mkdir "runner"
COPY ..\packages\NetRunner.1.0.11\tools\net45\NetRunner.Executable.exe .\runner\
COPY "..\packages\NetRunner.1.0.11\lib\portable-net4+sl4+wp7+win8\*" .\runner\
