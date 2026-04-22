import { useState } from "react";
import { createPortal } from "react-dom";

export default function SendComplaint({ disputeOpen, setDisputeOpen }) {
  const [disputeReason, setDisputeReason] = useState("");
  const [images, setImages] = useState([]);
  const [preview, setPreview] = useState(null);

  const handleUpload = (e) => {
    const files = Array.from(e.target.files);

    if (images.length + files.length > 4) {
      alert("Chỉ được upload tối đa 4 ảnh");
      return;
    }

    const newImages = files.map((file) => ({
      file,
      url: URL.createObjectURL(file),
    }));

    setImages((prev) => [...prev, ...newImages]);
  };

  const removeImage = (index) => {
    setImages((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSubmit = () => {
    if (!disputeReason.trim()) {
      alert("Vui lòng nhập lý do khiếu nại");
      return;
    }

    console.log({
      reason: disputeReason,
      images,
    });

    setDisputeOpen(false);
    setImages([]);
    setDisputeReason("");
  };

  if (!disputeOpen) return null;

  return (
    <div className="border p-4 rounded-xl space-y-4 bg-white shadow-sm">
      <textarea
        value={disputeReason}
        onChange={(e) => setDisputeReason(e.target.value)}
        className="w-full border rounded p-2 text-sm focus:outline-none focus:border-green-500"
        placeholder="Nhập lý do khiếu nại..."
      />

      <input
        type="file"
        multiple
        accept="image/*"
        onChange={handleUpload}
        className="text-sm"
      />

      {images.length > 0 && (
        <div className="flex items-center mt-2">
          {images.map((img, index) => (
            <div key={index} className="relative">
              <img
                src={img.url}
                onClick={() => setPreview(img.url)}
                className="h-16 w-16 object-cover rounded-lg border-2 border-white -ml-3 first:ml-0 cursor-pointer hover:scale-105 transition"
              />

              <button
                onClick={() => removeImage(index)}
                className="absolute -top-2 -right-2 bg-black text-white text-xs rounded-full px-1"
              >
                ✕
              </button>
            </div>
          ))}
        </div>
      )}

      <div className="flex gap-2">
        <button
          onClick={() => {
            setDisputeOpen(false);
            setImages([]);
            setDisputeReason("");
          }}
          className="flex-1 border rounded py-2 hover:bg-gray-100"
        >
          Hủy
        </button>

        <button
          onClick={handleSubmit}
          disabled={!disputeReason.trim()}
          className={`
            flex-1 rounded py-2 text-white
            ${
              disputeReason.trim()
                ? "bg-green-600 hover:bg-green-700"
                : "bg-gray-400 cursor-not-allowed"
            }
          `}
        >
          Gửi
        </button>
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
            <img src={preview} className="max-h-[85%] max-w-[90%] rounded-lg" />
          </div>,
          document.body,
        )}
    </div>
  );
}
