# Use this to bootstrap your FitNesse directory.

mkdir -p runner
cp ../packages/NetRunner.1.0.11/tools/net45/NetRunner.Executable.exe ./runner/
chmod 700 ./runner/NetRunner.Executable.exe

cp ../packages/NetRunner.1.0.11/lib/portable-net4+sl4+wp7+win8/* ./runner/
