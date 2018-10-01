---
--- Created by L.
--- DateTime: 2018/10/1 9:51 PM
---

require 'built_in'

function set_box_mode(mode)
    __Nova.dialogueBoxesManager.Mode = mode
end

function new_page()
    __Nova.dialogueBoxesManager:NewPage()
end