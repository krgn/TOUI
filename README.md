# TOUI
Totally Optional User Interface Protocol

It is intended to expose parameters (values) from a host application to a client in a standardized way.

##TOI distinguishes between:
- __Value__: Number, String, Color, … This is the value without visual representation
- __Widget__ (optional): Button, Slider, … This is the visual representation of a Value. A Widget must be implemented client-side. The protocol defines standard widgets for basic types. Optionally complex types can be added when needed.
- __Layout__ (optional): Placement of widgets on a screen: The Layouting of Widgets defines how widgets are placed on screen by defining standard containers.
- __Style__ (optional): Look (colors, shading, ...) of widgets on a screen: CSS styling of widgets

The first draft version of TOUI only defines 1. (Values) and 2. (Widgets)

A client can freely choose defaults for optional properties that are not used.

##Contributors
eno, ingo, ingolf, joreg, karsten

##Status
working draft...
