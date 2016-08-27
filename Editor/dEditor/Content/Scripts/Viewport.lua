--[[
    Viewport.lua - dEditor
    Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
 
    This program  is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    You should have received a copy of the GNU General Public
    along with this program. If not, see <http://www.gnu.org/licenses/>.
--]]
local ViewportViewModel = unpack(args)
print(args, ViewportViewModel)

-- Services
local CoreGui = core.CoreGui;
local SocialService = game:GetService("SocialService")

-- Variables
local Gui
local TopLeftStack

-- Helper
local UIHelper = {}
UIHelper.ButtonType = {Text=0, Image=1}
UIHelper.ButtonColour = Colour.new(0, 0, 0, 0.3)
UIHelper.ButtonHoverColour = Colour.new(0, 0, 0, 0.4)
UIHelper.ButtonClickColour = Colour.new(0, 0, 0, 0.5)

function UIHelper:MakeButton()
	local btn = Instance.new("Frame")
	btn.Size = UDim2.new(0, 32, 0, 32)
	btn.BackgroundColour = UIHelper.ButtonColour
	btn.BorderColour = Colour.Transparent

	local t = function()
		btn.BackgroundColour = UIHelper.ButtonHoverColour
	end

	print(type(t))

	btn.MouseEnter:connect(t)

	btn.MouseLeave:connect(function()
		btn.BackgroundColour = UIHelper.ButtonColour
	end)

	btn.MouseButton1Down:connect(function()
		btn.BackgroundColour = UIHelper.ButtonClickColour
	end)

	btn.MouseButton1Up:connect(function()
		btn.BackgroundColour = btn.IsMouseOver and UIHelper.ButtonHoverColour or UIHelper.ButtonColour
	end)

	return btn
end

function UIHelper:MakeTextButton(text)
	local btn = UIHelper:MakeButton()
	local label = Instance.new("TextLabel", btn)
	label.Name = "TextButton"
	label.Size = UDim2.new(1, 0, 1, 0)
	label.BackgroundColour = Colour.Transparent
	label.BorderColour = Colour.Transparent
	label.TextColour = Colour.White
	label.Text = text
	return btn
end

function UIHelper:MakeImageButton(image, imageSize)
	local btn = UIHelper:MakeButton()
	local label = Instance.new("ImageLabel", btn)
	label.Name = "ImageButton"
	label.ImageId = image
	label.Size = UDim2.new(0, imageSize, 0, imageSize)
	local pos = imageSize * (imageSize/btn.AbsoluteSize.X)
	label.Position = UDim2.new(0, pos, 0, pos)
	label.BackgroundColour = Colour.Transparent
	label.BorderColour = Colour.Transparent
	return btn
end

function UIHelper:MakeStack(direction)
	local stack = Instance.new("Stack")
	stack.BackgroundColour = UIHelper.ButtonColour
	stack.BorderColour = Colour.Transparent
	stack.Orientation = direction
	stack.Offset = Vector2.new(4, 0)
	stack.Size = UDim2(0, 0, 0, 32)
	return stack
end

function UIHelper:MakeAvatarButton(userId)
	local button = UIHelper:MakeImageButton(string.format("avatar://%d/small/", userId), 32)
	button.Name = string.format("Avatar_%d", userId)
	return button
end

function MakeMyAvatar()
	local avatarButton = UIHelper:MakeAvatarButton(SocialService:GetUserId())
	avatarButton.Parent = BottomLeftStack
	avatarButton.FrameIndex = 1
	print(avatarButton:GetFullName())
end

function Init()
	Gui = Instance.new("ScreenGui", CoreGui)
	Gui.Name = "ViewportGui"
	
	TopLeftStack = UIHelper:MakeStack(Enum.FlowDirection.Horizontal)
	TopLeftStack.Name= "TopLeft"
	TopLeftStack.Position = UDim2.new(0, 4, 0, 4)
	TopLeftStack.Orientation = Enum.FlowDirection.Horizontal
	TopLeftStack.AlignmentY = Enum.AlignmentY.Top
	TopLeftStack.Parent = Gui

	local sceneButton = UIHelper:MakeImageButton("editor://Icons/menu.png", 16)
	sceneButton.Parent = TopLeftStack
	sceneButton.MouseButton1Up:connect(function()
		clr.dEditor.Editor.Current.Shell.ShowProjectProperties()
	end)

	if (LoginService:IsLoggedIn()) then
		MakeMyAvatar()
	else
		LoginService.LoginSucceeded:connect(MakeMyAvatar)
	end
end

Init()
ViewportViewModel.ViewportGui = Gui