import { Leaf } from "lucide-react";

const platform = ["Giới thiệu", "Quy trình", "Phân loại rác"];
const support = ["Hướng dẫn sử dụng", "Câu hỏi thường gặp"];
const legal = ["Chính sách bảo mật", "Điều khoản sử dụng"];

export default function Footer() {
  return (
    <footer style={{ background: "hsl(155 30% 6%)" }}>
      <div className="container mx-auto px-4 lg:px-8 py-16">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
          <div className="col-span-2 md:col-span-1">
            <div className="flex items-center gap-2 mb-4">
              <div className="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
                <Leaf className="w-4 h-4 text-primary-foreground" />
              </div>
              <span
                className="font-extrabold text-lg"
                style={{ color: "hsl(0 0% 95%)" }}
              >
                Garbage Collection
              </span>
            </div>
            <p className="text-sm mb-4" style={{ color: "hsl(150 10% 45%)" }}>
              Vì một phường xanh hơn mỗi ngày
            </p>
            <div className="flex gap-3">
              {["Instagram", "Facebook", "Zalo"].map((s) => (
                <span
                  key={s}
                  className="text-xs px-3 py-1.5 rounded-full cursor-pointer transition-colors"
                  style={{
                    background: "hsl(152 20% 15%)",
                    color: "hsl(150 10% 55%)",
                  }}
                >
                  {s}
                </span>
              ))}
            </div>
          </div>

          <div>
            <h4
              className="font-bold text-sm mb-4"
              style={{ color: "hsl(0 0% 90%)" }}
            >
              Nền tảng
            </h4>
            <ul className="space-y-2">
              {platform.map((item) => (
                <li key={item}>
                  <a
                    href={`#${item === "Giới thiệu" ? "problem" : item === "Quy trình" ? "process" : "categories"}`}
                    className="text-sm transition-colors hover:underline"
                    style={{ color: "hsl(150 10% 50%)" }}
                  >
                    {item}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          <div>
            <h4
              className="font-bold text-sm mb-4"
              style={{ color: "hsl(0 0% 90%)" }}
            >
              Hỗ trợ
            </h4>
            <ul className="space-y-2">
              {support.map((item) => (
                <li key={item}>
                  <a
                    href="#"
                    className="text-sm transition-colors hover:underline"
                    style={{ color: "hsl(150 10% 50%)" }}
                  >
                    {item}
                  </a>
                </li>
              ))}
              <li className="text-sm" style={{ color: "hsl(150 10% 50%)" }}>
                Liên hệ: garbaeCollection.vn
              </li>
            </ul>
          </div>

          <div>
            <h4
              className="font-bold text-sm mb-4"
              style={{ color: "hsl(0 0% 90%)" }}
            >
              Pháp lý
            </h4>
            <ul className="space-y-2">
              {legal.map((item) => (
                <li key={item}>
                  <a
                    href="#"
                    className="text-sm transition-colors hover:underline"
                    style={{ color: "hsl(150 10% 50%)" }}
                  >
                    {item}
                  </a>
                </li>
              ))}
              <li
                className="text-xs mt-3"
                style={{ color: "hsl(150 10% 40%)" }}
              >
                Tuân thủ quy định PDPD Việt Nam
              </li>
            </ul>
          </div>
        </div>

        <div
          className="mt-12 pt-6 text-center text-sm"
          style={{
            borderTop: "1px solid hsl(152 15% 15%)",
            color: "hsl(150 10% 40%)",
          }}
        >
          © 2026 Garbage Collection · TP. Hồ Chí Minh
        </div>
      </div>
    </footer>
  );
}
