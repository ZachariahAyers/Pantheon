import assert from "node:assert/strict";
import test from "node:test";
import { stale } from "../tools/site-ribbon.mjs";

test("the desk's ribbon and icons match the add-in's ribbon", () => {
    assert.deepEqual(stale(), [], "run node tools/site-ribbon.mjs");
});
