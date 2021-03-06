
# Space Engineers: Ship Systems Manager
A Space Engineers script to automate block states, such as doors, lights and sound blocks based on certain conditions (such as decompression or enemies detected).

The script takes a snapshot of the state of all blocks which it has control over before the first time it edits them, allowing it to restore the blocks to default state when all states are cleared.

The script is completely automated, staggering it's execution over approximately every 10 frames.

This means that it can manage large grids at an acceptable latancy with very little impact on performance. On average a hull breach will be detected and sealed in under two seconds, including the time it takes for doors to shut.

## Special Thanks
Big shout out to [Malware](https://github.com/malware-dev) for his [Space Engineers Visual Studio Developers Kit](https://github.com/malware-dev/MDK-SE).

Without this I'd have never had the patience to go though with such an ambitious project!

# Work in Progress
Note that this script is a work in progress and might be buggy.

# Testing

You can test out some of the features of the mod using this [workshop item](https://steamcommunity.com/sharedfiles/filedetails/?id=1541443254).

# Wiki

More detailed information on setting up and customizing the script are available in the Wiki.
