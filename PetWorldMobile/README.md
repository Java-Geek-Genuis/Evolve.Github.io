# PetWorld Mobile

An original virtual-pet adventure for the Windows Phone / Windows 10 Mobile era.

## Current version

The petworld-mobile-v1 branch contains:

- ARM UWP project and Visual Studio solution
- local offline save data
- hunger, energy, happiness, bond and curiosity
- strength, speed, XP, coins, leveling and evolution
- feed, play, train, sleep and explore actions
- touch-friendly animated vector pet
- exportable pet link codes with checksum validation
- importing another player's code for a friend visit
- GitHub Actions self-tests
- GitHub Actions UWP ARM build and package signing pipeline

## Sharing pets

Open PET LINK / VISIT. Copy your code and send it to another player. Paste a friend's code to make a visit. The visit boosts happiness and bond without replacing your own pet.

## Building

Open PetWorldMobile/PetWorldMobile.sln with Visual Studio 2022 plus UWP development support. Select Release | ARM.

GitHub Actions installs the UWP build component, runs the core self-tests, builds an ARM APPX, creates a temporary test certificate, signs the APPX, verifies the signature, and uploads the package and certificate.

The phone itself remains the final runtime test because Windows 10 Mobile hardware and OS builds vary.

## Next gameplay work

The next feature pass will turn the current pet screen into a small explorable world with touch movement, collectible items, home decoration, NPC/friend encounters and additional pet species.
