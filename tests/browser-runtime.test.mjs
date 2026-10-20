import assert from "node:assert/strict";
import test from "node:test";
import { loadGenerator } from "../src/Pantheon.Browser/wwwroot/runtime.js";

test("keeps the exported adapter alive without running the empty Main", async () => {
    const generate = () => "result";
    const dxf = () => "drawing";
    let exits = 0;
    const dotnet = {
        run: () => {
            exits += 1;
        },
        runMain: () => {
            exits += 1;
        },
        async create() {
            return {
                getConfig: () => ({ mainAssemblyName: "Pantheon.Browser" }),
                getAssemblyExports: async (assembly) => {
                    assert.equal(assembly, "Pantheon.Browser");
                    return { Pantheon: { Browser: { BrowserAdapter: { Generate: generate, Dxf: dxf } } } };
                },
                run: () => {
                    exits += 1;
                },
                runMain: () => {
                    exits += 1;
                },
            };
        },
    };

    assert.deepEqual(await loadGenerator(dotnet), { generate, dxf });
    assert.equal(exits, 0);
});
