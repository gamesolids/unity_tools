# Unity Tools

A collection of reusable Unity Editor tools for project workflows and inspector productivity.

## Overview

This repository contains editor-focused utilities under the `GameSolids` menu hierarchy.

## Usage

After importing the tools into a Unity project, menu items and context actions are available under:

- `GameSolids`

## Included Tools

### InspectorHistory
Allows forward/backward navigation of selected objects. Fits nicely above or below the inspector window.


### FindMaterialSubmeshMismatches
A utility script that scans structure of imported models and identifies any where there are more materials than submeshes. 

Unity will trigger a warning about the mismatch, but convieniently doesn't say which submesh is causing performance issues.


### ConvertMaterialToUrpComplexLit
A tool to help convert materials to URP Complex Lit shader. More useful several years ago.


### AdditionalShortcuts
Editor shortcuts to clear scale `[Alt+S]` or rotation `[Alt+R]` of an object. 

Also a nice template to add more.


## Notes

These tools are intended for use inside the Unity Editor and are not included in runtime builds.