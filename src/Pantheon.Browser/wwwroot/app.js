import { dotnet } from "./_framework/dotnet.js";
import { loadGenerator } from "./runtime.js";

const SVG_NS = "http://www.w3.org/2000/svg";
const ROOFS = {
    gable: { title: "Gable workframe", eave: "Eave", pitch: "Pitch" },
    single: { title: "Single slope workframe", eave: "Low eave", pitch: "Pitch" },
    offset: { title: "Off-center ridge workframe", eave: "Eave", pitch: "Right pitch" },
};
const form = document.querySelector("#frame-form");
const submit = form.querySelector('button[type="submit"]');
const download = document.querySelector("#download");
const status = document.querySelector("#status");
const drawing = document.querySelector("#drawing-lines");
const bubbleLayer = document.querySelector("#drawing-bubbles");
const drawingTitle = document.querySelector("#drawing-title");
const readouts = document.querySelector("#readouts");
const modelNote = document.querySelector("#model-note");
let engine;
let updateTimer;

const roof = () => form.elements.roof.value;

const request = () => ({
    roof: roof(),
    spanFeet: Number(form.elements.spanFeet.value),
    lengthFeet: Number(form.elements.lengthFeet.value),
    eaveFeet: Number(form.elements.eaveFeet.value),
    pitchRise: Number(form.elements.pitchRise.value),
    bayCount: Number(form.elements.bayCount.value),
    highEaveFeet: Number(form.elements.highEaveFeet.value),
    ridgeFeet: Number(form.elements.ridgeFeet.value),
});

function setStatus(message, kind = "") {
    status.textContent = message;
    status.className = `status ${kind}`.trim();
}

/** Shows the fields the chosen roof reads; hidden fields are disabled so they never block the form. */
function showRoofFields() {
    const chosen = roof();
    for (const field of form.querySelectorAll("[data-roofs]")) {
        const shown = field.dataset.roofs.split(" ").includes(chosen);
        field.hidden = !shown;
        field.querySelector("input").disabled = !shown;
    }
    for (const label of form.querySelectorAll("[data-roof-label]")) {
        label.textContent = ROOFS[chosen][label.dataset.roofLabel];
    }
    drawingTitle.textContent = ROOFS[chosen].title;
    form.elements.ridgeFeet.max = String(Math.max(1, Number(form.elements.spanFeet.value) - 1));
}

function project(point) {
    return {
        x: point.x - point.y * 0.62,
        y: point.x * 0.32 + point.y * 0.32 - point.z,
    };
}

function draw(lines, bubbles) {
    const projected = lines.map((line) => ({
        kind: line.kind,
        start: project(line.start),
        end: project(line.end),
    }));
    const marks = bubbles.map((bubble) => ({ label: bubble.label, center: project(bubble.center), radius: bubble.radius }));
    const points = [
        ...projected.flatMap((line) => [line.start, line.end]),
        ...marks.flatMap((mark) => [
            { x: mark.center.x - mark.radius, y: mark.center.y - mark.radius },
            { x: mark.center.x + mark.radius, y: mark.center.y + mark.radius },
        ]),
    ];
    const minX = Math.min(...points.map((point) => point.x));
    const maxX = Math.max(...points.map((point) => point.x));
    const minY = Math.min(...points.map((point) => point.y));
    const maxY = Math.max(...points.map((point) => point.y));
    const width = Math.max(1, maxX - minX);
    const height = Math.max(1, maxY - minY);
    const scale = Math.min(850 / width, 480 / height);
    const offsetX = (1000 - width * scale) / 2 - minX * scale;
    const offsetY = (585 - height * scale) / 2 - minY * scale;
    const x = (value) => String(value * scale + offsetX);
    const y = (value) => String(value * scale + offsetY);

    drawing.replaceChildren();
    for (const line of projected.sort((left, right) => lineOrder(left.kind) - lineOrder(right.kind))) {
        const element = document.createElementNS(SVG_NS, "line");
        element.setAttribute("class", line.kind);
        element.setAttribute("x1", x(line.start.x));
        element.setAttribute("y1", y(line.start.y));
        element.setAttribute("x2", x(line.end.x));
        element.setAttribute("y2", y(line.end.y));
        drawing.append(element);
    }

    bubbleLayer.replaceChildren();
    for (const mark of marks) {
        const circle = document.createElementNS(SVG_NS, "circle");
        circle.setAttribute("cx", x(mark.center.x));
        circle.setAttribute("cy", y(mark.center.y));
        circle.setAttribute("r", String(Math.max(8, mark.radius * scale)));
        const text = document.createElementNS(SVG_NS, "text");
        text.setAttribute("x", x(mark.center.x));
        text.setAttribute("y", y(mark.center.y));
        text.textContent = mark.label;
        bubbleLayer.append(circle, text);
    }
}

function lineOrder(kind) {
    return { annotation: 0, base: 1, offset: 2, frame: 3 }[kind] ?? 0;
}

function showReadouts(values) {
    readouts.replaceChildren(
        ...values.map((readout) => {
            const row = document.createElement("div");
            const label = document.createElement("dt");
            const value = document.createElement("dd");
            label.textContent = readout.label;
            value.textContent = readout.value;
            value.title = readout.value;
            row.append(label, value);
            return row;
        }),
    );
}

function clearDrawing(note) {
    drawing.replaceChildren();
    bubbleLayer.replaceChildren();
    drawing.classList.remove("stale");
    readouts.replaceChildren();
    modelNote.textContent = note;
    download.disabled = true;
}

function rebuild(event) {
    event?.preventDefault();
    if (!engine) {
        return;
    }
    if (!form.checkValidity()) {
        if (event?.type === "submit") {
            form.reportValidity();
        }
        clearDrawing("No current frame · correct the inputs");
        setStatus("Correct the highlighted dimension before rebuilding.", "error");
        return;
    }

    let response;
    try {
        response = JSON.parse(engine.generate(JSON.stringify(request())));
    } catch (error) {
        console.error(error);
        clearDrawing("Geometry engine error");
        setStatus("The workframe could not be rebuilt. Check the inputs and try again.", "error");
        return;
    }
    if (!response.ok) {
        clearDrawing("No current frame · correct the inputs");
        setStatus(response.error, "error");
        return;
    }

    draw(response.lines, response.bubbles);
    drawing.classList.remove("stale");
    showReadouts(response.readouts);
    const { frameCount, bayCount, lineCount } = response.summary;
    modelNote.textContent = `${frameCount} frames · ${bayCount} equal bays · ${lineCount} model lines`;
    download.disabled = false;
    setStatus("Workframe rebuilt.", "ready");
}

function saveDxf() {
    const text = engine?.dxf(JSON.stringify(request()));
    if (!text) {
        setStatus("Correct the inputs before downloading the drawing.", "error");
        return;
    }
    const link = document.createElement("a");
    link.href = URL.createObjectURL(new Blob([text], { type: "application/dxf" }));
    link.download = `pantheon-${roof()}-workframe.dxf`;
    link.click();
    window.setTimeout(() => URL.revokeObjectURL(link.href), 1000);
    setStatus("Drawing saved. Open it in AutoCAD; units are inches.", "ready");
}

form.addEventListener("submit", rebuild);
form.addEventListener("input", (event) => {
    if (event.target.name === "roof" || event.target.name === "spanFeet") {
        showRoofFields();
    }
    drawing.classList.add("stale");
    modelNote.textContent = "Inputs changed · rebuilding";
    setStatus("Inputs changed. Rebuilding…");
    window.clearTimeout(updateTimer);
    updateTimer = window.setTimeout(rebuild, 140);
});
download.addEventListener("click", saveDxf);

async function start() {
    showRoofFields();
    submit.disabled = true;
    download.disabled = true;
    try {
        engine = await loadGenerator(dotnet);
        submit.disabled = false;
        rebuild();
    } catch (error) {
        console.error(error);
        clearDrawing("Geometry engine unavailable");
        setStatus("The geometry engine could not start in this browser. Reload the page to try again.", "error");
    }
}

await start();
