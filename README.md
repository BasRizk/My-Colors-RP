# My-Colors
An adaptive Colors Quiz based educative (game) system, that lets people of any age learn about colors through the mapping of each color with the color’s name in English. Frequently through the run-time, the system changes the next colors to learn set based on the players’ affections (brain signals). The research aims to test a hypothesis of higher user performance by making use of the correlation between colors and brain signals to apply some mechanism to generate colors through the runtime that reflects a balance of load on the brain.

The Project at the end consists of three major parts:

        1- Unity Engine made interface for the view of the game “System”.
        2- Python 3 scripts to handle backend manipulations and data analysis and creations.
        3- Python 2 scripts “CyKITv2”, working as a server that tracks signals received from Emotiv Epoc+.


# APIs Used

*-Color-Names
    
    https://github.com/meodai/color-names
    
*-CyKITv2 "with very few changes"
    
    https://github.com/CymatiCorp/CyKITv2/tree/master/Python
    
    
# Notes:

-- **Colors-Names** may or may not be set to work online, two base urls (Online & Offline) can be found in **ColorsRequest** Module in "**Assets/StreamingAssets/Backend/ColorsRequests.py**".
