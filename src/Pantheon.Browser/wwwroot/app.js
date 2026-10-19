import { dotnet } from "./_framework/dotnet.js";
import { loadGenerator } from "./runtime.js";

const SVG_NS = "http://www.w3.org/2000/svg";
const form = document.querySelector("#frame-form");
const submit = form.querySelector('button[type="submit"]');
const status = document.querySelector("#status");
const drawing = document.querySelector("#drawing-lines");
const modelNote = document.querySelector("#model-note");
const readoutValues = [...document.querySelectorAll("#readouts dd")];
let generate;
let updateTimer;

const request = () => ({
    spanFeet: Number(form.elements.spanFeet.value),
    lengthFeet: Number(form.elements.lengthFeet.value),
    eaveFeet: Number(form.elements.eaveFeet.value),
    pitchRise: Number(form.elements.pitchRise.value),
    bayCount: Number(form.elements.bayCount.value),
});

function setStatus(message, kind = "") {
    status.textContent = message;
    status.className = `status ${kind}`.trim();
}

function feetAndInches(value) {
    const total = Math.round(value * 16) / 16;
    let feet = Math.floor(total / 12);
    let inches = total - feet * 12;
    if (Math.abs(inches - 12) < 0.001) {
        feet += 1;
        inches = 0;
    }
    const inchText = Number.isInteger(inches) ? String(inches) : inches.toFixed(2).replace(/0+$/, "");
    return `${feet}′ ${inchText}″`;
}

function project(point) {
    return {
        x: point.x - point.y * 0.62,
        y: point.x * 0.32 + point.y * 0.32 - point.z,
    };
}

function draw(lines) {
    const projected = lines.map((line) => ({
        kind: line.kind,
        start: project(line.start),
        end: project(line.end),
    }));
    const points = projected.flatMap((line) => [line.start, line.end]);
    const minX = Math.min(...points.map((point) => point.x));
    const maxX = Math.max(...points.map((point) => point.x));
    const minY = Math.min(...points.map((point) => point.y));
    const maxY = Math.max(...points.map((point) => point.y));
    const width = Math.max(1, maxX - minX);
    const height = Math.max(1, maxY - minY);
    const scale = Math.min(850 / width, 480 / height);
    const offsetX = (1000 - width * scale) / 2 - minX * scale;
    const offsetY = (585 - height * scale) / 2 - minY * scale;

    drawing.replaceChildren();
    for (const line of projected.sort((left, right) => lineOrder(left.kind) - lineOrder(right.kind))) {
        const element = document.createElementNS(SVG_NS, "line");
        element.setAttribute("class", line.kind);
        element.setAttribute("x1", String(line.start.x * scale + offsetX));
        element.setAttribute("y1", String(line.start.y * scale + offsetY));
        element.setAttribute("x2", String(line.end.x * scale + offsetX));
        element.setAttribute("y2", String(line.end.y * scale + offsetY));
        drawing.append(element);
    }
}

function lineOrder(kind) {
    return { annotation: 0, base: 1, offset: 2, frame: 3 }[kind] ?? 0;
}

function showDimensions(dimensions) {
    const values = [
        feetAndInches(dimensions.span),
        feetAndInches(dimensions.length),
        feetAndInches(dimensions.eave),
        feetAndInches(dimensions.ridge),
        `${feetAndInches(dimensions.roofSlope)} · ${dimensions.pitchDegrees.toFixed(1)}°`,
        feetAndInches(dimensions.baySize),
    ];
    readoutValues.forEach((element, index) => {
        element.textContent = values[index];
    });
    modelNote.textContent = `${dimensions.frameCount} frames · ${dimensions.bayCount} equal bays · ${dimensions.lineCount} model lines`;
}

function clearDrawing(note) {
    drawing.replaceChildren();
    drawing.classList.remove("stale");
    readoutValues.forEach((element) => {
        element.textContent = "—";
    });
    modelNote.textContent = note;
}

function rebuild(event) {
    event?.preventDefault();
    if (!generate) {
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
        response = JSON.parse(generate(JSON.stringify(request())));
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

    draw(response.lines);
    drawing.classList.remove("stale");
    showDimensions(response.dimensions);
    setStatus("Workframe rebuilt.", "ready");
}

form.addEventListener("submit", rebuild);
form.addEventListener("input", () => {
    drawing.classList.add("stale");
    modelNote.textContent = "Inputs changed · rebuilding";
    setStatus("Inputs changed. Rebuilding…");
    window.clearTimeout(updateTimer);
    updateTimer = window.setTimeout(rebuild, 140);
});

async function start() {
    submit.disabled = true;
    try {
        generate = await loadGenerator(dotnet);
        submit.disabled = false;
        rebuild();
    } catch (error) {
        console.error(error);
        clearDrawing("Geometry engine unavailable");
        setStatus("The geometry engine could not start in this browser. Reload the page to try again.", "error");
    }
}

await start();
