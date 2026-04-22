import { Link, useParams } from "react-router-dom";
import { useState } from "react";
import { createPortal } from "react-dom";
// eslint-disable-next-line no-unused-vars
import { motion } from "framer-motion";
import { StatusBadge } from "./StatusBadge";
import { WasteType } from "./WasteTypeBadge";
import {
  formatRelativeTime,
  formatDate,
} from "../../../../shared/components/dateUtil";
import SendComplaint from "../Complaint/SendComplaint";
import { testReports } from "./test";

const timelineSteps = [
  "PENDING",
  "ACCEPTED",
  "ASSIGNED",
  "ON_THE_WAY",
  "COLLECTED",
  "VERIFIED",
];

const rejectTimelineSteps = ["PENDING", "REJECTED"];

const statusLabels = {
  PENDING: "Chờ xem xét",
  ACCEPTED: "Đã chấp nhận",
  REJECTED: "Bị từ chối",
  ASSIGNED: "Đã phân công",
  ON_THE_WAY: "Đang di chuyển",
  COLLECTED: "Đã thu gom",
  VERIFIED: "Đã xác nhận & cộng điểm",
  FAILED: "Thu gom hất bại",
  DISPUTED: "Xử lý khiếu nại",
  CANCELLED: "Đã hủy",
};

function isWithin48h(deadline) {
  if (!deadline) return false;
  return new Date(deadline).getTime() > Date.now();
}

function getStatusHistoryMap(status) {
  if (status === "REJECTED") return ["PENDING", "REJECTED"];

  const index = timelineSteps.indexOf(status);
  if (index === -1) return [];

  return timelineSteps.slice(0, index + 1); //[tra theo timelineSteps nhung dung dung o thu tu]
}

export default function ReportDetail() {
  const { reportId } = useParams();
  const report = testReports.find((item) => item.id === reportId);

  const [disputeOpen, setDisputeOpen] = useState(false);
  const [preview, setPreview] = useState(null);

  if (!report) {
    return (
      <div className="py-20 text-center">
        <p className="text-gray-500">Không tìm thấy báo cáo</p>
        <Link to="/citizen/report" className="mt-4 inline-block text-green-600">
          ← Quay lại
        </Link>
      </div>
    );
  }

  const currentIdx =
    report.status === "REJECTED"
      ? getStatusHistoryMap(report.status).length - 1
      : timelineSteps.indexOf(report.status);

  const historyMap = new Map(
    getStatusHistoryMap(report.status)?.map((item) => {
      if (item === "REJECTED") {
        return [item, { note: "Lý do bị từ chối" }];
      }

      if (item === "COLLECTED") {
        return [item, { note: "Đã thu gom", timestamp: 123 }];
      }

      if (item === "VERIFIED") {
        return [item, { note: "Điểm cộng" }];
      }

      return [item, {}];
    }),
  );

  return (
    <div className="space-y-6 lg:grid lg:grid-cols-2 lg:gap-8 lg:space-y-0">
      <div className="space-y-4">
        <Link
          to="/citizen/report"
          className="text-sm text-gray-500 hover:text-black"
        >
          ← Quay lại
        </Link>

        <div className="flex items-center">
          {report.photos.length === 1 &&
            report.photos.map((url, index) => (
              <img
                key={index}
                src={url}
                onClick={() => setPreview(url)}
                style={{ zIndex: 10 - index }}
                className="
      h-52 w-full object-cover rounded-xl border-4 border-white
      -ml-8 first:ml-0
      cursor-pointer hover:scale-105 transition
    "
              />
            ))}

          {report.photos.length === 2 &&
            report.photos.slice(0, 4).map((url, index) => (
              <img
                key={index}
                src={url}
                onClick={() => setPreview(url)}
                style={{ zIndex: 10 - index }}
                className="
      h-52 w-1/2 object-cover rounded-xl border-4 border-white
      -ml-8 first:ml-0
      cursor-pointer hover:scale-105 transition
    "
              />
            ))}
          {report.photos.length === 3 &&
            report.photos.slice(0, 4).map((url, index) => (
              <img
                key={index}
                src={url}
                onClick={() => setPreview(url)}
                style={{ zIndex: 10 - index }}
                className="
      h-52 w-1/3 object-cover rounded-xl border-4 border-white
      -ml-8 first:ml-0
      cursor-pointer hover:scale-105 transition
    "
              />
            ))}

          {report.photos.length === 4 &&
            report.photos.slice(0, 4).map((url, index) => (
              <img
                key={index}
                src={url}
                onClick={() => setPreview(url)}
                style={{ zIndex: 10 - index }}
                className="
      h-52 w-36 object-cover rounded-xl border-4 border-white
      -ml-8 first:ml-0
      cursor-pointer hover:scale-105 transition
    "
              />
            ))}

          {/* {report.photos.length > 4 && (
            <div className="h-16 w-16 -ml-3 rounded-lg bg-gray-200 flex items-center justify-center text-sm font-semibold">
              +{report.photos.length - 4}
            </div>
          )} */}
        </div>

        {preview &&
          createPortal(
            <div
              onClick={() => setPreview(null)}
              className="
        fixed inset-0
        bg-black/70
        flex items-center justify-center
        z-[9999]
      "
            >
              <img
                src={preview}
                className="max-h-[85%] max-w-[90%] rounded-lg"
              />
            </div>,
            document.body,
          )}

        <div className="flex flex-wrap gap-2">
          {report.wasteTypes.map((t) => (
            <WasteType key={t} type={t} />
          ))}
          <StatusBadge status={report.status} large />
        </div>

        <p className="text-sm text-gray-600">{report.address}</p>
        <p className="text-xs text-gray-400">
          {formatRelativeTime(report.createdAt)}
        </p>

        {report.description && <p className="text-sm">{report.description}</p>}

        {report.collectorName && (
          <div className="flex items-center gap-3 rounded-xl border p-3">
            <div className="h-9 w-9 rounded-full bg-green-200 flex items-center justify-center font-bold">
              {report.collectorName.charAt(0)}
            </div>
            <div>
              <p className="text-sm font-medium">{report.collectorName}</p>
              <p className="text-xs text-gray-500">đang phụ trách</p>
            </div>
          </div>
        )}

        {report.status === "VERIFIED" && report.pointReward && (
          <div className="bg-green-100 p-3 rounded-xl">
            +{report.pointReward} điểm
          </div>
        )}

        {(report.status === "COLLECTED" || report.status === "VERIFIED") &&
          isWithin48h(report.createdAt) && (
            <button
              onClick={() => setDisputeOpen(true)}
              className="w-full border rounded-xl py-2 text-sm"
            >
              Gửi khiếu nại
            </button>
          )}

        <SendComplaint
          disputeOpen={disputeOpen}
          setDisputeOpen={setDisputeOpen}
        />
      </div>

      <div className="space-y-4">
        <h2 className="text-lg font-bold">Tiến trình xử lý</h2>

        <div>
          {(report.status === "REJECTED"
            ? rejectTimelineSteps
            : timelineSteps
          ).map((stepStatus, i, arr) => {
            const entry = historyMap.get(stepStatus);

            const isCompleted =
              !!entry &&
              (report.status === "REJECTED" ||
                timelineSteps.indexOf(stepStatus) < currentIdx);

            const isCurrent = report.status === stepStatus;
            const isFuture = !entry;
            const isLast = i === arr.length - 1;

            return (
              <motion.div
                key={stepStatus}
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: i * 0.08, duration: 0.3 }}
                className="flex gap-3"
              >
                <div className="flex flex-col items-center">
                  <div
                    className={`flex h-6 w-6 items-center justify-center rounded-full border-2 ${
                      isCompleted
                        ? "border-green-600 bg-green-600"
                        : isCurrent
                          ? "border-green-600"
                          : "border-gray-300"
                    }`}
                  >
                    {isCompleted && (
                      <svg
                        className="h-3 w-3 text-white"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="3"
                      >
                        <polyline points="20,6 9,17 4,12" />
                      </svg>
                    )}

                    {isCurrent && (
                      <motion.div
                        animate={{ scale: [1, 1.3, 1] }}
                        transition={{ repeat: Infinity, duration: 1.5 }}
                        className="h-2 w-2 rounded-full bg-green-600"
                      />
                    )}
                  </div>

                  {!isLast && (
                    <div
                      className={`w-0.5 flex-1 min-h-[32px] ${
                        isCompleted ? "bg-green-600" : "bg-gray-300"
                      }`}
                    />
                  )}
                </div>

                <div className="pb-6">
                  <p
                    className={`text-sm font-medium ${
                      isCurrent
                        ? "text-green-600 font-bold"
                        : isFuture
                          ? "text-gray-400"
                          : "text-gray-800"
                    }`}
                  >
                    {statusLabels[stepStatus]}
                  </p>

                  {entry?.timestamp && (
                    <p className="text-xs text-gray-500">
                      {formatDate(entry.timestamp)}
                    </p>
                  )}

                  {entry?.note && (
                    <p className="text-xs text-gray-500">{entry.note}</p>
                  )}
                </div>
              </motion.div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
