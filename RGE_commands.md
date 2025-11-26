# RGE console commands

## Create
Creates a new entity in the scene with the specified parameters.
```r
create <world:number>0> <type:string> <pX:number> <pY:number> <pY:number>
```
**Example:**
```r
create 1 wedge -3487 64 1117
```

## Delete
Deletes an entity from the scene by its ID.
```r
delete <world:number>0> <id:number>
```
**Example:**
```r
delete 1 7d5c8396-28e4-4e0f-9f36-d35cebcb6c6f
```

## Move & Rotate
Moves and rotates an existing entity to the specified position and rotation.
```r
move <world:number>0> <id:number> <pX:number> <pY:number> <pZ:number> <rX:number> <rY:number> <rZ:number>
```
**Example:**
```r
move 1 9cf2f629-ab5c-452d-8156-bb7be6af0004 -3489.800048828125 64 1117 -0 0 0
```

# Scale
Scales an existing entity to the specified scale factors.
```r
scale <world:number>0> <id:number> <sX:number> <sY:number> <sZ:number>
```
**Example:**
```r
size 1 854d362a-6db0-485b-ad9a-8d0333f1f4bb 3 2 6
```

# Material
Changes the material of an existing entity.
```r
material <world:number>0> <id:number> <material:string>
```
**Example:**
```r
material 1 854d362a-6db0-485b-ad9a-8d0333f1f4bb RBLX/Plastic
```

# Group
Groups multiple entities together.
```r
group <world:number>0> <id1:number> <id2:number> ...
```
**Example:**
```r
group 1 68a53f45-b1cb-475b-9088-7ebaba1b8777 1dac3795-4d3c-49b9-ac9e-86521d539705 854d362a-6db0-485b-ad9a-8d0333f1f4bb
```