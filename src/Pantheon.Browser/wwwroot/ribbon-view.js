// Draws the Pantheon ribbon tab from the generated copy of the add-in's own (ribbon.js). Ribbon bitmaps
// come with a grey ground; it is made clear here, as the add-in's Icons.LoadRibbonImage does at load.
import { panels } from "./ribbon.js";

const RIBBON_GREY = [192, 192, 192];

function element(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}

async function iconUrl(path) {
  if (!path.endsWith(".bmp")) return path;
  const image = new Image();
  image.src = path;
  await image.decode();
  const canvas = document.createElement("canvas");
  canvas.width = image.naturalWidth;
  canvas.height = image.naturalHeight;
  const context = canvas.getContext("2d");
  context.drawImage(image, 0, 0);
  const pixels = context.getImageData(0, 0, canvas.width, canvas.height);
  for (let i = 0; i < pixels.data.length; i += 4) {
    if (RIBBON_GREY.every((value, channel) => pixels.data[i + channel] === value)) pixels.data[i + 3] = 0;
  }
  context.putImageData(pixels, 0, 0);
  return canvas.toDataURL("image/png");
}

/** Fills `ribbon` with the add-in's panels; `onPress` receives the pressed button. */
export function renderRibbon(ribbon, onPress) {
  ribbon.replaceChildren(
    ...panels.map((panel) => {
      const node = element("div", "cad-panel");
      const body = element("div", "cad-panel-body");
      for (const button of panel.buttons) {
        const press = element("button", button.large ? "cad-btn cad-btn-large" : "cad-btn cad-btn-text");
        press.type = "button";
        press.dataset.command = button.command;
        press.title = button.description;
        if (button.large) {
          const icon = element("img");
          icon.alt = "";
          icon.width = 32;
          icon.height = 32;
          iconUrl(button.large).then((url) => (icon.src = url), () => icon.remove());
          press.append(icon);
        }
        press.append(element("span", "", button.label));
        press.addEventListener("click", () => onPress(button));
        body.append(press);
      }
      node.append(body, element("div", "cad-panel-title", panel.title));
      return node;
    }),
  );
}

export function markActive(ribbon, command) {
  for (const button of ribbon.querySelectorAll(".cad-btn")) {
    button.classList.toggle("cad-btn-active", button.dataset.command === command);
  }
}
