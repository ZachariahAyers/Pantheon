// The Pantheon desk. It plays WORKFRAME from an empty drawing to a finished frame: a cursor presses the
// add-in's ribbon button, fills its dialog and picks the entry point, and Pantheon.Core (compiled to
// WebAssembly) works out every line. The visitor can then open any workframe dialog and draw their own.
import { dotnet } from "./_framework/dotnet.js";
import { loadGenerator } from "./runtime.js";
import { Cursor, wait } from "./cursor.js";
import { WorkframeDialog } from "./dialog.js";
import { dialogs, sampleEntries } from "./dialog-spec.js";
import { FrameScene } from "./frame-scene.js";
import { markActive, renderRibbon } from "./ribbon-view.js";

const SYMMETRICAL = "_WORKFRAME _Symmetrical";
const HISTORY = 6;

const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
const cad = document.querySelector("#cad");
const ribbon = document.querySelector("#ribbon");
const model = document.querySelector("#model");
const command = document.querySelector("#command");
const readouts = document.querySelector("#readouts");
const modelNote = document.querySelector("#model-note");
const clip = document.querySelector("#clip");
const tryButton = document.querySelector("#try");
const replayButton = document.querySelector("#replay");
const downloadButton = document.querySelector("#download");
const scene = new FrameScene(model, { reduceMotion });
const dialog = new WorkframeDialog(document.querySelector("#dialog"));
const cursor = new Cursor(cad, document.querySelector("#cursor"), { reduceMotion });
const enginePromise = loadGenerator(dotnet);
let engine;
let playing = false;
let drawn = null;

function say(text) {
  const line = document.createElement("li");
  line.textContent = text;
  command.append(line);
  while (command.children.length > HISTORY) command.firstElementChild.remove();
}

function showReadouts(values) {
  readouts.replaceChildren(
    ...values.map(({ label, value }) => {
      const row = document.createElement("div");
      const term = document.createElement("dt");
      const detail = document.createElement("dd");
      term.textContent = label;
      detail.textContent = value;
      row.append(term, detail);
      return row;
    }),
  );
}

function generate(request) {
  return JSON.parse(engine.generate(JSON.stringify(request)));
}

async function draw(request, response, mark) {
  drawn = { request, mark };
  await scene.draw(response);
  showReadouts(response.readouts);
  modelNote.textContent = `Block ${mark || "unnamed"} on layer WorkFrames · ${response.lines.length} lines`;
  downloadButton.disabled = false;
  say("Command:");
}

function fill(commandName) {
  for (const [key, value] of Object.entries(sampleEntries[commandName])) {
    const field = dialog.field(key);
    if (field) field.value = value;
  }
}

/** The demo: an empty drawing, the ribbon button, the dialog filled in, the entry point, the frame. */
async function play() {
  playing = true;
  for (const button of [tryButton, replayButton, downloadButton]) button.disabled = true;
  dialog.close();
  clip.hidden = true;
  scene.clear();
  readouts.replaceChildren();
  modelNote.textContent = "";
  command.replaceChildren();
  markActive(ribbon, null);
  say("Command:");
  cursor.show();
  await cursor.moveTo({ x: cad.clientWidth * 0.5, y: cad.clientHeight * 0.55 });
  await cursor.click(document.querySelector("#pantheon-tab"));

  const button = ribbon.querySelector(`[data-command="${SYMMETRICAL}"]`);
  await cursor.click(button);
  markActive(ribbon, SYMMETRICAL);
  say("Command: _WORKFRAME");
  say("Enter workframe type [SYMmetrical/SINgleslope/Offcenter]: _Symmetrical");
  dialog.open(SYMMETRICAL);
  await wait(reduceMotion ? 0 : 300);

  for (const [key, value] of Object.entries(sampleEntries[SYMMETRICAL])) {
    const field = dialog.field(key);
    await cursor.click(field);
    await cursor.typeValue(field, value);
  }
  await cursor.click(dialog.create);
  await enginePromise;
  const request = dialog.request();
  const response = generate(request);
  const mark = dialog.field("mark").value;
  dialog.close();

  say("Select bottom left entry point:");
  const entry = scene.entryPoint();
  await cursor.click(cursor.pointIn(model, entry.x, entry.y));
  if (response.ok) await draw(request, response, mark);
  else say(response.error);

  cursor.hide();
  playing = false;
  tryButton.disabled = false;
  replayButton.disabled = false;
}

function openDialog(commandName) {
  clip.hidden = true;
  markActive(ribbon, commandName);
  say(`Command: ${commandName.replace(" _", " ")}`);
  dialog.open(commandName);
  fill(commandName);
  (dialog.field("width") ?? dialog.field("left.width"))?.focus();
}

function pressRibbon(button) {
  if (playing) return;
  if (dialogs[button.command]) {
    openDialog(button.command);
    return;
  }
  dialog.close();
  markActive(ribbon, button.command);
  say(`Command: ${button.command}`);
  clip.hidden = false;
  document.querySelector("#clip-title").textContent = button.label;
  document.querySelector("#clip-text").textContent =
    `${button.description} It runs inside AutoCAD on your drawing; a short clip recorded in AutoCAD will show it here.`;
}

async function create() {
  if (playing) return;
  const request = dialog.request();
  const response = generate(request);
  if (!response.ok) {
    dialog.error.textContent = response.error;
    return;
  }
  const mark = dialog.field("mark").value;
  dialog.close();
  say("Select bottom left entry point:");
  await draw(request, response, mark);
}

function saveDxf() {
  if (!drawn) return;
  const text = engine.dxf(JSON.stringify(drawn.request));
  const link = document.createElement("a");
  link.href = URL.createObjectURL(new Blob([text], { type: "application/dxf" }));
  link.download = `pantheon-${(drawn.mark || "workframe").toLowerCase().replace(/[^a-z0-9]+/g, "-")}.dxf`;
  link.click();
  window.setTimeout(() => URL.revokeObjectURL(link.href), 1000);
  say("Drawing saved. Open it in AutoCAD; units are inches.");
}

async function start() {
  renderRibbon(ribbon, pressRibbon);
  dialog.create.addEventListener("click", create);
  dialog.cancel.addEventListener("click", () => {
    if (!playing) dialog.close();
  });
  tryButton.addEventListener("click", () => openDialog(SYMMETRICAL));
  replayButton.addEventListener("click", play);
  downloadButton.addEventListener("click", saveDxf);
  try {
    engine = await enginePromise;
  } catch (error) {
    console.error(error);
    say("Pantheon.Core could not start in this browser. Reload the page to try again.");
    return;
  }

  // Play once the window is on screen, so the visitor sees it from the empty drawing.
  const watcher = new IntersectionObserver(
    (entries) => {
      if (entries.some((entry) => entry.isIntersecting)) {
        watcher.disconnect();
        play();
      }
    },
    { threshold: 0.5 },
  );
  watcher.observe(cad);
}

start();
