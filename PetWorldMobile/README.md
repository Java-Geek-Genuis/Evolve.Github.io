# PetWorld Mobile

An original virtual-pet adventure for the Windows Phone / Windows 10 Mobile era.

Current features include the virtual pet, offline save data, evolution, pet sharing/visits, and a touch-controlled 7x7 meadow adventure with pickups, movement, treasure events, and rest.

The pet's position is saved, so leaving and reopening the adventure keeps its last location.

Build with Visual Studio 2022 plus UWP development support. Select Release | ARM for the phone build.

GitHub Actions runs gameplay self-tests and builds an ARM UWP APPX on Windows runners with the UWP tools installed by the workflow.

The phone itself remains the final runtime test because Windows 10 Mobile hardware and OS builds vary.
