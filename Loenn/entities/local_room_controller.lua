local controller = {}

controller.name = "HamburgerHelper/LocalRoomController"
controller.justification = {0.5, 0.5}

controller.placements = {
    {
        name = "local_room_controller",
        data = {
            flags = "",
            rooms = "",
        }
    }
}

controller.fieldInformation = {
    flags = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        },
    },
    rooms = {
        fieldType = "list",
        elementOptions = {
            fieldType = "string",
        },
    }
}

controller.texture = "loenn/hamburger/localRoomController"

return controller