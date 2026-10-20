// The drawing in the CAD window: Pantheon.Core's workframe lines and grid bubbles, seen from the
// south-west as the add-in turns the view after drawing, each line growing from its start point.

const SVG_NS = "http://www.w3.org/2000/svg";
const ORDER = { base: 0, frame: 1, offset: 2, annotation: 3 };
const WIDTH = 1000;
const HEIGHT = 620;

function svg(tag, attributes = {}) {
  const node = document.createElementNS(SVG_NS, tag);
  for (const [name, value] of Object.entries(attributes)) node.setAttribute(name, String(value));
  return node;
}

/** South-west isometric: X along the length runs right and down, Y across the width runs left. */
function project(point) {
  return { x: point.x - point.y * 0.62, y: point.x * 0.32 + point.y * 0.32 - point.z };
}

export class FrameScene {
  constructor(svgElement, { reduceMotion }) {
    this.svg = svgElement;
    this.reduceMotion = reduceMotion;
    this.svg.setAttribute("viewBox", `0 0 ${WIDTH} ${HEIGHT}`);
    this.lines = svg("g", { class: "cad-lines" });
    this.bubbles = svg("g", { class: "cad-bubbles" });
    this.svg.append(this.lines, this.bubbles);
  }

  clear() {
    this.lines.replaceChildren();
    this.bubbles.replaceChildren();
  }

  /** The point in the drawing where the frame's bottom-left corner lands, for the entry-point pick. */
  entryPoint() {
    return { x: WIDTH * 0.42, y: HEIGHT * 0.62 };
  }

  /** Draws `response`'s lines kind by kind, each growing from its start, then its grid bubbles. */
  async draw(response) {
    this.clear();
    const projected = response.lines
      .map((line) => ({ kind: line.kind, start: project(line.start), end: project(line.end) }))
      .sort((a, b) => ORDER[a.kind] - ORDER[b.kind]);
    const marks = response.bubbles.map((bubble) => ({ label: bubble.label, at: project(bubble.center), radius: bubble.radius }));
    const points = [
      ...projected.flatMap((line) => [line.start, line.end]),
      ...marks.flatMap((m) => [{ x: m.at.x - m.radius, y: m.at.y - m.radius }, { x: m.at.x + m.radius, y: m.at.y + m.radius }]),
    ];
    const minX = Math.min(...points.map((p) => p.x));
    const maxX = Math.max(...points.map((p) => p.x));
    const minY = Math.min(...points.map((p) => p.y));
    const maxY = Math.max(...points.map((p) => p.y));
    const scale = Math.min((WIDTH * 0.86) / Math.max(1, maxX - minX), (HEIGHT * 0.82) / Math.max(1, maxY - minY));
    const offsetX = (WIDTH - (maxX - minX) * scale) / 2 - minX * scale;
    const offsetY = (HEIGHT - (maxY - minY) * scale) / 2 - minY * scale;
    const fit = (p) => ({ x: p.x * scale + offsetX, y: p.y * scale + offsetY });

    const groups = Object.keys(ORDER).map((kind) => projected.filter((line) => line.kind === kind));
    for (const group of groups) {
      const drawn = group.map((line) => {
        const start = fit(line.start);
        const end = fit(line.end);
        const node = svg("line", { class: line.kind, x1: start.x, y1: start.y, x2: start.x, y2: start.y });
        this.lines.append(node);
        return { node, start, end };
      });
      await this.grow(drawn);
    }
    for (const mark of marks) {
      const at = fit(mark.at);
      const circle = svg("circle", { cx: at.x, cy: at.y, r: Math.max(9, mark.radius * scale) });
      const text = svg("text", { x: at.x, y: at.y });
      text.textContent = mark.label;
      this.bubbles.append(circle, text);
    }
    if (!this.reduceMotion) {
      await this.bubbles.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 400 }).finished;
    }
  }

  async grow(drawn) {
    const finish = () => {
      for (const { node, end } of drawn) {
        node.setAttribute("x2", end.x);
        node.setAttribute("y2", end.y);
      }
    };
    if (this.reduceMotion || drawn.length === 0) {
      finish();
      return;
    }
    const duration = 650;
    const started = performance.now();
    await new Promise((resolve) => {
      const step = (now) => {
        const t = Math.min(1, (now - started) / duration);
        const eased = 1 - (1 - t) ** 3;
        for (const { node, start, end } of drawn) {
          node.setAttribute("x2", start.x + (end.x - start.x) * eased);
          node.setAttribute("y2", start.y + (end.y - start.y) * eased);
        }
        if (t < 1) requestAnimationFrame(step);
        else resolve();
      };
      requestAnimationFrame(step);
    });
    finish();
  }
}
