// The desk's on-screen cursor: it glides to a target inside the CAD window and clicks, drags or
// types there, so a visitor can follow each step as if watching someone use Pantheon.

const MOVE_MS = 650;
const KEY_MS = 90;

export const wait = (ms) => new Promise((resolve) => window.setTimeout(resolve, ms));

export class Cursor {
  constructor(windowElement, cursorElement, { reduceMotion }) {
    this.window = windowElement;
    this.element = cursorElement;
    this.reduceMotion = reduceMotion;
    this.x = 40;
    this.y = 40;
    this.place(this.x, this.y);
  }

  place(x, y) {
    this.x = x;
    this.y = y;
    this.element.animate([{ transform: `translate(${x}px, ${y}px)` }], { duration: 0, fill: "forwards" });
  }

  show() {
    this.element.classList.add("cad-cursor-visible");
  }

  hide() {
    this.element.classList.remove("cad-cursor-visible");
  }

  /** The centre of an element, in the window's coordinates. */
  centreOf(target) {
    const box = target.getBoundingClientRect();
    const frame = this.window.getBoundingClientRect();
    return { x: box.left + box.width / 2 - frame.left, y: box.top + box.height / 2 - frame.top };
  }

  /** A point in an SVG's user units, in the window's coordinates. */
  pointIn(svg, x, y) {
    const point = new DOMPoint(x, y).matrixTransform(svg.getScreenCTM());
    const frame = this.window.getBoundingClientRect();
    return { x: point.x - frame.left, y: point.y - frame.top };
  }

  async moveTo({ x, y }, duration = MOVE_MS) {
    const from = `translate(${this.x}px, ${this.y}px)`;
    const to = `translate(${x}px, ${y}px)`;
    this.x = x;
    this.y = y;
    if (this.reduceMotion) {
      this.place(x, y);
      return;
    }
    await this.element.animate([{ transform: from }, { transform: to }], {
      duration,
      easing: "cubic-bezier(.45,.05,.25,1)",
      fill: "forwards",
    }).finished;
  }

  async click(target) {
    if (target) await this.moveTo(target instanceof Element ? this.centreOf(target) : target);
    this.element.classList.add("cad-cursor-press");
    const ring = document.createElement("span");
    ring.className = "cad-click-ring";
    this.window.append(ring);
    ring.animate([{ transform: `translate(${this.x}px, ${this.y}px)` }], { duration: 0, fill: "forwards" });
    await wait(this.reduceMotion ? 0 : 180);
    this.element.classList.remove("cad-cursor-press");
    window.setTimeout(() => ring.remove(), 600);
  }

  /** Drags from the cursor's place to `to`, calling `onMove` with each in-between point. */
  async drag(to, onMove, duration = 900) {
    const from = { x: this.x, y: this.y };
    const steps = this.reduceMotion ? 1 : Math.round(duration / 16);
    this.element.classList.add("cad-cursor-press");
    for (let i = 1; i <= steps; i++) {
      const t = i / steps;
      const eased = t < 0.5 ? 2 * t * t : 1 - (-2 * t + 2) ** 2 / 2;
      const point = { x: from.x + (to.x - from.x) * eased, y: from.y + (to.y - from.y) * eased };
      this.place(point.x, point.y);
      onMove(point);
      await wait(16);
    }
    this.element.classList.remove("cad-cursor-press");
  }

  /** Types `text` into an input, one key at a time, replacing what it held. */
  async typeValue(input, text) {
    input.value = "";
    for (const character of text) {
      input.value += character;
      await wait(this.reduceMotion ? 0 : KEY_MS);
    }
  }

  /** Types `text` into `line`, one key at a time. */
  async type(line, text) {
    for (const character of text) {
      line.textContent += character;
      await wait(this.reduceMotion ? 0 : KEY_MS);
    }
  }
}
