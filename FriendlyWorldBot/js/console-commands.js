global.showJobs = function (show = true){
    // used in RoomManager.TickCreep
    if (!Memory.config) Memory.config = {}
    Memory.config.showJobs = show
    return true;
}
