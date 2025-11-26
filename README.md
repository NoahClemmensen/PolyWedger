# PolyWedger

PolyWedger converts 3D models into Roblox-compatible wedge part data. It uses HelixToolkit to read model geometry and translates polygons into the wedge data.

## Highlights
- Loads many common 3D formats via HelixToolkit / Assimp.
- Converts mesh polygons into Roblox wedge part data.
- Produces a single output file (extension TBD) that you can import into RGE or other tools that accept wedge data.

## How it works
1. HelixToolkit loads the model and exposes the mesh geometry.
2. Polygons are analyzed and translated into wedge primitives.
3. Wedge primitives are serialized to an output file for later implementation into RGE.

## Limitations
- There is a hard limit of 400,000 wedges.
- A single polygon may produce multiple wedges depending on its shape.
- Models with more than ~200,000 polygons may not convert successfully due to the wedge limit.

## Supported input formats
See the full list of supported formats via Assimp:  
https://github.com/assimp/assimp/blob/master/doc/Fileformats.md

## Usage
1. Open a supported 3D model in PolyWedger.
2. Run the conversion process.
3. Import the generated output file into RGE (import procedure depends on your RGE workflow).

## Notes
- PolyWedger is a translator only; it does not integrate directly with RGE.
- Output file extension and schema will be documented when finalized.