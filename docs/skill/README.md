# Claude Code skill source

`static-components/` is the source for the downloadable skill served at `/static-components.skill`
on the docs site. The markdown here is the source of truth; the archive in `public/` is a build
artifact and is gitignored.

## Layout

```
static-components/
  SKILL.md                          conventions that apply to every component
  references/
    headless-components.md          base classes for form inputs, the static-* tag helpers
    attribute-passthrough.md        AdditionalAttributes, static-attributes, merge rules
    static-scripts.md               static-script, teleport, render-once, JavascriptConvert
    alpine-and-htmx.md              passing server state to Alpine, where x-/hx- attributes go
    overriding-rendering.md         overriding ProcessAsync
```

Claude always reads `SKILL.md` and opens a reference file only when the task calls for it, so
detail in `references/` costs nothing until it is needed. `SKILL.md` carries the routing table —
add a row to it whenever you add a reference file.

## Packaging

`npm run skill:zip` rebuilds `public/static-components.skill` from these files. The `predev`,
`prestart` and `prebuild` hooks run it automatically, so `npm run dev` and `npm run build` — and
therefore CI — always package the current sources. The archive can't drift from the markdown.

The build script (`../scripts/build-skill-zip.mjs`) writes the zip records by hand with `zlib`
rather than pulling in a packaging dependency, and stamps a fixed timestamp so identical inputs
produce an identical file.

## Versioning

Write the package version as `{{PACKAGE_VERSION}}` — never a literal. The build script substitutes
it from `docs/.env`, the same file the installation page reads, so a release only edits
`PACKAGE_VERSION` there and both the site and the skill follow.

Keep that value in sync with `<Version>` in
`src/StaticComponents/TechGems.StaticComponents.csproj`.
