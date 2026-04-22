const wasteConfig = {
  ["RECYCLABLE"]: {
    label: "Tái chế",
    colorClass: "bg-green-100 text-green-700",
  },
  ["ORGANIC"]: {
    label: "Hữu cơ",
    colorClass: "bg-emerald-100 text-emerald-700",
  },
  ["HAZARDOUS"]: {
    label: "Nguy hại",
    colorClass: "bg-red-100 text-red-700",
  },
  ["BULKY"]: {
    label: "Cồng kềnh",
    colorClass: "bg-orange-100 text-orange-700",
  },
};

export function WasteTypeBadge({ type }) {
  const config = wasteConfig[type];

  if (!config) return null;

  return (
    <span
      className={
        "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium " +
        config.colorClass
      }
    >
      {config.label}
    </span>
  );
}

export function WasteType({ type }) {
  const color = {
    ["RECYCLABLE"]: "bg-blue-100 text-blue-500",
    ["ORGANIC"]: "bg-green-100 text-green-500",
    ["HAZARDOUS"]: "bg-red-100 text-red-500",
    ["BULKY"]: "bg-orange-100 text-orange-500",
  };

  return (
    <div
      className={`inline-flex items-center justify-center rounded-full text-[10px] sm:text-xs leading-none px-1.5 py-[2px] sm:px-2 sm:py-0.5 whitespace-nowrap ${color[type]}`}
    >
      {type}
    </div>
  );
}
