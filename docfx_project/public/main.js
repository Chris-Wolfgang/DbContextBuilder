// The modern DocFX template imports public/main.js as an ES module on every page and reads its
// default export for options (defaultTheme, iconLinks, ...). Its top-level code runs on that
// import, which is where the version picker is loaded (#606): resolved next to this file, so
// the same line works at /<repo>/versions/<v>/ on GitHub Pages, on a CNAME and on localhost.
const versionPicker = document.createElement('script');
versionPicker.src = new URL('version-picker.js', import.meta.url).href;
document.head.appendChild(versionPicker);

export default {};
