# Difflytic

Difflytic creates and applies binary diffs, supporting multiple files and streaming diff access. This is ideal for scenarios where large files are split into smaller components, like audio, video and subtitle files extracted from a MKV container. Difflytic's diff format also enables software to stream files directly from the diff file without needing to extract the entire file first.

## Installation

### Downloading Releases

You can download pre-built binary releases from the [Releases](https://github.com/Deathspike/Difflytic/releases) page.

### Building from Source

Alternatively, you can build from source. You'll need the .NET SDK installed.

## Usage

Depending on the operating system and architecture, the binary may be named differently.

### Creating a Diff

To create a diff file from an old file and multiple new files:

    difflytic <oldPath> <newPaths...> <diffPath>

* `oldPath`: Path to the old file.
* `newPaths`: Paths to the new files (one or more).
* `diffPath`: Path where the diff file will be saved.

### Extracting a Diff

To extract the new files from a diff file:

    difflytic <oldPath> <diffPath>

* `oldPath`: Path to the old file.
* `diffPath`: Path to the diff file.

The new files will be saved next to the diff file.

## Contributions

We welcome contributions from the community! If you have suggestions for improvements, bug fixes, or new features, please feel free to submit a pull request or open an issue on our GitHub page. Your feedback and contributions make Difflytic better for everyone.
