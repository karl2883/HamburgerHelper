local entity = {}

entity.name = "HamburgerHelper/CursorPickup"
entity.justification = {0.5, 0.5}

entity.placements = {
    {
        name = "cursor_pickup",
        data = {
            respawnTime = 3,
            cursorTime = 2,
            cursorSpeed = 3,
            windowTexture = "objects/hamburger/cursorpickup/window",
        }
    }
}

entity.texture = "objects/hamburger/cursorpickup/mouse"

return entity