# Use this to bootstrap your FitNesse directory.

nuget install netrunner -o ./ -ExcludeVersion

mkdir -p runner
cp ./NetRunner/tools/net45/NetRunner.Executable.exe ./runner/
chmod 700 ./runner/NetRunner.Executable.exe

cp ./NetRunner/lib/portable-net4+sl4+wp7+win8/* ./runner/
