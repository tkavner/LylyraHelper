-- Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
-- See the LICENCE file in the repository root for full licence text.

local colors = require("consts.xna_colors")

local function makeOptions(options, defaults, ...)
    local requested = {...}
    if #requested == 0 then
        requested = defaults
    end
    local tbl = {}
    for _,v in ipairs(requested) do
        table.insert(tbl, {options[v], v})
    end
    return tbl
end

local consts = {
    modVersion = "1.3.0",
    ignoredFields = {
        "modVersion",
        "pluginVersion",
        "_name",
        "_id",
    }
}


return consts