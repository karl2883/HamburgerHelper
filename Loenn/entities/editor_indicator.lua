local drawableNinePatch = require("structs.drawable_nine_patch")

local entity = {}

entity.name = "HamburgerHelper/EditorIndicator"
entity.warnBelowSize = {16, 16}

entity.placements = {
    {
        name = "editor_indicator",
        data = {
            width = 16,
            height = 16,
            color = "ffffff",
            filled = true,
        }
    }
}

entity.fieldInformation = {
    color = {
        fieldType = "color"
    }
}

local function ninepatchTexture(entity) 
    return entity.filled and "loenn/hamburger/editorbox" or "loenn/hamburger/editorboxOpen"
end

local ninepatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

function entity.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local ninepatchTexture = ninepatchTexture(entity)
    local boxSprite = drawableNinePatch.fromTexture(ninepatchTexture, ninepatchOptions, x, y, width, height)
    boxSprite:setColor(entity.color)

    return boxSprite
end

return entity