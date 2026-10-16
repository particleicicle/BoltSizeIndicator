# BoltSizeIndicator

A mod for My Summer/Winter Car that indicates with a color if a bolt is larger or smaller than the tool you're holding. 

This was a feature that ToplessGun was seemingly developing at some point but decided to scrap based on the names of events that are still present in the current iteration of the bolting system. These events are named RED and YELLOW, and they are sent when a bolt is larger or smaller than the tool in hand. Combined with disabled actions that would set the bolt material, this feature is pretty easy to bring back, with only one custom state needed to wait for the bolt to be unselected to set the material back to normal for MSC and older versions of MWC (later MWC versions remove one of the material setting states, but it can also be easily recreated).

## Dependencies

- [MSCLoader](https://github.com/piotrulos/MSCModLoader) — Mod loader for My Summer Car.
- [OASIS](https://github.com/Horsey4/OASIS) — Required for BoltSizeIndicatorOASIS.

## License

This project (**BoltSizeIndicator** and **BoltSizeIndicatorOASIS**) is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

MSCLoader and OASIS are also licensed under GPL-3.0.
