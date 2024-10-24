# Screeps In C#

This is my bot for [Screeps: World](https://store.steampowered.com/app/464350/Screeps_World/) . It is based on [thomasfn's Screeps DotNet](https://github.com/thomasfn/ScreepsDotNet).

# Getting started

1. Clone this repository
2. Use this command to download WASI 
```shell 
dotnet workload install wasi-experimental 
```
3. Link publish folder to screeps
```shell 
dotnetmklink /J ...\screeps-in-csharp\FriendlyWorldBot\bin\Debug\net8.0\wasi-wasm\AppBundle\world ...AppData\Local\Screeps\scripts\...
```

# Publish Bot

To publish it in a way that can be copied to the Screeps folder:

```powershell
dotnet clean
dotnet publish -c Debug
OR
dotnet publish -c Release
```

# To Dos

- add screeps types to brand-new menu bar
- create log levels so that the log is not always sooo full of stuff
- make it possible to create stronger creeps

# Links

- [Screeps: World](https://store.steampowered.com/app/464350/Screeps_World/)
- [thomasfn's Screeps DotNet](https://github.com/thomasfn/ScreepsDotNet)
- [WASI SDK](https://github.com/WebAssembly/wasi-sdk?tab=readme-ov-file)
- [Screeps Wiki](https://screeps.fandom.com/wiki/Screeps_Wiki)
