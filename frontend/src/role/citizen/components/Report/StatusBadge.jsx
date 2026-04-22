const statusConfig = {
  ["PENDING"]: {
    label: "Chờ xem xét",
    colorClass: "bg-yellow-100 text-yellow-700",
  },
  ["ACCEPTED"]: {
    label: "Đã duyệt",
    colorClass: "bg-green-100 text-green-700",
  },
  ["REJECTED"]: {
    label: "Bị từ chối",
    colorClass: "bg-red-100 text-red-700",
  },
  ["ASSIGNED"]: {
    label: "Đã phân công",
    colorClass: "bg-blue-100 text-blue-700",
  },
  ["ON_THE_WAY"]: {
    label: "Đang di chuyển",
    colorClass: "bg-sky-100 text-sky-700",
  },
  ["COLLECTED"]: {
    label: "Đã thu gom",
    colorClass: "bg-emerald-100 text-emerald-700",
  },
  ["VERIFIED"]: {
    label: "Đã xác nhận",
    colorClass: "bg-green-200 text-green-800",
  },
  ["FAILED"]: {
    label: "Thu gom thất bại",
    colorClass: "bg-red-200 text-red-800",
  },
  ["DISPUTED"]: {
    label: "Xử lý khiếu nại",
    colorClass: "bg-orange-100 text-orange-700",
  },
  ["CANCELLED"]: {
    label: "Đã hủy",
    colorClass: "bg-gray-200 text-gray-700",
  },
};

export function StatusBadge({ status }) {
  const config = statusConfig[status];

  if (!config) return null;

  return (
    <span
      className={`
    inline-flex items-center
    rounded-full

    font-medium leading-none
    ${config.colorClass}

    text-[10px] sm:text-xs
    px-2 py-[2px] sm:px-3 sm:py-1

    max-w-[120px]
    truncate
  `}
    >
      {config.label}
    </span>
  );
}
