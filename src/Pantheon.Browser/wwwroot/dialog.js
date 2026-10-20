// The workframe dialog, laid out as the add-in's member dialog lays it out: the mark, then each group's
// heading and its captioned entries, then Create and Cancel.
import { dialogs } from "./dialog-spec.js";

function element(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}

export class WorkframeDialog {
  constructor(container) {
    this.container = container;
    this.title = container.querySelector(".cad-dialog-title");
    this.body = container.querySelector(".cad-dialog-body");
    this.error = container.querySelector(".cad-dialog-error");
    this.create = container.querySelector(".cad-dialog-create");
    this.cancel = container.querySelector(".cad-dialog-cancel");
    this.command = null;
  }

  /** Opens the dialog for a ribbon command, with every entry blank but its default. */
  open(command) {
    const spec = dialogs[command];
    this.command = command;
    this.title.textContent = spec.title;
    this.error.textContent = "";
    const rows = [this.row("Mark", this.input("mark", ""))];
    for (const group of spec.groups) {
      rows.push(element("div", "cad-dialog-heading", group.heading));
      for (const field of group.fields) {
        const input = field.choices ? this.select(field) : this.input(field.key, field.value ?? "");
        if (field.hint) input.title = field.hint;
        rows.push(this.row(field.caption, input));
      }
    }
    this.body.replaceChildren(...rows);
    this.container.hidden = false;
  }

  close() {
    this.container.hidden = true;
  }

  input(key, value) {
    const node = element("input");
    node.name = key;
    node.value = value;
    node.autocomplete = "off";
    node.spellcheck = false;
    return node;
  }

  select(field) {
    const node = element("select");
    node.name = field.key;
    node.append(...field.choices.map((choice) => element("option", "", choice)));
    return node;
  }

  row(caption, input) {
    const label = element("label", "cad-dialog-row");
    label.append(element("span", "", caption), input);
    return label;
  }

  field(key) {
    return this.body.querySelector(`[name="${CSS.escape(key)}"]`);
  }

  /** The entries as the desk's generator reads them. */
  request() {
    const spec = dialogs[this.command];
    const value = (key) => this.field(key)?.value ?? "";
    const sideOf = (prefix) => ({
      width: value(`${prefix}width`),
      eave: value(`${prefix}eave`),
      length: value(`${prefix}length`),
      pitch: value(`${prefix}pitch`),
      purlin: value(`${prefix}purlin`),
      girt: value(`${prefix}girt`),
      girts: value(`${prefix}girts`),
    });
    const request = { roof: spec.roof, sideBays: value("sideBays"), endBays: value("endBays") };
    if (spec.roof === "offcenter") return { ...request, left: sideOf("left."), right: sideOf("right.") };
    return {
      ...request,
      width: value("width"),
      eave: value("eave"),
      length: value("length"),
      pitch: value("pitch"),
      lowEave: value("lowEave"),
      highEave: value("highEave"),
      purlin: value("purlin"),
      girt: value("girt"),
      girts: value("girts"),
    };
  }
}
