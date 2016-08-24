# Making a string
The most common method of creating strings is to put double quotes around the characters you want. See the code below for an example:
```lua
local str = "Hello, world!"
```
This will cause str to contain the string `Hello, world`. However, what if you wanted to have double quotes within your string? If you have double quotes in a string, it will cause unwanted effects.
```lua
local str = "Hello, "Dave"!" -- We don't want this!
```
How can we fix this? There are other ways to create a string. We can use single quotes, or we can use double brackets. We could also escape the quotes.
```lua
local str0="Hello, world!"
local str1='Hello, "world"!'
local str2=[[Hello, "world"!]] 
local str3="Hello, \"world!\""
 
print(str0)
print(str1)
print(str2)
print(str3)
```
```plaintext
Hello, world!
Hello, "world"!
Hello, "world"!
Hello, "world"!`
```

# Combining strings
Let's say you wanted to combine two strings into one. This can be easily done by using ( .. ) in between the two strings.
```lua
local str0="Hello,"
local str1=" world!"
local str2=str0 .. str1
print(str0)
print(str1)
print(str2)
```

```plaintext
Hello,
world!
Hello, world!
The process of combining two strings into one is known as concatenation.
```

# Converting a string to a number
You can easily convert a string to a number by using the tonumber() function. This function takes one argument, which is a string, and will return the string into a number. The string must be a sequence of characters that resembles a number, such as "5128", "2", etc. Any strings that don't resemble numbers, such as "Hello", will return nil.

This is an example of tonumber() usage:
```lua 
a = "123"
b = 5 + tonumber(a) --tonumber() usage here
 
print(b) --128
```

# Math and strings
An important note with strings is that if you try to perform arithmetic on a string value, it will try to convert the string to a number. If your value can't be converted to a number, you will get an error.
```lua
print("5" + 1)
print ("whoops"+1)
```
```plaintext
6
attempt to perform arithmetic on a string value
```

In the first example, "5" was converted from a string to a number (notice "5" was in quotes, but 1 was not.) In the second example "whoops" could not be converted to a number, because it was a word.
```lua
print("50" == 50)           -- false, because a string is not equal to a number.
print(tostring(50) == "50") -- true, because you converted the number 50 to a string
print(tonumber("50") == 50) -- true, because you converted the string "50" to a number
print(50 .. "" == "50")     -- true, because you tacked on an empty string to the end of the number 50, converting 50 to a string.
```
### Advanced
This will also work with hexadecimal numbers:
```lua
print(0xf == 15) -- true, because 0xf is a hexadecimal number which equals 15
print(tonumber("0xf") == 15)   -- true, because you converted the string "0xf" to a number, 0xf
```
as well as with other based numbers, but you have to specify the base:
```lua
print(tonumber("00001111",2)) -- prints 15
print(tonumber("00001111",2)==15) -- prints true
 
print(tonumber("774",8)) -- prints 508
print(tonumber("774",8)==508) -- prints true
```

# Literals
```lua
print("hello")
print('hello')
print([[hello]])
```
Will all result in: hello

This allows you to nest a string within another string:
```lua
print('hello "Lua user"')
print("Its [[content]] hasn't got a substring.")
print([[Let's have more "strings" please.]])
```
```plaintext
hello "Lua user"
Its content hasn't got a substring.
Let's have more "strings" please.
```

# Multiline literals
```lua
print([[Multiple lines of text
can be enclosed in double square
brackets.]])
```
```plaintext
Multiple lines of text
 can be enclosed in double square
 brackets.
```

You can also use normal quotation marks with a backslash at the end of each line to create multiline strings:

```lua
local str = "line1\
line2\
line3"
print(str)
```
```plaintext
line1
line2
line3
```