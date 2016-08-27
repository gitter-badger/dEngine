Data types are a category of class in dEngine. They are usually immutable structs.

# Native Types
These are the standard types which are provided by either Lua or the CLR.

| Type                          | Description                                                    |
|-------------------------------|----------------------------------------------------------------|
| [Boolean](?title=bool)        | A value which is either true or false.                         |
| [Coroutine](?title=coroutine) | Thread-like object for executing code at the same time.        |
| [Function](?title=function)   | Represents a block of Lua code.                                |
| [Nil](?title=void)            | Represents nothing.                                            |
| [Double](?title=double)       | A 64-bit floating point value. The default number type in Lua. |
| [Single](?title=float)        | A 32-bit floating point value.                                 |
| [Integer](?title=int)         | A 32-bit integral value.                                       |
| [Long](?title=long)           | A 64-bit integer.                                              |
| [Short](?title=short)         | A 16-bit integer.                                              |
| [Byte](?title=byte)           | An 8-bit integer.                                              |
| [String](?title=string)       | A sequence of characters.                                      |
| [Table](?title=table)         | A key-value collection.                                        |
| [Userdata](?title=userdata)   | An interface to a dEngine class.                               |

# dEngine Types
These are types provided by dEngine.

| Type               | Description                                                            |
|--------------------|------------------------------------------------------------------------|
| Axes               | Represents a selection of X, Y, and/or Z.                              |
| Colour             | A 4-component colour value.                                            |
| CFrame             | Represents a position and orientation in 3D space.                     |
| ColourSequence     | A sequence of Colours.                                                 |
| Content            | Represents a link to an asset. (A texture or sound file, for example.) |
| Enum               | An enum is a list of constants.                                        |
| Faces              | A combination of normal IDs.                                           |
| NumberRange        | A minimum and maximum value.                                           |
| NumberSequence     | A sequence of numbers.                                                 |
| PhysicalProperties | A set of properties that control how a Part interacts.                 |
| Ray                | Represents a point and direction in 3D space.                          |
| Region3            | Represents a box in 3D space.                                          |
| Region3int16       | Represents a box in 3D space defined with 16-bit integral values.      |
| UDim2              | A screen-relative scale and pixel absolute value.                      |
| Vector2            | A 2-dimensional floating point vector.                                 |
| Vector3            | A 3-dimensional floating point vector.                                 |
| Vector4            | A 4-dimensional floating point vector.                                 |
| Vector3int16       | A 3-dimensional 16-bit integral vector.                                |
| DateTime           | Represents a date and time.                                            |
| TimeSpan           | Represents a time span.                                                |

