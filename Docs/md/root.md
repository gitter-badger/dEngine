dEngine is an open source engine based on the ROBLOX API for standalone games.  
It is currently in early development: there are no binaries, but you can download the source code and build it.

<a class="btn primary" href="https://github.com/DanDevPC/dEngine/archive/master.zip"><span>Download ZIP</span></button>
<a class="btn" href="https://github.com/DanDevPC/dEngine"><span>Visit on Github<img src="http://findicons.com/files/icons/2779/simple_icons/24/github_24_black.png"/></span></a>
<p></p>

The recommended setup:
```plaintext
mkdir Engine
mkdir Editor
git clone -b notready https://www.github.com/DanDevPC/dEngine.git Engine
git clone https://www.github.com/DanDevPC/dEditor.git Editor


```

The following is documentation for dEngine classes.

There are two types of classes in dEngine: Objects and DataTypes.
All objects inherit from [Instance](index.html?title=Instance) and most of them can be created with the `Instance.new()` factory method. DataTypes are usually immutable structs and have their own constructors (e.g. `CFrame.new()`).

In addition to the functions and members listed here, dEngine exposes many standard and custom Lua functions. Please see the [Function dump](index.html?index=function-dump) for a complete list of all global functions and members.
