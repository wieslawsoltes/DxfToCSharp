# DxfCodeGenerator Coverage vs. netDxf

## High-Level Findings
- `DxfCodeGenerator` walks a `netDxf.DxfDocument`, tracks referenced tables/objects, and emits imperative C# to rebuild the drawing. Options in `DxfCodeGenerationOptions` gate each family of objects.
- Every public `netDxf.Entities.EntityObject` derivative (34 types) is recognised in `GenerateEntity`, so entity type coverage is complete. The generator also reuses the same entity writers when populating block contents.
- Property support is uneven: general `EntityObject` state (layer, color, linetype, lineweight, linetype scale, transparency, visibility, normal) is emitted via `GenerateEntityPropertiesCore`, and dedicated helpers cover many per-entity fields. However, several specialised properties are skipped (for example polyline widths, spline knots, tolerance datum data, custom hatch/linetype definitions, insert colour overrides).
- Table objects and style dictionaries are only partially reconstructed. Layers, text styles, dimension styles, linetypes, etc. are emitted as bare objects without copying all fields present in the source `netDxf` definitions. View records and dictionary-based objects fall back to placeholder comments.
- Object collections (`Groups`, `Layouts`, `ImageDefinitions`, `UnderlayDefinitions`, `RasterVariables`, etc.) are emitted when publicly accessible through the netDxf API; internal collections (views, dictionaries, XRecords) are accompanied by explanatory comments instead of code.
- Header variables are reproduced for the subset explicitly enumerated in `GenerateHeaderVariables` (base point, units, linetype scales, current layer/style names, creation/update timestamps, current UCS). Remaining drawing variables keep defaults.

## Entity Coverage Details

### Geometric Primitives
| Entity | Properties Captured | Notable Gaps |
| --- | --- | --- |
| Line | Start/end via constructor; thickness, general entity props. | No explicit handling for `Line.Extension`-related metadata (handled internally). |
| Arc | Center, radius, start/end angles; thickness and general props. | Arc length/direction derived from constructor; ok. |
| Circle | Center, radius; thickness + general props. | — |
| Ellipse | Center, major/minor axes; optional start/end angles; general props. | Axis vector orientation only uses major/minor; ellipse param data otherwise ok. |
| Point | Position; thickness/general props. | Point display style/size is controlled by header variables (covered separately). |
| Ray / XLine | Origin/direction; general props. | — |
| Shape | Name, shape style, position, size, rotation, general props. | Shape style is recreated inline but without copying style metadata beyond file/name. |
| Underlay | Definition constructed, position/scale/rotation, clipping boundary, contrast/fade/options. | `DisplayOptions` written as `UnderlayDisplayFlags.{value}` which misrepresents combined flags; definition parameters (scale factors) beyond ctor defaults not exposed. |
| Image | Definition rebuilt with width/resolution; placement, size, rotation, brightness/contrast/fade, clipping, display flags, color. | Image map off/clipping booleans handled, but `Definition.ClippingBoundary` pixel data not restored. |
| Viewport | Center/size, status flags, view direction/target, clipping, grid/snap settings, UCS, frozen layers. | Requires entities referenced by frozen layers emitted as variables; assumes active viewport only. |

### Curves and Polylines
| Entity | Properties Captured | Notable Gaps |
| --- | --- | --- |
| Polyline2D | Vertex positions with bulge, closed flag, elevation, linetype generation, smooth type; thickness/general props. | Vertex `StartWidth`/`EndWidth` (and per-vertex constant width) are ignored, so tapered/wide segments are lost. |
| Polyline3D | Vertex list, closed flag, linetype generation; thickness/general props. | `Polyline3DVertex` flags (if any) are not reinstated. |
| Spline | Control points, weights, degree; fit/ctrl tolerances, knot parameterisation, start/end tangents; full entity props. | Knot vector and fit point collections are not emitted, so reproducing custom knot distribution may fail; rational/non-rational status inferred from weights only. |
| Hatch | Boundary loops rebuilt by converting hatch edges back into entities; supports solid fills and gradients (angle, scale, tint, origin). General props and elevation copied. | Pattern line definitions for custom hatch patterns are not recreated (`HatchPattern.LineDefinitions` ignored); edge entity styles (layer/linetype/width) not preserved; associative flag is carried. |
| Wipeout | Boundary vertices and layer/props. | Image handle/reactor data (for embedded image definitions) handled through new `ImageDefinition`, but wipeout-specific transparency not emitted (not exposed). |
| Mesh | Vertex list, faces (index arrays), optional edges, subdivision level, general props. | Vertex normals/crease data not exported. |
| PolyfaceMesh | Vertex array, face definitions, general props. | Face visibility flags beyond vertex index order are not emitted. |
| PolygonMesh | Vertex grid, U/V size, density, closed flags, linetype generation, general props. | Smooth type (quadratic/cubic) forced via density clamps—other mesh options not surfaced. |

### Text, Annotation, and Dimensions
| Entity | Properties Captured | Notable Gaps |
| --- | --- | --- |
| Text | Value, position, height; style, rotation, width factor, oblique angle, backward/upside flags; general props. | Alignment, width-aligned text (`Width` property) and second alignment point are not emitted, so `Align/Fit` texts won't reproduce precisely. |
| MText | Value, position, height; style, rectangle width, attachment point, rotation, spacing, drawing direction; general props. | Background fill, column settings, tabs and paragraph formatting beyond defaults are not generated. |
| Leader | Vertex polyline, style, arrow visibility, path type, hookline, colour, text height/width factors, landing settings, annotation handles; XData. | Association to annotation entity assumes entity variables exist; dogleg/annotation offset fine, but leader dimension style overrides beyond provided properties not handled. |
| Dimension (Linear/Aligned/Radial/Diametric/Angular2Line/Angular3Point/Ordinate/ArcLength) | Constructor populates measurement definition; `GenerateDimensionStyleProperties` copies style ref, text rotation, override text, attachment, spacing, elevation; `GenerateDimensionStyleOverrides` replays overrides with correct typed literals. | Dim-dependent props like user defined text position (`TextOffset`), `DimensionBlock` references, or jog symbols are not explicitly emitted (rely on override dictionary). |
| Tolerance | Position, text height, rotation, general props. | Critical data (entry1/entry2 contents, datum identifiers, projected tolerance zone, style overrides) ignored—tolerance feature control frame collapses to defaults. |
| Trace / Solid / Face3D | Vertex coordinates, thickness/general props; Face3D also writes `EdgeFlags`. | For `Solid`/`Trace`, only planarity preserved; no additional metadata. |
| Underlay | (Covered in primitives table). | — |
| Viewport | (Covered above). | — |

### Blocks, Inserts, and Attributes
| Entity/Object | Properties Captured | Notable Gaps |
| --- | --- | --- |
| Block definitions | Block name/origin/description/layer, attribute definitions (tag, prompt, default, constraints, style, linetype, lineweight, visibility, normal), block entities emitted via same entity writers. | Block `ExplodeType`, `IsExternalReference`, `ExternalName`, `Units` not emitted; attribute definitions omit alignment modes like `HorizontalAlignment`. |
| Insert | Position, scale, rotation, layer; attribute values assigned by tag; XData. | Does **not** call `GenerateEntityPropertiesCore`, so insert-level colour/linetype/lineweight/transparency overrides are lost; attributes rely on existing definitions in block, custom attributes without tags skipped. |
| Attribute (entity reference) | Tag/value, position, height, width factor, rotation, alignment, style, flags, back/upside/oblique, colour/linetype/lineweight/transparency/normal. | Field length, verification, locking status, and invisible flag already handled via `Flags`, but owners/external references not considered. |

### Miscellaneous Entities
| Entity | Properties Captured | Notable Gaps |
| --- | --- | --- |
| MLine | Vertex list, scale, justification, elevation, cap flags, style assignment, element colours/linetypes via `GenerateMLineStyle`. | Element visibility flags beyond basic properties not output. |
| Image | (See primitives) | Raster clip invert flag not surfaced. |
| Ray, XLine | (See primitives) | — |
| Shape | (See primitives) | Shape styles beyond font path need manual extension. |

## Table Objects and Style Dictionaries
| Table/Object | Generator Behaviour | Observations |
| --- | --- | --- |
| Layers | Creates `Layer(name)` and applies colour, lineweight, visibility, frozen/locked flags. | Linetype, plot style, transparency, material, and description are omitted. |
| Linetypes | Instantiates `Linetype(name)` only. | Dash/gap sequence (`Segments`) is not rebuilt, so custom linetypes collapse to continuous definition. |
| TextStyles | Creates `TextStyle(name, fontFile)` referencing existing font file or default. | Big font, fixed height, width factor, oblique angle, generation flags, prior style metrics are ignored. |
| Blocks | See entity section; block table entries created when referenced. | Block record flags limited; throwaway blocks not referenced remain absent. |
| DimensionStyles | Registers `new DimensionStyle(name)` without copying settings. | Custom styles lose overrides (unless present as entity overrides). |
| MLineStyles | Recreates styles with element offsets/colours, description, fill colour, start/end angles. | Flags such as `ShowDerivation` and `ShowMiters` not emitted. |
| UCS | Emits non-*ACTIVE* UCS with origin/X/Y vectors. | Full definition covered. |
| VPorts | Only mutates the active viewport (`doc.Viewport`) when deviations from defaults exist. | Named viewport table entries (`doc.VPorts`) are inaccessible via netDxf, so additional viewports aren't generated. |
| Views | `GenerateViewPlaceholder` emits commented sample creation because netDxf keeps `doc.Views` internal. |
| ShapeStyles | Adds `new ShapeStyle(name, file)`. | Style metadata (big font, fixed text height) skipped. |
| ApplicationRegistries | Adds non-default application registry names used by XData. | — |

## Object Collections (Named Object Dictionary)
| Object | Generator Behaviour | Observations |
| --- | --- | --- |
| Groups | Recreates groups with description/selectability and re-adds member entities (requires each member emitted as a variable). | Works if `_entitiesNeedingVariables` recorded the handles; fails for entities left inline in code. |
| Layouts | Builds layout with limits/extents/UCS/elevation data. | Plot settings pointer not emitted; paper space vs model automatically decided by netDxf. |
| ImageDefinitions | Recreated with file path, dimensions, DPI, resolution units. | Depends on the original file path being valid at runtime. |
| UnderlayDefinitions | Instantiates type-specific definitions (DGN/DWF/PDF) and adds them to the relevant collections. | No support for definition defaults such as `ShowClippingBoundary`. |
| RasterVariables | Toggles `DisplayFrame`, `DisplayQuality`, `Units` when non-default. | Other raster vars (u/v coordinate ratio) not exposed in netDxf. |
| LayerStates, PlotSettings, XRecords, DictionaryObject | Only placeholder comments outlining structure, because `netDxf` keeps these under internal dictionaries. Example snippets are emitted when `GenerateDetailedComments` is true. | Actual layer state/property persistence is unsupported. |
| MLineStyles (dictionary objects) | Comment noting they live in dictionaries; regeneration limited to table entries discussed above. |

## Header Variables
`GenerateHeaderVariables` emits assignments when drawing variables deviate from defaults for:
- Base insertion point (`InsBase`), version (`AcadVer`), angular base/direction, text size, global and current linetype scale, point size/mode, current colour/linetype/lineweight, current layer/dimension/text/mline styles, insertion units, attribute mode, drawing/angular/linear unit formats, mline justification/scale, spline segments, surface resolution, pline generation, plot-style scaling, linetype display, mirrtext, lineweight display.
- Current entity colour (`CeColor`), user metadata (`LastSavedBy`), creation/update timestamps and time-in-drawing, current UCS (`CurrentUCS`).

Missing variables retain defaults, notably: undo/redo settings, dimension variables exposed only through `DimensionStyle`, PDMODE/PDSIZE-specific variations, snap/grid settings (covered instead in viewport section), view modes, user-specified coordinate or view settings not surfaced through `DrawingVariables` API.

## Additional Observations and Risks
- Entity writers rely on `_entitiesNeedingVariables` to lift certain entities into variables (group members, entities with XData). If a grouped entity was emitted inline before the group is defined, the `group.Entities.Add(entityHandle)` statements reference undefined variables. The analyser adds every group member handle, but if generation options disable a type, handles are skipped and groups become incomplete.
- Insert entities omit direct colour/linetype overrides and normal vector data, so inserts that override block defaults revert to ByLayer behaviour.
- Tolerance, hatch, linetype, dimension style, and shape style data are lossy. Regenerating a document with heavy customisation (GD&T frames, bespoke patterns, styles) will not faithfully reproduce the source without manual post-editing.
- Placeholder sections (views, dictionaries, XRecords, layer states, plot settings) document intent but still require manual intervention to finish the translation; automated round-tripping is unsupported for those object families.
- Many API surfaces in netDxf remain internal (named object dictionary traversal, view table, dimension style data). Extending coverage would require exposing or reflecting into internal members of netDxf.

