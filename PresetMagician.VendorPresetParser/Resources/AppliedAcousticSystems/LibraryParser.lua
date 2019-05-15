function tableHasKey(table, key)
    return table[key] ~= nil
end

function loadLibrary (data, bankSource, bankName, bankPath)
    local serpent = require("serpent")
    ---@param presets table
    local ok, presets = serpent.load(data)

    local parsedPresets = {};
    local lastSchema = {};

    for k, value in pairs(presets) do
        local parsedPreset = {};

        parsedPreset["bankSource"] = bankSource

        if (tableHasKey(value, "schema")) then
            lastSchema = value["schema"]
            schema = value["schema"]
        else
            schema = lastSchema
        end

        parsedPreset["index"] = k
        parsedPreset["bankName"] = bankName
        parsedPreset["bankPath"] = bankPath
        parsedPreset["programName"] = value["name"]
        parsedPreset["lastUse"] = os.time(os.date("!*t"))
        parsedPreset["mdate"] = value["mdate"]
        parsedPreset["parameters"] = { version = value["version"], schema = schema, values = value["values"] }
        parsedPreset["loadedDomains"] = {}
        parsedPreset["version"] = 1

        parsedPresets[k] = {}
        parsedPresets[k]["presetData"] = serpent.block(parsedPreset)
        parsedPresets[k]["presetName"] = value["name"]

        if (tableHasKey(value, "metadata")) then
            local ok2, metaData = serpent.load(value["metadata"])
            parsedPresets[k]["metaData"] = metaData
            parsedPresets[k]["rawMetaData"] = value["metadata"]
        end

    end

    return parsedPresets
end
