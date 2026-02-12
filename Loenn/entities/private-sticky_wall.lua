local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local sticky_wall = {}

sticky_wall.name = "HamburgerHelper/StickyWalls"
sticky_wall.depth = 1999
sticky_wall.canResize = {false, true}
sticky_wall.placements = {
    {
        name = "sticky_left",
        placementType = "rectangle",
        data = {
            height = 8,
            left = true,
            color = "ff0000",
            spritePath = "hamburger/stickywall"
        }
    },
    {
        name = "sticky_right",
        placementType = "rectangle",
        data = {
            height = 8,
            left = false,
            color = "ff0000",
            spritePath = "hamburger/stickywall"
        }
    }
}

sticky_wall.fieldInformation = {
    color = {
        fieldType = "color"
    }
}

local function getWallTextures(entity)
    return "objects/" .. entity.spritePath .. "/top", "objects/" .. entity.spritePath .. "/middle", "objects/" .. entity.spritePath .. "/bottom"
end

function sticky_wall.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local height = entity.height or 8
    local tileHeight = math.floor(height / 8)
    local offsetX = left and 0 or 8
    local scaleX = left and 1 or -1

    local topTexture, middleTexture, bottomTexture = getWallTextures(entity)

    for i = 2, tileHeight - 1 do
        local middleSprite = drawableSprite.fromTexture(middleTexture, entity)

        middleSprite:addPosition(offsetX, (i - 1) * 8)
        middleSprite:setScale(scaleX, 1)
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local topSprite = drawableSprite.fromTexture(topTexture, entity)
    local bottomSprite = drawableSprite.fromTexture(bottomTexture, entity)

    topSprite:addPosition(offsetX, 0)
    topSprite:setScale(scaleX, 1)
    topSprite:setJustification(0.0, 0.0)

    bottomSprite:addPosition(offsetX, (tileHeight - 1) * 8)
    bottomSprite:setScale(scaleX, 1)
    bottomSprite:setJustification(0.0, 0.0)

    table.insert(sprites, topSprite)
    table.insert(sprites, bottomSprite)

    return sprites
end

function sticky_wall.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height or 8)
end

function sticky_wall.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.left = not entity.left
    end
end

return sticky_wall