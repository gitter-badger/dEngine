// ReSharper disable All

var api;
var allClasses = {};
var namespaces = {};
var rootClass;

var DEPTH_MARGIN = 12;
var depth = 0;

var icons = {};
var pageData;

var mdPages = {};

var groups = {
    "Bricks": "As the essential building blocks of dEngine, these objects interact with each other physically. All classes here inherit from [Part](index.html?index=Part). These parts are all rendered in 3D if placed in the [Workspace](index.html?title=Workspace)",
    "Brick equipment": "These objects, when [parented](index.html?title=Instance/Parent) to a brick, will change the appearance or behavior of the brick.",
    "Body movers": "Similar to brick equipment, these objects work on the brick they are parented to. These objects alter the position, orientation, or movement of a brick. All classes here inherit from the [BodyMover](index.html=BodyMover) abstract class.",
    "Gameplay": "These objects are used in general gameplay of a place.",
    "GUI": "These objects display graphical elements in 2-dimensional space on the player's screen, in front of the 3D world.",
    "3D GUI" : "These objects display graphical elements in 3-dimensional space. While they don't interact physically with bricks, they are usually attached to bricks in some way.",
    "Scripting" : "These objects are involved in adding scripted functionality to a game.",
    "Values" : "These objects exist to hold specific value types. They are essential for communicating data between [Scripts](index.html?index=Script).",
    "Constraints": "These objects add physical constraints to bricks, depending on the type of constraint in use. They are held together with [Attachments](index.html?index=Attachment).",
    "Joints": "These objects bind bricks together, sometimes with an extra effect such as a hinge or motor. All classes here inherit from the [JointInstance](index.html?index=JointInstance) abstract class.",
    "Containers": "These objects are designed to contain other objects as children. For example, the [Workspace](index.html?index=Workspace) holds objects that are simulated as the game runs.",
    "Services": "These objects generally operate in the background, and may be helpful with place development. They're unique in that they may only have one instance of themselves, and can be acquired with the [GetService](index.html?title=DataModel/GetService) method.",
}

var friendlyTypeDictionary = {
    "System.Void": "void",
    "System.Single": "float",
    "System.Boolean": "bool",
    "System.String": "string",
    "System.Int32": "int",
    "System.UInt32": "uint",
    "System.Int16": "short",
    "System.Byte": "byte",
    "Neo.IronLua.LuaTable": "table",
}

marked.setOptions({
    highlight: function (code) {
        return hljs.highlightAuto(code).value;
    },
    tables: true,
});

var PageNav = React.createClass({
    render: function () {

        var memberElement = null;
        var subItems = [];

        if (pageData.pageType != "root") {
            subItems[0] = React.createElement(
                "li",
                null,
                React.createElement(
                    "a",
                    { href: "index.html?title=" + pageData.kind },
                    pageData.kind
                )
            );
            subItems[1] = React.createElement(
                "li",
                null,
                React.createElement(
                    "a",
                    { href: "index.html?title=" + pageData.pageName },
                    pageData.pageName
                )
            );
            subItems[2] = memberElement;
        }

        return React.createElement(
            "ol",
            { "className": "breadcrumbs" },
            React.createElement(
                "li",
                null,
                React.createElement(
                    "a",
                    { href: "index.html" },
                    "dEngine API"
                )
            ),
            subItems
        );
    }
})

var Title = React.createClass({
    render: function () {
        return React.createElement(
            "h1",
            { "className": "title" },
            React.createElement(
                "span",
                { "className": "kind" },
                pageData["kind"]
            ),
            React.createElement(
                "span",
                { "className": "classname" },
                pageData["pageName"]
            ),
            React.createElement(
                "span",
                { "className": "namespace" },
                pageData["fullName"]
            )
        );
    }
})

var ClassBox = React.createClass({

    render: function () {
        var obj = pageData.targetClass;
        var elements = [];

        function addBox(title, md) {
            elements.push(React.createElement(
                "div",
                {
                    "className": "box " + title.toLowerCase(),
                    dangerouslySetInnerHTML: { __html: "<span class='name'>" + title + ":</span> " + marked(md) }
                },
                null
            ));
        }

        if ("UncreatableAttribute" in obj.Attributes) {
            addBox("Uncreatable", "This object cannot be created with 'Instance.new()'");
        }

        if ("ObsoleteAttribute" in obj.Attributes) {
            addBox("Deprecated", "This item is deprecated. Do not use it for new work.");
        }

        return React.createElement(
            "section",
            { "className": "boxes" },
            elements
        );
    }
});

var SubNav = React.createClass({
    render: function () {
        if (pageData.pageType == "class") {
            return React.createElement(
                "ul",
                { className: "subnav" },
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#properties" },
                        "Properties"
                    )
                ),
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#functions" },
                        "Functions"
                    )
                ),
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#events" },
                        "Events"
                    )
                )
            );
        }
        else if (pageData.pageType == "datatype") {
            return React.createElement(
                "ul",
                { className: "subnav" },
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#properties" },
                        "Properties"
                    )
                ),
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#functions" },
                        "Functions"
                    )
                ),
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#operators" },
                        "Operators"
                    )
                ),
                React.createElement(
                    "li",
                    null,
                    React.createElement(
                        "a",
                        { href: "#references" },
                        "References"
                    )
                )
            );
        }
        else if (pageData.pageType == "root" || pageData.pageType == "page") {
            return React.createElement(
                "ul",
                { className: "subnav" });
        }
    }
});

var Description = React.createClass({
    render: function render() {
        var obj = pageData.targetClass;
        return React.createElement(
            "span",
            { className: "comment", dangerouslySetInnerHTML: { __html: marked(obj.Summary) + "<p>" + marked(obj.Remarks) + "</p>" } },
            null
           )
    }
});

var Relationships = React.createClass({
    render: function render() {
        var obj = pageData.targetClass;

        var inheritants = [];

        let current;
        let i = 0;
        current = obj;
        while (current != null) {
            inheritants.unshift(React.createElement(
                            "li",
                            null,
                            i == 0 ? current.Name :
                            React.createElement(
                                "a",
                                { href: "index.html?title=" + current.Name },
                                current.Name
                            ))
                        )
            current = namespaces[current.BaseClass];
            i++;
        }

        return React.createElement(
            "div",
            null,
            React.createElement(
                "dl",
                { "className": "dl-horizontal" },
                React.createElement(
                    "dt",
                    null,
                    "Inheritance:"
                ),
                React.createElement(
                    "dd",
                    null,
                    React.createElement(
                        "ul",
                        { "className": "breadcrumbs dark" },
                        inheritants
                    )
                )
            )
        );
    }
});

function getFriendlyTypeName(name) {
    var mappedVal = friendlyTypeDictionary[name];

    if (mappedVal != null)
        return mappedVal;
    else {
        var grave = name.indexOf("`");
        if (grave != -1) { // generic
            var openBrackets = name.indexOf("[[");
            var generic = name.substring(0, grave);
            generic = generic.substring(generic.lastIndexOf(".") + 1);
            return generic;
        }
        return name.substring(name.lastIndexOf(".") + 1);
    }
}

var Properties = React.createClass({
    render: function render() {
        let propertyElements = [React.createElement("h2", { id: "properties" }, "Properties"), ];

        function addProperty(prop, inherited) {
            var features = [];

            if (prop.DeclaringType == null)
                return;

            var friendlyPropertyType = getFriendlyTypeName(prop.PropertyType);

            if (prop.ReadOnly)
                features.push("read-only");
            if (inherited)
                features.push("inherited");

            var scriptSecurity = prop.Attributes["ScriptSecurityAttribute"];
            if (scriptSecurity != null)
                features.push(("ScriptSecurity (" + scriptSecurity.Value + ")").toLowerCase())

            var el = React.createElement(
                "dl",
                { "className": "property-summary-list" },
                React.createElement(
                    "dt",
                    { id: "first", "className": "property" },
                    React.createElement(
                        "span",
                        { "className": "signature" },
                        React.createElement(
                            "a",
                            { href: "index.html?title=" + friendlyPropertyType },
                            friendlyPropertyType
                        )
                    ),
                    React.createElement(
                        "span",
                        { "className": "name" },
                        React.createElement(
                            "a",
                            { href: "index.html?title=" + getFriendlyTypeName(prop.DeclaringType != null ? prop.DeclaringType : "object") + "/" + prop.Name },
                            prop.Name
                        )
                    ),
                    React.createElement(
                        "dd",
                        { "className": "inherited" },
                        React.createElement(
                            "p",
                            { dangerouslySetInnerHTML: { __html: marked(linkifySeeTags(prop.Summary)) } }
                        ),
                        React.createElement(
                            "div",
                            { "className": "features" },
                            features.join(", ")
                        )
                    )
                )
            );
            propertyElements.push(el);
        }

        let cur = pageData.targetClass;
        var inherited = false;
        for (; ;) {
            for (var i in cur.Properties) {
                var prop = cur.Properties[i];
                addProperty(prop, inherited);
            }
            inherited = true;
            cur = namespaces[cur.BaseClass];
            if (cur == null)
                break;
        }

        return React.createElement("section", { "className": "summary offset-anchor", "id": "instance-properties" },
            propertyElements);
    }
})

var Methods = React.createClass({
    render: function render() {
        let methodElements = [React.createElement("h2", { id: "functions" }, "Functions"), ];

        function addMethod(method, inherited) {
            var features = [];

            var functionParamaters = React.createClass({
                render: function render() {
                    var content = "";

                    for (var i in method.Parameters) {
                        var param = method.Parameters[i];
                        if (i != 0)
                            content += ", ";
                        var parameterType = getFriendlyTypeName(param.ParameterType);
                        content += "[" + parameterType + "]" + "(" + "index.html?title=" + parameterType + ") ";
                        content += param.Name;
                    }

                    return React.createElement("span",
                        { className: "params-inner", dangerouslySetInnerHTML: { __html: marked(content) } })
                }
            });

            var friendlyReturnType = getFriendlyTypeName(method.ReturnType != null ? method.ReturnType : "System.Void");
            if (inherited)
                features.push("inherited");

            var scriptSecurity = method.Attributes["ScriptSecurityAttribute"];
            if (scriptSecurity != null)
                features.push(("ScriptSecurity (" + scriptSecurity.Value + ")").toLowerCase())

            var el = React.createElement(
                "dl",
                { "className": "property-summary-list" },
                React.createElement(
                    "dt",
                    { id: "first", "className": "property" },
                    React.createElement(
                        "span",
                        { "className": "signature" },
                        React.createElement(
                            "a",
                            { href: "index.html?title=" + friendlyReturnType },
                            friendlyReturnType
                        )
                    ),
                    React.createElement(
                        "span",
                        { "className": "name" },
                        React.createElement(
                            "a",
                            { href: "index.html?title=" + getFriendlyTypeName(method.DeclaringType != null ? method.DeclaringType : "object") + "/" + method.Name },
                            method.Name
                        ),
                        React.createElement("span", { className: "params" }, "(", React.createElement(functionParamaters), ")")
                    ),
                    React.createElement(
                        "dd",
                        { "className": "inherited" },
                        React.createElement(
                            "p",
                            { dangerouslySetInnerHTML: { __html: marked(linkifySeeTags(method.Summary === null ? "No summary available" : method.Summary)) } }
                        ),
                        React.createElement(
                            "div",
                            { "className": "features" },
                            features.join(", ")
                        )
                    )
                )
            );
            methodElements.push(el);
        }

        let cur = pageData.targetClass;
        var inherited = false;
        for (; ;) {
            for (var i in cur.Functions) {
                var func = cur.Functions[i];
                addMethod(func, inherited);
            }
            inherited = true;
            cur = namespaces[cur.BaseClass];
            if (cur == null)
                break;
        }

        return React.createElement("section", { "className": "summary offset-anchor", "id": "instance-methods" },
            methodElements);
    }
})

var Events = React.createClass({
    render: function render() {
        let eventElements = [React.createElement("h2", { id: "events" }, "Events"), ];

        function addEvent(event, inherited) {
            var features = [];

            var eventParameters = React.createClass({
                render: function render() {
                    var content = "";

                    for (var i in event.Parameters) {
                        var param = event.Parameters[i];
                        if (i != 0)
                            content += ", ";
                        var parameterType = getFriendlyTypeName(param.ParameterType);
                        content += "[" + parameterType + "]" + "(" + "index.html?title=" + parameterType + ") ";
                        content += param.Name;
                    }

                    return React.createElement("span",
                        { className: "params-inner", dangerouslySetInnerHTML: { __html: marked(content) } })
                }
            });

            if (inherited)
                features.push("inherited");

            var el = React.createElement(
                "dl",
                { "className": "property-summary-list" },
                React.createElement(
                    "dt",
                    { id: "first", "className": "property" },
                    React.createElement(
                        "span",
                        { "className": "name" },
                        React.createElement(
                            "a",
                            { href: "index.html?title=" + getFriendlyTypeName(event.DeclaringType != null ? event.DeclaringType : "object") + "/" + event.Name },
                            event.Name
                        ),
                        React.createElement("span", { className: "params" }, "(", React.createElement(eventParameters), ")")
                    ),
                    React.createElement(
                        "dd",
                        { "className": "inherited" },
                        React.createElement(
                            "p",
                            { dangerouslySetInnerHTML: { __html: marked(linkifySeeTags(event.Summary === null ? "No summary available" : event.Summary)) } }
                        ),
                        React.createElement(
                            "div",
                            { "className": "features" },
                            features.join(", ")
                        )
                    )
                )
            );
            eventElements.push(el);
        }

        let cur = pageData.targetClass;
        var inherited = false;
        for (; ;) {
            for (var i in cur.Signals) {
                var event = cur.Signals[i];
                addEvent(event, inherited);
            }
            inherited = true;
            cur = namespaces[cur.BaseClass];
            if (cur == null)
                break;
        }

        return React.createElement("section", { "className": "summary offset-anchor", "id": "instance-events" },
            eventElements);
    }
})

var Members = React.createClass({
    render: function render() {
        var properties = React.createElement(Properties);
        var methods = React.createElement(Methods);
        var events = React.createElement(Events);

        return React.createElement("div", null, properties, methods, events);
    }
});

function getIconForClass(obj) {
    var icon = icons[obj.Name];
    if (icon == null) {
        var baseClass = namespaces[obj.BaseClass]
        if (baseClass != null) {
            return getIconForClass(baseClass);
        } else {
            return "default"
        }
    } else {
        return icon;
    }
}

function addSystemTypes() {
    // hack to add in system types
    api.DataTypes["string"] = {
        "Name": "string",
        "BaseClass": "",
        "Summary": "Strings are sequences of letters, numbers, and symbols.",
        "Remarks": mdPages["string"],
        "FullName": "System.String",
        "Attributes": {},
        "SubClasses": [],
        "Kind": "datatype",
        "Properties": [],
        "Functions": [],
        "Signals": [],
    };

    api.DataTypes["void"] = {
        "Name": "void",
        "BaseClass": "",
        "Summary": "The void type is returned by a function to indicate that the member returns no value.",
        "Remarks": mdPages["void"],
        "FullName": "System.Void",
        "Attributes": {},
        "SubClasses": [],
        "Kind": "datatype",
        "Properties": [],
        "Functions": [],
        "Signals": [],
    };

    api.DataTypes["int"] = {
        "Name": "int",
        "BaseClass": "",
        "Summary": "An integer is classified as any rational number that has no fractional component.",
        "Remarks": "",
        "FullName": mdPages["int"],
        "Attributes": {},
        "SubClasses": [],
        "Kind": "datatype",
        "Properties": [],
        "Functions": [],
        "Signals": [],
    };

    api.DataTypes["bool"] = {
        "Name": "bool",
        "BaseClass": "",
        "Summary": "A Boolean, or Bool for short, is a very simple data type. It is either a true or false value.",
        "Remarks": "",
        "FullName": mdPages["bool"],
        "Attributes": {},
        "SubClasses": [],
        "Kind": "datatype",
        "Properties": [],
        "Functions": [],
        "Signals": [],
    };

    api.DataTypes["float"] = {
        "Name": "float",
        "BaseClass": "",
        "Summary": "A float is a simple type that stores 32-bit floating-point values.",
        "Remarks": mdPages["float"],
        "FullName": "System.Single",
        "Attributes": {},
        "SubClasses": [],
        "Kind": "datatype",
        "Properties": [],
        "Functions": [],
        "Signals": [],
    };

    api.DataTypes["double"] = {
        "Name": "double",
        "BaseClass": "",
        "Summary":
            "A Number in Lua is a double precision floating point number (or just 'double'). Every Lua variable that is simply a number (not a Vector3, just a number) with or without decimal places, negative or positive, is a double.",
        "Remarks": mdPages["double"],
        "FullName": "System.Double",
        "Attributes": {},
        "SubClasses": [],
        "Kind": "datatype",
        "Properties": [],
        "Functions": [],
        "Signals": [],
    };
}

function addPages() {
    api.Pages = {
        "function-dump": {
            Name: "function-dump",
            Content: mdPages["function-dump"]
        },

    }
}

function loadClasses() {
    var classList = $("#object-list");
    var dataList = $("#datatype-list");

    function appendInstance(obj) {
        allClasses[obj.Name] = obj;
        namespaces[obj.FullName] = obj;
        obj.Icon = getIconForClass(obj);
        var iconUrl = "icons/" + obj.Icon + ".png"

        var deprecated = $(obj.Attributes).filter(function (i, o) { return o.Name == "ObsoleteAttribute" }).length != 0;
        var decoration = deprecated ? 'line-through' : 'none';

        classList.append($("<li style='text-decoration:" + decoration + "; margin-left: " + (depth * DEPTH_MARGIN).toString() + "px'><img src='" + iconUrl + "'  width='16' height='16' style='background:transparent; margin-right: 4px;'/><a href='" + "index.html?title=" + obj.Name + "'>" + obj.Name + "</a></li>"));
        depth += 1;
        obj.SubClasses.forEach(function (obj) {
            appendInstance(obj);
        });
        depth -= 1;
    }

    classList.empty();
    appendInstance(rootClass)

    addSystemTypes();
    addPages();

    dataList.empty();
    for (var key in api.DataTypes) {
        var obj = api.DataTypes[key];
        allClasses[obj.Name] = obj;
        namespaces[obj.FullName] = obj;
        dataList.append($("<li><a href='index.html?title=" + obj.Name + "'>" + obj.Name + "</a></li>"));
    };
}

function isStringNull(input) {
    if (typeof input === 'undefined' || input == null) return true;
    return false;
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function getNamesFromTitle(title) {
    var result = title;

    if (result == null)
        return [];

    var hash = title.indexOf("#");
    if (hash != -1)
        result = result.substring(0, hash);

    return result.split('/');
}

function getIndicesOf(searchStr, str, caseSensitive) {
    var startIndex = 0, searchStrLen = searchStr.length;
    var index, indices = [];
    if (!caseSensitive) {
        str = str.toLowerCase();
        searchStr = searchStr.toLowerCase();
    }
    while ((index = str.indexOf(searchStr, startIndex)) > -1) {
        indices.push(index);
        startIndex = index + searchStrLen;
    }
    return indices;
}

function linkifySeeTags(comment) {
    let output = comment != null ? comment : "";
    for (; ;) {
        var seeTag = output.indexOf("<see");

        if (seeTag == -1)
            break;

        var openQuote = output.indexOf("\"", seeTag);
        var closeQuote = output.indexOf("\"", openQuote + 1);
        var closeTag = output.indexOf("\>", closeQuote);
        var fullName = output.substring(openQuote + 3, closeQuote);
        var className = fullName.substring(fullName.lastIndexOf(".") + 1);
        output = output.substring(0, seeTag) + "[" + className + "](index.html?title=" + className + ")" + output.substring(closeTag + 1);
    }
    return output;
}

function parseApi(obj) {
    api = obj;
    rootClass = api.RootClass;
    loadClasses();
}

function parseIcons(str) {
    var lines = str.split("\n");

    for (var i = lines.length - 1; i >= 0; i--) {
        var l = lines[i];
        if (isStringNull(l))
            continue;
        var key = l.substring(0, l.indexOf(':'));
        var openApos = l.indexOf('\'');
        var closeApos = l.indexOf('\'', openApos + 1);
        var val = l.substring(openApos + 1, closeApos)
        icons[key] = val;
    }
}

$(".container.body").attr("style", "visibility: visible");

$("#sidenav-left-toggle").click(function () {
    $("#overlay-under-drawer").addClass("active");
    $(".sidebar-offcanvas-left").each(function (o, v) {
        $(v).addClass("active");
    });
})

$("#overlay-under-drawer").click(function () {
    $("#overlay-under-drawer").removeClass("active");
    $(".sidebar-offcanvas-left").each(function (o, v) {
        $(v).removeClass("active");
    });
})


$.ajax({
    dataType: "json",
    async: true,
    type: 'GET',
    url: 'dump.json',
    success: function (obj) {

        $.ajax({
            async: false,
            type: 'GET',
            url: 'IconDictionary.yml',
            success: parseIcons,
        });

        function getMarkdown(file) {
            var output;
            $.ajax({
                async: false,
                type: 'GET',
                url: "md/" + file + '.md',
                success: function (md) {
                    output = md;
                },
            });
            return output;
        }

        function getPage(file) {
            var output;
            console.log("pages/" + file + '/index.md');
            $.ajax({
                async: false,
                type: 'GET',
                url: "pages/" + file + '/index.md',
                success: function (md) {
                    output = md;
                },
            });
            return output;
        }

        mdPages["root"] = getMarkdown('root');
        mdPages["string"] = getMarkdown('string');
        mdPages["float"] = getMarkdown('float');
        mdPages["double"] = getMarkdown('double');

        parseApi(obj);

        var query = getUrlVars();

        var title = query["title"];
        var names = getNamesFromTitle(title);
        var className = names[0];
        var obj = allClasses[className];
        var icon = "icons/dEngine.png";
        var page = (title != null && obj == null) ? getPage(title) : null;

        if (obj == null && page == null) {
            if (title == null) {
                var groupMd = "";

                for (var groupName in groups) {
                    var groupDesc = groups[groupName];
                    groupMd += "\n"
                    groupMd += "# " + groupName + "\n";
                    groupMd += groupDesc + "  \n\n";
                    groupMd += "|   |   |   |\n|---|---|---|";
                    let k = 0;
                    for (var j in allClasses) {
                        var c = allClasses[j];
                        var attr = c.Attributes["ToolboxGroupAttribute"];
                        if (!c.IsAbstract && attr != null && attr.Value == groupName) {
                            if (k == 0) {
                                groupMd += "| ";
                            }
                            if (k % 3 == 0) {
                                groupMd += "  \n | ";
                            };
                            var iconUrl = "icons/" + getIconForClass(c) + ".png"
                            groupMd += "<img src='" + iconUrl + "' width='16' height='16' style='background:transparent; margin-right: 4px;'/> [" + c.Name + "](index.html?title=" + c.Name + ") | ";
                            k++;
                        }
                    }
                }

                pageData = {
                    pageType: "root",
                    pageName: "dEngine API Reference",
                    fullName: "",
                    kind: "",
                    targetClass: { Summary: mdPages["root"] + groupMd, Remarks: "" }
                }

                document.title = "dEngine API";
                document.description = "This page covers the API of dEngine.";
            } else {
                pageData = {
                    pageType: "root",
                    pageName: className,
                    fullName: "",
                    kind: "",
                    targetClass: { Summary: "There is currently no text in this page. Click [here](index.html) to return to the API reference page.", Remarks: "" }
                }

                document.title = className;
                document.description = "This page covers the API of dEngine.";
            }
        }
        else if (page != null) {
            var isRoot = names[1] == null;
            pageData = {
                pageType: "page",
                pageName: isRoot ? names[0] : names[1],
                fullName: "",
                kind: isRoot ? "" : className,
                targetClass: { Summary: page, Remarks: "" }
            }

            document.title = className + " - dEngine API",
            document.description = page;
        }
        else if (obj != null) {
            obj.Summary = obj.Summary != null ? linkifySeeTags(obj.Summary) : "No summary is available for this class.";
            obj.Remarks = obj.Remarks != null ? linkifySeeTags(obj.Remarks) : "";
            pageData = { pageType: obj.Kind, pageName: className, fullName: obj.FullName, kind: obj.Kind, targetClass: obj }

            document.title = className + " - dEngine API";
            document.description = obj.Summary;
            icon = "icons/" + obj.Icon + ".png";
        };

        $("link[rel*='icon']").attr("href", icon);

        ReactDOM.render(
            React.createElement(PageNav, null),
            $("#nav")[0]
        );

        ReactDOM.render(
            React.createElement(Title, null),
            $("#title-description")[0]
        );

        ReactDOM.render(
            React.createElement(SubNav, null),
            $("#subNav-container")[0]
        );

        ReactDOM.render(
            React.createElement(Description, null),
            $(".desc.markdown")[0]
        );

        if (pageData.pageType == "class" || pageData.pageType == "datatype") {
            ReactDOM.render(
                React.createElement(Relationships, null),
                $("#relationship-container")[0]
            );
            ReactDOM.render(
                React.createElement(Members, null),
                $("#member-container")[0]
            );



            ReactDOM.render(
                React.createElement(ClassBox, null),
                $("#box-container")[0]
            );
        }

    },
});
