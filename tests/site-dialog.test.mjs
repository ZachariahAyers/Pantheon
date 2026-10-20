import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";
import { dialogs } from "../src/Pantheon.Browser/wwwroot/dialog-spec.js";

const catalog = readFileSync(new URL("../src/Pantheon.AutoCAD/Ui/Members/MemberCatalog.vb", import.meta.url), "utf8");
// VB doubles a quote inside a string: 8" is written "8""".
const vb = (text) => `"${text.replaceAll('"', '""')}"`;

test("each desk dialog has the add-in dialog's title", () => {
    for (const dialog of Object.values(dialogs)) assert.ok(catalog.includes(vb(dialog.title)), dialog.title);
});

test("each desk field has the caption, default, choices and hint the add-in gives it", () => {
    for (const dialog of Object.values(dialogs)) {
        for (const group of dialog.groups) {
            for (const field of group.fields) {
                assert.match(catalog, new RegExp(`MemberField\\.\\w+\\([^)]*${vb(field.caption).replace(/[.*+?^${}()|[\]\\]/g, "\\$&")}`), field.caption);
                if (field.value) assert.ok(catalog.includes(vb(field.value)), `${field.caption} default`);
                for (const choice of field.choices ?? []) assert.ok(catalog.includes(vb(choice)), choice);
                if (field.hint) assert.ok(catalog.includes(vb(field.hint)) || catalog.includes(field.hint), `${field.caption} hint`);
            }
        }
    }
});
