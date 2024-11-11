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
mklink /J ...\screeps-in-csharp\FriendlyWorldBot\bin\Debug\net8.0\wasi-wasm\AppBundle\world ...AppData\Local\Screeps\scripts\...
```


# General Helpers

- Search query `(XXX|TODO|FIXME)` to see open tasks


# Publish Bot

To publish it in a way that can be copied to the Screeps folder:

```powershell
dotnet dotnet clean | dotnet publish -c Debug
OR
dotnet dotnet clean | dotnet publish -c Release
```

# To Dos

- `Builder` to dos
- undertaker should put into own storage first
- create different containers with different spawn positions

# Public API

The public API can be found in _[FriendlyWorldBot/js/](./FriendlyWorldBot/js/)_

| Command                  | |                                  |
|--------------------------|-|----------------------------------|
| `showJobs(show = true)` | | <img src="readme/show-jobs.png"> |
| `showExtensions(show = true)` | | ![text](./readme/show-jobs.png)  |
| `showPaths(show = true)` | |                                  |
| `showStructureInfo(show = "")` | |                                  |



# Links

- [Screeps: World](https://store.steampowered.com/app/464350/Screeps_World/)
- [Screeps API](https://docs.screeps.com/api/)
- [thomasfn's Screeps DotNet](https://github.com/thomasfn/ScreepsDotNet)
- [WASI SDK](https://github.com/WebAssembly/wasi-sdk?tab=readme-ov-file)
- [Screeps Wiki](https://screeps.fandom.com/wiki/Screeps_Wiki)
