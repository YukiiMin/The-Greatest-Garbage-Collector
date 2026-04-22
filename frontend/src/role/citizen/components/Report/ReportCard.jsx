import { Link } from "react-router-dom";
import { StatusBadge } from "./StatusBadge";
import { WasteType } from "./WasteTypeBadge";
import { formatRelativeTime } from "../../../../shared/components/dateUtil";

export default function ReportCard({ report }) {
  return (
    <Link to={`/citizen/report/${report.id}`}>
      <div
        className="
      grid grid-cols-4 items-center
      gap-6
      rounded-xl border border-gray-200 bg-white
      p-4 transition
      hover:bg-gray-50 hover:shadow-sm
      active:scale-[0.98]
    "
      >
        <div className="min-w-0 text-left">
          <p className="font-semibold text-gray-900 truncate">
            {report.description}
          </p>

          <p className="truncate text-sm text-gray-700">{report.address}</p>

          <p className="text-xs text-gray-500">
            {formatRelativeTime(report.createdAt)}
          </p>
        </div>

        <div className="flex flex-col items-start gap-1 mr-auto">
          {report.wasteTypes?.map((t) => (
            <WasteType key={t} type={t} />
          ))}
        </div>

        <div className="flex items-start mr-auto">
          <StatusBadge status={report.status} />
        </div>

        <div className="flex justify-end gap-2">
          {report.photos.map((url, index) => {
            if (index > 3) {
              return <></>;
            }

            return (
              <img
                key={index}
                src={url}
                alt="Ảnh báo cáo"
                className={`rounded-lg object-cover h-14 w-14 ${index >= 1 ? "hidden sm:block" : ""}`}
              />
            );
          })}
        </div>
      </div>
    </Link>
  );
}
