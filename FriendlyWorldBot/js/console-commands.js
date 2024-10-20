global.showJobs = function (show = true){
    // used in RoomManager.TickCreep
    if (!Memory.config) Memory.config = {}
    Memory.config.showJobs = show
    return true;
}

global.showExtensions = function (show = true){
    // used in StructureManager.Tick
    if (!Memory.config) Memory.config = {}
    Memory.config.showExtensions = show
    return true;
}

global.showPaths = function (show = true){
    // used in CreepExtensions.BetterMoveTo
    if (!Memory.config) Memory.config = {}
    Memory.config.showPaths = show
    return true;
}