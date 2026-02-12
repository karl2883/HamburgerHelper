local fakeTilesHelper = require("helpers.fake_tiles")

local controller = {}

controller.name = "HamburgerHelper/TilesetSoundSplitter"
controller.justification = {0.5, 0.5}

controller.placements = {
    {
        name = "tileset_sound_splitter",
        data = {
            tileType = "3",
            wallSoundIndex = 4,
            stepSoundIndex = 4,
            landSoundIndex = 4,
        }
    }
}

local soundIndexes = {
    {"None", 0},
    {"Asphalt", 1},
    {"Car", 2},
    {"Dirt", 3},
    {"Snow", 4},
    {"Wood", 5},
    {"Bridge", 6},
    {"Girder", 7},
    {"Brick", 8},
    {"Traffic Block", 9},

    {"Dreamblock Inactive", 11},
    {"Dreamblock Active", 12},

    {"Resort Wood", 13},
    {"Resort Roof", 14},
    {"Resort Platforms", 15},
    {"Resort Basement", 16},
    {"Resort Laundry", 17},
    {"Resort Boxes", 18},
    {"Resort Books", 19},
    {"Resort Forcefield", 20},
    {"Resort Clutterswitch", 21},
    {"Resort Elevator", 22},

    {"Cliffside Snow", 23},
    {"Cliffside Grass", 25},
    {"Cliffside Whiteblock", 27},

    {"Glass", 32},
    {"Grass", 33},

    {"Cassette Block", 35},

    {"Core Ice", 36},
    {"Core Rock", 37},

    {"Glitch", 40},
    {"Internet Cafe", 42},
    {"Cloud", 43},
    {"Moon", 44},
}

function controller.fieldInformation()
    return {
        tileType = {
            options = fakeTilesHelper.getTilesOptions("tilesFg"),
            editable = false,
        },
        wallSoundIndex = {
            fieldType = "integer",
            options = soundIndexes,
            editable = false,
        },
        stepSoundIndex = {
            fieldType = "integer",
            options = soundIndexes,
            editable = false,
        },
        landSoundIndex = {
            fieldType = "integer",
            options = soundIndexes,
            editable = false,
        }
    }
end

controller.fieldOrder = {
    "x", "y",
    "tileType",
    "wallSoundIndex", "stepSoundIndex", "landSoundIndex"
}

controller.texture = "loenn/hamburger/TilesetSoundSplitter"

return controller