# Concurrent.FastReflection.NetStandard

A fast, concurrently safe NetStandard 2.0 Reflection.Emit based dynamic invocation
for parametric and paremeterless constructors, methods, properties and fields with
caching support for already created delegates.

Concurrent performancee could be still improved upon, but conccurent safety was achieved!

This is work based on fork of github project of author Vexe:  https://github.com/vexe/Fast.Reflection

I was interested to make this code concurrently safe and compliant to NetStandard 2.0 and accessible from nuget.org

# Fast.Reflection
Extension methods library emitting IL to generate delegates for fast reflection

View Unity Forum thread for readme/doc and see Sample Unity scene for benchmarks/examples
http://forum.unity3d.com/threads/open-source-fast-reflection-delegates-for-lightning-fast-metadata-reflection.305080/
