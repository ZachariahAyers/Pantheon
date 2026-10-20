let browserRuntime;

/** Loads the exported adapter while leaving the created runtime alive for later form edits. */
export async function loadGenerator(dotnet) {
    browserRuntime = await dotnet.create();
    const config = browserRuntime.getConfig();
    const exports = await browserRuntime.getAssemblyExports(config.mainAssemblyName);
    const { Generate, Dxf } = exports.Pantheon.Browser.BrowserAdapter;
    return { generate: Generate, dxf: Dxf };
}
