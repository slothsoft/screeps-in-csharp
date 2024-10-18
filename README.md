
https://store.steampowered.com/app/464350/Screeps_World/
https://screeps.fandom.com/wiki/Screeps_Wiki
https://github.com/thomasfn/ScreepsDotNet/tree/main

dotnet workload install wasi-experimental
dotnet restore
dotnet publish -c Debug
dotnet publish -c Release

https://github.com/WebAssembly/wasi-sdk?tab=readme-ov-file

\ScreepsDotNet.ExampleWorldBot\bin\Debug\net8.0\wasi-wasm\AppBundle\world

C:\Users\Steffi\AppData\Local\Screeps\scripts\old_bloodmoon_network_de___21025