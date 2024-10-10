# Difflytic

Difflytic creates and applies binary diffs, supporting multiple files and streaming diff access. This is ideal for scenarios where large files are split into smaller components, like audio, video and subtitle files extracted from a MKV container. Difflytic's diff format also enables software to stream files directly from the diff file without needing to extract the entire file first, thus ensuring efficient random access.

## CLI

### Installation

#### Downloading Releases

You can download pre-built binary releases from the [Releases](https://github.com/Deathspike/Difflytic/releases) page:

- `difflytic-linux`: _Linux (Any CPU)_ build. Requires `.NET 8`.
- `difflytic-linux-x64`: _Linux (x64)_ native build. No dependencies.
- `difflytic-macos`: _MacOS (Any CPU)_ build. Requires `.NET 8`.
- `difflytic-macos-arm64`: _MacOS (ARM64)_ native build. No dependencies.
- `difflytic-win`: _Windows (Any CPU)_ build. Requires `.NET 8`.
- `difflytic-win-x64`: _Windows (x64)_ native build. No dependencies.

#### Building from Source

Alternatively, you can build from source. You'll need the .NET SDK installed.

### Usage

Depending on the operating system and architecture, the binary may be named differently.

#### Creating a Diff

To create a diff file from an old file and one or more new files:

    difflytic-linux <oldPath> <newPaths...> <diffPath>

- `oldPath`: Path to the old file.
- `newPaths`: Path(s) to one or more new files.
- `diffPath`: Path where the diff file will be saved.

#### Extracting a Diff

To extract the new files from a diff file:

    difflytic-linux <oldPath> <diffPath>

- `oldPath`: Path to the old file.
- `diffPath`: Path to the diff file.

The new files will be saved next to the diff file.

## .NET

### Installation

    dotnet add Difflytic

### Usage

#### Creating a Diff

A block size is required to divide the old file into blocks. If you are unsure about the size, derive one:

```cs
var blockSize = BlockSize.GetSize(oldPath);
```

Now create a `Differ` using the default `MLCG` block hash:

```cs
var differ = new Differ(blockSize);
```

Then you can create the diff file:

```cs
differ.Diff(diffPath, newPaths, oldPath);
```

#### Extracting a Diff

Extracting a diff will use the block size and block hash used to create the diff:

```cs
Patcher.Patch(diffPath, oldPath, outputPath)
```

#### Streaming a Diff

Open a diff file for reading:

```cs
var reader = Reader.Create(diffPath, oldPath);
```

Now you can iterate over the new files in the diff file:

```cs
foreach (var file in reader) {}
```

Then you can open a new file as a stream:

```cs
using var stream = reader.Open(file);
```

## Contributions

We welcome contributions from the community! If you have suggestions for improvements, bug fixes, or new features, please feel free to submit a pull request or open an issue on our GitHub page. Your feedback and contributions make Difflytic better for everyone.
