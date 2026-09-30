# PetWorld Mobile

PetWorld Mobile is an original virtual-pet adventure designed for the Windows Phone / Windows 10 Mobile era.

## Version 0.1

The current branch contains a real UWP ARM project with:

- offline pet save data
- hunger, energy, happiness and bond
- curiosity, strength and speed
- feeding, playing, training, sleeping and exploration
- leveling and automatic evolution
- animated vector pet drawn entirely in XAML code
- shareable, checksummed pet link codes
- an ARM packaging configuration

The project deliberately has no server dependency for the core game.

## Build

Open PetWorldMobile.sln in Visual Studio 2022 with the UWP development tools and a Windows 10 SDK installed.

Select Release | ARM and build the solution.

## GitHub Actions

The workflow runs the platform-independent pet tests and attempts an ARM UWP package build on a Windows 2022 runner. Successful APPX files are uploaded as workflow artifacts.

## Roadmap

The next build will add:

1. import another player's pet code
2. pet visits and friendship records
3. a touch-controlled overworld
4. collectibles and home decoration
5. more species and branching evolution
6. direct local/network pet exchange where supported by the phone

## Note

Windows 10 Mobile is retired, and exact device compatibility depends on the phone's installed OS build. The real phone remains the final installation/runtime test.
