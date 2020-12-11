---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by shenyi.
--- DateTime: 2020/3/20 15:43
local base = require "Framework.Scene.BaseScene"
local MainScene = BaseClass("MainScene", base)

function MainScene:OnCreate()

end

function MainScene:OnPrepare(map_id)
    coroutine.Do(MapManager.LoadMap, nil, "Demo_1")
    UIManager:LoadView(UIConfig.MainUI)
    --UIManager:LoadView(UIConfig.BattleUI)
    AOIController:EnterMap()
end

function MainScene:OnEnter()


end

function MainScene:OnLeave()
    MapManager.Cleanup()
    EntityBehaviorManager.Cleanup()
end

return MainScene