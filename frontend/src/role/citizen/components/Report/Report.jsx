import { useState } from "react";
import ReportCard from "./ReportCard";
import { testReports } from "./test";

export default function Report() {
  const [reports] = useState(testReports);

  const stats = [
    { label: "Báo cáo", value: 100, icon: "📋" },
    { label: "Điểm", value: 100, icon: "⭐" },
    { label: "Hạng", value: 100, icon: "🏆" },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <div>
          <h1 className="text-xl font-bold lg:text-2xl">
            Xin chào, <span className="text-green-700">Hello</span>!
          </h1>
          <p className="text-sm text-gray-500">Hello</p>
        </div>
      </div>

      <div className="flex gap-3 overflow-x-auto lg:grid lg:grid-cols-3">
        {stats.map((stat) => (
          <div
            key={stat.label}
            className="relative min-w-[120px] flex-shrink-0 rounded-xl border bg-white px-4 py-3 transition hover:-translate-y-1 hover:shadow-md lg:min-w-0 overflow-visible"
          >
            <div className="absolute top-1 right-1 text-2xl opacity-20">
              {stat.icon}
            </div>

            <p className="text-xl font-bold">{stat.value}</p>
            <p className="text-xs text-gray-500">{stat.label}</p>
          </div>
        ))}
      </div>

      <div className="space-y-3">
        <h2 className="text-lg font-bold">Báo cáo gần đây</h2>

        {reports.length === 0 ? (
          <div className="flex flex-col items-center gap-4 py-12 text-center">
            <p className="text-gray-500">Chưa có báo cáo nào</p>
          </div>
        ) : (
          <div className="space-y-2">
            {reports.map((report) => (
              <ReportCard key={report.id} report={report} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
