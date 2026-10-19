import assert from "node:assert/strict";
import test from "node:test";
import { loadGenerator } from "../src/Pantheon.Browser/wwwroot/runtime.js";

test("keeps the exported adapter alive without running the empty Main", async () => {
    const generate = () => "result";
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
                    return { Pantheon: { Browser: { BrowserAdapter: { Generate: generate } } } };
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

    assert.equal(await loadGenerator(dotnet), generate);
    assert.equal(exits, 0);
});
