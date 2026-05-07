local trigger = {}

trigger.name = "HamburgerHelper/ChangeStateTrigger"
trigger.placements = {
    name = "change_state_trigger",
    data = {
        width = 16,
        height = 16,
        state = 0,
        triggerOnlyOnce = true,
        onlyOnce = false,
    }
}

local playerStates = {
    {"StNormal", 0},
    {"StClimb", 1},
    {"StDash", 2},
    {"StSwim", 3},
    {"StBoost", 4},
    {"StRedDash", 5},
    {"StHitSquash", 6},
    {"StLaunch", 7},
    {"StPickup", 8},
    {"StDreamDash", 9},
    {"StSummitLaunch", 10},
    {"StDummy", 11},
    {"StIntroWalk", 12},
    {"StIntroJump", 13},
    {"StIntroRespawn", 14},
    {"StIntroWakeUp", 15},
    {"StBirdDashTutorial", 16},
    {"StFrozen", 17},
    {"StReflectionFall", 18},
    {"StStarFly", 19},
    {"StTempleFall", 20},
    {"StCassetteFly", 21},
    {"StAttract", 22},
    {"StIntroMoonJump", 23},
    {"StFlingBird", 24},
    {"StIntroThinkForABit", 25},
}

trigger.fieldInformation = {
    state = {
        fieldType = "integer",
        options = playerStates,
        editable = false,
    }
}

return trigger