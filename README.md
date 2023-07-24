# Cloudsumé Services

This is a collection of services for Cloudsumé to run untrusted code. Eventhough it is written in C# but there are some parts that are written in C++, which is depend on Linux. That mean it cannot be run on Windows.

## Building from source

### Prerequisites

- .NET 6 SDK
- CMake
- GCC with C++
- Poppler

### Configure Native Library

```sh
cmake -D CMAKE_BUILD_TYPE=Release -S cpp -B cpp/build
```

### Build Native Library

```sh
cmake --build cpp/build && cmake --install cpp/build --prefix dotnet/Cloudsume.Native
```

### Build Cloudsumé Services

If the target machine has .NET runtime installed, run:

```sh
dotnet publish -c Release -o dist dotnet/Cloudsume.Services
```

Otherwise run:

```sh
dotnet publish -c Release -o dist -r linux-x64 dotnet/Cloudsume.Services
```

`dist` directory will contains the output binary. Please note that the output binary should be running on the same distro version as the builder.

## Runtime dependencies

- Poppler
- XeTeX

## Development

### Prerequisites

- [Visual Studio Code](https://wiki.archlinux.org/title/Visual_Studio_Code)
  - [C# support](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
  - [CMake](https://marketplace.visualstudio.com/items?itemName=twxs.cmake)
  - [CMake Tools](https://marketplace.visualstudio.com/items?itemName=ms-vscode.cmake-tools)
  - [C/C++](https://marketplace.visualstudio.com/items?itemName=ms-vscode.cpptools)

### Open VS Code

Restore NuGet packages before open the VS Code:

```sh
dotnet restore dotnet/Cloudsume.sln
```

Once VS Code is opened change the CMake target on the status bar from `[all]` to `[install]` then click on `Build`.

## License

GNU AGPLv3
