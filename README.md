PresetMagician is an application to automatically create NKS Presets from your non-NKS compatible VST plugins

**Note that PresetMagician is currently not maintained, see https://github.com/PresetMagician/PresetMagician/issues/1**

# Building from source

- Install Visual Studio Community Edition with the C++ and C# Desktop Development workloads
- Clone repository including submodules
- Add `aeffect.h` and `aeffectx.h` from your VST 2.x SDK to the `common/` directory
- Run `.paket/paket.exe install` to install dependencies
- Open the solution in Visual Studio or Jetbrains Rider and build


[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgitlab.com%2FDrachenkatze%2Fpresetmagician.svg?type=large)](https://app.fossa.io/projects/git%2Bgitlab.com%2FDrachenkatze%2Fpresetmagician?ref=badge_large)