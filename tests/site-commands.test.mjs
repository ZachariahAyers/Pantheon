import assert from "node:assert/strict";
import { readdirSync, readFileSync } from "node:fs";
import test from "node:test";

const source = (path) => readFileSync(new URL(`../${path}`, import.meta.url), "utf8");
const commandsDir = "src/Pantheon.AutoCAD/Commands/";
const registered = readdirSync(new URL(`../${commandsDir}`, import.meta.url))
    .filter((file) => file.endsWith(".vb"))
    .flatMap((file) => [...source(commandsDir + file).matchAll(/<CommandMethod\("([A-Z]+)"\)>/g)].map((match) => match[1]))
    .sort();

test("the add-in registers commands to compare against", () => {
    assert.ok(registered.includes("WORKFRAME"));
});

test("the page lists every add-in command and no other", () => {
    const page = source("src/Pantheon.Browser/wwwroot/index.html");
    const listed = [...page.matchAll(/<dt><code>([A-Z]+)<\/code><\/dt>/g)].map((match) => match[1]).sort();
    assert.deepEqual(listed, registered);
});

test("every ribbon button runs a registered command, and every command but PANTHEON has a button", () => {
    const ribbon = source("src/Pantheon.AutoCAD/Ui/PantheonRibbon.vb");
    const buttons = [...new Set([...ribbon.matchAll(/"_([A-Z]+) /g)].map((match) => match[1]))].sort();
    assert.deepEqual(buttons, registered.filter((command) => command !== "PANTHEON"));
});
