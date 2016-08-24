# Global Variables
These are built-in global variables that can be used without invoking a function.

## game
Shortcut to the [DataModel](index.html?title=DataModel).
## workspace
Shortcut to the [Workspace](index.html?title=Workspace).
## script
Shortcut to the [Script](index.html?title=Script) that is calling this code.
## plugin
Shortcut to the [Plugin](index.html?title=Plugin) this script is a part of. This function can only be called by editor plugins.
## stats
Shortcut to the [Stats](index.html?title=Stats).
## core
Shortcut to the [CoreEnvironment](index.html?title=CoreEnvironment). This function can only be called by scripts with a security level of `Clr`.


# Functions
These are built-in global functions. Most functions can be written in `PascalCase`, though `camelCase` is usually preferred.

## require
#### `require(module)`
Loads the given [ModuleScript](index.html?title=ModuleScript) and returns what the ModuleScript returned.
```lua
-- ModuleScript in ReplicatedStorage
print("ModuleScript loaded for the first time")
return "Hello, world!"
```
```lua
local module = game.ReplicatedStorage.ModuleScript
print(require(module))
print(require(module))
```
```plaintext
ModuleScript loaded for the first time
Hello, world!
Hello, world!
```

## Delay
#### `delay(t, f)` or `Delay(t, f)`
Executes function `f` after `t` seconds have elapsed. The function `f` is called with 2 arguments: The elapsed time and the current place time.
```lua
function returntime(timewaited, placetime)
    print("The delay time was " .. timewaited)
    print("The place time is " .. placetime)
end

delay(4, returntime)
```
```plaintext
The delay time was 4.0257607937525  
The place time is 61.028665867139
```

## Spawn
#### `spawn(f)` or `Spawn(f)`
Executes function `f` after thread next yields. The function `f` is called with 2 arguments: The elapsed time and the current place time.
```lua
function returntime(delay, placetime)
    print("The delay was " .. delay)
    print("The place time is " .. placetime)
end
 
spawn(returntime)
```
```plaintext
The delay was 0.038302775910779 
The place time is 61.028665867139
```
An alternative to this is coroutines. Spawned functions and coroutines aren't used in the same way. Spawn is used to delay the execution of a functions until the thread yields while coroutines are used to run code immediately on a separate thread. You can't send a Spawned function arguments unless you count this closure:
```lua
local a, b, c = 1
spawn(function() myfunc(a,b,c) end)
```

## ElapsedTime
#### `elapsedTime()` or `ElapsedTime()`
Returns how many seconds the program has been running for.
```lua
print(elapsedTime())
```
```plaintext
71.117055902735
```

## Tick
#### `tick()` or `Tick()`
Returns the local UNIX time, which is the number of seconds since the Unix epoch, January 1st, 1970.
<blockquote class="warning">
**Warning:** tick() returns local time, and time zones between servers may vary. A reliable option to get the same time on every server is to call [time()](#time).
</blockquote>

## Time
#### `time()` or `Time()`
Returns a [DateTime](index.html?title=DateTime) at the current UTC time.
```lua
local t = time()
print(type(t), t)
```
```plaintext
number 1471600630.23515
```

## Settings
#### `settings()` or `Settings()`
Returns the [global settings](index.html?title=GlobalSettings).
```lua
local s = Settings()
print(s:IsA("GenericSettings"))
print(s:IsA("GlobalSettings"))
```
```plaintext
true
true
```

## UserSettings
#### `userSettings()` or `UserSettings()`
Returns the [user settings](index.html?title=UserSettings).
```lua
local s = UserSettings()
print(s:IsA("GenericSettings"))
print(s:IsA("GlobalSettings"))
print(s:IsA("UserSettings"))
```
```plaintext
true
false
true
```

## DebuggerManager
#### `debuggerManager()` or `DebuggerManager()`
Returns the [DebuggerManager](index.html?title=DebuggerManager) when called by a [CoreScript](index.html?title=CoreScript) or from the editor.
```lua
local manager = DebuggerManager()
manager:AddDebugger(game.ServerScriptService.Script)
print(unpack(manager:GetDebuggers()))
```
```plaintext
ScriptDebugger
```

## Version
#### `version(meta = false)` or `Version(meta = false)`
Returns the version of the engine. `meta` determines whether additional info is included.
```lua
print(version())
print(version(true))
```
```plaintext
0.0.0.0
0.0.0.0-dev+sha.a9ccdf1
```

## wait
#### `wait(t)` or `Wait(t)`
Yields the current thread until `t` seconds have elapsed. If `t` is not specified, then it yields for a [very short period of time](index.html?title=LuaSettings/DefaultWaitTime). The function returns 2 values: The elapsed time and the current place time.
```lua
print(wait(1))
```
```plaintext
1.004991560393 566.78806415454
```

## warn
#### `warn(t)` or `Warn(t)`
An alternative version of [print](#print) that styles the text as a 'warning'.
```lua
warn("Example warning")
warn(123)
```
```warning
01:27:01.961 - Example warning
01:27:01.962 - 123
```

## printidentity
#### `printidentity` or `PrintIdentity()`
Prints the current [script security level](index.html?title=ScriptSecurity).
```lua
printidentity()
```
```plaintext
Current identity is 2 (Script)
```

## api
#### `api()`
Returns the API dump as a JSON string.
```lua
print(api())
```
```json
{
	"Name": "Animation",
	"Properties": [
		{
		"Name": "AnimationId",
		"PropertyType": "dEngine.Content`1[[dEngine.Data.AnimationData, dEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
		"Summary": "The content id for the animation data.",
		"Remarks": null,
		"ReadOnly": false,
		"DeclaringType": "dEngine.Instances.Animation",
		"Attributes": {}
		}
	],
	"Functions": [
		{
		"Name": "AdjustSpeed",
		"ReturnType": "System.Void",
		"DeclaringType": "dEngine.Instances.Animation",
		"Summary": null,
		"Remarks": null,
		"Parameters": [],
		"Attributes": {}
		}
	],
	"Signals": [],
	"BaseClass": "dEngine.Instances.Instance",
	"Summary": "An animation for a <see cref=\"T:dEngine.Instances.SkeletalMesh\" />.",
	"Remarks": null,
	"FullName": "dEngine.Instances.Animation",
	"Attributes": {
		"TypeIdAttribute": {
		"Name": "TypeIdAttribute",
		"Value": null
		},
		"ExplorerOrderAttribute": {
		"Name": "ExplorerOrderAttribute",
		"Value": null
		},
		"DefaultMemberAttribute": {
		"Name": "DefaultMemberAttribute",
		"Value": null
		}
	},
	"SubClasses": [],
	"Kind": "class"
}
```