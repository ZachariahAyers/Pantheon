// The add-in's three workframe dialogs, field for field, as Ui/Members/MemberCatalog.vb defines them.
// tests/site-dialog.test.mjs fails when a title, caption, default or hint here differs from it.

const BAY_HINT = "Bay sizes, for example 3*25',20'. Leave blank for none.";

const framing = (prefix, heading) => ({
  heading,
  fields: [
    { key: `${prefix}purlin`, caption: "Purlin depth", value: '8"' },
    { key: `${prefix}girt`, caption: "Girt depth", value: '8"' },
    { key: `${prefix}girts`, caption: "Girts", choices: ["Flush", "Bypass"] },
  ],
});

const side = (prefix, heading) => ({
  heading,
  fields: [
    { key: `${prefix}width`, caption: "Width" },
    { key: `${prefix}eave`, caption: "Eave height" },
    { key: `${prefix}length`, caption: "Length" },
    { key: `${prefix}pitch`, caption: "Pitch", hint: "Rise per 12 of run, rising toward the ridge." },
    ...framing(prefix, heading).fields,
  ],
});

const bays = {
  heading: "Bays",
  fields: [
    { key: "sideBays", caption: "Sidewall bays", hint: BAY_HINT },
    { key: "endBays", caption: "Endwall bays", hint: BAY_HINT },
  ],
};

export const dialogs = {
  "_WORKFRAME _Symmetrical": {
    roof: "symmetrical",
    title: "Symmetrical workframe",
    groups: [
      {
        heading: "Building",
        fields: [
          { key: "width", caption: "Width" },
          { key: "eave", caption: "Eave height" },
          { key: "length", caption: "Length" },
          { key: "pitch", caption: "Pitch", hint: "Rise per 12 of run, for example 1 for 1:12." },
        ],
      },
      framing("", "Framing"),
      bays,
    ],
  },
  "_WORKFRAME _Singleslope": {
    roof: "singleslope",
    title: "Single slope workframe",
    groups: [
      {
        heading: "Building",
        fields: [
          { key: "width", caption: "Width" },
          { key: "lowEave", caption: "Low eave height" },
          { key: "highEave", caption: "High eave height" },
          { key: "length", caption: "Length" },
        ],
      },
      framing("", "Framing"),
      bays,
    ],
  },
  "_WORKFRAME _Offcenter": {
    roof: "offcenter",
    title: "Off-center ridge workframe",
    groups: [side("left.", "Left side"), side("right.", "Right side"), bays],
  },
};

/** The entries the demo types, by dialog: a 60' x 120' shop and its two variations. */
export const sampleEntries = {
  "_WORKFRAME _Symmetrical": { mark: "SHOP", width: "60'", eave: "18'", length: "120'", pitch: "1", sideBays: "4*30'", endBays: "2*30'" },
  "_WORKFRAME _Singleslope": { mark: "LEAN-TO", width: "30'", lowEave: "14'", highEave: "17'", length: "120'", sideBays: "4*30'" },
  "_WORKFRAME _Offcenter": {
    mark: "BARN",
    "left.width": "20'",
    "left.eave": "16'",
    "left.length": "120'",
    "left.pitch": "6",
    "right.width": "40'",
    "right.eave": "16'",
    "right.length": "120'",
    "right.pitch": "3",
    sideBays: "4*30'",
  },
};
